using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Strategus;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Crpg.Application.System.Commands
{
    public record SeedStrategusDataCommand : IMediatorRequest
    {
        internal class Handler : IMediatorRequestHandler<SeedStrategusDataCommand>
        {
            private readonly ICrpgDbContext _db;
            private readonly IStrategusSettlementsSource _settlementsSource;
            private readonly IStrategusMap _strategusMap;
            private readonly IApplicationEnvironment _appEnv;

            public Handler(ICrpgDbContext db, IStrategusSettlementsSource settlementsSource, IStrategusMap strategusMap,
                IApplicationEnvironment appEnv)
            {
                _db = db;
                _settlementsSource = settlementsSource;
                _strategusMap = strategusMap;
                _appEnv = appEnv;
            }

            public async Task<Result> Handle(SeedStrategusDataCommand request, CancellationToken cancellationToken)
            {
                await SeedSettlements(cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);

                if (_appEnv.Environment == HostingEnvironment.Development)
                {
                    await SeedDevelopmentData(cancellationToken);
                    await _db.SaveChangesAsync(cancellationToken);
                }

                return Result.NoErrors;
            }

            private async Task SeedSettlements(CancellationToken cancellationToken)
            {
                var settlementsByName = (await _settlementsSource.LoadStrategusSettlements())
                    .ToDictionary(i => i.Name);
                var dbSettlementsByNameRegion = await _db.StrategusSettlements
                    .ToDictionaryAsync(di => (di.Name, di.Region), cancellationToken);

                foreach (var settlementCreation in settlementsByName.Values)
                {
                    foreach (var region in GetRegions())
                    {
                        var settlement = new StrategusSettlement
                        {
                            Name = settlementCreation.Name,
                            Type = settlementCreation.Type,
                            Culture = settlementCreation.Culture,
                            Region = region,
                            Position = _strategusMap.TranslatePositionForRegion(settlementCreation.Position, Region.Europe, region),
                            Scene = settlementCreation.Scene,
                        };

                        CreateOrUpdateSettlement(dbSettlementsByNameRegion, settlement);
                    }
                }

                foreach (var dbSettlement in dbSettlementsByNameRegion.Values)
                {
                    if (!settlementsByName.ContainsKey(dbSettlement.Name))
                    {
                        _db.StrategusSettlements.Remove(dbSettlement);
                    }
                }
            }

            private void CreateOrUpdateSettlement(Dictionary<(string name, Region region),
                    StrategusSettlement> dbSettlementsByName, StrategusSettlement settlement)
            {
                if (dbSettlementsByName.TryGetValue((settlement.Name, settlement.Region), out StrategusSettlement? dbSettlement))
                {
                    _db.Entry(dbSettlement).State = EntityState.Detached;

                    settlement.Id = dbSettlement.Id;
                    _db.StrategusSettlements.Update(settlement);
                }
                else
                {
                    _db.StrategusSettlements.Add(settlement);
                }
            }

            private async Task SeedDevelopmentData(CancellationToken cancellationToken)
            {
                var epicrotea = await GetSettlementByName("Epicrotea");
                var mecalovea = await GetSettlementByName("Mecalovea");
                var marathea = await GetSettlementByName("Marathea");
                var stathymos = await GetSettlementByName("Stathymos");
                var gersegosCastle = await GetSettlementByName("Gersegos Castle");
                var dyopalis = await GetSettlementByName("Dyopalis");
                var rhesosCastle = await GetSettlementByName("Rhesos Castle");
                var potamis = await GetSettlementByName("Potamis");
                var carphenion = await GetSettlementByName("Carphenion");
                var ataconiaCastle = await GetSettlementByName("Ataconia Castle");
                var ataconia = await GetSettlementByName("Ataconia");
                var elipa = await GetSettlementByName("Elipa");
                var rhotae = await GetSettlementByName("Rhotae");
                var hertogeaCastle = await GetSettlementByName("Hertogea Castle");
                var hertogea = await GetSettlementByName("Hertogea");
                var nideon = await GetSettlementByName("Nideon");
                var rhemtoil = await GetSettlementByName("Rhemtoil");

                var brainfart = NewHero("1", "Brainfart", 1, new Point(112, -88), StrategusHeroStatus.Idle);
                var kiwi = NewHero("2", "Kiwi", 87, new Point(123, -90), StrategusHeroStatus.Idle);
                var ikarooz = NewHero("3", "Ikarooz", 20, new Point(142, -90), StrategusHeroStatus.Idle);
                var godwere = NewHero("4", "Godwere", 14, new Point(135, -97), StrategusHeroStatus.Idle);
                var bryggan = NewHero("5", "Bryggan", 1, new Point(130, -102), StrategusHeroStatus.Idle);
                var dainMorgot = NewHero("6", "dainMorgot", 6, new Point(108, -98), StrategusHeroStatus.Idle);
                var schumetzq = NewHero("7", "Schumetzq", 230, new Point(119, -105), StrategusHeroStatus.Idle);
                var laenir = NewHero("8", "Laenir", 121, new Point(106, -112), StrategusHeroStatus.Idle);
                var traxits = NewHero("9", "Traxits", 98, new Point(114, -114), StrategusHeroStatus.Idle);
                var lamk10 = NewHero("10", "lamk10", 55, new Point(117, -112), StrategusHeroStatus.Idle);
                var veith = NewHero("11", "Veith", 29, new Point(105, -111), StrategusHeroStatus.Idle);
                var emirso = NewHero("12", "Emirso", 1, new Point(103, -102), StrategusHeroStatus.Idle);
                var kurosaki = NewHero("13", "kurosaki", 1, new Point(113, -112), StrategusHeroStatus.Idle);
                var ajroselle = NewHero(" 14", "ajroselle", 4, epicrotea.Position, StrategusHeroStatus.IdleInSettlement, settlementTarget: epicrotea);
                var bedo = NewHero("15", "bedo", 9, epicrotea.Position, StrategusHeroStatus.RecruitingInSettlement, settlementTarget: epicrotea);
                var sellka = NewHero("16", "Sellka", 3, dyopalis.Position, StrategusHeroStatus.RecruitingInSettlement, settlementTarget: dyopalis);
                var lambic = NewHero("17", "Lambic", 1, rhotae.Position, StrategusHeroStatus.RecruitingInSettlement, settlementTarget: rhotae);
                var aset = NewHero("18", "Aset", 120, rhotae.Position, StrategusHeroStatus.RecruitingInSettlement, settlementTarget: rhotae);
                var skrael = NewHero("19", "Skrael", 243, rhotae.Position, StrategusHeroStatus.IdleInSettlement, settlementTarget: rhotae);
                var aroyfalconer = NewHero("20", "aroyfalconer", 49, nideon.Position, StrategusHeroStatus.IdleInSettlement, settlementTarget: nideon);
                var caNp0G = NewHero("21", "CaNp0G", 10, new Point(107, -102), StrategusHeroStatus.MovingToSettlement, settlementTarget: rhotae);
                var mundete = NewHero("22", "Mundete", 20, new Point(112, -93), StrategusHeroStatus.MovingToSettlement, settlementTarget: rhotae);
                var noobamphetamine = NewHero("23", "Noobamphetamine", 1, new Point(122, -123), StrategusHeroStatus.MovingToSettlement, settlementTarget: rhotae);
                var insanitoid = NewHero("24", "Insanitoid", 3, new Point(124, -102), StrategusHeroStatus.MovingToSettlement, settlementTarget: rhotae);
                var scarface = NewHero("25", "Scarface", 2, new Point(108, -115), StrategusHeroStatus.MovingToSettlement, settlementTarget: rhemtoil);
                var disorot = NewHero("26", "Disorot", 9, new Point(120, -88), StrategusHeroStatus.MovingToSettlement, settlementTarget: mecalovea);
                var ace = NewHero("27", "Ace", 25, new Point(119, -105), StrategusHeroStatus.MovingToSettlement, settlementTarget: hertogeaCastle);
                var neostralie = NewHero("28", "Neostralie", 1, new Point(128, -97), StrategusHeroStatus.MovingToSettlement, settlementTarget: potamis);
                var jayJrod = NewHero("29", "JayJrod", 1, new Point(129, -102), StrategusHeroStatus.MovingToAttackHero, heroTarget: neostralie);
                var sagar = NewHero("30", "Sagar", 1, new Point(130, -107), StrategusHeroStatus.MovingToAttackHero, heroTarget: jayJrod);
                var hannibaru = NewHero("31", "Hannibaru", 1, new Point(126, -93), StrategusHeroStatus.MovingToAttackHero, heroTarget: neostralie);
                var drexx = NewHero("32", "Drexx", 300, new Point(114, -101), StrategusHeroStatus.MovingToAttackSettlement, settlementTarget: gersegosCastle);
                var localAlpha = NewHero("33", "LocalAlpha", 87, new Point(113, -98), StrategusHeroStatus.MovingToAttackSettlement, settlementTarget: gersegosCastle);
                var luQeRo = NewHero("34", "LuQeRo", 21, new Point(119, -101), StrategusHeroStatus.MovingToAttackSettlement, settlementTarget: rhotae);
                var alex = NewHero("35", "Alex", 1, rhesosCastle.Position, StrategusHeroStatus.MovingToPoint, new Point(125, -97));
                var kedrynFuel = NewHero("36", "KedrynFuel", 1, new Point(105, -107), StrategusHeroStatus.MovingToPoint, new Point(121, -99));
                var ilya2106 = NewHero("37", "ilya2106", 1, new Point(107, -100), StrategusHeroStatus.MovingToPoint, new Point(112, -88));
                var bchv = NewHero("38", "bchv", 1, new Point(112, -88), StrategusHeroStatus.MovingToPoint, new Point(107, -100));
                var kypak = NewHero("39", "Kypak", 1, new Point(112, -99), StrategusHeroStatus.FollowingHero, heroTarget: ilya2106);
                var telesto = NewHero("40", "Telesto", 1, new Point(123, -88), StrategusHeroStatus.MovingToPoint, new Point(135, -98));
                var devoidDragon = NewHero("41", "DevoidDragon", 1, new Point(135, -98), StrategusHeroStatus.MovingToPoint, new Point(123, -88));
                var namidaka = NewHero("42", "Namidaka", 11, new Point(135, -99), StrategusHeroStatus.FollowingHero, heroTarget: devoidDragon);

                var heroes = new[]
                {
                    brainfart, kiwi, ikarooz, godwere, bryggan, dainMorgot, schumetzq, laenir, traxits, lamk10, veith,
                    emirso, kurosaki, ajroselle, bedo, sellka, lambic, aset, skrael, aroyfalconer, caNp0G, mundete,
                    noobamphetamine, insanitoid, scarface, disorot, ace, neostralie, jayJrod, sagar, hannibaru, drexx,
                    localAlpha, luQeRo, alex, kedrynFuel, ilya2106, bchv, kypak, telesto, devoidDragon, namidaka,
                };

                var existingUsers = (await _db.Users.ToArrayAsync(cancellationToken))
                    .Select(u => (u.Platform, u.PlatformUserId))
                    .ToHashSet();
                foreach (var hero in heroes)
                {
                    if (!existingUsers.Contains((hero.User!.Platform, hero.User.PlatformUserId)))
                    {
                        _db.StrategusHeroes.Add(hero);
                    }
                }
            }

            private Task<StrategusSettlement> GetSettlementByName(string name)
            {
                return _db.StrategusSettlements.FirstAsync(s => s.Name == name);
            }

            private StrategusHero NewHero(string steamId, string name, int troops, Point position,
                StrategusHeroStatus status, Point? wayPoint = null, StrategusHero? heroTarget = null,
                StrategusSettlement? settlementTarget = null) =>
                new()
                {
                    Region = Region.Europe,
                    User = new User { Platform = Platform.Steam, PlatformUserId = steamId, Name = name },
                    Troops = troops,
                    Position = position,
                    Status = status,
                    Waypoints = new MultiPoint(wayPoint != null ? new[] { wayPoint } : Array.Empty<Point>()),
                    TargetedHero = heroTarget,
                    TargetedSettlement = settlementTarget,
                };

            private IEnumerable<Region> GetRegions() => Enum.GetValues(typeof(Region)).Cast<Region>();
        }
    }
}
