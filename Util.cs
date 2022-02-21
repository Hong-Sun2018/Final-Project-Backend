using System.Security.Cryptography;
using System.Text;

namespace Final_Project_Backend
{
    public static class Util
    {
        public static string CalculateMd5(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }

            return sb.ToString();
        }

        public static long CurrentTimestamp()
        {
            return (long)(DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static string GenerateUserToken(Models.Classes.User user)
        {
            Random random = new Random();
            string seed = user.UserName + random.NextInt64();
            return CalculateMd5(seed) + user.UserID;
        }
    }
}
