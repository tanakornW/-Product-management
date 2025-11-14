using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class CreateProductRequest
{
    [Required]
    public string Code { get; set; } = string.Empty;
}

