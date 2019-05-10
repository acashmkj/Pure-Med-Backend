using Microsoft.EntityFrameworkCore;
using PureMedBlockChain_Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PureMedBlockChain_Backend.Data
{
    public class BlockChainsDbContext:DbContext
    {
        public BlockChainsDbContext(DbContextOptions<BlockChainsDbContext> options):base(options)
        {

        }

        public DbSet<StoreBlockChains> AllBlockChains { get; set; }
    }
}
