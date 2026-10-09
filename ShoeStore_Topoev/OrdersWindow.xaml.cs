using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace ShoeStore_Topoev
{
    /// <summary>
    /// Логика взаимодействия для OrdersWindow.xaml
    /// </summary>
    public partial class OrdersWindow : Window
    {
        private Users _currentUser;
        private bool _isEditOpen = false;
        public OrdersWindow(Users user)
        {
            InitializeComponent();
            _currentUser = user;

            if (user != null)
            {
                txbUserName.Text = user.FullName;

                if (user.RoleID == (int)UserRole.Admin)
                    AdminPanel.Visibility = Visibility.Visible;
            }
            LoadOrders();
        }

        private void LoadOrders()
        {
            var context = ShoeStore_TopoevEntities.GetContext();
            ListOrders.ItemsSource = context.Orders.ToList();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnAddOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditOpen)
            {
                MessageBox.Show("Окно редактирования уже открыто!");
                return;
            }
            _isEditOpen = true;
            var window = new AddEditOrderWindow(null, _currentUser);
            window.Owner = this;
            if (window.ShowDialog() == true)
            {
                ListOrders.ItemsSource = null;
                LoadOrders();
            }
            _isEditOpen = false;
        }
        private void ListOrders_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null || _currentUser.RoleID != (int)UserRole.Admin)
                return;
            if (_isEditOpen)
            {
                MessageBox.Show("Окно редактирования уже открыто!");
                return;
            }

            var order = ListOrders.SelectedItem as Orders;
            if (order == null) return;

            _isEditOpen = true;
            var window = new AddEditOrderWindow(order, _currentUser);
            window.Owner = this;
            if (window.ShowDialog() == true)
            {
                ListOrders.ItemsSource = null;
                LoadOrders();
            }
            _isEditOpen = false;
        }

        private void BtnDeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            var order = ListOrders.SelectedItem as Orders;
            if (order == null)
            {
                MessageBox.Show("Выберите заказ");
                return;
            }

            if (MessageBox.Show($"Удалить заказ {order.OrderNumber}?",
                    "Подтверждение", MessageBoxButton.YesNo,
                    MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            var context = ShoeStore_TopoevEntities.GetContext();
            
            var itemsInOrder = context.OrderItems
                .Where(oi =>  oi.OrderID == order.OrderID).ToList();

            if (itemsInOrder.Any())
            {
                if (MessageBox.Show($"В заказе {itemsInOrder.Count} позиций. Удалить их тоже?",
                        "Внимание", MessageBoxButton.YesNo,
                        MessageBoxImage.Warning) != MessageBoxResult.Yes)
                    return;

                context.OrderItems.RemoveRange(itemsInOrder);
            }

            try
            {
                context.Orders.Remove(order);
                context.SaveChanges();
                MessageBox.Show("Заказ удален");
                LoadOrders();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}
