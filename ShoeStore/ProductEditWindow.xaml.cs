using System;
using System.Data;
using System.Windows;
using Npgsql;

namespace ShoeStore
{
    public partial class ProductEditWindow : Window
    {
        DatabaseHelper db = new DatabaseHelper();
        int? productId;

        public ProductEditWindow(int? id)
        {
            InitializeComponent();
            productId = id;
            LoadCategories();
            LoadManufacturers();
            if (id.HasValue)
                LoadProduct(id.Value);
            else
                btnDelete.Visibility = Visibility.Collapsed;
        }

        private void LoadCategories()
        {
            DataTable dt = db.ExecuteQuery("SELECT id, name FROM categories ORDER BY name");
            cmbCategory.ItemsSource = dt.DefaultView;
        }

        private void LoadManufacturers()
        {
            DataTable dt = db.ExecuteQuery("SELECT id, name FROM manufacturers ORDER BY name");
            cmbManufacturer.ItemsSource = dt.DefaultView;
        }

        private void LoadProduct(int id)
        {
            DataTable dt = db.ExecuteQuery("SELECT * FROM products WHERE id = @id",
                new NpgsqlParameter[] { new NpgsqlParameter("@id", id) });
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                txtId.Text = row["id"].ToString();
                txtName.Text = row["name"].ToString();
                cmbCategory.SelectedValue = row["category_id"];
                cmbManufacturer.SelectedValue = row["manufacturer_id"];
                txtSupplier.Text = row["supplier"].ToString();
                txtPrice.Text = row["price"].ToString();
                txtQuantity.Text = row["quantity"].ToString();
                txtDiscount.Text = row["discount"].ToString();
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text))
            {
                MessageBox.Show("Введите название товара", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Введите корректную цену (неотрицательное число)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Введите корректное количество (неотрицательное целое)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtDiscount.Text, out int discount) || discount < 0 || discount > 100)
            {
                MessageBox.Show("Скидка должна быть от 0 до 100", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (productId.HasValue)
                {
                    string query = @"UPDATE products SET name=@name, category_id=@cat, manufacturer_id=@man,
                                    supplier=@sup, price=@price, quantity=@qty, discount=@disc
                                    WHERE id=@id";
                    NpgsqlParameter[] pars = {
                        new NpgsqlParameter("@name", txtName.Text),
                        new NpgsqlParameter("@cat", cmbCategory.SelectedValue ?? DBNull.Value),
                        new NpgsqlParameter("@man", cmbManufacturer.SelectedValue ?? DBNull.Value),
                        new NpgsqlParameter("@sup", txtSupplier.Text),
                        new NpgsqlParameter("@price", price),
                        new NpgsqlParameter("@qty", quantity),
                        new NpgsqlParameter("@disc", discount),
                        new NpgsqlParameter("@id", productId.Value)
                    };
                    db.ExecuteNonQuery(query, pars);
                    MessageBox.Show("Товар обновлён", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    string query = @"INSERT INTO products (name, category_id, manufacturer_id, supplier, price, quantity, discount)
                                    VALUES (@name, @cat, @man, @sup, @price, @qty, @disc)";
                    NpgsqlParameter[] pars = {
                        new NpgsqlParameter("@name", txtName.Text),
                        new NpgsqlParameter("@cat", cmbCategory.SelectedValue ?? DBNull.Value),
                        new NpgsqlParameter("@man", cmbManufacturer.SelectedValue ?? DBNull.Value),
                        new NpgsqlParameter("@sup", txtSupplier.Text),
                        new NpgsqlParameter("@price", price),
                        new NpgsqlParameter("@qty", quantity),
                        new NpgsqlParameter("@disc", discount)
                    };
                    db.ExecuteNonQuery(query, pars);
                    MessageBox.Show("Товар добавлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
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
            if (MessageBox.Show("Удалить товар?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    db.ExecuteNonQuery("DELETE FROM products WHERE id = @id",
                        new NpgsqlParameter[] { new NpgsqlParameter("@id", productId.Value) });
                    MessageBox.Show("Товар удалён", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка удаления (возможно товар в заказе): " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}