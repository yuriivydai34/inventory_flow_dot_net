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
    public partial class Outcome : Form
    {

        RestTableClient materialsApi;
        RestTableClient outcomeLogApi;
        string vManufacturer;
        string vSeller;
        string vCurrQuantity;
        string vSN;

        string vUnits;
        string vOrder_Number;
        string vComment;



        string mat_ID;
        public Outcome(string apiBaseUrl, string apiKey, int matID, string catName, string manufacturer, string seller, string sn, string currentQuantity, string units, string order_number, string projectStorage, string previous_storage, string comment)
        {
            InitializeComponent();
            materialsApi = new RestTableClient(apiBaseUrl, apiKey, "materials");
            outcomeLogApi = new RestTableClient(apiBaseUrl, apiKey, "outcome_log");
            button1.Enabled = false;
            mat_ID = Convert.ToString(matID);

            lblCatName.Text = catName;
            vManufacturer = manufacturer;
            vSeller = seller;
            vSN = sn;
            vCurrQuantity = currentQuantity;

            lblCurrentQuantity.Text = Convert.ToString(currentQuantity);
            vUnits = units;
            vOrder_Number = order_number;
            lblProjectStorage.Text = projectStorage;
            lblUnits.Text = units;
            lblUnits2.Text = units;
            vComment = comment;



        }

        private async void button1_Click(object sender, EventArgs e)
        {
            int quantity_old = Convert.ToInt32(lblCurrentQuantity.Text);
            int quantity_new = Convert.ToInt32(tbCurrentQuantity.Text);
            int quantity_to_update = (quantity_old - quantity_new);

            try
            {
                await materialsApi.UpdateAsync(Convert.ToInt32(mat_ID), new Dictionary<string, object>
                {
                    ["quantity"] = quantity_to_update
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ((Form1)Application.OpenForms["Form1"]).loadMainTable();

            //outcome log
            try
            {
                string dtstring = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                await outcomeLogApi.CreateAsync(new Dictionary<string, object>
                {
                    ["cat_name"] = lblCatName.Text,
                    ["manufacturer"] = vManufacturer,
                    ["seller"] = vSeller,
                    ["sn"] = vSN,
                    ["quantity"] = vCurrQuantity,
                    ["units"] = vUnits,
                    ["order_number"] = vOrder_Number,
                    ["project_storage"] = lblProjectStorage.Text,
                    ["comment"] = vComment,
                    ["datetime"] = dtstring,
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            Close();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void checkBox1_CheckStateChanged(object sender, EventArgs e)
        {
            if(checkBox1.Checked == true)
            {
                button1.Enabled = true;
            }
            else
            {
                button1.Enabled = false;
            }
        }

        private void Outcome_Load(object sender, EventArgs e)
        {

        }

        private void lblCatName_Click(object sender, EventArgs e)
        {

        }
    }
}
