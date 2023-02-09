namespace Crpg.Module.DataExport;

public interface IDataExporter
{
    Task ComputeWeight(string gitRepoPath);
    Task Export(string gitRepoPath);
    Task ImageExport(string gitRepoPath);
}
