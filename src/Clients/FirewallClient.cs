using WindowsFirewallHelper;

namespace WslForward
{
    /// <summary>Firewall ルールの設定情報。</summary>
    internal sealed record FirewallRuleInfo(
        string Name,
        bool Enabled,
        string Direction,
        string Action,
        string Protocol,
        string LocalPorts,
        string Profiles
    );

    /// <summary>Windows Defender Firewall のルール操作を提供する。</summary>
    internal sealed class FirewallClient(string ruleName)
    {
        /// <summary>管理対象のルール名。</summary>
        public string RuleName => ruleName;

        /// <summary>ルールの情報を取得する。存在しなければ null。</summary>
        public FirewallRuleInfo? GetRule()
        {
            IFirewallRule? rule = FindRule();
            return rule == null
                ? null
                : new FirewallRuleInfo(
                Name: rule.Name,
                Enabled: rule.IsEnable,
                Direction: rule.Direction == FirewallDirection.Inbound ? "Inbound" : "Outbound",
                Action: rule.Action == FirewallAction.Allow ? "Allow" : "Block",
                Protocol: rule.Protocol.ProtocolNumber switch { 6 => "TCP", 17 => "UDP", var n => n.ToString(System.Globalization.CultureInfo.InvariantCulture) },
                LocalPorts: rule.LocalPorts.Length > 0 ? string.Join(", ", rule.LocalPorts.Select(p => p.ToString(System.Globalization.CultureInfo.InvariantCulture))) : "Any",
                Profiles: FormatProfiles(rule.Profiles)
            );
        }

        /// <summary>ルールを作成する。既存の同名ルールは先に削除する。</summary>
        public void AddRule(int port)
        {
            _ = DeleteRule();

            IFirewallRule rule = FirewallManager.Instance.CreatePortRule(
                FirewallProfiles.Domain | FirewallProfiles.Private | FirewallProfiles.Public,
                ruleName,
                FirewallAction.Allow,
                (ushort)port,
                FirewallProtocol.TCP);
            rule.Direction = FirewallDirection.Inbound;
            rule.IsEnable = true;

            FirewallManager.Instance.Rules.Add(rule);
        }

        /// <summary>ルールを削除する。存在しなければ false。</summary>
        public bool DeleteRule()
        {
            IFirewallRule? rule = FindRule();
            if (rule == null)
            {
                return false;
            }

            _ = FirewallManager.Instance.Rules.Remove(rule);
            return true;
        }

        private IFirewallRule? FindRule()
        {
            return FirewallManager.Instance.Rules
                .FirstOrDefault(r => string.Equals(r.Name, ruleName, StringComparison.OrdinalIgnoreCase));
        }

        private static string FormatProfiles(FirewallProfiles profiles)
        {
            List<string> parts = [];
            if (profiles.HasFlag(FirewallProfiles.Domain))
            {
                parts.Add("Domain");
            }

            if (profiles.HasFlag(FirewallProfiles.Private))
            {
                parts.Add("Private");
            }

            if (profiles.HasFlag(FirewallProfiles.Public))
            {
                parts.Add("Public");
            }

            return parts.Count > 0 ? string.Join(", ", parts) : "None";
        }
    }
}
