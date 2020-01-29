using AutoMapper;
using NUnit.Framework;
using Trpg.Application.Common.Mappings;
using IConfigurationProvider = AutoMapper.IConfigurationProvider;

namespace Trpg.Application.UTest.Common.Mappings
{
    public class MappingTest
    {
        private static readonly IConfigurationProvider ConfigurationProvider = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        private static readonly IMapper Mapper = ConfigurationProvider.CreateMapper();

        [Test]
        public void ShouldHaveValidConfiguration()
        {
            ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}