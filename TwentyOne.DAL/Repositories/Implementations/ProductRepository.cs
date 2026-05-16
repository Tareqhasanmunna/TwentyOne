using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TwentyOne.DAL.Context;
using TwentyOne.DAL.Entities;
using TwentyOne.DAL.Repositories.Interfaces;
using TwentyOne.Shared.DTOs.Requests;

namespace TwentyOne.DAL.Repositories.Implementations
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Product> Items, int TotalCount)> GetAllAsync(
            ProductFilterDto filter)
        {
            var query = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Where(p => !p.IsArchived)
                .AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(filter.Search))
                query = query.Where(p =>
                    p.Name.Contains(filter.Search) ||
                    p.Description!.Contains(filter.Search));

            // Filter by brand
            if (filter.BrandId.HasValue)
                query = query.Where(p => p.BrandId == filter.BrandId);

            // Filter by scale
            if (!string.IsNullOrEmpty(filter.Scale))
                query = query.Where(p => p.Scale == filter.Scale);

            // Filter by price range
            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.Price >= filter.MinPrice);
            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= filter.MaxPrice);

            // Filter by limited edition
            if (filter.IsLimitedEdition.HasValue)
                query = query.Where(p =>
                    p.IsLimitedEdition == filter.IsLimitedEdition);

            // Filter by pre-order
            if (filter.IsPreOrder.HasValue)
                query = query.Where(p => p.IsPreOrder == filter.IsPreOrder);

            // Filter by in stock
            if (filter.InStockOnly.HasValue && filter.InStockOnly.Value)
                query = query.Where(p => p.StockQuantity > 0);

            // Sorting
            query = filter.SortBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name" => query.OrderBy(p => p.Name),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product?> GetBySlugAsync(string slug)
        {
            return await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Slug == slug);
        }

        public async Task<bool> SlugExistsAsync(string slug)
        {
            return await _context.Products
                .AnyAsync(p => p.Slug == slug);
        }

        public async Task<Product> CreateAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task DeleteAsync(Product product)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }
}
