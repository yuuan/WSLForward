using System.Text.Json;
using System.Text.Json.Serialization;

namespace WslForward
{
    /// <summary>アプリケーション設定。config.json から読み込まれる。</summary>
    internal sealed record Config(
        [property: JsonPropertyName("distroName")] string DistroName = "Ubuntu",
        [property: JsonPropertyName("listenAddress")] string ListenAddress = "0.0.0.0",
        [property: JsonPropertyName("listenPort")] int ListenPort = 22,
        [property: JsonPropertyName("wslPort")] int WslPort = 22,
        [property: JsonPropertyName("taskFolder")] string TaskFolder = "WSL Forward"
    )
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        /// <summary>config.json のパスを返す。</summary>
        public static string GetPath()
        {
            string exeDir = Path.GetDirectoryName(Environment.ProcessPath) ?? ".";
            return Path.Combine(exeDir, "config.json");
        }

        /// <summary>exe と同じディレクトリの config.json から設定を読み込む。ファイルがなければデフォルト値を返す。</summary>
        public static Config Load()
        {
            string configPath = GetPath();

            if (!File.Exists(configPath))
            {
                return new Config();
            }

            string json = File.ReadAllText(configPath);
            return JsonSerializer.Deserialize<Config>(json) ?? new Config();
        }

        /// <summary>デフォルト値の config.json を作成する。既に存在する場合は作成しない。</summary>
        public static (bool created, string path) Init()
        {
            string configPath = GetPath();

            if (File.Exists(configPath))
            {
                return (false, configPath);
            }

            string json = JsonSerializer.Serialize(new Config(), JsonOptions);
            File.WriteAllText(configPath, json);
            return (true, configPath);
        }
    }
}
