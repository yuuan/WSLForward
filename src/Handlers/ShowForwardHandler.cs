namespace WslForward.Handlers
{
    /// <summary>show-forward: ポート転送設定を表示する。</summary>
    internal static class ShowForwardHandler
    {
        /// <summary>コマンドを実行する。</summary>
        public static void Execute(PortForwardClient pf)
        {
            List<PortForwardEntry> entries = pf.GetAll();
            if (entries.Count == 0)
            {
                Console.WriteLine("[INFO] ポート転送設定はありません。");
                return;
            }
            foreach (PortForwardEntry e in entries)
            {
                Console.WriteLine($"{e.ListenAddress}:{e.ListenPort} -> {e.ConnectAddress}:{e.ConnectPort}");
            }
        }
    }
}
