namespace Crpg.Module.DataExport;

public interface IDataExporter
{
    Task Scale(string gitRepoPath);
    Task RefundArmor(string gitRepoPath);
    Task RefundCrossbow(string gitRepoPath);
    Task RefundBow(string gitRepoPath);
    Task RefundThrowing(string gitRepoPath);
    Task RefundCav(string gitRepoPath);
    Task RefundShield(string gitRepoPath);
    Task ComputeAutoStats(string gitRepoPath);
    Task Export(string gitRepoPath);
    Task ImageExport(string gitRepoPath);
}
