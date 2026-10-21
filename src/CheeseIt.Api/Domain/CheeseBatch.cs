namespace CheeseIt.Api.Domain;

public class CheeseBatch
{
    public int Id { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateOnly ProducedOn { get; set; }
    public decimal MilkLiters { get; set; }
    public BatchStatus Status { get; set; } = BatchStatus.Curdling;

    public int RecipeId { get; set; }
    public Recipe? Recipe { get; set; }

    public bool IsFinalStage => Status == BatchStatus.ReadyForSale;

    /// <summary>Moves the batch to the next production stage.</summary>
    /// <exception cref="InvalidOperationException">The batch is already in its final stage.</exception>
    public void Advance()
    {
        if (IsFinalStage)
        {
            throw new InvalidOperationException(
                $"Batch '{BatchNumber}' is already in its final stage ({Status}).");
        }

        Status++;
    }

    /// <summary>Earliest day the batch may be sold, given the recipe's minimum maturation time.</summary>
    public DateOnly EarliestSaleDate(int minMaturationDays) => ProducedOn.AddDays(minMaturationDays);
}
