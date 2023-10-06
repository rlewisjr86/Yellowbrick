using Yellowbrick.Data.Providers;
using Yellowbrick.Models;
using Yellowbrick.Models.Domain;
using Yellowbrick.Models.Requests.Users;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Yellowbrick.Services
{
    public interface IUserService
    {
        Task<bool> LogInAsync(string email, string password);

        Task<bool> LogInTest(string email, string password, int id, string[] roles = null);

        int Create(UserAddRequest model);

        Task<bool> Login(UserLoginRequest loginModel);

        BaseUser VerifyEmail(string email);

        IUserAuthData GetAuthData(UserLoginRequest loginModel);

        string GetAvatarUrl(string email);

        void Confirm(string email, string token);

        void UpdatePassword(UserChangePasswordRequest updateModel);

        User GetById(int id);

        Paged<User> GetAll(int pageIndex, int pageSize);

        List<DashboardModel> GetAllAdminDashboard();

        List<DashboardModel> GetAllDashboardById(int id);

        string GetPassByEmail(string email);

        UserAuth GetAccountByEmail(string email);

        void UpdateStatus(int statusTypeId, string email);

        string GetCustomErrorMessage(int errorNumber);

        void CreateToken(int userId, int? tokenType, string token);

        void DeleteToken(int id, string token);

        UserAuth GetByToken(string token);

        List<User> GetAllUnpaginated();

        Paged<User> SearchUser(int pageIndex, int pageSize, string query);

        void Update(UserUpdateRequest model);
        Paged<User> GetAllOfRoleTypeV2(int role, int pageIndex, int pageSize);
    }
}