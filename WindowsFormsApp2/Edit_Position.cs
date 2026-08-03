using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;


namespace InventoryFlow
{
    public partial class Edit_Position : Form
    {
        string mid;
        RestTableClient materialsApi;
        public Edit_Position(int matID, string apiBaseUrl, string apiKey)
        {
            InitializeComponent();
            materialsApi = new RestTableClient(apiBaseUrl, apiKey, "materials");
            mid = Convert.ToString(matID);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            await setValues();
            ((Form1)Application.OpenForms["Form1"]).loadMainTable();
            Close();
        }

        private async void Edit_Position_Load(object sender, EventArgs e)
        {
            try
            {
                var material = await materialsApi.GetAsync(Convert.ToInt32(mid));
                tbName.Text = material.GetString("cat_name");
                tbOrderNum.Text = material.GetString("order_number");
                tbManufacturer.Text = material.GetString("manufacturer");
                tbSeller.Text = material.GetString("seller");
                tbSN.Text = material.GetString("sn");
                tbQty.Text = material.GetString("quantity");
                tbUnits.Text = material.GetString("units");
                tbPlace.Text = material.GetString("project_storage");
                tbComment.Text = material.GetString("comment");
                tbInvNum.Text = material.GetString("inventory_number");
                tbWidth.Text = material.GetString("size_width");
                tbDepth.Text = material.GetString("size_depth");
                tbHeight.Text = material.GetString("size_height");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private async Task setValues()
        {
            try
            {
                await materialsApi.UpdateAsync(Convert.ToInt32(mid), new Dictionary<string, object>
                {
                    ["cat_name"] = tbName.Text,
                    ["manufacturer"] = tbManufacturer.Text,
                    ["seller"] = tbSeller.Text,
                    ["sn"] = tbSN.Text,
                    ["quantity"] = tbQty.Text,
                    ["units"] = tbUnits.Text,
                    ["order_number"] = tbOrderNum.Text,
                    ["project_storage"] = tbPlace.Text,
                    ["comment"] = tbComment.Text,
                    ["inventory_number"] = tbInvNum.Text,
                    ["size_width"] = tbWidth.Text,
                    ["size_depth"] = tbDepth.Text,
                    ["size_height"] = tbHeight.Text,
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: t65" + ex.Message);
            }
        }
        private void tbName_TextChanged(object sender, EventArgs e)
        {

        }

        private void tbOrderNum_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void tbManufacturer_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void tbSeller_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void tbSN_TextChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void tbQty_TextChanged(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void tbUnits_TextChanged(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void tbPlace_TextChanged(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void tbComment_TextChanged(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void tbInvNum_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
