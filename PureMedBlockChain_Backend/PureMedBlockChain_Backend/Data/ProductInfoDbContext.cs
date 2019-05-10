using Microsoft.EntityFrameworkCore;
using PureMedBlockChain_Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PureMedBlockChain_Backend.Data
{
    public class ProductInfoDbContext:DbContext
    {
        public ProductInfoDbContext(DbContextOptions<ProductInfoDbContext> options):base(options)
        {

        }

        public DbSet<ProductInfo> ProductsInfos { set; get; }
    }
}
