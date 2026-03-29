using System.Text;
using System.Text.RegularExpressions;

namespace WslForward
{
    /// <summary>WSL ディストリビューションの状態情報。</summary>
    internal sealed record DistroInfo(string Name, bool IsDefault, string State, int Version);

    /// <summary>WSL ディストリビューションの操作を提供する。</summary>
    internal sealed partial class WslClient(ProcessRunner runner, string distroName)
    {
        /// <summary>対象ディストリビューション名。</summary>
        public string DistroName => distroName;

        /// <summary>対象ディストリビューションの情報を取得する。見つからなければ null。WSL が利用できなければ例外。</summary>
        public DistroInfo? GetDistroInfo()
        {
            (int exitCode, string stdout, string stderr) = runner.Run("wsl.exe", Encoding.Unicode, "--list", "--verbose");
            if (exitCode != 0)
            {
                throw new InvalidOperationException($"WSL の状態取得に失敗しました: {stderr}");
            }

            string cleaned = stdout.Replace("\0", "");
            Match match = Regex.Match(cleaned, $@"^(\s*\*?)\s*{Regex.Escape(distroName)}\s+(\S+)\s+(\d+)",
                RegexOptions.Multiline | RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return null;
            }

            bool isDefault = match.Groups[1].Value.Contains('*');
            string state = match.Groups[2].Value;
            int version = int.Parse(match.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture);

            return new DistroInfo(distroName, isDefault, state, version);
        }

        /// <summary>対象ディストリビューションが Running かどうか。WSL が利用できない場合も false を返す。</summary>
        public bool IsRunning()
        {
            try
            {
                DistroInfo? info = GetDistroInfo();
                return info != null && string.Equals(info.State, "Running", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>対象ディストリビューションを起動する。既に Running なら何もしない。</summary>
        public void Start()
        {
            if (IsRunning())
            {
                return;
            }

            (int exitCode, string _, string stderr) = runner.Run("wsl.exe", "-d", distroName, "--", "/bin/true");
            if (exitCode != 0)
            {
                throw new InvalidOperationException($"'{distroName}' の起動に失敗しました: {stderr}");
            }

            Thread.Sleep(3000);
        }

        /// <summary>対象ディストリビューションの IPv4 アドレスを取得する。</summary>
        public string GetIp()
        {
            (int exitCode, string stdout, string stderr) = runner.Run("wsl.exe", "-d", distroName, "--", "ip", "-4", "addr", "show", "eth0");
            if (exitCode != 0)
            {
                throw new InvalidOperationException($"WSL2 の IP アドレス取得に失敗しました: {stderr}");
            }

            Match match = InetRegex().Match(stdout);
            return !match.Success ? throw new InvalidOperationException("WSL2 の IPv4 アドレスを検出できませんでした") : match.Groups[1].Value;
        }

        [GeneratedRegex(@"inet\s+(\d+\.\d+\.\d+\.\d+)/")]
        private static partial Regex InetRegex();
    }
}
