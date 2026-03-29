namespace WslForward.Handlers
{
    /// <summary>uninstall: 全設定を一括削除する。</summary>
    internal static class UninstallHandler
    {
        /// <summary>コマンドを実行する。</summary>
        public static void Execute(PortForwardClient pf, Config cfg, FirewallClient fw, TaskSchedulerClient tasks, string[] taskNames)
        {
            Console.WriteLine("=== 全設定の削除 ===\n");

            UninstallForwardHandler.Execute(pf, cfg);
            UninstallFirewallHandler.Execute(fw);
            UninstallTasksHandler.Execute(tasks, taskNames);

            Console.WriteLine("\n=== 削除完了 ===");
        }
    }
}
