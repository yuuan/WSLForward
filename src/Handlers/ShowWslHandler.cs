namespace WslForward.Handlers
{
    /// <summary>show-wsl: WSL ディストリビューションの状態を表示する。</summary>
    internal static class ShowWslHandler
    {
        /// <summary>コマンドを実行する。</summary>
        public static void Execute(WslClient wsl, Config cfg)
        {
            DistroInfo info = wsl.GetDistroInfo()
                ?? throw new InvalidOperationException($"ディストリビューション '{cfg.DistroName}' が見つかりません");

            Log($"ディストリビューション: {info.Name}");
            Log($"状態: {info.State}");
            Log($"WSL バージョン: {info.Version}");

            if (string.Equals(info.State, "Running", StringComparison.OrdinalIgnoreCase))
            {
                try { Log($"IPv4: {wsl.GetIp()}"); }
                catch { Log("IPv4: (取得失敗)"); }
            }

        }

        private static void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
