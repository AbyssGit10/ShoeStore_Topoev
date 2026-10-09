using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ShoeStore_Topoev
{
    /// <summary>
    /// Логика взаимодействия для AddEditTovarWindow.xaml
    /// </summary>
    public partial class AddEditTovarWindow : Window
    {
        private Products _currentProduct;
        private bool _isNew;
        private string _selectedImagePath = null;
        private string _oldPhotoPath = null;

        private readonly string _imagesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProductImages");
        public AddEditTovarWindow(Products product)
        {
            InitializeComponent();

            if (!Directory.Exists(_imagesFolder))
                Directory.CreateDirectory(_imagesFolder);

            if (product == null)
            {
                _currentProduct = new Products();
                _currentProduct.ProductID = GetNextID();
                _currentProduct.CurrentDiscount = 0;
                _currentProduct.QuantityInStock = 0;

                var context = ShoeStore_TopoevEntities.GetContext();
                var firstType = context.ShoeTypes.FirstOrDefault();
                if (firstType != null)
                    _currentProduct.ShoeTypeID = firstType.ShoeTypeID;
                _isNew = true;
            }
            else
            {
                _currentProduct = product;
                _oldPhotoPath = product.Photo;
                _isNew = false;
                
                if (!string.IsNullOrWhiteSpace(product.Photo))
                {
                    string fullPath = Path.Combine(_imagesFolder, product.Photo);
                    if (File.Exists(fullPath))
                        ImgPhoto.Source = new BitmapImage(new Uri(fullPath));
                }
            }
            DataContext = _currentProduct;
            txbID.Text = _currentProduct.ProductID.ToString();
            LoadComboBoxes();
        }

        private int GetNextID()
        {
            var context = ShoeStore_TopoevEntities.GetContext();
            if (!context.Products.Any()) return 1;
            return context.Products.Max(p => p.ProductID) + 1;
        }

        private void LoadComboBoxes()
        {
            var context = ShoeStore_TopoevEntities.GetContext();

            cmbGender.ItemsSource = context.Genders.ToList();
            cmbManufacturer.ItemsSource = context.Manufacturers.ToList();
            cmbSupplier.ItemsSource = context.Suppliers.ToList();
        }

        private void NumberValidation(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0) && e.Text != "," && e.Text != ".";
        }
        private void SelectedImageBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image Files |*.jpg; *.jpeg; *.png; *.bmp; *.gif"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _selectedImagePath = dialog.FileName;

                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(_selectedImagePath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    ImgPhoto.Source = bitmap;

                    tbPhotoPath.Text = Path.GetFileName(_selectedImagePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки: {ex.Message}");
                }
            }    
        }

        private void saveBtn_Click( object sender, RoutedEventArgs e )
        {
            var errors = new System.Text.StringBuilder();

            if (string.IsNullOrWhiteSpace(_currentProduct.Article))
                errors.AppendLine("• Артикул не заполнен");
            if (string.IsNullOrWhiteSpace(_currentProduct.UnitOfMeasurement))
                errors.AppendLine("• Единица измерения не заполнена");
            if (_currentProduct.Price < 0)
                errors.AppendLine("• Цена не может быть отрицательной");
            if (_currentProduct.QuantityInStock < 0)
                errors.AppendLine("• Количество не может быть отрицательным");
            if (_currentProduct.GenderID == 0)
                errors.AppendLine("• Не выбрана категория");
            if (_currentProduct.ManufacturerID == 0)
                errors.AppendLine("• Не выбран производитель");
            if (_currentProduct.SupplierID == 0)
                errors.AppendLine("• Не выбран поставщик");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Ошибка заполнения");
                return;
            }

            var context = ShoeStore_TopoevEntities.GetContext();

            if (_selectedImagePath != null)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(_oldPhotoPath))
                    {
                        string oldFullPath = Path.Combine(_imagesFolder, _oldPhotoPath);
                        if (File.Exists(oldFullPath))
                            File.Delete(oldFullPath);
                    }

                    string fileName = $"{_currentProduct.ProductID}_{Guid.NewGuid().ToString().Substring(0, 8)}.jpg";
                    string newPath = Path.Combine(_imagesFolder, fileName);

                    SaveResizedImage(_selectedImagePath, newPath, 300, 200);
                    _currentProduct.Photo = fileName;

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения изображения: {ex.Message}");
                    return;
                }
            }

            try
            {
                if (_isNew)
                {
                    context.Products.Add(_currentProduct);
                    context.Entry(_currentProduct).State = System.Data.Entity.EntityState.Added;
                }
                else
                {
                    if (context.Entry(_currentProduct).State == System.Data.Entity.EntityState.Detached)
                        context.Products.Attach(_currentProduct);
                    context.Entry(_currentProduct).State = System.Data.Entity.EntityState.Modified;
                }
                context.SaveChanges();
                MessageBox.Show("Запись сохранена!");
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.InnerException?.Message ?? ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Ошибка: {inner}");
            }
        }

        private void SaveResizedImage(string sourcePath, string destPath, int maxWidth, int maxHeight)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(sourcePath);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            double ratioX = (double)maxWidth / bitmap.PixelWidth;
            double ratoY = (double)maxHeight / bitmap.PixelHeight;
            double ratio = Math.Min(ratioX, ratoY);
            if (ratio > 0) ratio = 1;

            int newWidth = (int)(bitmap.PixelWidth * ratio);
            int newHeight = (int)(bitmap.PixelHeight * ratio);

            var transformed = new TransformedBitmap(bitmap,
                new System.Windows.Media.ScaleTransform(ratio, ratio));

            var encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(transformed));

            using (var fs = new FileStream(destPath, FileMode.Create))
            {
                encoder.Save(fs);
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
