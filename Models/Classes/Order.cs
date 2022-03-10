using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project_Backend.Models.Classes
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        public int OrderID { get; set; }
        [Required]
        public int UserID { get; set; }
        [Required]
        public long OrderTime { get; set; }
        [Required]
        public OrderStatus Status { get; set; }
        [NotMapped]
        public double TotalAmount { get; set; }
        [NotMapped] 
        public List<OrderProduct>? OrderProducts { get; set; }
    }

    public enum OrderStatus
    {
        New, 
        Send, 
        Cancled,
    }
}
