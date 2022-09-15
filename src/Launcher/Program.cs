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
    private const string CrpgWebsite = "http://c-rpg.eu";
    private const string CrpgModFile = "cRPG.zip";
    private const string DownloadUrl = CrpgWebsite + "/" + CrpgModFile;
    private const string UserRoot = "HKEY_CURRENT_USER";
    private const string Subkey = @"Software\Valve\Steam";
    private const string KeyName = UserRoot + "\\" + Subkey;
    private const string CrpgLauncherConfig = @"\CrpgLauncherPath.txt";
    private const string CrpgLauncherVersion = @"\CrpgLauncherVersion.txt";

    [STAThread]
    private static void Main()
    {
        MainAsync().GetAwaiter().GetResult();
    }

    private static async Task MainAsync()
    {
        string crpgDocumentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Mount and Blade II Bannerlord\Configs";
        bool crpgLauncherConfigFound = false;
        string targetPath = string.Empty;
        string configPath = crpgDocumentPath + CrpgLauncherConfig;
        string versionPath = crpgDocumentPath + CrpgLauncherVersion;

        var (updateAvailable, tag) = await UpdateAvailable(versionPath);

        if (Directory.Exists(crpgDocumentPath) && File.Exists(configPath))
        {
            crpgLauncherConfigFound = true;
            targetPath = File.ReadAllText(configPath);
            if (!Directory.Exists(targetPath))
            {
                crpgLauncherConfigFound = false;
                File.Delete(configPath);
            }
        }

        string? steamInstallPath = (string?)Registry.GetValue(KeyName, "SteamPath", null);
        if (!crpgLauncherConfigFound)
        {
            if (steamInstallPath != null)
            {
                string steamLibraryVdfPath = steamInstallPath + "\\steamapps\\libraryfolders.vdf";
                VProperty libraryVdf = VdfConvert.Deserialize(File.ReadAllText(steamLibraryVdfPath));

                List<string> steamBlPaths = new();
                int counter = 0;
                while (true)
                {
                    string index = counter.ToString();
                    if (libraryVdf.Value[index] == null)
                    {
                        break;
                    }

                    if (libraryVdf.Value[index]?["path"] == null)
                    {
                        continue;
                    }

                    string? path = libraryVdf.Value[index]?["path"]?.ToString();
                    path += @"\steamapps\common\Mount & Blade II Bannerlord";
                    steamBlPaths.Add(path);
                    counter++;
                }

                foreach (string steamBlPath in steamBlPaths)
                {
                    if (Directory.Exists(steamBlPath))
                    {
                        targetPath = steamBlPath;
                        break;
                    }
                }
            }
        }

        if (targetPath == string.Empty)
        {
            var result = MessageBox.Show("Could not find your Mount & Blade II Bannerlord location.\n\nPlease select your Mount & Blade II Bannerlord directory.", "Mount & Blade II Bannerlord not found",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning);
            if (result != DialogResult.OK)
            {
                return;
            }

            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult pickedDir = fbd.ShowDialog();

                if (pickedDir != DialogResult.OK || string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    return;
                }

                targetPath = fbd.SelectedPath;
            }
        }

        string blPathExe = targetPath + @"\bin\Win64_Shipping_Client\Bannerlord.exe";
        if (!File.Exists(blPathExe))
        {
            MessageBox.Show("Could not find your Bannerlord.exe", "Bannerlord.exe not found",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            if (crpgLauncherConfigFound) // Delete config if file was invalid
            {
                File.Delete(configPath);
            }

            return;
        }

        if (steamInstallPath != null && Directory.Exists(steamInstallPath))
        {
            while (true)
            {
                if (!IsProcessRunning("steam"))
                {
                    var result = MessageBox.Show("Steam is not running. You need to run steam to play cRPG.", "Steam is not running",
                        MessageBoxButtons.AbortRetryIgnore,
                        MessageBoxIcon.Warning);
                    if (result == DialogResult.Abort)
                    {
                        return;
                    }
                    else if (result == DialogResult.Retry)
                    {
                        continue;
                    }
                    else if (result == DialogResult.Ignore)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        if (!crpgLauncherConfigFound)
        {
            File.WriteAllText(configPath, targetPath);
        }

        if (updateAvailable)
        {
            bool updated = await UpdateFiles(DownloadUrl, targetPath);
            if (updated)
            {
                File.WriteAllText(versionPath, tag ?? "error");
            }
        }

        ProcessStartInfo startInfo = new();
        startInfo.WorkingDirectory = Path.GetDirectoryName(blPathExe);
        startInfo.FileName = "Bannerlord.exe";
        startInfo.Arguments = "_MODULES_*Native*cRPG*_MODULES_ /multiplayer";
        startInfo.UseShellExecute = true;

        Process.Start(startInfo);
    }

    private static async Task<(bool updateAvailable, string? tag)> UpdateAvailable(string versionPath)
    {
        string? tag = null;
        if (File.Exists(versionPath))
        {
            tag = File.ReadAllText(versionPath);
        }

        using HttpClient httpClient = new() { BaseAddress = new Uri(CrpgWebsite) };
        HttpRequestMessage req = new(HttpMethod.Get, CrpgModFile);
        if (tag != null)
        {
            req.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(tag));
        }

        var res = await httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
        if (res.StatusCode == HttpStatusCode.NotModified)
        {
            return (false, null);
        }

        try
        {
            res.EnsureSuccessStatusCode();
            tag = res.Headers.ETag?.Tag;
        }
        catch (HttpRequestException)
        {
            MessageBox.Show("Could not check for any updates.", "Update check failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return (false, null);
        }

        return (true, tag);
    }

    private static async Task<bool> UpdateFiles(string downloadUrl, string targetPath)
    {
        string modulesPath = targetPath + @"\Modules";
        string crpgPath = modulesPath + @"\cRPG";

        string timeStamp = DateTime.Now.ToFileTime().ToString();
        string downloadPath = Path.GetTempPath() + @"\cRPG" + timeStamp + ".zip";

        var httpClient = new HttpClient();
        var httpResult = await httpClient.GetAsync(downloadUrl);
        using var resultStream = await httpResult.Content.ReadAsStreamAsync();
        using var fileStream = File.Create(downloadPath);
        resultStream.CopyTo(fileStream);
        fileStream.Close();

        if (!File.Exists(downloadPath))
        {
            return false;
        }

        if (Directory.Exists(crpgPath))
        {
            Directory.Delete(crpgPath, true);
        }

        Directory.CreateDirectory(crpgPath);
        ZipFile.ExtractToDirectory(downloadPath, crpgPath);
        File.Delete(downloadPath);
        return true;
    }

    private static bool IsProcessRunning(string name)
    {
        Process[] pname = Process.GetProcessesByName(name);
        return pname.Length != 0;
    }
}
