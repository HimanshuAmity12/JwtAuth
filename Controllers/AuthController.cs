using JwtAuth.Data;
using JwtAuth.Models;
using JwtAuth.Services;
using JwtAuthApi.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JwtAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly JwtService _jwtService;
        private readonly PasswordHasher<AppUser> _passwordHasher;
        public AuthController(AppDbContext db, JwtService jwtService)
        {
            _db = db;
            _jwtService = jwtService;
            _passwordHasher = new PasswordHasher<AppUser>();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var existingUser = await _db.Users.AnyAsync(u => u.Email == email);
            if (existingUser)
            {
                return Conflict(new
                {
                    message = "Email is already registered."
                });
            }

            var user = new AppUser
            {
                Name = request.Name.Trim(),
                Email = email,
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();

            return Ok(new 
            {
                message = "Registration Successfull."
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

            if(user == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }
            
            var result  = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if(result == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            var token = _jwtService.GenerateToken(user);

            return Ok(new LoginResponse
            {
                Token = token.Result,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30)
            });
        }
    }
}
