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
        protected CrpgDbContext _db;
        protected IMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<CrpgDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new CrpgDbContext(options, Mock.Of<IDateTime>());

            var configurationProvider = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = configurationProvider.CreateMapper();
        }

        [TearDown]
        public void TearDown()
        {
            _db.Dispose();
        }
    }
}