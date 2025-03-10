﻿using Marketplace.DataAccess.Entities;
using Marketplace.Helpers;

namespace Marketplace.DataAccess.Repositories
{
    public interface IProductRepository
    {
        Task<(IEnumerable<Product>, PaginationMetadata)> GetProductsAsync(string? title, string? category, decimal? minPrice, decimal? maxPrice, int pageNumber, int pageSize);
        Task<Product?> GetProductByIdAsync(Guid Id);
    }
}

