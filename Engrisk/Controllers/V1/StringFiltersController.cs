using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Application.Helper;
using AutoMapper;
using Engrisk.Data;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Engrisk.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[Controller]")]
    public class StringFiltersController : ControllerBase
    {
        private readonly ICRUDRepo _repo;
        private readonly IMapper _mapper;
        public StringFiltersController(ICRUDRepo repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery]SubjectParams subjectParams)
        {
            var stringFilters = await _repo.GetAll<StringFilter>(subjectParams);
            return Ok(stringFilters);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var stringFilterFromDb = await _repo.GetOneWithCondition<StringFilter>(sf => sf.Id == id);
            if (stringFilterFromDb == null)
            {
                return NotFound();
            }
            return Ok(stringFilterFromDb);
        }
        [HttpPost]
        public async Task<IActionResult> CreateStringFilter([FromBody] StringFilter stringFilter)
        {
            var properties = new Dictionary<dynamic, dynamic>();
            properties.Add("Word", stringFilter.Word);
            if (_repo.Exists<StringFilter>(properties))
            {
                return Conflict();
            }
            _repo.Create(stringFilter);
            if (await _repo.SaveAll())
            {
                return CreatedAtAction("GetDetail", new { id = stringFilter.Id }, stringFilter);
            }
            return StatusCode(500);
        }
        [HttpPost("import-json")]
        public async Task<IActionResult> ImportJson([FromForm] IFormFile file)
        {
            if (file.Length > 0)
            {
                if (file.ContentType.Equals("application/json"))
                {
                    var filters = new List<StringFilter>();
                    var result = await file.DeserializeJson();
                    foreach (var item in result)
                    {
                        var filter = new StringFilter()
                        {
                            Word = (string)item["word"],
                            Inserted = DateTime.Now
                        };
                        var filterFromDb = await _repo.GetOneWithCondition<StringFilter>(f => f.Word.ToLower().Trim().Equals(filter.Word.ToLower().Trim()));
                        if (filterFromDb == null)
                        {
                            filters.Add(filter);
                            _repo.Create(filter);
                        }
                        if (await _repo.SaveAll())
                        {
                            return Ok(filters);
                        }
                        return NoContent();
                    }
                }
                return BadRequest(new
                {
                    error = "File not supported"
                });
            }
            return NoContent();
        }
        [HttpPost("import-excel/{sheet}")]
        public async Task<IActionResult> ImportExcel([FromForm] IFormFile file, string sheet)
        {
            if (file.Length > 0)
            {
                var extension = Path.GetExtension(file.FileName);
                if (extension.Equals(".csv") || extension.Equals(".xlsx"))
                {
                    var filters = new List<StringFilter>();
                    var result = await file.ReadExcel();
                    var temp = result.Tables[sheet];
                    if (temp != null)
                    {
                        foreach (DataRow row in temp.Rows)
                        {
                            var filter = new StringFilter()
                            {
                                Word = row["Word"] == DBNull.Value ? null : (string)row["Word"],
                                Inserted = DateTime.Now
                            };
                            var filterFromDb = await _repo.GetOneWithCondition<StringFilter>(f => f.Word.ToLower().Trim().Equals(filter.Word.ToLower().Trim()));
                            if (filterFromDb == null)
                            {
                                filters.Add(filter);
                                _repo.Create(filter);
                            }
                            if (await _repo.SaveAll())
                            {
                                return Ok(filters);
                            }
                            return NoContent();
                        }
                    }
                }
                return BadRequest(new {
                    error = "File not supported"
                });
            }
            return NoContent();
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStringFilter(int id, [FromBody] StringFilter stringFilter)
        {
            var stringFilterFromDb = await _repo.GetOneWithConditionTracking<StringFilter>(filter => filter.Id == id);
            if (stringFilterFromDb == null)
            {
                return NotFound();
            }
            var properties = new Dictionary<dynamic, dynamic>();
            properties.Add("Word", stringFilter.Word);
            if (_repo.Exists<StringFilter>(properties))
            {
                return Conflict();
            }
            _mapper.Map(stringFilter, stringFilterFromDb);
            if (await _repo.SaveAll())
            {
                return Ok();
            }
            return StatusCode(500);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFilter(int id)
        {
            var stringFilterFromDb = await _repo.GetOneWithCondition<StringFilter>(filter => filter.Id == id);
            if (stringFilterFromDb == null)
            {
                return NotFound();
            }
            _repo.Delete(stringFilterFromDb);
            if (await _repo.SaveAll())
            {
                return NoContent();
            }
            return StatusCode(500);
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteFilters()
        {
            var stringFiltersFromDb = await _repo.GetAll<StringFilter>(null, "");
            _repo.Delete(stringFiltersFromDb);
            if (await _repo.SaveAll())
            {
                return NoContent();
            }
            return StatusCode(500);
        }
    }
}