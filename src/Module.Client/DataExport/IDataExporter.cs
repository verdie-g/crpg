namespace Crpg.Module.DataExport;

public interface IDataExporter
{
    Task Export(string gitRepoPath);
    Task ImageExport(string gitRepoPath);
}
