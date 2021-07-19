using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Helper;
using AutoMapper;
using Domain.Models;
using Engrisk.Data;
using Microsoft.AspNetCore.Mvc;

namespace Engrisk.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[Controller]")]
    public class BadgesController : ControllerBase
    {
        private readonly ICRUDRepo _repo;
        private readonly IMapper _mapper;
        public BadgesController(ICRUDRepo repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] SubjectParams subjectParams){
            var badgesFromDb = await _repo.GetAll<Badge>(subjectParams);
            return Ok(badgesFromDb);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(Guid id){
            var badgeFromDb = await _repo.GetOneWithCondition<Badge>(badge => badge.Id == id);
            if(badgeFromDb == null){
                return NotFound();
            }
            return Ok(badgeFromDb);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Badge badge){
            var properties = new Dictionary<dynamic,dynamic>();
            properties.Add("BadgeLogo",badge.BadgeName);
            return Ok();
        }
    }
}