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

    public bool UserExists(string username, string email, int? userId = null)
    {
        using var con = new SqlConnection(_connectionString);
        con.Open();

        const string query = @"
            SELECT COUNT(*) 
            FROM Users 
            WHERE (Username = @Username OR Email = @Email)
              AND (@UserId IS NULL OR User_ID <> @UserId)";

        using var cmd = new SqlCommand(query, con);
        cmd.Parameters.AddWithValue("@Username", username);
        cmd.Parameters.AddWithValue("@Email", email);
        cmd.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);

        return (int)cmd.ExecuteScalar() > 0;
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

        string query = user.Id == 0
            ? @"INSERT INTO Users (Username, Password, Email, Name, Role, Department, Phone_Number, Created_At)
            VALUES (@Username, @Password, @Email, @Name, @Role, @Department, @Phone_Number, GETDATE())"
            : @"UPDATE Users SET
            Username = @Username,
            Email = @Email,
            Name = @Name,
            Role = @Role,
            Department = @Department,
            Phone_Number = @Phone_Number
            WHERE User_ID = @User_ID";

        using var cmd = new SqlCommand(query, con);

        cmd.Parameters.AddWithValue("@Username", user.Username ?? "");
        cmd.Parameters.AddWithValue("@Password", user.Password ?? "");
        cmd.Parameters.AddWithValue("@Email", user.Email ?? "");
        cmd.Parameters.AddWithValue("@Name", user.FullName ?? "");
        cmd.Parameters.AddWithValue("@Role", user.Role ?? "");
        cmd.Parameters.AddWithValue("@Department", user.Department ?? "");
        cmd.Parameters.AddWithValue("@Phone_Number", user.Phone ?? "");

        if (user.Id != 0)
        {
            cmd.Parameters.Add("@User_ID", SqlDbType.Int).Value = user.Id;
        }
        else
        {
            // For INSERT, password is required
            if (string.IsNullOrEmpty(user.Password))
                throw new ArgumentException("Password is required for new users.");
        }

        cmd.ExecuteNonQuery();
    }

    public List<UserModel> SearchUsers(string? searchTerm)
    {
        var users = new List<UserModel>();
        using var con = new SqlConnection(_connectionString);
        con.Open();

        // Search in Name, Username, Email, Department
        string query = @"
        SELECT User_ID, Username, Email, Name, Role, Department, Phone_Number
        FROM Users
        WHERE @SearchTerm IS NULL 
           OR Name LIKE '%' + @SearchTerm + '%'
           OR Username LIKE '%' + @SearchTerm + '%'
           OR Email LIKE '%' + @SearchTerm + '%'
           OR Department LIKE '%' + @SearchTerm + '%'
        ORDER BY User_ID";

        using var cmd = new SqlCommand(query, con);
        cmd.Parameters.AddWithValue("@SearchTerm", (object?)searchTerm ?? DBNull.Value);

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
}