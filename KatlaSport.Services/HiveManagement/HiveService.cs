using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using KatlaSport.DataAccess;
using KatlaSport.DataAccess.ProductStoreHive;
using DbHive = KatlaSport.DataAccess.ProductStoreHive.StoreHive;

namespace KatlaSport.Services.HiveManagement
{
    /// <summary>
    /// Represents a hive service.
    /// </summary>
    public class HiveService : IHiveService
    {
        private readonly IProductStoreHiveContext _context;
        private readonly IUserContext _userContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="HiveService"/> class with specified <see cref="IProductStoreHiveContext"/> and <see cref="IUserContext"/>.
        /// </summary>
        /// <param name="context">A <see cref="IProductStoreHiveContext"/>.</param>
        /// <param name="userContext">A <see cref="IUserContext"/>.</param>
        public HiveService(IProductStoreHiveContext context, IUserContext userContext)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userContext = userContext ?? throw new ArgumentNullException();
        }

        /// <summary>
        /// Returns all provided hives
        /// </summary>
        /// <returns>List of Hives</returns>
        public async Task<List<HiveListItem>> GetHivesAsync()
        {
            var dbHives = await _context.Hives.OrderBy(h => h.Id).ToArrayAsync();
            var hives = dbHives.Select(h => Mapper.Map<HiveListItem>(h)).ToList();

            foreach (HiveListItem hive in hives)
            {
                hive.HiveSectionCount = await _context.Sections.Where(s => s.StoreHiveId == hive.Id).CountAsync();
            }

            return hives;
        }

        /// <summary>
        /// Returns hive according to its ID
        /// </summary>
        /// <param name="hiveId">ID for Hive element</param>
        /// <returns>Hive instance</returns>
        public async Task<Hive> GetHiveAsync(int hiveId)
        {
            var dbHives = await _context.Hives.Where(h => h.Id == hiveId).ToArrayAsync();
            if (dbHives.Length == 0)
            {
                throw new RequestedResourceNotFoundException();
            }

            return Mapper.Map<DbHive, Hive>(dbHives[0]);
        }

        /// <summary>
        /// Create new hive
        /// </summary>
        /// <param name="createRequest">Data provided from request</param>
        /// <returns>Hive's information</returns>
        public async Task<Hive> CreateHiveAsync(UpdateHiveRequest createRequest)
        {
            var dbHives = _context.Hives.Where(h => h.Code == createRequest.Code).ToArray();
            if (dbHives.Length > 0)
            {
                throw new RequestedResourceHasConflictException("code");
            }

            var dbHive = Mapper.Map<UpdateHiveRequest, DbHive>(createRequest);
            dbHive.CreatedBy = _userContext.UserId;
            dbHive.LastUpdatedBy = _userContext.UserId;
            _context.Hives.Add(dbHive);

            await _context.SaveChangesAsync();

            return Mapper.Map<Hive>(dbHive);
        }

        /// <summary>
        /// Update existing hive accoring to its id and data provided from request
        /// </summary>
        /// <param name="hiveId">Hive ID</param>
        /// <param name="updateRequest">Data provided from request</param>
        /// <returns>Hive's info</returns>
        public async Task<Hive> UpdateHiveAsync(int hiveId, UpdateHiveRequest updateRequest)
        {
            var dbHives = _context.Hives.Where(p => p.Code == updateRequest.Code && p.Id != hiveId).ToArray();
            if (dbHives.Length > 0)
            {
                throw new RequestedResourceHasConflictException("code");
            }

            dbHives = await _context.Hives.Where(p => p.Id == hiveId).ToArrayAsync();
            if (dbHives.Length == 0)
            {
                throw new RequestedResourceNotFoundException();
            }

            var dbHive = dbHives[0];

            Mapper.Map(updateRequest, dbHive);
            dbHive.LastUpdatedBy = _userContext.UserId;

            await _context.SaveChangesAsync();

            return Mapper.Map<Hive>(dbHive);
        }

        /// <summary>
        /// Delete existing hive according to its ID
        /// </summary>
        /// <param name="hiveId">Hive ID</param>
        /// <returns>No data</returns>
        public async Task DeleteHiveAsync(int hiveId)
        {
            var dbHives = await _context.Hives.Where(p => p.Id == hiveId).ToArrayAsync();
            if (dbHives.Length == 0)
            {
                throw new RequestedResourceNotFoundException();
            }

            var dbHive = dbHives[0];
            if (dbHive.IsDeleted == false)
            {
                throw new RequestedResourceHasConflictException();
            }

            _context.Hives.Remove(dbHive);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Changes deletion status for hive according to its ID
        /// </summary>
        /// <param name="hiveId">Hive ID</param>
        /// <param name="deletedStatus">Status for deletion</param>
        /// <returns>No data</returns>
        public async Task SetStatusAsync(int hiveId, bool deletedStatus)
        {
            var storeHives = await _context.Hives.Where(c => hiveId == c.Id).ToArrayAsync();

            if (storeHives.Length == 0)
            {
                throw new RequestedResourceNotFoundException();
            }

            var storeHive = storeHives[0];
            if (storeHive.IsDeleted != deletedStatus)
            {
                storeHive.IsDeleted = deletedStatus;
                storeHive.LastUpdated = DateTime.UtcNow;
                storeHive.LastUpdatedBy = _userContext.UserId;
                await _context.SaveChangesAsync();
            }
        }
    }
}
