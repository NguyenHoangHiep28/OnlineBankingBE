using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineBankingAPI.Models;
using OnlineBankingAPI.Models.DTOs;
using OnlineBankingAPI.Models.DTOs.Requests;
using OnlineBankingAPI.Models.Requests;
using OnlineBankingAPI.Models.Responses;
using OnlineBankingAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
        [Route("accounts")]
        [Authorize]
        [HttpPost]
        public List<AccountDTO> GetAccountList(UserIdRequest userId)
        {
            var accountList = _userService.GetAccounts(userId);
            if(accountList != null)
            {
                return accountList;
            }
            return null;
        }
        [Route("myaccount")]
        [Authorize]
        [HttpPost]
        public IActionResult GetAccount(AccountNumberRequest accountNumber)
        {
            var accountDTO = _userService.GetAccountById(accountNumber);
            if (accountDTO != null)
            {
                return Ok(accountDTO);
            }
            return NotFound("Cannot found account with number : " + accountNumber.AccountNumber);
        }

        [Route("dashboard")]
        [Authorize]
        [HttpPost]
        public IActionResult GetAccount(DashboardInfoRequest dashboardInfo)
        {
            var dashboardInfoRes = _userService.GetDashboardInfo(dashboardInfo);
            if(dashboardInfoRes != null)
            {
                return Ok(dashboardInfoRes);
            }
            return BadRequest("Cannot found you account! Please contact us to have support.");
        }
        [Route("lock-account")]
        [Authorize]
        [HttpPost]
        public IActionResult LockAccount(AccountNumberRequest accNumberRequest)
        {
            bool accountLockSuccess = _userService.LockAccount(accNumberRequest.AccountNumber);
            if(accountLockSuccess)
            {
                return Ok(new { message = $"Account {accNumberRequest.AccountNumber} locked successfully!" });
            }else
            {
                return NotFound(new { message = $"Cannot found available account for locking. Please contact us to get futher support!" });
            }
        }
    }
}
