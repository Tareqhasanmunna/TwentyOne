using System;
using System.Collections.Generic;
using System.Text;

namespace TwentyOne.DAL.Repositories.Interfaces
{
    public interface ISiteSettingRepository
    {
        Task<string?> GetValueAsync(string key);
        Task SetValueAsync(string key, string value);
    }
}
