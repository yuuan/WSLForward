namespace WslForward.Handlers
{
    /// <summary>update: WSL を起動し、ポート転送を差分更新する。</summary>
    internal static class UpdateHandler
    {
        /// <summary>コマンドを実行する。</summary>
        public static void Execute(WslClient wsl, PortForwardClient pf, Config cfg)
        {
            WslHelper.EnsureRunning(wsl);

            Log("[INFO] WSL2 の IPv4 アドレスを取得しています...");
            string wslIp = wsl.GetIp();
            Log($"[INFO] WSL2 IPv4: {wslIp}");

            Log("[INFO] 現在のポート転送設定を確認しています...");
            string? currentAddr = pf.GetConnectAddress(cfg.ListenAddress, cfg.ListenPort, cfg.WslPort);

            if (currentAddr != null)
            {
                Log($"[INFO] 既存のポート転送: {cfg.ListenAddress}:{cfg.ListenPort} -> {currentAddr}:{cfg.WslPort}");
            }
            else
            {
                Log("[INFO] 既存のポート転送設定はありません。");
            }

            if (currentAddr == wslIp)
            {
                Log("[OK] ポート転送は最新です。更新不要。");
                return;
            }

            Log($"[INFO] ポート転送を更新します: {cfg.ListenAddress}:{cfg.ListenPort} -> {wslIp}:{cfg.WslPort}");

            pf.Add(new PortForwardEntry(cfg.ListenAddress, cfg.ListenPort, wslIp, cfg.WslPort));

            Log("[OK] ポート転送を更新しました。");
        }

        private static void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
