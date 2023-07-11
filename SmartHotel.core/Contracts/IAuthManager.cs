using SmartHotel.core.Dtos.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHotel.core.Contracts
{
    public interface IAuthManager
    {
        Task<AuthModel> RegisterAsync(RegisterUserDto userDto);
        Task<AuthModel> GetTokenAsync(LoginUserDto model);
        Task<string> AddRoleAsync(AddToRoleDto model);
    }
}
