using System;
using System.Collections.Generic;
using System.Text;
using TwentyOne.DAL.Entities;

namespace TwentyOne.BLL.Services.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(ApplicationUser user, IList<string> roles);
    }
}
