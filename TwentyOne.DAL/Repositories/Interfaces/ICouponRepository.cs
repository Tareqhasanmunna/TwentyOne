using System;
using System.Collections.Generic;
using System.Text;
using TwentyOne.DAL.Entities;

namespace TwentyOne.DAL.Repositories.Interfaces
{
    public interface ICouponRepository
    {
        Task<List<Coupon>> GetAllAsync();
        Task<Coupon?> GetByIdAsync(int id);
        Task<Coupon?> GetByCodeAsync(string code);
        Task<Coupon> CreateAsync(Coupon coupon);
        Task<Coupon> UpdateAsync(Coupon coupon);
        Task DeleteAsync(Coupon coupon);
    }
}
