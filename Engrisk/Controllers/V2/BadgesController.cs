namespace Engrisk.Controllers.V2
{
    public class BadgesController : BaseApiController
    {
        // public BadgesController(IRepositoryWrapper repo) :
        // {
        // }

        // [HttpGet]
        // public async Task<IActionResult> GetAllBadges([FromQuery] PaginationDTO pagination)
        // {
        //     return Ok(await Repo.Badge.FindAllAsync());
        // }
        // [HttpGet("{id}")]
        // public async Task<IActionResult> GetBadge(Guid id)
        // {
        //     if (!await Repo.Badge.ExistAsync(id))
        //     {
        //         return NotFound();
        //     }
        //     return Ok(await Repo.Badge.FindOneAsync(b => b.Id == id));
        // }
        // [HttpGet("accounts")]
        // public async Task<IActionResult> GetAccountBadge([FromQuery] int accountId)
        // {
        //     return Ok();
        // }
        // [HttpPost]
        // public async Task<IActionResult> CreateBadge([FromBody] Badge badge)
        // {
        //     try
        //     {
        //         if (await Repo.Badge.ExistAsync(badge))
        //         {
        //             return Conflict();
        //         }
        //         Repo.Badge.Create(badge);
        //         if (await Repo.SaveAsync())
        //         {
        //             return CreatedAtAction("GetBadge", new { id = badge.Id }, badge);
        //         }
        //         return NoContent();
        //     }
        //     catch (System.Exception ex)
        //     {
        //         return BadRequest(new
        //     {
        //             Error = ex.Message
        //         });
        //     }

        // }
        // [HttpPut("{id}")]
        // public async Task<IActionResult> UpdateBadge(Guid id, [FromBody] Badge badge)
        // {
        //     try
        //     {
        //         if(id != badge.Id){
        //             return NotFound();
        //         }
        //         if(await Repo.Badge.ExistAsync(badge)){
        //             return Conflict();
        //         }

        //         return Ok();
        //     }
        //     catch (System.Exception ex)
        //     {
        //          // TODO
        //          throw ex;
        //     }
        //     return Ok();
        // }
        // [HttpDelete("{id}")]
        // public async Task<IActionResult> DeleteBadge(Guid id)
        // {
        //     if (!await Repo.Badge.ExistAsync(id))
        //     {
        //         return NotFound();
        //     }
        //     await Repo.Badge.DeleteBadgeAsync(id);
        //     if (await Repo.SaveAsync())
        //     {
        //         return Ok();
        //     }
        //     return NoContent();
        // }
    }
}