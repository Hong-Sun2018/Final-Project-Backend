namespace Final_Project_Backend.Models
{
    public static class DBConfig
    {
        public const string ConnString = @"
            Data Source=localhost;
            Initial Catalog=FinalProject_EF;
            User ID=SA;Password=Winzippp1!;
            Connect Timeout=30;
            Encrypt=False;
            TrustServerCertificate=False;
            ApplicationIntent=ReadWrite;
            MultiSubnetFailover=False
        ";

        public const int TokenLife = 24 * 3600;
        public const int LoginLift = 3 * 24 * 3600;
    }
}
