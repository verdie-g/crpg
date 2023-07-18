using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using LibGit2Sharp;
using Microsoft.Win32;

string? bannerlordPath = ResolveBannerlordPath();
if (bannerlordPath == null)
{
    Console.WriteLine("Could not find the location of your Bannerlord installation. Contact a moderator on discord. Press enter to exit.");
    Console.Read();
    return;
}

Console.WriteLine($"Using Bannerlord installed at '{bannerlordPath}'");

try
{
    await UpdateCrpgAsync(bannerlordPath);
}
catch (Exception e)
{
    Console.WriteLine("Could not update cRPG. The game will still launch but you might get a version mismatch error. Press enter to continue.");
    Console.WriteLine(e);
    Console.Read();
}

string bannerlordExePath = Path.Combine(bannerlordPath, "bin/Win64_Shipping_Client/Bannerlord.exe");
Process.Start(new ProcessStartInfo
{
    WorkingDirectory = Path.GetDirectoryName(bannerlordExePath),
    FileName = "Bannerlord.exe",
    Arguments = "_MODULES_*Native*cRPG_Exporter*_MODULES_ /singleplayer",
    UseShellExecute = true,
});

static string? ResolveBannerlordPath()
{
    string? bannerlordPath = ResolveBannerlordPathSteam();
    if (bannerlordPath != null)
    {
        return bannerlordPath;
    }

    bannerlordPath = ResolveBannerlordPathEpicGames();
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

static string? ResolveBannerlordPathSteam()
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
        if (File.Exists(Path.Combine(bannerlordPath, "bin/Win64_Shipping_Client/Bannerlord.exe")))
        {
            return bannerlordPath;
        }
    }

    return null;
}

static string? ResolveBannerlordPathEpicGames()
{
    const string defaultBannerlordPath = "C:/Program Files/Epic Games/MountAndBlade2";
    if (File.Exists(Path.Combine(defaultBannerlordPath, "bin/Win64_Shipping_Client/Bannerlord.exe")))
    {
        return defaultBannerlordPath;
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

static async Task UpdateCrpgAsync(string bannerlordPath)
{
    string crpgPath = Path.Combine(bannerlordPath, "Modules/cRPG_Exporter");
    string moduleDataPath = Path.Combine(crpgPath, "ModuleData");
    string tagPath = Path.Combine(crpgPath, "Tag.txt");
    string? tag = File.Exists(tagPath) ? File.ReadAllText(tagPath) : null;

    using HttpClient httpClient = new();
    HttpRequestMessage req = new(HttpMethod.Get, "https://namidaka.fr/cRPG_Exporter.zip");
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
        if (!Directory.Exists(crpgPath))
        {
            Directory.CreateDirectory(crpgPath);
        }

        var existingDirectories = Directory.EnumerateDirectories(crpgPath)
                                           .Where(d => !d.Contains("ModuleData"))
                                           .ToList();
        List<string> existingFiles = new();

        foreach (var directory in existingDirectories)
        {
            existingFiles.AddRange(Directory.EnumerateFiles(directory));
        }

        foreach (var file in existingFiles)
        {
            File.Delete(file);
        }

        foreach (var directory in existingDirectories)
        {
            Directory.Delete(directory, true);
        }

        bool moduleDataAlreadyExists = Directory.Exists(moduleDataPath);
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            string completeFileName = Path.Combine(crpgPath, entry.FullName);
            if (completeFileName.Contains("ModuleData") && moduleDataAlreadyExists)
            {
                continue;
            }

            if (File.Exists(completeFileName))
            {
                File.Delete(completeFileName);
            }

            if (entry.Name == "")
            {
                // Creating an empty DirectoryInfo just for Directory.CreateDirectory
                Directory.CreateDirectory(completeFileName);
                continue;
            }

            entry.ExtractToFile(completeFileName);
        }

    }

    tag = res.Headers.ETag?.Tag;
        if (tag != null)
    {
        File.WriteAllText(tagPath, tag);
    }

    if (Directory.Exists(moduleDataPath))
    {
        await UpdateGitRepositoryAsync(moduleDataPath);
    }
}


static async Task UpdateGitRepositoryAsync(string repositoryPath)
{
    using var repo = new LibGit2Sharp.Repository(repositoryPath);

    // Preserve the current branch
    var currentBranch = repo.Head;
    Console.WriteLine($"Current branch is {currentBranch.UpstreamBranchCanonicalName})");

    // Check for unstaged changes
    var unstagedChanges = repo.RetrieveStatus(new LibGit2Sharp.StatusOptions { IncludeUntracked = true }).IsDirty;
    Stash? stash = null;
    if (unstagedChanges)
    {
        Console.WriteLine($"Stashing Changes for {currentBranch.UpstreamBranchCanonicalName}");
        // Stash the unstaged changes
        var stasher = new LibGit2Sharp.Signature("Updater", "updater@crpg.com", DateTimeOffset.Now);
        stash = repo.Stashes.Add(stasher, "Temp stash before update", StashModifiers.Default);
    }

    // Get the remote repository
    var remote = repo.Network.Remotes["origin"];

    // Get or create the local main branch
    var mainBranch = repo.Branches["main"];
    if (mainBranch == null)
    {
        mainBranch = repo.CreateBranch("main");
        Console.WriteLine($"Creating {mainBranch.UpstreamBranchCanonicalName} Branch");
    }

    // Checkout to main branch
    Console.WriteLine($"Checking out {mainBranch.UpstreamBranchCanonicalName} Branch");
    Commands.Checkout(repo, mainBranch);

    // Fetch all branches from remote
    Console.WriteLine($"Fetching all Branches)");
    foreach (var refSpec in remote.FetchRefSpecs)
    {
        Commands.Fetch(repo, remote.Name, new string[] { refSpec.Specification }, null, "");
    }

    // Merge main branch with its remote counterpart
    var remoteMainBranch = repo.Branches["origin/main"];
    Console.WriteLine($"Merging {remoteMainBranch.UpstreamBranchCanonicalName} into {mainBranch.UpstreamBranchCanonicalName}");
    repo.Merge(remoteMainBranch, new LibGit2Sharp.Signature("Updater", "updater@crpg.com", DateTimeOffset.Now));

    // Checkout back to original branch
    Console.WriteLine($"Checking out {currentBranch.UpstreamBranchCanonicalName} Branch");
    Commands.Checkout(repo, currentBranch);

    if (unstagedChanges)
    {
        Console.WriteLine($"Reapplying stashed changes for  {currentBranch.UpstreamBranchCanonicalName} Branch");
        // Create the options for applying the stash
        var stashApplyOptions = new StashApplyOptions { ApplyModifiers = StashApplyModifiers.Default };

        // Apply the stash to restore unstaged changes
        repo.Stashes.Apply(0, stashApplyOptions);
        repo.Stashes.Remove(0);
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

