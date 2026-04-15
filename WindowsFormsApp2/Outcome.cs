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
    public partial class Outcome : Form
    {

        string connectionString;
        string vManufacturer;
        string vSeller;
        string vCurrQuantity;
        string vSN;

        string vUnits;
        string vOrder_Number;
        string vComment;



        string mat_ID;
        public Outcome(string connString, int matID, string catName, string manufacturer, string seller, string sn, string currentQuantity, string units, string order_number, string projectStorage, string previous_storage, string comment)
        {
            InitializeComponent();
            connectionString = connString;
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
        
        private void button1_Click(object sender, EventArgs e)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            connection.Open();
            int quantity_old = Convert.ToInt32(lblCurrentQuantity.Text);
            int quantity_new = Convert.ToInt32(tbCurrentQuantity.Text);
            int quantity_to_update = (quantity_old - quantity_new);
            string sqlQuery = "UPDATE materials SET quantity = " + quantity_to_update +" WHERE id = "+ mat_ID + ";";
            MySqlCommand cmd = new MySqlCommand(sqlQuery, connection);
            int rowsAffected = cmd.ExecuteNonQuery();
            connection.Close();

            //MessageBox.Show(Convert.ToString(quantity_to_update) +"-" + mat_ID, "Відвантажено", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            
            ((Form1)Application.OpenForms["Form1"]).loadMainTable();

            //insert into outcome log



            //get all data first
            try
            {
                MySqlConnection connection1 = new MySqlConnection(connectionString);
                connection1.Open();


            }
            catch (Exception ex)
            {
                MessageBox.Show(Convert.ToString(ex), "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }


            //outcome log query
            try
            {
                MySqlConnection connection1 = new MySqlConnection(connectionString);
                connection1.Open();
                //need to be fixed
                string dtstring = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string cmdText = string.Format(@"INSERT INTO outcome_log (cat_name, manufacturer, seller, sn, quantity, units, order_number, project_storage, comment, datetime) VALUES ('{0}','{1}','{2}','{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}')", lblCatName.Text, vManufacturer, vSeller, vSN, vCurrQuantity, vUnits, vOrder_Number, lblProjectStorage.Text, vComment, dtstring);
                new MySqlCommand(cmdText, connection1).ExecuteNonQuery();
                connection1.Close();

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
