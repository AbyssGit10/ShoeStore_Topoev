using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace ShoeStore_Topoev
{
    /// <summary>
    /// Логика взаимодействия для MainMenuWindow.xaml
    /// </summary>
    public partial class MainMenuWindow : Window
    {
        private Users _currentUser;
        private int _supplierId = 0;
        private bool _sortAscending = false;
        private bool _sortDescending = false;
        private bool _isEditWindowOpen = false;
        public MainMenuWindow(Users user)
        {
            InitializeComponent();
            _currentUser = user;

            if (user == null)
            {
                txbUserName.Text = "Гость";
                FilterPanel.Visibility = Visibility.Collapsed;

            }
            else
            {
                txbUserName.Text = user.FullName;

                if (user.RoleID == (int)UserRole.Client)
                {
                    FilterPanel.Visibility = Visibility.Collapsed;
                    BtnOrders.Visibility = Visibility.Collapsed;
                    AdminPanel.Visibility = Visibility.Collapsed;
                }
                else if (user.RoleID == (int)UserRole.Manager)
                {
                    FilterPanel.Visibility = Visibility.Visible;
                    BtnOrders.Visibility = Visibility.Visible;
                    AdminPanel.Visibility = Visibility.Collapsed;
                }
                else if (user.RoleID == (int)UserRole.Admin)
                {
                    FilterPanel.Visibility = Visibility.Visible;
                    BtnOrders.Visibility = Visibility.Visible;
                    AdminPanel.Visibility = Visibility.Visible;
                }

            }
            LoadSuppliers();
            Filtr();
        }

        private void LoadSuppliers()
        {
            var context = ShoeStore_TopoevEntities.GetContext();
            var list = new List<SupplierItem>
            {
                new SupplierItem { SupplierID = 0, SupplierName = "Все поставщики"}
            };

            foreach (var s in context.Suppliers.ToList())
                list.Add(new SupplierItem { SupplierID = s.SupplierID, SupplierName = s.SupplierName });

            CmbSupplier.ItemsSource = list;
            CmbSupplier.DisplayMemberPath = "SupplierName";
            CmbSupplier.SelectedValuePath = "SupplierID";
            CmbSupplier.SelectedIndex = 0;
        }

        private void Filtr()
        {
            var context = ShoeStore_TopoevEntities.GetContext();
            var SearchList = context.Products.ToList();

            if (CmbSupplier.SelectedIndex > 0)
                SearchList = SearchList.Where(x => x.SupplierID == _supplierId).ToList();

            if (!string.IsNullOrWhiteSpace(txbSearch.Text))
            {
                string search = txbSearch.Text.ToLower();
                SearchList = SearchList.Where(x =>
                (x.Article ?? "").ToLower().Contains(search) ||
                (x.Description ?? "").ToLower().Contains(search) ||
                (x.ShoeTypes.ShoeTypeName ?? "").ToLower().Contains(search) ||
                (x.Manufacturers.ManufacturerName ?? "").ToLower().Contains(search) ||
                (x.Suppliers.SupplierName ?? "").ToLower().Contains(search)
                ).ToList();
            }

            if (_sortAscending)
                SearchList = SearchList.OrderBy(x => x.QuantityInStock).ToList();
            else if (_sortDescending)
                SearchList = SearchList.OrderByDescending(x => x.QuantityInStock).ToList();
            ListTovar.ItemsSource = SearchList.ToList();
        }

        private void ListTovar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_currentUser == null || _currentUser.RoleID != (int)UserRole.Admin)
                return;

            if (_isEditWindowOpen)
            {
                MessageBox.Show("Окно редактирования уже открыто!");
                return;
            }

            var selecetedProduct = ListTovar.SelectedItem as Products;
            if (selecetedProduct == null) return;

            _isEditWindowOpen = true;
            var window = new AddEditTovarWindow(selecetedProduct);
            window.Owner = this;
            window.ShowDialog();
            _isEditWindowOpen = false;

            Filtr();
        }

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditWindowOpen)
            {
                MessageBox.Show("Окно редактирования уже открыто!");
                return;
            }

            _isEditWindowOpen = true;
            var window = new AddEditTovarWindow(null);
            window.Owner = this;
            window.ShowDialog();
            _isEditWindowOpen = false;

            Filtr();
        }

        private void BtnDeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            var product = ListTovar.SelectedItem as Products;
            if (product == null)
            {
                MessageBox.Show("Выберите товар");
                return;
            }

            var context = ShoeStore_TopoevEntities.GetContext();

            bool inOrders = context.OrderItems.Any(oi => oi.ProductID == product.ProductID);
            if (inOrders)
            {
                MessageBox.Show("Товар присутствует в заказе, удалить нельзя!");
                return;
            }

            if (MessageBox.Show($"Удалить товар «{product.Article}»?",
                    "Подтверждение", MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                if (!string.IsNullOrWhiteSpace(product.Photo))
                {
                    string photoPath = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory, "ProductImages", product.Photo);
                    if (File.Exists(photoPath))
                        File.Delete(photoPath);
                }

                context.Products.Remove(product);
                context.SaveChanges();
                MessageBox.Show("Товар удалён");
                Filtr();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void txbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            Filtr();
        }
        private void CmbSupplier_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbSupplier.SelectedValue == null) return;
            _supplierId = (int)CmbSupplier.SelectedValue;
            Filtr();
        }

        private void BtnSortAsc_Click(object sender, RoutedEventArgs e)
        {
            _sortAscending = true;
            _sortDescending = false;
            Filtr();
        }
        private void BtnSortDesc_Click(object sender, RoutedEventArgs e)
        {
            _sortAscending = false;
            _sortDescending = true;
            Filtr();
        }
        private void BtnReset_Click(object sender, EventArgs e)
        {
            txbSearch.Text = "";
            CmbSupplier.SelectedIndex = 0;
            _supplierId = 0;
            _sortAscending = false;
            _sortDescending = false;
            Filtr();
        }
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            new AuthorizationWindow().Show();
            this.Close();
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            var window = new OrdersWindow(_currentUser);
            window.Owner = this;
            window.ShowDialog();
        }
        public class SupplierItem
        {
            public int SupplierID { get; set; }
            public string SupplierName { get; set; }
        }
    }
}
