using JWTExemplo.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JWTExemplo.Data.Context
{
    public class AuthDBContext: DbContext
    {
        public AuthDBContext(DbContextOptions<AuthDBContext> options) :base(options)
        {

        }

        public DbSet<User> User { get; set; }
    }
}
