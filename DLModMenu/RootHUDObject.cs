using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.ObjectModel;

namespace DLModMenu
{
    public partial class RootHUDObject : UserControl
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

        public RootHUDObject()
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

        [Description("Customizable name of the object"), Category("HUD Object Properties"), BrowsableAttribute(true)]
        public string hudCustomName
        {
            get { return obj.CustomName; }
            set
            {
                int isExisting = 0;
                foreach (TreeNode t in obj.Node.Parent.Nodes)
                    if (t.Text == value)
                        isExisting++;

                if (isExisting == 0)
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

        [Description("For interaction between other HUD objects"), Category("HUD Object Properties"), BrowsableAttribute(true)]
        public int hudDataValue
        {
            get { return obj.DataValue; }
            set
            {
                int oldVal = _hudObject.DataValue;
                _hudObject.DataValue = value;

                inputHandler1.RaiseEvent("hudDataValue_Changed", new object[] { oldVal, _hudObject.DataValue });
            }
        }

        [Description("List of all roots to be displayed from this root. All non roots that are children of this root will be displayed."), Category("Root Interactive"), BrowsableAttribute(true)]
        public string[] hudSubRoots
        {
            get
            {
                return _hudObject.subItems;
            }
            set
            {
                _hudObject.subItems = value;
                ctlMain.Instance.SaveHUD(obj, obj.Node);
            }
        }

        [Description("The displayed root indexed from the SubRoots property"), Category("Root Interactive"), BrowsableAttribute(true)]
        public int hudSelectedRootIndex
        {
            get
            {
                return _hudObject.CurrentRootIndex;
            }
            set
            {
                _hudObject.CurrentRootIndex = value;

                if (_hudObject.subItems.Length == 0)
                    _hudObject.CurrentRootIndex = -1;
                if (_hudObject.CurrentRootIndex < -1)
                    _hudObject.CurrentRootIndex = -1;
                else if (_hudObject.CurrentRootIndex >= _hudObject.subItems.Length)
                    _hudObject.CurrentRootIndex = _hudObject.subItems.Length - 1;

                ctlMain.Instance.SaveHUD(obj, obj.Node);
            }
        }

        [Description("Where the HUD object is located in the memory"), Category("HUD Managed"), BrowsableAttribute(true)]
        public string hudAddress
        {
            get { return obj.Address.ToString("X8"); }
            //set { _hudObject.Address = value; }
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

        [Description("Where the root is on the screen horizontally. 1 being the right, and 0 being the left"), Category("HUD Appearance"), BrowsableAttribute(true)]
        public float hudXOff
        {
            get { return obj.XOff; }
            set
            {
                _hudObject.XOff = value;
                ctlMain.Instance.SaveHUD(obj, obj.Node);
            }
        }

        [Description("Where the root is on the screen vertically. 1 being the bottom, and 0 being the top"), Category("HUD Appearance"), BrowsableAttribute(true)]
        public float hudYOff
        {
            get { return obj.YOff; }
            set
            {
                _hudObject.YOff = value;
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

        #endregion

    }
}
