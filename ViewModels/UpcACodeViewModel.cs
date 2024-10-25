using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using IdCode.Classes;
using IdCode.Models;
using Microsoft.Win32;
using static IdCode.Helpers.Constants;

namespace IdCode.ViewModels {
    public class UpcACodeViewModel : INotifyPropertyChanged {
        private CodeModel _selectedCode;
        private string _settedCodeText;
        private BitmapImage _codeImage;
        private Bitmap _lastImageSaved;
        private DbProvider _dbProvider;
        private ProductInfoViewModel _productInfo;

        public UpcACodeViewModel() {
            _dbProvider = new DbProvider();
            GenerateCommand = new RelayCommand(GenerateExecute);
            LoadFileCommand = new RelayCommand(LoadFileExecute);
            DecodeBarcodeCommand = new RelayCommand(DecodeBarcodeExecute);
            SaveImageAsJpegFileCommand = new RelayCommand(SaveImageAsJpegFileExecute);
            AddNewProductCommand = new RelayCommand(AddNewProductExecute);
        }

        public string SettedCodeText {
            get => _settedCodeText;
            set {
                if (_settedCodeText == value)
                    return;

                _settedCodeText = value;
                OnPropertyChanged(nameof(SettedCodeText));
            }
        }

        public CodeModel SelectedCode {
            get => _selectedCode;
            set {
                _selectedCode = value;
                OnPropertyChanged(nameof(SelectedCode));
            }
        }

        public BitmapImage CodeImage {
            get => _codeImage;
            set {
                _codeImage = value;
                OnPropertyChanged(nameof(CodeImage));
                OnPropertyChanged(nameof(DecodeVisibility));
                OnPropertyChanged(nameof(CodeImageVisibility));
            }
        }

        public ProductInfoViewModel ProductInfo {
            get => _productInfo;
            set {
                _productInfo = value;
                OnPropertyChanged(nameof(ProductInfo));
                OnPropertyChanged(nameof(DecodeVisibility));
                OnPropertyChanged(nameof(ProductInfoVisibility)); 
                OnPropertyChanged(nameof(AddNewProductVisibility));
            }
        }

        public Visibility CodeImageVisibility => CodeImage != null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility DecodeVisibility => ProductInfo == null && CodeImage != null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ProductInfoVisibility => ProductInfo != null ? Visibility.Visible : Visibility.Collapsed;
        public Visibility AddNewProductVisibility => ProductInfo != null && string.IsNullOrWhiteSpace(ProductInfo.ProductName) ? Visibility.Visible : Visibility.Collapsed;


        #region Commands

        public ICommand GenerateCommand { get; }
        public ICommand LoadFileCommand { get; }
        public ICommand DecodeBarcodeCommand { get; }
        public ICommand SaveImageAsJpegFileCommand { get; }
        public ICommand AddNewProductCommand { get; }

        public void GenerateExecute(object obj) {
            if (SettedCodeText.Length != 11 || !long.TryParse(SettedCodeText, out _)) {
                MessageBox.Show("Будь ласка, введіть 11 цифр.");
                return;
            }

            var checkDigit = CalculateCheckDigit(SettedCodeText);
            SelectedCode = new CodeModel(SettedCodeText.Substring(0, 1),
                                         SettedCodeText.Substring(1, 5),
                                         SettedCodeText.Substring(6, 5),
                                         checkDigit);
            var encodedBytes = EncodeUpcA(SettedCodeText + checkDigit);
            var barcodeBitmap = CreateUpcAImage(encodedBytes);
            var product = _dbProvider.GetCodeInfo(SelectedCode.ProductId, SelectedCode.ManufacturerId);
            if (product == null)
                UpdateProductInfo(SelectedCode.ManufacturerId, null, null, SelectedCode.ProductId, null);
            else
                UpdateProductInfo(product.ManufacturerId,
                                  product.ManufacturerName,
                                  product.Price.ToString(CultureInfo.InvariantCulture),
                                  product.ProductId,
                                  product.ProductName);

            SetCodeImage(barcodeBitmap);
        }

        private string CalculateCheckDigit(string input) {
            var sum = 0;
            for (var i = 0; i < input.Length; i++) {
                var digit = int.Parse(input[i]
                                          .ToString());
                sum += i % 2 == 0 ? digit * 3 : digit;
            }

            var checkDigit = (10 - sum % 10) % 10;
            return checkDigit.ToString();
        }

        private byte[] EncodeUpcA(string input) {
            var prefix = input[0];
            var manufacturer = input.Substring(1, 5);
            var product = input.Substring(6, 5);
            var checkDigit = input[11];

            var result = H1.Concat(CodeSetADictionary[prefix]);
            result = manufacturer.Aggregate(result, (current, item) => current.Concat(CodeSetADictionary[item]));

            result = result.Concat(H4);
            result = product.Aggregate(result, (current, item) => current.Concat(CodeSetBDictionary[item]));

            result = result.Concat(CodeSetBDictionary[checkDigit]);
            result = result.Concat(H1);
            return result.ToArray();
        }

        private Bitmap CreateUpcAImage(byte[] encodedBytes) {
            const int width = 190;
            const int height = 60;
            var bitmap = new Bitmap(width, height);

            using var g = Graphics.FromImage(bitmap);
            g.Clear(Color.White);

            for (var i = 0; i < encodedBytes.Length; i++) {
                var color = encodedBytes[i] == 1 ? Color.Black : Color.White;
                using var pen = new Pen(color, 2);
                g.DrawLine(pen, i * 2, 0, i * 2, height);
            }

            return bitmap;
        }

        private BitmapImage BitmapToImageSource(Bitmap bitmap) {
            using (var memory = new MemoryStream()) {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        private void DecodeBarcodeExecute(object obj) {
            try {
                SelectedCode = null;
                for (var i = 2; i < 60; i++) {
                    var resultBytes = DecodeUpcAImage(_lastImageSaved, i);
                    if (!TryParseUpcACode(resultBytes, out var codeModel))
                        continue;

                    SelectedCode = codeModel;
                    var product = _dbProvider.GetCodeInfo(codeModel.ProductId, codeModel.ManufacturerId);
                    if (product == null) {
                        UpdateProductInfo(SelectedCode.ManufacturerId,
                                          null,
                                          null,
                                          SelectedCode.ProductId,
                                          null);
                        break;
                    }

                    UpdateProductInfo(SelectedCode.ManufacturerId,
                                      product.ManufacturerName,
                                      product.Price.ToString(CultureInfo.InvariantCulture),
                                      SelectedCode.ProductId,
                                      product.ProductName);
                    break;
                }
            }
            catch (Exception e) {
                MessageBox.Show("Виникла неочікувана помилка! Зверніться до адміністратора.");
            }
        }

        private byte[] DecodeUpcAImage(Bitmap bitmap, int analyzeLevelHeight) {
            const int brightnessThreshold = 128;
            const int width = 190;

            if (analyzeLevelHeight >= bitmap.Height)
                return null;

            var decodedBytes = new byte[95];

            for (var i = 0; i < width; i += 2) {
                var pixelColor = bitmap.GetPixel(i, analyzeLevelHeight);
                var brightness = (int)(0.3 * pixelColor.R + 0.6 * pixelColor.G + 0.114 * pixelColor.B);

                if (brightness < brightnessThreshold)
                    decodedBytes[i / 2] = 1;
                else
                    decodedBytes[i / 2] = 0;
            }

            return decodedBytes;
        }

        private void LoadFileExecute(object obj) {
            var openFileDialog = new OpenFileDialog {
                Filter = "JPEG files (*.jpeg;*.jpg)|*.jpeg;*.jpg|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() != true)
                return;

            var filePath = openFileDialog.FileName;
            SettedCodeText = string.Empty;
            ProductInfo = null;
            SetCodeImage(new Bitmap(filePath));
        }

        private void SetCodeImage(Bitmap bitmap) {
            _lastImageSaved = bitmap;
            CodeImage = BitmapToImageSource(bitmap);
        }

        private bool TryParseUpcACode(byte[] arrayBytes, out CodeModel code) {
            code = null;
            if (arrayBytes.Length != 95)
                return false;

            byte[] tmp = new byte[3];
            Array.Copy(arrayBytes, 0, tmp, 0, 3);
            if (!H1.SequenceEqual(tmp))
                return false;

            var firstPart = string.Empty;
            for (var i = 3; i < 45; i = i + 7) {
                var codeArray = new byte[7];
                Array.Copy(arrayBytes, i, codeArray, 0, 7);
                if(!CodeSetADictionary.Any(x => codeArray.SequenceEqual(x.Value)))
                    return false;

                var item = CodeSetADictionary.First(x => codeArray.SequenceEqual(x.Value));
                firstPart += item.Key;
            }

            byte[] tmpH4 = new byte[5];
            Array.Copy(arrayBytes, 45, tmpH4, 0, 5);
            if (!H4.SequenceEqual(tmpH4))
                return false;


            var secondPart = string.Empty;
            for (var i = 50; i < 92; i = i + 7)
            {
                var codeArray = new byte[7];
                Array.Copy(arrayBytes, i, codeArray, 0, 7);
                if (!CodeSetBDictionary.Any(x => codeArray.SequenceEqual(x.Value)))
                    return false;

                var item = CodeSetBDictionary.First(x => codeArray.SequenceEqual(x.Value));
                secondPart += item.Key;
            }


            Array.Copy(arrayBytes, 92, tmp, 0, 3);
            if (!H1.SequenceEqual(tmp))
                return false;

            code = new CodeModel(firstPart[0].ToString(),
                                 firstPart.Substring(1),
                                 secondPart.Substring(0, 5),
                                 secondPart[5].ToString());
            if (CheckUpcACodeData(code))
                return true;

            code = null;
            return false;
        }

        // Перевірка даних штрихкоду Upc-A по контрольній сумі
        private bool CheckUpcACodeData(CodeModel code) {
            var codeText = $"{code.Prefix}{code.ManufacturerId}{code.ProductId}";
            var r1 = (int.Parse(codeText[0].ToString()) + int.Parse(codeText[2].ToString()) + int.Parse(codeText[4].ToString()) + int.Parse(codeText[6].ToString()) + int.Parse(codeText[8].ToString()) + int.Parse(codeText[10].ToString())) *3;
            var r2 = int.Parse(codeText[1].ToString()) + int.Parse(codeText[3].ToString()) + int.Parse(codeText[5].ToString()) + int.Parse(codeText[7].ToString()) + int.Parse(codeText[9].ToString());
            var r = r1 + r2;
            var lastDigit = r % 10;
            var checkDigit = 10 - lastDigit;

            return code.CheckDigit == checkDigit;
        }

        private void SaveImageAsJpegFileExecute(object obj) {
            if (_lastImageSaved == null) {
                MessageBox.Show("Нема зображення для збереження.");
                return;
            }

            var saveFileDialog = new SaveFileDialog {
                Filter = "JPEG Image|*.jpeg",
                Title = "Save Image as JPEG"
            };

            if (saveFileDialog.ShowDialog() == true)
                _lastImageSaved.Save(saveFileDialog.FileName, ImageFormat.Jpeg);
        }

        private void AddNewProductExecute(object obj) {
            var dlg = new AddNewProductDialog(SelectedCode, _dbProvider);
            dlg.ShowDialog();
            if (dlg.DialogResult == true)
                UpdateProductInfo(SelectedCode.ManufacturerId,
                                  dlg.ManufacturerName,
                                  dlg.ProductPrice,
                                  SelectedCode.ProductId,
                                  dlg.ProductName);
        }

        // Оновлює інформацію по продукту
        private void UpdateProductInfo(int  manufacturerId, string manufacturerName, string price, int productId, string productName) {
            ProductInfo = new ProductInfoViewModel(manufacturerId.ToString(), manufacturerName, price, productId.ToString(), productName);
        }

        #endregion

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
    }
}