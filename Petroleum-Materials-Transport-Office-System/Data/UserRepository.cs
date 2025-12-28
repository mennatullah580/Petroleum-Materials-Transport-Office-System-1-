using Microsoft.Extensions.Configuration;
using Petroleum_Materials_Transport_Office_System.Models;
using System.Data;
using System.Data.SqlClient;

public class UserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("PetroleumDB");
    }

    public List<UserModel> GetAllUsers()
    {
        var users = new List<UserModel>();

        using var con = new SqlConnection(_connectionString);
        con.Open();

        const string query = @"
            SELECT User_ID, Username, Email, Name, Role, Department, Phone_Number
            FROM Users
            ORDER BY User_ID";

        using var cmd = new SqlCommand(query, con);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            users.Add(new UserModel
            {
                Id = reader.GetInt32("User_ID"),
                FullName = reader.IsDBNull("Name") ? "" : reader.GetString("Name"),
                Username = reader.IsDBNull("Username") ? "" : reader.GetString("Username"),
                Email = reader.IsDBNull("Email") ? "" : reader.GetString("Email"),
                Role = reader.IsDBNull("Role") ? "" : reader.GetString("Role"),
                Department = reader.IsDBNull("Department") ? "" : reader.GetString("Department"),
                Phone = reader.IsDBNull("Phone_Number") ? "" : reader.GetString("Phone_Number")
            });
        }

        return users;
    }

    public string? GetUserRoleById(int userId)
    {
        using var con = new SqlConnection(_connectionString);
        con.Open();
        using var cmd = new SqlCommand("SELECT Role FROM Users WHERE User_ID = @Id", con);
        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = userId;

        var result = cmd.ExecuteScalar();
        return result?.ToString();
    }

    public UserModel? GetUserById(int id)
    {
        using var con = new SqlConnection(_connectionString);
        con.Open();

        const string query = @"
        SELECT User_ID, Username, Email, Name, Role, Department, Phone_Number
        FROM Users 
        WHERE User_ID = @Id";

        using var cmd = new SqlCommand(query, con);
        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new UserModel
            {
                Id = reader.GetInt32("User_ID"),
                FullName = reader.IsDBNull("Name") ? "" : reader.GetString("Name"),
                Username = reader.IsDBNull("Username") ? "" : reader.GetString("Username"),
                Email = reader.IsDBNull("Email") ? "" : reader.GetString("Email"),
                Role = reader.IsDBNull("Role") ? "" : reader.GetString("Role"),
                Department = reader.IsDBNull("Department") ? "" : reader.GetString("Department"),
                Phone = reader.IsDBNull("Phone_Number") ? "" : reader.GetString("Phone_Number")
                // Note: Password is NOT loaded (for security)
            };
        }
        return null;
    }

    public bool UserExists(string username, string? email, int? userId = null)
    {
        using var con = new SqlConnection(_connectionString);
        con.Open();

        var conditions = new List<string>();
        var parameters = new List<SqlParameter>();

        conditions.Add("Username = @Username");
        parameters.Add(new SqlParameter("@Username", username));

        if (!string.IsNullOrEmpty(email))
        {
            conditions.Add("Email = @Email");
            parameters.Add(new SqlParameter("@Email", email));
        }

        if (userId.HasValue)
        {
            conditions.Add("User_ID <> @UserId");
            parameters.Add(new SqlParameter("@UserId", userId.Value));
        }

        string whereClause = string.Join(" AND ", conditions);
        string query = $@"
        SELECT COUNT(*) 
        FROM Users 
        WHERE {whereClause}";

        using var cmd = new SqlCommand(query, con);
        cmd.Parameters.AddRange(parameters.ToArray());

        int count = (int)cmd.ExecuteScalar();
        return count > 0;
    }

    public bool DeleteUser(int userId)
    {
        if (userId <= 0) return false;

        using var con = new SqlConnection(_connectionString);
        con.Open();

        const string query = "DELETE FROM Users WHERE User_ID = @UserId";

        using var cmd = new SqlCommand(query, con);
        cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

        return cmd.ExecuteNonQuery() > 0;
    }

    public void SaveUser(UserModel user)
    {
        using var con = new SqlConnection(_connectionString);
        con.Open();

        if (user.Id == 0)
        {
            // INSERT - password required
            if (string.IsNullOrWhiteSpace(user.Password))
                throw new ArgumentException("Password is required for new users.");

            const string query = @"
            INSERT INTO Users (Username, Password, Email, Name, Role, Department, Phone_Number, Created_At)
            VALUES (@Username, @Password, @Email, @Name, @Role, @Department, @Phone_Number, GETDATE())";

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Username", user.Username);
            cmd.Parameters.AddWithValue("@Password", user.Password);
            cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(user.Email) ? (object)DBNull.Value : user.Email);
            cmd.Parameters.AddWithValue("@Name", user.FullName);
            cmd.Parameters.AddWithValue("@Role", user.Role);
            cmd.Parameters.AddWithValue("@Department", string.IsNullOrEmpty(user.Department) ? (object)DBNull.Value : user.Department);
            cmd.Parameters.AddWithValue("@Phone_Number", string.IsNullOrEmpty(user.Phone) ? (object)DBNull.Value : user.Phone);
            cmd.ExecuteNonQuery();
        }
        else
        {
            // UPDATE - only update password if provided
            var setClause = @"
            Username = @Username,
            Email = @Email,
            Name = @Name,
            Role = @Role,
            Department = @Department,
            Phone_Number = @Phone_Number";

            if (!string.IsNullOrEmpty(user.Password))
            {
                setClause += ", Password = @Password";
            }

            var query = $"UPDATE Users SET {setClause} WHERE User_ID = @User_ID";

            using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Username", user.Username);
            cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(user.Email) ? (object)DBNull.Value : user.Email);
            cmd.Parameters.AddWithValue("@Name", user.FullName);
            cmd.Parameters.AddWithValue("@Role", user.Role);
            cmd.Parameters.AddWithValue("@Department", string.IsNullOrEmpty(user.Department) ? (object)DBNull.Value : user.Department);
            cmd.Parameters.AddWithValue("@Phone_Number", string.IsNullOrEmpty(user.Phone) ? (object)DBNull.Value : user.Phone);
            cmd.Parameters.AddWithValue("@User_ID", user.Id);

            if (!string.IsNullOrEmpty(user.Password))
            {
                cmd.Parameters.AddWithValue("@Password", user.Password);
            }

            cmd.ExecuteNonQuery();
        }
    }

    public List<UserModel> SearchUsers(string? searchTerm)
    {
        var users = new List<UserModel>();
        using var con = new SqlConnection(_connectionString);
        con.Open();

        // ✅ Search by ID (if search term is numeric) OR other fields
        string query = @"
        SELECT User_ID, Username, Email, Name, Role, Department, Phone_Number
        FROM Users
        WHERE @SearchTerm IS NULL 
           OR User_ID = @SearchId
           OR Name LIKE '%' + @SearchTerm + '%'
           OR Username LIKE '%' + @SearchTerm + '%'
           OR Email LIKE '%' + @SearchTerm + '%'
           OR Department LIKE '%' + @SearchTerm + '%'
        ORDER BY User_ID";

        using var cmd = new SqlCommand(query, con);

        // Handle ID search: if searchTerm is a number, use it for User_ID
        int? searchId = null;
        if (int.TryParse(searchTerm, out int id))
        {
            searchId = id;
        }

        cmd.Parameters.AddWithValue("@SearchTerm", (object?)searchTerm ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SearchId", (object?)searchId ?? DBNull.Value);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            users.Add(new UserModel
            {
                Id = reader.GetInt32("User_ID"),
                FullName = reader.IsDBNull("Name") ? "" : reader.GetString("Name"),
                Username = reader.IsDBNull("Username") ? "" : reader.GetString("Username"),
                Email = reader.IsDBNull("Email") ? "" : reader.GetString("Email"),
                Role = reader.IsDBNull("Role") ? "" : reader.GetString("Role"),
                Department = reader.IsDBNull("Department") ? "" : reader.GetString("Department"),
                Phone = reader.IsDBNull("Phone_Number") ? "" : reader.GetString("Phone_Number")
            });
        }

        return users;
    }



    public bool IsEmailTaken(string email, int? excludeUserId = null)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

        using var con = new SqlConnection(_connectionString);
        con.Open();

        // ✅ Key: Exclude current user by ID
        string query = excludeUserId.HasValue
            ? "SELECT COUNT(*) FROM Users WHERE Email = @Email AND User_ID <> @ExcludeId"
            : "SELECT COUNT(*) FROM Users WHERE Email = @Email";

        using var cmd = new SqlCommand(query, con);
        cmd.Parameters.Add("@Email", SqlDbType.NVarChar).Value = email;
        if (excludeUserId.HasValue)
            cmd.Parameters.Add("@ExcludeId", SqlDbType.Int).Value = excludeUserId.Value;

        return (int)cmd.ExecuteScalar() > 0;
    }

    public bool IsUsernameTaken(string username, int? excludeUserId = null)
    {
        if (string.IsNullOrWhiteSpace(username)) return false;

        using var con = new SqlConnection(_connectionString);
        con.Open();

        string query = excludeUserId.HasValue
            ? "SELECT COUNT(*) FROM Users WHERE Username = @Username AND User_ID <> @ExcludeId"
            : "SELECT COUNT(*) FROM Users WHERE Username = @Username";

        using var cmd = new SqlCommand(query, con);
        cmd.Parameters.Add("@Username", SqlDbType.NVarChar).Value = username;
        if (excludeUserId.HasValue)
            cmd.Parameters.Add("@ExcludeId", SqlDbType.Int).Value = excludeUserId.Value;

        return (int)cmd.ExecuteScalar() > 0;
    }
}