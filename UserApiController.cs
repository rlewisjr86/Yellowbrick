using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing.Text;
using System.Dynamic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using Yellowbrick.Models;
using Yellowbrick.Models.Domain;
using Yellowbrick.Models.Domain.Lookups;
using Yellowbrick.Models.Enums;
using Yellowbrick.Models.Requests.Users;
using Yellowbrick.Services;
using Yellowbrick.Services.Interfaces;
using Yellowbrick.Web.Controllers;
using Yellowbrick.Web.Models.Responses;

namespace Yellowbrick.Web.Api.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserApiController : BaseApiController
    {
        private IUserService _service = null;
        private IEmailService _emailService = null;
        private IAuthenticationService<int> _authService = null;

        public UserApiController(IUserService service,
            IEmailService emailService,
            ILogger<UserService> logger,
            IAuthenticationService<int> authService) : base(logger)
        {
            _emailService = emailService;
            _service = service;
            _authService = authService;
        }

        [HttpPost("admininvite")]
        public ActionResult<SuccessResponse> Invite(UserInviteRequest inviteModel)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                BaseUser user = _service.VerifyEmail(inviteModel.Email);

                if (user == null)
                {
                    IUserAuthData admin = _authService.GetCurrentUser();
                    int adminId = admin.Id;

                    int? tokenType = inviteModel.UserRoleId;
                    string token = Guid.NewGuid().ToString();

                    _service.CreateToken(adminId, tokenType, token);

                    _emailService.AdminInviteBody(inviteModel.Email, token);

                    response = new SuccessResponse();
                }
                else
                {
                    code = 403;
                    response = new ErrorResponse("That user already exists!");
                }
                
            }

            catch (Exception ex)
            {
                code = 500;
                base.Logger.LogError(ex.ToString());
                response = new ErrorResponse($"Generic Error: {ex.Message}");
            }
            return StatusCode(code, response);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult<ItemResponse<int>> Create(UserAddRequest model)
        {
            ObjectResult result = null;

            try
            {
                string token = Guid.NewGuid().ToString();
                int id = _service.Create(model);
                int tokenType = (int)TokenType.NewUser;
                _service.CreateToken(id, tokenType, token);

                //_emailService.ConfirmEmailBody(model.Email, model.FirstName, token);

                ItemResponse<int> response = new ItemResponse<int>() { Item = id };
                result = Created201(response);
            }
            catch (SqlException ex)
            {
                int errorNumber = ex.Number;
                //string errorMessage = ex.Message;
                string customErrorMessage = _service.GetCustomErrorMessage(errorNumber);
                ErrorResponse response = new ErrorResponse(customErrorMessage);

                result = StatusCode(403, response);
            }
            catch (Exception ex)
            {
                base.Logger.LogError(ex.ToString());
                ErrorResponse response = new ErrorResponse(ex.Message);

                result = StatusCode(500, response);
            }
            return result;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<SuccessResponse>> Login(UserLoginRequest loginModel)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                bool isSuccessful = await _service.Login(loginModel);

                if (!isSuccessful)
                {
                    code = 404;
                    response = new ErrorResponse("Invalid email or password.");
                }
                else
                {
                    response = new SuccessResponse();
                }
            }
            catch (Exception ex)
            {
                code = 500;
                base.Logger.LogError(ex.ToString());
                response = new ErrorResponse($"{ex.Message}");
            }

            return StatusCode(code, response);
        }

        [AllowAnonymous]
        [HttpPut("requestreset")]
        public ActionResult<ItemResponse<int>> ResetPassword(string email)
        {
            int code = 200;
            BaseResponse response = null;
            try
            {
                BaseUser user = _service.VerifyEmail(email);

                if (user == null)
                {
                    code = 404;
                    response = new ErrorResponse("A user with that email doesn't exist!");
                }
                else
                {
                    int id = user.Id;
                    string token = Guid.NewGuid().ToString();
                    int tokenType = (int)TokenType.ResetPassword;
                    _service.CreateToken(id, tokenType, token);

                    _emailService.ResetPasswordBody(email, user.FirstName, token);

                    response = new ItemResponse<int> { Item = id };
                }
            }
            catch (Exception ex)
            {
                code = 500;
                base.Logger.LogError(ex.ToString());
                response = new ErrorResponse($"Generic Error: {ex.Message}");
            }

            return StatusCode(code, response);

        }

        [AllowAnonymous]
        [HttpPut("updatepassword")]
        public ActionResult<SuccessResponse> UpdatePassword(UserChangePasswordRequest updateModel)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                _service.UpdatePassword(updateModel);
                response = new SuccessResponse();
            }
            catch (Exception ex) 
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
            }

            return StatusCode(code, response);  
        }

        [AllowAnonymous]
        [HttpPost("current")]
        public ActionResult<ItemResponse<CurrentUser>> GetCurrrentPost()
        {
            int code = 200;
            BaseResponse response = null;
            CurrentUser currentUser = null;

            try
            {
                IUserAuthData user = _authService.GetCurrentUser();


                if (user == null)
                {
                    code = 404;
                    response = new ErrorResponse("User not found.");
                }
                else
                {
                    currentUser = new CurrentUser();
                    currentUser.Email = user.Email;
                    currentUser.Name = user.Name;
                    currentUser.Roles = user.Roles;
                    currentUser.Id = user.Id;
                    currentUser.TenantId = user.TenantId;
                    currentUser.AvatarUrl = _service.GetAvatarUrl(user.Email);
                    response = new ItemResponse<CurrentUser> { Item = currentUser };
                }
            }
            catch (Exception ex)
            {
                code = 500;
                base.Logger.LogError(ex.ToString());
                response = new ErrorResponse($"Generic Error: {ex.Message}");
            }

            return StatusCode(code, response);
        }

        [HttpGet("logout")]
        public async Task<ActionResult<SuccessResponse>> LogoutAsync()
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                await _authService.LogOutAsync();
                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                code = 500;
                base.Logger.LogError(ex.ToString());
                response = new ErrorResponse($"Generic Error: {ex.Message}");
            }
            return StatusCode(code, response);
        }

        [AllowAnonymous]
        [HttpPut("confirm")]
        public ActionResult<SuccessResponse> Confirm(string email, string token)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                _service.Confirm(email, token);
                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
            }

            return StatusCode(code, response);
        }

        [HttpGet("{id:int}")]
        public ActionResult<ItemResponse<User>> GetById(int id)
        {
            int code = 200;
            BaseResponse response = null;

            try 
            {
                User user = _service.GetById(id);

                if (user == null)
                {
                    code = 404;
                    response = new ErrorResponse("Application Resource not found.");
                }
                else
                {
                    response = new ItemResponse<User> {  Item = user};
                }
            }
            catch (Exception ex)
            {
                code = 500;
                base.Logger.LogError(ex.ToString());
                response = new ErrorResponse($"Generic Error: {ex.Message}");
            }

            return StatusCode(code, response);
        }

        [HttpGet("paginate")]
        public ActionResult<ItemResponse<Paged<User>>> GetAll(int pageIndex, int pageSize)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                Paged<User> page = _service.GetAll(pageIndex, pageSize);
                if (page == null)
                {
                    code = 404;
                    response = new ErrorResponse("App Resource not found.");
                }
                else
                {
                    response = new ItemResponse<Paged<User>> { Item = page };
                }
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                base.Logger.LogError(ex.ToString());
            }

            return StatusCode(code, response);
        }

        [HttpGet("dashboard")]
        public ActionResult<ItemResponse<List<DashboardModel>>> GetAllDashboard()
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                List<DashboardModel> list = _service.GetAllAdminDashboard();
                if (list == null)
                {
                    code = 404;
                    response = new ErrorResponse("App Resource not found.");
                }
                else
                {
                    response = new ItemResponse<List<DashboardModel>> { Item = list };
                }
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                base.Logger.LogError(ex.ToString());
            }

            return StatusCode(code, response);
        }

        [HttpGet("dashboard/{id:int}")]
        public ActionResult<ItemResponse<List<DashboardModel>>> GetAllDashboardById(int id)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                List<DashboardModel> list = _service.GetAllDashboardById(id);
                if (list == null)
                {
                    code = 404;
                    response = new ErrorResponse("App Resource not found.");
                }
                else
                {
                    response = new ItemResponse<List<DashboardModel>> { Item = list };
                }
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                base.Logger.LogError(ex.ToString());
            }

            return StatusCode(code, response);
        }

        [HttpGet("paginate/role-filter")]
        public ActionResult<ItemResponse<Paged<User>>> GetAllByRole(int role, int pageIndex, int pageSize)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                Paged<User> page = _service.GetAllOfRoleTypeV2(role, pageIndex, pageSize);
                if (page == null)
                {
                    code = 404;
                    response = new ErrorResponse("App Resource not found.");
                }
                else
                {
                    response = new ItemResponse<Paged<User>> { Item = page };
                }
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                base.Logger.LogError(ex.ToString());
            }

            return StatusCode(code, response);
        }

        [HttpGet]
        public ActionResult<ItemsResponse<User>> GetAllUnpaginated()
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                List<User> userList = _service.GetAllUnpaginated();
                if (userList == null)
                {
                    code = 404;
                    response = new ErrorResponse("App Resource not found.");
                }
                else
                {
                    response = new ItemsResponse<User> { Items = userList };
                }
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                base.Logger.LogError(ex.ToString());
            }

            return StatusCode(code, response);
        }

        [HttpGet("email")]
        public ActionResult<ItemResponse<UserAuth>> SelectByEmail(string email)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                UserAuth user = _service.GetAccountByEmail(email);

                if (user == null)
                {
                    code = 404;
                    response = new ErrorResponse("Email not found.");
                }
                else
                {
                    response = new ItemResponse<UserAuth> { Item = user };
                }
            }
            catch (Exception ex)
            {
                code = 500;
                base.Logger.LogError(ex.ToString());
                response = new ErrorResponse($"Generic Error: {ex.Message}");
            }
            return StatusCode(code, response);
        }

        [HttpPut("changestatus")]
        public ActionResult<ItemResponse<int>> UpdateStatus(int statusTypeId, string email)
        {
            //just use int id as the argument in this api controller
            int code = 200;
            BaseResponse response = null;

            try
            {
                _service.UpdateStatus(statusTypeId, email);

                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
            }

            return StatusCode(code, response);
        }

        [HttpGet("token")]
        public ActionResult<ItemResponse<int>> SelectByUserToken(string token)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                UserAuth user = _service.GetByToken(token);

                if (user == null)
                {
                    code = 404;
                    response = new ErrorResponse("Application Resource Not Found");
                }
                else
                {
                    response = new ItemResponse<UserAuth> { Item = user };
                }

            }
            catch (Exception ex) 
            { 
                code = 500;
                base.Logger.LogError(ex.ToString());
                response = new ErrorResponse($"Generic Error: {ex.Message}");
            }

            return StatusCode(code, response);
        }

        [HttpGet("search")]
        public ActionResult<ItemResponse<Paged<User>>> SearchUser(int pageIndex, int pageSize,string query)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                Paged<User> page = _service.SearchUser(pageIndex, pageSize, query);
                if (page == null)
                {
                    code = 404;
                    response = new ErrorResponse("App Resource not found.");
                }
                else
                {
                    response = new ItemResponse<Paged<User>> { Item = page };
                }
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
                base.Logger.LogError(ex.ToString());
            }

            return StatusCode(code, response);
        }

        [HttpPut("{id:int}")]
        public ActionResult<SuccessResponse> Update(UserUpdateRequest model)
        {
            int code = 200;
            BaseResponse response = null;

            try
            {
                _service.Update(model);
                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
            }
            return StatusCode(code, response);
        }
    }
}
