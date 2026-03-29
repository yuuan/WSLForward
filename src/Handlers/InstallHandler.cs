namespace WslForward.Handlers
{
    /// <summary>install: Firewall + タスク登録 + ポート転送を一括セットアップする。</summary>
    internal static class InstallHandler
    {
        /// <summary>コマンドを実行する。</summary>
        public static void Execute(
            FirewallClient fw, Config cfg,
            TaskSchedulerClient tasks, string logonTaskName, string intervalTaskName,
            WslClient wsl, PortForwardClient pf)
        {
            InstallFirewallHandler.Execute(fw, cfg);
            InstallTasksHandler.Execute(tasks, logonTaskName, intervalTaskName);
            UpdateHandler.Execute(wsl, pf, cfg);

            Console.WriteLine("\n=== セットアップ完了 ===");
        }
    }
}
