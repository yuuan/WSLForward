namespace WslForward.Handlers
{
    /// <summary>init-config: デフォルト値の config.json を作成する。</summary>
    internal static class InitConfigHandler
    {
        /// <summary>コマンドを実行する。</summary>
        public static void Execute()
        {
            (bool created, string path) = Config.Init();
            Console.WriteLine(created
                ? $"[OK] {path} を作成しました。"
                : $"[SKIP] {path} は既に存在します。");
        }
    }
}
