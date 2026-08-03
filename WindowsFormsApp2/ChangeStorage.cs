using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;

namespace InventoryFlow
{
    public partial class ChangeStorage : Form
    {

        RestTableClient materialsApi;
        List<Dictionary<string, object>> allMaterials = new List<Dictionary<string, object>>();
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
            string apiBaseUrl,
            string apiKey,
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

            materialsApi = new RestTableClient(apiBaseUrl, apiKey, "materials");
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

            Load += async (s, e) => await fill_combobox();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            int quantity_old = Convert.ToInt32(lblCurrentQuantity.Text);
            int quantity_new = Convert.ToInt32(tbCurrentQuantity.Text);
            //розраховуємо залишкову кільість на попередньому складі
            int quantity_to_update = (quantity_old - quantity_new);
            string storage_new = cbProjectStorage.Text;

            //перевіряємо, чи не більше нова кількість за стару
            if (quantity_new > quantity_old)
            {
                MessageBox.Show("Невірно вказано кількість", "Помилка!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //перевіряємо, чи не дорівнює новий склад старому
            if (vproject_storage == cbProjectStorage.Text)
            {
                MessageBox.Show("Невірно вказано склад", "Помилка!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var materials = await materialsApi.ListAsync();

                //перевіряємо, чи є вже у project_storage даний cat_name
                var existing = materials.FirstOrDefault(m =>
                    m.GetString("cat_name") == vcat_name && m.GetString("project_storage") == storage_new);

                bool hasSerial = !string.IsNullOrWhiteSpace(vsn);

                if (existing != null && !hasSerial)
                {
                    // додаємо до існуючої кількості на складі (немає серійного номеру)
                    int existingQty = Convert.ToInt32(ToNumberString(existing.GetString("quantity")));
                    int existingId = Convert.ToInt32(existing.GetString("id"));
                    await materialsApi.UpdateAsync(existingId, new Dictionary<string, object>
                    {
                        ["quantity"] = quantity_new + existingQty
                    });
                }
                else
                {
                    // серійний номер є (окремий запис завжди) АБО на новому складі такої позиції ще немає
                    await CreateMovedRecordAsync(quantity_new, storage_new);
                }

                // update кількості на старому складі
                await materialsApi.UpdateAsync(Convert.ToInt32(vid), new Dictionary<string, object>
                {
                    ["quantity"] = quantity_to_update,
                    ["date_moved_out"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                });

                ((Form1)Application.OpenForms["Form1"]).loadMainTable();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex), "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task CreateMovedRecordAsync(int quantity_new, string storage_new)
        {
            var fields = new Dictionary<string, object>
            {
                ["cat_name"] = vcat_name,
                ["manufacturer"] = vmanufacturer,
                ["seller"] = vseller,
                ["sn"] = vsn,
                ["quantity"] = quantity_new,
                ["units"] = vunits,
                ["order_number"] = vorder_number,
                ["project_storage"] = storage_new,
                ["comment"] = vcomment,
                ["inventory_number"] = vinventory_number,
                ["size_width"] = string.IsNullOrEmpty(vsize_width) ? 0 : Convert.ToInt32(ToNumberString(vsize_width)),
                ["size_depth"] = string.IsNullOrEmpty(vsize_depth) ? 0 : Convert.ToInt32(ToNumberString(vsize_depth)),
                ["size_height"] = string.IsNullOrEmpty(vsize_height) ? 0 : Convert.ToInt32(ToNumberString(vsize_height)),
                ["date_moved_in"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            };

            AddDateIfValid(fields, "date_of_check", vdate_of_check);
            AddDateIfValid(fields, "date_added", vdate_added);
            AddDateIfValid(fields, "date_of_maintenance", vdate_of_maintenance);
            AddDateIfValid(fields, "date_end_warranty", vdate_end_warranty);

            await materialsApi.CreateAsync(fields);
        }

        // Значення полів приходять із REST API (ISO 8601 чи вже "yyyy-MM-dd HH:mm:ss") — приймаємо обидва формати.
        private static void AddDateIfValid(Dictionary<string, object> fields, string key, string inputDate)
        {
            if (string.IsNullOrWhiteSpace(inputDate)) return;
            if (DateTime.TryParse(inputDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                fields[key] = parsed.ToString("yyyy-MM-dd HH:mm:ss");
        }

        // "7" чи "7.0" з API -> нормалізований рядок для Convert.ToInt32
        private static string ToNumberString(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "0";
            return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d.ToString(CultureInfo.InvariantCulture) : value;
        }

        private async Task fill_combobox()
        {
            try
            {
                allMaterials = await materialsApi.ListAsync();
                cbProjectStorage.Items.Clear();
                foreach (var v in DistinctProjectStorages(null))
                    cbProjectStorage.Items.Add(v);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex), "Error837", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private IEnumerable<string> DistinctProjectStorages(string containing)
        {
            var query = allMaterials.Select(m => m.GetString("project_storage")).Where(s => !string.IsNullOrEmpty(s));
            if (!string.IsNullOrEmpty(containing))
                query = query.Where(s => s.IndexOf(containing, StringComparison.OrdinalIgnoreCase) >= 0);
            return query.Distinct().OrderBy(s => s);
        }

        private void btnClose_Click_1(object sender, EventArgs e)
        {
            Close();
        }

        private void cbProjectStorage_TextUpdate(object sender, EventArgs e)
        {
            cbProjectStorage.Items.Clear();
            foreach (var v in DistinctProjectStorages(cbProjectStorage.Text))
                cbProjectStorage.Items.Add(v);

            if (!string.IsNullOrEmpty(cbProjectStorage.Text) && !cbProjectStorage.Items.Contains(cbProjectStorage.Text))
                cbProjectStorage.Items.Add(cbProjectStorage.Text);

            cbProjectStorage.SelectionStart = cbProjectStorage.Text.Length;
        }

        private void cbProjectStorage_Click(object sender, EventArgs e)
        {
            int itemHeight = cbProjectStorage.ItemHeight;
            cbProjectStorage.DropDownHeight = cbProjectStorage.Height + (itemHeight * 10);
            cbProjectStorage.DroppedDown = true;
        }
    }
}
