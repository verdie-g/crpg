using System.Threading.Tasks;

namespace Crpg.GameMod.DataExport
{
    public interface IDataExporter
    {
        Task Export(string outputPath);
    }
}
