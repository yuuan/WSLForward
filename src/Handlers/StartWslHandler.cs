namespace WslForward.Handlers
{
    /// <summary>start-wsl: WSL ディストリビューションを起動する。</summary>
    internal static class StartWslHandler
    {
        /// <summary>コマンドを実行する。</summary>
        public static void Execute(WslClient wsl)
        {
            WslHelper.EnsureRunning(wsl);
        }
    }
}
