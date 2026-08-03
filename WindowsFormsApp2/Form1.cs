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
using Microsoft.Office.Interop.Excel;
using System.Drawing.Printing;
using ZXing;
using ZXing.Common;
using System.Diagnostics;
using ZXing.Rendering;
using System.Globalization;

using System.Text.RegularExpressions;
using System.Net.Http;




namespace InventoryFlow
{
    public partial class Form1 : Form
    {

        private PrintDocument printDocument;
        private static readonly HttpClient httpClient = new HttpClient();
        string connectionString = "";
        string printServerUrl = "";
        string apiBaseUrl = "";
        string apiKey = "";
        string filePath = Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "iflow.ini");
        int selected_id;
        RestTableClient materialsApi;
        public Form1(string user, string role)
        {
            InitializeComponent();
            getdbconnectionline();
            materialsApi = new RestTableClient(apiBaseUrl, apiKey, "materials");
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
                var lines = File.ReadAllLines(filePath)
                    .Select(l => l.Trim())
                    .Where(l => l.Length > 0 && !l.StartsWith(";"))
                    .ToArray();

                if (lines.Length > 0) connectionString = lines[0];
                if (lines.Length > 1) printServerUrl = lines[1];

                // Key=value lines (order-independent, added for the photos REST API), e.g.:
                // ApiBaseUrl=http://78.27.202.210/api
                // ApiKey=xxxxx
                foreach (var line in lines)
                {
                    if (line.StartsWith("ApiBaseUrl=", StringComparison.OrdinalIgnoreCase))
                        apiBaseUrl = line.Substring("ApiBaseUrl=".Length).Trim();
                    else if (line.StartsWith("ApiKey=", StringComparison.OrdinalIgnoreCase))
                        apiKey = line.Substring("ApiKey=".Length).Trim();
                }
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
                new Income(apiBaseUrl, apiKey).ShowDialog();

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

        private static readonly string[] GridColumns =
        {
            "id", "order_number", "inventory_number", "cat_name", "manufacturer",
            "seller", "sn", "quantity", "units", "project_storage", "comment", "date_of_check"
        };

        private static System.Data.DataTable BuildGridTable(IEnumerable<Dictionary<string, object>> rows)
        {
            var table = new System.Data.DataTable();
            foreach (var col in GridColumns)
                table.Columns.Add(col, typeof(object));

            foreach (var row in rows)
            {
                var dataRow = table.NewRow();
                foreach (var col in GridColumns)
                    dataRow[col] = (row.TryGetValue(col, out var v) && v != null) ? v : DBNull.Value;
                table.Rows.Add(dataRow);
            }
            return table;
        }

        private static decimal ToDecimalOrZero(object value)
        {
            if (value == null) return 0m;
            return decimal.TryParse(Convert.ToString(value), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : 0m;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await loadMainTable();
        }

        public async Task loadMainTable()
        {
                try
                {
                var materials = await materialsApi.ListAsync();
                if (cbShowNull.Checked)
                    materials = materials.Where(m => ToDecimalOrZero(m.ContainsKey("quantity") ? m["quantity"] : null) > 0).ToList();

                dataGridView1.DataSource = BuildGridTable(materials);
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

        private async void btnRefresh_Click(object sender, EventArgs e)
        {
            await loadMainTable();
        }

        private void btn_outcome_Click(object sender, EventArgs e)
        {
            int selectedrowindex = dataGridView1.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = dataGridView1.Rows[selectedrowindex];
            int cellValue = Convert.ToInt32(selectedRow.Cells["ID"].Value);

            // Всі потрібні поля вже є в рядку гріда (loadMainTable), запит до БД не потрібен.
            string catName = Convert.ToString(selectedRow.Cells["cat_name"].Value);
            string manufacturer = Convert.ToString(selectedRow.Cells["manufacturer"].Value);
            string seller = Convert.ToString(selectedRow.Cells["seller"].Value);
            string sn = Convert.ToString(selectedRow.Cells["sn"].Value);
            string quantity = Convert.ToString(selectedRow.Cells["quantity"].Value);
            string units = Convert.ToString(selectedRow.Cells["units"].Value);
            string order_number = Convert.ToString(selectedRow.Cells["order_number"].Value);
            string projectStorage = Convert.ToString(selectedRow.Cells["project_storage"].Value);
            string previous_storage = projectStorage;
            string comment = Convert.ToString(selectedRow.Cells["comment"].Value);

            new Outcome(apiBaseUrl, apiKey, cellValue, catName, manufacturer, seller, sn, quantity, units, order_number, projectStorage, previous_storage, comment).ShowDialog();
        }

        private async void button1_Click(object sender, EventArgs e)
        {

            if (cbxFilter.Text == "Артикул")
            {
                await filter_select("order_number");
            }
            if (cbxFilter.Text == "Назва")
            {
                await filter_select("cat_name");
            }
            if (cbxFilter.Text == "Виробник")
            {
                await filter_select("manufacturer");
            }
            if (cbxFilter.Text == "Постачальник")
            {
                await filter_select("seller");
            }
            if (cbxFilter.Text == "Серійний номер")
            {
                await filter_select("sn");
            }
            if (cbxFilter.Text == "Кількість")
            {
                await filter_select("quantity");
            }
            if (cbxFilter.Text == "Одиниці вимірювання")
            {
                await filter_select("units");
            }
            if (cbxFilter.Text == "Місце / проект")
            {
                await filter_select("project_storage");
            }
            if (cbxFilter.Text == "Коментар / теги")
            {
                await filter_select("comment");
            }

            if (cbxFilter.Text == "Інвентарний номер")
            {
                await filter_select("inventory_number");
            }

        }

        private async Task filter_select(string field)
        {
            try
            {
                var materials = await materialsApi.ListAsync();

                string needle = tbFilterField.Text ?? "";
                materials = materials
                    .Where(m => (m.ContainsKey(field) ? Convert.ToString(m[field]) : "")
                        .IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
                if (cbShowNull.Checked)
                    materials = materials.Where(m => ToDecimalOrZero(m.ContainsKey("quantity") ? m["quantity"] : null) > 0).ToList();

                dataGridView1.DataSource = BuildGridTable(materials);
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

            // Всі потрібні поля вже є в рядку гріда (loadMainTable), запит до БД не потрібен.
            string catName = Convert.ToString(selectedRow.Cells["cat_name"].Value);
            string manufacturer = Convert.ToString(selectedRow.Cells["manufacturer"].Value);
            string seller = Convert.ToString(selectedRow.Cells["seller"].Value);
            string sn = Convert.ToString(selectedRow.Cells["sn"].Value);
            string quantity = Convert.ToString(selectedRow.Cells["quantity"].Value);
            string units = Convert.ToString(selectedRow.Cells["units"].Value);
            string order_number = Convert.ToString(selectedRow.Cells["order_number"].Value);
            string projectStorage = Convert.ToString(selectedRow.Cells["project_storage"].Value);
            string previous_storage = projectStorage;
            string comment = Convert.ToString(selectedRow.Cells["comment"].Value);

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

        private async void button4_Click(object sender, EventArgs e) //print_sticker
        {
            //string code = lblCodeValue.Text;
            //if (string.IsNullOrEmpty(code))
            //{
            //    MessageBox.Show("Оберіть позицію у таблиці", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

            //try
            //{
            //    string json = "{\"code\":\"" + code + "\"}";
            //    var content = new StringContent(json, Encoding.UTF8, "application/json");
            //    var response = await httpClient.PostAsync(printServerUrl, content);

            //    if (response.IsSuccessStatusCode)
            //    {
            //        MessageBox.Show("Друк відправлено: " + code, "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    }
            //    else
            //    {
            //        MessageBox.Show("Помилка сервера: " + (int)response.StatusCode, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Не вдалося підключитися до принтера:\n" + ex.Message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}

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



        private async void button5_Click(object sender, EventArgs e)
        {

            DialogResult result = MessageBox.Show("Do you want to proceed?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                int selectedrowindex = dataGridView1.SelectedCells[0].RowIndex;
                DataGridViewRow selectedRow = dataGridView1.Rows[selectedrowindex];
                int cellValue = Convert.ToInt32(selectedRow.Cells["ID"].Value);

                try
                {
                    await materialsApi.DeleteAsync(cellValue);
                    await loadMainTable();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Convert.ToString(ex), "Помилка видалення", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void перенесенняToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int selectedrowindex = dataGridView1.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = dataGridView1.Rows[selectedrowindex];
            int cellValue = Convert.ToInt32(selectedRow.Cells["ID"].Value);

            // Всі потрібні поля вже є в рядку гріда (loadMainTable), запит до БД не потрібен.
            string catName = Convert.ToString(selectedRow.Cells["cat_name"].Value);
            string manufacturer = Convert.ToString(selectedRow.Cells["manufacturer"].Value);
            string seller = Convert.ToString(selectedRow.Cells["seller"].Value);
            string sn = Convert.ToString(selectedRow.Cells["sn"].Value);
            string quantity = Convert.ToString(selectedRow.Cells["quantity"].Value);
            string units = Convert.ToString(selectedRow.Cells["units"].Value);
            string order_number = Convert.ToString(selectedRow.Cells["order_number"].Value);
            string projectStorage = Convert.ToString(selectedRow.Cells["project_storage"].Value);
            string previous_storage = projectStorage;
            string comment = Convert.ToString(selectedRow.Cells["comment"].Value);

            new Outcome(apiBaseUrl, apiKey, cellValue, catName, manufacturer, seller, sn, quantity, units, order_number, projectStorage, previous_storage, comment).ShowDialog();
        }

        private async void оновитиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await loadMainTable();
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
                // Інвентарний номер вже є в рядку гріда (loadMainTable), запит до БД не потрібен.
                lblCodeValue.Text = Convert.ToString(selectedRow.Cells["inventory_number"].Value);

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
        //private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        //{

        //    // Example data to encode in the barcode
        //    string data = lblCodeValue.Text;

        //    // Generate the DataMatrix barcode
        //    Bitmap barcodeImage = GenerateDataMatrixBarcode(data);

        //    // Set up label dimensions (58x40 mm at 300 dpi)
        //    int labelWidth = 228; // 58 mm in 300 dpi
        //    int labelHeight = 157; // 40 mm in 300 dpi

        //    // Set up barcode and text dimensions
        //    int barcodeWidth = 100;
        //    int barcodeHeight = 100;
        //    int textHeight = 20;

        //    // Calculate positions for centering
        //    int barcodeX = (labelWidth - barcodeWidth) / 2;
        //    int barcodeY = (labelHeight - (barcodeHeight + textHeight + 10)) / 2; // 10 is a small gap between the barcode and text
        //    int textX = barcodeX;
        //    int textY = barcodeY + barcodeHeight + 10;

        //    // Draw the barcode centered on the label
        //    e.Graphics.DrawImage(barcodeImage, new System.Drawing.Rectangle(barcodeX, barcodeY, barcodeWidth, barcodeHeight));

        //    // Draw the text centered below the barcode
        //    System.Drawing.Font font = new System.Drawing.Font("Verdana", 18);
        //    SizeF textSize = e.Graphics.MeasureString(data, font);
        //    int centeredTextX = (labelWidth - (int)textSize.Width) / 2;

        //    e.Graphics.DrawString(data, font, Brushes.Black, new PointF(centeredTextX, textY));
        //}
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
        private async void button8_Click(object sender, EventArgs e)
        {
            //new ChangeStorage().ShowDialog();
            int selectedrowindex = dataGridView1.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = dataGridView1.Rows[selectedrowindex];
            int cellValue = Convert.ToInt32(selectedRow.Cells["ID"].Value);

            Dictionary<string, object> material;
            try
            {
                material = await materialsApi.GetAsync(cellValue);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex), "Помилка завантаження позиції", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string F(string key) => material.TryGetValue(key, out var v) && v != null ? Convert.ToString(v) : "";

            string id = Convert.ToString(cellValue);
            string cat_name = F("cat_name");
            string manufacturer = F("manufacturer");
            string seller = F("seller");
            string sn = F("sn");
            string quantity = F("quantity");
            string units = F("units");
            string order_number = F("order_number");
            string project_storage = F("project_storage");
            string comment = F("comment");
            string inventory_number = F("inventory_number");
            string size_width = F("size_width");
            string size_depth = F("size_depth");
            string size_height = F("size_height");
            string date_of_check = F("date_of_check");
            string date_added = F("date_added");
            string date_moved_in = F("date_moved_in");
            string date_moved_out = F("date_moved_out");
            string date_of_maintenance = F("date_of_maintenance");
            string date_end_warranty = F("date_end_warranty");

            new ChangeStorage(apiBaseUrl, apiKey,
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
            new InfoTab(apiBaseUrl, apiKey, selected_id).ShowDialog();
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

                if (string.IsNullOrEmpty(apiBaseUrl))
                {
                    MessageBox.Show("Не задано ApiBaseUrl у файлі конфігурації (iflow.ini).", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                new PhotoManager(apiBaseUrl, apiKey, inventoryNumber).ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при роботі з фото: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void cbShowNull_CheckedChanged(object sender, EventArgs e)
        {
            await loadMainTable();
        }
    }
}
