namespace DLModMenu
{
    partial class InputHandler
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
            this.compileButt = new System.Windows.Forms.Button();
            this.inputBox = new FastColoredTextBoxNS.FastColoredTextBox();
            this.execButt = new System.Windows.Forms.Button();
            this.statusLabel = new System.Windows.Forms.Label();
            this.buildButt = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.inputBox)).BeginInit();
            this.SuspendLayout();
            // 
            // compileButt
            // 
            this.compileButt.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.compileButt.Location = new System.Drawing.Point(165, 147);
            this.compileButt.Name = "compileButt";
            this.compileButt.Size = new System.Drawing.Size(75, 23);
            this.compileButt.TabIndex = 2;
            this.compileButt.Text = "Save";
            this.compileButt.UseVisualStyleBackColor = true;
            this.compileButt.Click += new System.EventHandler(this.compileButt_Click);
            // 
            // inputBox
            // 
            this.inputBox.AutoScrollMinSize = new System.Drawing.Size(25, 13);
            this.inputBox.BackBrush = null;
            this.inputBox.BackColor = System.Drawing.Color.Black;
            this.inputBox.CaretColor = System.Drawing.Color.White;
            this.inputBox.ChangedLineColor = System.Drawing.Color.Purple;
            this.inputBox.CharHeight = 13;
            this.inputBox.CharWidth = 7;
            this.inputBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.inputBox.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.inputBox.Font = new System.Drawing.Font("Courier New", 9F);
            this.inputBox.ForeColor = System.Drawing.Color.White;
            this.inputBox.IndentBackColor = System.Drawing.Color.Black;
            this.inputBox.IsReplaceMode = false;
            this.inputBox.Language = FastColoredTextBoxNS.Language.CSharp;
            this.inputBox.LineNumberColor = System.Drawing.Color.White;
            this.inputBox.Location = new System.Drawing.Point(3, 3);
            this.inputBox.Name = "inputBox";
            this.inputBox.Paddings = new System.Windows.Forms.Padding(0);
            this.inputBox.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.inputBox.Size = new System.Drawing.Size(446, 138);
            this.inputBox.TabIndex = 3;
            this.inputBox.TextAreaBorderColor = System.Drawing.Color.White;
            this.inputBox.Zoom = 100;
            this.inputBox.TextChanged += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(this.inputBox_TextChanged);
            this.inputBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.inputBox_KeyDown);
            // 
            // execButt
            // 
            this.execButt.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.execButt.Location = new System.Drawing.Point(3, 147);
            this.execButt.Name = "execButt";
            this.execButt.Size = new System.Drawing.Size(75, 23);
            this.execButt.TabIndex = 4;
            this.execButt.Text = "Execute";
            this.execButt.UseVisualStyleBackColor = true;
            this.execButt.Click += new System.EventHandler(this.execButt_Click);
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(246, 152);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(83, 13);
            this.statusLabel.TabIndex = 5;
            this.statusLabel.Text = "Status: Not Built";
            // 
            // buildButt
            // 
            this.buildButt.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buildButt.Location = new System.Drawing.Point(84, 147);
            this.buildButt.Name = "buildButt";
            this.buildButt.Size = new System.Drawing.Size(75, 23);
            this.buildButt.TabIndex = 6;
            this.buildButt.Text = "Build";
            this.buildButt.UseVisualStyleBackColor = true;
            this.buildButt.Click += new System.EventHandler(this.buildButt_Click);
            // 
            // InputHandler
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buildButt);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.execButt);
            this.Controls.Add(this.inputBox);
            this.Controls.Add(this.compileButt);
            this.Name = "InputHandler";
            this.Size = new System.Drawing.Size(452, 174);
            this.Load += new System.EventHandler(this.InputHandler_Load);
            this.Resize += new System.EventHandler(this.InputHandler_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.inputBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button execButt;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Button buildButt;
        public System.Windows.Forms.Button compileButt;
        public FastColoredTextBoxNS.FastColoredTextBox inputBox;

    }
}
