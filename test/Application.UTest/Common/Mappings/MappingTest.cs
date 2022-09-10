using AutoMapper;
using Crpg.Application.Common.Mappings;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Mappings;

public class MappingTest
{
    private static readonly IConfigurationProvider ConfigurationProvider = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());

    [Test]
    public void ShouldHaveValidConfiguration()
    {
        ConfigurationProvider.AssertConfigurationIsValid();
    }
}
