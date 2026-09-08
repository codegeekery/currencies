namespace CurrenciesApi.Models;

public class Rate
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public string Base { get; set; } = default!;
    public string Target { get; set; } = default!;
    public decimal Value { get; set; }
}