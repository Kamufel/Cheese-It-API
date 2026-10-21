namespace CheeseIt.Api.Domain;

public class Recipe
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public MilkType Milk { get; set; }
    public int MinMaturationDays { get; set; }
    public string? Notes { get; set; }

    public List<CheeseBatch> Batches { get; set; } = new();
}
