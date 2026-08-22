using BinaryViewer.Controls;
using FirmwareImageFormat; // This should be your custom namespace
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace BinaryViewer
{
    public partial class MainForm : Form
    {
        public class Components
        {
            public FirmwareImage Image { set; get; }
            public string[] RawText { set; get; }
            public string Path { set; get; }

            public Components()
            {
                Image = new FirmwareImage();
                RawText = new string[0];
                Path = string.Empty;
            }
        }

        private string AssemblyVersion
        {
            get
            {
                string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

                var splited = version.Split('.');
                if (splited.Count() == 4)
                    version = splited[0] + "." + splited[1] + "." + splited[2];

                return version;
            }
        }

        private string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        private string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        private const string RepositoryUrl = "https://github.com/NaoNaoMe/BinaryViewer";

        private Components myComponents;
        private readonly string _windowTitle;
        private readonly string _initialFilePath;
        private bool _isModified = false;

        private FindForm _findFormInstance = null;
        private byte[] _lastSearchBytes = null;
        private int _currentAlignPadding = 0; // Number of padding bytes prepended for 16-byte alignment
        private int _previousPageIndex = -1;
        private long _contextMenuByteIndex = 0; // Byte index captured when context menu opens

        private InMemoryByteBuffer _currentBuffer; // The current HexBox provider, tracked for save-time snapshotting

        // pasteBehaviorLabel's two font variants: struck through once Free Editing lifts the
        // "Paste overwrites in place" behavior the label describes. Cached rather than built
        // fresh on every AttachByteProvider() call.
        private System.Drawing.Font _pasteBehaviorNormalFont;
        private System.Drawing.Font _pasteBehaviorStrikeoutFont;

        // Once set, Insert/Delete/Cut/Paste stay unlocked for the rest of this file's session
        // (see EnableFreeEditingButton_Click). Reset only where the user explicitly (re)opens
        // a file — not on the internal reload SaveFile() does after writing to disk.
        private bool _freeEditingUnlocked = false;

        private static System.Drawing.Icon LoadEmbeddedIcon()
        {
            using var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("BinaryViewer.Resources.AppIcon.ico");
            return new System.Drawing.Icon(stream);
        }

        public MainForm(string initialFilePath = null)
        {
            InitializeComponent();
            _initialFilePath = initialFilePath;
            _windowTitle = $"{AssemblyProduct} {AssemblyVersion}"; ;
            this.Text = _windowTitle;
            this.Load += MainForm_Load;

            _pasteBehaviorNormalFont = pasteBehaviorLabel.Font;
            _pasteBehaviorStrikeoutFont = new System.Drawing.Font(_pasteBehaviorNormalFont, System.Drawing.FontStyle.Strikeout);

            // --- Menu Event Handlers ---
            // File
            this.newToolStripMenuItem.Click += (s, e) => NewFileWithDialog();
            this.openToolStripMenuItem.Click += (s, e) => OpenFileWithDialog();
            this.saveToolStripMenuItem.Click += (s, e) => SaveFile(false);
            this.saveAsToolStripMenuItem.Click += (s, e) => SaveFile(true);
            this.exitToolStripMenuItem.Click += (s, e) => this.Close();

            // Edit — all items are built here; the Designer only defines editToolStripMenuItem itself.
            BuildEditMenu();

            // Help — all items are built here; the Designer only defines helpToolStripMenuItem itself.
            BuildHelpMenu();

            // --- Control Event Handlers ---
            this.PageAddressComboBox.SelectedIndexChanged += PageAddressComboBox_SelectedIndexChanged;
            this.bigEndianCheckBox.CheckedChanged += (s, e) => UpdateDataInspector();
            this.readOnlyCheckBox.CheckedChanged += (s, e) => mainHexBox.ReadOnly = readOnlyCheckBox.Checked;
            this.enableFreeEditingButton.Click += EnableFreeEditingButton_Click;
            this.mainHexBox.AllowDrop = true;
            this.mainHexBox.DragDrop += MainHexBox_DragDrop;
            this.mainHexBox.DragEnter += MainHexBox_DragEnter;
            this.mainHexBox.SelectionStartChanged += MainHexBox_SelectionChanged;
            this.mainHexBox.SelectionLengthChanged += MainHexBox_SelectionChanged;
            this.FormClosing += MainForm_FormClosing;


            // HexBox context menu
            InitializeHexBoxContextMenu();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_initialFilePath) && System.IO.File.Exists(_initialFilePath))
            {
                _freeEditingUnlocked = false;
                LoadFile(_initialFilePath);
            }
            else
            {
                NewFile();
            }
        }

        #region File Operations

        private void NewFile()
        {
            if (CheckUnsavedChanges())
            {
                return;
            }
            myComponents = new Components();
            myComponents.Path = null;

            var emptyBytes = new List<byte>();
            var buffer = new InMemoryByteBuffer(emptyBytes);
            AttachByteProvider(buffer);
            mainHexBox.LineInfoOffset = 0;
            mainHexBox.LeadingPaddingByteCount = 0;

            PageAddressComboBox.Items.Clear();
            FormatTextBox.Text = "N/A";
            PageSizeTextBox.Text = "0";

            SetModified(false);
            UpdateStatus("Empty", 0, 0);
        }

        private void NewFileWithDialog()
        {
            if (CheckUnsavedChanges())
                return;

            var options = new NewFileOptions();
            if (NewFileDialog.Show(ref options) != DialogResult.OK)
                return;

            myComponents = new Components();
            myComponents.Image.Format = options.Format;
            _freeEditingUnlocked = false;

            // myComponents.Path is intentionally left null so that Ctrl+S triggers Save As.
            // The file name is used for the window title only.
            var displayName = string.IsNullOrEmpty(options.FileName) ? "Untitled" : options.FileName;
            this.Text = $"{_windowTitle} - {displayName}";

            var section = new Section { StartAddress = options.StartAddress };
            section.Bytes.AddRange(System.Linq.Enumerable.Repeat(options.FillValue, options.Size));
            myComponents.Image.Sections.Add(section);

            _previousPageIndex = -1;
            mainHexBox.LineInfoOffset = 0;
            mainHexBox.LeadingPaddingByteCount = 0;

            SetModified(false);
            UpdateFileInfoUI();
        }

        private void OpenFileWithDialog()
        {
            if (CheckUnsavedChanges())
            {
                return;
            }

            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Supported files (*.hex, *.mot, *.srec, *.s19, *.bin)|*.hex;*.mot;*.srec;*.s19;*.bin|All files (*.*)|*.*",
                Title = "Open File"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _freeEditingUnlocked = false;
                LoadFile(ofd.FileName);
            }
        }

        private void LoadFile(string path)
        {
            myComponents = new Components();

            try
            {
                // Try parsing as text-based formats (Intel HEX, Motorola S-Record)
                string wholeText = System.IO.File.ReadAllText(path, Encoding.ASCII);
                var rawText = wholeText.Replace("\r\n", "\n").Split('\n');
                if (string.IsNullOrEmpty(rawText.Last()))
                {
                    rawText = rawText.Take(rawText.Length - 1).ToArray();
                }
                myComponents.RawText = rawText;
                myComponents.Path = path;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool isSuccess = FirmwareImageReader.TryRead(myComponents.RawText, out var image, out _);
            if (isSuccess)
                myComponents.Image = image;

            if (!isSuccess)
            {
                // Fallback to reading as a raw binary file
                try
                {
                    long startAddress = 0;
                    string ext = System.IO.Path.GetExtension(path).ToLowerInvariant();
                    if (ext == ".bin")
                    {
                        string input = "0x00000000";
                        if (InputDialog.Show("Binary Start Address", "Enter start address (hex):", ref input) != DialogResult.OK)
                            return;

                        input = input.Trim().Replace("0x", "").Replace("0X", "");
                        if (!long.TryParse(input, System.Globalization.NumberStyles.HexNumber, null, out startAddress))
                        {
                            MessageBox.Show("Invalid address format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    byte[] fileBytes = System.IO.File.ReadAllBytes(myComponents.Path);
                    var section = new Section { StartAddress = startAddress };
                    section.Bytes.AddRange(fileBytes);
                    myComponents.Image.Sections.Add(section);
                    myComponents.Image.Format = FormatType.Binary;
                    isSuccess = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading binary file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (isSuccess)
            {
                var name = string.IsNullOrEmpty(myComponents.Path) ? "Untitled" : System.IO.Path.GetFileName(myComponents.Path);
                this.Text = $"{_windowTitle} - {name}";
                _isModified = false;
                UpdateFileInfoUI();
            }
            else
            {
                MessageBox.Show("Could not open or parse the file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveFile(bool saveAs)
        {
            string savePath = myComponents.Path;

            if (saveAs || string.IsNullOrEmpty(savePath))
            {
                SaveFileDialog sfd = new SaveFileDialog { Title = "Save File As" };

                // Set filter and default extension based on the current file format
                if (myComponents.Image.Format == FormatType.S1Record ||
                    myComponents.Image.Format == FormatType.S2Record ||
                    myComponents.Image.Format == FormatType.S3Record ||
                    myComponents.Image.Format == FormatType.CombinedSRecord)
                {
                    sfd.Filter = "Motorola S-Record (*.mot;*.srec;*.s19)|*.mot;*.srec;*.s19|All files (*.*)|*.*";
                    sfd.DefaultExt = "mot";
                }
                else if (myComponents.Image.Format == FormatType.IntelHexLinear ||
                         myComponents.Image.Format == FormatType.IntelHexExLinear ||
                         myComponents.Image.Format == FormatType.IntelHexExSegment ||
                         myComponents.Image.Format == FormatType.IntelHexCombined)
                {
                    sfd.Filter = "Intel HEX (*.hex)|*.hex|All files (*.*)|*.*";
                    sfd.DefaultExt = "hex";
                }
                else if (myComponents.Image.Format == FormatType.Binary)
                {
                    sfd.Filter = "Binary file (*.bin)|*.bin|All files (*.*)|*.*";
                    sfd.DefaultExt = "bin";
                }
                else
                {
                    sfd.Filter = "All files (*.*)|*.*";
                }

                // Pre-populate the filename with the original filename
                if (!string.IsNullOrEmpty(myComponents.Path))
                {
                    sfd.FileName = System.IO.Path.GetFileName(myComponents.Path);
                }

                if (sfd.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                savePath = sfd.FileName;
            }

            // Build a snapshot of the current HexBox content without touching myComponents.Image,
            // so a failed/aborted save never leaves the in-memory model in a broken, half-applied state.
            var candidateImage = BuildSnapshotForSave();

            // Use Generate() for brand-new files (no original text to patch); otherwise keep the
            // surgical, line-structure-preserving Write(). Resize is impossible for Intel HEX/S-record
            // (AttachByteProvider locks Insert/Delete for those formats), so Write() never has to deal
            // with a size mismatch here.
            bool isNewFile = myComponents.RawText == null || myComponents.RawText.Length == 0;

            try
            {
                List<string> newImageArray = new List<string>();

                bool isBianry = false;
                if (candidateImage.Format == FormatType.S1Record ||
                    candidateImage.Format == FormatType.S2Record ||
                    candidateImage.Format == FormatType.S3Record ||
                    candidateImage.Format == FormatType.CombinedSRecord)
                {
                    newImageArray = isNewFile
                        ? SrecFormat.Generate(candidateImage, candidateImage.Format)
                        : SrecFormat.Write(myComponents.RawText, candidateImage);
                }
                else if (candidateImage.Format == FormatType.IntelHexLinear ||
                         candidateImage.Format == FormatType.IntelHexExLinear ||
                         candidateImage.Format == FormatType.IntelHexExSegment ||
                         candidateImage.Format == FormatType.IntelHexCombined)
                {
                    newImageArray = isNewFile
                        ? IntelHexFormat.Generate(candidateImage, candidateImage.Format)
                        : IntelHexFormat.Write(myComponents.RawText, candidateImage);
                }
                else if (candidateImage.Format == FormatType.Binary)
                {
                    string saveExt = System.IO.Path.GetExtension(savePath).ToLowerInvariant();
                    if (saveExt == ".hex")
                    {
                        newImageArray = IntelHexFormat.Generate(candidateImage, FormatType.IntelHexExLinear);
                    }
                    else if (saveExt == ".mot" || saveExt == ".srec" || saveExt == ".s19")
                    {
                        newImageArray = SrecFormat.Generate(candidateImage, FormatType.S3Record);
                    }
                    else
                    {
                        isBianry = true;
                    }
                }

                if (isBianry)
                {
                    using (System.IO.FileStream sw = new System.IO.FileStream(savePath, System.IO.FileMode.Create))
                    {
                        if (candidateImage.Sections.Count == 1)
                        {
                            foreach (var abyte in candidateImage.Sections[0].Bytes)
                                sw.WriteByte(abyte);

                        }

                    }


                }
                else
                {
                    if (newImageArray.Count <= 0)
                        return;

                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(savePath, false, Encoding.GetEncoding("ascii")))
                    {
                        foreach (var item in newImageArray)
                            sw.WriteLine(item);

                    }


                }

                LoadFile(savePath);

            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Save failed. Your in-memory data was not modified — you can fix the issue and try Save again.\n\nDetails: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // Builds a save-time snapshot of the current data without mutating myComponents.Image,
        // so a validation failure or write error never corrupts the live in-memory model.
        private FirmwareImage BuildSnapshotForSave()
        {
            var candidate = new FirmwareImage
            {
                Format = myComponents.Image.Format,
                OffsetAddress = myComponents.Image.OffsetAddress,
                Sections = myComponents.Image.Sections.Select(s => new Section(s)).ToList()
            };

            int currentIndex = PageAddressComboBox.SelectedIndex;
            if (currentIndex >= 0 && currentIndex < candidate.Sections.Count && _currentBuffer != null)
                CopyProviderBytesIntoSection(_currentBuffer, _currentAlignPadding, candidate.Sections[currentIndex]);

            return candidate;
        }

        #endregion

        #region UI Event Handlers

        private void PageAddressComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (PageAddressComboBox.SelectedIndex < 0 || myComponents.Image.Sections.Count == 0)
                return;

            if (_previousPageIndex >= 0)
            {
                // Save edits of the previous page before switching to current page
                UpdateImageFromHexBox(_previousPageIndex);
            }

            int index = PageAddressComboBox.SelectedIndex;
            _previousPageIndex = index;
            var section = myComponents.Image.Sections[index];
            PageSizeTextBox.Text = section.Bytes.Count.ToString();

            // --- 16-byte alignment padding ---
            // When the start address isn't 16-byte aligned, prepend (startAddress & 0xF)
            // dummy bytes so the hex view starts at the aligned row and the actual data
            // begins at the correct column. mainHexBox.LeadingPaddingByteCount tells HexBox
            // to render those leading bytes blank and keep them out of reach of selection
            // and keyboard navigation, instead of showing them as literal 00s.
            int alignPadding = (int)(section.StartAddress & 0xF);
            long alignedStart = section.StartAddress - alignPadding;

            _currentAlignPadding = alignPadding;

            var displayBytes = new List<byte>();
            for (int i = 0; i < alignPadding; i++)
                displayBytes.Add(0x00); // padding (not real data)
            displayBytes.AddRange(section.Bytes);

            var buffer = new InMemoryByteBuffer(displayBytes);
            AttachByteProvider(buffer);
            mainHexBox.LineInfoOffset = alignedStart;
            mainHexBox.LeadingPaddingByteCount = alignPadding;
        }

        private void MainHexBox_SelectionChanged(object sender, EventArgs e)
        {
            // SelectionStart is relative to the current page; LineInfoOffset is that page's
            // base address, so the two together give the absolute file address.
            long absoluteAddress = mainHexBox.SelectionStart + mainHexBox.LineInfoOffset;
            addressStatusLabel.Text = $"Address: 0x{absoluteAddress:X8} | Length: {mainHexBox.SelectionLength}";
            UpdateDataInspector();
        }

        private void MainHexBox_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void MainHexBox_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                if (CheckUnsavedChanges())
                {
                    return;
                }
                LoadFile(files[0]);
            }
        }

        private void GoToAddress()
        {
            string input = "0";
            if (InputDialog.Show("Go To Address", "Enter address (hex):", ref input) == DialogResult.OK)
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    return;
                }

                long address = 0;
                try
                {
                    address = Convert.ToInt64(input.Replace("0x", ""), 16);
                }
                catch
                {
                    MessageBox.Show("Invalid address format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                long start = address - mainHexBox.LineInfoOffset;
                if (start > mainHexBox.ByteProvider.Length)
                {
                    MessageBox.Show("The addres is not available in this section.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                mainHexBox.Select(start, 1);
                mainHexBox.Focus();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CheckUnsavedChanges())
            {
                e.Cancel = true;
            }
        }

        #endregion

        #region Helper Methods

        private void AttachByteProvider(InMemoryByteBuffer buffer)
        {
            if (_currentBuffer != null)
            {
                _currentBuffer.Changed -= HexBoxProvider_Changed;
                _currentBuffer.LengthChanged -= HexBoxProvider_LengthChanged;
            }

            _currentBuffer = buffer;
            buffer.Changed += HexBoxProvider_Changed;
            buffer.LengthChanged += HexBoxProvider_LengthChanged;

            // Intel HEX / S-record files loaded from disk have an original line structure that
            // resize would break (the on-disk record structure is address-fixed); Binary has no
            // such structure to protect, and neither does a brand-new (not-yet-saved) Intel
            // HEX/S-record file — there's no original text to preserve, so Save already falls
            // back to Generate() for it regardless (see the isNewFile check in SaveFile()).
            // The user can explicitly discard an existing original structure via "Enable Free
            // Editing...", which lifts the lock for the rest of this file's session
            // (see _freeEditingUnlocked).
            bool hasOriginalStructure = myComponents.RawText != null && myComponents.RawText.Length > 0;
            bool lockResize = myComponents.Image.Format != FormatType.Binary && hasOriginalStructure && !_freeEditingUnlocked;
            mainHexBox.ByteProvider = lockResize ? (IByteProvider)new SizeLockedByteProvider(buffer) : buffer;
            mainHexBox.ReadOnly = readOnlyCheckBox.Checked;

            enableFreeEditingButton.Enabled = lockResize;

            // While locked, Paste can only overwrite bytes in place (see PasteHex()'s
            // SupportsInsertBytes() branch) — struck through once Free Editing lifts that and
            // Paste goes back to inserting/replacing the selection like a normal editor.
            pasteBehaviorLabel.Font = lockResize ? _pasteBehaviorNormalFont : _pasteBehaviorStrikeoutFont;
        }

        private void EnableFreeEditingButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(this,
                "This discards this file's original line-by-line structure. Insert, Delete, Cut, and " +
                "Paste become fully available, but from now on Save will regenerate the file from " +
                "scratch using a standard layout — even a small edit may produce a large diff against " +
                "the original file.\n\n" +
                "This cannot be undone for this file except by reopening it.\n\n" +
                "Continue?",
                "Enable Free Editing",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            myComponents.RawText = new string[0];
            _freeEditingUnlocked = true;
            SetModified(true);
            AttachByteProvider(_currentBuffer);
        }

        private void HexBoxProvider_Changed(object sender, EventArgs e) => SetModified(true);

        private void HexBoxProvider_LengthChanged(object sender, EventArgs e) => RefreshSizeDisplay();

        // Keeps the "Size:" field and the status bar's byte count live during Insert/Delete
        // editing instead of only reflecting whatever they were at load/page-switch time.
        private void RefreshSizeDisplay()
        {
            if (_currentBuffer == null) return;

            long currentSectionSize = Math.Max(0, _currentBuffer.Length - _currentAlignPadding);
            PageSizeTextBox.Text = currentSectionSize.ToString();

            int currentIndex = PageAddressComboBox.SelectedIndex;
            long totalSize = 0;
            for (int i = 0; i < myComponents.Image.Sections.Count; i++)
                totalSize += i == currentIndex ? currentSectionSize : myComponents.Image.Sections[i].Bytes.Count;

            UpdateStatus(myComponents.Image.Format.ToString(), totalSize, myComponents.Image.Sections.Count);
        }

        private bool CheckUnsavedChanges()
        {
            if (!_isModified)
            {
                return false;
            }

            var result = MessageBox.Show(
                $"Do you want to save changes to {(string.IsNullOrEmpty(myComponents.Path) ? "Untitled" : myComponents.Path)}?",
                "Binary Viewer",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                SaveFile(false);
                return _isModified; // Return true if save failed
            }
            return result == DialogResult.Cancel;
        }

        private void SetModified(bool modified)
        {
            if (_isModified == modified) return;

            _isModified = modified;
            string path = string.IsNullOrEmpty(myComponents.Path) ? "Untitled" : System.IO.Path.GetFileName(myComponents.Path);
            this.Text = $"{_windowTitle} - {path}{(_isModified ? " *" : "")}";
        }

        private void UpdateFileInfoUI()
        {
            long totalSize = myComponents.Image.Sections.Sum(f => (long)f.Bytes.Count);
            UpdateStatus(myComponents.Image.Format.ToString(), totalSize, myComponents.Image.Sections.Count);

            FormatTextBox.Text = myComponents.Image.Format.ToString();

            PageAddressComboBox.SelectedIndexChanged -= PageAddressComboBox_SelectedIndexChanged;
            PageAddressComboBox.Items.Clear();
            foreach (var section in myComponents.Image.Sections)
            {
                string address = $"0x{section.StartAddress:X8}";
                PageAddressComboBox.Items.Add(address);
            }

            if (PageAddressComboBox.Items.Count > 0)
            {
                PageAddressComboBox.SelectedIndex = 0;
                _previousPageIndex = 0;
                PageAddressComboBox_SelectedIndexChanged(null, null);
            }
            PageAddressComboBox.SelectedIndexChanged += PageAddressComboBox_SelectedIndexChanged;
        }

        private void UpdateStatus(string format, long totalSize, int sectionCount)
        {
            fileFormatStatusLabel.Text = $"Format: {format} | Size: {totalSize} bytes | Sections: {sectionCount}";
        }

        // Skips the leading alignment-padding bytes (not real data) and copies the rest
        // of the provider's content into the target section.
        private static void CopyProviderBytesIntoSection(IByteProvider provider, int alignPadding, Section targetSection)
        {
            targetSection.Bytes.Clear();
            for (long i = alignPadding; i < provider.Length; i++)
            {
                targetSection.Bytes.Add(provider.ReadByte(i));
            }
        }

        private void UpdateImageFromHexBox(int index)
        {
            if (PageAddressComboBox.SelectedIndex < 0 || _currentBuffer == null)
                return;

            if(index >= PageAddressComboBox.Items.Count)
                return;

            if (!_isModified)
                return;

            CopyProviderBytesIntoSection(_currentBuffer, _currentAlignPadding, myComponents.Image.Sections[index]);
        }

        private void UpdateDataInspector()
        {
            long position = mainHexBox.SelectionStart;
            if (mainHexBox.ByteProvider == null || position < 0 || position >= mainHexBox.ByteProvider.Length)
            {
                sbyteLabel.Text = byteLabel.Text = shortLabel.Text = ushortLabel.Text = intLabel.Text =
                uintLabel.Text = longLabel.Text = ulongLabel.Text = floatLabel.Text = doubleLabel.Text = "-";
                return;
            }

            int readLength = (int)Math.Min(8, mainHexBox.ByteProvider.Length - position);
            byte[] rawData = new byte[8]; // Always use an 8-byte buffer
            for (int i = 0; i < readLength; i++)
            {
                rawData[i] = mainHexBox.ByteProvider.ReadByte(position + i);
            }

            bool wantBigEndian = bigEndianCheckBox.Checked;

            // Single-byte values are not affected by endianness
            sbyteLabel.Text = readLength >= 1 ? ((sbyte)rawData[0]).ToString() : "-";
            byteLabel.Text = readLength >= 1 ? rawData[0].ToString() : "-";

            // Multi-byte values
            UpdateMultiByteLabel(shortLabel, rawData, 2, wantBigEndian, (b) => BitConverter.ToInt16(b, 0).ToString());
            UpdateMultiByteLabel(ushortLabel, rawData, 2, wantBigEndian, (b) => BitConverter.ToUInt16(b, 0).ToString());
            UpdateMultiByteLabel(intLabel, rawData, 4, wantBigEndian, (b) => BitConverter.ToInt32(b, 0).ToString());
            UpdateMultiByteLabel(uintLabel, rawData, 4, wantBigEndian, (b) => BitConverter.ToUInt32(b, 0).ToString());
            UpdateMultiByteLabel(longLabel, rawData, 8, wantBigEndian, (b) => BitConverter.ToInt64(b, 0).ToString());
            UpdateMultiByteLabel(ulongLabel, rawData, 8, wantBigEndian, (b) => BitConverter.ToUInt64(b, 0).ToString());
            UpdateMultiByteLabel(floatLabel, rawData, 4, wantBigEndian, (b) => BitConverter.ToSingle(b, 0).ToString());
            UpdateMultiByteLabel(doubleLabel, rawData, 8, wantBigEndian, (b) => BitConverter.ToDouble(b, 0).ToString());
        }

        private void UpdateMultiByteLabel(Label label, byte[] source, int size, bool wantBigEndian, Func<byte[], string> converter)
        {
            if (source.Length < size)
            {
                label.Text = "-";
                return;
            }

            byte[] buffer = new byte[size];
            Array.Copy(source, buffer, size);

            // Reverse the buffer if the desired endianness is different from the system's native endianness
            bool needsReverse = (BitConverter.IsLittleEndian && wantBigEndian) ||
                                (!BitConverter.IsLittleEndian && !wantBigEndian);

            if (needsReverse)
            {
                Array.Reverse(buffer);
            }

            label.Text = converter(buffer);
        }

        #endregion


        #region Edit Menu Construction

        /// <summary>
        /// Builds all items under the Edit(&amp;E) menu programmatically.
        /// The Designer defines only the top-level editToolStripMenuItem.
        ///
        /// Final menu order:
        ///   [hidden] Undo          Ctrl+Z
        ///   ─────────────────────────────
        ///   Copy Hex               Ctrl+C
        ///   Copy Hex String ▶
        ///       Space separated
        ///       Tab separated
        ///       C array initializer
        ///   Paste                  Ctrl+V
        ///   Select All             Ctrl+A
        ///   Find...                Ctrl+F
        ///   ─────────────────────────────
        ///   Go To...               Ctrl+G
        ///   Reload                 Ctrl+R
        /// </summary>
        private void BuildEditMenu()
        {
            // ── Copy Hex ─────────────────────────────────────────────────────
            var copyHexItem = new ToolStripMenuItem("Copy Hex")
            {
                ShortcutKeys = Keys.Control | Keys.C
            };
            copyHexItem.Click += (s, e) => mainHexBox.CopyHex();

            // ── Copy Hex String (submenu) ─────────────────────────────────────
            var copyHexStringItem = new ToolStripMenuItem("Copy Hex String");

            var copySpaceItem = new ToolStripMenuItem("Space separated  (DB 0F ...)");
            copySpaceItem.Click += (s, e) => CopyHexString(' ');

            var copyTabItem = new ToolStripMenuItem("Tab separated  (for Excel)");
            copyTabItem.Click += (s, e) => CopyHexString('\t');

            var copyCArrayItem = new ToolStripMenuItem("C array initializer  ({ 0xDB, 0x0F, ... })");
            copyCArrayItem.Click += (s, e) => CopyHexStringCArray();

            copyHexStringItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                copySpaceItem,
                copyTabItem,
                copyCArrayItem
            });
            copyHexStringItem.DropDownOpening += (s, e) =>
            {
                bool has = mainHexBox.ByteProvider != null && mainHexBox.SelectionLength > 0;
                copySpaceItem.Enabled  = has;
                copyTabItem.Enabled    = has;
                copyCArrayItem.Enabled = has;
            };

            // ── Paste ─────────────────────────────────────────────────────────
            var pasteItem = new ToolStripMenuItem("Paste")
            {
                ShortcutKeys = Keys.Control | Keys.V
            };
            pasteItem.Click += (s, e) => mainHexBox.PasteHex();

            // ── Select All ───────────────────────────────────────────────────
            var selectAllItem = new ToolStripMenuItem("Select All")
            {
                ShortcutKeys = Keys.Control | Keys.A
            };
            selectAllItem.Click += (s, e) =>
            {
                if (mainHexBox.CanSelectAll())
                    mainHexBox.SelectAll();
            };

            // ── Find ──────────────────────────────────────────────────────────
            var findItem = new ToolStripMenuItem("Find...")
            {
                ShortcutKeys = Keys.Control | Keys.F
            };
            findItem.Click += (s, e) => ShowFindDialog();

            var sep1 = new ToolStripSeparator();

            // ── Go To ─────────────────────────────────────────────────────────
            var goToItem = new ToolStripMenuItem("Go To...")
            {
                ShortcutKeys = Keys.Control | Keys.G
            };
            goToItem.Click += (s, e) => GoToAddress();

            // ── Reload ────────────────────────────────────────────────────────
            var reloadItem = new ToolStripMenuItem("Reload")
            {
                ShortcutKeys = Keys.Control | Keys.R
            };
            reloadItem.Click += (s, e) => ReloadFile();

            // ── Assemble ──────────────────────────────────────────────────────
            editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
            {
                copyHexItem,
                copyHexStringItem,
                pasteItem,
                selectAllItem,
                findItem,
                sep1,
                goToItem,
                reloadItem
            });
        }

        #endregion


        #region Help Menu Construction

        /// <summary>
        /// Builds all items under the Help(&amp;H) menu programmatically.
        /// The Designer defines only the top-level helpToolStripMenuItem.
        /// </summary>
        private void BuildHelpMenu()
        {
            var aboutItem = new ToolStripMenuItem("About...");
            aboutItem.Click += (s, e) => ShowAboutDialog();

            helpToolStripMenuItem.DropDownItems.Add(aboutItem);
        }

        private void ShowAboutDialog()
        {
            var page = new TaskDialogPage
            {
                Caption = "About",
                Heading = $"{AssemblyProduct} {AssemblyVersion}",
                Text = $"{AssemblyCopyright}\n{RepositoryUrl}",
                Icon = TaskDialogIcon.Information,
                Buttons = { TaskDialogButton.Close }
            };

            var copyButton = new TaskDialogButton("Copy URL");
            copyButton.Click += (s, e) => Clipboard.SetText(RepositoryUrl);
            page.Buttons.Add(copyButton);

            TaskDialog.ShowDialog(this, page);
        }

        #endregion


        #region Edit Value (Context Menu)

        private void InitializeHexBoxContextMenu()
        {
            var editValueMenuItem = new ToolStripMenuItem("Edit Value...");
            editValueMenuItem.Click += (s, e) => ShowEditValueDialog(_contextMenuByteIndex);

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add(editValueMenuItem);

            // Capture the current byte position and validate when menu opens
            contextMenu.Opening += (s, e) =>
            {
                _contextMenuByteIndex = mainHexBox.SelectionStart;

                bool canEdit = mainHexBox.ByteProvider != null
                            && mainHexBox.ByteProvider.Length > 0
                            && _contextMenuByteIndex >= _currentAlignPadding
                            && _contextMenuByteIndex < mainHexBox.ByteProvider.Length;

                editValueMenuItem.Enabled = canEdit;
            };

            mainHexBox.ContextMenuStrip = contextMenu;
        }

        private void ShowEditValueDialog(long byteIndex)
        {
            var provider = mainHexBox.ByteProvider;
            if (provider == null || provider.Length == 0) return;
            if (byteIndex < _currentAlignPadding || byteIndex >= provider.Length) return;

            // Read up to 8 bytes from the current position
            int readCount = (int)Math.Min(8, provider.Length - byteIndex);
            byte[] sourceBytes = new byte[readCount];
            for (int i = 0; i < readCount; i++)
                sourceBytes[i] = provider.ReadByte(byteIndex + i);

            // Absolute address for display (matches the address shown in the status bar)
            long absoluteAddress = byteIndex + mainHexBox.LineInfoOffset;

            // Default endian matches the Data Inspector's current setting
            bool defaultLittleEndian = !bigEndianCheckBox.Checked;

            using (var dlg = new EditValueDialog(absoluteAddress, sourceBytes, defaultLittleEndian))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK || dlg.ResultBytes == null)
                    return;

                // Write bytes back to ByteProvider at the same position
                for (int i = 0; i < dlg.ResultBytes.Length; i++)
                    provider.WriteByte(byteIndex + i, dlg.ResultBytes[i]);

                // Refresh the HexBox display
                mainHexBox.Invalidate();
            }
        }

        #endregion


        #region Reload

        private void ReloadFile()
        {
            if (string.IsNullOrEmpty(myComponents?.Path))
                return;

            if (_isModified)
            {
                var result = MessageBox.Show(
                    "You have unsaved changes. Do you want to reload?",
                    "Binary Viewer",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                    return;
            }

            string path = myComponents.Path;
            _freeEditingUnlocked = false;
            LoadFile(path);
        }

        #endregion


        #region Copy Hex String

        /// <summary>
        /// Reads the current HexBox selection (excluding alignment padding) and copies
        /// each byte as a two-digit uppercase hex value joined by <paramref name="separator"/>,
        /// wrapping every 16 bytes onto a new line.
        /// Example (space): "DB 0F 49 40 ... (16 bytes)\nXX XX ..."
        /// Example (tab):   "DB\t0F\t...\nXX\t..."  ← each row = one Excel row of 16 columns
        /// </summary>
        private void CopyHexString(char separator)
        {
            byte[] bytes = GetSelectedBytes(out long startAddress);
            if (bytes == null || bytes.Length == 0) return;

            const int bytesPerLine = 16;
            var lines = SplitIntoAddressAlignedRows(bytes, startAddress, bytesPerLine)
                .Select(row => string.Join(separator.ToString(), row.Select(b => b.ToString("X2"))));

            Clipboard.SetText(string.Join(Environment.NewLine, lines));
        }

        /// <summary>
        /// Reads the current HexBox selection and copies it as a C array initializer,
        /// wrapping every 16 bytes onto a new indented line.
        /// Example:
        /// {
        ///     0xDB, 0x0F, 0x49, 0x40, ...,
        ///     0x00, 0x01, ...
        /// }
        /// </summary>
        private void CopyHexStringCArray()
        {
            byte[] bytes = GetSelectedBytes(out long startAddress);
            if (bytes == null || bytes.Length == 0) return;

            const int bytesPerLine = 16;
            var sb = new StringBuilder();
            sb.AppendLine("{");

            foreach (var row in SplitIntoAddressAlignedRows(bytes, startAddress, bytesPerLine))
            {
                var chunk = row.Select(b => $"0x{b:X2}");
                // Add a trailing comma after every line, including the last
                sb.Append("    ");
                sb.Append(string.Join(", ", chunk));
                sb.AppendLine(",");
            }

            sb.Append("}");
            Clipboard.SetText(sb.ToString());
        }

        /// <summary>
        /// Returns the currently selected bytes from the HexBox ByteProvider,
        /// skipping any leading alignment-padding bytes, along with the absolute
        /// file address of the first returned byte.
        /// Returns null if nothing is selected or no data is loaded.
        /// </summary>
        private byte[] GetSelectedBytes(out long startAddress)
        {
            startAddress = 0;

            var provider = mainHexBox.ByteProvider;
            if (provider == null || mainHexBox.SelectionLength <= 0)
                return null;

            long start = mainHexBox.SelectionStart;
            long length = mainHexBox.SelectionLength;

            // Clamp the start to the first real (non-padding) byte
            if (start < _currentAlignPadding)
            {
                long skip = _currentAlignPadding - start;
                start  += skip;
                length -= skip;
            }

            if (length <= 0 || start >= provider.Length)
                return null;

            length = Math.Min(length, provider.Length - start);

            byte[] result = new byte[length];
            for (long i = 0; i < length; i++)
                result[i] = provider.ReadByte(start + i);

            startAddress = start + mainHexBox.LineInfoOffset;
            return result;
        }

        /// <summary>
        /// Splits <paramref name="bytes"/> into rows of <paramref name="bytesPerLine"/>, with
        /// the first row shortened so row boundaries fall on the same 16-byte address
        /// boundaries the hex grid itself uses. Without this, a selection that doesn't start
        /// on a row boundary (e.g. a non-16-aligned section start address) would produce line
        /// breaks that don't match what's shown on screen.
        /// </summary>
        private static IEnumerable<byte[]> SplitIntoAddressAlignedRows(byte[] bytes, long startAddress, int bytesPerLine)
        {
            int firstRowSize = bytesPerLine - (int)(startAddress % bytesPerLine);
            int offset = 0;

            if (firstRowSize < bytesPerLine)
            {
                int count = Math.Min(firstRowSize, bytes.Length);
                yield return bytes.Take(count).ToArray();
                offset = count;
            }

            for (int i = offset; i < bytes.Length; i += bytesPerLine)
            {
                int count = Math.Min(bytesPerLine, bytes.Length - i);
                yield return bytes.Skip(i).Take(count).ToArray();
            }
        }

        #endregion


        #region Search Methods

        private void ShowFindDialog()
        {
            // Create a new instance if the form has not been created or has been disposed
            if (_findFormInstance == null || _findFormInstance.IsDisposed)
            {
                _findFormInstance = new FindForm();
                // Subscribe to the FindNext event of FindForm
                _findFormInstance.FindNext += FindForm_FindNext;
                // Set the instance to null when the form is closed
                _findFormInstance.FormClosed += (s, e) => { _findFormInstance = null; };
            }

            // Show and activate the form
            _findFormInstance.Show(this); // Pass 'this' to set the owner
            _findFormInstance.Activate();
        }

        // Called when the FindNext event is raised by FindForm
        private void FindForm_FindNext(object sender, FindEventArgs e)
        {
            // Determine whether the search string has changed or this is the first search
            bool isNewSearch = (_lastSearchBytes == null || !_lastSearchBytes.SequenceEqual(e.SearchBytes));

            // Save the current search string
            _lastSearchBytes = e.SearchBytes;

            PerformSearch(isNewSearch);
        }

        /// <summary>
        /// Main search execution method. Manages the search start position and handles
        /// wrap-around behavior (returning to the beginning after reaching the end).
        /// </summary>
        /// <param name="isNewSearch">True if the search condition has changed or this is the first search.</param>
        private void PerformSearch(bool isNewSearch)
        {
            if (_lastSearchBytes == null || _lastSearchBytes.Length == 0) return;
            var provider = mainHexBox.ByteProvider;
            if (provider == null || provider.Length == 0)
            {
                MessageBox.Show("There is no data to search.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            long startIndex = isNewSearch ? 0 : mainHexBox.SelectionStart + 1;

            // 1. Search from the start position to the end of the file
            long foundPosition = FindBytesInRange(startIndex, provider.Length);

            if (foundPosition != -1)
            {
                // Found
                mainHexBox.Select(foundPosition, _lastSearchBytes.Length);
                mainHexBox.Focus();
                return;
            }

            // 2. If not found by the end, ask whether to wrap around and search from the beginning
            if (startIndex > 0)
            {
                var result = MessageBox.Show("Reached the end of the data. Continue search from the beginning?", "Binary Viewer", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    return;
                }

                // Search from the beginning up to the original start position
                foundPosition = FindBytesInRange(0, startIndex);

                if (foundPosition != -1)
                {
                    // Found
                    mainHexBox.Select(foundPosition, _lastSearchBytes.Length);
                    mainHexBox.Focus();
                    return;
                }
            }

            // 3. Not found after searching the entire file
            MessageBox.Show("Cannot find the specified value.", "Binary Viewer", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Helper method to search for a byte sequence within a specified range.
        /// </summary>
        /// <returns>The position where the sequence was found, or -1 if not found.</returns>
        private long FindBytesInRange(long startIndex, long endIndex)
        {
            var provider = mainHexBox.ByteProvider;
            long limit = Math.Min(endIndex, provider.Length);

            for (long i = startIndex; i <= limit - _lastSearchBytes.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < _lastSearchBytes.Length; j++)
                {
                    if (provider.ReadByte(i + j) != _lastSearchBytes[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                {
                    return i; // Return the found position
                }
            }
            return -1; // Not found
        }

        #endregion
    }


    /// <summary>
    /// Helper class to replace Microsoft.VisualBasic.Interaction.InputBox.
    /// </summary>
    public static class InputDialog
    {
        public static DialogResult Show(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new System.Drawing.Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new System.Drawing.Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }
    }
}