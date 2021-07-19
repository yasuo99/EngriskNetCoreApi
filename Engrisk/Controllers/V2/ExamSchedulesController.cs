using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.ExamSchedule;
using Application.Services.Core.Abstraction;
using Microsoft.AspNetCore.Mvc;

namespace Engrisk.Controllers.V2
{
    public class ExamSchedulesController : BaseApiController
    {
        private IExamOnlineScheduleService _examOnlineScheduleService;
        public ExamSchedulesController(IExamOnlineScheduleService examOnlineScheduleService)
        {
            _examOnlineScheduleService = examOnlineScheduleService;
        }
        // [HttpGet]
        // public async Task<IActionResult> GetAll([FromQuery] PaginationDTO pagination){
        //     PagingList<ExamOnlineSchedule> examSchedules = await Repo.ExamSchedule.FindAllAsync(pagination: pagination);
        //     return Ok(examSchedules.CreatePaginate());
        // }
        // [HttpGet("{id}")]
        // public async Task<IActionResult> GetDetail(Guid id){
        //     var examSchedule = await Repo.ExamSchedule.FindOneAsync(es => es.Id == id);
        //     if(examSchedule == null){
        //         return NotFound();
        //     }
        //     return Ok(examSchedule);
        // }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateExamScheduleDTO examOnlineSchedule){
            if(!ModelState.IsValid){
                return BadRequest(ModelState.Values.SelectMany(it => it.Errors).Select(it => it.ErrorMessage));
            }
            var examSchedule = await _examOnlineScheduleService.CreateExamScheduleAsync(examOnlineSchedule);
            if(examSchedule != null)
            {
                return Ok(examSchedule);
            }
            return NoContent();
        }
    }
}
