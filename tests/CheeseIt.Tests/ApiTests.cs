using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CheeseIt.Api.Domain;
using CheeseIt.Api.Dtos;
using Xunit;

namespace CheeseIt.Tests;

public class ApiTests : IClassFixture<ApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _client;

    public ApiTests(ApiFactory factory) => _client = factory.CreateClient();

    private async Task<RecipeDto> CreateRecipeAsync(string name, int maturationDays = 60)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/recipes", new RecipeRequest(name, MilkType.Cow, maturationDays, null), Json);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<RecipeDto>(Json))!;
    }

    [Fact]
    public async Task CreateRecipe_ThenGetById_ReturnsIt()
    {
        var created = await CreateRecipeAsync("Bergkaese");

        var fetched = await _client.GetFromJsonAsync<RecipeDto>($"/api/recipes/{created.Id}", Json);

        Assert.Equal("Bergkaese", fetched!.Name);
        Assert.Equal(MilkType.Cow, fetched.Milk);
    }

    [Fact]
    public async Task CreateRecipe_WithDuplicateName_ReturnsConflict()
    {
        await CreateRecipeAsync("Ziegenfrischkaese");

        var response = await _client.PostAsJsonAsync(
            "/api/recipes", new RecipeRequest("Ziegenfrischkaese", MilkType.Goat, 7, null), Json);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateRecipe_WithInvalidData_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/recipes", new RecipeRequest("", MilkType.Cow, -5, null), Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateBatch_WithUnknownRecipe_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/batches", new CreateBatchRequest("2026-900", 99999, new DateOnly(2026, 3, 1), 20m), Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Batch_CanBeAdvancedUntilFinalStage_ThenConflicts()
    {
        var recipe = await CreateRecipeAsync("Hartkaese", maturationDays: 90);
        var create = await _client.PostAsJsonAsync(
            "/api/batches", new CreateBatchRequest("2026-001", recipe.Id, new DateOnly(2026, 3, 1), 40m), Json);
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var batch = (await create.Content.ReadFromJsonAsync<BatchDto>(Json))!;

        Assert.Equal(BatchStatus.Curdling, batch.Status);
        Assert.Equal(new DateOnly(2026, 5, 30), batch.EarliestSaleDate);

        for (var i = 0; i < 4; i++)
        {
            var step = await _client.PostAsync($"/api/batches/{batch.Id}/advance", null);
            Assert.Equal(HttpStatusCode.OK, step.StatusCode);
        }

        var fetched = await _client.GetFromJsonAsync<BatchDto>($"/api/batches/{batch.Id}", Json);
        Assert.Equal(BatchStatus.ReadyForSale, fetched!.Status);

        var tooFar = await _client.PostAsync($"/api/batches/{batch.Id}/advance", null);
        Assert.Equal(HttpStatusCode.Conflict, tooFar.StatusCode);
    }

    [Fact]
    public async Task DeleteRecipe_WithExistingBatches_ReturnsConflict()
    {
        var recipe = await CreateRecipeAsync("Schnittkaese");
        await _client.PostAsJsonAsync(
            "/api/batches", new CreateBatchRequest("2026-777", recipe.Id, new DateOnly(2026, 3, 1), 15m), Json);

        var response = await _client.DeleteAsync($"/api/recipes/{recipe.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
