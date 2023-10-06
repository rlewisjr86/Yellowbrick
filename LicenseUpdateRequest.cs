using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yellowbrick.Models.Requests.Licenses
{
    public class LicenseUpdateRequest : LicenseAddRequest , IModelIdentifier
    {
        public int Id { get; set; }
    }
}
