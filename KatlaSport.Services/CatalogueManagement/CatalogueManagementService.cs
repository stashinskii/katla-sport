using System;
using System.Collections.Generic;
using System.Linq;
using KatlaSport.DataAccess.ProductCatalogue;

namespace KatlaSport.Services.CatalogueManagement
{
    /// <summary>
    /// Represents service for managing products and its categories
    /// </summary>
    internal class CatalogueManagementService : ICatalogueManagementService
    {
        /// <summary>
        /// Represents context to get access to tables of Catalogues and Products
        /// </summary>
        private readonly IProductCatalogueContext _catalogueContext;

        public CatalogueManagementService(IProductCatalogueContext catalogueContext)
        {
            _catalogueContext = catalogueContext ?? throw new ArgumentNullException(nameof(catalogueContext));
        }

        /// <summary>
        /// Get all provided categories of products
        /// </summary>
        /// <returns>Returns collection of categories</returns>
        public IList<Category> GetProductCategories()
        {
            var categories = _catalogueContext.Categories.ToArray();

            return categories.Select(c => new Category
            {
                Id = c.Id,
                Name = c.Name
            }).ToList();
        }

        /// <summary>
        /// Add new product category
        /// </summary>
        public void AddProductCategory()
        {
        }
    }
}
