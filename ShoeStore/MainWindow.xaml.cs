using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Npgsql;

namespace ShoeStore
{
    public partial class MainWindow : Window
    {
        DatabaseHelper db = new DatabaseHelper();
        User currentUser;
        string currentSearch = "";
        string currentSupplier = "";
        string currentSort = "";

        public MainWindow(User user)
        {
            InitializeComponent();
            currentUser = user;
            txtUserInfo.Text = user.FullName + " (" + user.Role + ")";

            if (user.Role == "manager" || user.Role == "admin")
            {
                panelFilters.Visibility = Visibility.Visible;
                btnOrders.Visibility = Visibility.Visible;
                LoadSuppliers();
            }

            if (user.Role == "admin")
            {
                btnAddProduct.Visibility = Visibility.Visible;
            }

            LoadProducts();
        }

        private void LoadSuppliers()
        {
            try
            {
                DataTable dt = db.ExecuteQuery("SELECT DISTINCT supplier FROM products WHERE supplier IS NOT NULL AND supplier != ''");
                cmbSupplier.Items.Clear();
                cmbSupplier.Items.Add("Все поставщики");
                foreach (DataRow row in dt.Rows)
                {
                    string supplier = row["supplier"].ToString();
                    if (!string.IsNullOrEmpty(supplier))
                        cmbSupplier.Items.Add(supplier);
                }
                cmbSupplier.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                txtStatus.Text = "Ошибка: " + ex.Message;
            }
        }

        private void LoadProducts()
        {
            try
            {
                string query = @"SELECT p.id, p.name, COALESCE(c.name, 'Нет') as category, 
                                        COALESCE(m.name, 'Нет') as manufacturer, 
                                        p.supplier, p.price, p.quantity, p.discount,
                                        CASE WHEN p.discount > 0 THEN p.price * (100 - p.discount)/100 ELSE p.price END as final_price
                                 FROM products p
                                 LEFT JOIN categories c ON p.category_id = c.id
                                 LEFT JOIN manufacturers m ON p.manufacturer_id = m.id
                                 WHERE 1=1";

                if (!string.IsNullOrEmpty(currentSearch))
                    query += " AND (p.name ILIKE '%" + currentSearch.Replace("'", "''") + "%')";

                if (!string.IsNullOrEmpty(currentSupplier) && currentSupplier != "Все поставщики")
                    query += " AND p.supplier = '" + currentSupplier.Replace("'", "''") + "'";

                if (currentSort == "asc")
                    query += " ORDER BY p.quantity ASC";
                else if (currentSort == "desc")
                    query += " ORDER BY p.quantity DESC";
                else
                    query += " ORDER BY p.id";

                DataTable dt = db.ExecuteQuery(query);
                dgProducts.ItemsSource = dt.DefaultView;
                txtStatus.Text = "Загружено товаров: " + dt.Rows.Count;
            }
            catch (Exception ex)
            {
                txtStatus.Text = "Ошибка: " + ex.Message;
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentSearch = txtSearch.Text;
            LoadProducts();
        }

        private void CmbSupplier_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSupplier.SelectedItem != null)
            {
                currentSupplier = cmbSupplier.SelectedItem.ToString();
                LoadProducts();
            }
        }

        private void Sort_Changed(object sender, RoutedEventArgs e)
        {
            if (rbAsc.IsChecked == true) currentSort = "asc";
            else if (rbDesc.IsChecked == true) currentSort = "desc";
            else currentSort = "";
            LoadProducts();
        }

        private void DgProducts_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (currentUser.Role != "admin")
            {
                MessageBox.Show("Только администратор может редактировать товары", "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dgProducts.SelectedItem != null)
            {
                DataRowView row = dgProducts.SelectedItem as DataRowView;
                if (row != null)
                {
                    int productId = Convert.ToInt32(row["id"]);
                    ProductEditWindow editWin = new ProductEditWindow(productId);
                    editWin.ShowDialog();
                    LoadProducts();
                }
            }
        }

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            ProductEditWindow editWin = new ProductEditWindow(null);
            editWin.ShowDialog();
            LoadProducts();
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            OrdersWindow ordersWin = new OrdersWindow(currentUser.Role);
            ordersWin.ShowDialog();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }
}