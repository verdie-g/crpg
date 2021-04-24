using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Strategus.Models;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;

namespace Crpg.Application.Common.Files
{
    internal class FileStrategusSettlementsSource : IStrategusSettlementsSource
    {
        private static readonly string StrategusSettlementsPath =
            Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) + "/Common/Files/settlements.json";

        public async Task<IEnumerable<StrategusSettlementCreation>> LoadStrategusSettlements()
        {
            await using var file = File.OpenRead(StrategusSettlementsPath);
            return (await JsonSerializer.DeserializeAsync<IEnumerable<StrategusSettlementCreation>>(file, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new GeoJsonConverterFactory(GeometryFactory.Default), new JsonStringEnumConverter() },
            }).AsTask())!;
        }
    }
}
