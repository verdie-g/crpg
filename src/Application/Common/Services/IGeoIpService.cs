using System.Net;
using Crpg.Domain.Entities;
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Responses;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Common.Services;

public interface IGeoIpService
{
    Region? ResolveRegionFromIp(IPAddress ipAddress);
}

internal class MaxMindGeoIpService : IGeoIpService
{
    private static readonly ILogger Logger = LoggerFactory.CreateLogger<MaxMindGeoIpService>();

    // https://en.wikipedia.org/wiki/ISO_3166-1
    private static readonly Dictionary<string, Region> CountryRegions = new()
    {
        ["AE"] = Region.Eu,
        ["BH"] = Region.Eu,
        ["CY"] = Region.Eu,
        ["EG"] = Region.Eu,
        ["GE"] = Region.Eu,
        ["IL"] = Region.Eu,
        ["IQ"] = Region.Eu,
        ["IR"] = Region.Eu,
        ["JO"] = Region.Eu,
        ["KW"] = Region.Eu,
        ["LB"] = Region.Eu,
        ["OM"] = Region.Eu,
        ["PS"] = Region.Eu,
        ["QA"] = Region.Eu,
        ["SA"] = Region.Eu,
        ["SY"] = Region.Eu,
        ["TR"] = Region.Eu,
        ["YE"] = Region.Eu,
    };

    private static readonly Dictionary<string, Region> ContinentRegions = new()
    {
        ["AF"] = Region.Eu,
        ["AN"] = Region.Oc,
        ["AS"] = Region.As,
        ["EU"] = Region.Eu,
        ["NA"] = Region.Na,
        ["OC"] = Region.Oc,
        ["SA"] = Region.Na,
    };

    private readonly IGeoIP2DatabaseReader _geoIpDatabase;

    public MaxMindGeoIpService(IGeoIP2DatabaseReader geoIpDatabase)
    {
        _geoIpDatabase = geoIpDatabase;
    }

    public Region? ResolveRegionFromIp(IPAddress ipAddress)
    {
        CountryResponse countryResponse;
        try
        {
            countryResponse = _geoIpDatabase.Country(ipAddress);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error while resolving user region");
            return null;
        }

        string? countryCode = countryResponse.Country.IsoCode;
        if (countryCode != null && CountryRegions.TryGetValue(countryCode, out Region region))
        {
            return region;
        }

        string? continentCode = countryResponse.Continent.Code;
        if (continentCode != null && ContinentRegions.TryGetValue(continentCode, out region))
        {
            return region;
        }

        return Region.Eu;
    }
}

internal class StubGeoIpService : IGeoIpService
{
    public Region? ResolveRegionFromIp(IPAddress ipAddress) => Region.Eu;
}
