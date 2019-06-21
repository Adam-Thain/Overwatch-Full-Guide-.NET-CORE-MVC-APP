using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OW_Full_Guide_NetCore_MVC.Web.Server.Authentication;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace OW_Full_Guide_NetCore_MVC.Web.Server.Controllers
{
    /// <summary>
    /// Manages the Web API calls
    /// </summary>
    public class ApiController : Controller
    {
        /// <summary>
        /// Logs in a user using token-based authentication
        /// </summary>
        /// <returns></returns>
        [Route("api/login")]
        public IActionResult LogIn()
        {
            // TODO: Get users login information and check it is correct

            // For now set username
            var username = "uponia";

            var email = "adam.r.thain@gmail.com";

            // Set our tokens claims
            var claims = new[]
            {
                // Unique ID for this token
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimsIdentity.DefaultNameClaimType, "uponia"),
                new Claim("my key","my value"),
            };

            // Create the credentials used to generate the token
            var credentials = new SigningCredentials(
                // Get the secret key from configuration
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(IoCContainer.Configuration["Jwt:SecretKey"])),
                // Use HS256 algorithm
                SecurityAlgorithms.HmacSha256);

            // Generate the Jwt Token
            var token = new JwtSecurityToken(
                issuer: IoCContainer.Configuration["Jwt:Issuer"],
                audience: IoCContainer.Configuration["Jwt:Audience"],
                claims: claims,
                signingCredentials: credentials,
                // Expire if not used for 3 months
                expires: DateTime.Now.AddMonths(3)
                );

            // Return token to user
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }

        /// <summary>
        /// Test private area for token-based authentication
        /// </summary>
        /// <returns></returns>
        [AuthorizeToken]
        [Route("api/private")]
        public IActionResult Private()
        {
            // Get the authenticated user
            var user = HttpContext.User;

            // Tell them a secret
            return Ok(new { privateData = $"some secret for {user.Identity.Name}" });
        }
    }
}