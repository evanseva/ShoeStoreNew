using System;
using System.Data;
using System.Windows;
using Npgsql;

namespace ShoeStore
{
    public partial class LoginWindow : Window
    {
        DatabaseHelper db = new DatabaseHelper();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string query = "SELECT id, full_name, role FROM users WHERE login = @login AND password = @password";
            NpgsqlParameter[] pars = {
                new NpgsqlParameter("@login", login),
                new NpgsqlParameter("@password", password)
            };

            DataTable dt = db.ExecuteQuery(query, pars);

            if (dt.Rows.Count > 0)
            {
                User user = new User
                {
                    Id = Convert.ToInt32(dt.Rows[0]["id"]),
                    FullName = dt.Rows[0]["full_name"].ToString(),
                    Role = dt.Rows[0]["role"].ToString()
                };
                OpenMainWindow(user);
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            User guest = new User { Id = 0, FullName = "Гость", Role = "guest" };
            OpenMainWindow(guest);
        }

        private void OpenMainWindow(User user)
        {
            MainWindow main = new MainWindow(user);
            main.Show();
            this.Close();
        }
    }
}