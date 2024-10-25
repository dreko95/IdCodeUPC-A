using System.ComponentModel;

namespace IdCode.ViewModels;

public class ProductInfoViewModel : INotifyPropertyChanged {
    private string _manufacturerId;
    private string _manufacturerName;
    private string _price;
    private string _productId;
    private string _productName;

    public ProductInfoViewModel(string manufacturerId, string manufacturerName, string price, string productId, string productName) {
        _manufacturerId = manufacturerId;
        _manufacturerName = manufacturerName;
        _price = price;
        _productId = productId;
        _productName = productName;
    }

    public string ManufacturerId {
        get => _manufacturerId;
        set {
            _manufacturerId = value;
            OnPropertyChanged(nameof(ManufacturerId));
        }
    }

    public string ManufacturerName {
        get => _manufacturerName;
        set {
            _manufacturerName = value;
            OnPropertyChanged(nameof(ManufacturerName));
        }
    }

    public string ProductId {
        get => _productId;
        set {
            _productId = value;
            OnPropertyChanged(nameof(ProductId));
        }
    }

    public string ProductName {
        get => _productName;
        set {
            _productName = value;
            OnPropertyChanged(nameof(ProductName));
        }
    }

    public string Price {
        get => _price;
        set {
            _price = value;
            OnPropertyChanged(nameof(Price));
        }
    }


    #region PropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion
}