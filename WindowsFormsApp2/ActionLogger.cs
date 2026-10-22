using System;
using MySql.Data.MySqlClient;

namespace InventoryFlow
{
    static class ActionLogger
    {
        public static void Log(
            string connectionString,
            int userId,
            string actionType,
            string catName,
            string manufacturer,
            string seller,
            string sn,
            object quantity,
            string units,
            string orderNumber,
            string projectStorage,
            string previousStorage,
            string comment)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string cmdText = @"INSERT INTO income_log
                    (user_id, action_type, cat_name, manufacturer, seller, sn, quantity, units, order_number,
                     project_storage, previous_storage, comment, datetime)
                    VALUES
                    (@user_id, @action_type, @cat_name, @manufacturer, @seller, @sn, @quantity, @units, @order_number,
                     @project_storage, @previous_storage, @comment, @datetime)";

                    using (MySqlCommand cmd = new MySqlCommand(cmdText, connection))
                    {
                        cmd.Parameters.AddWithValue("@user_id", userId);
                        cmd.Parameters.AddWithValue("@action_type", actionType);
                        cmd.Parameters.AddWithValue("@cat_name", (object)catName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@manufacturer", (object)manufacturer ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@seller", (object)seller ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@sn", (object)sn ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@quantity", quantity ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@units", (object)units ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@order_number", (object)orderNumber ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@project_storage", (object)projectStorage ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@previous_storage", (object)previousStorage ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@comment", (object)comment ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@datetime", DateTime.Now);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // Логування дій не повинно блокувати основну операцію
            }
        }
    }
}
