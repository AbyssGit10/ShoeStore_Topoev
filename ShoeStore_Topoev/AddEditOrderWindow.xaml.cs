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
    /// Логика взаимодействия для AddEditOrderWindow.xaml
    /// </summary>
    public partial class AddEditOrderWindow : Window
    {
        private Orders _currentOrder;
        private Users _admin;
        private bool _isNew;

        public AddEditOrderWindow(Orders order, Users admin)
        {
            InitializeComponent();
            _admin = admin;

            if (order == null)
            {
                _currentOrder = new Orders();
                _currentOrder.OrderID = GetNextID();
                _currentOrder.OrderDate = DateTime.Now;
                _currentOrder.DeliveryDate = DateTime.Now.AddDays(7);
                _currentOrder.UserID = admin.UserID;
                _currentOrder.PickupCode = GeneratePickupCode();
                _isNew = true;
            }
            else
            {
                _currentOrder = order;
                _isNew = false;
            }

            DataContext = _currentOrder;
            LoadComboBoxes();
            TxbID.Text = _currentOrder.OrderID.ToString();
        }

        private int GetNextID()
        {
            var context = ShoeStore_TopoevEntities.GetContext();
            if (!context.Orders.Any()) return 1;
            return context.Orders.Max(o => o.OrderID) + 1;
        }

        private string GeneratePickupCode()
        {
            var context = ShoeStore_TopoevEntities.GetContext();
            return (900 + GetNextID()).ToString();
        }

        private void LoadComboBoxes()
        {
            var context = ShoeStore_TopoevEntities.GetContext();

            CmbStatus.ItemsSource = context.OrderStatuses.ToList();
            CmbPoint.ItemsSource = context.PickupPoints.ToList();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            var errors = new System.Text.StringBuilder();

            if (string.IsNullOrWhiteSpace(_currentOrder.OrderNumber))
                errors.AppendLine("• Артикул заказа не заполнен");
            if (_currentOrder.StatusID == 0)
                errors.AppendLine("• Не выбран статус");
            if (_currentOrder.PointID == 0)
                errors.AppendLine("• Не выбран пункт выдачи");
            if (_currentOrder.OrderDate == null)
                errors.AppendLine("• Не указана дата заказа");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Ошибки заполнения");
                return;
            }

            var context = ShoeStore_TopoevEntities.GetContext();

            try
            {
                if (_isNew)
                {
                    context.Orders.Add(_currentOrder);
                    context.Entry(_currentOrder).State = System.Data.Entity.EntityState.Added;
                }
                else
                {
                    if (context.Entry(_currentOrder).State == System.Data.Entity.EntityState.Detached)
                        context.Orders.Attach(_currentOrder);
                    context.Entry(_currentOrder).State = System.Data.Entity.EntityState.Modified;
                }

                context.SaveChanges();
                MessageBox.Show("Заказ сохранён!");
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.InnerException?.Message ?? ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Ошибка: {inner}");
            }
        }
    }
}
