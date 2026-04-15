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
        private void btn_income_Click(object sender, EventArgs e)
        {

        try
        {

            MySqlConnection connection = new MySqlConnection(connectionstring);
            connection.Open();
            }
            catch(Exception ex)
            {
                //MessageBox.Show(Convert.ToString(ex), "Exception", MessageBoxButtons.OK);
                //
            }

            if (tbLogin.Text == "user" && tbPass.Text == "1234")
            {
                new Form1("user", "Operator").ShowDialog();
                Close();
            }
            else
            {
                tbLogin.Text = "";
                tbPass.Text = "";
                MessageBox.Show("Невірний пароль", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Close();
        }
    }
}
