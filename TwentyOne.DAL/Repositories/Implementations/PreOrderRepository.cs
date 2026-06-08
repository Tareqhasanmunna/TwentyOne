using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TwentyOne.DAL.Context;
using TwentyOne.DAL.Entities;
using TwentyOne.DAL.Repositories.Interfaces;
using TwentyOne.Shared.Enums;

namespace TwentyOne.DAL.Repositories.Implementations
{
    public class PreOrderRepository: IPreOrderRepository
    {
        private readonly AppDbContext _context;

        public PreOrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<PreOrder>> GetAllAsync()
        {
            return await _context.PreOrders
                .Include(p => p.Product)
                    .ThenInclude(p => p.Images)
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<PreOrder>> GetByUserIdAsync(string userId)
        {
            return await _context.PreOrders
                .Include(p => p.Product)
                    .ThenInclude(p => p.Images)
                .Include(p => p.User)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<PreOrder>> GetByStatusAsync(
            PreOrderStatus status)
        {
            return await _context.PreOrders
                .Include(p => p.Product)
                    .ThenInclude(p => p.Images)
                .Include(p => p.User)
                .Where(p => p.Status == status)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<PreOrder?> GetByIdAsync(int id)
        {
            return await _context.PreOrders
                .Include(p => p.Product)
                    .ThenInclude(p => p.Images)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<PreOrder?> GetByPreOrderNumberAsync(
            string preOrderNumber)
        {
            return await _context.PreOrders
                .Include(p => p.Product)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p =>
                    p.PreOrderNumber == preOrderNumber);
        }

        public async Task<bool> HasUserPreOrderedAsync(
            string userId, int productId)
        {
            return await _context.PreOrders
                .AnyAsync(p =>
                    p.UserId == userId &&
                    p.ProductId == productId &&
                    p.Status != PreOrderStatus.Cancelled);
        }

        public async Task<PreOrder> CreateAsync(PreOrder preOrder)
        {
            _context.PreOrders.Add(preOrder);
            await _context.SaveChangesAsync();
            return preOrder;
        }

        public async Task<PreOrder> UpdateAsync(PreOrder preOrder)
        {
            _context.PreOrders.Update(preOrder);
            await _context.SaveChangesAsync();
            return preOrder;
        }
    }
}
