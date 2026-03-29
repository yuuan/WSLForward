namespace WslForward.Handlers
{
    /// <summary>install-tasks: タスクスケジューラにタスクを登録する。</summary>
    internal static class InstallTasksHandler
    {
        /// <summary>コマンドを実行する。</summary>
        public static void Execute(TaskSchedulerClient tasks, string logonTaskName, string intervalTaskName)
        {
            string exePath = Environment.ProcessPath
                ?? throw new InvalidOperationException("実行ファイルのパスを取得できません");

            string command = $"\"{exePath}\" update";

            Console.WriteLine("=== ログイン時タスクの登録 ===");
            tasks.CreateOnLogon(logonTaskName, command);
            Console.WriteLine($"[OK] ログイン時タスク '{logonTaskName}' を登録しました。");

            Console.WriteLine("\n=== 10 分間隔タスクの登録 ===");
            tasks.CreateInterval(intervalTaskName, command, 10);
            Console.WriteLine($"[OK] 10 分間隔タスク '{intervalTaskName}' を登録しました。");

            Console.WriteLine("\n--- 登録済みタスクの確認 ---");
            ShowTasksHandler.Execute(tasks, [logonTaskName, intervalTaskName]);
        }
    }
}
