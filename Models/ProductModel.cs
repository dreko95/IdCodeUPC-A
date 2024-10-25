namespace IdCode.Models;

public class ProductModel {
    public ProductModel(int productId, string productName, double price, int manufacturerId, string manufacturerName) {
        ProductId = productId;
        ProductName = productName;
        Price = price;
        ManufacturerId = manufacturerId;
        ManufacturerName = manufacturerName;
    }

    public int ProductId { get; set; }
    public int ManufacturerId { get; set; }
    public string ManufacturerName { get; set; }
    public string ProductName { get; set; }
    public double Price { get; set; }
}