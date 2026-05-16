using System;
using System.Collections.Generic;
using System.Text;
using TwentyOne.DAL.Context;
using TwentyOne.DAL.Entities;
using TwentyOne.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TwentyOne.DAL.Repositories.Implementations
{
    public class CouponRepository : ICouponRepository
    {
        private readonly AppDbContext _context;

        public CouponRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Coupon>> GetAllAsync()
        {
            return await _context.Coupons
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Coupon?> GetByIdAsync(int id)
        {
            return await _context.Coupons
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Coupon?> GetByCodeAsync(string code)
        {
            return await _context.Coupons
                .FirstOrDefaultAsync(c =>
                    c.Code.ToLower() == code.ToLower());
        }

        public async Task<Coupon> CreateAsync(Coupon coupon)
        {
            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();
            return coupon;
        }

        public async Task<Coupon> UpdateAsync(Coupon coupon)
        {
            _context.Coupons.Update(coupon);
            await _context.SaveChangesAsync();
            return coupon;
        }

        public async Task DeleteAsync(Coupon coupon)
        {
            _context.Coupons.Remove(coupon);
            await _context.SaveChangesAsync();
        }
    }
}
