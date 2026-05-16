using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TwentyOne.DAL.Context;
using TwentyOne.DAL.Entities;
using TwentyOne.DAL.Repositories.Interfaces;

namespace TwentyOne.DAL.Repositories.Implementations
{
    public class BrandRepository: IBrandRepository
    {
        private readonly AppDbContext _context;

        public BrandRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Brand>> GetAllAsync()
        {
            return await _context.Brands
                .Include(b => b.Products)
                .OrderBy(b => b.Name)
                .ToListAsync();
        }

        public async Task<Brand?> GetByIdAsync(int id)
        {
            return await _context.Brands
                .Include(b => b.Products)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Brand?> GetByNameAsync(string name)
        {
            return await _context.Brands
                .FirstOrDefaultAsync(b => b.Name.ToLower() == name.ToLower());
        }

        public async Task<Brand> CreateAsync(Brand brand)
        {
            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();
            return brand;
        }

        public async Task<Brand> UpdateAsync(Brand brand)
        {
            _context.Brands.Update(brand);
            await _context.SaveChangesAsync();
            return brand;
        }

        public async Task DeleteAsync(Brand brand)
        {
            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasProductsAsync(int brandId)
        {
            return await _context.Products
                .AnyAsync(p => p.BrandId == brandId);
        }
    }
}
