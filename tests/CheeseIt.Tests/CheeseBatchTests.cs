using CheeseIt.Api.Domain;
using Xunit;

namespace CheeseIt.Tests;

public class CheeseBatchTests
{
    [Fact]
    public void NewBatch_StartsInCurdlingStage()
    {
        var batch = new CheeseBatch { BatchNumber = "2026-001" };

        Assert.Equal(BatchStatus.Curdling, batch.Status);
    }

    [Fact]
    public void Advance_WalksThroughAllStagesInOrder()
    {
        var batch = new CheeseBatch { BatchNumber = "2026-001" };
        var visited = new List<BatchStatus>();

        while (!batch.IsFinalStage)
        {
            batch.Advance();
            visited.Add(batch.Status);
        }

        Assert.Equal(
            new[] { BatchStatus.Pressing, BatchStatus.Brining, BatchStatus.Maturing, BatchStatus.ReadyForSale },
            visited);
    }

    [Fact]
    public void Advance_InFinalStage_Throws()
    {
        var batch = new CheeseBatch { BatchNumber = "2026-001", Status = BatchStatus.ReadyForSale };

        Assert.Throws<InvalidOperationException>(() => batch.Advance());
    }

    [Theory]
    [InlineData(0, "2026-03-01")]
    [InlineData(30, "2026-03-31")]
    [InlineData(90, "2026-05-30")]
    public void EarliestSaleDate_AddsMaturationDays(int days, string expected)
    {
        var batch = new CheeseBatch { ProducedOn = new DateOnly(2026, 3, 1) };

        Assert.Equal(DateOnly.Parse(expected), batch.EarliestSaleDate(days));
    }
}
