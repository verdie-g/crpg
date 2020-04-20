using System;
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
        public void SetUp()
        {
            // force creation of those
            _db = null;
            _mapper = null;
        }

        [TearDown]
        public void TearDown()
        {
            _db?.Dispose();
        }

        private CrpgDbContext InitDb()
        {
            var options = new DbContextOptionsBuilder<CrpgDbContext>()
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