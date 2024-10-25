using System.ComponentModel;
using System.Windows;
using IdCode.Classes;
using IdCode.Models;

namespace IdCode;

/// <summary>
///     Interaction logic for AddNewProductDialog.xaml
/// </summary>
public partial class AddNewProductDialog : INotifyPropertyChanged {
    private readonly DbProvider _dbProvider;
    private readonly CodeModel _selectedCode;
    private string _productName;
    private string _productPrice;
    private string _manufacturerName;

    public AddNewProductDialog(CodeModel selectedCode, DbProvider dbProvider) {
        _dbProvider = dbProvider;
        _selectedCode = selectedCode;
        InitializeComponent();
        ManufacturerIdTextBox.Text = _selectedCode.ManufacturerId.ToString();
        ProductIdTextBox.Text = _selectedCode.ProductId.ToString();
        LoadManufacturerName();
    }

    public string ManufacturerName {
        get => _manufacturerName;
        set {
            _manufacturerName = value;
            OnPropertyChanged(nameof(ManufacturerName));
        }
    }

    public string ProductName {
        get => _productName;
        set {
            _productName = value;
            OnPropertyChanged(nameof(ProductName));
        }
    }

    public string ProductPrice {
        get => _productPrice;
        set {
            _productPrice = value;
            OnPropertyChanged(nameof(ProductPrice));
        }
    }

    private void LoadManufacturerName() {
        var manufacturer = _dbProvider.GetManufacturerById(_selectedCode.ManufacturerId);
        if (manufacturer == null) {
            ManufacturerNameTextBox.IsReadOnly = false;
            return;
        }

        ManufacturerNameTextBox.Text = manufacturer.ManufacturerName;
        ManufacturerNameTextBox.IsReadOnly = true;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e) {
        var manufacturerName = ManufacturerNameTextBox.Text;
        var productName = ProductNameTextBox.Text;
        if (decimal.TryParse(ProductPriceTextBox.Text, out var productPrice) && !string.IsNullOrWhiteSpace(manufacturerName) &&
            !string.IsNullOrWhiteSpace(productName)) {
            if (!ManufacturerNameTextBox.IsReadOnly)
                _dbProvider.SaveManufacturer(_selectedCode.ManufacturerId, manufacturerName);

            _dbProvider.SaveProduct(_selectedCode.ProductId, productName, productPrice, _selectedCode.ManufacturerId);
            MessageBox.Show("Новий продукт збережено успішно.");
            this.Close();
        }
        else
            MessageBox.Show("Данні заповнено не коректно! Перевірте данні.");
    }


    #region PropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion
}