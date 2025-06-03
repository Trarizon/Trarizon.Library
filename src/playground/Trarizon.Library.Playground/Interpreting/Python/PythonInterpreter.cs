using System.Diagnostics;

namespace Trarizon.Library.Playground.Interpreting.Python;
internal class PythonInterpreter
{
    public void Intepret(string code)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = $@"-c ""{code.Replace("\"", @"\""")}""",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        Process.Start(psi);
    }
}
