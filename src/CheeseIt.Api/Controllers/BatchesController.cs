using CheeseIt.Api.Data;
using CheeseIt.Api.Domain;
using CheeseIt.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CheeseIt.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BatchesController : ControllerBase
{
    private readonly CheeseItDbContext _db;

    public BatchesController(CheeseItDbContext db) => _db = db;

    /// <summary>Lists batches, optionally filtered by production stage.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BatchDto>>> GetAll([FromQuery] BatchStatus? status)
    {
        var query = _db.Batches.AsNoTracking().Include(b => b.Recipe).AsQueryable();
        if (status is not null)
        {
            query = query.Where(b => b.Status == status);
        }

        var batches = await query.OrderByDescending(b => b.ProducedOn).ThenBy(b => b.BatchNumber).ToListAsync();
        return Ok(batches.Select(b => b.ToDto()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BatchDto>> GetById(int id)
    {
        var batch = await _db.Batches.AsNoTracking().Include(b => b.Recipe).FirstOrDefaultAsync(b => b.Id == id);
        return batch is null ? NotFound() : Ok(batch.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<BatchDto>> Create(CreateBatchRequest request)
    {
        var recipeExists = await _db.Recipes.AnyAsync(r => r.Id == request.RecipeId);
        if (!recipeExists)
        {
            ModelState.AddModelError(nameof(request.RecipeId), $"Recipe {request.RecipeId} does not exist.");
            return ValidationProblem(ModelState);
        }

        if (await _db.Batches.AnyAsync(b => b.BatchNumber == request.BatchNumber))
        {
            return Conflict($"Batch number '{request.BatchNumber}' already exists.");
        }

        var batch = new CheeseBatch
        {
            BatchNumber = request.BatchNumber,
            RecipeId = request.RecipeId,
            ProducedOn = request.ProducedOn,
            MilkLiters = request.MilkLiters
        };

        _db.Batches.Add(batch);
        await _db.SaveChangesAsync();

        await _db.Entry(batch).Reference(b => b.Recipe).LoadAsync();
        return CreatedAtAction(nameof(GetById), new { id = batch.Id }, batch.ToDto());
    }

    /// <summary>Moves a batch to its next production stage.</summary>
    [HttpPost("{id:int}/advance")]
    public async Task<ActionResult<BatchDto>> Advance(int id)
    {
        var batch = await _db.Batches.Include(b => b.Recipe).FirstOrDefaultAsync(b => b.Id == id);
        if (batch is null)
        {
            return NotFound();
        }

        try
        {
            batch.Advance();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }

        await _db.SaveChangesAsync();
        return Ok(batch.ToDto());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var batch = await _db.Batches.FindAsync(id);
        if (batch is null)
        {
            return NotFound();
        }

        _db.Batches.Remove(batch);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
