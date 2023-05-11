using System;
using System.Collections.Generic;
using System.Text;

namespace Crpg.Module.Api.Models;
internal class CrpgUserUpkeepCompensation
{
    public int RepairCostToCompensate { get; set; }
    public int CompensatedRepairCost { get; set; }
    public int TotalRepairCost { get; set; }

    public int GetNetUpkeepCompensation()
    {
        return CompensatedRepairCost - RepairCostToCompensate;
    }
}
