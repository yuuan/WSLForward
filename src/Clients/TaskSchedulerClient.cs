using Microsoft.Win32.TaskScheduler;

namespace WslForward
{
    /// <summary>タスクスケジューラのタスク情報。</summary>
    internal sealed record TaskInfo(
        string Name,
        bool Enabled,
        string? TriggerType,
        string? Command,
        string? Arguments,
        string RunAsUser
    );

    /// <summary>Windows タスクスケジューラのタスク操作を提供する。</summary>
    internal sealed class TaskSchedulerClient
    {
        /// <summary>ログイン時に実行するタスクを登録する。</summary>
        public void CreateOnLogon(string taskName, string command)
        {
            TryDelete(taskName);
            (string folderPath, string baseName) = ParseTaskName(taskName);
            (string exePath, string? args) = ParseCommand(command);

            using TaskService ts = new();
            TaskDefinition td = ts.NewTask();
            ConfigureDefaults(td);
            _ = td.Triggers.Add(new LogonTrigger());
            _ = td.Actions.Add(new ExecAction(exePath, args));

            TaskFolder folder = GetOrCreateFolder(ts, folderPath);
            _ = folder.RegisterTaskDefinition(baseName, td);
        }

        /// <summary>指定間隔(分)で繰り返すタスクを登録する。</summary>
        public void CreateInterval(string taskName, string command, int minutes)
        {
            TryDelete(taskName);
            (string folderPath, string baseName) = ParseTaskName(taskName);
            (string exePath, string? args) = ParseCommand(command);

            using TaskService ts = new();
            TaskDefinition td = ts.NewTask();
            ConfigureDefaults(td);
            _ = td.Triggers.Add(new TimeTrigger
            {
                StartBoundary = DateTime.Today,
                Repetition = { Interval = TimeSpan.FromMinutes(minutes) }
            });
            _ = td.Actions.Add(new ExecAction(exePath, args));

            TaskFolder folder = GetOrCreateFolder(ts, folderPath);
            _ = folder.RegisterTaskDefinition(baseName, td);
        }

        /// <summary>タスクを削除する。フォルダが空になった場合はフォルダも削除する。存在しなければ false。</summary>
        public bool Delete(string taskName)
        {
            try
            {
                (string folderPath, string baseName) = ParseTaskName(taskName);
                using TaskService ts = new();
                TaskFolder folder = ts.GetFolder(folderPath);
                folder.DeleteTask(baseName, false);
                TryDeleteEmptyFolder(ts, folderPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>タスクの情報を取得する。存在しなければ null。</summary>
        public TaskInfo? Query(string taskName)
        {
            try
            {
                using TaskService ts = new();
                Microsoft.Win32.TaskScheduler.Task task = ts.GetTask(taskName);
                if (task == null)
                {
                    return null;
                }

                TaskDefinition def = task.Definition;

                string? triggerType = def.Triggers.Count > 0
                    ? def.Triggers[0] switch
                    {
                        LogonTrigger => "Logon",
                        TimeTrigger => "Time",
                        var t => t.TriggerType.ToString()
                    }
                    : null;

                string? command = null;
                string? arguments = null;
                if (def.Actions.Count > 0 && def.Actions[0] is ExecAction exec)
                {
                    command = exec.Path;
                    arguments = string.IsNullOrEmpty(exec.Arguments) ? null : exec.Arguments;
                }

                (string _, string baseName) = ParseTaskName(taskName);

                return new TaskInfo(
                    Name: baseName,
                    Enabled: def.Settings.Enabled,
                    TriggerType: triggerType,
                    Command: command,
                    Arguments: arguments,
                    RunAsUser: def.Principal.UserId ?? "Unknown"
                );
            }
            catch
            {
                return null;
            }
        }

        private void TryDelete(string taskName)
        {
            try { _ = Delete(taskName); } catch { }
        }

        private static void TryDeleteEmptyFolder(TaskService ts, string folderPath)
        {
            if (folderPath == @"\")
            {
                return;
            }

            try
            {
                TaskFolder folder = ts.GetFolder(folderPath);
                if (folder.Tasks.Count == 0 && folder.SubFolders.Count == 0)
                {
                    ts.RootFolder.DeleteFolder(folderPath, false);
                }
            }
            catch { }
        }

        private static void ConfigureDefaults(TaskDefinition td)
        {
            td.Principal.RunLevel = TaskRunLevel.Highest;
            td.Settings.Enabled = true;
            td.Settings.StopIfGoingOnBatteries = false;
            td.Settings.DisallowStartIfOnBatteries = false;
            td.Settings.StartWhenAvailable = true;
        }

        private static TaskFolder GetOrCreateFolder(TaskService ts, string folderPath)
        {
            TaskFolder folder = ts.GetFolder(folderPath);
            if (folder != null)
            {
                return folder;
            }

            string name = folderPath.TrimStart('\\');
            return ts.RootFolder.CreateFolder(name)
                ?? throw new InvalidOperationException($"タスクフォルダ '{folderPath}' の作成に失敗しました");
        }

        private static (string exePath, string? args) ParseCommand(string command)
        {
            if (command.StartsWith('"'))
            {
                int endQuote = command.IndexOf('"', 1);
                if (endQuote > 0)
                {
                    string path = command[1..endQuote];
                    string args = command[(endQuote + 1)..].TrimStart();
                    return (path, string.IsNullOrEmpty(args) ? null : args);
                }
            }
            else
            {
                int spaceIdx = command.IndexOf(' ');
                if (spaceIdx > 0)
                {
                    return (command[..spaceIdx], command[(spaceIdx + 1)..].TrimStart());
                }
            }
            return (command, null);
        }

        private static (string folderPath, string baseName) ParseTaskName(string taskName)
        {
            taskName = taskName.TrimStart('\\');
            int lastSep = taskName.LastIndexOf('\\');
            if (lastSep < 0)
            {
                return (@"\", taskName);
            }

            return ($@"\{taskName[..lastSep]}", taskName[(lastSep + 1)..]);
        }
    }
}
