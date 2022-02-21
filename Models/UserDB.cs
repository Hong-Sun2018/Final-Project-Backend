using Final_Project_Backend.Models.Classes;
using System.Data.SqlClient;

namespace Final_Project_Backend.Models
{
    public class UserDB
    {
        private readonly string connStr = new DbConnection().GetConnStr();

        private const int TokenLifeLength = 24 * 60 * 60;
        private const int LoginLifeLength = 3 * 24 * 60 * 60;

        public async Task<bool> CreateUser(User user)
        {
            User? existedUser = GetUserByUsername(user.UserName);
            if (existedUser != null)
                return false;

            string sql = @"INSERT INTO [User] (UserName, Password, Token, TimeLogin, NumWrongPwd, TimeWrongPwd)
                VALUES (@userName, @password, @token, @timeLogin, @numWrongPwd, @timeWrongPwd)";

            using (SqlConnection connection = new SqlConnection(connStr))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add(new SqlParameter("userName", user.UserName));
                command.Parameters.Add(new SqlParameter("password", user.Password));
                command.Parameters.Add(new SqlParameter("token", user.Token));
                command.Parameters.Add(new SqlParameter("timeLogin", Util.CurrentTimestamp()));
                command.Parameters.Add(new SqlParameter("numWrongPwd", user.NumWrongPwd));
                command.Parameters.Add(new SqlParameter("timeWrongPwd", user.TimeWrongPwd));
                
                try
                {
                    connection.Open();
                    await command.ExecuteNonQueryAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }

        }

        public User? GetUserByUsername(string username)
        {
            string sql = @"SELECT UserID, [UserName], [Password], [IsAdmin], [Token], [TimeLogin], [NumWrongPwd], [TimeWrongPwd] FROM [User] WHERE UserName=@username";
            Console.WriteLine("2222222");
            using (SqlConnection connection = new SqlConnection(connStr))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();
                command.Parameters.Add(new SqlParameter("username", username));
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    Console.Error.WriteLine("1111111");
                    User user = new User(
                        reader.GetSqlString(1).Value,
                        reader.GetSqlString(2).Value
                    );

                    user.UserID = reader.GetSqlInt32(0).Value;
                    user.IsAdmin = reader.GetSqlBoolean(3).Value;
                    user.Token = reader.GetSqlString(4).Value;
                    user.TimeLogin = reader.GetSqlInt64(5).Value;
                    user.NumWrongPwd = reader.GetSqlInt32(6).Value;
                    user.TimeWrongPwd = reader.GetSqlInt64(7).Value;
                    return user;
                }
            }
            return null;
        }

        

        public List<User> GetUsers()
        {
            List<User> users = new List<User>();
            string sql = @"SELECT [UserID], [UserName] FROM User WHERE IsAdmin=0";

            using (SqlConnection connection = new SqlConnection(connStr))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    User user = new User(
                        reader.GetSqlString(1).Value,
                        ""
                    );
                    user.UserID = reader.GetSqlInt32(0).Value;
                    users.Add(user);
                }
                return users;
                
            }
        }

        public User? GetUserById(int userID)
        {
            string sql = @"SELECT [UserID], [UserName] FROM User WHERE UserID=@userID";

            using (SqlConnection connection = new SqlConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();
                command.Parameters.Add(new SqlParameter("userID", userID));
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    User user = new User(
                        reader.GetSqlString(1).Value,
                        ""
                    );

                    user.UserID = userID;
                    return user;
                }
                else
                    return null;
            }
        }

        public async void UpdateUser(User user)
        {
            string sql = @"UPDATE [User] SET Token=@token, TimeLogin=@timeLogin, NumWrongPwd=@numWrongPwd, TimeWrongPwd=@timeWrongPwd WHERE UserID=@userID";

            using (SqlConnection connection = new SqlConnection(connStr))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();
                command.Parameters.Add(new SqlParameter("token", user.Token));
                command.Parameters.Add(new SqlParameter("timeLogin", user.TimeLogin));
                command.Parameters.Add(new SqlParameter("numWrongPwd", user.NumWrongPwd));
                command.Parameters.Add(new SqlParameter("timeWrongPwd", user.TimeWrongPwd));
                command.Parameters.Add(new SqlParameter("userID", user.UserID));

                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
