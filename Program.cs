var builder = WebApplication.CreateBuilder(args);

// Serviços
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// -------------------------------
// 0) Rota raiz - Informação da API
// -------------------------------
app.MapGet("/", () =>
{
    var info = new
    {
        nome = "Currencies API",
        descricao = "API para consultar taxas de câmbio de moedas usando a Frankfurter API",
        fonte = "https://api.frankfurter.app/",
        endpoints = new[]
        {
            new { rota = "/api/moedas", metodo = "GET", descricao = "Lista todas as moedas disponíveis" },
        },
        documentacao = "/swagger"
    };

    return Results.Json(info);
})
.WithName("Inicio")
.WithSummary("Informação geral da API");

// -------------------------------
// 1) Listar todas as moedas
// -------------------------------
app.MapGet("/api/moedas", async (IHttpClientFactory httpClientFactory) =>
{
    var client = httpClientFactory.CreateClient();
    var response = await client.GetAsync("https://api.frankfurter.dev/v2/currencies");

    if (!response.IsSuccessStatusCode)
        return Results.Problem("Erro ao consultar a Frankfurter API");

    var json = await response.Content.ReadAsStringAsync();
    return Results.Content(json, "application/json");
})
.WithName("ListarMoedas")
.WithSummary("Lista todas as moedas disponíveis")
.WithDescription("Consulta o endpoint /currencies da Frankfurter API e devolve a listagem completa.");


app.Run();