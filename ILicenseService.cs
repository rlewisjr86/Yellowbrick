using Yellowbrick.Models;
using Yellowbrick.Models.Domain.License;
using Yellowbrick.Models.Requests.Licenses;


namespace Yellowbrick.Services.Interfaces
{
    public interface ILicenseService
    {

        int Create(LicenseAddRequest model, int userId);
        void Delete(int id);
        Paged<License> LicensePagination(int pageIndex, int pageSize);

        public void Update(LicenseUpdateRequest model, int userId);

        public Paged<License> SelectByCreatedBy(int createdByUserId, int pageIndex, int pageSize);

        public License Get(int id);
        public Paged<License> Search(int pageIndex, int pageSize, string query);
    }
}