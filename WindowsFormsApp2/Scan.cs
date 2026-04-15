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
    public partial class Scan : Form
    {
        string mid;
        string connectionstring;
        public Scan(string con)
        {
            connectionstring = con;
            InitializeComponent();
        }





        private void btn_income_Click(object sender, EventArgs e)
        {
            string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //MySqlCommand command = new MySqlCommand("UPDATE materials SET date_of_check = "+currentDateTime+" WHERE inventory_number = "+tbInventoryNumber.Text+";");
            //MySqlConnection cnnctn = new MySqlConnection(connectionstring);
            //cnnctn.Open();
            //cnnctn.EndExecuteNonQuery(command, cnnctn);
            //cnnctn.Close();





            // Replace with your actual MySQL connection string

            // SQL query to update a record
            string query = "UPDATE materials SET date_of_check = @datechecked WHERE inventory_number = @inventorynum";

            using (MySqlConnection cnnctn = new MySqlConnection(connectionstring))
            {
                try
                {
                    cnnctn.Open();

                    using (MySqlCommand command = new MySqlCommand(query, cnnctn))
                    {
                        // Use parameters to safely pass values
                        command.Parameters.AddWithValue("@datechecked", currentDateTime);
                        command.Parameters.AddWithValue("@inventorynum", tbInventoryNumber.Text);

                        // Execute the query
                        int rowsAffected = command.ExecuteNonQuery();


                        if(rowsAffected >0)
                        {
                            CloseAfterDelaySuccess();

                        }
                        else
                        {
                            CloseAfterDelayFail();

                        }

                        //MessageBox.Show(rowsAffected > 0 ? "Record updated successfully!" : "No matching record found.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
                finally
                {
                    cnnctn.Close();
                    ((Form1)Application.OpenForms["Form1"]).loadMainTable();

                }
            }



        }

        private async void CloseAfterDelaySuccess()
        {
            BackColor = Color.LightGreen;
            await Task.Delay(1000); // Wait for 1 second (1000 milliseconds)
            string query2 = "SELECT cat_name FROM materials WHERE inventory_number = @inventorynum"; // Adjust this query as needed
            using (MySqlConnection connection2 = new MySqlConnection(connectionstring))
            {
                try
                {
                    connection2.Open();
                    using (MySqlCommand command = new MySqlCommand(query2, connection2))
                    {
                        command.Parameters.AddWithValue("@someValue", mid);
                        command.Parameters.AddWithValue("@inventorynum", tbInventoryNumber.Text);

                        // Execute the query and retrieve the result
                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            MessageBox.Show(result.ToString(), "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("No Data found", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error 678: " + ex.Message);
                }
                finally
                {
                    connection2.Close();
                }
            }





            Close();
        }

        private async void CloseAfterDelayFail()
        {
            BackColor = Color.Red;
            await Task.Delay(3000); // Wait for 1 second (1000 milliseconds)

        }



        private void UpdateDateTime()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionstring))
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE your_table SET date_of_check = @currentDateTime WHERE id = @id";

                }
                catch (MySqlException ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
                finally
                {
                    conn.Close();
                }
            }
        }
    }
}
