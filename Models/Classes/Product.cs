using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project_Backend.Models.Classes
{
    [Table("Products")]
    public class Product
    {
        [Key]
        public int ProductID { get; set; }
        [Required]
        [MaxLength(50)]
        public string? ProductName { get; set; } 
        [Required]
        public double ProductPrice { get; set; }
        [Required]
        public int ProductStock { get; set; }
        public string? ProductDesc { get; set; }
        [Required] 
        public int CategoryID { get; set; } 
        public string? File1 { get; set; }
        public string? File2 { get; set; }
        public string? File3 { get; set; }

        [NotMapped]
        public IFormFile? FormFile1 { get; set; }
        [NotMapped]
        public IFormFile? FormFile2 { get; set; }

        [NotMapped]
        public IFormFile? FormFile3 { get; set; }

    }
}
