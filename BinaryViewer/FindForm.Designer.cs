namespace BinaryViewer
{
    partial class FindForm
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnFindNext = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbDataType = new System.Windows.Forms.ComboBox();
            this.txtValue = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.endianGroupBox = new System.Windows.Forms.GroupBox();
            this.rbBigEndian = new System.Windows.Forms.RadioButton();
            this.rbLittleEndian = new System.Windows.Forms.RadioButton();
            this.endianGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(297, 137);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(87, 27);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnFindNext
            // 
            this.btnFindNext.Location = new System.Drawing.Point(204, 137);
            this.btnFindNext.Name = "btnFindNext";
            this.btnFindNext.Size = new System.Drawing.Size(87, 27);
            this.btnFindNext.TabIndex = 3;
            this.btnFindNext.Text = "Find Next";
            this.btnFindNext.UseVisualStyleBackColor = true;
            this.btnFindNext.Click += new System.EventHandler(this.btnFindNext_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Data Type:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "Value:";
            // 
            // cmbDataType
            // 
            this.cmbDataType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDataType.FormattingEnabled = true;
            this.cmbDataType.Location = new System.Drawing.Point(83, 20);
            this.cmbDataType.Name = "cmbDataType";
            this.cmbDataType.Size = new System.Drawing.Size(139, 23);
            this.cmbDataType.TabIndex = 0;
            this.cmbDataType.SelectedIndexChanged += new System.EventHandler(this.cmbDataType_SelectedIndexChanged);
            // 
            // txtValue
            // 
            this.txtValue.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.txtValue.Location = new System.Drawing.Point(83, 61);
            this.txtValue.Name = "txtValue";
            this.txtValue.Size = new System.Drawing.Size(301, 23);
            this.txtValue.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label3.Location = new System.Drawing.Point(80, 87);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(201, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Prefix with \'0x\' for hexadecimal input.";
            // 
            // endianGroupBox
            // 
            this.endianGroupBox.Controls.Add(this.rbBigEndian);
            this.endianGroupBox.Controls.Add(this.rbLittleEndian);
            this.endianGroupBox.Enabled = false;
            this.endianGroupBox.Location = new System.Drawing.Point(240, 9);
            this.endianGroupBox.Name = "endianGroupBox";
            this.endianGroupBox.Size = new System.Drawing.Size(144, 46);
            this.endianGroupBox.TabIndex = 2;
            this.endianGroupBox.TabStop = false;
            this.endianGroupBox.Text = "Endianness";
            // 
            // rbBigEndian
            // 
            this.rbBigEndian.AutoSize = true;
            this.rbBigEndian.Location = new System.Drawing.Point(87, 20);
            this.rbBigEndian.Name = "rbBigEndian";
            this.rbBigEndian.Size = new System.Drawing.Size(43, 19);
            this.rbBigEndian.TabIndex = 1;
            this.rbBigEndian.Text = "Big";
            this.rbBigEndian.UseVisualStyleBackColor = true;
            // 
            // rbLittleEndian
            // 
            this.rbLittleEndian.AutoSize = true;
            this.rbLittleEndian.Checked = true;
            this.rbLittleEndian.Location = new System.Drawing.Point(15, 20);
            this.rbLittleEndian.Name = "rbLittleEndian";
            this.rbLittleEndian.Size = new System.Drawing.Size(52, 19);
            this.rbLittleEndian.TabIndex = 0;
            this.rbLittleEndian.TabStop = true;
            this.rbLittleEndian.Text = "Little";
            this.rbLittleEndian.UseVisualStyleBackColor = true;
            // 
            // FindForm
            // 
            this.AcceptButton = this.btnFindNext;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(396, 176);
            this.Controls.Add(this.endianGroupBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtValue);
            this.Controls.Add(this.cmbDataType);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnFindNext);
            this.Controls.Add(this.btnCancel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FindForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Find by Type";
            this.Load += new System.EventHandler(this.FindForm_Load);
            this.endianGroupBox.ResumeLayout(false);
            this.endianGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnFindNext;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbDataType;
        private System.Windows.Forms.TextBox txtValue;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox endianGroupBox;
        private System.Windows.Forms.RadioButton rbBigEndian;
        private System.Windows.Forms.RadioButton rbLittleEndian;
    }
}