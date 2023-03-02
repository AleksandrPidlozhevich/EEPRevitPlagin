namespace EEPRevitPlagin.EEPRPCommandModules.FontChanging
{
    partial class FontChangingForm
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
            this.fontList = new System.Windows.Forms.ComboBox();
            this.doButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // fontList
            // 
            this.fontList.FormattingEnabled = true;
            this.fontList.Location = new System.Drawing.Point(12, 12);
            this.fontList.Name = "fontList";
            this.fontList.Size = new System.Drawing.Size(150, 21);
            this.fontList.TabIndex = 0;
            this.fontList.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // doButton
            // 
            this.doButton.Location = new System.Drawing.Point(179, 12);
            this.doButton.Name = "doButton";
            this.doButton.Size = new System.Drawing.Size(75, 21);
            this.doButton.TabIndex = 1;
            this.doButton.Text = "Применить";
            this.doButton.UseVisualStyleBackColor = true;
            this.doButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // FontChangingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 61);
            this.Controls.Add(this.doButton);
            this.Controls.Add(this.fontList);
            this.Name = "FontChangingForm";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox fontList;
        private System.Windows.Forms.Button doButton;
    }
}