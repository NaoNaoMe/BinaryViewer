namespace BinaryViewer
{
    partial class MainForm
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.fileFormatStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.addressStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.mainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.mainHexBox = new BinaryViewer.Controls.HexBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.PageAddressComboBox = new System.Windows.Forms.ComboBox();
            this.PageSizeTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.FormatTextBox = new System.Windows.Forms.TextBox();
            this.readOnlyCheckBox = new System.Windows.Forms.CheckBox();
            this.enableFreeEditingButton = new System.Windows.Forms.Button();
            this.pasteBehaviorLabel = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.doubleLabel = new System.Windows.Forms.Label();
            this.floatLabel = new System.Windows.Forms.Label();
            this.ulongLabel = new System.Windows.Forms.Label();
            this.longLabel = new System.Windows.Forms.Label();
            this.uintLabel = new System.Windows.Forms.Label();
            this.intLabel = new System.Windows.Forms.Label();
            this.ushortLabel = new System.Windows.Forms.Label();
            this.shortLabel = new System.Windows.Forms.Label();
            this.byteLabel = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.sbyteLabel = new System.Windows.Forms.Label();
            this.bigEndianCheckBox = new System.Windows.Forms.CheckBox();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
            this.mainSplitContainer.Panel1.SuspendLayout();
            this.mainSplitContainer.Panel2.SuspendLayout();
            this.mainSplitContainer.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(884, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.toolStripSeparator2,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(51, 20);
            this.fileToolStripMenuItem.Text = "File(&F)";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.newToolStripMenuItem.Text = "New";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.openToolStripMenuItem.Text = "Open...";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(191, 6);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.saveToolStripMenuItem.Text = "Save";
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.saveAsToolStripMenuItem.Text = "Save As...";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(191, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
            this.editToolStripMenuItem.Text = "Edit(&E)";
            //
            // helpToolStripMenuItem
            //
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this.helpToolStripMenuItem.Text = "Help(&H)";
            //
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileFormatStatusLabel,
            this.addressStatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 539);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            this.statusStrip1.Size = new System.Drawing.Size(884, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // fileFormatStatusLabel
            // 
            this.fileFormatStatusLabel.Name = "fileFormatStatusLabel";
            this.fileFormatStatusLabel.Size = new System.Drawing.Size(50, 17);
            this.fileFormatStatusLabel.Text = "Format: ";
            // 
            // addressStatusLabel
            // 
            this.addressStatusLabel.Name = "addressStatusLabel";
            this.addressStatusLabel.Size = new System.Drawing.Size(115, 17);
            this.addressStatusLabel.Text = "Address: 0x00000000";
            // 
            // mainSplitContainer
            // 
            this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.mainSplitContainer.Location = new System.Drawing.Point(0, 24);
            this.mainSplitContainer.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.mainSplitContainer.Name = "mainSplitContainer";
            // 
            // mainSplitContainer.Panel1
            // 
            this.mainSplitContainer.Panel1.Controls.Add(this.mainHexBox);
            // 
            // mainSplitContainer.Panel2
            // 
            this.mainSplitContainer.Panel2.Controls.Add(this.tableLayoutPanel1);
            this.mainSplitContainer.Size = new System.Drawing.Size(884, 515);
            this.mainSplitContainer.SplitterDistance = 599;
            this.mainSplitContainer.TabIndex = 3;
            // 
            // mainHexBox
            // 
            this.mainHexBox.ColumnInfoVisible = true;
            this.mainHexBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainHexBox.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.mainHexBox.LineInfoVisible = true;
            this.mainHexBox.Location = new System.Drawing.Point(0, 0);
            this.mainHexBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.mainHexBox.Name = "mainHexBox";
            this.mainHexBox.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
            this.mainHexBox.Size = new System.Drawing.Size(599, 515);
            this.mainHexBox.StringViewVisible = true;
            this.mainHexBox.TabIndex = 1;
            this.mainHexBox.UseFixedBytesPerLine = true;
            this.mainHexBox.VScrollBarVisible = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox3, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(281, 515);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.tableLayoutPanel2);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 4);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Size = new System.Drawing.Size(275, 198);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "File Info";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.PageAddressComboBox, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.PageSizeTextBox, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.label9, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.FormatTextBox, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.readOnlyCheckBox, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.enableFreeEditingButton, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.pasteBehaviorLabel, 0, 5);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 20);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 6;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(269, 174);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 15);
            this.label1.TabIndex = 10;
            this.label1.Text = "Start Address:";
            // 
            // PageAddressComboBox
            // 
            this.PageAddressComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.PageAddressComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.PageAddressComboBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PageAddressComboBox.FormattingEnabled = true;
            this.PageAddressComboBox.Location = new System.Drawing.Point(88, 34);
            this.PageAddressComboBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.PageAddressComboBox.Name = "PageAddressComboBox";
            this.PageAddressComboBox.Size = new System.Drawing.Size(178, 22);
            this.PageAddressComboBox.TabIndex = 1;
            // 
            // PageSizeTextBox
            // 
            this.PageSizeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.PageSizeTextBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PageSizeTextBox.Location = new System.Drawing.Point(88, 64);
            this.PageSizeTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.PageSizeTextBox.Name = "PageSizeTextBox";
            this.PageSizeTextBox.ReadOnly = true;
            this.PageSizeTextBox.Size = new System.Drawing.Size(178, 22);
            this.PageSizeTextBox.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 67);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 15);
            this.label2.TabIndex = 11;
            this.label2.Text = "Size:";
            // 
            // label9
            // 
            this.label9.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(3, 7);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(48, 15);
            this.label9.TabIndex = 12;
            this.label9.Text = "Format:";
            // 
            // FormatTextBox
            // 
            this.FormatTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.FormatTextBox.Font = new System.Drawing.Font("Consolas", 9F);
            this.FormatTextBox.Location = new System.Drawing.Point(88, 4);
            this.FormatTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.FormatTextBox.Name = "FormatTextBox";
            this.FormatTextBox.ReadOnly = true;
            this.FormatTextBox.Size = new System.Drawing.Size(178, 22);
            this.FormatTextBox.TabIndex = 13;
            // 
            // readOnlyCheckBox
            //
            this.readOnlyCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.readOnlyCheckBox.AutoSize = true;
            this.tableLayoutPanel2.SetColumnSpan(this.readOnlyCheckBox, 2);
            this.readOnlyCheckBox.Checked = true;
            this.readOnlyCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.readOnlyCheckBox.Location = new System.Drawing.Point(3, 105);
            this.readOnlyCheckBox.Name = "readOnlyCheckBox";
            this.readOnlyCheckBox.Size = new System.Drawing.Size(155, 19);
            this.readOnlyCheckBox.TabIndex = 16;
            this.readOnlyCheckBox.Text = "Read only";
            this.readOnlyCheckBox.UseVisualStyleBackColor = true;
            //
            // enableFreeEditingButton
            //
            this.enableFreeEditingButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tableLayoutPanel2.SetColumnSpan(this.enableFreeEditingButton, 2);
            this.enableFreeEditingButton.Enabled = false;
            this.enableFreeEditingButton.Location = new System.Drawing.Point(3, 127);
            this.enableFreeEditingButton.Name = "enableFreeEditingButton";
            this.enableFreeEditingButton.Size = new System.Drawing.Size(160, 25);
            this.enableFreeEditingButton.TabIndex = 17;
            this.enableFreeEditingButton.Text = "Enable Free Editing...";
            this.enableFreeEditingButton.UseVisualStyleBackColor = true;
            //
            // pasteBehaviorLabel
            //
            this.pasteBehaviorLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel2.SetColumnSpan(this.pasteBehaviorLabel, 2);
            this.pasteBehaviorLabel.AutoSize = false;
            this.pasteBehaviorLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.pasteBehaviorLabel.Location = new System.Drawing.Point(3, 149);
            this.pasteBehaviorLabel.Name = "pasteBehaviorLabel";
            this.pasteBehaviorLabel.Size = new System.Drawing.Size(263, 30);
            this.pasteBehaviorLabel.TabIndex = 18;
            this.pasteBehaviorLabel.Text = "Paste overwrites in place, size unchanged.";
            //
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.tableLayoutPanel4);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(3, 156);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox3.Size = new System.Drawing.Size(275, 332);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Data Inspector";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this.doubleLabel, 1, 9);
            this.tableLayoutPanel4.Controls.Add(this.floatLabel, 1, 8);
            this.tableLayoutPanel4.Controls.Add(this.ulongLabel, 1, 7);
            this.tableLayoutPanel4.Controls.Add(this.longLabel, 1, 6);
            this.tableLayoutPanel4.Controls.Add(this.uintLabel, 1, 5);
            this.tableLayoutPanel4.Controls.Add(this.intLabel, 1, 4);
            this.tableLayoutPanel4.Controls.Add(this.ushortLabel, 1, 3);
            this.tableLayoutPanel4.Controls.Add(this.shortLabel, 1, 2);
            this.tableLayoutPanel4.Controls.Add(this.byteLabel, 1, 1);
            this.tableLayoutPanel4.Controls.Add(this.label18, 0, 9);
            this.tableLayoutPanel4.Controls.Add(this.label17, 0, 8);
            this.tableLayoutPanel4.Controls.Add(this.label16, 0, 7);
            this.tableLayoutPanel4.Controls.Add(this.label15, 0, 6);
            this.tableLayoutPanel4.Controls.Add(this.label14, 0, 5);
            this.tableLayoutPanel4.Controls.Add(this.label13, 0, 4);
            this.tableLayoutPanel4.Controls.Add(this.label12, 0, 3);
            this.tableLayoutPanel4.Controls.Add(this.label11, 0, 2);
            this.tableLayoutPanel4.Controls.Add(this.label10, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.label4, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.sbyteLabel, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.bigEndianCheckBox, 0, 10);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 20);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 11;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 9F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 9F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 9F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 9F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 9F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 9F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 9F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 9F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 9F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 9F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 9F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(269, 308);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // doubleLabel
            // 
            this.doubleLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.doubleLabel.AutoSize = true;
            this.doubleLabel.Location = new System.Drawing.Point(73, 258);
            this.doubleLabel.Name = "doubleLabel";
            this.doubleLabel.Size = new System.Drawing.Size(12, 15);
            this.doubleLabel.TabIndex = 19;
            this.doubleLabel.Text = "-";
            // 
            // floatLabel
            // 
            this.floatLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.floatLabel.AutoSize = true;
            this.floatLabel.Location = new System.Drawing.Point(73, 230);
            this.floatLabel.Name = "floatLabel";
            this.floatLabel.Size = new System.Drawing.Size(12, 15);
            this.floatLabel.TabIndex = 18;
            this.floatLabel.Text = "-";
            // 
            // ulongLabel
            // 
            this.ulongLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.ulongLabel.AutoSize = true;
            this.ulongLabel.Location = new System.Drawing.Point(73, 202);
            this.ulongLabel.Name = "ulongLabel";
            this.ulongLabel.Size = new System.Drawing.Size(12, 15);
            this.ulongLabel.TabIndex = 17;
            this.ulongLabel.Text = "-";
            // 
            // longLabel
            // 
            this.longLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.longLabel.AutoSize = true;
            this.longLabel.Location = new System.Drawing.Point(73, 174);
            this.longLabel.Name = "longLabel";
            this.longLabel.Size = new System.Drawing.Size(12, 15);
            this.longLabel.TabIndex = 16;
            this.longLabel.Text = "-";
            // 
            // uintLabel
            // 
            this.uintLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.uintLabel.AutoSize = true;
            this.uintLabel.Location = new System.Drawing.Point(73, 146);
            this.uintLabel.Name = "uintLabel";
            this.uintLabel.Size = new System.Drawing.Size(12, 15);
            this.uintLabel.TabIndex = 15;
            this.uintLabel.Text = "-";
            // 
            // intLabel
            // 
            this.intLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.intLabel.AutoSize = true;
            this.intLabel.Location = new System.Drawing.Point(73, 118);
            this.intLabel.Name = "intLabel";
            this.intLabel.Size = new System.Drawing.Size(12, 15);
            this.intLabel.TabIndex = 14;
            this.intLabel.Text = "-";
            // 
            // ushortLabel
            // 
            this.ushortLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.ushortLabel.AutoSize = true;
            this.ushortLabel.Location = new System.Drawing.Point(73, 90);
            this.ushortLabel.Name = "ushortLabel";
            this.ushortLabel.Size = new System.Drawing.Size(12, 15);
            this.ushortLabel.TabIndex = 13;
            this.ushortLabel.Text = "-";
            // 
            // shortLabel
            // 
            this.shortLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.shortLabel.AutoSize = true;
            this.shortLabel.Location = new System.Drawing.Point(73, 62);
            this.shortLabel.Name = "shortLabel";
            this.shortLabel.Size = new System.Drawing.Size(12, 15);
            this.shortLabel.TabIndex = 12;
            this.shortLabel.Text = "-";
            // 
            // byteLabel
            // 
            this.byteLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.byteLabel.AutoSize = true;
            this.byteLabel.Location = new System.Drawing.Point(73, 34);
            this.byteLabel.Name = "byteLabel";
            this.byteLabel.Size = new System.Drawing.Size(12, 15);
            this.byteLabel.TabIndex = 11;
            this.byteLabel.Text = "-";
            // 
            // label18
            // 
            this.label18.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(3, 258);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(47, 15);
            this.label18.TabIndex = 10;
            this.label18.Text = "double:";
            // 
            // label17
            // 
            this.label17.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(3, 230);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(34, 15);
            this.label17.TabIndex = 9;
            this.label17.Text = "float:";
            // 
            // label16
            // 
            this.label16.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(3, 202);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(41, 15);
            this.label16.TabIndex = 8;
            this.label16.Text = "ulong:";
            // 
            // label15
            // 
            this.label15.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(3, 174);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(34, 15);
            this.label15.TabIndex = 7;
            this.label15.Text = "long:";
            // 
            // label14
            // 
            this.label14.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(3, 146);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(31, 15);
            this.label14.TabIndex = 6;
            this.label14.Text = "uint:";
            // 
            // label13
            // 
            this.label13.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(3, 118);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(24, 15);
            this.label13.TabIndex = 5;
            this.label13.Text = "int:";
            // 
            // label12
            // 
            this.label12.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(3, 90);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(44, 15);
            this.label12.TabIndex = 4;
            this.label12.Text = "ushort:";
            // 
            // label11
            // 
            this.label11.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(3, 62);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(37, 15);
            this.label11.TabIndex = 3;
            this.label11.Text = "short:";
            // 
            // label10
            // 
            this.label10.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(3, 34);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(33, 15);
            this.label10.TabIndex = 2;
            this.label10.Text = "byte:";
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 6);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 15);
            this.label4.TabIndex = 0;
            this.label4.Text = "sbyte:";
            // 
            // sbyteLabel
            // 
            this.sbyteLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.sbyteLabel.AutoSize = true;
            this.sbyteLabel.Location = new System.Drawing.Point(73, 6);
            this.sbyteLabel.Name = "sbyteLabel";
            this.sbyteLabel.Size = new System.Drawing.Size(12, 15);
            this.sbyteLabel.TabIndex = 1;
            this.sbyteLabel.Text = "-";
            // 
            // bigEndianCheckBox
            // 
            this.bigEndianCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.bigEndianCheckBox.AutoSize = true;
            this.tableLayoutPanel4.SetColumnSpan(this.bigEndianCheckBox, 2);
            this.bigEndianCheckBox.Location = new System.Drawing.Point(3, 284);
            this.bigEndianCheckBox.Name = "bigEndianCheckBox";
            this.bigEndianCheckBox.Size = new System.Drawing.Size(84, 19);
            this.bigEndianCheckBox.TabIndex = 20;
            this.bigEndianCheckBox.Text = "Big-Endian";
            this.bigEndianCheckBox.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 561);
            this.Controls.Add(this.mainSplitContainer);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = LoadEmbeddedIcon();
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "MainForm";
            this.Text = "title text";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.mainSplitContainer.Panel1.ResumeLayout(false);
            this.mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
            this.mainSplitContainer.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.SplitContainer mainSplitContainer;
        private BinaryViewer.Controls.HexBox mainHexBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox PageAddressComboBox;
        private System.Windows.Forms.TextBox PageSizeTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel addressStatusLabel;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label sbyteLabel;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox FormatTextBox;
        private System.Windows.Forms.Label doubleLabel;
        private System.Windows.Forms.Label floatLabel;
        private System.Windows.Forms.Label ulongLabel;
        private System.Windows.Forms.Label longLabel;
        private System.Windows.Forms.Label uintLabel;
        private System.Windows.Forms.Label intLabel;
        private System.Windows.Forms.Label ushortLabel;
        private System.Windows.Forms.Label shortLabel;
        private System.Windows.Forms.Label byteLabel;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ToolStripStatusLabel fileFormatStatusLabel;
        private System.Windows.Forms.CheckBox bigEndianCheckBox;
        private System.Windows.Forms.CheckBox readOnlyCheckBox;
        private System.Windows.Forms.Button enableFreeEditingButton;
        private System.Windows.Forms.Label pasteBehaviorLabel;
    }
}