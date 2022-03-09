using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project_Backend.Models.Classes
{
    [Table("CartProduct")]
    public class CartProduct
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int UserID { get; set; }
        [Required]
        public int ProductID { get; set; }
        [Required]
        public int ProductAmount { get; set; }

        public CartProduct(int userID, int productID, int productAmount)
        {
            this.UserID = userID;
            this.ProductID = productID;
            this.ProductAmount = productAmount;
        }
        
    }
}
