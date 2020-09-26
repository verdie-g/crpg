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
        /// <summary>
        /// The database that should be used by the 3 contexts (arrange, act, assert). Tests shouldn't use a single
        /// context because the following scenario:
        /// - db = new CrpgDbContext()
        /// - db.Characters.Add(Character { User = new User() })
        /// - new MyHandler(db).Handle
        /// Inside Handle db.Character.First().User != null even though Include(c => c.User) was not used. This is
        /// because the User was already loaded in the DbContext so EntityFramework return it. So the tested code
        /// is not running in the same condition that in production (i.e. with a clean DbContext) which make the
        /// test not terrible. The test could be much stronger by using ArrangeDb when calling Characters.Add and
        /// using ActDb to create the Handler.
        /// </summary>
        private DbContextOptions<CrpgDbContext>? _dbOptions;

        private CrpgDbContext? _arrangeDb;
        private CrpgDbContext? _actDb;
        private CrpgDbContext? _assertDb;
        private IMapper? _mapper;

        /// <summary>
        /// DbContext to use to initialize the database for a test.
        /// </summary>
        protected CrpgDbContext ArrangeDb => _arrangeDb ??= InitDb();

        /// <summary>
        /// DbContext to use for the tested code.
        /// </summary>
        protected CrpgDbContext ActDb => _actDb ??= InitDb();

        /// <summary>
        /// DbContext to use to assert the state of the database.
        /// </summary>
        protected CrpgDbContext AssertDb => _assertDb ??= InitDb();

        protected IMapper Mapper => _mapper ??= InitMapper();

        [SetUp]
        public virtual Task SetUp()
        {
            _dbOptions = new DbContextOptionsBuilder<CrpgDbContext>()
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            // force creation of those
            _arrangeDb = null;
            _actDb = null;
            _assertDb = null;
            _mapper = null;
            return Task.CompletedTask;
        }

        [TearDown]
        public virtual Task TearDown()
        {
            _arrangeDb?.Dispose();
            _actDb?.Dispose();
            _assertDb?.Dispose();
            return Task.CompletedTask;
        }

        private CrpgDbContext InitDb() => new CrpgDbContext(_dbOptions!, Mock.Of<IDateTimeOffset>());

        private IMapper InitMapper()
        {
            var configurationProvider = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            return configurationProvider.CreateMapper();
        }
    }
}
