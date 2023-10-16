using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Web;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using Microsoft.Win32;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

Console.WriteLine("Make sure Steam/Epic Games/Xbox is running and that Bannerlord is up-to-date");
(bool isServer,bool isBeta, string path) = IsServerLauncher(args);
if (isServer)
{
    if (path == null)
    {
        Console.WriteLine("invalid path");
        return;
    }

    try
    {
        await UpdateCrpgAsync(path, isBeta, isServer);
        return;
    }
    catch (Exception e)
    {
        Console.WriteLine("Could not update cRPG Server.");
        Console.WriteLine(e);
        Console.Read();
        return;
    }
}

var bannerlordInstallation = ResolveBannerlordInstallation();
if (bannerlordInstallation == null)
{
    Console.WriteLine("Could not find the location of your Bannerlord installation. Contact a moderator on discord. Press enter to exit.");
    Console.Read();
    return;
}

Console.WriteLine($"Using Bannerlord installed at '{bannerlordInstallation.InstallationPath}'");

try
{
    await UpdateCrpgAsync(bannerlordInstallation.InstallationPath);
}
catch (Exception e)
{
    Console.WriteLine("Could not update cRPG. The game will still launch but you might get a version mismatch error. Press enter to continue.");
    Console.WriteLine(e);
    Console.Read();
}

Process.Start(new ProcessStartInfo
{
    WorkingDirectory = bannerlordInstallation.ProgramWorkingDirectory,
    FileName = bannerlordInstallation.Program,
    Arguments = bannerlordInstallation.ProgramArguments ?? string.Empty,
    UseShellExecute = true,
});
Console.WriteLine("Make sure Steam/Epic Games/Xbox is running and that Bannerlord is up-to-date");

static GameInstallationInfo? ResolveBannerlordInstallation()
{
    GameInstallationInfo? bannerlordInstallation = ResolveBannerlordSteamInstallation();
    if (bannerlordInstallation != null)
    {
        return bannerlordInstallation;
    }

    bannerlordInstallation = ResolveBannerlordEpicGamesInstallation();
    if (bannerlordInstallation != null)
    {
        return bannerlordInstallation;
    }

    bannerlordInstallation = ResolveBannerlordXboxInstallation();
    if (bannerlordInstallation != null)
    {
        return bannerlordInstallation;
    }

    return null;
}

static GameInstallationInfo? ResolveBannerlordSteamInstallation()
{
    string? steamPath = (string?)Registry.GetValue("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamPath", null);
    if (steamPath == null)
    {
        return null;
    }

    string vdfPath = Path.Combine(steamPath, "steamapps/libraryfolders.vdf");
    if (!File.Exists(vdfPath))
    {
        return null;
    }

    VProperty vdf = VdfConvert.Deserialize(File.ReadAllText(vdfPath));

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
        string bannerlordExePath = Path.Combine(bannerlordPath, "bin/Win64_Shipping_Client/Bannerlord.exe");
        if (File.Exists(bannerlordExePath))
        {
            return new GameInstallationInfo(
                bannerlordPath,
                bannerlordExePath,
                "_MODULES_*Native*cRPG*_MODULES_ /multiplayer",
                Path.GetDirectoryName(bannerlordExePath));
        }
    }

    return null;
}

static GameInstallationInfo? ResolveBannerlordEpicGamesInstallation()
{
    const string appName = "Chickadee";

    string manifestsFolderPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "Epic/EpicGamesLauncher/Data/Manifests");
    if (!Directory.Exists(manifestsFolderPath))
    {
        return null;
    }

    foreach (string manifestPath in Directory.EnumerateFiles(manifestsFolderPath, "*.item"))
    {
        var manifestDoc = JsonSerializer.Deserialize<JsonDocument>(File.ReadAllText(manifestPath));
        if (manifestDoc == null
            || !manifestDoc.RootElement.TryGetProperty("AppName", out var appNameEl)
            || appNameEl.GetString() != appName)
        {
            continue;
        }

        string bannerlordPath = manifestDoc.RootElement.GetProperty("InstallLocation").GetString()!;
        string catalogNamespace = manifestDoc.RootElement.GetProperty("CatalogNamespace").GetString()!;
        string catalogItemId = manifestDoc.RootElement.GetProperty("CatalogItemId").GetString()!;

        string app = $"{catalogNamespace}:{catalogItemId}:{appName}";
        string program = $"com.epicgames.launcher://apps/{HttpUtility.UrlEncode(app)}?action=launch&silent=true";

        string bannerlordExePath = Path.Combine(bannerlordPath, "bin/Win64_Shipping_Client/Bannerlord.exe");
        if (File.Exists(bannerlordExePath))
        {
            return new GameInstallationInfo(bannerlordPath, program, null, null);
        }
    }

    return null;
}

static GameInstallationInfo? ResolveBannerlordXboxInstallation()
{
    // I couldn't find a smart way to find an xbox game so let's just try to find a XboxGames folder in single letter disks.
    for (char disk = 'A'; disk <= 'Z'; disk += (char)1)
    {
        string bannerlordPath = disk + ":/XboxGames/Mount & Blade II- Bannerlord/Content";
        string bannerlordExePath = Path.Combine(bannerlordPath, "bin/Gaming.Desktop.x64_Shipping_Client/Launcher.Native.exe");
        if (File.Exists(bannerlordExePath))
        {
            return new GameInstallationInfo(bannerlordPath, bannerlordExePath, null, null);
        }
    }

    return null;
}

static async Task UpdateCrpgAsync(string bannerlordPath, bool isBeta = false,bool isServer = false)
{
    string crpgPath = Path.Combine(bannerlordPath, "Modules/cRPG");
    string tagPath = Path.Combine(crpgPath, "Tag.txt");
    string? tag = File.Exists(tagPath) ? File.ReadAllText(tagPath) : null;
    string websiteUrl = "https://c-rpg.eu/";
    string fileName = "cRPG.zip";
    if (isBeta)
    {
        websiteUrl = "https://namidaka.fr/";
        crpgPath = Path.Combine(bannerlordPath, "Modules/cRPG_Beta");
    }

    if (isServer)
    {
        fileName = "cRPGServer.zip";
    }

    string crpgUrl = websiteUrl + fileName;
    using HttpClient httpClient = new(new SocketsHttpHandler
    {
        AllowAutoRedirect = true,
    });
    HttpRequestMessage req = new(HttpMethod.Get, crpgUrl);
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

    string temporaryFilePath = Path.GetTempFileName() + ".zip";
    await using var temporaryFileStream = File.Create(temporaryFilePath);

    await CopyToWithProgressBarAsync(contentStream, contentLength, temporaryFileStream);

    temporaryFileStream.Seek(0, SeekOrigin.Begin);
    using (ZipArchive archive = new(temporaryFileStream))
    {
        if (Directory.Exists(crpgPath))
        {
            Directory.Delete(crpgPath, true);
        }

        Directory.CreateDirectory(crpgPath);
        archive.ExtractToDirectory(crpgPath); // No async overload :(
    }

    tag = res.Headers.ETag?.Tag;
    if (tag != null)
    {
        File.WriteAllText(tagPath, tag);
    }

    File.Delete(temporaryFilePath);
}

static async Task CopyToWithProgressBarAsync(
    Stream inputStream,
    long inputStreamLength,
    Stream outputStream)
{
    byte[] buffer = new byte[100 * 1000];
    long totalBytesRead = 0;
    int bytesRead;

    while ((bytesRead = await inputStream.ReadAsync(buffer)) != 0)
    {
        outputStream.Write(buffer, 0, bytesRead);

        totalBytesRead += bytesRead;
        float progression = (float)Math.Round(100 * (float)totalBytesRead / inputStreamLength, 2);
        string lengthStr = inputStreamLength.ToString();
        string totalBytesReadStr = totalBytesRead.ToString().PadLeft(lengthStr.Length);
        Console.Write($"\rDownloading {totalBytesReadStr} / {lengthStr} ({progression:00.00}%)");
    }

    Console.WriteLine();
}

static (bool isServer,bool isBeta, string path) IsServerLauncher(string[] args)
{
    bool isServer = false;
    string path = string.Empty;
    bool isBeta = false;
    for (int i = 0; i < args.Length; i++)
    {
        switch (args[i].ToLower()) // Using ToLower for case-insensitive comparison
        {
            case "-server":
                isServer = true;
                break;
            case "-path":
                if (i + 1 < args.Length) // Ensure there's another argument after -path
                {
                    path = args[++i].Trim('"');
                }
                else
                {
                    Console.WriteLine("Error: Expected a path after -path argument.");
                }

                break;
            case "-beta":
                isBeta = true;
                break;
            default:
                Console.WriteLine($"Warning: Unknown argument {args[i]}");
                break;
        }
    }

    return (isServer, isBeta, path);
}

record GameInstallationInfo(string InstallationPath, string Program, string? ProgramArguments, string? ProgramWorkingDirectory);
