using System.Data;
using System.Windows;

namespace ShoeStore
{
    public partial class OrdersWindow : Window
    {
        DatabaseHelper db = new DatabaseHelper();

        public OrdersWindow()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void LoadOrders()
        {
            DataTable dt = db.ExecuteQuery("SELECT * FROM orders ORDER BY id");
            dgOrders.ItemsSource = dt.DefaultView;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            OrderEditWindow editWin = new OrderEditWindow(null);
            editWin.ShowDialog();
            LoadOrders();
        }
    }
}