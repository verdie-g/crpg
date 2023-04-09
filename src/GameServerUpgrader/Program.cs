using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net;

namespace RemoteGameServerAutomation
{
    class Program
    {
        private static void ExecuteRemotePowerShellCommand(string server, NetworkCredential credentials, string command)
        {
            var psCredentials = new PSCredential(credentials.UserName, credentials.SecurePassword);

            using (var runspace = RunspaceFactory.CreateRunspace(new WSManConnectionInfo(new Uri($"http://{server}:5985/wsman"), "http://schemas.microsoft.com/powershell/Microsoft.PowerShell", psCredentials)))
            {
                runspace.Open();

                using (PowerShell ps = PowerShell.Create())
                {
                    ps.Runspace = runspace;
                    ps.AddScript(command);

                    try
                    {
                        var results = ps.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error executing remote command: {ex.Message}");
                    }
                }

                runspace.Close();
            }
        }

        private static void UploadZipFile(string server, NetworkCredential credentials, string localZipPath, string remoteZipPath)
        {
            byte[] zipFileBytes = File.ReadAllBytes(localZipPath);
            string base64Zip = Convert.ToBase64String(zipFileBytes);

            var command = $@"
                $base64Zip = '{base64Zip}'
                $zipFileBytes = [System.Convert]::FromBase64String($base64Zip)
                [System.IO.File]::WriteAllBytes('{remoteZipPath}', $zipFileBytes)
            ";

            ExecuteRemotePowerShellCommand(server, credentials, command);
        }

        private static void ExtractRemoteZipFile(string server, NetworkCredential credentials, string remoteZipPath, string extractionFolder)
        {
            var command = $@"
                Add-Type -AssemblyName System.IO.Compression.FileSystem
                [System.IO.Compression.ZipFile]::ExtractToDirectory('{remoteZipPath}', '{extractionFolder}')
            ";

            ExecuteRemotePowerShellCommand(server, credentials, command);
        }

        private static string CreateZipFileFromFolder(string sourceFolderPath)
        {
            string zipFilePath = Path.Combine(Path.GetTempPath(), "crpg.zip");
            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }
            ZipFile.CreateFromDirectory(sourceFolderPath, zipFilePath);

            return zipFilePath;
        }

        public static void Main(string[] args)
        {
            var credentials = CredentialCache.DefaultNetworkCredentials;

            // Replace these with your actual server addresses
            var servers = new List<string> { "server1_address", "server2_address", "server3_address" };
            // Create the zip file
            string sourceFolderPath = Path.Combine(Environment.GetEnvironmentVariable("MB_CLIENT_PATH")!, "Modules", "crpg");
            string localZipPath = CreateZipFileFromFolder(sourceFolderPath);

            foreach (var server in servers)
            {
                // Close processes
                ExecuteRemotePowerShellCommand(server, credentials, "Get-Process -Name powershell, bannerlord | Stop-Process -Force");

                // Upload zip file
                string remoteZipPath = "path_to_remote_zip_file";
                UploadZipFile(server, credentials, localZipPath, remoteZipPath);

                // Update game folder
                string extractionFolder = "path_to_extraction_folder";
                ExtractRemoteZipFile(server, credentials, remoteZipPath, extractionFolder);

                // Start PowerShell scripts
                string[] scriptPaths = new[]
                {
                    "path_to_script_1.ps1",
                    "path_to_script_2.ps1",
                    "path_to_script_3.ps1",
                    "path_to_script_4.ps1",
                    "path_to_script_5.ps1"
                };

                foreach (var scriptPath in scriptPaths)
                {
                    string checkScriptExistsCommand = $"if (Test-Path -Path '{scriptPath}') {{ & '{scriptPath}' }}";
                    ExecuteRemotePowerShellCommand(server, credentials, checkScriptExistsCommand);
                }
            }
        }
    }
}
