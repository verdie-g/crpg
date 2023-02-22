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

        return countryResponse.Continent.Code switch
        {
            "AF" => Region.Eu,
            "AN" => Region.Oc,
            "AS" => Region.As,
            "EU" => Region.Eu,
            "NA" => Region.Na,
            "OC" => Region.Oc,
            "SA" => Region.Na,
            _ => Region.Eu,
        };
    }
}

internal class StubGeoIpService : IGeoIpService
{
    public Region? ResolveRegionFromIp(IPAddress ipAddress) => Region.Eu;
}
