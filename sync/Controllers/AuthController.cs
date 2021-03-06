using System;
using System.Threading.Tasks;
using GenshinSchedule.SyncServer.Database;
using GenshinSchedule.SyncServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenshinSchedule.SyncServer.Controllers
{
    [ApiController, Route("auth")]
    public class AuthController : ControllerBase
    {
        readonly SyncDbContext _db;
        readonly HashHelper _hash;
        readonly AuthHelper _auth;
        readonly ILogger<AuthController> _logger;

        public AuthController(SyncDbContext db, HashHelper hash, AuthHelper auth, ILogger<AuthController> logger)
        {
            _db     = db;
            _hash   = hash;
            _auth   = auth;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<AuthResponse>> AuthAsync(AuthRequest request)
        {
            try
            {
                var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == request.Username);

                if (user == null)
                {
                    user = new DbUser
                    {
                        Username    = request.Username,
                        Password    = _hash.Hash(request.Password),
                        CreatedTime = DateTime.UtcNow,

                        WebData = new DbWebData
                        {
                            Token = Guid.NewGuid(),
                            Data  = "{}"
                        }
                    };

                    _db.Add(user);

                    await _db.SaveChangesAsync();

                    _logger.LogInformation($"Created user '{request.Username}'.");
                }
                else
                {
                    if (!_hash.Test(user.Password, request.Password))
                        return Unauthorized("Invalid username or password.");
                }

                return Ok(new AuthResponse
                {
                    Token = _auth.CreateToken(user),
                    User  = Models.User.FromDbModel(user)
                });
            }
            catch (Exception e)
            {
                var message = $"Could not authenticate user '{request.Username}'.";

                _logger.LogWarning(e, message);

                return BadRequest(message);
            }
        }
    }
}