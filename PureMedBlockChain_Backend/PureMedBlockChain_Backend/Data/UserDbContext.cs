using Microsoft.EntityFrameworkCore;
using PureMedBlockChain_Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PureMedBlockChain_Backend.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {

        }

        public DbSet<User> UserAccounts { get; set; }
    }
}
