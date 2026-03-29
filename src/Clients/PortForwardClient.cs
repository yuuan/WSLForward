using System.Text.RegularExpressions;

namespace WslForward
{
    /// <summary>ポート転送エントリの定義。</summary>
    internal sealed record PortForwardEntry(string ListenAddress, int ListenPort, string ConnectAddress, int ConnectPort);

    /// <summary>Windows のポート転送 (netsh portproxy) の操作を提供する。</summary>
    internal sealed partial class PortForwardClient(ProcessRunner runner)
    {
        /// <summary>全ポート転送設定を取得する。</summary>
        public List<PortForwardEntry> GetAll()
        {
            (int _, string stdout, string _) = runner.Run("netsh", "interface", "portproxy", "show", "v4tov4");
            List<PortForwardEntry> entries = [];
            foreach (Match m in EntryRegex().Matches(stdout))
            {
                if (int.TryParse(m.Groups[2].Value, out int lp) && int.TryParse(m.Groups[4].Value, out int cp))
                {
                    entries.Add(new PortForwardEntry(m.Groups[1].Value, lp, m.Groups[3].Value, cp));
                }
            }
            return entries;
        }

        /// <summary>指定した listenAddress:listenPort に一致するエントリの転送先アドレスを取得する。</summary>
        public string? GetConnectAddress(string listenAddress, int listenPort, int connectPort)
        {
            (int _, string stdout, string _) = runner.Run("netsh", "interface", "portproxy", "show", "v4tov4");
            string pattern = $@"^\s*{Regex.Escape(listenAddress)}\s+{listenPort}\s+(\d+\.\d+\.\d+\.\d+)\s+{connectPort}";
            Match match = Regex.Match(stdout, pattern, RegexOptions.Multiline);
            return match.Success ? match.Groups[1].Value : null;
        }

        /// <summary>ポート転送を追加する。既存の同一エントリは先に削除する。</summary>
        public void Add(PortForwardEntry entry)
        {
            _ = Delete(entry.ListenAddress, entry.ListenPort);

            (int exitCode, _, string stderr) = runner.Run("netsh", "interface", "portproxy", "add", "v4tov4",
                $"listenaddress={entry.ListenAddress}", $"listenport={entry.ListenPort}",
                $"connectaddress={entry.ConnectAddress}", $"connectport={entry.ConnectPort}");

            if (exitCode != 0)
            {
                throw new InvalidOperationException($"ポート転送の追加に失敗しました: {stderr}");
            }
        }

        /// <summary>ポート転送を削除する。存在しなければ false。</summary>
        public bool Delete(string listenAddress, int listenPort)
        {
            (int exitCode, string _, string _) = runner.Run("netsh", "interface", "portproxy", "delete", "v4tov4",
                $"listenaddress={listenAddress}", $"listenport={listenPort}");
            return exitCode == 0;
        }

        [GeneratedRegex(@"^\s*(\S+)\s+(\d+)\s+(\S+)\s+(\d+)\s*$", RegexOptions.Multiline)]
        private static partial Regex EntryRegex();
    }
}
