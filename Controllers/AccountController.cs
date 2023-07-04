using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dotnet6.Jwt.Dtos;
using Dotnet6.Jwt.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Dotnet6.Jwt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> GetLoginToken(LoginDto login)
        {
            var result = await _accountService.GetAuthToken(login);
            if (result == null)
            {
                return ValidationProblem("Invalid Credentials");
            }

            return Ok(result);
            {
                
            }
        }

        [HttpGet]
        [Route("auth-test")]
        [Authorize]
        public IActionResult GetTest()
        {
            return Ok("Only for Auhtnticated user");
        }
        
        [HttpPost]
        [Route("renew-token")]
        public async Task<IActionResult> RenewTokens(RefreshTokenDto refreshToken)
        {
            var tokens = await _accountService.RenewTokens(refreshToken);

            if (tokens == null)
            {
                return ValidationProblem("Invalid Refresh Token");
            }

            return Ok(tokens);
        }
    }
}
