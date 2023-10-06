using Yellowbrick.Models.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yellowbrick.Models.Requests.Licenses
{
    public class LicenseAddRequest
    {
        [Required]
        public int LicenseTypeId { get; set; }
        public int LicenseStateId { get; set; }
        [Required]
        public string LicenseNumber { get; set; }
        public DateTime DateExpiration { get; set; }
        public DateTime IssueDate { get; set; }
        public bool IsActive { get; set; }
        [Required]
        public int IsFederal { get; set; }


    }
}
