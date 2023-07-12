using Microsoft.AspNetCore.Identity;
using SmartHotel.core.Models;
using SmartHotel.ef.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHotel.ef.DataConfigurations
{
    public class DataSeeding
    {
        private ApplicationDbContext context { get;  }
        public DataSeeding(ApplicationDbContext _context)
        {
            context = _context;
        }

       

        public async Task initialize(UserManager<ApiUser> _userManager )
        {
            context.Database.EnsureCreated();
            try
            {
                var x = await _userManager.FindByEmailAsync("Ali@admin.com");
            }
            catch(Exception ex)
            {

            }

            if (await _userManager.FindByEmailAsync("Ali@admin.com") == null )
            {
                var user = new ApiUser
                {
                    Email = "Ali@admin.com",
                    FirstName = "Ali",
                    LastName = "Ali",
                    UserName = "user11",

                };
                await _userManager.CreateAsync(user, "Youssef@123456");
                await _userManager.AddToRoleAsync(user, "Admin");





            }
        }

    }
}
 