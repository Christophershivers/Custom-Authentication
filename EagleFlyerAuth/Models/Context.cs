using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EagleFlyerAuth.Models
{
    public class Context: DbContext
    {
        public Context(DbContextOptions options) : base(options)
        {

        }
        public DbSet<SignupModel> Users { get; set; }
    }
}
