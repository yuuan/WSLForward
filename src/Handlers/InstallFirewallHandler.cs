namespace WslForward.Handlers
{
    /// <summary>install-firewall: Firewall の受信許可ルールを作成する。</summary>
    internal static class InstallFirewallHandler
    {
        /// <summary>コマンドを実行する。</summary>
        public static void Execute(FirewallClient fw, Config cfg)
        {
            Console.WriteLine("[INFO] ファイアウォールルールを作成しています...");
            fw.AddRule(cfg.ListenPort);
            Console.WriteLine("[OK] ファイアウォールルールを作成しました。");

            Console.WriteLine("\n--- ファイアウォールルールの確認 ---");
            ShowFirewallHandler.Execute(fw);
        }
    }
}
