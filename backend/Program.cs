using Microsoft.EntityFrameworkCore;
using CurrenciesApi.Data;
using CurrenciesApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite("Data Source=currencies.db"));

var app = builder.Build();

Console.WriteLine($"BD en: {Path.GetFullPath("currencies.db")}");

using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// -------------------------------
// 0) Rota raiz - Informação da API
// Mostra um resumo do que a API faz e lista os endpoints disponíveis,
// útil como "porta de entrada" para quem consome a API pela primeira vez.
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
            new { rota = "/api/currencies", metodo = "GET", descricao = "Lista todas as moedas disponíveis" },
            new { rota = "/api/convert", metodo = "GET", descricao = "Converte um valor entre duas moedas (parâmetros: from, to, amount)" },
            new { rota = "/api/rates/{date}", metodo = "GET", descricao = "Retorna as taxas de câmbio (base EUR) para uma data específica (yyyy-MM-dd)" },
        },
        documentacao = "/swagger"
    };

    return Results.Json(info);
})
.WithName("Inicio")
.WithSummary("Informação geral da API");


// -------------------------------
// 1) Listar moedas
// Retorna todas as moedas suportadas. Na primeira chamada, se a tabela
// estiver vazia, busca a lista completa na Frankfurter API e a persiste
// no banco; nas próximas chamadas, os dados já vêm direto do banco local.
// -------------------------------
app.MapGet("/api/currencies", async (AppDbContext db, IHttpClientFactory httpFactory) =>
{
    if (!await db.Currencies.AnyAsync())
    {
        var client = httpFactory.CreateClient();
        var dict = await client.GetFromJsonAsync<Dictionary<string, string>>(
            "https://api.frankfurter.dev/v2/currencies");

        foreach (var (code, name) in dict!)
            db.Currencies.Add(new Currency { Code = code, Name = name });

        await db.SaveChangesAsync();
    }

    var moedas = await db.Currencies.OrderBy(c => c.Code).ToListAsync();
    return Results.Ok(moedas);
});


// -------------------------------
// 2) Converter valores entre moedas
// Recebe moeda de origem, moeda de destino e o valor a converter.
// Verifica se já existe a taxa do dia no banco; caso não exista, busca
// as taxas atuais na Frankfurter API, salva no banco e então calcula
// o resultado da conversão.
// -------------------------------
app.MapGet("/api/convert", async (string from, string to, decimal amount, AppDbContext db, IHttpClientFactory httpFactory) =>
{
    from = from.ToUpper();
    to = to.ToUpper();
    var hoje = DateOnly.FromDateTime(DateTime.UtcNow);

    var taxa = await db.Rates.FirstOrDefaultAsync(r =>
        r.Date == hoje && r.Base == from && r.Target == to);

    if (taxa == null)
    {
        var client = httpFactory.CreateClient();
        var json = await client.GetFromJsonAsync<FrankfurterResponse>(
            $"https://api.frankfurter.dev/v1/latest?base={from}");

        foreach (var (target, rate) in json!.Rates)
            db.Rates.Add(new Rate { Date = hoje, Base = from, Target = target, Value = rate });

        await db.SaveChangesAsync();

        taxa = await db.Rates.FirstOrDefaultAsync(r =>
            r.Date == hoje && r.Base == from && r.Target == to);

        if (taxa == null)
            return Results.NotFound($"Não existe taxa {from}->{to}");
    }

    return Results.Ok(new { from, to, amount, rate = taxa.Value, result = amount * taxa.Value });
});


// -------------------------------
// 3) Consultar taxas por data específica
// Retorna as taxas de câmbio (base EUR) de uma data informada no formato
// yyyy-MM-dd. Funciona como um cache: se a data já foi consultada antes,
// os valores vêm do banco; caso contrário, busca na Frankfurter API
// e grava os resultados para consultas futuras.
// -------------------------------
app.MapGet("/api/rates/{date}", async (string date, AppDbContext db, IHttpClientFactory httpFactory) =>
{
    if (!DateOnly.TryParse(date, out var data))
        return Results.BadRequest("Formato inválido, use yyyy-MM-dd");

    var existentes = await db.Rates.Where(r => r.Date == data && r.Base == "EUR").ToListAsync();

    if (existentes.Count == 0)
    {
        var client = httpFactory.CreateClient();
        var json = await client.GetFromJsonAsync<FrankfurterResponse>(
            $"https://api.frankfurter.dev/v1/{date}");

        foreach (var (target, rate) in json!.Rates)
        {
            var r = new Rate { Date = data, Base = "EUR", Target = target, Value = rate };
            db.Rates.Add(r);
            existentes.Add(r);
        }
        await db.SaveChangesAsync();
    }

    return Results.Ok(new
    {
        date = data,
        rates = existentes.ToDictionary(r => r.Target, r => r.Value)
    });
});

app.Run();