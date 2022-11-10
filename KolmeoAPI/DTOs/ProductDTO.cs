using System.ComponentModel.DataAnnotations;

namespace KolmeoAPI.DTOs
{
    public record ProductDTO
    {
        public long Id { get; init; }
        [Required]
        public string? Name { get; init; }
        public string? Description { get; init; }
        [Range(0, double.MaxValue)]
        public decimal Price { get; init; }
    }
}
