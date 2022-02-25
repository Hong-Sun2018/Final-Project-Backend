using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Project_Backend.Models.Classes
{

    [Table("Users")]
    public class User
    {
        [Key]
        public int UserID { get; set; }
       
        public string UserName { get; set; }
        public string Password { get; set; }

        public string Token { get; set; }   
        public long TimeLogin { get; set; }    
        public bool IsAdmin { get; set; }
        public int NumWrongPwd { get; set; }
        public long TimeWrongPwd { get; set; }

        public User(string userName, string password,string token="", long timeLogin = 0, bool isAdmin=false, int numWrongPwd=0, long timeWrongPwd=0 )
        {
            this.UserName = userName;   
            this.Password=password;
            this.IsAdmin = isAdmin  ;
            this.Token = token;
            this.TimeLogin = timeLogin;
            this.NumWrongPwd = numWrongPwd;
            this.TimeWrongPwd = timeWrongPwd;
        }
    }
}
