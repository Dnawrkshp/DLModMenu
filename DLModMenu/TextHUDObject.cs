using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Azuria.Common.Controls.FilteredPropertyGrid;
using dhx;

namespace DLModMenu
{
    public partial class TextHUDObject : UserControl
    {
        private ctlMain.HUDObject _hudObject = new ctlMain.HUDObject();
        public ctlMain.HUDObject obj
        {
            get { return _hudObject; }
            set
            {
                _hudObject = value;
                PopulateControl();
                propGrid.Refresh();
                //inputHandler1.tag = _hudObject.Name;
                inputHandler1.hudObject = obj;
            }
        }

        public InputHandler inputH
        {
            get { return inputHandler1; }
        }

        public Color Fore
        {
            get { return propGrid.ViewForeColor; }
            set
            {
                propGrid.ViewForeColor = value;
                propGrid.CategoryForeColor = value;
                propGrid.HelpForeColor = value;
                propGrid.CommandsForeColor = value;
                ForeColor = value;
            }
        }

        public Color Back
        {
            get { return propGrid.ViewBackColor; }
            set
            {
                propGrid.ViewBackColor = value;
                propGrid.BackColor = value;
                propGrid.HelpBackColor = value;
                propGrid.CommandsBackColor = value;
                propGrid.LineColor = value;
                BackColor = value;
            }
        }

        void PopulateControl()
        {
            //propGrid.BrowsableProperties = new string[] { "HUD Text Properties", "HUD Appearance" };
        }

        public TextHUDObject()
        {
            InitializeComponent();
            inputHandler1.compileButt.Click += new EventHandler(SaveClickHander);
        }

        void SaveClickHander(object sender, EventArgs e)
        {
            _hudObject.InputCode = inputHandler1.inputBox.Text;
        }

        public void ResizeControls(Size size)
        {
            inputHandler1.Height = size.Height;
            inputHandler1.Width = size.Width - 230;
            inputHandler1.Location = new Point(5, 5);

            propGrid.Height = size.Height - 10;
            propGrid.Width = 220;
            propGrid.Location = new Point(size.Width - propGrid.Width - 5, 5);

            Size = size;
        }

        #region HUD Properties

        [Description("If true, the object and its children will be visible and executable."), Category("HUD Object Properties"), BrowsableAttribute(true)]
        public bool hudEnabled
        {
            get { return obj.Enabled; }
            set
            {
                _hudObject.Enabled = value;
                ctlMain.Instance.SaveHUD(obj, obj.Node);
            }
        }

        [Description("For interaction between other HUD objects"), Category("HUD Object Properties"), BrowsableAttribute(true)]
        public int hudDataValue
        {
            get { return obj.DataValue; }
            set
            {
                int oldVal = _hudObject.DataValue;
                _hudObject.DataValue = value;

                inputHandler1.RaiseEvent("hudDataValue_Changed", new object [] { oldVal, _hudObject.DataValue });
            }
        }

        [Description("Customizable name of the object"), Category("HUD Object Properties"), BrowsableAttribute(true)]
        public string hudCustomName
        {
            get { return obj.CustomName; }
            set
            {
                bool isExisting = ctlMain.FindObjectWithName(value).Type != null;

                if (!isExisting)
                {
                    _hudObject.CustomName = value;
                    obj.Node.Text = value;
                    inputHandler1.hCustomName = value;
                }
            }
        }

        [Description("Index of drawing. 0 is topmost."), Category("HUD Object Properties"), BrowsableAttribute(true)]
        public int hudDrawIndex
        {
            get
            {
                if (obj.Name == "Main Root")
                    return 0;

                int val = ctlMain.FindTNodeWithTagFromTop(obj.Name).Index;
                return val;
            }
            set
            {
                if (obj.Name == "Main Root")
                    return;

                TreeNode sourceNode = ctlMain.FindTNodeWithTagFromTop(obj.Name);
                if (sourceNode == null || sourceNode.Parent == null)
                    return;

                int val = value;
                if (val < 0)
                    val = 0;
                if (val >= sourceNode.Parent.Nodes.Count)
                    val = sourceNode.Parent.Nodes.Count - 1;

                TreeNode newNode = (TreeNode)sourceNode.Clone();
                TreeNode parent = sourceNode.Parent;
                sourceNode.Parent.Nodes.Remove(sourceNode);
                parent.Nodes.Insert(val, newNode);
                ctlMain.Instance.modMenuLayout.SelectedNode = newNode;

                ctlMain.Instance.SaveHUD(ctlMain.FindObjectWithTag(parent.Tag.ToString()), parent);
            }
        }

        [Description("Where the HUD object is located in the memory"), Category("HUD Managed"), BrowsableAttribute(true)]
        public string hudAddress
        {
            get { return obj.Address.ToString("X8"); }
            //set { _hudObject.Address = value; }
        }

        [Description("Printed text"), Category("HUD Text Properties"), BrowsableAttribute(true)]
        public string hudText
        {
            get { return obj.Text; }
            set
            {
                _hudObject.Text = value;
                ctlMain.Instance.SaveHUD(obj, obj.Node);
            }
        }

        [Description("Original Name (program, not PS3)"), Category("HUD Managed"), BrowsableAttribute(true)]
        public string hudName
        {
            get { return obj.Name; }
            //set { _hudObject.Name = value; }
        }

        [Description("Type (program, not PS3)"), Category("HUD Managed"), BrowsableAttribute(true)]
        public string hudType
        {
            get { return obj.Type; }
            //set { _hudObject.Type = value; }
        }

        [Description("Type ID of the object"), Category("HUD Managed"), BrowsableAttribute(true)]
        public string hudObjectID
        {
            get
            {
                return obj.ObjectID.ToString("X8");
            }
            //set { _hudObject.ObjectID = value; }
        }

        [Description("Whether the text is left, center, or right aligned"), Category("HUD Text Properties"), BrowsableAttribute(true)]
        public HorizontalAlignment hudAlignment
        {
            get
            {
                int val = (int)((obj.Alignment << 20) >> 28);
                switch (val)
                {
                    case 2:
                        return HorizontalAlignment.Left;
                    case 4:
                        return HorizontalAlignment.Right;
                    case 6:
                        return HorizontalAlignment.Center;
                    default:
                        return HorizontalAlignment.Left;
                }
            }
            set
            {
                uint align = 2;
                if (value == System.Windows.Forms.HorizontalAlignment.Right)
                    align = 4;
                else
                    align = 6;

                uint val = (uint)0x00011047 + (align << 8);
                _hudObject.Alignment = val;
                ctlMain.Instance.SaveHUD(obj, obj.Node);
            }
        }

        [Description("Where the text is on the screen horizontally. 1 being the right, and 0 being the left"), Category("HUD Appearance"), BrowsableAttribute(true)]
        public float hudXOff
        {
            get { return obj.XOff; }
            set
            {
                _hudObject.XOff = value;
                ctlMain.Instance.SaveHUD(obj, obj.Node);
            }
        }

        [Description("Where the text is on the screen vertically. 1 being the bottom, and 0 being the top"), Category("HUD Appearance"), BrowsableAttribute(true)]
        public float hudYOff
        {
            get { return obj.YOff; }
            set
            {
                _hudObject.YOff = value;
                ctlMain.Instance.SaveHUD(obj, obj.Node);
            }
        }

        [Description("How large the text is"), Category("HUD Text Properties"), BrowsableAttribute(true)]
        public float hudSize
        {
            get { return obj.Size; }
            set
            {
                _hudObject.Size = value;
                ctlMain.Instance.SaveHUD(obj, obj.Node);
            }
        }

        [Description("How far horizontally the shadow is from the text"), Category("HUD Appearance"), BrowsableAttribute(true)]
        public float hudShadowHorOff
        {
            get { return obj.ShadowHorOff; }
            set
            {
                _hudObject.ShadowHorOff = value;
                ctlMain.Instance.SaveHUD(obj, obj.Node);
            }
        }

        [Description("How far vertically the shadow is from the text"), Category("HUD Appearance"), BrowsableAttribute(true)]
        public float hudShadowVerOff
        {
            get { return obj.ShadowVerOff; }
            set
            {
                _hudObject.ShadowVerOff = value;
                ctlMain.Instance.SaveHUD(obj, obj.Node);
            }
        }

        [Description("How visible the text is. The value should be set between 0 and 1"), Category("HUD Appearance"), BrowsableAttribute(true)]
        public float hudVisibility
        {
            get { return obj.Visibility; }
            set
            {
                _hudObject.Visibility = value;
                if (_hudObject.Visibility < 0f)
                    _hudObject.Visibility = 0f;
                else if (_hudObject.Visibility > 1f)
                    _hudObject.Visibility = 1f;
                ctlMain.Instance.SaveHUD(obj, obj.Node);
            }
        }

        [Description("Color of the text"), Category("HUD Text Properties"), BrowsableAttribute(true)]
        public Color hudTextABGR
        {
            get
            {
                if (obj.TextABGR == null)
                    return Color.White;
                return Color.FromArgb(obj.TextABGR[0], obj.TextABGR[3], obj.TextABGR[2], obj.TextABGR[1]);
            }
            set
            {
                _hudObject.TextABGR = new byte[] { value.A, value.B, value.G, value.R };
                ctlMain.Instance.SaveHUD(obj, obj.Node);
            }
        }

        [Description("Color of the shadow"), Category("HUD Appearance"), BrowsableAttribute(true)]
        public Color hudShadowABGR
        {
            get
            {
                if (obj.ShadowABGR == null)
                    return Color.Black;
                return Color.FromArgb(obj.ShadowABGR[0], obj.ShadowABGR[3], obj.ShadowABGR[2], obj.ShadowABGR[1]);
            }
            set
            {
                _hudObject.ShadowABGR = new byte[] { value.A, value.B, value.G, value.R };
                ctlMain.Instance.SaveHUD(obj, obj.Node);
            }
        }

        [Description("Whether the text is mono spaced or not"), Category("HUD Text Properties"), BrowsableAttribute(true)]
        public bool hudMonoSpaced
        {
            get { return obj.Spacing == 0x01000000; }
            set
            {
                uint val = (uint)(value ? 0x01000000 : 0x02000000);
                _hudObject.Spacing = val;
                ctlMain.Instance.SaveHUD(obj, obj.Node);
            }
        }

        [Description("Whether the text is bold or not"), Category("HUD Text Properties"), BrowsableAttribute(true)]
        public bool hudTextBold
        {
            get { return obj.TextBold; }
            set
            {
                _hudObject.TextBold = value;
                ctlMain.Instance.SaveHUD(obj, obj.Node);
            }
        }

        #endregion

    }
}
