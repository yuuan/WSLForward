namespace WslForward.Handlers
{
    /// <summary>show-tasks: タスクスケジューラの登録状況を表示する。</summary>
    internal static class ShowTasksHandler
    {
        /// <summary>コマンドを実行する。</summary>
        public static void Execute(TaskSchedulerClient tasks, string[] taskNames)
        {
            foreach (string taskName in taskNames)
            {
                TaskInfo? info = tasks.Query(taskName);
                if (info == null)
                {
                    Console.WriteLine($"[INFO] タスク '{taskName}' は存在しません。");
                    continue;
                }
                Console.WriteLine($"タスク名: {info.Name}");
                Console.WriteLine($"有効: {info.Enabled}");
                Console.WriteLine($"トリガー: {info.TriggerType ?? "不明"}");
                Console.WriteLine($"コマンド: {info.Command}{(info.Arguments != null ? $" {info.Arguments}" : "")}");
                Console.WriteLine($"実行ユーザー: {info.RunAsUser}");
                Console.WriteLine("");
            }
        }
    }
}
