using System;
using System.Collections.Generic;
using System.Text;
using TwentyOne.DAL.Entities;

namespace TwentyOne.DAL.Repositories.Interfaces
{
    public interface IBrandRepository
    {
        Task<List<Brand>> GetAllAsync();
        Task<Brand?> GetByIdAsync(int id);
        Task<Brand?> GetByNameAsync(string name);
        Task<Brand> CreateAsync(Brand brand);
        Task<Brand> UpdateAsync(Brand brand);
        Task DeleteAsync(Brand brand);
        Task<bool> HasProductsAsync(int brandId);
    }
}
