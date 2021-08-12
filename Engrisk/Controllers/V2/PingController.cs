using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Engrisk.Controllers.V2
{
    public class PingController: BaseApiController
    {
        [HttpGet]
        public async Task<IActionResult> PingToServer(){
            return Ok();
        }
    }
}