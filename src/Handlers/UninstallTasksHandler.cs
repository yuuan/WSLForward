namespace WslForward.Handlers
{
    /// <summary>uninstall-tasks: タスクスケジューラのタスクを削除する。</summary>
    internal static class UninstallTasksHandler
    {
        /// <summary>コマンドを実行する。</summary>
        public static void Execute(TaskSchedulerClient tasks, string[] taskNames)
        {
            Console.WriteLine("[INFO] タスクスケジューラのタスクを削除しています...");
            foreach (string taskName in taskNames)
            {
                bool deleted = tasks.Delete(taskName);
                Console.WriteLine(deleted
                    ? $"[OK] タスク '{taskName}' を削除しました。"
                    : $"[SKIP] タスク '{taskName}' は存在しません。");
            }
        }
    }
}
