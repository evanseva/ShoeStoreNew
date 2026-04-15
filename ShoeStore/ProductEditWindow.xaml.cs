using System;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Npgsql;

namespace ShoeStore
{
    public partial class ProductEditWindow : Window
    {
        DatabaseHelper db = new DatabaseHelper();
        int? productId;
        string currentImagePath = "";

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

                if (row["image_path"] != DBNull.Value && !string.IsNullOrEmpty(row["image_path"].ToString()))
                {
                    currentImagePath = row["image_path"].ToString();
                    string fullPath = AppDomain.CurrentDomain.BaseDirectory + currentImagePath;
                    if (File.Exists(fullPath))
                        imgProduct.Source = new BitmapImage(new Uri(fullPath));
                }
            }
        }

        private void BtnLoadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";
            if (openDialog.ShowDialog() == true)
            {
                string imagesFolder = AppDomain.CurrentDomain.BaseDirectory + "Images\\";
                if (!Directory.Exists(imagesFolder))
                    Directory.CreateDirectory(imagesFolder);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(openDialog.FileName);
                string savePath = imagesFolder + fileName;
                File.Copy(openDialog.FileName, savePath, true);
                imgProduct.Source = new BitmapImage(new Uri(savePath));
                currentImagePath = "Images/" + fileName;
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
                MessageBox.Show("Введите корректную цену", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Введите корректное количество", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtDiscount.Text, out int discount) || discount < 0 || discount > 100)
            {
                MessageBox.Show("Скидка от 0 до 100", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (productId.HasValue)
                {
                    string query = @"UPDATE products SET name=@name, category_id=@cat, manufacturer_id=@man,
                                    supplier=@sup, price=@price, quantity=@qty, discount=@disc";
                    if (!string.IsNullOrEmpty(currentImagePath))
                        query += ", image_path=@img";
                    query += " WHERE id=@id";

                    var pars = new System.Collections.Generic.List<NpgsqlParameter>();
                    pars.Add(new NpgsqlParameter("@name", txtName.Text));
                    pars.Add(new NpgsqlParameter("@cat", cmbCategory.SelectedValue ?? DBNull.Value));
                    pars.Add(new NpgsqlParameter("@man", cmbManufacturer.SelectedValue ?? DBNull.Value));
                    pars.Add(new NpgsqlParameter("@sup", txtSupplier.Text));
                    pars.Add(new NpgsqlParameter("@price", price));
                    pars.Add(new NpgsqlParameter("@qty", quantity));
                    pars.Add(new NpgsqlParameter("@disc", discount));
                    pars.Add(new NpgsqlParameter("@id", productId.Value));
                    if (!string.IsNullOrEmpty(currentImagePath))
                        pars.Add(new NpgsqlParameter("@img", currentImagePath));

                    db.ExecuteNonQuery(query, pars.ToArray());
                    MessageBox.Show("Товар обновлён", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    string query = @"INSERT INTO products (name, category_id, manufacturer_id, supplier, price, quantity, discount, image_path)
                                    VALUES (@name, @cat, @man, @sup, @price, @qty, @disc, @img)";
                    NpgsqlParameter[] pars = {
                        new NpgsqlParameter("@name", txtName.Text),
                        new NpgsqlParameter("@cat", cmbCategory.SelectedValue ?? DBNull.Value),
                        new NpgsqlParameter("@man", cmbManufacturer.SelectedValue ?? DBNull.Value),
                        new NpgsqlParameter("@sup", txtSupplier.Text),
                        new NpgsqlParameter("@price", price),
                        new NpgsqlParameter("@qty", quantity),
                        new NpgsqlParameter("@disc", discount),
                        new NpgsqlParameter("@img", string.IsNullOrEmpty(currentImagePath) ? "Images/1.jpg" : currentImagePath)
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
                    MessageBox.Show("Ошибка удаления: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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