using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Crpg.Module.Modes.Dtv;

[XmlRoot(ElementName = "DtvData")]
public class CrpgDtvData
{
    public CrpgDtvData()
    {
        Rounds = new List<CrpgDtvRound>();
    }

    [XmlElement(ElementName = "Round")]
    public List<CrpgDtvRound>? Rounds { get; set; }

    public CrpgDtvRound this[int id]
    {
        get { return Rounds.FirstOrDefault(s => s.Id == id); }
    }
}

public class CrpgDtvRound
{
    public CrpgDtvRound()
    {
        Waves = new List<CrpgDtvWave>();
    }

    [XmlAttribute(AttributeName = "id")]
    public int Id { get; set; }

    [XmlElement(ElementName = "Wave")]
    public List<CrpgDtvWave>? Waves { get; set; }

    public CrpgDtvWave this[int id]
    {
        get { return Waves.FirstOrDefault(s => s.Id == id); }
    }
}

public class CrpgDtvWave
{
    public CrpgDtvWave()
    {
        Groups = new List<CrpgDtvGroup>();
    }

    [XmlAttribute(AttributeName = "id")]
    public int Id { get; set; }

    [XmlElement(ElementName = "Group")]
    public List<CrpgDtvGroup>? Groups { get; set; }
}

public class CrpgDtvGroup
{
    [XmlAttribute(AttributeName = "classDivisionId")]
    public string? ClassDivisionId { get; set; }

    [XmlAttribute(AttributeName = "count")]
    public int Count { get; set; }
}
