using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
        RestTableClient materialsApi;
        List<Dictionary<string, object>> allMaterials = new List<Dictionary<string, object>>();


        public Income(string apiBaseUrl, string apiKey)
        {
            InitializeComponent();
            materialsApi = new RestTableClient(apiBaseUrl, apiKey, "materials");
            cbxSerialNumber.Checked = false;
            tbSerialNumber.Enabled = false;
            dateTimePicker1.Enabled = false;
        }




        private async void ReloadMaterialsCache()
        {
            try
            {
                allMaterials = await materialsApi.ListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                allMaterials = new List<Dictionary<string, object>>();
            }

            fillcombobox(cbxName, "cat_name");
            fillcombobox(cbxManufacturer, "manufacturer");
            fillcombobox(cbxSeller, "seller");
            fillcombobox(cbxProjectStorage, "project_storage");
            fillcombobox(cbxUnits, "units");
        }

        private void fillcombobox(ComboBox cb, string columnName)
        {
            cb.Items.Clear();
            foreach (var v in DistinctColumnValues(columnName, null))
                cb.Items.Add(v);
        }

        private IEnumerable<string> DistinctColumnValues(string columnName, string containing)
        {
            var query = allMaterials.Select(m => m.GetString(columnName)).Where(s => !string.IsNullOrEmpty(s));
            if (!string.IsNullOrEmpty(containing))
                query = query.Where(s => s.IndexOf(containing, StringComparison.OrdinalIgnoreCase) >= 0);
            return query.Distinct().OrderBy(s => s);
        }

        private Dictionary<string, object> FindMaterialBy(string columnName, string value)
        {
            return allMaterials.FirstOrDefault(m => string.Equals(m.GetString(columnName), value, StringComparison.OrdinalIgnoreCase));
        }


        private async void button1_Click(object sender, EventArgs e)
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
            string nowStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            for (int i = 0; i < positionsCount; i++)
            {
                try
                {
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

                    var fields = new Dictionary<string, object>
                    {
                        ["cat_name"] = cbxName.Text,
                        ["manufacturer"] = cbxManufacturer.Text,
                        ["seller"] = cbxSeller.Text,
                        ["sn"] = tbSerialNumber.Text,
                        ["quantity"] = Convert.ToInt32(tbQuantity.Text),
                        ["units"] = cbxUnits.Text,
                        ["order_number"] = cbxOrderNumber.Text,
                        ["project_storage"] = cbxProjectStorage.Text,
                        ["comment"] = tbComment.Text,
                        ["inventory_number"] = inventoryNumber,
                        ["size_width"] = Convert.ToInt16(tbWidth.Text),
                        ["size_depth"] = Convert.ToInt16(tbDepth.Text),
                        ["size_height"] = Convert.ToInt16(tbHeight.Text),
                        ["date_of_check"] = nowStr,
                        ["date_added"] = nowStr,
                        ["date_moved_in"] = nowStr,
                    };
                    if (checkBox1.Checked)
                        fields["date_end_warranty"] = dateTimePicker1.Value.ToString("yyyy-MM-dd HH:mm:ss");

                    await materialsApi.CreateAsync(fields);
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
            foreach (var v in DistinctColumnValues("cat_name", cbxName.Text))
                cbxName.Items.Add(v);
            cbxName.Items.Add(cbxName.Text);
            //cbxName.DroppedDown = true;
            cbxName.SelectionStart = cbxName.Text.Length;
        }

        private void cbxManufacturer_TextUpdate(object sender, EventArgs e)
        {
            cbxManufacturer.Items.Clear();
            foreach (var v in DistinctColumnValues("manufacturer", cbxManufacturer.Text))
                cbxManufacturer.Items.Add(v);
            cbxManufacturer.Items.Add(cbxManufacturer.Text);
            //cbxManufacturer.DroppedDown = true;
            cbxManufacturer.SelectionStart = cbxManufacturer.Text.Length;
        }

        private void cbxSeller_TextUpdate(object sender, EventArgs e)
        {
            cbxSeller.Items.Clear();
            foreach (var v in DistinctColumnValues("seller", cbxSeller.Text))
                cbxSeller.Items.Add(v);
            cbxSeller.Items.Add(cbxSeller.Text);
            //cbxSeller.DroppedDown = true;
            cbxSeller.SelectionStart = cbxSeller.Text.Length;
        }

        private void cbxUnits_TextUpdate(object sender, EventArgs e)
        {
            cbxUnits.Items.Clear();
            foreach (var v in DistinctColumnValues("units", cbxUnits.Text))
                cbxUnits.Items.Add(v);
            cbxUnits.Items.Add(cbxUnits.Text);
            //cbxUnits.DroppedDown = true;
            cbxUnits.SelectionStart = cbxUnits.Text.Length;
        }

        private void cbxProjectStorage_TextUpdate(object sender, EventArgs e)
        {
            cbxProjectStorage.Items.Clear();
            foreach (var v in DistinctColumnValues("project_storage", cbxProjectStorage.Text))
                cbxProjectStorage.Items.Add(v);
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
            foreach (var v in DistinctColumnValues("order_number", cbxOrderNumber.Text))
                cbxOrderNumber.Items.Add(v);
            cbxOrderNumber.Items.Add(cbxOrderNumber.Text);
            //cbxProjectStorage.DroppedDown = true;
            cbxOrderNumber.SelectionStart = cbxOrderNumber.Text.Length;
        }

        private void cbxName_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            var material = FindMaterialBy("cat_name", cbxName.Text);
            string qstr = material != null ? material.GetString("manufacturer") : "";
            cbxManufacturer.Text = qstr;
            cbxManufacturer.SelectedValue = qstr;
            cbxManufacturer.SelectedItem = qstr;
            cbxManufacturer.Text = qstr;
        }

        private void cbxName_SelectedValueChanged(object sender, EventArgs e)
        {
            var material = FindMaterialBy("cat_name", cbxName.Text);

            cbxManufacturer.Text = material != null ? material.GetString("manufacturer") : "";
            cbxSeller.Text = material != null ? material.GetString("seller") : "";
            cbxUnits.Text = material != null ? material.GetString("units") : "";
            cbxOrderNumber.Text = material != null ? material.GetString("order_number") : "";

            // Завантаження габаритів
            string qs5 = material != null ? material.GetString("size_width") : "";
            tbWidth.Text = string.IsNullOrEmpty(qs5) ? "0" : qs5;

            string qs6 = material != null ? material.GetString("size_depth") : "";
            tbDepth.Text = string.IsNullOrEmpty(qs6) ? "0" : qs6;

            string qs7 = material != null ? material.GetString("size_height") : "";
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
            ReloadMaterialsCache();
        }

        private void cbxOrderNumber_SelectedValueChanged(object sender, EventArgs e)
        {
            var material = FindMaterialBy("order_number", cbxOrderNumber.Text);

            cbxManufacturer.Text = material != null ? material.GetString("manufacturer") : "";
            cbxSeller.Text = material != null ? material.GetString("seller") : "";
            cbxUnits.Text = material != null ? material.GetString("units") : "";
            cbxName.Text = material != null ? material.GetString("cat_name") : "";

            // Завантаження габаритів
            string qs5 = material != null ? material.GetString("size_width") : "";
            tbWidth.Text = string.IsNullOrEmpty(qs5) ? "0" : qs5;

            string qs6 = material != null ? material.GetString("size_depth") : "";
            tbDepth.Text = string.IsNullOrEmpty(qs6) ? "0" : qs6;

            string qs7 = material != null ? material.GetString("size_height") : "";
            tbHeight.Text = string.IsNullOrEmpty(qs7) ? "0" : qs7;
        }
        private void cbxOrderNumber_SelectionChangeCommitted(object sender, EventArgs e)
        {

        }



        private bool isCodeExists(string code)
        {
            return allMaterials.Any(m => string.Equals(m.GetString("inventory_number"), code, StringComparison.OrdinalIgnoreCase));
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
