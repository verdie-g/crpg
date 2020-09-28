using System;
using TaleWorlds.MountAndBlade;

namespace Crpg.GameMod.DefendTheVirgin
{
    internal class WaveController : MissionLogic
    {
        private static readonly TimeSpan WaveEndDuration = TimeSpan.FromSeconds(4);

        private readonly int _maxWave;

        private int _waveCount = 0;
        private WaveState _waveState = WaveState.Ended;
        private MissionTimer? _waveEndTimer;

        public event Action<int> OnWaveStarted = _ => { };
        public event Action<int> OnWaveEnding = _ => { };
        public event Action<int> OnWaveEnded = _ => { };

        public WaveController(int maxWave)
        {
            _maxWave = maxWave;
        }

        public override void OnPreDisplayMissionTick(float dt)
        {
            if (_waveState == WaveState.Ended)
            {
                BeginNewWave();
            }
            else if (_waveState == WaveState.InProgress)
            {
                if (!CheckForWaveEnd())
                {
                    return;
                }

                EndWave();
            }
            else if (_waveState == WaveState.Ending)
            {
                if (!_waveEndTimer!.Check())
                {
                    return;
                }

                PostWaveEnd();
            }
        }

        private bool CheckForWaveEnd()
        {
            return Mission.AttackerTeam.ActiveAgents.Count == 0 || Mission.DefenderTeam.ActiveAgents.Count < 2;
        }

        private void BeginNewWave()
        {
            Mission.Current.ResetMission();
            _waveState = WaveState.InProgress;
            ++_waveCount;
            OnWaveStarted(_waveCount);
        }

        private void EndWave()
        {
            _waveState = WaveState.Ending;
            _waveEndTimer = new MissionTimer((int)WaveEndDuration.TotalSeconds);
            OnWaveEnding(_waveCount);
        }

        private void PostWaveEnd()
        {
            if (Mission.DefenderTeam.ActiveAgents.Count < 2 || _waveCount == _maxWave)
            {
                Mission.Current.EndMission();
            }

            _waveState = WaveState.Ended;
            OnWaveEnded(_waveCount);
        }

        private enum WaveState
        {
            InProgress,
            Ending,
            Ended,
        }
    }
}
