using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project_Backend.Models.Classes
{
    [Table("Debugs")]
    public class Debug
    {
        [Key]
        public int DebugID { get; set; }
        public string Msg { get; set; }

        public Debug(string msg)
        {
            Msg = msg;
        }
    }
}
