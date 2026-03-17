using System.Diagnostics;

namespace FrontOfficeERP.Services;

public class PrintService
{
    /// <summary>
    /// Opens a file with the default application (useful for printing PDFs).
    /// </summary>
    public static void OpenFile(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        Process.Start(new ProcessStartInfo
        {
            FileName = filePath,
            UseShellExecute = true
        });
    }

    /// <summary>
    /// Sends a file to the default printer using the system print verb.
    /// </summary>
    public static void PrintFile(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                Verb = "print",
                UseShellExecute = true,
                CreateNoWindow = true
            });
        }
        catch
        {
            // Fallback: open the file so user can print manually
            OpenFile(filePath);
        }
    }
}
