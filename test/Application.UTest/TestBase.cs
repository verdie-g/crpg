using System;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Trpg.Application.Common.Mappings;
using Trpg.Common;
using Trpg.Persistence;

namespace Trpg.Application.UTest
{
    public class TestBase
    {
        protected TrpgDbContext _db;
        protected IMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<TrpgDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new TrpgDbContext(options, Mock.Of<ILoggerFactory>(), Mock.Of<IDateTime>());

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