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
using System.IO;


namespace InventoryFlow
{
    public partial class Edit_Position : Form
    {
        string mid;
        string connectionString = "";
        public Edit_Position(int matID, string getconnectionString)
        {
            InitializeComponent();
            connectionString = getconnectionString;


            mid = Convert.ToString(matID);
            loadCatName();
            loadOrderNum();
            loadMAnufacturer();
            loadSeller();
            loadSN();
            loadQuantity();
            loadUnits();
            loadStorage();
            loadComment();
            loadInvNum();
            loadWidth();
            loadDepth();
            loadHeight();
            //MessageBox.Show(getconnectionString, "Message", MessageBoxButtons.OK);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            setValues();
            ((Form1)Application.OpenForms["Form1"]).loadMainTable();
            Close();


        }

        private void loadWidth()
        {
            string query = "SELECT size_width FROM materials WHERE id = @someValue";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@someValue", mid);

                        object result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            tbWidth.Text = result.ToString();
                        }
                        else
                        {
                            tbWidth.Text = "";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loadWidth: " + ex.Message);
                }
            }
        }

        private void loadDepth()
        {
            string query = "SELECT size_depth FROM materials WHERE id = @someValue";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@someValue", mid);

                        object result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            tbDepth.Text = result.ToString();
                        }
                        else
                        {
                            tbDepth.Text = "";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loadDepth: " + ex.Message);
                }
            }
        }

        private void loadHeight()
        {
            string query = "SELECT size_height FROM materials WHERE id = @someValue";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@someValue", mid);

                        object result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            tbHeight.Text = result.ToString();
                        }
                        else
                        {
                            tbHeight.Text = "";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loadHeight: " + ex.Message);
                }
            }
        }

        private void loadCatName() 
        {

            //else
            //{
            //}
            //string connectionString = @"Server = localhost; Database = elitbudgrup_inv; Uid = root; Pwd = passwd; Charset = utf8";
            string query = "SELECT cat_name FROM materials WHERE id = @someValue"; // Adjust this query as needed
            using (MySqlConnection connection = new MySqlConnection(@connectionString))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@someValue", mid);

                        // Execute the query and retrieve the result
                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            tbName.Text = result.ToString();
                        }
                        else
                        {
                            tbName.Text = "No data found";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error 678: " + ex.Message);
                }
            }
        }
        private void loadOrderNum()
        {
            //string connectionString = @"Server = localhost; Database = elitbudgrup_inv; Uid = root; Pwd = passwd; Charset = utf8";
            string query = "SELECT order_number FROM materials WHERE id = @someValue"; 

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@someValue", mid);

                        // Execute the query and retrieve the result
                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            tbOrderNum.Text = result.ToString();
                        }
                        else
                        {
                            tbOrderNum.Text = "No data found";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: 2342" + ex.Message);
                }
            }
        }
        private void loadMAnufacturer()
        {
            //string connectionString = @"Server = localhost; Database = elitbudgrup_inv; Uid = root; Pwd = passwd; Charset = utf8";
            string query = "SELECT manufacturer FROM materials WHERE id = @someValue";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@someValue", mid);

                        // Execute the query and retrieve the result
                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            tbManufacturer.Text = result.ToString();
                        }
                        else
                        {
                            tbManufacturer.Text = "No data found";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: 345" + ex.Message);
                }
            }
        }
        private void loadSeller()
        {
            //string connectionString = @"Server = localhost; Database = elitbudgrup_inv; Uid = root; Pwd = passwd; Charset = utf8";
            string query = "SELECT seller FROM materials WHERE id = @someValue";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@someValue", mid);

                        // Execute the query and retrieve the result
                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            tbSeller.Text = result.ToString();
                        }
                        else
                        {
                            tbSeller.Text = "No data found";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: 134" + ex.Message);
                }
            }
        }
        private void loadSN()
        {
            //string connectionString = @"Server = localhost; Database = elitbudgrup_inv; Uid = root; Pwd = passwd; Charset = utf8";
            string query = "SELECT sn FROM materials WHERE id = @someValue";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@someValue", mid);

                        // Execute the query and retrieve the result
                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            tbSN.Text = result.ToString();
                        }
                        else
                        {
                            tbSN.Text = "No data found";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: 876" + ex.Message);
                }
            }
        }
        private void loadQuantity()
        {
            //string connectionString = @"Server = localhost; Database = elitbudgrup_inv; Uid = root; Pwd = passwd; Charset = utf8";
            string query = "SELECT quantity FROM materials WHERE id = @someValue";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@someValue", mid);

                        // Execute the query and retrieve the result
                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            tbQty.Text = result.ToString();
                        }
                        else

                        {
                            tbQty.Text = "No data found";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: 4532" + ex.Message);
                }
            }
        }
        private void loadUnits()
        {
            //string connectionString = @"Server = localhost; Database = elitbudgrup_inv; Uid = root; Pwd = passwd; Charset = utf8";
            string query = "SELECT units FROM materials WHERE id = @someValue";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@someValue", mid);

                        // Execute the query and retrieve the result
                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            tbUnits.Text = result.ToString();
                        }
                        else
                        {
                            tbUnits.Text = "No data found";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: 432" + ex.Message);
                }
            }
        }
        private void loadStorage()
        {
            //string connectionString = @"Server = localhost; Database = elitbudgrup_inv; Uid = root; Pwd = passwd; Charset = utf8";
            string query = "SELECT project_storage FROM materials WHERE id = @someValue";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@someValue", mid);

                        // Execute the query and retrieve the result
                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            tbPlace.Text = result.ToString();
                        }
                        else
                        {
                            tbPlace.Text = "No data found";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: 62" + ex.Message);
                }
            }
        }
        private void loadComment()
        {
            //string connectionString = @"Server = localhost; Database = elitbudgrup_inv; Uid = root; Pwd = passwd; Charset = utf8";
            string query = "SELECT comment FROM materials WHERE id = @someValue";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@someValue", mid);

                        // Execute the query and retrieve the result
                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            tbComment.Text = result.ToString();
                        }
                        else
                        {
                            tbComment.Text = "No data found";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: 213" + ex.Message);
                }
            }

        }
        private void loadInvNum()
        {
            //string connectionString = @"Server = localhost; Database = elitbudgrup_inv; Uid = root; Pwd = passwd; Charset = utf8";
            string query = "SELECT inventory_number FROM materials WHERE id = @someValue";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@someValue", mid);

                        // Execute the query and retrieve the result
                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            tbInvNum.Text = result.ToString();
                        }
                        else
                        {
                            tbInvNum.Text = "No data found";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: 75" + ex.Message);
                }
            }

        }


        private void setValues()
        {
            string query = @"UPDATE materials SET cat_name = @value1, 
manufacturer = @value2, 
seller = @value3, 
sn = @value4,
quantity = @value5, 
units = @value6, 
order_number = @value7, 
project_storage = @value8, 
comment = @value9, 
inventory_number = @value10,
size_width = @value11,
size_depth = @value12,
size_height = @value13
WHERE id = @id";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@value1", tbName.Text);
                        command.Parameters.AddWithValue("@value2", tbManufacturer.Text);
                        command.Parameters.AddWithValue("@value3", tbSeller.Text);
                        command.Parameters.AddWithValue("@value4", tbSN.Text);
                        command.Parameters.AddWithValue("@value5", tbQty.Text);
                        command.Parameters.AddWithValue("@value6", tbUnits.Text);
                        command.Parameters.AddWithValue("@value7", tbOrderNum.Text);
                        command.Parameters.AddWithValue("@value8", tbPlace.Text);
                        command.Parameters.AddWithValue("@value9", tbComment.Text);
                        command.Parameters.AddWithValue("@value10", tbInvNum.Text);
                        command.Parameters.AddWithValue("@value11", tbWidth.Text);
                        command.Parameters.AddWithValue("@value12", tbDepth.Text);
                        command.Parameters.AddWithValue("@value13", tbHeight.Text);
                        command.Parameters.AddWithValue("@id", mid);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            //MessageBox.Show("Data updated successfully!");
                        }
                        else
                        {
                            MessageBox.Show("Error 530: Update failed or ID not found.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: t65" + ex.Message);
                }
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

        private void Edit_Position_Load(object sender, EventArgs e)
        {

        }
    }
}
