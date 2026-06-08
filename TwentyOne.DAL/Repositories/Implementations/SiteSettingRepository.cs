using System;
using System.Collections.Generic;
using System.Text;
using TwentyOne.DAL.Context;
using TwentyOne.DAL.Entities;
using TwentyOne.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TwentyOne.DAL.Repositories.Implementations
{
    public class SiteSettingRepository : ISiteSettingRepository
    {
        private readonly AppDbContext _context;

        public SiteSettingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string?> GetValueAsync(string key)
        {
            var setting = await _context.SiteSettings
                .FirstOrDefaultAsync(s => s.Key == key);
            return setting?.Value;
        }

        public async Task SetValueAsync(string key, string value)
        {
            var setting = await _context.SiteSettings
                .FirstOrDefaultAsync(s => s.Key == key);

            if (setting == null)
            {
                _context.SiteSettings.Add(new SiteSetting
                {
                    Key = key,
                    Value = value
                });
            }
            else
            {
                setting.Value = value;
                _context.SiteSettings.Update(setting);
            }

            await _context.SaveChangesAsync();
        }
    }
}
