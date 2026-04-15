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
using System.Globalization;

namespace InventoryFlow
{
    public partial class ChangeStorage : Form
    {

        string vconnstring;
        string vid; //ID
        string vcat_name; //Повна назва
        string vmanufacturer; //Виробник
        string vseller; //Постачальник
        string vsn; //Серійний номер
        string vquantity; //Кількість
        string vunits; //Одииці вимірювання
        string vorder_number; //Артикул
        string vproject_storage; //Місце
        string vcomment; //Коментар
        string vinventory_number; //Інвентарний номер
        string vsize_width; //Розмір: ширина(мм)
        string vsize_depth; //Розмір: глибина(мм)
        string vsize_height; //Розмір: висота(мм)
        string vdate_of_check; //Дата перевірки
        string vdate_added; //Дата створення запису
        string vdate_moved_in; //Дата ввезення
        string vdate_moved_out; //Дата вивезення
        string vdate_of_maintenance; //Дата сервісного обслуговування
        string vdate_end_warranty;



        public ChangeStorage
            (
            string connstrng,
            string id, //ID
        string cat_name, //Повна назва
        string manufacturer, //Виробник
        string seller, //Постачальник
        string sn, //Серійний номер
        string quantity, //Кількість
        string units, //Одииці вимірювання
        string order_number, //Артикул
        string project_storage, //Місце
        string comment, //Коментар
        string inventory_number, //Інвентарний номер
        string size_width, //Розмір: ширина(мм)
        string size_depth, //Розмір: глибина(мм)
        string size_height, //Розмір: висота(мм)
        string date_of_check, //Дата перевірки
        string date_added, //Дата створення запису
        string date_moved_in, //Дата ввезення
        string date_moved_out, //Дата вивезення
        string date_of_maintenance, //Дата сервісного обслуговування
        string date_end_warranty) //Дата закінчення гарантії

        {
            InitializeComponent();

            lblCatName.Text = cat_name;
            lblCurrentQuantity.Text = quantity;
            lblUnits.Text = units;
            lblUnits2.Text = units;
            lblProjectStorage.Text = project_storage;
            lblCatName.Text = cat_name;

            vconnstring = connstrng;
            vid = id;
            vcat_name = cat_name;
            vmanufacturer = manufacturer;
            vseller = seller;
            vsn = sn;
            vquantity = quantity;
            vunits = units;
            vorder_number = order_number;
            vproject_storage = project_storage;
            vcomment = comment;
            vinventory_number = inventory_number;
            vsize_width = size_width;
            vsize_depth = size_depth;
            vsize_height = size_height;
            vdate_of_check = date_of_check;
            vdate_added = date_added;
            vdate_moved_in = date_moved_in;
            vdate_moved_out = date_moved_out;
            vdate_of_maintenance = date_of_maintenance;
            vdate_end_warranty = date_end_warranty;

            fill_combobox();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int quantity_old = Convert.ToInt32(lblCurrentQuantity.Text);
            string storage_old = vproject_storage;
            int quantity_new = Convert.ToInt32(tbCurrentQuantity.Text);
            //розраховуємо залишкову кільість на попередньому складі
            int quantity_to_update = (quantity_old - quantity_new);
            string storage_new = cbProjectStorage.Text;

            //перевіряємо, чи не більше нова кількість за стару
            if (quantity_new <= quantity_old)
            {
                //перевіряємо, чи не дорівнює новий склад старому
                if (vproject_storage != cbProjectStorage.Text)
                {
                    //перевіряємо, чи є вже у project_storage даний cat_name
                    string positionID = "N";
                    MySqlConnection conn = new MySqlConnection(vconnstring);
                    conn.Open();
                    try
                    {
                        positionID = Convert.ToString(new MySqlCommand(@"SELECT IFNULL((SELECT id FROM materials WHERE cat_name = '" + vcat_name + "' AND  project_storage = '" + cbProjectStorage.Text + "'),'00');", conn).ExecuteScalar());

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Дублювання запису позиції. " + Convert.ToString(ex), "Помилка 752", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    conn.Close();

                    //якщо позиція вже є
                    if (positionID != "00")
                    {

                        //перевіряємо, чи є серійний номер у cat_name & project_storage
                        string hasserial;
                        try
                        {
                            MySqlConnection conn1 = new MySqlConnection(vconnstring);
                            conn1.Open();
                            hasserial = Convert.ToString(new MySqlCommand(@"SELECT IFNULL((SELECT sn FROM materials WHERE cat_name = '" + vcat_name + "' AND  project_storage = '" + cbProjectStorage.Text + "'),'00');", conn1).ExecuteScalar());
                            conn1.Close();

                            MessageBox.Show(Convert.ToString(hasserial), " hasserial", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        catch (Exception)
                        {
                            hasserial = "";
                        }



                        ////якщо true є серійний номер
                        if (!string.IsNullOrEmpty(hasserial) && hasserial != "00")
                        {
                            //інсерт нового запису
                            using (MySqlConnection connect7 = new MySqlConnection(vconnstring))
                            {
                                string cmdText = @"INSERT INTO materials 
                        (cat_name, manufacturer, seller, sn, quantity, units, order_number, project_storage, comment, inventory_number, size_width, size_depth, size_height, date_of_check, date_added, date_moved_in, date_moved_out, date_of_maintenance, date_end_warranty) 
                        VALUES (@cat_name, @manufacturer, @seller, @sn, @quantity, @units, @order_number, @project_storage, @comment, @inventory_number, @size_width, @size_depth, @size_height, @date_of_check, @date_added, @date_moved_in, @date_moved_out, @date_of_maintenance, @date_end_warranty)";

                                using (MySqlCommand cmd = new MySqlCommand(cmdText, connect7))
                                {

                                    cmd.Parameters.AddWithValue("@cat_name", vcat_name);
                                    cmd.Parameters.AddWithValue("@manufacturer", vmanufacturer);
                                    cmd.Parameters.AddWithValue("@seller", vseller);
                                    cmd.Parameters.AddWithValue("@sn", vsn);
                                    cmd.Parameters.AddWithValue("@quantity", quantity_new);
                                    cmd.Parameters.AddWithValue("@units", vunits);
                                    cmd.Parameters.AddWithValue("@order_number", vorder_number);
                                    cmd.Parameters.AddWithValue("@project_storage", storage_new);
                                    cmd.Parameters.AddWithValue("@comment", vcomment);
                                    cmd.Parameters.AddWithValue("@inventory_number", vinventory_number);
                                    cmd.Parameters.AddWithValue("@size_width", string.IsNullOrEmpty(vsize_width) ? 0 : Convert.ToInt32(vsize_width));
                                    cmd.Parameters.AddWithValue("@size_depth", string.IsNullOrEmpty(vsize_depth) ? 0 : Convert.ToInt32(vsize_depth));
                                    cmd.Parameters.AddWithValue("@size_height", string.IsNullOrEmpty(vsize_height) ? 0 : Convert.ToInt32(vsize_height));
                                    cmd.Parameters.AddWithValue("@date_of_check", ConvertToDbValue(vdate_of_check));
                                    cmd.Parameters.AddWithValue("@date_added", ConvertToDbValue(vdate_added));
                                    cmd.Parameters.AddWithValue("@date_moved_in", DateTime.Now);
                                    cmd.Parameters.AddWithValue("@date_moved_out", DBNull.Value);
                                    cmd.Parameters.AddWithValue("@date_of_maintenance", ConvertToDbValue(vdate_of_maintenance));
                                    cmd.Parameters.AddWithValue("@date_end_warranty", ConvertToDbValue(vdate_end_warranty));


                                    connect7.Open();
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            //update кількості попереднього запису
                            //update кількість в оригінальному записі
                            MySqlConnection conn4 = new MySqlConnection(vconnstring);
                            conn4.Open();

                            string cmdupd1 = "UPDATE materials SET quantity = " + quantity_to_update + ", date_moved_out = NOW() WHERE id = " + vid + ";";
                            new MySqlCommand(cmdupd1, conn4).ExecuteNonQuery();
                            conn4.Close();


                        }
                        ////якщо false (немає серійного номеру)
                        else
                        {
                            // додаємо до існуючої кількості на складі
                            //Знаходимо  ід запису на новому складі

                            MySqlConnection conn77 = new MySqlConnection(vconnstring);
                            conn77.Open();
                            string positionFound = Convert.ToString(new MySqlCommand(@"SELECT id FROM materials WHERE cat_name = '" + vcat_name + "' AND  project_storage = '" + cbProjectStorage.Text + "';", conn77).ExecuteScalar());
                            conn77.Close();

                            //select Знаходимо поточну кількість на новому складі

                            MySqlConnection conn88 = new MySqlConnection(vconnstring);
                            conn88.Open();
                            string QuantityFound = Convert.ToString(new MySqlCommand(@"SELECT quantity FROM materials WHERE cat_name = '" + vcat_name + "' AND  project_storage = '" + cbProjectStorage.Text + "';", conn88).ExecuteScalar());
                            conn88.Close();



                            //update Додаємо кількіть
                            MySqlConnection conn3 = new MySqlConnection(vconnstring);
                            conn3.Open();

                            string sqlQuery = "UPDATE materials SET quantity = " + Convert.ToString(Convert.ToInt64(tbCurrentQuantity.Text) + Convert.ToInt64(QuantityFound)) + " WHERE id = " + positionFound + ";";
                            new MySqlCommand(sqlQuery, conn3).ExecuteNonQuery();

                            conn3.Close();
                            //update кількість в оригінальному записі
                            MySqlConnection conn4 = new MySqlConnection(vconnstring);
                            conn4.Open();
                            string cmdupd1 = "UPDATE materials SET quantity = " + quantity_to_update + ", date_moved_out = NOW() WHERE id = " + vid + ";";
                            new MySqlCommand(cmdupd1, conn4).ExecuteNonQuery();
                            conn4.Close();


                        }
                    }

                    ////якщо проблема із дублюванням знайденого запису
                    else if (positionID == "N")
                    {
                        //MessageBox.Show("Не вдалося оновити позицію на складі", "Log", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    }

                    //якщо false (cat_name на новому складі ще немає такої)
                    else
                    {
                        using (MySqlConnection connect7 = new MySqlConnection(vconnstring))
                        {
                            string cmdText = @"INSERT INTO materials 
                        (cat_name, manufacturer, seller, sn, quantity, units, order_number, project_storage, comment, inventory_number, size_width, size_depth, size_height, date_of_check, date_added, date_moved_in, date_moved_out, date_of_maintenance, date_end_warranty) 
                        VALUES (@cat_name, @manufacturer, @seller, @sn, @quantity, @units, @order_number, @project_storage, @comment, @inventory_number, @size_width, @size_depth, @size_height, @date_of_check, @date_added, @date_moved_in, @date_moved_out, @date_of_maintenance, @date_end_warranty)";

                            using (MySqlCommand cmd = new MySqlCommand(cmdText, connect7))
                            {

                                cmd.Parameters.AddWithValue("cat_name", vcat_name);
                                cmd.Parameters.AddWithValue("manufacturer", vmanufacturer);
                                cmd.Parameters.AddWithValue("seller", vseller);
                                cmd.Parameters.AddWithValue("sn", vsn);
                                cmd.Parameters.AddWithValue("quantity", quantity_new);  // ВИПРАВЛЕНО: нова кількість
                                cmd.Parameters.AddWithValue("units", vunits);
                                cmd.Parameters.AddWithValue("order_number", vorder_number);
                                cmd.Parameters.AddWithValue("project_storage", storage_new);  // ВИПРАВЛЕНО: нове місце
                                cmd.Parameters.AddWithValue("comment", vcomment);
                                cmd.Parameters.AddWithValue("inventory_number", vinventory_number);
                                cmd.Parameters.AddWithValue("@size_width", string.IsNullOrEmpty(vsize_width) ? 0 : Convert.ToInt32(vsize_width));
                                cmd.Parameters.AddWithValue("@size_depth", string.IsNullOrEmpty(vsize_depth) ? 0 : Convert.ToInt32(vsize_depth));
                                cmd.Parameters.AddWithValue("@size_height", string.IsNullOrEmpty(vsize_height) ? 0 : Convert.ToInt32(vsize_height));
                                cmd.Parameters.AddWithValue("date_of_check", ConvertToDbValue(vdate_of_check));
                                cmd.Parameters.AddWithValue("date_added", ConvertToDbValue(vdate_added));  // Оригінальна дата створення
                                cmd.Parameters.AddWithValue("date_moved_in", DateTime.Now);  // ВИПРАВЛЕНО: щойно прибув на нове місце
                                cmd.Parameters.AddWithValue("date_moved_out", DBNull.Value);  // ВИПРАВЛЕНО: ще не виїхав
                                cmd.Parameters.AddWithValue("date_of_maintenance", ConvertToDbValue(vdate_of_maintenance));
                                cmd.Parameters.AddWithValue("date_end_warranty", ConvertToDbValue(vdate_end_warranty));


                                connect7.Open();
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // update кількості на старому складі
                        MySqlConnection conn4 = new MySqlConnection(vconnstring);
                        conn4.Open();
                        string cmdupd1 = "UPDATE materials SET quantity = " + quantity_to_update + ", date_moved_out = NOW() WHERE id = " + vid + ";";

                        new MySqlCommand(cmdupd1, conn4).ExecuteNonQuery();
                        conn4.Close();
                    }

                ((Form1)Application.OpenForms["Form1"]).loadMainTable();
                    Close();
                }

                else
                {
                    MessageBox.Show("Невірно вказано склад", "Помилка!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Невірно вказано кількість", "Помилка!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void fill_combobox()
        {
            try
            {
                MySqlConnection connection347 = new MySqlConnection(vconnstring);
                connection347.Open();
                MySqlDataReader mySqlDataReader5 = new MySqlCommand("SELECT DISTINCT project_storage FROM materials WHERE project_storage LIKE '%" + cbProjectStorage.Text + "%' ;", connection347).ExecuteReader();
                while (mySqlDataReader5.Read())
                    cbProjectStorage.Items.Add(mySqlDataReader5.GetString("project_storage"));
                connection347.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex), "Error837", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void btnClose_Click_1(object sender, EventArgs e)
        {
            Close();
        }
        object ConvertToDbValue(string inputDate)
        {
            if (string.IsNullOrWhiteSpace(inputDate)) return DBNull.Value; // Insert NULL if empty

            DateTime parsedDate;
            if (DateTime.TryParseExact(inputDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
            {
                return parsedDate.ToString("yyyy-MM-dd HH:mm:ss"); // Convert to MySQL format
            }

            throw new Exception($"Invalid date format: {inputDate}"); // Handle incorrect formats
        }

        private void cbProjectStorage_TextUpdate(object sender, EventArgs e)
        {
            cbProjectStorage.Items.Clear();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(vconnstring))
                {
                    connection.Open();
                    string query = "SELECT DISTINCT project_storage FROM materials WHERE project_storage LIKE @filter;";
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@filter", "%" + cbProjectStorage.Text + "%");
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cbProjectStorage.Items.Add(reader.GetString("project_storage"));
                            }
                        }
                    }
                }

                // Додаємо поточний текст як варіант (якщо його ще немає)
                if (!string.IsNullOrEmpty(cbProjectStorage.Text) && !cbProjectStorage.Items.Contains(cbProjectStorage.Text))
                {
                    cbProjectStorage.Items.Add(cbProjectStorage.Text);
                }

                // Зберігаємо позицію курсора в кінці тексту
                cbProjectStorage.SelectionStart = cbProjectStorage.Text.Length;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка фільтрації: " + ex.Message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void cbProjectStorage_Click(object sender, EventArgs e)
        {
            int itemHeight = cbProjectStorage.ItemHeight;
            cbProjectStorage.DropDownHeight = cbProjectStorage.Height + (itemHeight * 10);
            cbProjectStorage.DroppedDown = true;
        }
    }
}