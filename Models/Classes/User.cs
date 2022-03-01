using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project_Backend.Models.Classes
{

    [Table("Users")]
    public class User
    {
        [Key]
        public int UserID { get; set; }
       
        [MaxLength(30)]
        public string UserName { get; set; }
        [MaxLength(32)]
        public string Password { get; set; }
        [MaxLength(64)]
        public string Token { get; set; }   
        public long TimeToken { get; set; }
        public long TimeLogin { get; set; }    
        public bool IsAdmin { get; set; }
        public int NumWrongPwd { get; set; }
        public long TimeWrongPwd { get; set; }

        public User(string userName, string password,string token="", long timeToken = 0, long timeLogin = 0, bool isAdmin=false, int numWrongPwd=0, long timeWrongPwd=0 )
        {
            this.UserName = userName;   
            this.Password=password;
            this.IsAdmin = isAdmin  ;
            this.Token = token;
            this.TimeToken = timeToken;
            this.TimeLogin = timeLogin;
            this.NumWrongPwd = numWrongPwd;
            this.TimeWrongPwd = timeWrongPwd;
        }
    }
}
