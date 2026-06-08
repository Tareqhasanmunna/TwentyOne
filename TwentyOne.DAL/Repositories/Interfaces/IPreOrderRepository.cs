using System;
using System.Collections.Generic;
using System.Text;
using TwentyOne.DAL.Entities;
using TwentyOne.Shared.Enums;

namespace TwentyOne.DAL.Repositories.Interfaces
{
    public interface IPreOrderRepository
    {
        Task<List<PreOrder>> GetAllAsync();
        Task<List<PreOrder>> GetByUserIdAsync(string userId);
        Task<List<PreOrder>> GetByStatusAsync(PreOrderStatus status);
        Task<PreOrder?> GetByIdAsync(int id);
        Task<PreOrder?> GetByPreOrderNumberAsync(string preOrderNumber);
        Task<bool> HasUserPreOrderedAsync(string userId, int productId);
        Task<PreOrder> CreateAsync(PreOrder preOrder);
        Task<PreOrder> UpdateAsync(PreOrder preOrder);
    }
}
