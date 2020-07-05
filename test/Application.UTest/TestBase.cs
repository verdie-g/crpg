using System;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Common.Mappings;
using Crpg.Common;
using Crpg.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest
{
    public class TestBase
    {
        private CrpgDbContext? _db;
        private IMapper? _mapper;

        protected CrpgDbContext Db => _db ??= InitDb();
        protected IMapper Mapper => _mapper ??= InitMapper();

        [SetUp]
        public virtual Task SetUp()
        {
            // force creation of those
            _db = null;
            _mapper = null;
            return Task.CompletedTask;
        }

        [TearDown]
        public virtual Task TearDown()
        {
            _db?.Dispose();
            return Task.CompletedTask;
        }

        private CrpgDbContext InitDb()
        {
            var options = new DbContextOptionsBuilder<CrpgDbContext>()
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new CrpgDbContext(options, Mock.Of<IDateTimeOffset>());
        }

        private IMapper InitMapper()
        {
            var configurationProvider = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            return configurationProvider.CreateMapper();
        }
    }
}
