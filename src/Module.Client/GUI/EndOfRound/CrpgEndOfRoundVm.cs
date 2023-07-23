using Crpg.Module.Common;
using Crpg.Module.Helpers;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.EndOfRound;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.GUI.EndOfRound;

public class CrpgEndOfRoundVm : ViewModel
{
    private readonly MissionScoreboardComponent _scoreboardComponent;
    private readonly IRoundComponent _multiplayerRoundComponent;
    private readonly string _victoryText;
    private readonly string _defeatText;
    private readonly TextObject _roundEndReasonAllyTeamSideDepletedTextObject;
    private readonly TextObject _roundEndReasonEnemyTeamSideDepletedTextObject;
    private readonly TextObject _roundEndReasonAllyTeamRoundTimeEndedTextObject;
    private readonly TextObject _roundEndReasonEnemyTeamRoundTimeEndedTextObject;
    private readonly TextObject _roundEndReasonAllyTeamGameModeSpecificEndedTextObject;
    private readonly TextObject _roundEndReasonEnemyTeamGameModeSpecificEndedTextObject;
    private readonly TextObject _roundEndReasonRoundTimeEndedWithDrawTextObject;
    private bool _isShown;
    private bool _hasAttackerMvp;
    private bool _hasDefenderMvp;
    private string _title = string.Empty;
    private string _description = string.Empty;
    private string _cultureId = string.Empty;
    private bool _isRoundWinner;
    private MultiplayerEndOfRoundSideVM _attackerSide = default!;
    private MultiplayerEndOfRoundSideVM _defenderSide = default!;
    private MPPlayerVM _attackerMvp = default!;
    private MPPlayerVM _defenderMvp = default!;
    private string _attackerMvpTitleText = string.Empty;
    private string _defenderMvpTitleText = string.Empty;

    public CrpgEndOfRoundVm(MissionScoreboardComponent scoreboardComponent,
        MissionLobbyComponent missionLobbyComponent, IRoundComponent multiplayerRoundComponent)
    {
        _scoreboardComponent = scoreboardComponent;
        _multiplayerRoundComponent = multiplayerRoundComponent;
        _victoryText = new TextObject("{=RCuCoVgd}ROUND WON").ToString();
        _defeatText = new TextObject("{=Dbkx4v90}ROUND LOST").ToString();
        _roundEndReasonAllyTeamSideDepletedTextObject = new TextObject("{=9M4G8DDd}Your team was wiped out");
        _roundEndReasonEnemyTeamSideDepletedTextObject =
            new TextObject("{=jPXglGWT}Enemy team was wiped out");
        _roundEndReasonAllyTeamRoundTimeEndedTextObject =
            new TextObject("{=x1HZy70i}Your team had the upper hand at timeout");
        _roundEndReasonEnemyTeamRoundTimeEndedTextObject =
            new TextObject("{=Dc3fFblo}Enemy team had the upper hand at timeout");
        _roundEndReasonRoundTimeEndedWithDrawTextObject =
            new TextObject("{=i3dJSlD0}No team had the upper hand at timeout");
        if (missionLobbyComponent.MissionType == MultiplayerGameType.Battle ||
            missionLobbyComponent.MissionType == MultiplayerGameType.Captain ||
            missionLobbyComponent.MissionType == MultiplayerGameType.Skirmish)
        {
            _roundEndReasonAllyTeamGameModeSpecificEndedTextObject =
                new TextObject("{=xxuzZJ3G}Your team ran out of morale");
            _roundEndReasonEnemyTeamGameModeSpecificEndedTextObject =
                new TextObject("{=c6c9eYrD}Enemy team ran out of morale");
        }
        else
        {
            _roundEndReasonAllyTeamGameModeSpecificEndedTextObject = TextObject.Empty;
            _roundEndReasonEnemyTeamGameModeSpecificEndedTextObject = TextObject.Empty;
        }

        AttackerSide = new MultiplayerEndOfRoundSideVM();
        DefenderSide = new MultiplayerEndOfRoundSideVM();
    }

    public override void RefreshValues()
    {
        base.RefreshValues();
        Refresh();
    }

    public void Refresh()
    {
        NetworkCommunicator myPeer = GameNetwork.MyPeer;
        MissionPeer missionPeer = myPeer.GetComponent<MissionPeer>();
        BattleSideEnum allyBattleSide = missionPeer.Team?.Side ?? BattleSideEnum.None;

        BattleSideEnum battleSideEnum = allyBattleSide == BattleSideEnum.Attacker
            ? BattleSideEnum.Defender
            : BattleSideEnum.Attacker;
        BasicCultureObject @object = MBObjectManager.Instance.GetObject<BasicCultureObject>(
            MultiplayerOptions.OptionType.CultureTeam1.GetStrValue());
        BasicCultureObject object2 = MBObjectManager.Instance.GetObject<BasicCultureObject>(
            MultiplayerOptions.OptionType.CultureTeam2.GetStrValue());
        MissionScoreboardComponent.MissionScoreboardSide missionScoreboardSide =
            _scoreboardComponent.Sides.First(s =>
                s != null && s.Side == BattleSideEnum.Attacker);
        MissionScoreboardComponent.MissionScoreboardSide missionScoreboardSide2 =
            _scoreboardComponent.Sides.First(s =>
                s != null && s.Side == BattleSideEnum.Defender);
        bool isWinner = _multiplayerRoundComponent.RoundWinner == BattleSideEnum.Attacker;
        bool isWinner2 = _multiplayerRoundComponent.RoundWinner == BattleSideEnum.Defender;
        Team? team = missionPeer.Team;
        if (team != null && team.Side == BattleSideEnum.Attacker)
        {
            AttackerMVPTitleText = GetMvpTitleText(@object);
            DefenderMVPTitleText = GetMvpTitleText(object2);
            AttackerSide.SetData(@object, missionScoreboardSide.SideScore, isWinner, false);
            DefenderSide.SetData(object2, missionScoreboardSide2.SideScore, isWinner2, @object == object2);
        }
        else
        {
            DefenderMVPTitleText = GetMvpTitleText(@object);
            AttackerMVPTitleText = GetMvpTitleText(object2);
            DefenderSide.SetData(@object, missionScoreboardSide.SideScore, isWinner, @object == object2);
            AttackerSide.SetData(object2, missionScoreboardSide2.SideScore, isWinner2, false);
        }

        if (_scoreboardComponent.Sides.FirstOrDefault(s => s != null && s.Side == allyBattleSide) != null)
        {
            bool flag = false;
            if (_multiplayerRoundComponent.RoundWinner == allyBattleSide)
            {
                IsRoundWinner = true;
                Title = _victoryText;
            }
            else if (_multiplayerRoundComponent.RoundWinner == battleSideEnum)
            {
                IsRoundWinner = false;
                Title = _defeatText;
            }
            else
            {
                flag = true;
            }

            RoundEndReason roundEndReason = _multiplayerRoundComponent.RoundEndReason;
            if (roundEndReason == RoundEndReason.SideDepleted)
            {
                Description = IsRoundWinner
                    ? _roundEndReasonEnemyTeamSideDepletedTextObject.ToString()
                    : _roundEndReasonAllyTeamSideDepletedTextObject.ToString();
                return;
            }

            if (roundEndReason == RoundEndReason.GameModeSpecificEnded)
            {
                Description = IsRoundWinner
                    ? _roundEndReasonEnemyTeamGameModeSpecificEndedTextObject.ToString()
                    : _roundEndReasonAllyTeamGameModeSpecificEndedTextObject.ToString();
                return;
            }

            if (roundEndReason == RoundEndReason.RoundTimeEnded)
            {
                Description = IsRoundWinner
                    ? _roundEndReasonAllyTeamRoundTimeEndedTextObject.ToString()
                    : flag
                        ? _roundEndReasonRoundTimeEndedWithDrawTextObject.ToString()
                        : _roundEndReasonEnemyTeamRoundTimeEndedTextObject.ToString();
            }
        }
    }

    public void OnMVPSelected(MissionPeer mvpPeer)
    {
        BasicCharacterObject @object = MBObjectManager.Instance.GetObject<BasicCharacterObject>("mp_character");
        @object.UpdatePlayerCharacterBodyProperties(mvpPeer.Peer.BodyProperties, mvpPeer.Peer.Race,
            mvpPeer.Peer.IsFemale);
        @object.Age = mvpPeer.Peer.BodyProperties.Age;

        var crpgUser = mvpPeer.Peer.GetComponent<CrpgPeer>()?.User;
        if (crpgUser != null)
        {
            var equipment = CrpgCharacterBuilder.CreateCharacterEquipment(crpgUser.Character.EquippedItems);
            MBEquipmentRoster equipmentRoster = new();
            ReflectionHelper.SetField(equipmentRoster, "_equipments", new MBList<Equipment> { equipment });
            ReflectionHelper.SetField(@object, "_equipmentRoster", equipmentRoster);
        }

        NetworkCommunicator myPeer = GameNetwork.MyPeer;
        MissionPeer missionPeer = myPeer.GetComponent<MissionPeer>();
        Team team = mvpPeer.Team;
        BattleSideEnum? battleSideEnum = team != null ? new BattleSideEnum?(team.Side) : null;
        Team team2 = missionPeer.Team;
        BattleSideEnum? battleSideEnum2 = team2 != null ? new BattleSideEnum?(team2.Side) : null;
        if (battleSideEnum.GetValueOrDefault() == battleSideEnum2.GetValueOrDefault() &
            battleSideEnum != null == (battleSideEnum2 != null))
        {
            AttackerMVP = new MPPlayerVM(mvpPeer);
            AttackerMVP.RefreshDivision();
            AttackerMVP.RefreshPreview(@object, mvpPeer.Peer.BodyProperties.DynamicProperties,
                mvpPeer.Peer.IsFemale);
            HasAttackerMVP = true;
            return;
        }

        DefenderMVP = new MPPlayerVM(mvpPeer);
        DefenderMVP.RefreshDivision();
        DefenderMVP.RefreshPreview(@object, mvpPeer.Peer.BodyProperties.DynamicProperties, mvpPeer.Peer.IsFemale);
        HasDefenderMVP = true;
    }

    private string GetMvpTitleText(BasicCultureObject culture)
    {
        if (culture.StringId == "vlandia")
        {
            return new TextObject("{=3VosbFR0}Vlandian Champion").ToString();
        }

        if (culture.StringId == "sturgia")
        {
            return new TextObject("{=AGUXiN8u}Voivode").ToString();
        }

        if (culture.StringId == "khuzait")
        {
            return new TextObject("{=F2h2cT4q}Khan's Chosen").ToString();
        }

        if (culture.StringId == "battania")
        {
            return new TextObject("{=eWPN3HmE}Hero of Battania").ToString();
        }

        if (culture.StringId == "aserai")
        {
            return new TextObject("{=5zNfxZ7B}War Prince").ToString();
        }

        if (culture.StringId == "empire")
        {
            return new TextObject("{=wwbIcqsq}Conqueror").ToString();
        }

        Debug.FailedAssert("Invalid Culture ID for MVP Title");
        return string.Empty;
    }

    private void OnIsShownChanged()
    {
        if (!IsShown)
        {
            HasAttackerMVP = false;
            HasDefenderMVP = false;
        }
    }

    [DataSourceProperty]
    public bool IsShown
    {
        get
        {
            return _isShown;
        }
        set
        {
            if (value != _isShown)
            {
                _isShown = value;
                OnPropertyChangedWithValue(value);
                OnIsShownChanged();
            }
        }
    }

    [DataSourceProperty]
    // ReSharper disable once InconsistentNaming
    public bool HasAttackerMVP
    {
        get
        {
            return _hasAttackerMvp;
        }
        set
        {
            if (value != _hasAttackerMvp)
            {
                _hasAttackerMvp = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    // ReSharper disable once InconsistentNaming
    public bool HasDefenderMVP
    {
        get
        {
            return _hasDefenderMvp;
        }
        set
        {
            if (value != _hasDefenderMvp)
            {
                _hasDefenderMvp = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string Title
    {
        get
        {
            return _title;
        }
        set
        {
            if (value != _title)
            {
                _title = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string Description
    {
        get
        {
            return _description;
        }
        set
        {
            if (value != _description)
            {
                _description = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public string CultureId
    {
        get
        {
            return _cultureId;
        }
        set
        {
            if (value != _cultureId)
            {
                _cultureId = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public bool IsRoundWinner
    {
        get
        {
            return _isRoundWinner;
        }
        set
        {
            if (value != _isRoundWinner)
            {
                _isRoundWinner = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public MultiplayerEndOfRoundSideVM AttackerSide
    {
        get
        {
            return _attackerSide;
        }
        set
        {
            if (value != _attackerSide)
            {
                _attackerSide = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public MultiplayerEndOfRoundSideVM DefenderSide
    {
        get
        {
            return _defenderSide;
        }
        set
        {
            if (value != _defenderSide)
            {
                _defenderSide = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    // ReSharper disable once InconsistentNaming
    public MPPlayerVM AttackerMVP
    {
        get
        {
            return _attackerMvp;
        }
        set
        {
            if (value != _attackerMvp)
            {
                _attackerMvp = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    // ReSharper disable once InconsistentNaming
    public MPPlayerVM DefenderMVP
    {
        get
        {
            return _defenderMvp;
        }
        set
        {
            if (value != _defenderMvp)
            {
                _defenderMvp = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    // ReSharper disable once InconsistentNaming
    public string AttackerMVPTitleText
    {
        get
        {
            return _attackerMvpTitleText;
        }
        set
        {
            if (value != _attackerMvpTitleText)
            {
                _attackerMvpTitleText = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    // ReSharper disable once InconsistentNaming
    public string DefenderMVPTitleText
    {
        get
        {
            return _defenderMvpTitleText;
        }
        set
        {
            if (value != _defenderMvpTitleText)
            {
                _defenderMvpTitleText = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }
}
