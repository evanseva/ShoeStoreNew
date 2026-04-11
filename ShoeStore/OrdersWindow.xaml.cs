using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace ShoeStore
{
    public partial class OrdersWindow : Window
    {
        DatabaseHelper db = new DatabaseHelper();
        string userRole;

        public OrdersWindow(string role)
        {
            InitializeComponent();
            userRole = role;
            if (role != "admin")
                btnAdd.Visibility = Visibility.Collapsed;
            LoadOrders();
        }

        private void LoadOrders()
        {
            DataTable dt = db.ExecuteQuery("SELECT * FROM orders ORDER BY id");
            dgOrders.ItemsSource = dt.DefaultView;
        }

        private void DgOrders_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (userRole == "admin" && dgOrders.SelectedItem != null)
            {
                DataRowView row = dgOrders.SelectedItem as DataRowView;
                int orderId = Convert.ToInt32(row["id"]);
                OrderEditWindow editWin = new OrderEditWindow(orderId);
                editWin.ShowDialog();
                LoadOrders();
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            OrderEditWindow editWin = new OrderEditWindow(null);
            editWin.ShowDialog();
            LoadOrders();
        }
    }
}