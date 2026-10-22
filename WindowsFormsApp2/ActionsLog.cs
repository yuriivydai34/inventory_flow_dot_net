using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace InventoryFlow
{
    public partial class ActionsLog : Form
    {
        string connectionString;

        public ActionsLog(string connectionString)
        {
            InitializeComponent();
            this.connectionString = connectionString;

            cbxActionFilter.Items.Add("Усі дії");
            cbxActionFilter.Items.Add("Прихід");
            cbxActionFilter.Items.Add("Редагування");
            cbxActionFilter.Items.Add("Видалення");
            cbxActionFilter.Items.Add("Переміщення");
            cbxActionFilter.Items.Add("Сканування");
            cbxActionFilter.SelectedIndex = 0;

            dgvLog.CellFormatting += dgvLog_CellFormatting;
        }

        private void ActionsLog_Load(object sender, EventArgs e)
        {
            LoadLog();
        }

        private string SelectedActionType()
        {
            switch (cbxActionFilter.SelectedItem as string)
            {
                case "Прихід": return "income";
                case "Редагування": return "edit";
                case "Видалення": return "delete";
                case "Переміщення": return "move";
                case "Сканування": return "scan";
                default: return null;
            }
        }

        private void LoadLog()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"SELECT il.datetime, il.action_type, u.name_full AS user_name,
                        il.cat_name, il.quantity, il.units, il.project_storage, il.previous_storage, il.comment
                        FROM income_log il
                        LEFT JOIN users u ON u.id = il.user_id";

                    string actionType = SelectedActionType();
                    if (actionType != null)
                        query += " WHERE il.action_type = @action_type";

                    query += " ORDER BY il.datetime DESC LIMIT 500";

                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        if (actionType != null)
                            cmd.Parameters.AddWithValue("@action_type", actionType);

                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable table = new DataTable();
                            adapter.Fill(table);
                            dgvLog.DataSource = table;
                        }
                    }
                }

                dgvLog.Columns["datetime"].HeaderText = "Дата/час";
                dgvLog.Columns["action_type"].HeaderText = "Дія";
                dgvLog.Columns["user_name"].HeaderText = "Користувач";
                dgvLog.Columns["cat_name"].HeaderText = "Товар";
                dgvLog.Columns["quantity"].HeaderText = "Кількість";
                dgvLog.Columns["units"].HeaderText = "Од. виміру";
                dgvLog.Columns["project_storage"].HeaderText = "Місце";
                dgvLog.Columns["previous_storage"].HeaderText = "Попереднє місце";
                dgvLog.Columns["comment"].HeaderText = "Коментар";
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex), "Помилка підключення", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvLog_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvLog.Columns[e.ColumnIndex].Name != "action_type" || e.Value == null) return;

            switch (e.Value.ToString())
            {
                case "income": e.Value = "Прихід"; break;
                case "edit": e.Value = "Редагування"; break;
                case "delete": e.Value = "Видалення"; break;
                case "move": e.Value = "Переміщення"; break;
                case "scan": e.Value = "Сканування"; break;
            }
            e.FormattingApplied = true;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadLog();
        }

        private void cbxActionFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadLog();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
