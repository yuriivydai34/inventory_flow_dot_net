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
using ZXing;
using ZXing.Common;
using ZXing.Rendering;
using System.Drawing.Printing;
using System.Text.RegularExpressions;



namespace InventoryFlow
{
    public partial class Income : Form
    {
        private PrintDocument printDocument;
        string CodeGenerated = "_";
        string connectionstring;


        public Income(string con)
        {
            InitializeComponent();
            connectionstring = con;
            cbxSerialNumber.Checked = false;
            tbSerialNumber.Enabled = false;
            dateTimePicker1.Enabled = false;


            fillcombobox(cbxName, "cat_name");
            fillcombobox(cbxManufacturer, "manufacturer");
            fillcombobox(cbxSeller, "seller");
            fillcombobox(cbxProjectStorage, "project_storage");
            fillcombobox(cbxUnits, "units");


        }





        private void fillcombobox(ComboBox cb, string columnName)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionstring))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand("SELECT DISTINCT " + columnName + " FROM materials;", connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            cb.Items.Clear(); // Clear previous items to avoid duplicates
                            while (reader.Read())
                            {
                                cb.Items.Add(reader.GetString(0)); // Use index 0
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            generateCode();

            if (string.IsNullOrWhiteSpace(cbxName.Text) ||
                string.IsNullOrWhiteSpace(cbxUnits.Text) ||
                string.IsNullOrWhiteSpace(cbxProjectStorage.Text) ||
                string.IsNullOrWhiteSpace(tbQuantity.Text))
            {
                MessageBox.Show("Порожні обов'язкові поля!", "Увага!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            int positionsCount = Convert.ToInt32(tbPositions.Text);

            for (int i = 0; i < positionsCount; i++)
            {
                try
                {
                    using (MySqlConnection connection1 = new MySqlConnection(connectionstring))
                    {
                        connection1.Open();

                        string inventoryNumber;
                        if (positionsCount > 1)
                        {
                            // XX1234E3-1, XX1234E3-2, XX1234E3-3
                            inventoryNumber = CodeGenerated + "-" + (i + 1);
                        }
                        else
                        {
                            // XX1234E1
                            inventoryNumber = CodeGenerated;
                        }

                        string cmdText = @"INSERT INTO materials 
                    (cat_name, manufacturer, seller, sn, quantity, units, order_number, 
                     project_storage, comment, inventory_number, size_width, size_depth, 
                     size_height, date_of_check, date_added, date_moved_in, date_end_warranty) 
                    VALUES 
                    (@cat_name, @manufacturer, @seller, @sn, @quantity, @units, @order_number,
                     @project_storage, @comment, @inventory_number, @size_width, @size_depth,
                     @size_height, @date_of_check, @date_added, @date_moved_in, @date_end_warranty)";

                        MySqlCommand cmd = new MySqlCommand(cmdText, connection1);
                        cmd.Parameters.AddWithValue("@cat_name", cbxName.Text);
                        cmd.Parameters.AddWithValue("@manufacturer", cbxManufacturer.Text);
                        cmd.Parameters.AddWithValue("@seller", cbxSeller.Text);
                        cmd.Parameters.AddWithValue("@sn", tbSerialNumber.Text);
                        cmd.Parameters.AddWithValue("@quantity", Convert.ToInt32(tbQuantity.Text));
                        cmd.Parameters.AddWithValue("@units", cbxUnits.Text);
                        cmd.Parameters.AddWithValue("@order_number", cbxOrderNumber.Text);
                        cmd.Parameters.AddWithValue("@project_storage", cbxProjectStorage.Text);
                        cmd.Parameters.AddWithValue("@comment", tbComment.Text);
                        cmd.Parameters.AddWithValue("@inventory_number", inventoryNumber);
                        cmd.Parameters.AddWithValue("@size_width", Convert.ToInt16(tbWidth.Text));
                        cmd.Parameters.AddWithValue("@size_depth", Convert.ToInt16(tbDepth.Text));
                        cmd.Parameters.AddWithValue("@size_height", Convert.ToInt16(tbHeight.Text));
                        cmd.Parameters.AddWithValue("@date_of_check", DateTime.Now);
                        cmd.Parameters.AddWithValue("@date_added", DateTime.Now);
                        cmd.Parameters.AddWithValue("@date_moved_in", DateTime.Now);
                        cmd.Parameters.AddWithValue("@date_end_warranty",
                            checkBox1.Checked ? dateTimePicker1.Value : (object)DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка при додаванні: " + ex.Message, "Помилка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            // Оновити таблицю на головному екрані
            ((Form1)Application.OpenForms["Form1"]).loadMainTable();
            Close();
        }


        private void insertSingleSQL(string column, string val)
        {
            string cmdText = @"INSERT INTO materials Value";
        }







        //Чи використовується серійний номер при введенні даних
        private void cbxSerialNumber_MouseClick(object sender, MouseEventArgs e)
        {
            //Перевіряємо, чи є серійний номер
            if (cbxSerialNumber.Checked == true)
            {
                tbSerialNumber.Enabled = true;
                tbQuantity.Text = "1";
                tbQuantity.Enabled = false;
            }
            else
            {
                //Якщо серійного номера нема
                //Перевіряємо, чи вже є дана позиція в базі

                
                tbSerialNumber.Enabled = false;
                tbQuantity.Enabled = true;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void comboBox1_DropDown(object sender, EventArgs e)
        {

        }

        private void cbxName_TextChanged(object sender, EventArgs e)
        {



        }

        private void cbxManufacturer_TextChanged(object sender, EventArgs e)
        {



        }

        private void cbxSeller_TextChanged(object sender, EventArgs e)
        {

        }

        private void cbxUnits_TextChanged(object sender, EventArgs e)
        {

        }

        private void cbxProjectStorage_TextChanged(object sender, EventArgs e)
        {

        }

        private void cbxName_Click(object sender, EventArgs e)
        {
            //cbxName.DroppedDown = false;
            int itemHeight = (int)cbxName.ItemHeight;
            cbxName.DropDownHeight = cbxName.Height + (itemHeight * 10);
            cbxName.DroppedDown = true;
        }

        private void cbxName_TextUpdate(object sender, EventArgs e)
        {
            cbxName.Items.Clear();
            MySqlConnection connection = new MySqlConnection(connectionstring);
            connection.Open();
            MySqlDataReader mySqlDataReader1 = new MySqlCommand("SELECT DISTINCT cat_name FROM materials WHERE cat_name LIKE '%" + cbxName.Text + "%' ;", connection).ExecuteReader();
            while (mySqlDataReader1.Read())
                cbxName.Items.Add((object)mySqlDataReader1.GetString("cat_name"));
            connection.Close();
            cbxName.Items.Add(cbxName.Text);
            //cbxName.DroppedDown = true;
            cbxName.SelectionStart = cbxName.Text.Length;


        }

        private void cbxManufacturer_TextUpdate(object sender, EventArgs e)
        {

            cbxManufacturer.Items.Clear();
            MySqlConnection connection = new MySqlConnection(connectionstring);
            connection.Open();
            MySqlDataReader mySqlDataReader1 = new MySqlCommand("SELECT DISTINCT manufacturer FROM materials WHERE manufacturer LIKE '%" + cbxManufacturer.Text + "%' ;", connection).ExecuteReader();
            while (mySqlDataReader1.Read())
                cbxManufacturer.Items.Add((object)mySqlDataReader1.GetString("manufacturer"));
            connection.Close();
            cbxManufacturer.Items.Add(cbxManufacturer.Text);
            //cbxManufacturer.DroppedDown = true;
            cbxManufacturer.SelectionStart = cbxManufacturer.Text.Length;
        }

        private void cbxSeller_TextUpdate(object sender, EventArgs e)
        {
            cbxSeller.Items.Clear();
            MySqlConnection connection = new MySqlConnection(connectionstring);
            connection.Open();
            MySqlDataReader mySqlDataReader1 = new MySqlCommand("SELECT DISTINCT seller FROM materials WHERE seller LIKE '%" + cbxSeller.Text + "%' ;", connection).ExecuteReader();
            while (mySqlDataReader1.Read())
                cbxSeller.Items.Add((object)mySqlDataReader1.GetString("seller"));
            connection.Close();
            cbxSeller.Items.Add(cbxSeller.Text);
            //cbxSeller.DroppedDown = true;
            cbxSeller.SelectionStart = cbxSeller.Text.Length;
        }

        private void cbxUnits_TextUpdate(object sender, EventArgs e)
        {
            cbxUnits.Items.Clear();

            MySqlConnection connection = new MySqlConnection(connectionstring);
            connection.Open();
            MySqlDataReader mySqlDataReader1 = new MySqlCommand("SELECT DISTINCT units FROM materials WHERE units LIKE '%" + cbxUnits.Text + "%' ;", connection).ExecuteReader();
            while (mySqlDataReader1.Read())
                cbxUnits.Items.Add((object)mySqlDataReader1.GetString("units"));
            connection.Close();
            cbxUnits.Items.Add(cbxUnits.Text);
            //cbxUnits.DroppedDown = true;
            cbxUnits.SelectionStart = cbxUnits.Text.Length;
        }

        private void cbxProjectStorage_TextUpdate(object sender, EventArgs e)
        {
            cbxProjectStorage.Items.Clear();
            MySqlConnection connection = new MySqlConnection(connectionstring);
            connection.Open();
            MySqlDataReader mySqlDataReader1 = new MySqlCommand("SELECT DISTINCT project_storage FROM materials WHERE project_storage LIKE '%" + cbxProjectStorage.Text + "%' ;", connection).ExecuteReader();
            while (mySqlDataReader1.Read())
                cbxProjectStorage.Items.Add(mySqlDataReader1.GetString("project_storage"));
            connection.Close();
            cbxProjectStorage.Items.Add(cbxProjectStorage.Text);
            //cbxProjectStorage.DroppedDown = true;
            cbxProjectStorage.SelectionStart = cbxProjectStorage.Text.Length;
        }

        private void cbxName_SelectionChangeCommitted(object sender, EventArgs e) ///not edited yet
        {
            try
            {



            }
            catch(Exception eeex)
            {
                MessageBox.Show(Convert.ToString(eeex));
            }
        }

        private void cbxOrderNumber_TextUpdate(object sender, EventArgs e)
        {
            cbxOrderNumber.Items.Clear();
            MySqlConnection connection = new MySqlConnection(connectionstring);
            connection.Open();
            MySqlDataReader mySqlDataReader1 = new MySqlCommand("SELECT DISTINCT order_number FROM materials WHERE order_number LIKE '%" + cbxOrderNumber.Text + "%' ;", connection).ExecuteReader();
            while (mySqlDataReader1.Read())
                cbxOrderNumber.Items.Add(mySqlDataReader1.GetString("order_number"));
            connection.Close();
            cbxOrderNumber.Items.Add(cbxOrderNumber.Text);
            //cbxProjectStorage.DroppedDown = true;
            cbxOrderNumber.SelectionStart = cbxOrderNumber.Text.Length;
        }

        private void cbxName_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            MySqlConnection connection1 = new MySqlConnection(connectionstring);
            connection1.Open();
            string qstr = Convert.ToString(new MySqlCommand(@"SELECT manufacturer FROM materials WHERE cat_name = '" + cbxName.Text + "';", connection1).ExecuteScalar());
            connection1.Close();
            cbxManufacturer.Text = qstr;
            cbxManufacturer.SelectedValue = qstr;
            cbxManufacturer.SelectedItem = qstr;
            cbxManufacturer.Text = qstr;
        }

        private void cbxName_SelectedValueChanged(object sender, EventArgs e)
        {
            MySqlConnection connection1 = new MySqlConnection(connectionstring);
            connection1.Open();
            string qs1 = Convert.ToString(new MySqlCommand(@"SELECT manufacturer FROM materials WHERE cat_name = '" + cbxName.Text + "';", connection1).ExecuteScalar());
            connection1.Close();
            cbxManufacturer.Text = qs1;

            MySqlConnection connection2 = new MySqlConnection(connectionstring);
            connection2.Open();
            string qs2 = Convert.ToString(new MySqlCommand(@"SELECT seller FROM materials WHERE cat_name = '" + cbxName.Text + "';", connection2).ExecuteScalar());
            connection2.Close();
            cbxSeller.Text = qs2;

            MySqlConnection connection3 = new MySqlConnection(connectionstring);
            connection3.Open();
            string qs3 = Convert.ToString(new MySqlCommand(@"SELECT units FROM materials WHERE cat_name = '" + cbxName.Text + "';", connection3).ExecuteScalar());
            connection3.Close();
            cbxUnits.Text = qs3;

            MySqlConnection connection4 = new MySqlConnection(connectionstring);
            connection4.Open();
            string qs4 = Convert.ToString(new MySqlCommand(@"SELECT order_number FROM materials WHERE cat_name = '" + cbxName.Text + "';", connection4).ExecuteScalar());
            connection4.Close();
            cbxOrderNumber.Text = qs4;

            // Завантаження габаритів
            MySqlConnection connection5 = new MySqlConnection(connectionstring);
            connection5.Open();
            string qs5 = Convert.ToString(new MySqlCommand(@"SELECT size_width FROM materials WHERE cat_name = '" + cbxName.Text + "';", connection5).ExecuteScalar());
            connection5.Close();
            tbWidth.Text = string.IsNullOrEmpty(qs5) ? "0" : qs5;

            MySqlConnection connection6 = new MySqlConnection(connectionstring);
            connection6.Open();
            string qs6 = Convert.ToString(new MySqlCommand(@"SELECT size_depth FROM materials WHERE cat_name = '" + cbxName.Text + "';", connection6).ExecuteScalar());
            connection6.Close();
            tbDepth.Text = string.IsNullOrEmpty(qs6) ? "0" : qs6;

            MySqlConnection connection7 = new MySqlConnection(connectionstring);
            connection7.Open();
            string qs7 = Convert.ToString(new MySqlCommand(@"SELECT size_height FROM materials WHERE cat_name = '" + cbxName.Text + "';", connection7).ExecuteScalar());
            connection7.Close();
            tbHeight.Text = string.IsNullOrEmpty(qs7) ? "0" : qs7;
        }
        private void cbxSeller_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void Income_Load(object sender, EventArgs e)
        {

        }

        private void cbxOrderNumber_SelectedValueChanged(object sender, EventArgs e)
        {
            MySqlConnection connection1 = new MySqlConnection(connectionstring);
            connection1.Open();
            string qs1 = Convert.ToString(new MySqlCommand(@"SELECT manufacturer FROM materials WHERE order_number = '" + cbxOrderNumber.Text + "';", connection1).ExecuteScalar());
            connection1.Close();
            cbxManufacturer.Text = qs1;

            MySqlConnection connection2 = new MySqlConnection(connectionstring);
            connection2.Open();
            string qs2 = Convert.ToString(new MySqlCommand(@"SELECT seller FROM materials WHERE order_number = '" + cbxOrderNumber.Text + "';", connection2).ExecuteScalar());
            connection2.Close();
            cbxSeller.Text = qs2;

            MySqlConnection connection3 = new MySqlConnection(connectionstring);
            connection3.Open();
            string qs3 = Convert.ToString(new MySqlCommand(@"SELECT units FROM materials WHERE order_number = '" + cbxOrderNumber.Text + "';", connection3).ExecuteScalar());
            connection3.Close();
            cbxUnits.Text = qs3;

            MySqlConnection connection4 = new MySqlConnection(connectionstring);
            connection4.Open();
            string qs4 = Convert.ToString(new MySqlCommand(@"SELECT cat_name FROM materials WHERE order_number = '" + cbxOrderNumber.Text + "';", connection4).ExecuteScalar());
            connection4.Close();
            cbxName.Text = qs4;

            // Завантаження габаритів
            MySqlConnection connection5 = new MySqlConnection(connectionstring);
            connection5.Open();
            string qs5 = Convert.ToString(new MySqlCommand(@"SELECT size_width FROM materials WHERE order_number = '" + cbxOrderNumber.Text + "';", connection5).ExecuteScalar());
            connection5.Close();
            tbWidth.Text = string.IsNullOrEmpty(qs5) ? "0" : qs5;

            MySqlConnection connection6 = new MySqlConnection(connectionstring);
            connection6.Open();
            string qs6 = Convert.ToString(new MySqlCommand(@"SELECT size_depth FROM materials WHERE order_number = '" + cbxOrderNumber.Text + "';", connection6).ExecuteScalar());
            connection6.Close();
            tbDepth.Text = string.IsNullOrEmpty(qs6) ? "0" : qs6;

            MySqlConnection connection7 = new MySqlConnection(connectionstring);
            connection7.Open();
            string qs7 = Convert.ToString(new MySqlCommand(@"SELECT size_height FROM materials WHERE order_number = '" + cbxOrderNumber.Text + "';", connection7).ExecuteScalar());
            connection7.Close();
            tbHeight.Text = string.IsNullOrEmpty(qs7) ? "0" : qs7;
        }
        private void cbxOrderNumber_SelectionChangeCommitted(object sender, EventArgs e)
        {

        }



        private bool isCodeExists(string code)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionstring))
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM materials WHERE inventory_number = @code";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@code", code);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка перевірки коду: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false; // При помилці дозволяємо продовжити
            }
        }

        // 2. НОВИЙ МЕТОД - генерація випадкового 4-значного числа
        private int GenerateRandomNumber4()
        {
            Random random = new Random();
            return random.Next(0, 10000); // 0000-9999
        }

        // 3. НОВИЙ МЕТОД - витягнути перші 2 цифри з рядка
        private string ExtractFirst2Digits(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                // Немає тексту - дві випадкові цифри
                Random rnd = new Random();
                return rnd.Next(0, 10).ToString() + rnd.Next(0, 10).ToString();
            }

            var digits = Regex.Matches(input, @"\d")
                              .Cast<Match>()
                              .Select(m => m.Value)
                              .ToList();

            if (digits.Count >= 2)
            {
                // Є 2+ цифри - беремо перші дві
                return digits[0] + digits[1];
            }
            else if (digits.Count == 1)
            {
                // Тільки одна цифра (поверх) - додаємо випадкову справа
                Random rnd = new Random();
                return digits[0] + rnd.Next(0, 10).ToString();
            }
            else
            {
                // Немає цифр взагалі - дві випадкові
                Random rnd = new Random();
                return rnd.Next(0, 10).ToString() + rnd.Next(0, 10).ToString();
            }
        }



        private void cbxOrderNumber_Click(object sender, EventArgs e)
        {
            int itemHeight = (int)cbxOrderNumber.ItemHeight;
            cbxOrderNumber.DropDownHeight = cbxOrderNumber.Height + (itemHeight * 10);
            cbxOrderNumber.DroppedDown = true;
        }

        private void cbxManufacturer_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cbxManufacturer_Click(object sender, EventArgs e)
        {
            int itemHeight = (int)cbxManufacturer.ItemHeight;
            cbxManufacturer.DropDownHeight = cbxManufacturer.Height + (itemHeight * 10);
            cbxManufacturer.DroppedDown = true;
        }

        private void cbxSeller_Click(object sender, EventArgs e)
        {
            int itemHeight = (int)cbxSeller.ItemHeight;
            cbxSeller.DropDownHeight = cbxSeller.Height + (itemHeight * 10);
            cbxSeller.DroppedDown = true;
        }

        private void cbxUnits_Click(object sender, EventArgs e)
        {
            int itemHeight = (int)cbxUnits.ItemHeight;
            cbxUnits.DropDownHeight = cbxUnits.Height + (itemHeight * 10);
            cbxUnits.DroppedDown = true;
        }

        private void cbxProjectStorage_Click(object sender, EventArgs e)
        {
            int itemHeight = (int)cbxProjectStorage.ItemHeight;
            cbxProjectStorage.DropDownHeight = cbxProjectStorage.Height + (itemHeight * 10);
            cbxProjectStorage.DroppedDown = true;
        }

        private void button2_Click_1(object sender, EventArgs e)
        {




            generateCode();










        }


        /// <summary>
        /// WIP below
        /// </summary>
        /// 


            private void checkCode(string codeTocheck)
            {
                MySqlConnection conn = new MySqlConnection(connectionstring);

            //generateCode();


            //string checkvalue = SELECT IFNULL(inventory_number, "free") FROM materials WHERE inventory_number = {generatednumber} ;


            //if checkvalue == "free"
            //OK
            //else
            //repeat
            //
            string query = "SELECT COUNT(*) FROM your_table WHERE barcode = @barcode";
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@barcode", CodeGenerated);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());

                    if (count == 0)
                    {
                        // Barcode not found, run the method
                        generateCode();
                    }
                    else
                    {
                        MessageBox.Show("Barcode already exists.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error086: {ex.Message}");
                }
            }

        // 4. ЗАМІНИТИ МЕТОД generateCode() на цей:
        private void generateCode()
        {
            try
            {
                string storagePrefix = ExtractFirst2Digits(cbxProjectStorage.Text);
                int positionsCount = Convert.ToInt32(tbPositions.Text);
                int maxAttempts = 50;
                bool codeGenerated = false;
                string newCode = "";

                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    int randomNumber = GenerateRandomNumber4();

                    if (positionsCount > 1)
                    {
                        // Формат: XX1234E3
                        newCode = storagePrefix + randomNumber.ToString("D4") + "E" + positionsCount;
                    }
                    else
                    {
                        // Формат: XX1234E
                        newCode = storagePrefix + randomNumber.ToString("D4") + "E";
                    }

                    // Перевіряємо чи код унікальний
                    bool exists = false;
                    if (positionsCount > 1)
                    {
                        for (int i = 1; i <= positionsCount; i++)
                        {
                            if (isCodeExists(newCode + "-" + i))
                            {
                                exists = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        exists = isCodeExists(newCode);
                    }

                    if (!exists)
                    {
                        codeGenerated = true;
                        break;
                    }
                }

                if (!codeGenerated)
                {
                    MessageBox.Show(
                        "Увага! Не вдалося згенерувати унікальний інвентарний номер після " + maxAttempts + " спроб.\n" +
                        "Буде використано неунікальний код. Будь ласка, виправте це вручну пізніше.",
                        "Попередження",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }

                CodeGenerated = newCode;

                if (positionsCount > 1)
                {
                    txtBarcode.Text = newCode + "-[1.." + positionsCount + "]";
                }
                else
                {
                    txtBarcode.Text = newCode;
                }

                try
                {
                    string previewCode = positionsCount > 1 ? newCode + "-1" : newCode;
                    var barcodeBitmap = GenerateDataMatrixBarcode(previewCode);
                    picCode.Image = barcodeBitmap;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Помилка генерації штрих-коду: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка: " + Convert.ToString(ex));
            }
        }









        private Bitmap GenerateDataMatrixBarcode(string data)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.DATA_MATRIX,
                Options = new EncodingOptions
                {
                    Height = 150,
                    Width = 150,
                    Margin = 0
                }
            };

            writer.Options.Hints.Add(EncodeHintType.CHARACTER_SET, "UTF-8");
            return writer.Write(data);
        }

        private int GenerateRandomNumber()
        {
            // Generate a random number between 10000 and 99999
            Random random = new Random();
            return random.Next(10000, 100000);
        }


        private void btnPrint_Click(object sender, EventArgs e)
        {
            // Create a PrintDocument and set the print event handler
            printDocument = new PrintDocument();
            printDocument.PrintPage += PrintDocument_PrintPage;

            // Display a PrintDialog to select the printer
            PrintDialog printDialog = new PrintDialog
            {
                Document = printDocument
            };
            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                // Print the document
                printDocument.Print();
            }
        }



        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {

            // Example data to encode in the barcode
            string data = txtBarcode.Text;

            // Generate the DataMatrix barcode
            Bitmap barcodeImage = GenerateDataMatrixBarcode(data);

            // Set up label dimensions (58x40 mm at 300 dpi)
            int labelWidth = 228; // 58 mm in 300 dpi
            int labelHeight = 157; // 40 mm in 300 dpi

            // Set up barcode and text dimensions
            int barcodeWidth = 100;
            int barcodeHeight = 100;
            int textHeight = 20;

            // Calculate positions for centering
            int barcodeX = (labelWidth - barcodeWidth) / 2;
            int barcodeY = (labelHeight - (barcodeHeight + textHeight + 10)) / 2; // 10 is a small gap between the barcode and text
            int textX = barcodeX;
            int textY = barcodeY + barcodeHeight + 10;

            // Draw the barcode centered on the label
            e.Graphics.DrawImage(barcodeImage, new Rectangle(barcodeX, barcodeY, barcodeWidth, barcodeHeight));

            // Draw the text centered below the barcode
            Font font = new Font("Verdana", 18);
            SizeF textSize = e.Graphics.MeasureString(data, font);
            int centeredTextX = (labelWidth - (int)textSize.Width) / 2;

            e.Graphics.DrawString(data, font, Brushes.Black, new PointF(centeredTextX, textY));



            //////////////
            //// Example data to encode in the barcode
            //string data = txtBarcode.Text;

            //// Generate the DataMatrix barcode
            //Bitmap barcodeImage = GenerateDataMatrixBarcode(data);

            //// Set up positions and dimensions for printing
            //int labelWidth = 228; // 58 mm in 300 dpi
            //int labelHeight = 157; // 40 mm in 300 dpi
            //int barcodeWidth = 100;
            //int barcodeHeight = 100;
            //int textYPosition = barcodeHeight + 10;

            //// Draw the barcode on the label
            //e.Graphics.DrawImage(barcodeImage, new Rectangle(10, 10, barcodeWidth, barcodeHeight));

            //// Draw the text below the barcode
            //Font font = new Font("Arial", 12);
            //e.Graphics.DrawString(data, font, Brushes.Black, new PointF(10, textYPosition));
        }


        private string ExtractNumbersFromString(string input)
        {
            // Use regular expressions to find all digits in the string
            var matches = Regex.Matches(input, @"\d");

            // Concatenate all matched digits into a single string
            string result = string.Concat(matches.Cast<Match>().Select(m => m.Value));

            return result;
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void cbxProjectStorage_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void tbQuantity_TextChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                dateTimePicker1.Enabled = true;
            }
            else
            {
                dateTimePicker1.Enabled = false;
            }


        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            generateCode();
        }
    }
}

