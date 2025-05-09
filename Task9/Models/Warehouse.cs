using System.ComponentModel.DataAnnotations;

namespace Task9.Models;

public class Warehouse
{
    [Required]
    public int IdProduct { get; set; }
}