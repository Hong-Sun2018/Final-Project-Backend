namespace Final_Project_Backend.Models.Classes
{
    public class UserInfo
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public bool isAdmin { get; set; }

        public UserInfo(int userID, string username)
        {
            this.UserID = userID;   
            this.UserName = username;
            this.isAdmin = false;
        }

    }
}
