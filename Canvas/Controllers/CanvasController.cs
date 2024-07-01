using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Google.Apis.Auth;

namespace Canvas.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CanvasController : ControllerBase
    {

        private readonly ILogger<CanvasController> _logger;
        private CanvasOptions _canvasOptions;
        private DatabaseOperations _databaseOperations;

        public CanvasController(ILogger<CanvasController> logger, IOptions<CanvasOptions> canvasOptions, DatabaseOperations databaseOperations)
        {
            _logger = logger;
            _canvasOptions = canvasOptions.Value;
            _databaseOperations = databaseOperations;
        }

        [HttpGet("GetCanvas")]
        [ResponseCache(Duration = 1 )]
        public async Task<IActionResult> GetCanvas()
        {
            int height = _canvasOptions.Height;
            int width = _canvasOptions.Width;
            Pixel[,] pixels = await _databaseOperations.GetAllPixels(height, width);
            Canvas canvas = new Canvas(width, height, pixels);
            var serilized = JsonConvert.SerializeObject(canvas);
            return Content(serilized);
        }

        [HttpGet("ChangePixel")]
        public async Task<IActionResult> ChangePixel(string sessionId, int x, int y, int green, int red, int blue)
        {
            if (x < 0 || y < 0 || x > _canvasOptions.Width || y > _canvasOptions.Height) return BadRequest();
            if (!(await _databaseOperations.IsValidSession(sessionId))) return BadRequest();
            if ((await _databaseOperations.GetRemainingTime(sessionId)) > 0) return BadRequest();
            await _databaseOperations.ReplacePixel(x, y, red, green, blue, sessionId);
            return Content("success");
        }

        [HttpGet("Login")]
        public async Task<IActionResult> Login(string googleToken)
        {
            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(googleToken);
            if (payload == null) return BadRequest();
            var organization = payload.HostedDomain;
            if (organization != "taded.org.il" && organization != "bengurion.org")
            {
                return BadRequest();
            }
            var userId = payload.Subject;
            if (await _databaseOperations.DoesUserExist(userId))
            {
                var sessionId = await _databaseOperations.Login(userId);
                return Content(sessionId);
            }
            else
            {
                var userName = payload.Name;
                var sessionId = await _databaseOperations.Register(userId, userName);
                return Content(sessionId);
            }
        }

        [HttpGet("Logout")]
        public async Task<IActionResult> Logout(string sessionId)
        {
            if (!(await _databaseOperations.IsValidSession(sessionId))) return BadRequest();
            await _databaseOperations.Logout(sessionId);
            return Content("logout success");
        }

        [HttpGet("GetRemainingTime")]
        public async Task<IActionResult> GetRemainingTime(string sessionId)
        {
            if (!(await _databaseOperations.IsValidSession(sessionId))) return BadRequest();
            return Content((await _databaseOperations.GetRemainingTime(sessionId)).ToString());
        }

        [HttpGet("CheckSessionId")]
        public async Task<IActionResult> CheckSessionId(string sessionId)
        {
            var isValid = await _databaseOperations.IsValidSession(sessionId);
            return Content(isValid.ToString());
        }
    }
}
