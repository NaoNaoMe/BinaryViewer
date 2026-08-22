using System;
using System.Windows.Forms;
using FirmwareImageFormat;

namespace BinaryViewer
{
    public class NewFileOptions
    {
        public FormatType Format { get; set; } = FormatType.Binary;
        public long StartAddress { get; set; } = 0;
        public int Size { get; set; } = 256;
        public byte FillValue { get; set; } = 0xFF;
        public string FileName { get; set; } = string.Empty;
    }

    public static class NewFileDialog
    {
        public static DialogResult Show(ref NewFileOptions options)
        {
            using (Form form = new Form())
            {
                form.Text = "New File";
                form.ClientSize = new System.Drawing.Size(370, 230);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterParent;
                form.MinimizeBox = false;
                form.MaximizeBox = false;

                const int labelX = 12, controlX = 130, rowH = 32, startY = 14, controlW = 210;

                // --- Format ---
                var lblFormat = new Label { Text = "Format:", Left = labelX, Top = startY + 4, AutoSize = true };
                var cmbFormat = new ComboBox
                {
                    Left = controlX, Top = startY, Width = controlW,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                cmbFormat.Items.AddRange(new string[]
                {
                    "Binary",
                    "Intel HEX  16-bit (Linear)",
                    "Intel HEX  32-bit (Extended Linear)",
                    "Motorola S-Record  S1 (16-bit)",
                    "Motorola S-Record  S3 (32-bit)"
                });
                cmbFormat.SelectedIndex = 0;

                // --- Start Address ---
                var lblAddr = new Label { Text = "Start Address:", Left = labelX, Top = startY + rowH + 4, AutoSize = true };
                var txtAddr = new TextBox { Left = controlX, Top = startY + rowH, Width = controlW, Text = "0x00000000" };

                // --- Size ---
                var lblSize = new Label { Text = "Size (bytes):", Left = labelX, Top = startY + rowH * 2 + 4, AutoSize = true };
                var txtSize = new TextBox { Left = controlX, Top = startY + rowH * 2, Width = controlW, Text = "256" };

                // --- Fill Value ---
                var lblFill = new Label { Text = "Fill Value:", Left = labelX, Top = startY + rowH * 3 + 4, AutoSize = true };
                var cmbFill = new ComboBox
                {
                    Left = controlX, Top = startY + rowH * 3, Width = controlW,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                cmbFill.Items.AddRange(new string[] { "0xFF", "0x00" });
                cmbFill.SelectedIndex = 0;

                // --- File Name (optional, for window title only) ---
                var lblFile = new Label { Text = "File Name:", Left = labelX, Top = startY + rowH * 4 + 4, AutoSize = true };
                var txtFile = new TextBox { Left = controlX, Top = startY + rowH * 4, Width = controlW, Text = string.Empty };

                // --- Buttons ---
                var btnOk = new Button
                {
                    Text = "OK", DialogResult = DialogResult.OK,
                    Left = 199, Top = startY + rowH * 5 + 10, Width = 75
                };
                var btnCancel = new Button
                {
                    Text = "Cancel", DialogResult = DialogResult.Cancel,
                    Left = 283, Top = startY + rowH * 5 + 10, Width = 75
                };

                form.Controls.AddRange(new Control[]
                {
                    lblFormat, cmbFormat,
                    lblAddr, txtAddr,
                    lblSize, txtSize,
                    lblFill, cmbFill,
                    lblFile, txtFile,
                    btnOk, btnCancel
                });
                form.AcceptButton = btnOk;
                form.CancelButton = btnCancel;

                if (form.ShowDialog() != DialogResult.OK)
                    return DialogResult.Cancel;

                // --- Parse Format ---
                switch (cmbFormat.SelectedIndex)
                {
                    case 0: options.Format = FormatType.Binary; break;
                    case 1: options.Format = FormatType.IntelHexLinear; break;
                    case 2: options.Format = FormatType.IntelHexExLinear; break;
                    case 3: options.Format = FormatType.S1Record; break;
                    case 4: options.Format = FormatType.S3Record; break;
                    default: options.Format = FormatType.Binary; break;
                }

                // --- Parse Start Address ---
                try
                {
                    string addrText = txtAddr.Text.Replace("0x", "").Replace("0X", "").Trim();
                    options.StartAddress = Convert.ToInt64(addrText, 16);
                }
                catch
                {
                    MessageBox.Show("Invalid address format. Please enter a hex value (e.g. 0x00000000).",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return DialogResult.None;
                }

                // --- Parse Size ---
                try
                {
                    options.Size = int.Parse(txtSize.Text.Trim());
                    if (options.Size <= 0)
                        throw new ArgumentException("Size must be a positive value.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Invalid size: {ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return DialogResult.None;
                }

                // --- Fill Value ---
                options.FillValue = cmbFill.SelectedIndex == 0 ? (byte)0xFF : (byte)0x00;

                // --- 16-bit address range validation ---
                bool is16bit = options.Format == FormatType.IntelHexLinear ||
                               options.Format == FormatType.S1Record;
                if (is16bit)
                {
                    long lastAddress = options.StartAddress + options.Size - 1;
                    if (options.StartAddress > 0xFFFF || lastAddress > 0xFFFF)
                    {
                        MessageBox.Show(
                            $"For 16-bit formats, the address range must be within 0x0000-0xFFFF.\n" +
                            $"Current range: 0x{options.StartAddress:X4}-0x{lastAddress:X4}",
                            "Address Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return DialogResult.None;
                    }
                }

                // --- File Name (optional, used for window title only) ---
                options.FileName = txtFile.Text.Trim();

                return DialogResult.OK;
            }
        }
    }
}
