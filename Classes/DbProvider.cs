using System.Data.SqlClient;
using IdCode.Models;

namespace IdCode.Classes;

public class DbProvider {
    private readonly string _connectionString = "Data Source=DESKTOP-DOVQPAO\\MYTFS;User Id=us;Password=159753;Initial Catalog=IdCode";
        //"Server=DESKTOP-DOVQPAO\\MYTFS; Database=IdCode; Integrated Security=True;";

    //"Server=DESKTOP-DOVQPAO\\MYTFS;Database=IdCode;User Id=us;Password=159753;";
    //"Data Source=DESKTOP-DOVQPAO\\MYTFS;User Id=us;Password=159753;Initial Catalog=IdCode";

    public void SaveManufacturer(int id, string name) {
        using (var connection = new SqlConnection(_connectionString)) {
            connection.Open();

            const string query = "INSERT INTO Manufacturers (Id, Name) VALUES (@id, @name)";

            using (var command = new SqlCommand(query, connection)) {
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@name", name);

                command.ExecuteNonQuery();
            }
        }
    }

    public void SaveProduct(int id, string name, decimal price, int manufacturerId) {
        using (var connection = new SqlConnection(_connectionString)) {
            connection.Open();

            const string query = "INSERT INTO Products (Id, Name, Price, ManufacturerId) VALUES (@id, @name, @price, @manufacturerId)";

            using (var command = new SqlCommand(query, connection)) {
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@price", price);
                command.Parameters.AddWithValue("@manufacturerId", manufacturerId);

                command.ExecuteNonQuery();
            }
        }
    }

    public ManufacturerModel GetManufacturerById(int id) {
        const string query = "SELECT Name FROM Manufacturers WHERE Id = @id";
        using (var connection = new SqlConnection(_connectionString)) {
            connection.Open();
            using (var command = new SqlCommand(query, connection)) {
                command.Parameters.AddWithValue("@id", id);
                using (var reader = command.ExecuteReader()) {
                    if (reader.Read()) {
                        return new ManufacturerModel(id,
                                                     reader["Name"]
                                                         .ToString());
                    }

                    return null;
                }
            }
        }
    }

    public ProductModel GetCodeInfo(int productId, int manufacturerId) {
        const string query = @"SELECT m.Name ManufacturerName, p.Name ProductName, p.Price FROM Manufacturers m
LEFT JOIN dbo.Products p ON p.Id = @id AND p.ManufacturerId = m.Id
WHERE m.Id = @manufacturerId";
        using (var connection = new SqlConnection(_connectionString)) {
            connection.Open();
            using (var command = new SqlCommand(query, connection)) {
                command.Parameters.AddWithValue("@id", productId);
                command.Parameters.AddWithValue("@manufacturerId", manufacturerId);
                using (var reader = command.ExecuteReader()) {
                    if (!reader.Read())
                        return null;

                    var price = reader.IsDBNull(reader.GetOrdinal("Price")) ? 0 : reader.GetDouble(reader.GetOrdinal("Price"));
                    return new ProductModel(productId,
                                            reader["ProductName"]
                                                .ToString(),
                                            price,
                                            manufacturerId,
                                            reader["ManufacturerName"]
                                                .ToString()
                                           );
                }
            }
        }
    }
}