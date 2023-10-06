using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yellowbrick.Models.Domain.Lookups;

namespace Yellowbrick.Models.Domain
{
    public class UserAuth
    {
        public int Id { get; set;}
        public string Email { get; set;}
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Mi { get; set; }
        public List<LookUp> Roles { get; set; }
        public string Password {  get; set; }
       
    }
}
