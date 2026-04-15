namespace InventoryFlow
{
    partial class Scan
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btn_income = new System.Windows.Forms.Button();
            this.tbInventoryNumber = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btn_income
            // 
            this.btn_income.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_income.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_income.Location = new System.Drawing.Point(12, 49);
            this.btn_income.Name = "btn_income";
            this.btn_income.Size = new System.Drawing.Size(118, 31);
            this.btn_income.TabIndex = 2;
            this.btn_income.Text = "Додати";
            this.btn_income.UseVisualStyleBackColor = true;
            this.btn_income.Click += new System.EventHandler(this.btn_income_Click);
            // 
            // tbInventoryNumber
            // 
            this.tbInventoryNumber.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbInventoryNumber.Location = new System.Drawing.Point(12, 12);
            this.tbInventoryNumber.Name = "tbInventoryNumber";
            this.tbInventoryNumber.Size = new System.Drawing.Size(440, 31);
            this.tbInventoryNumber.TabIndex = 35;
            // 
            // Scan
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(464, 95);
            this.Controls.Add(this.tbInventoryNumber);
            this.Controls.Add(this.btn_income);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Scan";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Scan";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_income;
        private System.Windows.Forms.TextBox tbInventoryNumber;
    }
}