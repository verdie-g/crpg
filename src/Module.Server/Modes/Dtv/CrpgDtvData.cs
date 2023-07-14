using System.Xml.Serialization;

namespace Crpg.Module.Modes.Dtv;

// XmlSerializer expects the classes to be public for legacy reasons.
[XmlRoot(ElementName = "DtvData")]
public class CrpgDtvData
{
    [XmlElement(ElementName = "Round")]
    public List<CrpgDtvRound> Rounds { get; set; } = new();

    public CrpgDtvRound? this[int id] => Rounds.FirstOrDefault(s => s.Id == id);
}

public class CrpgDtvRound
{
    [XmlAttribute(AttributeName = "id")]
    public int Id { get; set; }

    [XmlElement(ElementName = "Wave")]
    public List<CrpgDtvWave> Waves { get; set; } = new();

    public CrpgDtvWave? this[int id] => Waves.FirstOrDefault(s => s.Id == id);
}

public class CrpgDtvWave
{
    [XmlAttribute(AttributeName = "id")]
    public int Id { get; set; }

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
