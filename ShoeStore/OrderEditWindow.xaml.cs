using System;
using System.Data;
using System.Windows;
using Npgsql;

namespace ShoeStore
{
    public partial class OrderEditWindow : Window
    {
        DatabaseHelper db = new DatabaseHelper();
        int? orderId;

        public OrderEditWindow(int? id)
        {
            InitializeComponent();
            orderId = id;
            if (id.HasValue)
            {
                LoadOrder(id.Value);
                btnDelete.Visibility = Visibility.Visible;
            }
            else
            {
                dpOrderDate.SelectedDate = DateTime.Today;
                btnDelete.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadOrder(int id)
        {
            DataTable dt = db.ExecuteQuery("SELECT * FROM orders WHERE id = @id",
                new NpgsqlParameter[] { new NpgsqlParameter("@id", id) });
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                txtArticle.Text = row["article"].ToString();
                cmbStatus.Text = row["status"].ToString();
                txtAddress.Text = row["pickup_address"].ToString();
                dpOrderDate.SelectedDate = row["order_date"] != DBNull.Value ? Convert.ToDateTime(row["order_date"]) : (DateTime?)null;
                dpIssueDate.SelectedDate = row["issue_date"] != DBNull.Value ? Convert.ToDateTime(row["issue_date"]) : (DateTime?)null;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtArticle.Text))
            {
                MessageBox.Show("Введите артикул", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (orderId.HasValue)
                {
                    string query = @"UPDATE orders SET article=@art, status=@stat, pickup_address=@addr,
                                    order_date=@od, issue_date=@isd WHERE id=@id";
                    NpgsqlParameter[] pars = {
                        new NpgsqlParameter("@art", txtArticle.Text),
                        new NpgsqlParameter("@stat", cmbStatus.Text),
                        new NpgsqlParameter("@addr", txtAddress.Text),
                        new NpgsqlParameter("@od", dpOrderDate.SelectedDate ?? (object)DBNull.Value),
                        new NpgsqlParameter("@isd", dpIssueDate.SelectedDate ?? (object)DBNull.Value),
                        new NpgsqlParameter("@id", orderId.Value)
                    };
                    db.ExecuteNonQuery(query, pars);
                }
                else
                {
                    string query = @"INSERT INTO orders (article, status, pickup_address, order_date, issue_date)
                                    VALUES (@art, @stat, @addr, @od, @isd)";
                    NpgsqlParameter[] pars = {
                        new NpgsqlParameter("@art", txtArticle.Text),
                        new NpgsqlParameter("@stat", cmbStatus.Text),
                        new NpgsqlParameter("@addr", txtAddress.Text),
                        new NpgsqlParameter("@od", dpOrderDate.SelectedDate ?? (object)DBNull.Value),
                        new NpgsqlParameter("@isd", dpIssueDate.SelectedDate ?? (object)DBNull.Value)
                    };
                    db.ExecuteNonQuery(query, pars);
                }
                MessageBox.Show("Сохранено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Удалить заказ?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                db.ExecuteNonQuery("DELETE FROM orders WHERE id = @id",
                    new NpgsqlParameter[] { new NpgsqlParameter("@id", orderId.Value) });
                MessageBox.Show("Удалено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}