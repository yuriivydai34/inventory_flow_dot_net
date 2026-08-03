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
    public partial class Scan : Form
    {
        RestTableClient materialsApi;
        public Scan(string apiBaseUrl, string apiKey)
        {
            materialsApi = new RestTableClient(apiBaseUrl, apiKey, "materials");
            InitializeComponent();
        }

        private async void btn_income_Click(object sender, EventArgs e)
        {
            string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            try
            {
                var materials = await materialsApi.ListAsync();
                var match = materials.FirstOrDefault(m =>
                    string.Equals(m.GetString("inventory_number"), tbInventoryNumber.Text, StringComparison.OrdinalIgnoreCase));

                if (match != null)
                {
                    int id = Convert.ToInt32(match.GetString("id"));
                    await materialsApi.UpdateAsync(id, new Dictionary<string, object> { ["date_of_check"] = currentDateTime });
                    await CloseAfterDelaySuccess(match.GetString("cat_name"));
                }
                else
                {
                    await CloseAfterDelayFail();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            finally
            {
                ((Form1)Application.OpenForms["Form1"]).loadMainTable();
            }
        }

        private async Task CloseAfterDelaySuccess(string catName)
        {
            BackColor = Color.LightGreen;
            await Task.Delay(1000); // Wait for 1 second (1000 milliseconds)

            MessageBox.Show(!string.IsNullOrEmpty(catName) ? catName : "No Data found", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

            Close();
        }

        private async Task CloseAfterDelayFail()
        {
            BackColor = Color.Red;
            await Task.Delay(3000); // Wait for 1 second (1000 milliseconds)
        }
    }
}
