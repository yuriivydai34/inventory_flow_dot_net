using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.IO;
using System.Security.Cryptography;
namespace InventoryFlow
{
    public partial class Login : Form
    {
        string connectionstring;

        public Login()
        {
            InitializeComponent();
            getconnectionstring();
            // Set the window to be borderless and without a title bar
            this.FormBorderStyle = FormBorderStyle.None;

            // Disable resizing
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ControlBox = false;

            // Center the window on the screen
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
        private void getconnectionstring()
        {

                if (File.Exists("iflow.ini"))
                {
                    // Read all lines from the file
                    string[] lines = File.ReadAllLines("iflow.ini");
                    connectionstring = lines[0];
                }
        }
        private string HashSHA256(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        private void btn_income_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbLogin.Text) || string.IsNullOrWhiteSpace(tbPass.Text))
            {
                MessageBox.Show("Введіть логін та пароль", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            try
            {
                string hashedPass = HashSHA256(tbPass.Text);
                using (MySqlConnection connection = new MySqlConnection(connectionstring))
                {
                    connection.Open();
                    string query = "SELECT name_full, `group` FROM users WHERE login = @login AND pass = @pass";
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@login", tbLogin.Text);
                        cmd.Parameters.AddWithValue("@pass", hashedPass);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string nameFullVal = reader["name_full"].ToString();
                                string groupVal = reader["group"].ToString();
                                new Form1(nameFullVal, groupVal).ShowDialog();
                                Close();
                            }
                            else
                            {
                                tbPass.Text = "";
                                MessageBox.Show("Невірний логін або пароль", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex), "Помилка підключення", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Close();
        }
    }
}
