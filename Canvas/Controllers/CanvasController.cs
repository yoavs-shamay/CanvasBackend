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
    public class CanvasController : ControllerBase //TODO 1 second caching?
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
        public IActionResult GetCanvas()
        {
            int height = _canvasOptions.Height;
            int width = _canvasOptions.Width;
            Pixel[,] pixels = _databaseOperations.GetAllPixels(height, width);
            Canvas canvas = new Canvas(width, height, pixels);
            var serilized = JsonConvert.SerializeObject(canvas);
            return Content(serilized);
        }

        [HttpGet("ChangePixel")]
        public IActionResult ChangePixel(string sessionId, int x, int y, int green, int red, int blue) //TODO all async
        {
            if (x < 0 || y < 0 || x > _canvasOptions.Width || y > _canvasOptions.Height) return BadRequest();
            if (!_databaseOperations.IsValidSession(sessionId)) return BadRequest();
            if (_databaseOperations.GetRemainingTime(sessionId) > 0) return BadRequest();
            _databaseOperations.ReplacePixel(x, y, red, green, blue, sessionId);
            return Content("success");
        }

        [HttpGet("Login")]
        public async Task<IActionResult> Login(string googleToken)
        {
            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(googleToken);
            if (payload == null) return BadRequest();
            var organization = payload.HostedDomain;
            if (organization != "taded.org.il")
            {
                return BadRequest();
            }
            var userId = payload.Subject;
            if (_databaseOperations.DoesUserExist(userId))
            {
                var sessionId = _databaseOperations.Login(userId);
                return Content(sessionId);
            }
            else
            {
                var userName = payload.Name;
                var sessionId = _databaseOperations.Register(userId, userName);
                return Content(sessionId);
            }
        }

        [HttpGet("Logout")]
        public IActionResult Logout(string sessionId)
        {
            if (!_databaseOperations.IsValidSession(sessionId)) return BadRequest();
            _databaseOperations.Logout(sessionId);
            return Content("logout success");
        }

        [HttpGet("GetRemainingTime")]
        public IActionResult GetRemainingTime(string sessionId)
        {
            if (!_databaseOperations.IsValidSession(sessionId)) return BadRequest();
            return Content(_databaseOperations.GetRemainingTime(sessionId).ToString());
        }
    }
}
