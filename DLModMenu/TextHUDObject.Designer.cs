namespace DLModMenu
{
    partial class TextHUDObject
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextHUDObject));
            this.propGrid = new Azuria.Common.Controls.FilteredPropertyGrid.FilteredPropertyGrid();
            this.inputHandler1 = new DLModMenu.InputHandler();
            this.SuspendLayout();
            // 
            // propGrid
            // 
            this.propGrid.BrowsableProperties = new string[] {
        "hudAddress",
        "hudText",
        "hudName",
        "hudType",
        "hudObjectID",
        "hudAlignment",
        "hudXOff",
        "hudYOff",
        "hudSize",
        "hudShadowHorOff",
        "hudShadowVerOff",
        "hudVisibility",
        "hudTextABGR",
        "hudShadowABGR",
        "hudTextBold",
        "hudCustomName",
        "hudDrawIndex",
        "hudDataValue",
        "hudEnabled"};
            this.propGrid.HiddenAttributes = null;
            this.propGrid.HiddenProperties = null;
            this.propGrid.Location = new System.Drawing.Point(507, 4);
            this.propGrid.Name = "propGrid";
            this.propGrid.SelectedObject = this;
            this.propGrid.Size = new System.Drawing.Size(220, 420);
            this.propGrid.TabIndex = 0;
            this.propGrid.ToolbarVisible = false;
            // 
            // inputHandler1
            // 
            this.inputHandler1.Location = new System.Drawing.Point(3, 3);
            this.inputHandler1.Name = "inputHandler1";
            this.inputHandler1.Size = new System.Drawing.Size(498, 420);
            this.inputHandler1.TabIndex = 1;
            // 
            // TextHUDObject
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.inputHandler1);
            this.Controls.Add(this.propGrid);
            this.Name = "TextHUDObject";
            this.Size = new System.Drawing.Size(730, 427);
            this.ResumeLayout(false);

        }

        #endregion

        private Azuria.Common.Controls.FilteredPropertyGrid.FilteredPropertyGrid propGrid;
        private InputHandler inputHandler1;



    }
}
