using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;
using System.Web.Script.Serialization;
namespace InventoryFlow
{
    public partial class Login : Form
    {
        string apiBaseUrl;
        string apiKey;

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
            string iniPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "iflow.ini");
            if (File.Exists(iniPath))
            {
                var lines = File.ReadAllLines(iniPath)
                    .Select(l => l.Trim())
                    .Where(l => l.Length > 0 && !l.StartsWith(";"))
                    .ToArray();

                foreach (var line in lines)
                {
                    if (line.StartsWith("ApiBaseUrl=", StringComparison.OrdinalIgnoreCase))
                        apiBaseUrl = line.Substring("ApiBaseUrl=".Length).Trim();
                    else if (line.StartsWith("ApiKey=", StringComparison.OrdinalIgnoreCase))
                        apiKey = line.Substring("ApiKey=".Length).Trim();
                }
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

        private async void btn_income_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbLogin.Text) || string.IsNullOrWhiteSpace(tbPass.Text))
            {
                MessageBox.Show("Введіть логін та пароль", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (string.IsNullOrEmpty(apiBaseUrl))
            {
                MessageBox.Show("Не задано ApiBaseUrl у файлі конфігурації (iflow.ini).", "Помилка підключення", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btn_income.Enabled = false;
            try
            {
                string hashedPass = HashSHA256(tbPass.Text);
                using (var http = new HttpClient())
                {
                    if (!string.IsNullOrEmpty(apiKey))
                        http.DefaultRequestHeaders.Add("x-api-key", apiKey);

                    var payload = new StringContent(
                        new JavaScriptSerializer().Serialize(new { login = tbLogin.Text, pass_sha256 = hashedPass }),
                        Encoding.UTF8, "application/json");

                    var response = await http.PostAsync(apiBaseUrl.TrimEnd('/') + "/auth/login", payload);
                    var body = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        tbPass.Text = "";
                        MessageBox.Show("Невірний логін або пароль", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        btn_income.Enabled = true;
                        return;
                    }
                    if (!response.IsSuccessStatusCode)
                        throw new Exception($"{(int)response.StatusCode}: {body}");

                    var result = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(body);
                    Hide();
                    new Form1(result["name_full"], result["group"]).ShowDialog();
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex), "Помилка підключення", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btn_income.Enabled = true;
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Close();
        }
    }
}
