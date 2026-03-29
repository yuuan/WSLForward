using System.Diagnostics;
using System.Text;

namespace WslForward
{
    /// <summary>外部プロセスを実行し、結果を取得する。</summary>
    internal sealed class ProcessRunner
    {
        /// <summary>指定したエンコーディングで外部プロセスを実行する。</summary>
        public (int exitCode, string stdout, string stderr) Run(
            string fileName, Encoding? outputEncoding, params string[] args)
        {
            ProcessStartInfo psi = new()
            {
                FileName = fileName,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            foreach (string arg in args)
            {
                psi.ArgumentList.Add(arg);
            }

            if (outputEncoding != null)
            {
                psi.StandardOutputEncoding = outputEncoding;
            }

            using Process process = Process.Start(psi)
                ?? throw new InvalidOperationException($"プロセスの起動に失敗しました: {fileName}");
            // stderr を非同期で読み、stdout の ReadToEnd とのデッドロックを防ぐ
            Task<string> stderrTask = process.StandardError.ReadToEndAsync();
            string stdout = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            string stderr = stderrTask.GetAwaiter().GetResult();
            return (process.ExitCode, stdout, stderr);
        }

        /// <summary>デフォルトエンコーディングで外部プロセスを実行する。</summary>
        public (int exitCode, string stdout, string stderr) Run(
            string fileName, params string[] args)
        {
            return Run(fileName, null, args);
        }
    }
}
