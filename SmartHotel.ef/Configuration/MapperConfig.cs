using AutoMapper;
using SmartHotel.core.Dtos.User;
using SmartHotel.core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHotel.ef.Configuration
{
    public class MapperConfig : Profile
    {
        public MapperConfig() 
        {
            //creater maping configuration of user model
            CreateMap<ApiUser, LoginUserDto>().ReverseMap();
            CreateMap<ApiUser, RegisterUserDto>().ReverseMap();
            CreateMap<ApiUser, AuthModel>().ReverseMap();
            CreateMap<ApiUser, AddToRoleDto>().ReverseMap();

        }
    }
}
