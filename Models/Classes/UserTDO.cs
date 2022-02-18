namespace Final_Project_Backend.Models.Classes
{
    public class UserTDO
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string Token { get; set; }
        public long TimeLogin { get; set; }
        public bool IsAdmin { get; set; }
        public int NumWrongPwd { get; set; }
        public long TimeWrongPwd { get; set; }

        public UserTDO()
        {
            this.Token = "";
            this.UserName = "";
        }

    }
}
