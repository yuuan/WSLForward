namespace WslForward.Handlers
{
    /// <summary>WSL 操作に関する共通処理。</summary>
    internal static class WslHelper
    {
        /// <summary>WSL ディストリビューションが起動していなければ起動する。</summary>
        public static void EnsureRunning(WslClient wsl)
        {
            Console.WriteLine($"[INFO] WSL ディストリビューション '{wsl.DistroName}' の状態を確認しています...");

            if (wsl.IsRunning())
            {
                return;
            }

            Console.WriteLine($"[INFO] '{wsl.DistroName}' が停止中です。起動します...");
            wsl.Start();
            Console.WriteLine($"[INFO] '{wsl.DistroName}' を起動しました。");
        }
    }
}
