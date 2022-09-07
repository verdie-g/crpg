namespace Crpg.Module.DataExport;

public interface IDataExporter
{
    Task Export(string gitRepoPath);
}
