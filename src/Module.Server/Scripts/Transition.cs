using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Scripts;

/// <summary>A script that moves an entity.</summary>
internal class Transition : ScriptComponentBehavior
{
    private static readonly Dictionary<TimingFunctionType, CubicBezier> TimingFunctions = new()
    {
        [TimingFunctionType.Linear] = CubicBezier.CreateEase(0, 0, 1.0, 1.0),
        [TimingFunctionType.Ease] = CubicBezier.CreateEase(0.25, 0.1, 0.25, 1.0),
        [TimingFunctionType.EaseIn] = CubicBezier.CreateEase(0.42, 0.0, 1.0, 1.0),
        [TimingFunctionType.EaseOut] = CubicBezier.CreateEase(0.0, 0.0, 0.58, 1.0),
        [TimingFunctionType.EaseInOut] = CubicBezier.CreateEase(0.42, 0.0, 0.58, 1.0),
    };

    private SynchedMissionObject? _synchedObject;
    private float _duration;
    private Vec3 _initialPosition;
    private MissionTimer? _repeatDelayTimer;

#pragma warning disable SA1401 // Bannerlord editor expects fields
#pragma warning disable SA1202
    public float Duration = 5f;
    public Vec3 Translation = Vec3.Zero;
    public float RepeatDelay = 8f;

    /// <summary>As defined in https://developer.mozilla.org/en-US/docs/Web/CSS/easing-function.</summary>
    public TimingFunctionType TimingFunction = TimingFunctionType.Linear;

#pragma warning restore SA1202
#pragma warning restore SA1401

    public override TickRequirement GetTickRequirement()
    {
        return base.GetTickRequirement() | TickRequirement.TickParallel;
    }

    protected override void OnInit()
    {
        base.OnInit();
        Debug.Print($"Initialize {nameof(Transition)} script on entity {GameEntity.GetGuid()}");
        SetScriptComponentToTick(GetTickRequirement());

        _synchedObject = GameEntity.GetFirstScriptOfType<SynchedMissionObject>();
        if (_synchedObject == null)
        {
            Debug.Print($"Entity {GameEntity.GetGuid()} has a {nameof(Transition)} script but no {nameof(SynchedMissionObject)} one");
            return;
        }

        _initialPosition = GameEntity.GetFrame().origin;
    }

    protected override void OnTickParallel(float dt)
    {
        if (!GameNetwork.IsServer || _synchedObject == null)
        {
            return;
        }

        if (_duration >= Duration)
        {
            if (RepeatDelay <= 0)
            {
                return;
            }

            _repeatDelayTimer ??= new MissionTimer(RepeatDelay);
            if (_repeatDelayTimer.Check())
            {
                _duration = 0f;
                _initialPosition += Translation;
                _repeatDelayTimer = null;
                Translation = -Translation;
            }

            return;
        }

        var frame = GameEntity.GetFrame();
        float durationProgress = _duration / Duration;
        float transitionProgress = (float)TimingFunctions[TimingFunction].Sample(durationProgress);
        frame.origin = _initialPosition + Translation * transitionProgress;
        _synchedObject.SetFrameSynched(ref frame);
        _duration += dt;
    }

    public enum TimingFunctionType
    {
        Linear,
        Ease,
        EaseIn,
        EaseOut,
        EaseInOut,
    }
}
