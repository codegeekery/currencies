var builder = WebApplication.CreateBuilder(args);

// Servicios
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient("frankfurter", client =>
{
    client.BaseAddress = new Uri("https://api.frankfurter.app/v2");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// -------------------------------
// 0) Ruta raíz - Informacao da api
// -------------------------------
app.MapGet("/", () =>
{
    var info = new
    {
        nome = "Currencies API",
        descripcao = "API para consultar tasas de cambio de monedas usando Frankfurter API",
        fonte = "https://api.frankfurter.app/",
        endpoints = new[]
        {
            new { ruta = "/api/notas", metodo = "GET", descripcion = "Lista todas las monedas disponibles" },
        },
        documentacao = "/swagger"
    };

    return Results.Json(info);
})
.WithName("Inicio")
.WithSummary("Información general de la API");

// -------------------------------
// 1) Listar todas as moedas
// -------------------------------
app.MapGet("/api/notas", async (IHttpClientFactory httpClientFactory) =>
{
    var client = httpClientFactory.CreateClient("frankfurter");
    var response = await client.GetAsync("https://api.frankfurter.dev/v2/currencies");

    if (!response.IsSuccessStatusCode)
        return Results.Problem("Error al consultar Frankfurter API");

    var json = await response.Content.ReadAsStringAsync();
    return Results.Content(json, "application/json");
})
.WithName("ListarMonedas")
.WithSummary("Lista todas las monedas disponibles")
.WithDescription("Consulta el endpoint /currencies de Frankfurter API y devuelve el listado completo.");


app.Run();