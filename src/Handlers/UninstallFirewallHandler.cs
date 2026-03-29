namespace WslForward.Handlers
{
    /// <summary>uninstall-firewall: Firewall ルールを削除する。</summary>
    internal static class UninstallFirewallHandler
    {
        /// <summary>コマンドを実行する。</summary>
        public static void Execute(FirewallClient fw)
        {
            Console.WriteLine("[INFO] ファイアウォールルールを削除しています...");
            bool deleted = fw.DeleteRule();
            Console.WriteLine(deleted
                ? $"[OK] ファイアウォールルール '{fw.RuleName}' を削除しました。"
                : $"[SKIP] ファイアウォールルール '{fw.RuleName}' は存在しません。");
        }
    }
}
