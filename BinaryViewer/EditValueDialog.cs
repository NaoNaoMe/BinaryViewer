using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace BinaryViewer
{
    /// <summary>
    /// Dialog for editing a typed value (Int8–Double) at a specific address in the hex view.
    /// </summary>
    public class EditValueDialog : Form
    {
        // Source bytes read from ByteProvider (memory order, up to 8 bytes)
        private readonly byte[] _sourceBytes;
        private readonly long _address;

        private ComboBox _cmbType;
        private RadioButton _rbLittle;
        private RadioButton _rbBig;
        private TextBox _txtValue;
        private Label _lblRaw;
        private Button _btnOk;

        // True while programmatically updating controls, to avoid recursive refresh
        private bool _updating = false;

        /// <summary>Resulting bytes to write back (in memory order). Null if cancelled.</summary>
        public byte[] ResultBytes { get; private set; }

        public EditValueDialog(long address, byte[] sourceBytes, bool defaultLittleEndian)
        {
            _address = address;
            _sourceBytes = sourceBytes;
            BuildUI(defaultLittleEndian);
            RefreshFromBytes();
        }

        // ── UI Construction ───────────────────────────────────────────────────

        private void BuildUI(bool defaultLittleEndian)
        {
            Text = $"Edit Value at 0x{_address:X8}";
            ClientSize = new System.Drawing.Size(390, 200);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;

            const int lx = 12, cx = 110, rh = 30, sy = 14, cw = 255;

            // Type
            var lblType = new Label { Text = "Type:", Left = lx, Top = sy + 4, AutoSize = true };
            _cmbType = new ComboBox { Left = cx, Top = sy, Width = cw, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbType.Items.AddRange(new string[]
            {
                "Int8   (1 byte)",   "UInt8   (1 byte)",
                "Int16  (2 bytes)",  "UInt16  (2 bytes)",
                "Int32  (4 bytes)",  "UInt32  (4 bytes)",
                "Int64  (8 bytes)",  "UInt64  (8 bytes)",
                "Float  (4 bytes)",  "Double  (8 bytes)"
            });
            _cmbType.SelectedIndex = 4; // Int32 default
            _cmbType.SelectedIndexChanged += (s, e) => RefreshFromBytes();

            // Endian
            var lblEndian = new Label { Text = "Endian:", Left = lx, Top = sy + rh + 4, AutoSize = true };
            _rbLittle = new RadioButton { Text = "Little", Left = cx,      Top = sy + rh, Width = 70, Checked = defaultLittleEndian };
            _rbBig    = new RadioButton { Text = "Big",    Left = cx + 75, Top = sy + rh, Width = 60, Checked = !defaultLittleEndian };
            _rbLittle.CheckedChanged += (s, e) => { if (_rbLittle.Checked) RefreshFromBytes(); };
            _rbBig.CheckedChanged    += (s, e) => { if (_rbBig.Checked)    RefreshFromBytes(); };

            // Value
            var lblValue = new Label { Text = "Value:", Left = lx, Top = sy + rh * 2 + 4, AutoSize = true };
            _txtValue = new TextBox { Left = cx, Top = sy + rh * 2, Width = cw };
            _txtValue.TextChanged += (s, e) => RefreshRaw();

            // Raw (read-only display of bytes in memory order)
            var lblRawTitle = new Label { Text = "Raw:", Left = lx, Top = sy + rh * 3 + 6, AutoSize = true };
            _lblRaw = new Label
            {
                Left = cx, Top = sy + rh * 3 + 5, Width = cw, AutoSize = false,
                ForeColor = System.Drawing.SystemColors.GrayText
            };

            // Buttons
            _btnOk = new Button { Text = "OK", Left = 219, Top = sy + rh * 4 + 10, Width = 75 };
            var btnCancel = new Button
            {
                Text = "Cancel", DialogResult = DialogResult.Cancel,
                Left = 303, Top = sy + rh * 4 + 10, Width = 75
            };
            _btnOk.Click += BtnOk_Click;

            Controls.AddRange(new Control[]
            {
                lblType, _cmbType,
                lblEndian, _rbLittle, _rbBig,
                lblValue, _txtValue,
                lblRawTitle, _lblRaw,
                _btnOk, btnCancel
            });
            AcceptButton = _btnOk;
            CancelButton = btnCancel;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private int ByteCount()
        {
            switch (_cmbType.SelectedIndex)
            {
                case 0: case 1: return 1;
                case 2: case 3: return 2;
                case 4: case 5: case 8: return 4;
                case 6: case 7: case 9: return 8;
                default: return 1;
            }
        }

        /// <summary>
        /// Converts source bytes (memory order) to a buffer in the endian order
        /// that BitConverter expects for parsing.
        /// </summary>
        private byte[] ToParseBuffer(int size)
        {
            byte[] buf = new byte[size];
            Array.Copy(_sourceBytes, buf, size);
            // BitConverter is always little-endian on x86.
            // If user wants big-endian interpretation, reverse the bytes.
            if (size > 1 && _rbBig.Checked)
                Array.Reverse(buf);
            return buf;
        }

        // ── Refresh logic ─────────────────────────────────────────────────────

        /// <summary>Fills the Value TextBox from _sourceBytes according to the current type/endian.</summary>
        private void RefreshFromBytes()
        {
            if (_updating) return;
            _updating = true;
            try
            {
                int size = ByteCount();
                bool enough = _sourceBytes.Length >= size;
                _btnOk.Enabled = enough;

                if (!enough)
                {
                    _txtValue.Text = "(not enough bytes)";
                    _lblRaw.Text = string.Empty;
                    return;
                }

                byte[] buf = ToParseBuffer(size);

                string text;
                switch (_cmbType.SelectedIndex)
                {
                    case 0: text = ((sbyte)buf[0]).ToString(); break;
                    case 1: text = buf[0].ToString(); break;
                    case 2: text = BitConverter.ToInt16(buf, 0).ToString(); break;
                    case 3: text = BitConverter.ToUInt16(buf, 0).ToString(); break;
                    case 4: text = BitConverter.ToInt32(buf, 0).ToString(); break;
                    case 5: text = BitConverter.ToUInt32(buf, 0).ToString(); break;
                    case 6: text = BitConverter.ToInt64(buf, 0).ToString(); break;
                    case 7: text = BitConverter.ToUInt64(buf, 0).ToString(); break;
                    case 8: text = BitConverter.ToSingle(buf, 0).ToString("G9", CultureInfo.InvariantCulture); break;
                    case 9: text = BitConverter.ToDouble(buf, 0).ToString("G17", CultureInfo.InvariantCulture); break;
                    default: text = "0"; break;
                }

                _txtValue.Text = text;

                // Raw: always show memory-order bytes
                _lblRaw.ForeColor = System.Drawing.SystemColors.GrayText;
                _lblRaw.Text = string.Join(" ", _sourceBytes.Take(size).Select(b => $"0x{b:X2}"));
            }
            catch
            {
                _txtValue.Text = "0";
            }
            finally
            {
                _updating = false;
            }
        }

        /// <summary>Updates the Raw label from the current Value TextBox content.</summary>
        private void RefreshRaw()
        {
            if (_updating) return;

            byte[] bytes = ParseValueToBytes();
            if (bytes == null)
            {
                _lblRaw.Text = "(invalid)";
                _lblRaw.ForeColor = System.Drawing.Color.Red;
                _btnOk.Enabled = false;
            }
            else
            {
                _lblRaw.Text = string.Join(" ", bytes.Select(b => $"0x{b:X2}"));
                _lblRaw.ForeColor = System.Drawing.SystemColors.GrayText;
                _btnOk.Enabled = true;
            }
        }

        /// <summary>
        /// Parses the Value TextBox into bytes in memory order.
        /// Returns null on parse error.
        /// </summary>
        private byte[] ParseValueToBytes()
        {
            string text = _txtValue.Text.Trim();
            try
            {
                byte[] bytes;
                switch (_cmbType.SelectedIndex)
                {
                    case 0: bytes = new byte[] { (byte)sbyte.Parse(text) }; break;
                    case 1: bytes = new byte[] { byte.Parse(text) }; break;
                    case 2: bytes = BitConverter.GetBytes(short.Parse(text)); break;
                    case 3: bytes = BitConverter.GetBytes(ushort.Parse(text)); break;
                    case 4: bytes = BitConverter.GetBytes(int.Parse(text)); break;
                    case 5: bytes = BitConverter.GetBytes(uint.Parse(text)); break;
                    case 6: bytes = BitConverter.GetBytes(long.Parse(text)); break;
                    case 7: bytes = BitConverter.GetBytes(ulong.Parse(text)); break;
                    case 8: bytes = BitConverter.GetBytes(float.Parse(text, CultureInfo.InvariantCulture)); break;
                    case 9: bytes = BitConverter.GetBytes(double.Parse(text, CultureInfo.InvariantCulture)); break;
                    default: return null;
                }

                // BitConverter.GetBytes returns little-endian (x86).
                // If user wants big-endian, reverse to get memory order.
                if (bytes.Length > 1 && _rbBig.Checked)
                    Array.Reverse(bytes);

                return bytes;
            }
            catch { return null; }
        }

        // ── OK button ─────────────────────────────────────────────────────────

        private void BtnOk_Click(object sender, EventArgs e)
        {
            ResultBytes = ParseValueToBytes();
            if (ResultBytes == null)
            {
                MessageBox.Show("Could not parse the entered value.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None; // keep dialog open
                return;
            }
            DialogResult = DialogResult.OK;
        }
    }
}
