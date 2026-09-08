namespace CurrenciesApi.Models;

public class FrankfurterResponse
{
    public string Base { get; set; } = default!;
    public string Date { get; set; } = default!;
    public Dictionary<string, decimal> Rates { get; set; } = default!;
}