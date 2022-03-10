using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project_Backend.Models.Classes
{
    [Table("OrderProduct")]
    public class OrderProduct
    {
        [Key]
        public int Id { get; set; }
        [Required]  
        public int OrderId { get; set; }
        [Required]
        public int ProductId { get; set; }
        [Required]
        public double ProductPris { get; set; }
        [Required]
        public int ProductQuantity { get; set; }

    }
}
