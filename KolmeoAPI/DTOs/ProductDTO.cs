namespace KolmeoAPI.DTOs
{
    public record ProductDTO
    {
        public long Id { get; init; }
        public string? Name { get; init; }
        public string? Description { get; init; }
        public decimal Price { get; init; }
    }
}
