namespace WorldCitiesApi.Dtos;

public class CountryPopulation
{
    public int Id { get; set; }
    public string Name { get; init; } = null!;
    public int Population { get; set; }
}