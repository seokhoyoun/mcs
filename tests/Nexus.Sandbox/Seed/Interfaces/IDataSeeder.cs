using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.Sandbox.Seed.Interfaces
{
    internal interface IDataSeeder
    {
        Task SeedAsync();
    }
}
