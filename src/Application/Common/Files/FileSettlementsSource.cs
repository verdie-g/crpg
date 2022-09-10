﻿using System.Text.Json;
using System.Text.Json.Serialization;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Settlements.Models;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;

namespace Crpg.Application.Common.Files;

internal class FileSettlementsSource : ISettlementsSource
{
    private static readonly string StrategusSettlementsPath =
        Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) + "/Common/Files/settlements.json";

    public async Task<IEnumerable<SettlementCreation>> LoadStrategusSettlements()
    {
        await using var file = File.OpenRead(StrategusSettlementsPath);
        return (await JsonSerializer.DeserializeAsync<IEnumerable<SettlementCreation>>(file, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new GeoJsonConverterFactory(GeometryFactory.Default), new JsonStringEnumConverter() },
        }).AsTask())!;
    }
}
