using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using WslForward.Handlers;

namespace WslForward
{
    internal static class Program
    {
        private static Config Cfg = new();
        private static readonly ProcessRunner Proc = new();
        private static WslClient Wsl = null!;
        private static FirewallClient Fw = null!;
        private static TaskSchedulerClient Tasks = null!;
        private static PortForwardClient Pf = null!;

        private const string FirewallRuleName = "WSL Forward";
        private static string LogonTaskName => BuildTaskName("WSL Forward - Logon");
        private static string IntervalTaskName => BuildTaskName("WSL Forward - Interval");

        private static string BuildTaskName(string baseName)
        {
            string folder = Cfg.TaskFolder.Trim().TrimStart('\\');
            return string.IsNullOrEmpty(folder) ? baseName : $@"{folder}\{baseName}";
        }
        private static string[] TaskNames => [LogonTaskName, IntervalTaskName];

        private sealed record Command(string Name, string Description, bool Admin, Action Handler);

        private static readonly Command[] Commands =
        [
            new("init-config",        "デフォルト値の config.json を作成する",                false, InitConfigHandler.Execute),
            new("install",            "Firewall + タスク登録 + ポート転送を一括セットアップする", true,  () => InstallHandler.Execute(Fw, Cfg, Tasks, LogonTaskName, IntervalTaskName, Wsl, Pf)),
            new("install-firewall",   "Firewall の受信許可ルールを作成する",                  true,  () => InstallFirewallHandler.Execute(Fw, Cfg)),
            new("install-tasks",      "タスクスケジューラにタスクを登録する",                 true,  () => InstallTasksHandler.Execute(Tasks, LogonTaskName, IntervalTaskName)),
            new("show-firewall",      "Firewall ルールを表示する",                            false, () => ShowFirewallHandler.Execute(Fw)),
            new("show-tasks",         "タスクスケジューラの登録状況を表示する",               false, () => ShowTasksHandler.Execute(Tasks, TaskNames)),
            new("show-forward",       "ポート転送設定を表示する",                             false, () => ShowForwardHandler.Execute(Pf)),
            new("show-wsl",           "WSL ディストリビューションの状態を表示する",           false, () => ShowWslHandler.Execute(Wsl, Cfg)),
            new("start-wsl",          "WSL ディストリビューションを起動する",                 false, () => StartWslHandler.Execute(Wsl)),
            new("update",             "WSL を起動し、ポート転送を差分更新する",               true,  () => UpdateHandler.Execute(Wsl, Pf, Cfg)),
            new("uninstall-firewall", "Firewall ルールを削除する",                            true,  () => UninstallFirewallHandler.Execute(Fw)),
            new("uninstall-tasks",    "タスクスケジューラのタスクを削除する",                 false, () => UninstallTasksHandler.Execute(Tasks, TaskNames)),
            new("uninstall-forward",  "ポート転送設定を削除する",                             true,  () => UninstallForwardHandler.Execute(Pf, Cfg)),
            new("uninstall",          "全設定を一括削除する",                                 true,  () => UninstallHandler.Execute(Pf, Cfg, Fw, Tasks, TaskNames)),
        ];

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(int dwProcessId);

        private static int Main(string[] args)
        {
            _ = AttachConsole(-1);
            Console.OutputEncoding = Encoding.UTF8;

            Cfg = Config.Load();
            Wsl = new WslClient(Proc, Cfg.DistroName);
            Fw = new FirewallClient(FirewallRuleName);
            Tasks = new TaskSchedulerClient();
            Pf = new PortForwardClient(Proc);

            if (args.Length == 0 || args[0] == "help")
            {
                PrintHelp();
                return 0;
            }

            try
            {
                Command? cmd = Commands.FirstOrDefault(c => c.Name == args[0]);
                if (cmd == null)
                {
                    return UnknownCommand(args[0]);
                }

                if (cmd.Admin && !RequireAdministrator())
                {
                    return 1;
                }

                cmd.Handler();
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ERROR] {ex.Message}");
                return 1;
            }
        }

        private static bool RequireAdministrator()
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new(identity);
            if (principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                return true;
            }

            Console.Error.WriteLine("[ERROR] このコマンドの実行には管理者権限が必要です。管理者として実行してください。");
            return false;
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Usage: WslForward <command>");
            Console.WriteLine("");
            Console.WriteLine("Commands:");
            int maxLen = Commands.Max(c => c.Name.Length);
            foreach (Command cmd in Commands)
            {
                string admin = cmd.Admin ? " *" : "";
                Console.WriteLine($"  {cmd.Name.PadRight(maxLen + 2)}{cmd.Description}{admin}");
            }
            Console.WriteLine($"  {"help".PadRight(maxLen + 2)}このヘルプを表示する");
            Console.WriteLine("");
            Console.WriteLine("  * 管理者権限が必要");
        }

        private static int UnknownCommand(string command)
        {
            Console.Error.WriteLine($"不明なコマンド: {command}");
            Console.WriteLine("");
            PrintHelp();
            return 1;
        }
    }
}
