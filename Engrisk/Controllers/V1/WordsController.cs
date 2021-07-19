using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Helper;
using AutoMapper;
using Engrisk.Data;
using Application.DTOs.Word;
using Domain.Models;
using Engrisk.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Engrisk.Controllers.V1
{
    [Route("api/v1/[Controller]")]
    [ApiController]
    public class WordsController : ControllerBase
    {
        private readonly ICRUDRepo _repo;
        private readonly IMapper _mapper;
        public WordsController(ICRUDRepo repo, IMapper mapper, IOptions<CloudinarySettings> cloudinarySettings)
        {
            _mapper = mapper;
            _repo = repo;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllWord([FromQuery] SubjectParams subjectParams)
        {
            var wordsQueryable = await _repo.GetOneWithManyToMany<Word>();
            var words = await wordsQueryable.ToListAsync();
            var returnWords = _mapper.Map<IEnumerable<WordDTO>>(words);
            return Ok(returnWords);
        }
        [HttpGet("export-json")]
        public async Task<IActionResult> Export()
        {
            var wordsFromDb = await _repo.GetAll<Word>(null, "");
            var result = JsonConvert.SerializeObject(wordsFromDb);
            await System.IO.File.WriteAllTextAsync(@"E:\words.json", result);
            return Ok();
        }
        [HttpGet("accounts/ranking")]
        public async Task<IActionResult> GetWordLearntRanking()
        {
            var wordLearnt = await _repo.GetAll<WordLearnt>(null, "Account");
            var returnRanking = wordLearnt.GroupBy(g => g.AccountId).Select(w => new WordLearnedDTO() { AccountId = w.Key, Learned = w.Count(), AccountUsername = w.FirstOrDefault().Account.UserName, AccountPhotourl = w.FirstOrDefault().Account.PhotoUrl, AccountFullname = w.FirstOrDefault().Account.Fullname }).OrderByDescending(l => l.Learned).Take(3);
            return Ok(returnRanking);
        }
        [Authorize]
        [HttpGet("accounts/{accountId}/learnt")]
        public async Task<IActionResult> GetWordLearnt(int accountId)
        {
            var learntWords = await _repo.GetAll<WordLearnt>(w => w.AccountId == accountId, "Word");
            var returnLearntWords = _mapper.Map<IEnumerable<WordLearntDTO>>(learntWords);
            return Ok(returnLearntWords);
        }
        // [HttpPost("practice")]
        // public async Task<IActionResult> Practice([FromBody] IEnumerable<Word> words)
        // {
        //     var random = new Random();
        //     List<WordPracticeDTO> questions = new List<WordPracticeDTO>();
        //     foreach (var word in words)
        //     {
        //         var type = random.Next(1, 5);
        //         var answer = random.Next(1, 5);
        //         var wordsFromDb = await _repo.GetAll<Word>(w => w.Id != word.Id, "");
        //         WordPracticeDTO q = new WordPracticeDTO();

        //         switch (type)
        //         {
        //             case 1:
        //                 try
        //                 {
        //                     List<string> splittedWord = new List<string>();
        //                     foreach (var singleWord in word.Eng.Split(new char[] { ' ' }))
        //                     {
        //                         splittedWord.Add(singleWord);
        //                     }
        //                     var examples = await _repo.GetAll<Example>(null, "");
        //                     examples = examples.Where(e => e.Eng.CompareStringPropWithList(splittedWord) == true);
        //                     q.Content = examples.GetOneRandomFromList() != null ? examples.GetOneRandomFromList().Eng.ReplaceStringPropWithList(splittedWord) : word.Vie;
        //                     q.IsQuizQuestion = true;
        //                     q.IsFillOutQuestion = examples.GetOneRandomFromList() != null ? true : false;
        //                     q.A = answer == 1 ? word.Eng : wordsFromDb.GetOneRandomFromList().Eng;
        //                     q.B = answer == 2 ? word.Eng : wordsFromDb.GetOneRandomFromList().Eng;
        //                     q.C = answer == 3 ? word.Eng : wordsFromDb.GetOneRandomFromList().Eng;
        //                     q.D = answer == 4 ? word.Eng : wordsFromDb.GetOneRandomFromList().Eng;
        //                     break;
        //                 }
        //                 catch (System.Exception e)
        //                 {
        //                     System.Console.WriteLine(word.Eng);
        //                     throw e;
        //                 }

        //             case 2:
        //                 q.PhotoUrl = word.WordImg;
        //                 q.Audio = word.WordVoice;
        //                 q.IsListeningQuestion = true;
        //                 q.IsQuizQuestion = true;
        //                 q.A = answer == 1 ? word.Vie : wordsFromDb.GetOneRandomFromList().Vie;
        //                 q.B = answer == 2 ? word.Vie : wordsFromDb.GetOneRandomFromList().Vie;
        //                 q.C = answer == 3 ? word.Vie : wordsFromDb.GetOneRandomFromList().Vie;
        //                 q.D = answer == 4 ? word.Vie : wordsFromDb.GetOneRandomFromList().Vie;
        //                 break;
        //             case 3:
        //                 var listeningQuestion = await _repo.GetAll<Question>(q => q.Answer.ToLower().Equals(word.Eng.ToLower()));
        //                 q.Audio = listeningQuestion.GetOneRandomFromList() != null ? listeningQuestion.GetOneRandomFromList().AudioFileName : word.WordVoice;
        //                 if (q.Audio == null)
        //                 {
        //                     q.PhotoUrl = word.WordImg;
        //                 }
        //                 q.IsListeningQuestion = true;
        //                 q.IsQuizQuestion = true;
        //                 q.A = answer == 1 ? word.Vie : wordsFromDb.GetOneRandomFromList().Vie;
        //                 q.B = answer == 2 ? word.Vie : wordsFromDb.GetOneRandomFromList().Vie;
        //                 q.C = answer == 3 ? word.Vie : wordsFromDb.GetOneRandomFromList().Vie;
        //                 q.D = answer == 4 ? word.Vie : wordsFromDb.GetOneRandomFromList().Vie;
        //                 break;
        //             default:
        //                 q.Content = word.Vie;
        //                 q.IsQuizQuestion = true;
        //                 q.A = answer == 1 ? word.Eng : wordsFromDb.GetOneRandomFromList().Eng;
        //                 q.B = answer == 2 ? word.Eng : wordsFromDb.GetOneRandomFromList().Eng;
        //                 q.C = answer == 3 ? word.Eng : wordsFromDb.GetOneRandomFromList().Eng;
        //                 q.D = answer == 4 ? word.Eng : wordsFromDb.GetOneRandomFromList().Eng;
        //                 break;
        //         }
        //         switch (answer)
        //         {
        //             case 1:
        //                 q.Answer = q.A;
        //                 break;
        //             case 2:
        //                 q.Answer = q.B;
        //                 break;
        //             case 3:
        //                 q.Answer = q.C;
        //                 break;
        //             case 4:
        //                 q.Answer = q.D;
        //                 break;
        //             default:
        //                 break;
        //         }
        //         questions.Add(q);
        //     }
        //     return Ok(questions.Shuffle());
        // }
        [Authorize]
        [HttpPost("practice/done")]
        public async Task<IActionResult> PracticeDone([FromBody] IEnumerable<Word> words)
        {
            foreach (var word in words)
            {
                var wordLearntFromDb = await _repo.GetOneWithConditionTracking<WordLearnt>(w => w.WordId == word.Id);
                if (wordLearntFromDb == null)
                {
                    var wordLearn = new WordLearnt()
                    {
                        WordId = word.Id,
                        AccountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value),
                    };
                    _repo.Create(wordLearn);
                }
            }
            if (await _repo.SaveAll())
            {
                return Ok();
            }
            return NoContent();
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWord(Guid id)
        {
            var wordsQueryable = await _repo.GetOneWithManyToMany<Word>(w => w.Id == id);
            var word = await wordsQueryable.FirstOrDefaultAsync();
            if (word == null)
            {
                return NotFound();
            }
            var returnWord = _mapper.Map<WordDetailDTO>(word);
            return Ok(returnWord);
        }
        [HttpGet("search/{word}")]
        public async Task<IActionResult> SearchWord([FromQuery] SubjectParams subjectParams, string word)
        {
            var words = await _repo.GetAll<Word>(subjectParams, w => w.Eng.Contains(word) || w.Vie.Contains(word));
            return Ok(words);
        }
        [HttpGet("detail")]
        public async Task<IActionResult> GetWord([FromQuery] string search)
        {
            var wordsQueryable = await _repo.GetOneWithManyToMany<Word>(w => w.Eng.ToLower().Equals(search.ToLower()) || w.Vie.ToLower().Equals(search.ToLower()));
            var word = await wordsQueryable.FirstOrDefaultAsync();
            if (word == null)
            {
                return NotFound();
            }
            var returnWord = _mapper.Map<WordDetailDTO>(word);
            if (returnWord.Eng.ToLower().Equals(search.ToLower()))
            {
                return Ok(new
                {
                    direction = "en",
                    word = returnWord
                });
            }
            else
            {
                return Ok(new
                {
                    direction = "vi",
                    word = returnWord
                });
            }
        }
        [HttpGet("examples")]
        public async Task<IActionResult> GetExamples([FromBody] Word word)
        {
            var examplesFromDb = await _repo.GetAll<Example>(e => e.Eng.ToLower().Contains(word.Eng.ToLower()));
            return Ok(examplesFromDb);
        }
        [HttpPost]
        public async Task<IActionResult> CreateWord([FromForm] WordCreateDTO wordDTO)
        {
            var properties = new Dictionary<dynamic, dynamic>();
            if (string.IsNullOrEmpty(wordDTO.Eng))
            {
                return BadRequest(new
                {
                    Error = "Không được để trống fields"
                });
            }
            properties.Add("Eng", wordDTO.Eng);
            if (_repo.Exists<Word>(properties))
            {
                return Conflict(new
                {
                    Error = "Từ vựng bị trùng"
                });
            }
            var word = _mapper.Map<Word>(wordDTO);
            _repo.Create(word);
            if (await _repo.SaveAll())
            {
                return CreatedAtAction("GetWord", new { id = word.Id }, word);
            }
            return BadRequest("Error on creating word");
        }
        [HttpPost("import-json")]
        public async Task<IActionResult> ImportFromJson([FromForm] IFormFile file)
        {
            try
            {
                if (file.Length > 0)
                {
                    if (file.ContentType.ToLower().Equals("application/json"))
                    {
                        var words = new List<Word>();
                        var result = await file.DeserializeJson();
                        foreach (var jToken in result)
                        {
                            var word = new Word()
                            {
                                WordImg = (string)jToken["wordImg"],
                                Eng = (string)jToken["eng"],
                                Vie = (string)jToken["vie"],
                                Spelling = (string)jToken["spelling"],
                                WordVoice = (string)jToken["wordVoice"]
                            };
                            var wordFromDb = await _repo.GetOneWithCondition<Word>(w => w.Eng.ToLower().Trim().Equals(word.Eng.ToLower().Trim()));
                            if (wordFromDb == null)
                            {
                                words.Add(word);
                                _repo.Create(word);
                            }
                        }
                        if (await _repo.SaveAll())
                        {
                            return Ok(words);
                        }
                        else
                        {
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
                if (extension.Equals(".csv") || extension.Equals(".xlsx"))
                {
                    var result = await file.ReadExcel();
                    if (result != null)
                    {
                        var words = new List<Word>();
                        var temp = result.Tables[sheet];
                        if (temp != null)
                        {
                            foreach (DataRow row in temp.Rows)
                            {
                                var word = new Word()
                                {
                                    Eng = row["Eng"] == DBNull.Value ? null : (string)row["Eng"],
                                    Vie = row["Vie"] == DBNull.Value ? null : (string)row["Vie"],
                                    Spelling = row["Spelling"] == DBNull.Value ? null : (string)row["Spelling"],
                                    WordImg = row["WordImg"] == DBNull.Value ? null : (string)row["WordImg"],
                                };
                                var wordFromDb = await _repo.GetOneWithCondition<Word>(w => w.Eng.ToLower().Trim().Equals(word.Eng.ToLower().Trim()));
                                if (wordFromDb == null)
                                {
                                    words.Add(word);
                                    _repo.Create(word);
                                    await _repo.SaveAll();
                                }
                            }
                            return Ok(words);
                        }
                    }
                }
                return BadRequest(new
                {
                    error = "File not supported"
                });
            }
            return NoContent();
        }

        // [HttpPost("examples/import-excel")]
        // public async Task<IActionResult> ImportExamplesExcel([FromQuery] string sheet, [FromForm] IFormFile file)
        // {
        //     if (file.Length > 0)
        //     {
        //         var extension = Path.GetExtension(file.FileName);
        //         var wordExamples = new List<WordExample>();
        //         if (extension.Equals(".csv") || extension.Equals(".xlsx"))
        //         {
        //             var result = await file.ReadExcel();
        //             var temp = result.Tables[sheet];
        //             if (temp != null)
        //             {
        //                 foreach (DataRow row in temp.Rows)
        //                 {
        //                     var wordExample = new WordExample()
        //                     {
        //                         WordId = row["WordId"] == DBNull.Value ? Guid.Parse("") : Guid.Parse((string)row["WordId"]),
        //                         ExampleId = row["ExampleId"] == DBNull.Value ? Guid.Parse("") : Guid.Parse((string)row["ExampleId"]),
        //                     };
        //                     var wordexampleFromDb = await _repo.GetOneWithCondition<WordExample>(e => e.WordId == wordExample.WordId && e.ExampleId == wordExample.ExampleId);
        //                     if (wordexampleFromDb == null)
        //                     {
        //                         wordExamples.Add(wordExample);
        //                         _repo.Create(wordExample);
        //                         await _repo.SaveAll();
        //                     }
        //                 }
        //                 return Ok(wordExamples);
        //             }
        //         }
        //         return BadRequest(new
        //         {
        //             error = "File not supported"
        //         });
        //     }
        //     return NoContent();
        // }
        [HttpPut("{wordId}")]
        public async Task<IActionResult> UpdateWord(Guid wordId, [FromForm] WordCreateDTO word)
        {
            try
            {
                var wordFromDb = await _repo.GetOneWithConditionTracking<Word>(word => word.Id == wordId);
                if (wordFromDb == null)
                {
                    return NotFound();
                }
                _mapper.Map(word, wordFromDb);
                if (await _repo.SaveAll())
                {
                    return Ok();
                }
                return NoContent();
            }
            catch (System.Exception e)
            {

                throw e;
            }
        }
        // [HttpPost("{wordId}/examples")]
        // public async Task<IActionResult> AddWordExample(Guid wordId, [FromBody] Example example)
        // {
        //     var wordFromDb = await _repo.GetOneWithCondition<Word>(word => word.Id == wordId);
        //     var exampleFromDb = await _repo.GetOneWithCondition<Example>(example => example.Eng.TrimStart().TrimEnd().Equals(example.Eng.TrimStart().TrimEnd()) || example.Vie.TrimStart().TrimEnd().Equals(example.Vie.TrimStart().TrimEnd()));
        //     if (wordFromDb == null)
        //     {
        //         return NotFound();
        //     }
        //     if (exampleFromDb == null)
        //     {
        //         var newExample = new Example()
        //         {
        //             Eng = example.Eng,
        //             Vie = example.Vie,
        //         };
        //         _repo.Create(newExample);
        //         var wordExample = new WordExample()
        //         {
        //             WordId = wordId,
        //             ExampleId = newExample.Id
        //         };
        //         _repo.Create(wordExample);
        //     }
        //     else
        //     {
        //         var wordExample = new WordExample()
        //         {
        //             WordId = wordId,
        //             ExampleId = exampleFromDb.Id
        //         };
        //         _repo.Create(wordExample);
        //     }

        //     if (await _repo.SaveAll())
        //     {
        //         return Ok();
        //     }
        //     return BadRequest("Error on adding example");
        // }
        [HttpPut("{wordId}/examples/{exampleId}")]
        public async Task<IActionResult> UpdateExample(Guid wordId, Guid exampleId, [FromBody] Example example)
        {
            var wordFromDb = await _repo.GetOneWithCondition<Word>(word => word.Id == wordId);
            if (wordFromDb == null)
            {
                return NotFound(new
                {
                    NotFound = "Không tìm thấy từ"
                });
            }
            var properties = new Dictionary<dynamic, dynamic>();
            properties.Add("Eng", example.Eng);
            if (_repo.Exists<Example>(properties))
            {
                return Conflict();
            }
            var exampleFromDb = await _repo.GetOneWithCondition<Example>(e => e.Id == exampleId);
            if (exampleFromDb == null)
            {
                return NotFound(new
                {
                    NotFound = "Không tìm thấy ví dụ"
                });
            }
            _mapper.Map(example, exampleFromDb);
            var temp = exampleFromDb;
            if (await _repo.SaveAll())
            {
                return Ok();
            }
            return StatusCode(500);
        }
        [HttpDelete("{wordId}/examples/{exampleId}")]
        public async Task<IActionResult> DeleteExample(Guid wordId, Guid exampleId)
        {
            var wordFromDb = await _repo.GetOneWithCondition<Word>(word => word.Id == wordId);
            if (wordFromDb == null)
            {
                return NotFound();
            }
            var exampleFromDb = await _repo.GetOneWithCondition<Example>(e => e.Id == exampleId);
            if (exampleFromDb == null)
            {
                return NotFound(new
                {
                    NotFound = "Không tìm thấy ví dụ"
                });
            }
            _repo.Delete(exampleFromDb);
            if (await _repo.SaveAll())
            {
                return NoContent();
            }
            return StatusCode(500);
        }
        [HttpDelete("{wordId}")]
        public async Task<IActionResult> DeleteWord(Guid wordId)
        {
            var wordFromDb = await _repo.GetOneWithCondition<Word>(word => word.Id == wordId);
            if (wordFromDb == null)
            {
                return NotFound();
            }
            _repo.Delete(wordFromDb);
            if (await _repo.SaveAll())
            {
                return NoContent();
            }
            return BadRequest(new { Error = "Error on deleting word" });
        }
    }
}