using System;
using System.Collections.Generic;
using System.Text;
using TwentyOne.DAL.Entities;
using TwentyOne.Shared.DTOs.Requests;

namespace TwentyOne.DAL.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<(List<Product> Items, int TotalCount)> GetAllAsync(
            ProductFilterDto filter);
        Task<Product?> GetByIdAsync(int id);
        Task<Product?> GetBySlugAsync(string slug);
        Task<bool> SlugExistsAsync(string slug);
        Task<Product> CreateAsync(Product product);
        Task<Product> UpdateAsync(Product product);
        Task DeleteAsync(Product product);
    }
}
