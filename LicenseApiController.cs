using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Yellowbrick.Models;

using Yellowbrick.Models.Domain.License;
using Yellowbrick.Models.Requests.Licenses;
using Yellowbrick.Services;
using Yellowbrick.Services.Interfaces;
using Yellowbrick.Web.Controllers;
using Yellowbrick.Web.Models.Responses;
using System;


namespace Yellowbrick.Web.Api.Controllers
{
    [Route("api/licenses")]
    [ApiController]
    public class LicenseApiController : BaseApiController
    {

        private ILicenseService _LicenseService = null;
        private IAuthenticationService<int> _AuthService = null;
        public LicenseApiController(
                ILicenseService service
                , ILogger<LicenseApiController> logger
                , IAuthenticationService<int> authService) : base(logger)
        {
            _LicenseService = service;
            _AuthService = authService;
        }

        [HttpPost ("new")]
        public ActionResult<ItemResponse<int>> Create(LicenseAddRequest model)
        {
            ObjectResult result = null;

            
            try
            {
                int userId = _AuthService.GetCurrentUserId();
                int id = _LicenseService.Create(model, userId);
                

                ItemResponse<int> response = new ItemResponse<int>() { Item = id };

                result = Created201(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                ErrorResponse response = new ErrorResponse(ex.Message);

                result = StatusCode(500, response);
            }
            return result;

        }
        
        [HttpPut("{id:int}")]
        public ActionResult<ItemResponse<int>> Update(LicenseUpdateRequest model)
        {
            
            int code = 200;
            BaseResponse response = null;
            try
            {
                int userId = _AuthService.GetCurrentUserId();
                _LicenseService.Update(model, userId);

                response = new SuccessResponse();
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message);
            }

            return StatusCode(code, response);
        }
       
        [HttpGet("paginate/current")]
        public ActionResult<ItemResponse<Paged<License>>> SelectByCreatedBy( int pageIndex, int pageSize)
        {
            int code = 200;
            BaseResponse response = null;
            try
            {
                int userId = _AuthService.GetCurrentUserId();
                Paged<License> paged = _LicenseService.SelectByCreatedBy(userId, pageIndex,  pageSize);

                if (paged == null)
                {
                    code = 404;
                    response = new ErrorResponse("App Resource not found.");
                }
                else
                {
                    response = new ItemResponse<Paged<License>> { Item = paged };
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

        [HttpGet("paginate")]
        public ActionResult<ItemResponse<Paged<License>>> LicensePagination(int pageIndex, int pageSize)
        {
            int code = 200;
            BaseResponse response = null;
            try
            {
                Paged<License> paged = _LicenseService.LicensePagination(pageIndex, pageSize);

                if (paged == null)
                {
                    code = 404;
                    response = new ErrorResponse("App Resource not found.");
                }
                else
                {
                    response = new ItemResponse<Paged<License>> { Item = paged };
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

        [HttpGet("{id:int}")]
        public ActionResult<ItemResponse<License>> Get(int id)
        {
            int code = 200;
            BaseResponse response = null;
            try
            {
                License license = _LicenseService.Get(id);
                if (license == null)
                {
                    code = 404;
                    response = new ErrorResponse("resource not found");
                }
                else
                {
                    response = new ItemResponse<License> { Item = license };
                }
            }
            catch (Exception ex)
            {
                response = new ErrorResponse(ex.Message);
                code = 500;
                base.Logger.LogError(ex.ToString());
            }
            return StatusCode(code, response);


        }

        [HttpGet("search")]
        public ActionResult<ItemResponse<Paged<License>>> Search(int pageIndex, int pageSize, string query)
        {
            int code = 200;
            BaseResponse response = null;
            try
            {
                Paged<License> paged = _LicenseService.Search(pageIndex, pageSize, query);

                if (paged == null)
                {
                    code = 404;
                    response = new ErrorResponse("Application resource not found");
                }
                else
                {
                    response = new ItemResponse<Paged<License>> { Item = paged };
                }
            }
            catch (Exception ex)
            {
                code = 500;
                response = new ErrorResponse(ex.Message.ToString());
                base.Logger.LogError(ex.ToString());
            }
            return StatusCode(code, response);
        }



        [HttpDelete("{id:int}")]
        public ActionResult<SuccessResponse> Delete(int id)
        {
            int code = 200;

            BaseResponse response = null;

            try
            {
                _LicenseService.Delete(id);

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
