using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sabio.Models.Domain;
using Sabio.Models.Requests;
using Sabio.Services;
using Sabio.Services.Services;
using Sabio.Web.Controllers;
using Sabio.Web.Models.Responses;


namespace Sabio.Web.Api.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class LoginApiController : BaseApiController
    {
        private ILoginService _loginService = null;
        private IScraper _scraper = null;
        private IAuthenticationService<int> _authService;
 


        public LoginApiController(ILoginService loginService, IAuthenticationService<int> authService, IScraper scraper,
            ILogger<LoginApiController> logger) : base(logger)
        {
            _loginService = loginService;
            _authService = authService;
            _scraper = scraper;
 
        }

       //users api ___________________________________


        [HttpPost]
        public async Task<ActionResult<ItemResponse<int>>> Insert(UserInfo model)
        {

            try
            {
                string token = Guid.NewGuid().ToString();
                int id = _loginService.Register(model);
                await _scraper.confirm(model, token);
                ItemResponse<int> response = new ItemResponse<int>();
                response.Item = id;
                return Created201(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                return StatusCode(500, new ErrorResponse(ex.Message));
            }

        }

        [HttpPost("login"), AllowAnonymous]
        public ActionResult<ItemResponse<UserInfo>> Login(LoginRequest model)
        {
            try
            {
                UserInfo data = _loginService.selectByUsername(model);

                if (data != null && data.IsConfirmed == 1)
                {
                    return StatusCode(401, new ErrorResponse("Please confirm your account"));
                }
                else
                {
                    if (data == null )
                    {
                        return StatusCode(401, new ErrorResponse("Incorrect password or username"));
                    }
                    else
                    {
                        ItemResponse<UserInfo> response = new ItemResponse<UserInfo>();
                        response.Item = data;
                        return Ok200(response);

                    }


                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                return StatusCode(500, new ErrorResponse(ex.Message));
            }
        }


        


        [HttpPost("updatepass")]
        public ActionResult<ItemResponse<UserInfo>> Update(LoginRequest model)
        {
            try
            {
                UserInfo data = _loginService.selectByUsername(model);

                if (data != null && data.IsConfirmed == 1)
                {
                    return StatusCode(401, new ErrorResponse("Please confirm your account"));
                }
                else
                {
                    if (data == null)
                    {
                        return StatusCode(401, new ErrorResponse("Incorrect password or username"));
                    }
                    else
                    {
                        int update = _loginService.updatePassword(model); 
                        ItemResponse<int> response = new ItemResponse<int>();
                        response.Item = update;
                        return Ok200(response);

                    }


                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                return StatusCode(500, new ErrorResponse(ex.Message));
            }
        }









        [HttpDelete("{id:int}")]
        public ActionResult<SuccessResponse> Delete(LoginRequest model)
        {

            try
            {
                UserInfo data = _loginService.selectByUsername(model);

                if (data != null && data.IsConfirmed == 1)
                {
                    return StatusCode(401, new ErrorResponse("Please confirm your account"));
                }
                else
                {
                    if (data == null)
                    {
                        return StatusCode(401, new ErrorResponse("Incorrect password or username"));
                    }
                    else
                    {
                        _loginService.Delete(model.UserId);
                 
                        SuccessResponse response = new SuccessResponse();
                        return Ok200(response);

                    }


                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                return StatusCode(500, new ErrorResponse(ex.Message));
            }

        }





        //////// scraper api 
        ///

        [HttpGet("web")]
        public ActionResult<ItemsResponse<EntryModel>> crawler()
        {

            try
            {
                List<EntryModel> models = _scraper.ScrapeData();

                _scraper.DeleteAll();

                List<EntryModel> withId = _scraper.Insert(models);


                ItemsResponse<EntryModel> response = new ItemsResponse<EntryModel>();
                response.Items = withId;
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                return StatusCode(500, new ErrorResponse(ex.Message));
            }

        }




        [HttpGet("scrape/all")]
        public ActionResult<ItemsResponse<EntryModel>> GetAll()
        {

            try
            {
                List<EntryModel> models = _scraper.Get();

                ItemsResponse<EntryModel> response = new ItemsResponse<EntryModel>();
                response.Items = models;
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                return StatusCode(500, new ErrorResponse(ex.Message));
            }

        }




        //// update info 


        [HttpPut("listing/update")]
        public ActionResult<SuccessResponse> Put(EntryModel model)
        {
            try
            {
                _scraper.update(model);
                SuccessResponse response = new SuccessResponse();
                return Ok200(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                return StatusCode(500, new ErrorResponse(ex.Message));
            }
        }








        //delete info 

        [HttpDelete("listing/delete/{id:int}")]
        public ActionResult<SuccessResponse> Delete(int id)
        {
            try
            {
                _scraper.DeleteById(id);
                SuccessResponse response = new SuccessResponse();
                return Ok200(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                return StatusCode(500, new ErrorResponse(ex.Message));
            }
        }





    }

}