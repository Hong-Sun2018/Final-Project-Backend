namespace Final_Project_Backend.Models.Classes
{
    public class User
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public string Token { get; set; }   
        public long TimeLogin { get; set; }    
        public bool IsAdmin { get; set; }
        public int NumWrongPwd { get; set; }
        public long TimeWrongPwd { get; set; }

        public User(int userID, string userName, string password)
        {
            this.UserID = userID;
            this.UserName = userName;   
            this.Password=password;
            this.IsAdmin = false;
            this.Token = "";
            this.TimeLogin = 0; 
            this.TimeWrongPwd = 0;
            this.NumWrongPwd = 0;
        }
    }
}
