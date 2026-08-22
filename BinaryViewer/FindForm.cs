using System;
using System.Globalization;
using System.Windows.Forms;

namespace BinaryViewer
{

    public partial class FindForm : Form
    {
        public event EventHandler<FindEventArgs> FindNext;

        public FindForm()
        {
            InitializeComponent();
        }

        private void FindForm_Load(object sender, EventArgs e)
        {
            // Populate the data type ComboBox
            cmbDataType.Items.AddRange(new string[]
            {
                "Int8", "UInt8",
                "Int16", "UInt16",
                "Int32", "UInt32",
                "Int64", "UInt64",
                "Float", "Double"
            });
            cmbDataType.SelectedIndex = 0;
        }

        private void btnFindNext_Click(object sender, EventArgs e)
        {
            string selectedType = cmbDataType.SelectedItem.ToString();
            string valueText = txtValue.Text;

            if (string.IsNullOrWhiteSpace(valueText))
            {
                MessageBox.Show("Please enter a value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Convert the input value string and data type into a byte array
                byte[] searchBytes = ConvertValueToBytes(selectedType, valueText);
                if (searchBytes == null) return; // Error message already shown in helper

                // Adjust for endianness if necessary
                if (searchBytes.Length > 1)
                {
                    bool wantLittleEndian = rbLittleEndian.Checked;
                    if (BitConverter.IsLittleEndian != wantLittleEndian)
                    {
                        Array.Reverse(searchBytes);
                    }
                }

                // Raise the event to notify MainForm
                FindNext?.Invoke(this, new FindEventArgs(searchBytes));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not convert value: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private byte[] ConvertValueToBytes(string type, string value)
        {
            // Determine if input is hex or decimal
            NumberStyles style = NumberStyles.Integer;
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring(2);
                style = NumberStyles.HexNumber;
            }

            switch (type)
            {
                case "Int8": return new byte[] { (byte)sbyte.Parse(value, style) };
                case "UInt8": return new byte[] { byte.Parse(value, style) };
                case "Int16": return BitConverter.GetBytes(short.Parse(value, style));
                case "UInt16": return BitConverter.GetBytes(ushort.Parse(value, style));
                case "Int32": return BitConverter.GetBytes(int.Parse(value, style));
                case "UInt32": return BitConverter.GetBytes(uint.Parse(value, style));
                case "Int64": return BitConverter.GetBytes(long.Parse(value, style));
                case "UInt64": return BitConverter.GetBytes(ulong.Parse(value, style, CultureInfo.InvariantCulture)); // Hex ulong needs InvariantCulture
                case "Float": return BitConverter.GetBytes(float.Parse(value));
                case "Double": return BitConverter.GetBytes(double.Parse(value));
                default:
                    throw new ArgumentException("Unsupported data type.");
            }
        }

        private void cmbDataType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Enable endianness option only for multi-byte types
            string selectedType = cmbDataType.SelectedItem.ToString();
            bool isMultiByte = !(selectedType == "Int8" || selectedType == "UInt8");
            endianGroupBox.Enabled = isMultiByte;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }

    public class FindEventArgs : EventArgs
    {
        public byte[] SearchBytes { get; }
        public FindEventArgs(byte[] searchBytes)
        {
            this.SearchBytes = searchBytes;
        }
    }
}