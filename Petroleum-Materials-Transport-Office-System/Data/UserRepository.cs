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

        using SqlConnection con = new SqlConnection(_connectionString);
        con.Open();

        string query = @"
        SELECT User_ID, Username, Email, Name, Role, Department, Phone_Number
        FROM Users
        ORDER BY User_ID";

        using SqlCommand cmd = new SqlCommand(query, con);
        using SqlDataReader reader = cmd.ExecuteReader();

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
                // ❌ NO Branch — not in DB!
            });
        }

        return users;
    }

    public bool UserExists(string username, string email, int? userId = null)
    {
        using SqlConnection con = new SqlConnection(_connectionString);
        con.Open();

        string query = @"
            SELECT COUNT(*) 
            FROM Users 
            WHERE (Username = @Username OR Email = @Email)
            AND (@UserId IS NULL OR User_ID <> @UserId)"; // ✅ User_ID, not Id

        using SqlCommand cmd = new SqlCommand(query, con);
        cmd.Parameters.AddWithValue("@Username", username);
        cmd.Parameters.AddWithValue("@Email", email);
        cmd.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);

        int count = (int)cmd.ExecuteScalar();
        return count > 0;
    }

    public bool DeleteUser(int userId)
    {
        // Optional: Prevent deleting yourself or last admin
        if (userId <= 0) return false;

        using var con = new SqlConnection(_connectionString);
        con.Open();

        // Use a transaction if you have related data (not needed here)
        string query = "DELETE FROM Users WHERE User_ID = @UserId";

        using var cmd = new SqlCommand(query, con);
        cmd.Parameters.AddWithValue("@UserId", userId);

        int rowsAffected = cmd.ExecuteNonQuery();
        return rowsAffected > 0;
    }

    public void SaveUser(UserModel user)
    {
        using SqlConnection con = new SqlConnection(_connectionString);
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
                WHERE User_ID = @User_ID"; // ✅ User_ID

        using SqlCommand cmd = new SqlCommand(query, con);

        // Always add all parameters (even if not used in INSERT/UPDATE)
        cmd.Parameters.AddWithValue("@Username", user.Username ?? "");
        cmd.Parameters.AddWithValue("@Password", user.Password ?? ""); // ⚠️ Hash in real app!
        cmd.Parameters.AddWithValue("@Email", user.Email ?? "");
        cmd.Parameters.AddWithValue("@Name", user.FullName ?? "");      // ✅ FullName → Name
        cmd.Parameters.AddWithValue("@Role", user.Role ?? "");
        cmd.Parameters.AddWithValue("@Department", user.Department ?? "");
        cmd.Parameters.AddWithValue("@Phone_Number", user.Phone ?? ""); // ✅ Phone → Phone_Number

        if (user.Id != 0)
        {
            cmd.Parameters.AddWithValue("@User_ID", user.Id); // Only used in UPDATE
        }

        cmd.ExecuteNonQuery();
    }
}