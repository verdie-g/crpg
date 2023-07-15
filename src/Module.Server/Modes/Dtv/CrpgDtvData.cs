using System.Xml.Serialization;

namespace Crpg.Module.Modes.Dtv;

// XmlSerializer expects the classes to be public for legacy reasons.
[XmlRoot(ElementName = "DtvData")]
public class CrpgDtvData
{
    [XmlElement(ElementName = "Round")]
    public List<CrpgDtvRound> Rounds { get; set; } = new();
}

public class CrpgDtvRound
{
    [XmlAttribute(AttributeName = "reward")]
    public int Reward { get; set; }

    [XmlElement(ElementName = "Wave")]
    public List<CrpgDtvWave> Waves { get; set; } = new();
}

public class CrpgDtvWave
{
    [XmlElement(ElementName = "Group")]
    public List<CrpgDtvGroup> Groups { get; set; } = new();
}

public class CrpgDtvGroup
{
    [XmlAttribute(AttributeName = "classDivisionId")]
    public string ClassDivisionId { get; set; } = string.Empty;

    [XmlAttribute(AttributeName = "count")]
    public int Count { get; set; }
}
