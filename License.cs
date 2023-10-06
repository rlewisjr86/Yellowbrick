using System;
using Yellowbrick.Models.Domain.Lookups;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yellowbrick.Models.Domain.License
{
    public class License
    {
        public int Id { get; set; }
        public LookUp LicenseType { get; set; }
        public LookUp LicenseState { get; set; }
        public string LicenseNumber { get; set; }
        public DateTime DateExpiration { get; set; }
        public DateTime IssueDate { get; set; }
        public bool IsActive { get; set; }
        public int IsFederal { get; set; }
        public BaseUser CreatedBy { get; set; }
        public DateTime DateCreated { get; set; }

    }
}
