using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using Microsoft.Win32;

string? bannerlordPath = ResolveBannerlordPath();
if (bannerlordPath == null)
{
    Console.WriteLine("Could not find the location of your Bannerlord installation. Contact a moderator on discord.");
    Console.Read();
    return;
}

if (!CheckSteamIsRunning())
{
    Console.WriteLine("Steam is not running. Run it and try again.");
    Console.Read();
    return;
}

try
{
    await UpdateCrpgAsync(bannerlordPath);
}
catch
{
    Console.WriteLine("Could not update cRPG. The game will still launch but you might get a version mismatch error.");
    Console.Read();
}

string bannerlordExePath = Path.Combine(bannerlordPath, "bin/Win64_Shipping_Client/Bannerlord.exe");
Process.Start(new ProcessStartInfo
{
    WorkingDirectory = Path.GetDirectoryName(bannerlordExePath),
    FileName = "Bannerlord.exe",
    Arguments = "_MODULES_*Native*cRPG*_MODULES_ /multiplayer",
    UseShellExecute = true,
});

static string? ResolveBannerlordPath()
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

    bannerlordPath = AskForBannerlordPath();
    if (bannerlordPath != null)
    {
        Directory.CreateDirectory(Directory.GetParent(bannerlordPathFile)!.FullName);
        File.WriteAllText(bannerlordPathFile, bannerlordPath);
        return bannerlordPath;
    }

    return null;
}

static string? ResolveBannerlordPathFromRegistry()
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

        string bannerlordPath = Path.Combine(path, "steamapps/common/Mount & Blade II Bannerlord");
        if (File.Exists(Path.Combine(bannerlordPath, "bin/Win64_Shipping_Client/Bannerlord.exe")))
        {
            return bannerlordPath;
        }
    }

    return null;
}

static string? AskForBannerlordPath()
{
    string? bannerlordPath = null;
    while (bannerlordPath == null)
    {
        Console.WriteLine("Enter your Mount & Blade II Bannerlord location (e.g. D:\\Steam\\steamapps\\common\\Mount & Blade II Bannerlord):");
        bannerlordPath = Console.ReadLine();
        if (bannerlordPath == null)
        {
            break;
        }

        bannerlordPath = bannerlordPath.Trim();
        string bannerlordExePath = Path.Combine(bannerlordPath, "bin/Win64_Shipping_Client/Bannerlord.exe");
        if (!File.Exists(bannerlordExePath))
        {
            Console.WriteLine($"Could not find Bannerlord at '{bannerlordExePath}'");
            bannerlordPath = null;
        }
    }

    return bannerlordPath;
}

static bool CheckSteamIsRunning()
{
    return Process.GetProcessesByName("steam").Length != 0;
}

static async Task UpdateCrpgAsync(string bannerlordPath)
{
    string crpgPath = Path.Combine(bannerlordPath, "Modules/cRPG");
    string tagPath = Path.Combine(crpgPath, "Tag.txt");
    string? tag = File.Exists(tagPath) ? File.ReadAllText(tagPath) : null;

    using HttpClient httpClient = new();
    HttpRequestMessage req = new(HttpMethod.Get, "https://c-rpg.eu/cRPG.zip");
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

    long contentLength = res.Content.Headers.ContentLength!.Value;
    await using var contentStream = await res.Content.ReadAsStreamAsync();
    using MemoryStream ms = await DownloadWithProgressBarAsync(contentStream, contentLength);

    using (ZipArchive archive = new(ms))
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

static async Task<MemoryStream> DownloadWithProgressBarAsync(Stream stream, long length)
{
    MemoryStream ms = new();
    byte[] buffer = new byte[100 * 1000];
    int totalBytesRead = 0;
    int bytesRead;
    while ((bytesRead = await stream.ReadAsync(buffer)) != 0)
    {
        ms.Write(buffer, 0, bytesRead);

        totalBytesRead += bytesRead;
        float progression = (float)Math.Round(100 * (float)totalBytesRead / length, 2);
        string lengthStr = length.ToString();
        string totalBytesReadStr = totalBytesRead.ToString().PadLeft(lengthStr.Length);
        Console.WriteLine($"Downloading {totalBytesReadStr} / {lengthStr} ({progression}%)");
    }

    return ms;
}
