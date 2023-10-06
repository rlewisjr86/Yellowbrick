using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yellowbrick.Models.Domain.Lookups;

namespace Yellowbrick.Models.Domain
{
    public class UserBase : IUserAuthData
    {
        public int Id { get; set; }
        public string Name { get; set;  }
        public string Email { get; set; }
        public IEnumerable<string> Roles { get; set; }
        public object TenantId { get; set; }
    }
}
