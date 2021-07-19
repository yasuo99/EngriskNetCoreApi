using System.Threading.Tasks;
using Application.Helper;
using Engrisk.Data;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Engrisk.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[Controller]")]
    public class RedeemScoresController : ControllerBase
    {
        private readonly ICRUDRepo _repo;
        public RedeemScoresController(ICRUDRepo repo)
        {
            _repo = repo;
        }
        [HttpGet("listening")]
        public async Task<IActionResult> GetAllListeningRedeem([FromQuery]SubjectParams subjectParams){
            var listeningToeicRedeems = await _repo.GetAll<ListeningToeicRedeem>(subjectParams);
            return Ok(listeningToeicRedeems);
        }
        [HttpGet("listening/{id}")]
        public async Task<IActionResult> GetDetailListeningRedeem(int id){
            var listeningToeicRedeemFromDb = await _repo.GetOneWithCondition<ListeningToeicRedeem>(l => l.Id == id);
            if(listeningToeicRedeemFromDb == null){
                return NotFound();
            }
            return Ok(listeningToeicRedeemFromDb);
        }
        [HttpGet("reading")]
        public async Task<IActionResult> GetAllReadingRedeem(SubjectParams subjectParams){
            var readingToeicRedeems = await _repo.GetAll<ReadingToeicRedeem>(subjectParams);
            return Ok(readingToeicRedeems);
        }
        [HttpPost("listening")]
        public async Task<IActionResult> AddListeningRedeem(ListeningToeicRedeem listening){
            var listeningToeicRedeemFromDb = await _repo.GetOneWithConditionTracking<ListeningToeicRedeem>(l => l.NumOfSentences == listening.NumOfSentences);
            if(listeningToeicRedeemFromDb != null){
                listeningToeicRedeemFromDb.Score = listening.Score;
                if(await _repo.SaveAll()){
                    return Ok(new {
                        updated = "Updated listening:  num of sentences " +listening.NumOfSentences + "with score " + listening.Score
                    });
                }
            }
            _repo.Create(listening);
            if(await _repo.SaveAll()){
                return CreatedAtAction("GetDetailListeningRedeem",new {id = listening.Id}, listening);
            }
            return StatusCode(500);
        }
        [HttpPost("reading")]
        public async Task<IActionResult> AddReadingRedeem(ReadingToeicRedeem reading){
            var readingToeicRedeemFromDb = await _repo.GetOneWithConditionTracking<ReadingToeicRedeem>(l => l.NumOfSentences == reading.NumOfSentences);
            if(readingToeicRedeemFromDb != null){
                readingToeicRedeemFromDb.Score = reading.Score;
                if(await _repo.SaveAll()){
                    return Ok(new {
                        updated = "Updated reading:  num of sentences " +reading.NumOfSentences + "with score " + reading.Score
                    });
                }
            }
            _repo.Create(reading);
            if(await _repo.SaveAll()){
                return CreatedAtAction("GetDetailReadingRedeem",new {id = reading.Id}, reading);
            }
            return StatusCode(500);
        }
    }
}