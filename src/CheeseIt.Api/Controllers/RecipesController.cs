using CheeseIt.Api.Data;
using CheeseIt.Api.Domain;
using CheeseIt.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CheeseIt.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecipesController : ControllerBase
{
    private readonly CheeseItDbContext _db;

    public RecipesController(CheeseItDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RecipeDto>>> GetAll()
    {
        var recipes = await _db.Recipes.AsNoTracking().OrderBy(r => r.Name).ToListAsync();
        return Ok(recipes.Select(r => r.ToDto()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RecipeDto>> GetById(int id)
    {
        var recipe = await _db.Recipes.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        return recipe is null ? NotFound() : Ok(recipe.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<RecipeDto>> Create(RecipeRequest request)
    {
        if (await _db.Recipes.AnyAsync(r => r.Name == request.Name))
        {
            return Conflict($"A recipe named '{request.Name}' already exists.");
        }

        var recipe = new Recipe
        {
            Name = request.Name,
            Milk = request.Milk,
            MinMaturationDays = request.MinMaturationDays,
            Notes = request.Notes
        };

        _db.Recipes.Add(recipe);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = recipe.Id }, recipe.ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<RecipeDto>> Update(int id, RecipeRequest request)
    {
        var recipe = await _db.Recipes.FindAsync(id);
        if (recipe is null)
        {
            return NotFound();
        }

        if (await _db.Recipes.AnyAsync(r => r.Name == request.Name && r.Id != id))
        {
            return Conflict($"A recipe named '{request.Name}' already exists.");
        }

        recipe.Name = request.Name;
        recipe.Milk = request.Milk;
        recipe.MinMaturationDays = request.MinMaturationDays;
        recipe.Notes = request.Notes;
        await _db.SaveChangesAsync();

        return Ok(recipe.ToDto());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var recipe = await _db.Recipes.FindAsync(id);
        if (recipe is null)
        {
            return NotFound();
        }

        if (await _db.Batches.AnyAsync(b => b.RecipeId == id))
        {
            return Conflict("The recipe is used by existing batches and cannot be deleted.");
        }

        _db.Recipes.Remove(recipe);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
