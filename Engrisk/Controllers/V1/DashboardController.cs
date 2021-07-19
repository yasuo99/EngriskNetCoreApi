using Engrisk.Data;
using Microsoft.AspNetCore.Mvc;

namespace Engrisk.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[Controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ICRUDRepo _repo;
        public DashboardController(ICRUDRepo repo)
        {
            _repo = repo;
        }
    }
}