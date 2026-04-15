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
        string connstring;
        public InfoTab(string connectionstring, int matID)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            matid = Convert.ToString(matID);
            connstring = connectionstring;

            




        }

        private void LoadHistory()
        {
            string connectionString = connstring;
            string query = "SELECT cat_name, project_storage, date_moved_in, date_moved_out, date_added FROM materials WHERE inventory_number = '" + inventory_number + "';";

            using (MySqlConnection conn2 = new MySqlConnection(connectionString)) 
            {
                try
                {
                    conn2.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn2))
                    {
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            dataGridView1.DataSource = dt; 
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
                dataGridView1.AllowUserToAddRows = false;
                dataGridView1.AllowUserToAddRows = false;

                dataGridView1.RowHeadersVisible = false;
            }
        }

            private void InfoTab_Load(object sender, EventArgs e)
        {


            MySqlConnection connection = new MySqlConnection(connstring);

            id = matid;
            cat_name = getsingleSQLvalue("cat_name");
            manufacturer = getsingleSQLvalue("manufacturer");
            seller = getsingleSQLvalue("seller");
            sn = getsingleSQLvalue("sn");
            quantity = getsingleSQLvalue("quantity");
            units = getsingleSQLvalue("units");
            order_number = getsingleSQLvalue("order_number");
            project_storage = getsingleSQLvalue("project_storage");
            comment = getsingleSQLvalue("comment");
            inventory_number = getsingleSQLvalue("inventory_number");
            size_width = getsingleSQLvalue("size_width");
            size_depth = getsingleSQLvalue("size_depth");
            size_height = getsingleSQLvalue("size_height");
            date_of_check = getsingleSQLvalue("date_of_check");
            date_added = getsingleSQLvalue("date_added");
            date_moved_in = getsingleSQLvalue("date_moved_in");
            date_moved_out = getsingleSQLvalue("date_moved_out");
            date_of_maintenance = getsingleSQLvalue("date_of_maintenance");
            date_end_warranty = getsingleSQLvalue("date_end_warranty");

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

            LoadHistory();
        }

        private string getsingleSQLvalue(string clmn)
        {
            object result;

            using (MySqlConnection conn = new MySqlConnection(connstring))
            {
                conn.Open();
                string query = "SELECT "+ clmn+" FROM materials WHERE id = "+matid+";";
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    result = cmd.ExecuteScalar();

                }
            }
            string val = Convert.ToString(result);
            return val;
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
