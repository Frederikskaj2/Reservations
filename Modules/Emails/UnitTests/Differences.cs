using System.Diagnostics;
using System.IO;

namespace Frederikskaj2.Reservations.Emails.UnitTests;

static class Differences
{
    public static void Show(string? actual, string? expected)
    {
        if (expected == actual)
            return;
        var expectedFileName = Path.GetTempFileName();
        var actualFileName = Path.GetTempFileName();
        try
        {
            File.WriteAllText(expectedFileName, expected ?? "");
            File.WriteAllText(actualFileName, actual ?? "");
            Process.Start(@"C:\Program Files\WinMerge\WinMergeU.exe", [expectedFileName, actualFileName]).WaitForExit();
        }
        finally
        {
            File.Delete(expectedFileName);
            File.Delete(actualFileName);
        }
    }
}
