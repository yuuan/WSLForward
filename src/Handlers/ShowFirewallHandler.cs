namespace WslForward.Handlers
{
    /// <summary>show-firewall: Firewall ルールを表示する。</summary>
    internal static class ShowFirewallHandler
    {
        /// <summary>コマンドを実行する。</summary>
        public static void Execute(FirewallClient fw)
        {
            FirewallRuleInfo? rule = fw.GetRule();
            if (rule == null)
            {
                Console.WriteLine($"[INFO] ファイアウォールルール '{fw.RuleName}' は存在しません。");
                return;
            }
            Console.WriteLine($"ルール名: {rule.Name}");
            Console.WriteLine($"有効: {rule.Enabled}");
            Console.WriteLine($"方向: {rule.Direction}");
            Console.WriteLine($"操作: {rule.Action}");
            Console.WriteLine($"プロトコル: {rule.Protocol}");
            Console.WriteLine($"ローカルポート: {rule.LocalPorts}");
            Console.WriteLine($"プロファイル: {rule.Profiles}");
        }
    }
}
