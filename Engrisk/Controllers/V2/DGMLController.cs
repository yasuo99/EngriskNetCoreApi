using System.IO;
using Microsoft.AspNetCore.Mvc;
using Persistence;
using EntityFrameworkCore.Diagrams;
namespace Engrisk.Controllers.V2
{
    public class DGMLController: BaseApiController
    {
        private ApplicationDbContext _context { get; set; }
        public DGMLController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}