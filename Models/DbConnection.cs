namespace Final_Project_Backend.Models
{
    public class DbConnection
    {
        private const string conString = @"
            Data Source=h0n9.com;
            Initial Catalog=FinalProject;
            User ID=SA;Password=Winzippp1!;
            Connect Timeout=30;
            Encrypt=False;
            TrustServerCertificate=False;
            ApplicationIntent=ReadWrite;
            MultiSubnetFailover=False
        ";

        public string GetConnStr()
        {
            return conString;
        }
    }
}
