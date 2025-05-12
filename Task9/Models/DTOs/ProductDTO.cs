using System.ComponentModel.DataAnnotations;

namespace Task9.Models.DTOs;

public class ProductDTO
{
    [Required]
    public int IdProduct { get; set; }
    
    [Required]
    public int IdWarehouse { get; set; }
    
    [Required]
    public int Amount { get; set; }
    
    [Required]
    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; }   
}