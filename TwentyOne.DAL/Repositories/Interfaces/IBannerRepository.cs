using System;
using System.Collections.Generic;
using System.Text;
using TwentyOne.DAL.Entities;

namespace TwentyOne.DAL.Repositories.Interfaces
{
    public interface IBannerRepository
    {
        Task<List<Banner>> GetAllAsync();
        Task<List<Banner>> GetActiveAsync();
        Task<Banner?> GetByIdAsync(int id);
        Task<Banner> CreateAsync(Banner banner);
        Task<Banner> UpdateAsync(Banner banner);
        Task DeleteAsync(Banner banner);
    }
}
