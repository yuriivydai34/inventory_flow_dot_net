using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InventoryFlow
{
    public partial class InfoTab : Form
    {

        string id; //ID
        string cat_name; //Повна назва
        string manufacturer; //Виробник
        string seller; //Постачальник
        string sn; //Серійний номер
        string quantity; //Кількість
        string units; //Одииці вимірювання
        string order_number; //Артикул
        string project_storage; //Місце
        string comment; //Коментар
        string inventory_number; //Інвентарний номер
        string size_width; //Розмір: ширина(мм)
        string size_depth; //Розмір: глибина(мм)
        string size_height; //Розмір: висота(мм)
        string date_of_check; //Дата перевірки
        string date_added; //Дата створення запису
        string date_moved_in; //Дата ввезення
        string date_moved_out; //Дата вивезення
        string date_of_maintenance; //Дата сервісного обслуговування
        string date_end_warranty; //Дата закінчення гарантії



        string matid;
        RestTableClient materialsApi;
        public InfoTab(string apiBaseUrl, string apiKey, int matID)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            matid = Convert.ToString(matID);
            materialsApi = new RestTableClient(apiBaseUrl, apiKey, "materials");
        }

        private void LoadHistory(List<Dictionary<string, object>> materials)
        {
            var matches = materials.Where(m => string.Equals(m.GetString("inventory_number"), inventory_number, StringComparison.OrdinalIgnoreCase));

            var dt = new DataTable();
            dt.Columns.Add("cat_name", typeof(object));
            dt.Columns.Add("project_storage", typeof(object));
            dt.Columns.Add("date_moved_in", typeof(object));
            dt.Columns.Add("date_moved_out", typeof(object));
            dt.Columns.Add("date_added", typeof(object));
            foreach (var m in matches)
            {
                var row = dt.NewRow();
                row["cat_name"] = m.Get("cat_name") ?? (object)DBNull.Value;
                row["project_storage"] = m.Get("project_storage") ?? (object)DBNull.Value;
                row["date_moved_in"] = m.Get("date_moved_in") ?? (object)DBNull.Value;
                row["date_moved_out"] = m.Get("date_moved_out") ?? (object)DBNull.Value;
                row["date_added"] = m.Get("date_added") ?? (object)DBNull.Value;
                dt.Rows.Add(row);
            }
            dataGridView1.DataSource = dt;

            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.RowHeadersVisible = false;
        }

        private async void InfoTab_Load(object sender, EventArgs e)
        {
            List<Dictionary<string, object>> materials;
            try
            {
                materials = await materialsApi.ListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                return;
            }

            var material = materials.FirstOrDefault(m => m.GetString("id") == matid);
            if (material == null)
            {
                MessageBox.Show("Позицію не знайдено.");
                return;
            }

            id = matid;
            cat_name = material.GetString("cat_name");
            manufacturer = material.GetString("manufacturer");
            seller = material.GetString("seller");
            sn = material.GetString("sn");
            quantity = material.GetString("quantity");
            units = material.GetString("units");
            order_number = material.GetString("order_number");
            project_storage = material.GetString("project_storage");
            comment = material.GetString("comment");
            inventory_number = material.GetString("inventory_number");
            size_width = material.GetString("size_width");
            size_depth = material.GetString("size_depth");
            size_height = material.GetString("size_height");
            date_of_check = material.GetString("date_of_check");
            date_added = material.GetString("date_added");
            date_moved_in = material.GetString("date_moved_in");
            date_moved_out = material.GetString("date_moved_out");
            date_of_maintenance = material.GetString("date_of_maintenance");
            date_end_warranty = material.GetString("date_end_warranty");

            tbInfo.Text = "ID: "+ id + Environment.NewLine +
cat_name + Environment.NewLine +
manufacturer + Environment.NewLine +
"Продавець: "+ seller + Environment.NewLine +
"Серійний номер: " + sn + Environment.NewLine +
"Кількість: " + quantity +" "+ units + Environment.NewLine +
"Артикул: " + order_number + Environment.NewLine +
"Місце: " + project_storage + Environment.NewLine +
"Продавець: " + comment + Environment.NewLine +
"Інв. : " + inventory_number + Environment.NewLine +
"Ширина: " + size_width + Environment.NewLine +
"Глибина: " + size_depth + Environment.NewLine +
"Висота: " + size_height + Environment.NewLine +
"Дата перевірки: " + date_of_check + Environment.NewLine +
"Дата створення: " + date_added + Environment.NewLine +
"Заїзд: " + date_moved_in + Environment.NewLine +
"Виїзд: " + date_of_maintenance + Environment.NewLine +
"Дата завершення гарантії: "+date_end_warranty + Environment.NewLine;

            LoadHistory(materials);
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }



        private void btnClose_Click_1(object sender, EventArgs e)
        {
            Close();
        }

        private void tbInfo_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
