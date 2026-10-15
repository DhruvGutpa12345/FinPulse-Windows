using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        try
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string target = Path.Combine(baseDir, "standalone", "FinPulse.exe");
            if (!File.Exists(target))
            {
                MessageBox.Show("Could not find FinPulse application binary at:\n" + target, "FinPulse Windows", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = target,
                WorkingDirectory = Path.Combine(baseDir, "standalone"),
                UseShellExecute = true
            };
            if (args != null && args.Length > 0)
            {
                startInfo.Arguments = string.Join(" ", args);
            }
            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Failed to launch FinPulse:\n" + ex.Message, "FinPulse Windows", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
