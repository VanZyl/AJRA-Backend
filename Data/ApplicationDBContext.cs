using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AJRAApis.Models;
using Microsoft.EntityFrameworkCore;

namespace AJRAApis.Data
{
    public class ApplicationDBContext: DbContext
    {
        public ApplicationDBContext(DbContextOptions dbContextOptions): base(dbContextOptions) {}

        public DbSet<Employee> Employees { get; set; }

        public DbSet<PaySlips> PaySlips { get; set; }

        public DbSet<Redbook> Redbook { get; set; }
        public DbSet<EmployeeLeave> EmployeeLeave { get; set; }

        
    }
}