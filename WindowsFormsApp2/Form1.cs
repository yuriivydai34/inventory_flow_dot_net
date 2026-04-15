using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using MySql;
using MySql.Data.MySqlClient;
using Microsoft.Office.Interop.Excel;
using System.Drawing.Printing;
using ZXing;
using ZXing.Common;
using System.Diagnostics;
using ZXing.Rendering;
using System.Globalization;

using System.Text.RegularExpressions;




namespace InventoryFlow
{
    public partial class Form1 : Form
    {
        private PrintDocument printDocument;
        string connectionString = "";
        string filePath = "iflow.ini";
        int selected_id;
        public Form1(string user, string role)
        {
            InitializeComponent();
            getdbconnectionline();
            WindowState = FormWindowState.Maximized;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            cbxFilter.Items.Add("Артикул");
            cbxFilter.Items.Add("Назва");
            cbxFilter.Items.Add("Виробник");
            cbxFilter.Items.Add("Постачальник");
            cbxFilter.Items.Add("Серійний номер");
            cbxFilter.Items.Add("Кількість");
            cbxFilter.Items.Add("Одиниці вимірювання");
            cbxFilter.Items.Add("Місце / проект");
            cbxFilter.Items.Add("Коментар / теги");
            cbxFilter.Items.Add("Інвентарний номер");
            cbxFilter.SelectedIndex = 9;
            lbUser.Text = user;
            
            //lbGroup.Text = role;
        }

        private void getdbconnectionline()
        {

            if (File.Exists(filePath))
            {
                // Read all lines from the file
                string[] lines = File.ReadAllLines(filePath);
                connectionString = lines[0];
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void LogMessage(string message)
        {
            try
            {
                string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log.log");
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string logEntry = $"{timestamp} - {message}";

                // Append the log entry to the file using UTF-8 encoding
                File.AppendAllText(logFilePath, logEntry + Environment.NewLine, System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                // Optional: Handle logging errors 
                // You might want to use a fallback logging method or show a message box
                MessageBox.Show($"Error writing to log file: {ex.Message}", "Logging Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_income_Click(object sender, EventArgs e)
        {
            try
            {
                //LogMessage(connectionString);
                new Income(connectionString).ShowDialog();

            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error755", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }






        private void AuditLogEntry(string message)
        {
            using (StreamWriter streamWriter = File.AppendText(Directory.GetCurrentDirectory() + "/AuditLog.alog"))
            {

                string readAllLine = File.ReadAllLines(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "/wfs.gvi")[0];
                string str = DateTime.Now.ToString("HH:mm:ss dd.MM.yyyy ") + " - " + readAllLine + " - " + message;
                streamWriter.WriteLine(str);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            loadMainTable();
        }

        public void loadMainTable()
        {
                MySqlConnection connection = new MySqlConnection(connectionString);
                try
                {
                    connection.Open();

                string query = "SELECT id, order_number, inventory_number, cat_name, manufacturer, seller, sn, quantity, units, project_storage, comment, date_of_check FROM materials";
                if (cbShowNull.Checked)
                {
                    query += " WHERE quantity > 0";
                }
                query += ";";
                MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(query, connection);




                DataSet dataSet1 = new DataSet();
                    connection.Close();
                    DataSet dataSet2 = dataSet1;
                    mySqlDataAdapter.Fill(dataSet2);
                    dataGridView1.DataSource = dataSet1.Tables[0];
                }
                catch (Exception ex)
                {
                    int num = (int)MessageBox.Show(Convert.ToString(ex), "Exception main tab sql", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                try
                {
                    dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.RowCount - 1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message, "Error642", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
                dataGridView1.Columns[0].HeaderCell.Value = "ID";
                dataGridView1.Columns[1].HeaderCell.Value = "Артикул";
                dataGridView1.Columns[2].HeaderCell.Value = "Інвентарний номер";
                dataGridView1.Columns[3].HeaderCell.Value = "Назва";
                dataGridView1.Columns[4].HeaderCell.Value = "Виробник";
                dataGridView1.Columns[5].HeaderCell.Value = "Постачальник";
                dataGridView1.Columns[6].HeaderCell.Value = "Серійний номер";
                dataGridView1.Columns[7].HeaderCell.Value = "Кількість";
                dataGridView1.Columns[8].HeaderCell.Value = "Одиниці виміру";
                dataGridView1.Columns[9].HeaderCell.Value = "Проект/Склад";
                dataGridView1.Columns[10].HeaderCell.Value = "Коментар";


                dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView1.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dataGridView1.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView1.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView1.Columns[5].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView1.Columns[9].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            HighlightRowsBasedOnDate();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            loadMainTable();
        }

        private void btn_outcome_Click(object sender, EventArgs e)
        {
            int selectedrowindex = dataGridView1.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = dataGridView1.Rows[selectedrowindex];
            int cellValue = Convert.ToInt32(selectedRow.Cells["ID"].Value);

            MySqlConnection connection1 = new MySqlConnection(connectionString);
            //int quantityselected;
            string catName = "";
            string manufacturer = "";
            string seller = "";
            string sn = "";
            string quantity = "";
            string units = "";
            string order_number = "";
            string projectStorage = "";
            string previous_storage = "";
            string comment = "";
            string dtstring = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string datetime = "";

            connection1.Open();
            catName = Convert.ToString(new MySqlCommand(@"SELECT cat_name FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
            manufacturer = Convert.ToString(new MySqlCommand(@"SELECT manufacturer FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
            seller = Convert.ToString(new MySqlCommand(@"SELECT seller FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
            sn = Convert.ToString(new MySqlCommand(@"SELECT sn FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
            quantity = Convert.ToString(new MySqlCommand(@"SELECT quantity FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
            units = Convert.ToString(new MySqlCommand(@"SELECT units FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
            order_number = Convert.ToString(new MySqlCommand(@"SELECT order_number FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
            projectStorage = Convert.ToString(new MySqlCommand(@"SELECT project_storage FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
            previous_storage = Convert.ToString(new MySqlCommand(@"SELECT project_storage FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
            comment = Convert.ToString(new MySqlCommand(@"SELECT comment FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
            datetime = dtstring;


            connection1.Close();
            new Outcome(connectionString, cellValue, catName, manufacturer, seller, sn, quantity, units, order_number, projectStorage, previous_storage, comment).ShowDialog();
            //int matID, int currentQuantity, string catName, string units, string projectstorage)


        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (cbxFilter.Text == "Артикул")
            {
                filter_select("order_number");
            }
            if (cbxFilter.Text == "Назва")
            {
                filter_select("cat_name");
            }
            if (cbxFilter.Text == "Виробник")
            {
                filter_select("manufacturer");
            }
            if (cbxFilter.Text == "Постачальник")
            {
                filter_select("seller");
            }
            if (cbxFilter.Text == "Серійний номер")
            {
                filter_select("sn");
            }
            if (cbxFilter.Text == "Кількість")
            {
                filter_select("quantity");
            }
            if (cbxFilter.Text == "Одиниці вимірювання")
            {
                filter_select("units");
            }
            if (cbxFilter.Text == "Місце / проект")
            {
                filter_select("project_storage");
            }
            if (cbxFilter.Text == "Коментар / теги")
            {
                filter_select("comment");
            }

            if (cbxFilter.Text == "Інвентарний номер")
            {
                filter_select("inventory_number");
            }
            
        }

        private void filter_select(string field)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();

                string query = @"SELECT id, order_number, inventory_number, cat_name, manufacturer, seller, sn, quantity, units, project_storage, comment, date_of_check FROM materials WHERE " + field + " LIKE '%" + tbFilterField.Text + "%'";
                if (cbShowNull.Checked)
                {
                    query += " AND quantity > 0";
                }
                query += ";";
                MySqlDataAdapter mySqlDataAdapter = new MySqlDataAdapter(query, connection);

                DataSet dataSet1 = new DataSet();
                connection.Close();
                DataSet dataSet2 = dataSet1;
                mySqlDataAdapter.Fill(dataSet2);
                dataGridView1.DataSource = (object)dataSet1.Tables[0];
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show(Convert.ToString((object)ex), "Exception main tab sql", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            try
            {
                dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.RowCount - 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error602", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            dataGridView1.Columns[0].HeaderCell.Value = "ID";
            dataGridView1.Columns[1].HeaderCell.Value = "Номер для заказу";
            dataGridView1.Columns[2].HeaderCell.Value = "Інвентарний номер"; 
            dataGridView1.Columns[3].HeaderCell.Value = "Назва";
            dataGridView1.Columns[4].HeaderCell.Value = "Виробник";
            dataGridView1.Columns[5].HeaderCell.Value = "Продавець";
            dataGridView1.Columns[6].HeaderCell.Value = "Серійний номер";
            dataGridView1.Columns[7].HeaderCell.Value = "Кількість";
            dataGridView1.Columns[8].HeaderCell.Value = "Одиниці вимірювання";
            dataGridView1.Columns[9].HeaderCell.Value = "Місце / проект";
            dataGridView1.Columns[10].HeaderCell.Value = "Коментар / теги";





            dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView1.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView1.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView1.Columns[5].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView1.Columns[9].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

        }

        private void button2_Click(object sender, EventArgs e) // change storage
        {
            //new ChangeStorage().ShowDialog();
            int selectedrowindex = dataGridView1.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = dataGridView1.Rows[selectedrowindex];
            int cellValue = Convert.ToInt32(selectedRow.Cells["ID"].Value);

            MySqlConnection connection1 = new MySqlConnection(connectionString);
            //int quantityselected;
            string catName = "";
            string manufacturer = "";
            string seller = "";
            string sn = "";
            string quantity = "";
            string units = "";
            string order_number = "";
            string projectStorage = "";
            string previous_storage = "";
            string comment = "";
            string dtstring = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string datetime = "";

            connection1.Open();
            catName = Convert.ToString(new MySqlCommand(@"SELECT cat_name FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
            manufacturer = Convert.ToString(new MySqlCommand(@"SELECT manufacturer FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
            seller = Convert.ToString(new MySqlCommand(@"SELECT seller FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
            sn = Convert.ToString(new MySqlCommand(@"SELECT sn FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
            quantity = Convert.ToString(new MySqlCommand(@"SELECT quantity FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
            units = Convert.ToString(new MySqlCommand(@"SELECT units FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
            order_number = Convert.ToString(new MySqlCommand(@"SELECT order_number FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
            projectStorage = Convert.ToString(new MySqlCommand(@"SELECT project_storage FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
            previous_storage = Convert.ToString(new MySqlCommand(@"SELECT project_storage FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
            comment = Convert.ToString(new MySqlCommand(@"SELECT comment FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
            datetime = dtstring;


            connection1.Close();
            //new ChangeStorage(connectionString, cellValue, catName, manufacturer, seller, sn, quantity, units, order_number, projectStorage, previous_storage, comment).ShowDialog();
            //int matID, int currentQuantity, string catName, string units, string projectstorage)
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                // Create a new Excel application
                var excelApp = new Microsoft.Office.Interop.Excel.Application();

                // Create a new workbook
                var workbook = excelApp.Workbooks.Add();

                // Create a new worksheet
                var worksheet = (Worksheet)workbook.Worksheets[1];

                // Set the header row
                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                {
                    worksheet.Cells[1, i + 1] = dataGridView1.Columns[i].HeaderText;
                }

                // Populate the data rows
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    for (int j = 0; j < dataGridView1.Columns.Count; j++)
                    {
                        worksheet.Cells[i + 2, j + 1] = dataGridView1.Rows[i].Cells[j].Value.ToString();
                    }
                }

                // Save the workbook
                string folderPath = Directory.GetCurrentDirectory() + @"/Exported";
                string fileName = "ExportedData_" + DateTime.Now.ToString("dd-MMM-yyyy") + ".xlsx";
                string filePath = System.IO.Path.Combine(folderPath, fileName);

                workbook.SaveAs(filePath);

                // Close the workbook and Excel application
                workbook.Close();
                excelApp.Quit();

                Process.Start(folderPath);

                //MessageBox.Show("Success", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);


            }
            catch(Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error864", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button4_Click(object sender, EventArgs e) //print_sticker
        {
            //get positions' inventory number
            // Example data to encode in the barcode


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

        private void button5_Click(object sender, EventArgs e)
        {

            DialogResult result = MessageBox.Show("Do you want to proceed?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                int selectedrowindex = dataGridView1.SelectedCells[0].RowIndex;
                DataGridViewRow selectedRow = dataGridView1.Rows[selectedrowindex];
                int cellValue = Convert.ToInt32(selectedRow.Cells["ID"].Value);


                MySqlConnection connection1 = new MySqlConnection(connectionString);
                connection1.Open();
                string cmdText = string.Format(@"DELETE FROM materials WHERE id = '" + cellValue + "' ");
                new MySqlCommand(cmdText, connection1).ExecuteNonQuery();
                connection1.Close();
                loadMainTable();

            }
            else
            {

            }
        }

        private void перенесенняToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int selectedrowindex = dataGridView1.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = dataGridView1.Rows[selectedrowindex];
            int cellValue = Convert.ToInt32(selectedRow.Cells["ID"].Value);

            MySqlConnection connection1 = new MySqlConnection(connectionString);
            //int quantityselected;
            string catName = "";
            string manufacturer = "";
            string seller = "";
            string sn = "";
            string quantity = "";
            string units = "";
            string order_number = "";
            string projectStorage = "";
            string previous_storage = "";
            string comment = "";
            string dtstring = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string datetime = "";

            connection1.Open();
            catName = Convert.ToString(new MySqlCommand(@"SELECT cat_name FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
            manufacturer = Convert.ToString(new MySqlCommand(@"SELECT manufacturer FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
            seller = Convert.ToString(new MySqlCommand(@"SELECT seller FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
            sn = Convert.ToString(new MySqlCommand(@"SELECT sn FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
            quantity = Convert.ToString(new MySqlCommand(@"SELECT quantity FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
            units = Convert.ToString(new MySqlCommand(@"SELECT units FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
            order_number = Convert.ToString(new MySqlCommand(@"SELECT order_number FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
            projectStorage = Convert.ToString(new MySqlCommand(@"SELECT project_storage FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
            previous_storage = Convert.ToString(new MySqlCommand(@"SELECT project_storage FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
            comment = Convert.ToString(new MySqlCommand(@"SELECT comment FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
            datetime = dtstring;


            connection1.Close();
            new Outcome(connectionString, cellValue, catName, manufacturer, seller, sn, quantity, units, order_number, projectStorage, previous_storage, comment).ShowDialog();
            //int matID, int currentQuantity, string catName, string units, string projectstorage)



        }

        private void оновитиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadMainTable();
        }

        private string mysqlSelectOneValue(string query)
        {
            MySqlConnection connection1 = new MySqlConnection(connectionString);
            connection1.Open();
            string result = Convert.ToString(new MySqlCommand(query, connection1).ExecuteScalar());
            connection1.Close();
            return result;
        }
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int cellValue = 0;
            int selectedrowindex = dataGridView1.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = dataGridView1.Rows[selectedrowindex];
            cellValue = Convert.ToInt32(selectedRow.Cells["ID"].Value);
            selected_id = cellValue; 
            try
            {
                MySqlConnection connection1 = new MySqlConnection(connectionString);
                connection1.Open();
                lblCodeValue.Text = Convert.ToString(new MySqlCommand(@"SELECT inventory_number FROM materials WHERE id= '" + cellValue + "';", connection1).ExecuteScalar());
                connection1.Close();

                // Display the result in the output TextBox


                string data = lblCodeValue.Text;
                var barcodeBitmap = GenerateDataMatrixBarcode(data);
                picCodeM.Image = barcodeBitmap;
            }
            catch(Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex));
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

            return writer.Write(data);
        }
        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {

            // Example data to encode in the barcode
            string data = lblCodeValue.Text;

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
            e.Graphics.DrawImage(barcodeImage, new System.Drawing.Rectangle(barcodeX, barcodeY, barcodeWidth, barcodeHeight));

            // Draw the text centered below the barcode
            System.Drawing.Font font = new System.Drawing.Font("Verdana", 18);
            SizeF textSize = e.Graphics.MeasureString(data, font);
            int centeredTextX = (labelWidth - (int)textSize.Width) / 2;

            e.Graphics.DrawString(data, font, Brushes.Black, new PointF(centeredTextX, textY));
        }
        private void button2_Click_1(object sender, EventArgs e)
        {
            int selectedrowindex = dataGridView1.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = dataGridView1.Rows[selectedrowindex];
            int cellValue = Convert.ToInt32(selectedRow.Cells["ID"].Value);

            new Edit_Position(cellValue, connectionString).ShowDialog();
        }
        private void button7_Click(object sender, EventArgs e)
        {

            new Login().Show();
            //Hide();

        }
        private void button6_Click(object sender, EventArgs e)
        {
            new Scan(connectionString).ShowDialog();

        }
        private void HighlightRowsBasedOnDate()
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                // Skip empty or new rows
                if (row.IsNewRow) continue;

                string dateValue = row.Cells["date_of_check"].Value?.ToString();
                if (string.IsNullOrWhiteSpace(dateValue))
                {
                    // Highlight rows with an empty date
                    row.DefaultCellStyle.BackColor = Color.LightGray;
                    continue;
                }

                if (DateTime.TryParse(dateValue, out DateTime dateOfCheck))
                {
                    // Check the age of the date
                    if ((DateTime.Now - dateOfCheck).TotalDays > 365)
                    {
                        // Date is older than one year
                        row.DefaultCellStyle.BackColor = Color.LightPink;
                    }
                    else
                    {
                        // Date is within one year
                        row.DefaultCellStyle.BackColor = Color.PaleGreen;
                    }
                }
                else
                {
                    // If parsing fails, treat it as an invalid date
                    row.DefaultCellStyle.BackColor = Color.Red;
                }
            }
        }
        private void button8_Click(object sender, EventArgs e)
        {
            //new ChangeStorage().ShowDialog();
            int selectedrowindex = dataGridView1.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = dataGridView1.Rows[selectedrowindex];
            int cellValue = Convert.ToInt32(selectedRow.Cells["ID"].Value);

            MySqlConnection connection1 = new MySqlConnection(connectionString);
            //int quantityselected;


            string id = Convert.ToString(cellValue);
            string cat_name = mysqlSelectOneValue(@"SELECT cat_name FROM materials WHERE id= '" + cellValue + "';");
            string manufacturer = mysqlSelectOneValue(@"SELECT manufacturer FROM materials WHERE id= '" + cellValue + "';");
            string seller = mysqlSelectOneValue(@"SELECT seller FROM materials WHERE id= '" + cellValue + "';");
            string sn = mysqlSelectOneValue(@"SELECT sn FROM materials WHERE id= '" + cellValue + "';");
            string quantity = mysqlSelectOneValue(@"SELECT quantity FROM materials WHERE id= '" + cellValue + "';");
            string units = mysqlSelectOneValue(@"SELECT units FROM materials WHERE id= '" + cellValue + "';");
            string order_number = mysqlSelectOneValue(@"SELECT order_number FROM materials WHERE id= '" + cellValue + "';");
            string project_storage = mysqlSelectOneValue(@"SELECT project_storage FROM materials WHERE id= '" + cellValue + "';");
            string comment = mysqlSelectOneValue(@"SELECT comment FROM materials WHERE id= '" + cellValue + "';");
            string inventory_number = mysqlSelectOneValue(@"SELECT inventory_number FROM materials WHERE id= '" + cellValue + "';");
            string size_width = mysqlSelectOneValue(@"SELECT size_width FROM materials WHERE id= '" + cellValue + "';");
            string size_depth = mysqlSelectOneValue(@"SELECT size_depth FROM materials WHERE id= '" + cellValue + "';");
            string size_height = mysqlSelectOneValue(@"SELECT size_height FROM materials WHERE id= '" + cellValue + "';");
            string date_of_check = mysqlSelectOneValue(@"SELECT DATE_FORMAT(date_of_check, '%Y-%m-%d %H:%i:%s') FROM materials WHERE id= '" + cellValue + "';");
            string date_added = mysqlSelectOneValue(@"SELECT DATE_FORMAT(date_added, '%Y-%m-%d %H:%i:%s') FROM materials WHERE id= '" + cellValue + "';");
            string date_moved_in = mysqlSelectOneValue(@"SELECT DATE_FORMAT(date_moved_in, '%Y-%m-%d %H:%i:%s') FROM materials WHERE id= '" + cellValue + "';");
            string date_moved_out = mysqlSelectOneValue(@"SELECT DATE_FORMAT(date_moved_out, '%Y-%m-%d %H:%i:%s') FROM materials WHERE id= '" + cellValue + "';");
            string date_of_maintenance = mysqlSelectOneValue(@"SELECT DATE_FORMAT(date_of_maintenance, '%Y-%m-%d %H:%i:%s') FROM materials WHERE id= '" + cellValue + "';");
            string date_end_warranty = mysqlSelectOneValue(@"SELECT DATE_FORMAT(date_end_warranty, '%Y-%m-%d %H:%i:%s') FROM materials WHERE id= '" + cellValue + "';");

            connection1.Open();
            connection1.Close();
            new ChangeStorage(connectionString,
                id,
                cat_name,
                manufacturer,
                seller,
                sn,
                quantity,
                units,
                order_number,
                project_storage,
                comment,
                inventory_number,
                size_width,
                size_depth,
                size_height,
                date_of_check,
                date_added,
                date_moved_in,
                date_moved_out,
                date_of_maintenance,
                date_end_warranty
).ShowDialog();


    }
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            new InfoTab(connectionString, selected_id).ShowDialog();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                // Перевіряємо, чи обрано рядок у таблиці
                if (dataGridView1.SelectedRows.Count == 0 && dataGridView1.SelectedCells.Count == 0)
                {
                    MessageBox.Show("Будь ласка, оберіть рядок у таблиці.", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Отримуємо обраний рядок
                int selectedRowIndex = dataGridView1.SelectedCells[0].RowIndex;
                DataGridViewRow selectedRow = dataGridView1.Rows[selectedRowIndex];

                // Отримуємо інвентарний номер з обраного рядка
                var inventoryNumberCell = selectedRow.Cells["inventory_number"].Value;

                if (inventoryNumberCell == null || string.IsNullOrWhiteSpace(inventoryNumberCell.ToString()))
                {
                    MessageBox.Show("Інвентарний номер не заповнений для обраного рядка.", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string inventoryNumber = inventoryNumberCell.ToString().Trim();

                // Очищуємо назву папки від недопустимих символів
                string folderName = inventoryNumber;
                char[] invalidChars = Path.GetInvalidFileNameChars();
                foreach (char invalidChar in invalidChars)
                {
                    folderName = folderName.Replace(invalidChar, '_');
                }

                // Створюємо шлях до папки
                string exeDirectory = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath); string inventoryFilesPath = Path.Combine(exeDirectory, "InventoryFiles");
                string targetFolderPath = Path.Combine(inventoryFilesPath, folderName);

                // Створюємо папку InventoryFiles, якщо вона не існує
                if (!Directory.Exists(inventoryFilesPath))
                {
                    Directory.CreateDirectory(inventoryFilesPath);
                }

                // Створюємо цільову папку, якщо вона не існує
                if (!Directory.Exists(targetFolderPath))
                {
                    Directory.CreateDirectory(targetFolderPath);
                }

                // Відкриваємо папку в провіднику
                Process.Start("explorer.exe", targetFolderPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при роботі з папкою: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cbShowNull_CheckedChanged(object sender, EventArgs e)
        {
            loadMainTable();
        }
    }
}
