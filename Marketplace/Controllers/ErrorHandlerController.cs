using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class ErrorHandlerController : ControllerBase
    {
        [Route("error")]
        public IActionResult HandleError() => Problem();

        [HttpGet("throw")]
        public IActionResult ThrowException()
        {
            throw new InvalidOperationException("This is a test exception to check exception pages.");
        }
    }
}
