﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using KatlaSport.DataAccess;
using KatlaSport.DataAccess.ProductStoreHive;
using DbHiveSection = KatlaSport.DataAccess.ProductStoreHive.StoreHiveSection;

namespace KatlaSport.Services.HiveManagement
{
    /// <summary>
    /// Represents a hive section service.
    /// </summary>
    public class HiveSectionService : IHiveSectionService
    {
        private readonly IProductStoreHiveContext _context;
        private readonly IUserContext _userContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="HiveSectionService"/> class with specified <see cref="IProductStoreHiveContext"/> and <see cref="IUserContext"/>.
        /// </summary>
        /// <param name="context">A <see cref="IProductStoreHiveContext"/>.</param>
        /// <param name="userContext">A <see cref="IUserContext"/>.</param>
        public HiveSectionService(IProductStoreHiveContext context, IUserContext userContext)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userContext = userContext ?? throw new ArgumentNullException();
        }

        /// <summary>
        /// Returns all provided sections
        /// </summary>
        /// <returns>List of items of HiveSectionListItem</returns>
        public async Task<List<HiveSectionListItem>> GetHiveSectionsAsync()
        {
            var dbHiveSections = await _context.Sections.OrderBy(s => s.Id).ToArrayAsync();
            var hiveSections = dbHiveSections.Select(s => Mapper.Map<HiveSectionListItem>(s)).ToList();
            return hiveSections;
        }

        /// <summary>
        /// Returns hive section according to section id
        /// </summary>
        /// <param name="hiveSectionId">Section id</param>
        /// <returns>Item of HiveSection</returns>
        public async Task<HiveSection> GetHiveSectionAsync(int hiveSectionId)
        {
            var dbHiveSections = await _context.Sections.Where(s => s.Id == hiveSectionId).ToArrayAsync();
            if (dbHiveSections.Length == 0)
            {
                throw new RequestedResourceNotFoundException();
            }

            return Mapper.Map<DbHiveSection, HiveSection>(dbHiveSections[0]);
        }

        /// <summary>
        /// Returns hive section according to hive id
        /// </summary>
        /// <param name="hiveId">Hive id</param>
        /// <returns>Item of HiveSectionListItem</returns>
        public async Task<List<HiveSectionListItem>> GetHiveSectionsAsync(int hiveId)
        {
            var dbHiveSections = await _context.Sections.Where(s => s.StoreHiveId == hiveId).OrderBy(s => s.Id).ToArrayAsync();
            var hiveSections = dbHiveSections.Select(s => Mapper.Map<HiveSectionListItem>(s)).ToList();
            return hiveSections;
        }

        /// <summary>
        /// Set status to HiveSection according to its hiveSectionId and new status
        /// </summary>
        /// <param name="hiveSectionId">Hive section id</param>
        /// <param name="deletedStatus">New status</param>
        /// <returns>Instance of Task</returns>
        public async Task SetStatusAsync(int hiveSectionId, bool deletedStatus)
        {
            var storeHiveSections = await _context.Sections.Where(c => hiveSectionId == c.Id).ToArrayAsync();

            if (storeHiveSections.Length == 0)
            {
                throw new RequestedResourceNotFoundException();
            }

            var storeHiveSection = storeHiveSections[0];
            if (storeHiveSection.IsDeleted != deletedStatus)
            {
                storeHiveSection.IsDeleted = deletedStatus;
                storeHiveSection.LastUpdated = DateTime.UtcNow;
                storeHiveSection.LastUpdatedBy = _userContext.UserId;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Create new hive section
        /// </summary>
        /// <param name="createRequest">Data provided from request</param>
        /// <returns>Hive Section's information</returns>
        public async Task<HiveSection> CreateHiveSectionAsync(UpdateHiveSectionRequest createRequest)
        {
            var dbHiveSections = await _context.Sections.Where(h => h.Code == createRequest.Code).ToArrayAsync();
            if (dbHiveSections.Length > 0)
            {
                throw new RequestedResourceHasConflictException("code");
            }

            var dbHive = Mapper.Map<UpdateHiveSectionRequest, DbHiveSection>(createRequest);
            dbHive.CreatedBy = _userContext.UserId;
            dbHive.LastUpdatedBy = _userContext.UserId;
            _context.Sections.Add(dbHive);

            await _context.SaveChangesAsync();

            return Mapper.Map<HiveSection>(dbHive);
        }

        /// <summary>
        /// Update existing hive section accoring to its id and data provided from request
        /// </summary>
        /// <param name="hiveId">Hive ID</param>
        /// <param name="updateRequest">Data provided from request</param>
        /// <returns>Hive Section's info</returns>
        public async Task<HiveSection> UpdateHiveSectionAsync(int hiveId, UpdateHiveSectionRequest updateRequest)
        {
            var dbHives = _context.Sections.Where(p => p.Code == updateRequest.Code && p.Id != hiveId).ToArray();
            if (dbHives.Length > 0)
            {
                throw new RequestedResourceHasConflictException("code");
            }

            dbHives = await _context.Sections.Where(p => p.Id == hiveId).ToArrayAsync();
            if (dbHives.Length == 0)
            {
                throw new RequestedResourceNotFoundException();
            }

            var dbHive = dbHives[0];

            Mapper.Map(updateRequest, dbHive);
            dbHive.LastUpdatedBy = _userContext.UserId;

            await _context.SaveChangesAsync();

            return Mapper.Map<HiveSection>(dbHive);
        }

        /// <summary>
        /// Delete existing hive section according to its ID
        /// </summary>
        /// <param name="hiveId">Hive ID</param>
        /// <returns>No data</returns>
        public async Task DeleteHiveSectionAsync(int hiveId)
        {
            var dbHives = await _context.Sections.Where(p => p.Id == hiveId).ToArrayAsync();
            if (dbHives.Length == 0)
            {
                throw new RequestedResourceNotFoundException();
            }

            var dbHive = dbHives[0];
            if (dbHive.IsDeleted == false)
            {
                throw new RequestedResourceHasConflictException();
            }

            _context.Sections.Remove(dbHive);
            await _context.SaveChangesAsync();
        }
    }
}
