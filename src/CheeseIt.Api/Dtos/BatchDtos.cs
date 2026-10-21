using System.ComponentModel.DataAnnotations;
using CheeseIt.Api.Domain;

namespace CheeseIt.Api.Dtos;

public record BatchDto(
    int Id,
    string BatchNumber,
    int RecipeId,
    string RecipeName,
    DateOnly ProducedOn,
    DateOnly EarliestSaleDate,
    decimal MilkLiters,
    BatchStatus Status);

public record CreateBatchRequest(
    [Required, StringLength(50, MinimumLength = 3)] string BatchNumber,
    int RecipeId,
    DateOnly ProducedOn,
    [Range(0.1, 100000)] decimal MilkLiters);
