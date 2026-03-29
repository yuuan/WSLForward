namespace WslForward.Handlers
{
    /// <summary>uninstall-forward: ポート転送設定を削除する。</summary>
    internal static class UninstallForwardHandler
    {
        /// <summary>コマンドを実行する。</summary>
        public static void Execute(PortForwardClient pf, Config cfg)
        {
            Console.WriteLine("[INFO] ポート転送を削除しています...");
            bool deleted = pf.Delete(cfg.ListenAddress, cfg.ListenPort);
            Console.WriteLine(deleted
                ? "[OK] ポート転送を削除しました。"
                : "[SKIP] 該当するポート転送が見つかりませんでした。");
        }
    }
}
