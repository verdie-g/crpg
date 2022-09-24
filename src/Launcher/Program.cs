using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using Microsoft.Win32;

namespace Crpg.Launcher;

internal static class Program
{
    private const string BannerlordExePath = "bin/Win64_Shipping_Client/Bannerlord.exe";
    private const string BannerlordCrpgPath = "Modules/cRPG";
    private const string BannerlordSteamPath = "steamapps/common/Mount & Blade II Bannerlord";
    private const string CrpgZipUrl = "https://c-rpg.eu/cRPG.zip";

    [STAThread]
    internal static void Main()
    {
        MainAsync().Wait();
    }

    private static async Task MainAsync()
    {
        string? bannerlordPath = ResolveBannerlordPath();
        if (bannerlordPath == null)
        {
            MessageBox.Show("Could not find the location of your Bannerlord installation.", "Bannerlord not found",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (!CheckSteamIsRunning())
        {
            MessageBox.Show("Steam is not running. You need to run steam to play cRPG.", "Steam is not running",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            await UpdateCrpgAsync(bannerlordPath);
        }
        catch
        {
            MessageBox.Show(
                "Could not update cRPG. The game will still launch but you might get a version mismatch error",
                "cRPG update failed",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        string bannerlordExePath = Path.Combine(bannerlordPath, BannerlordExePath);
        Process.Start(new ProcessStartInfo
        {
            WorkingDirectory = Path.GetDirectoryName(bannerlordExePath),
            FileName = "Bannerlord.exe",
            Arguments = "_MODULES_*Native*cRPG*_MODULES_ /multiplayer",
            UseShellExecute = true,
        });
    }

    private static string? ResolveBannerlordPath()
    {
        string? bannerlordPath = ResolveBannerlordPathFromRegistry();
        if (bannerlordPath != null)
        {
            return bannerlordPath;
        }

        string bannerlordPathFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Mount and Blade II Bannerlord/cRPG/BannerlordPath.txt");
        if (File.Exists(bannerlordPathFile))
        {
            bannerlordPath = File.ReadAllText(bannerlordPathFile);
            if (File.Exists(bannerlordPath))
            {
                return bannerlordPath;
            }
        }

        bannerlordPath = OpenDialogForBannerlordPath();
        if (bannerlordPath != null)
        {
            Directory.CreateDirectory(Directory.GetParent(bannerlordPathFile)!.FullName);
            File.WriteAllText(bannerlordPathFile, bannerlordPath);
            return bannerlordPath;
        }

        return null;
    }

    private static string? ResolveBannerlordPathFromRegistry()
    {
        string? steamPath = (string?)Registry.GetValue("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamPath", null);
        if (steamPath == null)
        {
            return null;
        }

        VProperty vdf = VdfConvert.Deserialize(File.ReadAllText(Path.Combine(steamPath, "steamapps/libraryfolders.vdf")));

        for (int i = 0; ; i += 1)
        {
            string index = i.ToString();
            if (vdf.Value[index] == null)
            {
                break;
            }

            string? path = vdf.Value[index]?["path"]?.ToString();
            if (path == null)
            {
                continue;
            }

            string bannerlordPath = Path.Combine(path, BannerlordSteamPath);
            if (File.Exists(Path.Combine(bannerlordPath, BannerlordExePath)))
            {
                return bannerlordPath;
            }
        }

        return null;
    }

    private static string? OpenDialogForBannerlordPath()
    {
        var dialogResult = MessageBox.Show(
            "Could not find your Mount & Blade II Bannerlord location.\n\nPlease select your Mount & Blade II Bannerlord directory.",
            "Mount & Blade II Bannerlord not found",
            MessageBoxButtons.OKCancel,
            MessageBoxIcon.Warning);
        if (dialogResult != DialogResult.OK)
        {
            return null;
        }

        using var fbd = new FolderBrowserDialog();
        dialogResult = fbd.ShowDialog();

        if (dialogResult != DialogResult.OK
            || fbd.SelectedPath == null
            || !File.Exists(Path.Combine(fbd.SelectedPath, BannerlordExePath)))
        {
            return null;
        }

        return fbd.SelectedPath;
    }

    private static bool CheckSteamIsRunning()
    {
        return Process.GetProcessesByName("steam").Length != 0;
    }

    private static async Task UpdateCrpgAsync(string bannerlordPath)
    {
        string crpgPath = Path.Combine(bannerlordPath, BannerlordCrpgPath);
        string tagPath = Path.Combine(crpgPath, "Tag.txt");
        string? tag = File.Exists(tagPath) ? File.ReadAllText(tagPath) : null;

        using HttpClient httpClient = new();
        HttpRequestMessage req = new(HttpMethod.Get, CrpgZipUrl);
        if (tag != null)
        {
            req.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(tag));
        }

        var res = await httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
        if (res.StatusCode == HttpStatusCode.NotModified)
        {
            return;
        }

        res.EnsureSuccessStatusCode();

        using (var contentStream = await res.Content.ReadAsStreamAsync())
        using (ZipArchive archive = new(contentStream))
        {
            Directory.Delete(crpgPath, true);
            Directory.CreateDirectory(crpgPath);
            archive.ExtractToDirectory(crpgPath); // No async overload :(
        }

        tag = res.Headers.ETag?.Tag;
        if (tag != null)
        {
            File.WriteAllText(tagPath, tag);
        }
    }
}
