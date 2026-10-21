using CheeseIt.Api.Domain;

namespace CheeseIt.Api.Dtos;

public static class Mappings
{
    public static RecipeDto ToDto(this Recipe r) =>
        new(r.Id, r.Name, r.Milk, r.MinMaturationDays, r.Notes);

    /// <remarks>The batch must be loaded together with its <see cref="CheeseBatch.Recipe"/>.</remarks>
    public static BatchDto ToDto(this CheeseBatch b) =>
        new(b.Id, b.BatchNumber, b.RecipeId, b.Recipe!.Name, b.ProducedOn,
            b.EarliestSaleDate(b.Recipe.MinMaturationDays), b.MilkLiters, b.Status);
}
