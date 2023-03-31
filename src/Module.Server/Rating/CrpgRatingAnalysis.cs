using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Crpg.Module.Api.Models.Characters;
using TaleWorlds.Core;

namespace Crpg.Module.UTest.Rating;
internal class CrpgRatingAnalysis
{

        private readonly List<RoundResultData>? _results;
        public CrpgRatingAnalysis(string logLocation)
        {

            Regex roundDataRegex = new("\\[.+\\] Round result data: ([\\w=]+)", RegexOptions.Compiled);
            using StreamReader logFile = new(@logLocation);
            List<RoundResultData> results = new List<RoundResultData>();
            while (logFile.ReadLine() is { } line)
            {
                var m = roundDataRegex.Match(line);
                if (!m.Success)
                {
                    continue;
                }

                string base64RoundResultJson = m.Groups[1].Value;
                string roundResultJson = Encoding.UTF8.GetString(Convert.FromBase64String(base64RoundResultJson));
                var roundResult = JsonSerializer.Deserialize<RoundResultData>(roundResultJson)!;
                results.Add(roundResult);
            }

            _results = results;
        }

        public float AccuratePredictionPercentage(Func<RoundResultData, BattleSideEnum> predictionMethod)
        {
            return _results.Sum(r => predictionMethod(r) == r.WinnerSide ? 1 : 0) / (float)_results.Count();
        }

        public class RoundResultData
        {
            public BattleSideEnum WinnerSide { get; set; }
            public string MapId { get; set; } = string.Empty;
            public Version Version { get; set; } = default!;
            public DateTime Date { get; set; }
            public List<RoundPlayerData> Defenders { get; set; } = new();
            public List<RoundPlayerData> Attackers { get; set; } = new();
        }

        public class RoundPlayerData
        {
            public int UserId { get; set; }
            public string UserName { get; set; } = string.Empty;
            public float Weight { get; set; }
            public int Level { get; set; }
            public float LevelWeight { get; set; }
            public CrpgCharacterClass Class { get; set; }
            public int Score { get; set; }
            public int Kills { get; set; }
            public int Deaths { get; set; }
            public int Assists { get; set; }
            public float Rating { get; set; }
            public float RatingWeight { get; set; }
            public int EquipmentCost { get; set; }
            public float EquipmentWeight { get; set; }
            public string? ClanTag { get; set; }
        }
    }

