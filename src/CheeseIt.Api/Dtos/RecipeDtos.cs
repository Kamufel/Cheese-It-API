using System.ComponentModel.DataAnnotations;
using CheeseIt.Api.Domain;

namespace CheeseIt.Api.Dtos;

public record RecipeDto(int Id, string Name, MilkType Milk, int MinMaturationDays, string? Notes);

public record RecipeRequest(
    [Required, StringLength(100, MinimumLength = 2)] string Name,
    MilkType Milk,
    [Range(0, 3650)] int MinMaturationDays,
    [StringLength(2000)] string? Notes);
