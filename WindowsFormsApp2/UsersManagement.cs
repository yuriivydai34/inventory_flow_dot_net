using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace InventoryFlow
{
    public partial class UsersManagement : Form
    {
        string connectionString;
        int? selectedUserId;

        public UsersManagement(string connectionString)
        {
            InitializeComponent();
            this.connectionString = connectionString;
            cbxRole.Items.Add(new RoleOption { Display = "Користувач", Value = "3" });
            cbxRole.Items.Add(new RoleOption { Display = "Адміністратор", Value = "1" });
            cbxRole.SelectedIndex = 0;
            dgvUsers.CellFormatting += dgvUsers_CellFormatting;
        }

        private void dgvUsers_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvUsers.Columns[e.ColumnIndex].Name != "group" || e.Value == null) return;
            e.Value = e.Value.ToString() == "1" ? "Адміністратор" : "Користувач";
            e.FormattingApplied = true;
        }

        private class RoleOption
        {
            public string Display;
            public string Value;
            public override string ToString() => Display;
        }

        private void UsersManagement_Load(object sender, EventArgs e)
        {
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT id, login, name_full, `group` FROM users ORDER BY login";
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                    {
                        DataTable table = new DataTable();
                        adapter.Fill(table);
                        dgvUsers.DataSource = table;
                    }
                }

                dgvUsers.Columns["id"].Visible = false;
                dgvUsers.Columns["login"].HeaderText = "Логін";
                dgvUsers.Columns["name_full"].HeaderText = "ПІБ";
                dgvUsers.Columns["group"].HeaderText = "Роль";
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex), "Помилка підключення", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvUsers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvUsers.CurrentRow == null)
            {
                selectedUserId = null;
                btnUpdate.Enabled = false;
                btnDelete.Enabled = false;
                return;
            }

            DataRowView row = (DataRowView)dgvUsers.CurrentRow.DataBoundItem;
            if (row["id"] == DBNull.Value)
            {
                selectedUserId = null;
                btnUpdate.Enabled = false;
                btnDelete.Enabled = false;
                return;
            }

            selectedUserId = Convert.ToInt32(row["id"]);
            tbLogin.Text = row["login"].ToString();
            tbFullName.Text = row["name_full"].ToString();
            tbPassword.Text = "";

            string group = row["group"].ToString();
            for (int i = 0; i < cbxRole.Items.Count; i++)
            {
                if (((RoleOption)cbxRole.Items[i]).Value == group)
                {
                    cbxRole.SelectedIndex = i;
                    break;
                }
            }

            btnUpdate.Enabled = true;
            btnDelete.Enabled = true;
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

        private bool LoginTaken(MySqlConnection connection, string login, int? excludeId)
        {
            string query = excludeId.HasValue
                ? "SELECT COUNT(*) FROM users WHERE login = @login AND id <> @id"
                : "SELECT COUNT(*) FROM users WHERE login = @login";

            using (MySqlCommand cmd = new MySqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@login", login);
                if (excludeId.HasValue) cmd.Parameters.AddWithValue("@id", excludeId.Value);
                return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
            }
        }

        private void ClearForm()
        {
            selectedUserId = null;
            tbLogin.Text = "";
            tbPassword.Text = "";
            tbFullName.Text = "";
            cbxRole.SelectedIndex = 0;
            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;
            dgvUsers.ClearSelection();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string login = tbLogin.Text.Trim();
            string password = tbPassword.Text;
            string fullName = tbFullName.Text.Trim();
            string role = (cbxRole.SelectedItem as RoleOption)?.Value;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(fullName))
            {
                MessageBox.Show("Заповніть логін, пароль та ПІБ", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    if (LoginTaken(connection, login, null))
                    {
                        MessageBox.Show("Користувач з таким логіном вже існує", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }

                    string hashedPass = HashSHA256(password);
                    using (MySqlCommand insertCmd = new MySqlCommand("INSERT INTO users (login, pass, name_full, `group`) VALUES (@login, @pass, @name, @group)", connection))
                    {
                        insertCmd.Parameters.AddWithValue("@login", login);
                        insertCmd.Parameters.AddWithValue("@pass", hashedPass);
                        insertCmd.Parameters.AddWithValue("@name", fullName);
                        insertCmd.Parameters.AddWithValue("@group", role);
                        insertCmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Користувача додано", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex), "Помилка підключення", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!selectedUserId.HasValue)
            {
                MessageBox.Show("Оберіть користувача у списку", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            string login = tbLogin.Text.Trim();
            string password = tbPassword.Text;
            string fullName = tbFullName.Text.Trim();
            string role = (cbxRole.SelectedItem as RoleOption)?.Value;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(fullName))
            {
                MessageBox.Show("Заповніть логін та ПІБ", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    if (LoginTaken(connection, login, selectedUserId.Value))
                    {
                        MessageBox.Show("Користувач з таким логіном вже існує", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }

                    string query = string.IsNullOrEmpty(password)
                        ? "UPDATE users SET login = @login, name_full = @name, `group` = @group WHERE id = @id"
                        : "UPDATE users SET login = @login, name_full = @name, `group` = @group, pass = @pass WHERE id = @id";

                    using (MySqlCommand updateCmd = new MySqlCommand(query, connection))
                    {
                        updateCmd.Parameters.AddWithValue("@login", login);
                        updateCmd.Parameters.AddWithValue("@name", fullName);
                        updateCmd.Parameters.AddWithValue("@group", role);
                        updateCmd.Parameters.AddWithValue("@id", selectedUserId.Value);
                        if (!string.IsNullOrEmpty(password))
                            updateCmd.Parameters.AddWithValue("@pass", HashSHA256(password));
                        updateCmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Зміни збережено", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex), "Помилка підключення", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (!selectedUserId.HasValue)
            {
                MessageBox.Show("Оберіть користувача у списку", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                $"Видалити користувача \"{tbLogin.Text}\"?",
                "Підтвердження",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    using (MySqlCommand deleteCmd = new MySqlCommand("DELETE FROM users WHERE id = @id", connection))
                    {
                        deleteCmd.Parameters.AddWithValue("@id", selectedUserId.Value);
                        deleteCmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Користувача видалено", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex), "Помилка підключення", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
