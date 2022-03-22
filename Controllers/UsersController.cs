using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBankingAPI.Models.Requests;
using OnlineBankingAPI.Models.Responses;
using OnlineBankingAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OnlineBankingAPI.Controllers
{
    [ApiController]
    [Route("controller")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }
        public static bool IsPhoneNumber(string number)
        {
            return Regex.Match(number, @"^?([0-9]{10})$").Success;
        }
        [HttpPost("authenticate")]
        public AuthenticateResponse Authenticate(AuthenticateRequest model)
        {
            if (IsPhoneNumber(model.Phone))
            {
                var response = _userService.Authenticate(model);
                if (response == null)
                    return new AuthenticateResponse (new List<string> { "Incorrect phone number or password!" });

                return response;
            }
            else
            {
                return new AuthenticateResponse(new List<string> { "Invalid phone number!" });
            }  
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }
    }
}
