using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Application.Helper;
using Engrisk.Data;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Engrisk.Controllers.V1
{
    [Route("api/v1/[Controller]")]
    [ApiController]
    public class ExamplesController : ControllerBase
    {
        private readonly ICRUDRepo _repo;
        public ExamplesController(ICRUDRepo repo)
        {
            _repo = repo;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] SubjectParams subjectParams)
        {
            var examplesFromDb = await _repo.GetAll<Example>(subjectParams);
            Response.AddPaginationHeader(examplesFromDb.CurrentPage, examplesFromDb.PageSize, examplesFromDb.TotalItems, examplesFromDb.TotalPages);
            return Ok(examplesFromDb);
        }
        [HttpGet("{exampleId}")]
        public async Task<IActionResult> GetExample(Guid exampleId)
        {
            var example = await _repo.GetOneWithCondition<Example>(example => example.Id == exampleId);
            if (example == null)
            {
                return NotFound();
            }
            return Ok(example);
        }

        [HttpPost]
        public async Task<IActionResult> CreateExample(Example example)
        {
            var properties = new Dictionary<dynamic, dynamic>();
            properties.Add("Eng", example.Eng);
            if (_repo.Exists<Example>(properties))
            {
                return StatusCode(409);
            }
            _repo.Create(example);
            if (await _repo.SaveAll())
            {
                return CreatedAtAction("GetExample", new { id = example.Id }, example);
            }
            return BadRequest();
        }
        [HttpPost("import-json")]
        public async Task<IActionResult> ImportJson([FromForm] IFormFile file)
        {
            try
            {
                if (file.Length > 0)
                {
                    var examples = new List<Example>();
                    if (file.ContentType.ToLower().Equals("application/json"))
                    {
                        var result = await file.DeserializeJson();
                        foreach (var item in result)
                        {
                            var example = new Example()
                            {
                                Eng = (string)item["eng"],
                                Vie = (string)item["vie"],
                            };
                            if (!await _repo.Exists(example))
                            {
                                examples.Add(example);
                                _repo.Create(example);
                            }
                        }
                        if (await _repo.SaveAll())
                        {
                            return Ok(examples);
                        }
                        return NoContent();
                    }
                }
                return NoContent();
            }
            catch (System.Exception e)
            {
                throw e;
            }

        }
        [HttpPost("import-excel")]
        public async Task<IActionResult> ImportExcel([FromQuery] string sheet, [FromForm] IFormFile file)
        {
            if (file.Length > 0)
            {
                var extension = Path.GetExtension(file.FileName);
                var examples = new List<Example>();
                if (extension.Equals(".csv") || extension.Equals(".xlsx"))
                {
                    var result = await file.ReadExcel();
                    var temp = result.Tables[sheet];
                    if (temp != null)
                    {
                        foreach (DataRow row in temp.Rows)
                        {
                            var example = new Example()
                            {
                                Eng = row["Eng"] == DBNull.Value ? null : (string)row["Eng"],
                                Vie = row["Vie"] == DBNull.Value ? null : (string)row["Vie"],
                            };
                            var exampleFromDb = await _repo.GetOneWithCondition<Example>(e => e.Eng.ToLower().Trim().Equals(example.Eng.ToLower().Trim()));
                            if (exampleFromDb == null)
                            {
                                examples.Add(example);
                                _repo.Create(example);
                                await _repo.SaveAll();
                            }
                        }
                        return Ok(examples);
                    }
                }
                return BadRequest(new
                {
                    error = "File not supported"
                });
            }
            return NoContent();
        }
        [HttpPut("{exampleId}")]
        public async Task<IActionResult> UpdateExample(Guid exampleId, Example example)
        {
            var exampleFromDb = await _repo.GetOneWithCondition<Example>(example => example.Id == exampleId);
            if (exampleFromDb == null)
            {
                return NotFound();
            }
            var properties = new Dictionary<dynamic, dynamic>();
            properties.Add("Eng", example.Eng);
            if (_repo.Exists<Example>(properties))
            {
                return StatusCode(409);
            }
            _repo.Update(example);
            return Ok();
        }
        [HttpDelete("{exampleId}")]
        public async Task<IActionResult> DeleteExample(Guid exampleId)
        {
            var exampleFromDb = await _repo.GetOneWithCondition<Example>(example => example.Id == exampleId);
            if (exampleFromDb == null)
            {
                return NotFound();
            }
            return Ok();
        }
    }
}