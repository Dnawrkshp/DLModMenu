namespace DLModMenu
{
    partial class ctlMain
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Root");
            this.modMenuLayout = new System.Windows.Forms.TreeView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addBoxToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addRootToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nodeControlStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.copyNodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteNodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.controlsBox = new System.Windows.Forms.PictureBox();
            this.LoadMMenu = new System.Windows.Forms.Button();
            this.SaveMMenu = new System.Windows.Forms.Button();
            this.isThreadRunning = new System.Windows.Forms.CheckBox();
            this.addPrefixesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addSelectorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addColoredBoolTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controlsBox)).BeginInit();
            this.SuspendLayout();
            // 
            // modMenuLayout
            // 
            this.modMenuLayout.HideSelection = false;
            this.modMenuLayout.Location = new System.Drawing.Point(3, 3);
            this.modMenuLayout.Name = "modMenuLayout";
            treeNode3.Name = "nullRoot";
            treeNode3.Tag = "Main Root";
            treeNode3.Text = "Root";
            this.modMenuLayout.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode3});
            this.modMenuLayout.Size = new System.Drawing.Size(152, 377);
            this.modMenuLayout.TabIndex = 0;
            this.modMenuLayout.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.modMenuLayout_AfterSelect);
            this.modMenuLayout.MouseUp += new System.Windows.Forms.MouseEventHandler(this.modMenuLayout_MouseUp);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addTextToolStripMenuItem,
            this.addBoxToolStripMenuItem,
            this.addRootToolStripMenuItem,
            this.nodeControlStripMenuItem1,
            this.addPrefixesToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(152, 114);
            // 
            // addTextToolStripMenuItem
            // 
            this.addTextToolStripMenuItem.Name = "addTextToolStripMenuItem";
            this.addTextToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.addTextToolStripMenuItem.Text = "Add Text";
            this.addTextToolStripMenuItem.Click += new System.EventHandler(this.addTextToolStripMenuItem_Click);
            // 
            // addBoxToolStripMenuItem
            // 
            this.addBoxToolStripMenuItem.Name = "addBoxToolStripMenuItem";
            this.addBoxToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.addBoxToolStripMenuItem.Text = "Add Box";
            this.addBoxToolStripMenuItem.Click += new System.EventHandler(this.addBoxToolStripMenuItem_Click);
            // 
            // addRootToolStripMenuItem
            // 
            this.addRootToolStripMenuItem.Name = "addRootToolStripMenuItem";
            this.addRootToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.addRootToolStripMenuItem.Text = "Add Root";
            this.addRootToolStripMenuItem.Click += new System.EventHandler(this.addRootToolStripMenuItem_Click);
            // 
            // nodeControlStripMenuItem1
            // 
            this.nodeControlStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyNodeToolStripMenuItem,
            this.pasteNodeToolStripMenuItem,
            this.removeToolStripMenuItem});
            this.nodeControlStripMenuItem1.Name = "nodeControlStripMenuItem1";
            this.nodeControlStripMenuItem1.Size = new System.Drawing.Size(151, 22);
            this.nodeControlStripMenuItem1.Text = "Node Controls";
            // 
            // copyNodeToolStripMenuItem
            // 
            this.copyNodeToolStripMenuItem.Name = "copyNodeToolStripMenuItem";
            this.copyNodeToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.copyNodeToolStripMenuItem.Text = "Copy Node";
            this.copyNodeToolStripMenuItem.Click += new System.EventHandler(this.copyNodeToolStripMenuItem_Click);
            // 
            // pasteNodeToolStripMenuItem
            // 
            this.pasteNodeToolStripMenuItem.Name = "pasteNodeToolStripMenuItem";
            this.pasteNodeToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.pasteNodeToolStripMenuItem.Text = "Paste Node";
            this.pasteNodeToolStripMenuItem.Click += new System.EventHandler(this.pasteNodeToolStripMenuItem_Click);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.removeToolStripMenuItem.Text = "Remove Node";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
            // 
            // controlsBox
            // 
            this.controlsBox.Location = new System.Drawing.Point(161, 3);
            this.controlsBox.Name = "controlsBox";
            this.controlsBox.Size = new System.Drawing.Size(730, 427);
            this.controlsBox.TabIndex = 1;
            this.controlsBox.TabStop = false;
            // 
            // LoadMMenu
            // 
            this.LoadMMenu.Location = new System.Drawing.Point(3, 386);
            this.LoadMMenu.Name = "LoadMMenu";
            this.LoadMMenu.Size = new System.Drawing.Size(75, 23);
            this.LoadMMenu.TabIndex = 2;
            this.LoadMMenu.Text = "Load";
            this.LoadMMenu.UseVisualStyleBackColor = true;
            this.LoadMMenu.Click += new System.EventHandler(this.LoadMMenu_Click);
            // 
            // SaveMMenu
            // 
            this.SaveMMenu.Location = new System.Drawing.Point(80, 386);
            this.SaveMMenu.Name = "SaveMMenu";
            this.SaveMMenu.Size = new System.Drawing.Size(75, 23);
            this.SaveMMenu.TabIndex = 3;
            this.SaveMMenu.Text = "Save";
            this.SaveMMenu.UseVisualStyleBackColor = true;
            this.SaveMMenu.Click += new System.EventHandler(this.SaveMMenu_Click);
            // 
            // isThreadRunning
            // 
            this.isThreadRunning.AutoSize = true;
            this.isThreadRunning.Location = new System.Drawing.Point(3, 415);
            this.isThreadRunning.Name = "isThreadRunning";
            this.isThreadRunning.Size = new System.Drawing.Size(130, 17);
            this.isThreadRunning.TabIndex = 4;
            this.isThreadRunning.Text = "Input Thread Running";
            this.isThreadRunning.UseVisualStyleBackColor = true;
            this.isThreadRunning.CheckedChanged += new System.EventHandler(this.isThreadRunning_CheckedChanged);
            // 
            // addPrefixesToolStripMenuItem
            // 
            this.addPrefixesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addSelectorToolStripMenuItem,
            this.addColoredBoolTextToolStripMenuItem});
            this.addPrefixesToolStripMenuItem.Name = "addPrefixesToolStripMenuItem";
            this.addPrefixesToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.addPrefixesToolStripMenuItem.Text = "Add Prefixes";
            // 
            // addSelectorToolStripMenuItem
            // 
            this.addSelectorToolStripMenuItem.Name = "addSelectorToolStripMenuItem";
            this.addSelectorToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.addSelectorToolStripMenuItem.Text = "Add Selector";
            this.addSelectorToolStripMenuItem.Click += new System.EventHandler(this.addSelectorToolStripMenuItem_Click);
            // 
            // addColoredBoolTextToolStripMenuItem
            // 
            this.addColoredBoolTextToolStripMenuItem.Name = "addColoredBoolTextToolStripMenuItem";
            this.addColoredBoolTextToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.addColoredBoolTextToolStripMenuItem.Text = "Add Colored Bool Text";
            this.addColoredBoolTextToolStripMenuItem.Click += new System.EventHandler(this.addColoredBoolTextToolStripMenuItem_Click);
            // 
            // ctlMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.isThreadRunning);
            this.Controls.Add(this.SaveMMenu);
            this.Controls.Add(this.LoadMMenu);
            this.Controls.Add(this.controlsBox);
            this.Controls.Add(this.modMenuLayout);
            this.Name = "ctlMain";
            this.Size = new System.Drawing.Size(897, 441);
            this.Load += new System.EventHandler(this.ctlMain_Load);
            this.Resize += new System.EventHandler(this.ctlMain_Resize);
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.controlsBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem addTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addBoxToolStripMenuItem;
        private System.Windows.Forms.PictureBox controlsBox;
        private System.Windows.Forms.ToolStripMenuItem addRootToolStripMenuItem;
        public System.Windows.Forms.TreeView modMenuLayout;
        private System.Windows.Forms.ToolStripMenuItem nodeControlStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem copyNodeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteNodeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.Button LoadMMenu;
        private System.Windows.Forms.Button SaveMMenu;
        private System.Windows.Forms.CheckBox isThreadRunning;
        private System.Windows.Forms.ToolStripMenuItem addPrefixesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addSelectorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addColoredBoolTextToolStripMenuItem;
    }
}
