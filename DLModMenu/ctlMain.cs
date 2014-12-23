using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using System.Threading;
using PluginInterface;

namespace DLModMenu
{
    public partial class ctlMain : UserControl
    {

        #region Declarations

        ManualResetEvent gate = new ManualResetEvent(false);

        public static CompiledCodeHandler cch = new CompiledCodeHandler();

        static public string AssemblyDirectory
        {
            get
            {
                string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        TreeNode copiedNode;
        HUDObject copiedHObj;

        static int globalXInc = 0;
        static ulong globalAddr = 0x00B04000;
        bool hasInitBeenWritten = false;
        ulong rootAddress = 0x00B04000;
        //TreeNode globalRootNode;

        [Serializable]
        public struct HUDObject
        {
            public int DataValue;

            public ulong Address;

            public string InputCode;
            public string Text;
            public string Name;
            public string Type;
            public string CustomName;
            public bool Enabled;

            public TreeNode Node
            {
                get
                {
                    return FindTNodeWithTagFromTop(Name);
                }
            }

            /* Box */
            public float rotation; //PC: Degrees, PS3: Radians
            public byte[] TRCornerColor;
            public byte[] BLCornerColor;
            public byte[] BRCornerColor;

            /* Root */
            public string[] subItems;
            public int CurrentRootIndex;
            public float Width;
            public float Height;
            public int DisplayedIndex;

            /* Text */
            public uint ObjectID;
            //4 - 00000001
            //8
            public uint Alignment;
            //10
            public float XOff;
            public float YOff;
            public float Size;
            //20 - 3F6364C3
            public float ShadowHorOff;
            public float ShadowVerOff;
            public float Visibility;
            public byte[] TextABGR;
            public byte[] ShadowABGR;
            //38
            //3C
            //40
            //44
            //48
            //4C -- Point to string (defined by program)
            //50
            public uint Spacing;
            //58
            //5C - 3F800000
            //60 - 10000000
            public bool TextBold;
            //68 - 3F800000
            //6C - 3F800000
        };

        public static IPluginHost NCInterface;

        public static List<TextHUDObject> textList = new List<TextHUDObject>();
        public static List<RootHUDObject> rootList = new List<RootHUDObject>();
        public static List<BoxHUDObject> boxList = new List<BoxHUDObject>();

        public static ctlMain Instance;

        #endregion

        public ctlMain()
        {
            InitializeComponent();

            Instance = this;

            this.Disposed += new EventHandler(handleDisposeEvent);
        }

        private void handleDisposeEvent(object sender, EventArgs e)
        {
            if (tRunCodeLoop != null && tRunCodeLoop.ThreadState == ThreadState.Running)
                tRunCodeLoop.Abort();
        }

        #region Thread RunCode() Loop

        Thread tRunCodeLoop;

        void ThreadRunCodeLoop()
        {
            try
            {
                while (tRunCodeLoop.ThreadState == ThreadState.Running || tRunCodeLoop.ThreadState == ThreadState.Background)
                {
                    gate.WaitOne();

                    if (Instance != null)
                    {
                        TreeNode top = null;
                        Invoke((MethodInvoker)delegate
                        {
                            top = Instance.modMenuLayout.TopNode;

                            while (top != null) 
                                top = ExecuteNode(top);
                        });
                    }

                    System.Threading.Thread.Sleep(10);
                }
            }
            catch (Exception e)
            {
                if (e.Message != "Invoke or BeginInvoke cannot be called on a control until the window handle has been created.")
                {
                    Invoke((MethodInvoker)delegate
                    {
                        if (isThreadRunning.Checked)
                            isThreadRunning.Checked = false;
                    });
                }
            }
        }

        TreeNode ExecuteNode(TreeNode node)
        {
            if (node == null || node.Nodes == null)
                return null;

            RootHUDObject r = null;
            Invoke((MethodInvoker)delegate
            {
                r = (RootHUDObject)FindHUDObjectUControlFromTag(node.Tag.ToString());
            });

            if (r == null)
                return null;

            if (!r.obj.Enabled)
            {
                r.inputH.RunCode(true);
                return null;
            }

            if (r.inputH != null)
                r.inputH.RunCode(true);

            for (int x = 0; x < node.Nodes.Count; x++)
            {
                if (node.Nodes[x].Tag.ToString().IndexOf("Root") < 0)
                {
                    Invoke((MethodInvoker)delegate
                    {
                        object a = Instance.FindHUDObjectUControlFromTag(node.Nodes[x].Tag.ToString());
                        if (a != null)
                        {
                            if (node.Nodes[x].Tag.ToString().IndexOf("Text") >= 0)
                            {
                                if ((a as TextHUDObject).inputH != null && (a as TextHUDObject).obj.Enabled)
                                    (a as TextHUDObject).inputH.RunCode(true);
                            }
                            else if (node.Nodes[x].Tag.ToString().IndexOf("Box") >= 0)
                            {
                                if ((a as BoxHUDObject).inputH != null && (a as BoxHUDObject).obj.Enabled)
                                    (a as BoxHUDObject).inputH.RunCode(true);
                            }
                        }
                    });
                }
            }

            int ind = r.hudSelectedRootIndex;
            if (ind < 0)
                return null;
            else
                return Instance.FindTNodeWithNameFromTopInvoke(r.hudSubRoots[ind]);
        }

        #endregion

        #region Save and Load Tree View

        /*
         * http://stackoverflow.com/questions/5868790/saving-content-of-a-treeview-to-a-file-and-load-it-later
         */

        public static void SaveTree(TreeView tree, string filename)
        {
            using (Stream file = File.Open(filename, FileMode.Create))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(file, tree.Nodes.Cast<TreeNode>().ToList());
            }
        }

        public static void SaveModMenu(string filename)
        {
            List<string> tempFile = new List<string>();

            tempFile.Add(Path.Combine(Path.GetTempPath(), "treeview"));
            SaveTree(Instance.modMenuLayout, tempFile[0]);

            if (rootList.Count > 0)
            {
                tempFile.Add(Path.Combine(Path.GetTempPath(), "root"));
                string store = "";
                foreach (RootHUDObject r in rootList)
                {
                    if (r.obj.Type != null)
                        store += HUDObjectToString(r.obj) + (char)5;
                }
                File.WriteAllText(tempFile[tempFile.Count - 1], store);
            }

            if (textList.Count > 0)
            {
                tempFile.Add(Path.Combine(Path.GetTempPath(), "text"));
                string store = "";
                foreach (TextHUDObject r in textList)
                {
                    if (r.obj.Type != null)
                        store += HUDObjectToString(r.obj) + (char)5;
                }
                File.WriteAllText(tempFile[tempFile.Count - 1], store);
            }


            if (boxList.Count > 0)
            {
                tempFile.Add(Path.Combine(Path.GetTempPath(), "box"));
                string store = "";
                foreach (BoxHUDObject r in boxList)
                {
                    if (r.obj.Type != null)
                        store += HUDObjectToString(r.obj) + (char)5;
                }
                File.WriteAllText(tempFile[tempFile.Count - 1], store);
            }

            foreach (string path in tempFile)
            {
                ZipClass.AddFileToZip(filename, path);
                File.Delete(path);
            }
        }

        public static void LoadTree(TreeView tree, string filename)
        {
            using (Stream file = File.Open(filename, FileMode.Open))
            {
                BinaryFormatter bf = new BinaryFormatter();
                object obj = bf.Deserialize(file);

                TreeNode[] nodeList = (obj as IEnumerable<TreeNode>).ToArray();
                tree.Nodes.AddRange(nodeList);
            }
        }

        uint getByteVal(byte[] array, int ind)
        {
            if (ind + 4 >= array.Length)
                return 0;

            return (uint)(array[ind] << 24) + (uint)(array[ind + 1] << 16) + (uint)(array[ind + 2] << 8) + (uint)array[ind + 3];
        }

        public static void LoadModMenu(string filename)
        {
            Instance.modMenuLayout.Nodes.Clear();
            rootList.Clear();
            boxList.Clear();
            textList.Clear();

            globalAddr = 0x00B04200;
            globalXInc = 0;

            string[] huds = null;
            List<string> paths = ZipClass.ExtractZIP(filename, Path.GetTempPath());

            foreach (string tree in paths)
            {
                if (tree.IndexOf("treeview") >= 0)
                {
                    FileInfo fi = new FileInfo(tree);
                    LoadTree(Instance.modMenuLayout, fi.FullName);
                }
            }


            foreach (string path in paths)
            {
                FileInfo fi = new FileInfo(path);

                switch (fi.Name)
                {
                    case "treeview":
                        //LoadTree(Instance.modMenuLayout, fi.FullName);
                        break;
                    case "root":
                        huds = File.ReadAllText(fi.FullName).Split((char)5);

                        foreach (string line in huds)
                        {
                            HUDObject h = HUDObjectFromString(line);

                            if (h.Name == "Main Root")
                                h.Address = 0x00B04000;
                            else
                            {
                                h.Address = globalAddr;
                                globalAddr += 0x200;
                            }
                            RootHUDObject r = new RootHUDObject();
                            r.obj = h;
                            rootList.Add(r);
                            globalXInc++;
                        }
                        break;
                    case "text":
                        huds = File.ReadAllText(fi.FullName).Split((char)5);

                        foreach (string line in huds)
                        {
                            HUDObject h = HUDObjectFromString(line);
                            h.Address = globalAddr;
                            globalAddr += 0x100;
                            TextHUDObject r = new TextHUDObject();
                            r.obj = h;
                            textList.Add(r);
                            Instance.SaveHUD(h, h.Node);
                            globalXInc++;
                        }
                        break;
                    case "box":
                        huds = File.ReadAllText(fi.FullName).Split((char)5);

                        foreach (string line in huds)
                        {
                            HUDObject h = HUDObjectFromString(line);
                            h.Address = globalAddr;
                            globalAddr += 0x100;
                            BoxHUDObject r = new BoxHUDObject();
                            r.obj = h;
                            boxList.Add(r);
                            Instance.SaveHUD(h, h.Node);
                            globalXInc++;
                        }
                        break;
                }
                File.Delete(path);
            }

            foreach (RootHUDObject r in rootList)
                Instance.SaveHUD(r.obj, r.obj.Node);
        }

        public static string HUDObjectToString(HUDObject h)
        {
            string ret = "";

            if (h.Type == null)
                return null;

            ret += h.Address.ToString() + (char)1;
            ret += h.Alignment.ToString() + (char)1;
            ret += ((h.BLCornerColor == null) ? Convert.ToBase64String(new byte[4]) : Convert.ToBase64String(h.BLCornerColor)) + (char)1;
            ret += ((h.BRCornerColor == null) ? Convert.ToBase64String(new byte[4]) : Convert.ToBase64String(h.BRCornerColor)) + (char)1;
            ret += h.CurrentRootIndex.ToString() + (char)1;
            ret += (h.CustomName == null) ? (h.Type + globalXInc.ToString()) : h.CustomName.ToString() + (char)1;
            ret += h.DataValue.ToString() + (char)1;
            ret += h.DisplayedIndex.ToString() + (char)1;
            ret += h.Height.ToString() + (char)1;
            ret += (h.Name == null) ? "Main Root" : h.Name.ToString() + (char)1;
            ret += h.ObjectID.ToString() + (char)1;
            ret += h.rotation.ToString() + (char)1;
            ret += ((h.ShadowABGR == null) ? Convert.ToBase64String(new byte[4]) : Convert.ToBase64String(h.ShadowABGR)) + (char)1;
            ret += h.ShadowHorOff.ToString() + (char)1;
            ret += h.ShadowVerOff.ToString() + (char)1;
            ret += h.Size.ToString() + (char)1;
            ret += h.Spacing.ToString() + (char)1;
            ret += ((h.subItems == null) ? "" : String.Join(((char)2).ToString(), h.subItems)) + (char)1;
            ret += ((h.Text == null) ? "" : h.Text.ToString()) + (char)1;
            ret += ((h.TextABGR == null) ? Convert.ToBase64String(new byte[4]) : Convert.ToBase64String(h.TextABGR)) + (char)1;
            ret += h.TextBold.ToString() + (char)1;
            ret += ((h.TRCornerColor == null) ? Convert.ToBase64String(new byte[4]) : Convert.ToBase64String(h.TRCornerColor)) + (char)1;
            ret += h.Type.ToString() + (char)1;
            ret += h.Visibility.ToString() + (char)1;
            ret += h.Width.ToString() + (char)1;
            ret += h.XOff.ToString() + (char)1;
            ret += h.YOff.ToString() + (char)1;
            ret += ((h.InputCode == null) ? "" : h.InputCode.Replace("\r\n", ((char)2).ToString())) + (char)1;
            ret += h.Enabled.ToString() + (char)1;

            return ret;
        }

        public static HUDObject HUDObjectFromString(string h)
        {
            HUDObject ret = new HUDObject();

            if (h == null || h == "")
                return ret;

            string[] items = h.Split((char)1);

            try
            {

                ret.Address = ulong.Parse(items[0]);
                ret.Alignment = uint.Parse(items[1]);
                ret.BLCornerColor = Convert.FromBase64String(items[2]);
                ret.BRCornerColor = Convert.FromBase64String(items[3]);
                ret.CurrentRootIndex = int.Parse(items[4]);
                ret.CustomName = items[5];
                ret.DataValue = int.Parse(items[6]);
                ret.DisplayedIndex = int.Parse(items[7]);
                ret.Height = float.Parse(items[8]);
                ret.Name = items[9];
                ret.ObjectID = uint.Parse(items[10]);
                ret.rotation = float.Parse(items[11]);
                ret.ShadowABGR = Convert.FromBase64String(items[12]);
                ret.ShadowHorOff = float.Parse(items[13]);
                ret.ShadowVerOff = float.Parse(items[14]);
                ret.Size = float.Parse(items[15]);
                ret.Spacing = uint.Parse(items[16]);
                ret.subItems = items[17].Split((char)2);
                ret.Text = items[18];
                ret.TextABGR = Convert.FromBase64String(items[19]);
                ret.TextBold = bool.Parse(items[20]);
                ret.TRCornerColor = Convert.FromBase64String(items[21]);
                ret.Type = items[22];
                ret.Visibility = float.Parse(items[23]);
                ret.Width = float.Parse(items[24]);
                ret.XOff = float.Parse(items[25]);
                ret.YOff = float.Parse(items[26]);
                ret.InputCode = items[27].Replace(((char)2).ToString(), "\r\n");
                ret.Enabled = bool.Parse(items[28]);
            }
            catch (Exception)
            {

            }

            return ret;
        }

        #endregion

        #region Context Menu Strip (Tree View)

        private void addSelectorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!hasInitBeenWritten)
                WriteInitial();

            TreeView obj = modMenuLayout;

            HUDObject hudo = generateBoxHUD();
            hudo.Height = 0.03f;
            SaveHUD(hudo, null);

            TreeNode tn = new TreeNode("BoxSel" + globalXInc.ToString());
            tn.Tag = tn.Text;
            hudo.Name = tn.Text;
            hudo.CustomName = tn.Text;
            hudo.InputCode = generateSelector(hudo.Name);

            TreeNode addTo = obj.SelectedNode;
            if (addTo.Tag.ToString().IndexOf("Root") < 0)
                addTo = addTo.Parent;
            addTo.Nodes.Add(tn);

            BoxHUDObject t = new BoxHUDObject();
            t.obj = hudo;
            boxList.Add(t);

            if (!obj.SelectedNode.IsExpanded)
                obj.SelectedNode.Expand();

            globalXInc++;

            if (tn.Parent != null)
            {
                SaveHUD(FindObjectWithTag(tn.Parent.Tag.ToString()), tn.Parent);
            }
        }

        private void addColoredBoolTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!hasInitBeenWritten)
                WriteInitial();

            TreeView obj = modMenuLayout;


            HUDObject hudo = generateTextHUD("Hello World");
            TreeNode tn = new TreeNode("CBoolText" + globalXInc.ToString());
            tn.Tag = tn.Text;
            hudo.Name = tn.Text;
            hudo.CustomName = tn.Text;
            hudo.InputCode = generateColoredBoolText(hudo.Name);

            TreeNode addTo = obj.SelectedNode;
            if (addTo.Tag.ToString().IndexOf("Root") < 0)
                addTo = addTo.Parent;
            addTo.Nodes.Add(tn);

            TextHUDObject t = new TextHUDObject();
            t.obj = hudo;
            textList.Add(t);

            if (!obj.SelectedNode.IsExpanded)
                obj.SelectedNode.Expand();

            globalXInc++;

            if (tn.Parent != null)
            {
                SaveHUD(FindObjectWithTag(tn.Parent.Tag.ToString()), tn.Parent);
            }
        }

        private void addRootToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!hasInitBeenWritten)
                WriteInitial();

            TreeView obj = modMenuLayout;


            HUDObject hudo = generateRootHUD();
            TreeNode tn = new TreeNode("Root" + globalXInc.ToString());
            tn.Tag = tn.Text;
            //hudo.Node = tn;
            hudo.Name = tn.Text;
            hudo.CustomName = tn.Text;
            hudo.InputCode = generateRootInput(hudo.Name);

            TreeNode addTo = obj.SelectedNode;
            if (addTo.Tag.ToString().IndexOf("Root") < 0)
                addTo = addTo.Parent;
            addTo.Nodes.Add(tn);

            RootHUDObject t = new RootHUDObject();
            t.obj = hudo;
            rootList.Add(t);

            if (!obj.SelectedNode.IsExpanded)
                obj.SelectedNode.Expand();

            globalXInc++;

            if (tn.Parent != null)
            {
                SaveHUD(FindObjectWithTag(tn.Parent.Tag.ToString()), tn.Parent);
            }
        }

        private void addTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!hasInitBeenWritten)
                WriteInitial();

            TreeView obj = modMenuLayout;


            HUDObject hudo = generateTextHUD("Hello World");
            hudo.Type = "Text";

            TreeNode tn = new TreeNode("Text" + globalXInc.ToString());
            tn.Tag = tn.Text;
            hudo.Name = tn.Text;
            hudo.CustomName = tn.Text;
            hudo.InputCode = generateGenericInput(hudo.Name);

            TreeNode addTo = obj.SelectedNode;
            if (addTo.Tag.ToString().IndexOf("Root") < 0)
                addTo = addTo.Parent;
            addTo.Nodes.Add(tn);

            TextHUDObject t = new TextHUDObject();
            t.obj = hudo;
            textList.Add(t);

            if (!obj.SelectedNode.IsExpanded)
                obj.SelectedNode.Expand();

            globalXInc++;

            if (tn.Parent != null)
            {
                SaveHUD(FindObjectWithTag(tn.Parent.Tag.ToString()), tn.Parent);
            }
        }

        private void addBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!hasInitBeenWritten)
                WriteInitial();

            TreeView obj = modMenuLayout;

            HUDObject hudo = generateBoxHUD();

            TreeNode tn = new TreeNode("Box" + globalXInc.ToString());
            tn.Tag = tn.Text;
            hudo.Name = tn.Text;
            hudo.CustomName = tn.Text;
            hudo.InputCode = generateGenericInput(hudo.Name);

            TreeNode addTo = obj.SelectedNode;
            if (addTo.Tag.ToString().IndexOf("Root") < 0)
                addTo = addTo.Parent;
            addTo.Nodes.Add(tn);

            BoxHUDObject t = new BoxHUDObject();
            t.obj = hudo;
            boxList.Add(t);

            if (!obj.SelectedNode.IsExpanded)
                obj.SelectedNode.Expand();

            globalXInc++;

            if (tn.Parent != null)
            {
                SaveHUD(FindObjectWithTag(tn.Parent.Tag.ToString()), tn.Parent);
            }
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!hasInitBeenWritten)
                WriteInitial();

            TreeView obj = modMenuLayout;
            if (FindObjectWithTag(obj.SelectedNode.Tag.ToString()).Name != "Main Root")
            {
                string name = obj.SelectedNode.Tag.ToString();
                RemoveHUDWithTag(name);

                TreeNode parent = obj.SelectedNode.Parent;
                obj.SelectedNode.Remove();
                SaveHUD(FindObjectWithTag(parent.Tag.ToString()), parent);
            }
        }

        private void copyNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            copiedNode = modMenuLayout.SelectedNode;
            copiedHObj = FindObjectWithTag(modMenuLayout.SelectedNode.Tag.ToString());
        }

        private void pasteNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (copiedNode != null && copiedHObj.Type != null)
            {
                TreeNode rootN = modMenuLayout.SelectedNode;
                if (rootN.Tag.ToString().IndexOf("Root") < 0)
                    rootN = rootN.Parent;

                TreeNode newNode = (TreeNode)copiedNode.Clone();
                HUDObject newHObj = HUDObjectFromString(HUDObjectToString(copiedHObj));
                newHObj.Name += "_" + globalXInc.ToString();
                globalXInc++;
                newHObj.Address = globalAddr;
                globalAddr += 0x100;
                if (newHObj.Type == "Root")
                    globalAddr += 0x100;

                newNode.Tag = newHObj.Name;
                newNode.Text = newHObj.Name;

                rootN.Nodes.Add(newNode);

                switch (newHObj.Type)
                {
                    case "Text":
                        TextHUDObject t = new TextHUDObject();
                        t.obj = newHObj;
                        textList.Add(t);
                        break;
                    case "Root":
                        RootHUDObject r = new RootHUDObject();
                        r.obj = newHObj;
                        rootList.Add(r);
                        break;
                    case "Box":
                        BoxHUDObject b = new BoxHUDObject();
                        b.obj = newHObj;
                        boxList.Add(b);
                        break;
                }
            }
        }

        #endregion

        #region Mod Menu Layout (Tree View) events

        private void isThreadRunning_CheckedChanged(object sender, EventArgs e)
        {
            if (tRunCodeLoop == null)
                tRunCodeLoop = new Thread(ThreadRunCodeLoop);

            if (isThreadRunning.Checked)
            {
                BuildAll();
                SelectAllNodesTemp(modMenuLayout.TopNode);
                //modMenuLayout.CollapseAll();
                //modMenuLayout.TopNode.Expand();
                modMenuLayout.SelectedNode = modMenuLayout.TopNode;

                if (tRunCodeLoop.ThreadState == ThreadState.Unstarted)
                    tRunCodeLoop.Start();
                gate.Set();
            }
            else
            {
                gate.Reset();
            }
        }

        private void ctlMain_Resize(object sender, EventArgs e)
        {
            modMenuLayout.Height = Height - 55;
            LoadMMenu.Top = Height - 50;
            SaveMMenu.Top = Height - 50;
            isThreadRunning.Top = Height - 25;
            controlsBox.Height = Height - 10;
            controlsBox.Width = Width - 167;

            if (modMenuLayout.SelectedNode != null)
            {
                if (modMenuLayout.SelectedNode.Tag.ToString().IndexOf("Text") >= 0)
                    ((TextHUDObject)ctlMain.FindHUDObjectUControlFromTagX(modMenuLayout.SelectedNode.Tag.ToString())).ResizeControls(controlsBox.Size);
                else if (modMenuLayout.SelectedNode.Tag.ToString().IndexOf("Box") >= 0)
                    ((BoxHUDObject)ctlMain.FindHUDObjectUControlFromTagX(modMenuLayout.SelectedNode.Tag.ToString())).ResizeControls(controlsBox.Size);
                else if (modMenuLayout.SelectedNode.Tag.ToString().IndexOf("Root") >= 0)
                    ((RootHUDObject)ctlMain.FindHUDObjectUControlFromTagX(modMenuLayout.SelectedNode.Tag.ToString())).ResizeControls(controlsBox.Size);
            }
        }

        private void ctlMain_Load(object sender, EventArgs e)
        {
            gate.Reset();
            tRunCodeLoop = new Thread(ThreadRunCodeLoop);
            tRunCodeLoop.IsBackground = true;
            tRunCodeLoop.Start();

            WriteInitial();
        }

        private void SaveMMenu_Click(object sender, EventArgs e)
        {
            SaveFileDialog fd = new SaveFileDialog();
            fd.Filter = "Mettle Mod Menu files (*.mettle)|*.mettle|All files (*.*)|*.*";
            fd.RestoreDirectory = true;

            if (fd.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(fd.FileName))
                    File.Delete(fd.FileName);
                SaveModMenu(fd.FileName);
                //SaveTree(modMenuLayout, fd.FileName);
            }
        }

        private void LoadMMenu_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "Mettle Mod Menu files (*.mettle)|*.mettle|All files (*.*)|*.*";
            fd.RestoreDirectory = true;

            if (fd.ShowDialog() == DialogResult.OK)
            {
                LoadModMenu(fd.FileName);
                modMenuLayout.Refresh();
                modMenuLayout.TopNode.Expand();

                //LoadTree(modMenuLayout, fd.FileName);
            }
        }

        private void SelectAllNodesTemp(TreeNode t)
        {
            if (t == null)
                return;

            foreach (TreeNode node in t.Nodes)
            {
                if (node != null)
                {
                    modMenuLayout.SelectedNode = node;
                    SelectAllNodesTemp(node);
                }
            }

            return;
        }

        private void modMenuLayout_MouseUp(object sender, MouseEventArgs e)
        {
            //Right click
            if (e.Button == MouseButtons.Right && NCInterface.ConnectionState() == 2)
            {
                if (copiedNode == null)
                    ((ToolStripMenuItem)contextMenuStrip1.Items[3]).DropDownItems[1].Enabled = false;
                else
                    ((ToolStripMenuItem)contextMenuStrip1.Items[3]).DropDownItems[1].Enabled = true;

                var loc = modMenuLayout.HitTest(e.Location);
                if (loc.Node != null)
                {
                    ((ToolStripMenuItem)contextMenuStrip1.Items[3]).DropDownItems[0].Enabled = true;
                    ((ToolStripMenuItem)contextMenuStrip1.Items[3]).DropDownItems[2].Enabled = true;

                    contextMenuStrip1.Show(modMenuLayout, e.Location);
                }
                else if (modMenuLayout.TopNode != null)
                {
                    ((ToolStripMenuItem)contextMenuStrip1.Items[3]).DropDownItems[0].Enabled = false;
                    ((ToolStripMenuItem)contextMenuStrip1.Items[3]).DropDownItems[2].Enabled = false;

                    modMenuLayout.SelectedNode = modMenuLayout.TopNode;
                    contextMenuStrip1.Show(modMenuLayout, e.Location);
                }

            }
        }

        private void modMenuLayout_AfterSelect(object sender, TreeViewEventArgs e)
        {
            controlsBox.Controls.Clear();

            HUDObject obj = FindObjectWithTag(modMenuLayout.SelectedNode.Tag.ToString());
            if (obj.Address != 0)
            {
                switch (obj.Type)
                {
                    case "Text":
                        TextHUDObject t = FindTHUDOWithTag(modMenuLayout.SelectedNode.Tag.ToString());

                        t.Back = BackColor;
                        t.Fore = ForeColor;
                        t.ResizeControls(controlsBox.Size);

                        controlsBox.Controls.Add(t);
                        break;
                    case "Box":
                        BoxHUDObject b = FindBHUDOWithTag(modMenuLayout.SelectedNode.Tag.ToString());

                        b.Back = BackColor;
                        b.Fore = ForeColor;
                        b.ResizeControls(controlsBox.Size);

                        controlsBox.Controls.Add(b);
                        break;
                    case "Root":
                        RootHUDObject r = FindRHUDOWithTag(modMenuLayout.SelectedNode.Tag.ToString());

                        r.Back = BackColor;
                        r.Fore = ForeColor;
                        r.ResizeControls(controlsBox.Size);

                        controlsBox.Controls.Add(r);
                        break;
                }
            }
        }

        #endregion

        #region HUD Saving and Generating

        HUDObject generateBoxHUD()
        {
            HUDObject ret = new HUDObject();

            //Set storage address
            ret.Address = globalAddr;
            globalAddr += 0x100;

            ret.Alignment = 0x00011E07;
            ret.ObjectID = 0x008B94E0; //(uint)nc.ByteAToULong(nc.GetMemory(0x008B9510, 4), 0, 4);
            ret.Width = 0.5f;
            ret.Height = 0.5f;
            ret.Size = 1f;
            ret.TextABGR = new byte[] { 0x3E, 0x00, 0x00, 0x00 };
            ret.BLCornerColor = new byte[4];
            ret.BRCornerColor = new byte[4];
            ret.TRCornerColor = new byte[4];
            ret.Type = "Box";
            ret.Visibility = 1f;
            ret.XOff = 0.5f;
            ret.YOff = 0.5f;
            ret.Enabled = true;

            SaveHUD(ret, null);

            //nc.SetMemory(rootAddress + 0x40, UIntToBA((uint)ret.Address));
            //nc.SetMemory(rootAddress + 0x1E8, new byte[] { 0, 0, 0, 1 });

            return ret;
        }

        HUDObject generateTextHUD(string text)
        {
            HUDObject ret = new HUDObject();

            //Set storage address
            ret.Address = globalAddr;
            globalAddr += 0x100;

            ret.Alignment = 0x00011247;
            ret.ObjectID = 0x008B9510; //(uint)nc.ByteAToULong(nc.GetMemory(0x008B9510, 4), 0, 4);
            ret.ShadowABGR = new byte[] { 0x40, 0, 0, 0 };
            ret.ShadowHorOff = 0.004808f;
            ret.ShadowVerOff = 0.004808f;
            ret.Size = 0.800000f;
            ret.Spacing = 0x02000000;
            ret.Text = text;
            ret.TextABGR = new byte[] { 0x80, 0xD0, 0xD0, 0xD0 };
            ret.TextBold = true;
            ret.Type = "Text";
            ret.Visibility = 1f;
            ret.XOff = 0.5f;
            ret.YOff = 0.5f;
            ret.Enabled = true;

            SaveHUD(ret, null);

            //nc.SetMemory(rootAddress + 0x40, UIntToBA((uint)ret.Address));
            //nc.SetMemory(rootAddress + 0x1E8, new byte[] { 0, 0, 0, 1 });

            return ret;
        }

        string generateGenericInput(string tag)
        {
            tag = tag.Replace(" ", "");
            StringBuilder classbuilder = new StringBuilder("using System;\r\nusing System.Windows.Forms;\r\nusing System.Drawing;\r\nusing dhx;\r\n\r\n");
            classbuilder.Append("AddRef(\"mscorlib.dll\");\r\nAddRef(\"System.dll\");\r\nAddRef(\"System.Windows.Forms.dll\");\r\nAddRef(\"System.Drawing.dll\");\r\n\r\n");
            
            classbuilder.AppendFormat("namespace {0}\r\n{{\r\n", GetType().Namespace);
            classbuilder.AppendFormat("\tpublic class {0} : ICompiledCode\r\n\t{{\r\n", tag);

            classbuilder.Append("\t\tstatic ICompiledCodeHost myHost = null;\r\n");
            classbuilder.Append("\t\tpublic ICompiledCodeHost host {\r\n");
            classbuilder.Append("\t\t\tget { return myHost; }\r\n");
            classbuilder.Append("\t\t\tset { myHost = value; }\r\n");
            classbuilder.Append("\t\t}\r\n");

            classbuilder.Append("\r\n\t\t//This is only called on initialization\r\n");
            classbuilder.Append("\t\tpublic static void Main() { }\r\n");

            classbuilder.Append("\r\n\t\t\r\n\t\t//This is called every loop if the object is being printed\r\n");
            classbuilder.Append("\t\tpublic int Start()\r\n\t\t{\r\n");

            classbuilder.Append("\t\t\t\r\n\t\t\treturn 0;\r\n\t\t}");

            classbuilder.Append("\r\n\t\t\r\n\t\t//This is raised when the hudDataValue changes for this object\r\n");
            classbuilder.Append("\t\tpublic void hudDataValue_Changed(int oldValue, int newValue)\r\n\t\t{\r\n");

            classbuilder.Append("\t\t\t\r\n\t\t}");

            classbuilder.Append("\r\n\t}\r\n}");
            return classbuilder.ToString();
        }

        string generateRootInput(string tag)
        {
            tag = tag.Replace(" ", "");
            string ret = "using System;\r\nusing System.Windows.Forms;\r\nusing dhx;\r\n\r\nAddRef(\"mscorlib.dll\");\r\nAddRef(\"System.dll\");\r\nAddRef(\"System.Windows.Forms.dll\");\r\nAddRef(\"System.Drawing.dll\");\r\n\r\nnamespace DLModMenu\r\n{\r\n    public class NAMEHERE : ICompiledCode\r\n    {\r\n        static ICompiledCodeHost myHost = null;\r\n        public ICompiledCodeHost host\r\n\t\t{\r\n            get { return myHost; }\r\n            set { myHost = value; }\r\n        }\r\n        \r\n        public static void Main() { }\r\n        \r\n        RootHUDObject r = null;\r\n        \r\n        public int Start()\r\n        {\r\n            if (r == null)\r\n                r = Family.this;\r\n            \r\n            if (myHost.AreButtonsPressed(new string[] { \"L3\" }))\r\n            {\r\n                if (r.hudVisibility > 0f)\r\n                    r.hudVisibility = 0f;\r\n                else\r\n                    r.hudVisibility = 1f;\r\n            }\r\n            else if (myHost.AreButtonsPressed(new string[] { \"Left\" }))\r\n            {\r\n                r.hudDataValue--;\r\n            }\r\n            else if (myHost.AreButtonsPressed(new string[] { \"Right\" }))\r\n            {\r\n                r.hudDataValue++;\r\n            }\r\n            \r\n            return 0;\r\n        }\r\n\r\n        public void hudDataValue_Changed(int oldValue, int newValue)\r\n        {\r\n            if (r == null)\r\n                r = Family.this;\r\n            \r\n            if (newValue > r.hudSubRoots.Length)\r\n                r.hudDataValue = r.hudSubRoots.Length - 1;\r\n            else if (newValue < 0)\r\n                r.hudDataValue = 0;\r\n            else\r\n            {\r\n                r.hudSelectedRootIndex = newValue;\r\n            }\r\n        }\r\n    }\r\n}\r\n";
            return ret.Replace("NAMEHERE", tag);
        }

        string generateColoredBoolText(string tag)
        {
            tag = tag.Replace(" ", "");
            StringBuilder classbuilder = new StringBuilder("using System;\r\nusing System.Windows.Forms;\r\nusing System.Drawing;\r\nusing dhx;\r\n\r\n");
            classbuilder.Append("AddRef(\"mscorlib.dll\");\r\nAddRef(\"System.dll\");\r\nAddRef(\"System.Windows.Forms.dll\");\r\nAddRef(\"System.Drawing.dll\");\r\n\r\n");
            
            classbuilder.AppendFormat("namespace {0}\r\n{{\r\n", GetType().Namespace);
            classbuilder.AppendFormat("\tpublic class {0} : ICompiledCode\r\n\t{{\r\n", tag);

            classbuilder.Append("\t\tstatic Color colorOn = Color.Aqua;\r\n");
            classbuilder.Append("\t\tstatic Color colorOff = Color.FromArgb(128, 208, 208, 208);\r\n\t\t\r\n");

            classbuilder.Append("\t\tstatic ICompiledCodeHost myHost = null;\r\n");
            classbuilder.Append("\t\tpublic ICompiledCodeHost host {\r\n");
            classbuilder.Append("\t\t\tget { return myHost; }\r\n");
            classbuilder.Append("\t\t\tset { myHost = value; }\r\n");
            classbuilder.Append("\t\t}\r\n");

            classbuilder.Append("\r\n\t\t//This is only called on initialization\r\n");
            classbuilder.Append("\t\tpublic static void Main() { }\r\n");

            classbuilder.Append("\r\n\t\t\r\n\t\t//This is called every loop if the object is being printed\r\n");
            classbuilder.Append("\t\tpublic int Start()\r\n\t\t{\r\n");

            classbuilder.Append("\t\t\t\r\n\t\t\treturn 0;\r\n\t\t}");

            classbuilder.Append("\r\n\t\t\r\n\t\t//This is raised when the hudDataValue changes for this object\r\n");
            classbuilder.Append("\t\tpublic void hudDataValue_Changed(int oldValue, int newValue)\r\n\t\t{\r\n");

            classbuilder.Append("\t\t\tTextHUDObject t = Family.this;\r\n");
            classbuilder.Append("\t\t\tif (newValue == 0)\r\n\t\t\t{\r\n");
                classbuilder.Append("\t\t\t\tt.hudTextABGR = colorOff;\r\n");
                classbuilder.Append("\t\t\t\t//Code here -- myHost.SetMem(0x009C2904, new byte[] { 0x3C, 0x88, 0x88, 0x89 });\r\n\t\t\t}\r\n");
            classbuilder.Append("\t\t\telse\r\n\t\t\t{\r\n");
                classbuilder.Append("\t\t\t\tt.hudTextABGR = colorOn;\r\n");
                classbuilder.Append("\t\t\t\t//Code here -- myHost.SetMem(0x009C2904, new byte[] { 0x41, 0, 0, 0 });\r\n\t\t\t}\r\n");

            classbuilder.Append("\t\t\t\r\n\t\t}");

            classbuilder.Append("\r\n\t}\r\n}");
            return classbuilder.ToString();
        }

        string generateSelector(string tag)
        {
            tag = tag.Replace(" ", "");

            string ret = "using System;\r\nusing System.Windows.Forms;\r\nusing dhx;\r\n";
            ret += "\r\nAddRef(\"mscorlib.dll\");\r\nAddRef(\"System.dll\");\r\nAddRef(\"System.Windows.Forms.dll\");\r\nAddRef(\"System.Drawing.dll\");\r\n";
            ret += "\r\nnamespace DLModMenu\r\n{\r\n    public class NAMEHERE : ICompiledCode\r\n    {\r\n        static ICompiledCodeHost myHost = null;\r\n        public ICompiledCodeHost host {\r\n            get { return myHost; }\r\n            set { myHost = value; }\r\n        }\r\n        \r\n        public static void Main() { }\r\n        \r\n\t\t//Positions of the Text object (YOff based)\r\n        float[] ySelOffs = { 0.50f };\r\n\t\t\r\n\t\t//Name of the object to activate with hudDataValue\r\n\t\t//string[] optNames = { \"Text0\" };\r\n        string[] optNames = new string[0];\r\n        \r\n        BoxHUDObject b = null;\r\n        \r\n        public int Start()\r\n        {\r\n            if (b == null)\r\n                b = Family.this;\r\n            \r\n            if (myHost.AreButtonsPressed(new string[] { \"O\" }))\r\n            {\r\n\t\t\t\tif (optNames.Length > 0) {\r\n\t\t\t\t\tTextHUDObject t = (TextHUDObject)myHost.FindObj(optNames[b.hudDataValue]);\r\n\t\t\t\t\tif (t != null)\r\n\t\t\t\t\t{\r\n\t\t\t\t\t\tif (t.hudDataValue == 0)\r\n\t\t\t\t\t\t\tt.hudDataValue = 1;\r\n\t\t\t\t\t\telse\r\n\t\t\t\t\t\t\tt.hudDataValue = 0;\r\n\t\t\t\t\t}\r\n\t\t\t\t}\r\n            }\r\n            else if (myHost.AreButtonsPressed(new string[] { \"Up\" }))\r\n            {\r\n                b.hudDataValue--;\r\n            }\r\n            else if (myHost.AreButtonsPressed(new string[] { \"Down\" }))\r\n            {\r\n                b.hudDataValue++;\r\n            }\r\n            \r\n            return 0;       \r\n        }\r\n\r\n        public void hudDataValue_Changed(int oldValue, int newValue)\r\n        {\r\n            if (b == null)\r\n                b = Family.this;\r\n            \r\n            if (newValue >= ySelOffs.Length)\r\n                b.hudDataValue = ySelOffs.Length - 1;\r\n            else if (newValue < 0)\r\n                b.hudDataValue = 0;\r\n            else if (b.hudDataValue < ySelOffs.Length)\r\n            {\r\n                b.hudYOff = ySelOffs[newValue];\r\n            }\r\n        }\r\n    }\r\n}";

            return ret.Replace("NAMEHERE", tag);
        }

        HUDObject generateRootHUD()
        {
            HUDObject ret = new HUDObject();

            rootAddress = globalAddr;
            globalAddr += 0x200;

            ret.Address = rootAddress;
            ret.CurrentRootIndex = -1;
            ret.Alignment = 0x00001E07;
            ret.XOff = 0.5f;
            ret.YOff = 0.5f;
            ret.Visibility = 1f;
            ret.Width = 1f;
            ret.Height = 1f;
            ret.Type = "Root";
            ret.ObjectID = 0x008B9420;
            ret.subItems = new string[0];
            ret.Enabled = true;

            SaveHUD(ret, null);

            return ret;
        }

        void WriteInitial()
        {
            if (NCInterface.ConnectionState() == 2)
            {
                HUDObject root = generateRootHUD();
                root.Name = "Main Root";
                root.CustomName = "Root";
                modMenuLayout.TopNode.Tag = "Main Root";
                root.InputCode = generateRootInput("Main Root");
                //root.Node = modMenuLayout.TopNode;
                RootHUDObject rootH = new RootHUDObject();
                rootH.obj = root;
                rootList.Add(rootH);

                modMenuLayout.SelectedNode = null;
                modMenuLayout.SelectedNode = modMenuLayout.TopNode;

                hasInitBeenWritten = true;
            }
        }


        public void SaveHUD(HUDObject h, TreeNode rootNode)
        {
            if (h.Type == null || (rootNode == null && h.Type == "Root") || h.Address == 0)
                return;

            byte[] store;

            byte[] rootAddr = BitConverter.GetBytes(rootAddress);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(rootAddr);
            NCInterface.SetMemory(0x011CBE24, rootAddr);

            switch (h.Type)
            {
                case "Text":
                    store = new byte[0x70];

                    Array.Copy(UIntToBA(h.ObjectID), 0, store,              0x00, 4);
                    Array.Copy(new byte[] { 0, 0, 0, 1 }, 0, store,         0x04, 4);
                    Array.Copy(UIntToBA(h.Alignment), 0, store,             0x0C, 4);
                    Array.Copy(FloatToHex(h.XOff), 0, store,                0x14, 4);
                    Array.Copy(FloatToHex(h.YOff), 0, store,                0x18, 4);
                    Array.Copy(FloatToHex(h.Size), 0, store,                0x1C, 4);
                    Array.Copy(FloatToHex(h.ShadowHorOff), 0, store,        0x24, 4);
                    Array.Copy(FloatToHex(h.ShadowVerOff), 0, store,        0x28, 4);
                    if (h.Enabled)
                        Array.Copy(FloatToHex(h.Visibility), 0, store,          0x2C, 4);
                    else
                        Array.Copy(new byte[4], 0, store, 0x2C, 4);
                    Array.Copy(h.TextABGR, 0, store,                        0x30, 4);
                    Array.Copy(h.ShadowABGR, 0, store,                      0x34, 4);
                    Array.Copy(UIntToBA((uint)h.Address + 0x70), 0, store,  0x4C, 4);
                    Array.Copy(UIntToBA(h.Spacing), 0, store,               0x54, 4);
                    Array.Copy(h.TextBold ? new byte[] { 0, 0, 0, 2 } : new byte[] { 0, 0, 0, 1 }, 0, store, 0x64, 4);
                    NCInterface.SetMemory(h.Address, store);

                    byte[] txt = NCInterface.StringToByteA(h.Text);
                    Array.Resize(ref txt, txt.Length + 1);
                    NCInterface.SetMemory(h.Address + 0x70, txt);

                    break;
                case "Box":
                    store = new byte[0x70];

                    Array.Copy(UIntToBA(h.ObjectID), 0, store,              0x00, 4);
                    Array.Copy(new byte[] { 0, 0, 0, 1 }, 0, store,         0x04, 4);
                    Array.Copy(UIntToBA(h.Alignment), 0, store,             0x0C, 4);
                    Array.Copy(FloatToHex(h.rotation * (float)(Math.PI / 180f)), 0, store,            0x10, 4);
                    Array.Copy(FloatToHex(h.XOff), 0, store,                0x14, 4);
                    Array.Copy(FloatToHex(h.YOff), 0, store,                0x18, 4);
                    Array.Copy(FloatToHex(h.Width), 0, store,               0x1C, 4);
                    Array.Copy(FloatToHex(h.Height), 0, store,              0x20, 4);
                    if (h.Enabled)
                        Array.Copy(FloatToHex(h.Visibility), 0, store, 0x2C, 4);
                    else
                        Array.Copy(new byte[4], 0, store, 0x2C, 4);
                    Array.Copy(h.TextABGR, 0, store,                        0x30, 4);
                    Array.Copy(h.TRCornerColor, 0, store,                   0x44, 4);
                    Array.Copy(h.BLCornerColor, 0, store,                   0x48, 4);
                    Array.Copy(h.BRCornerColor, 0, store,                   0x4C, 4);
                    NCInterface.SetMemory(h.Address, store);

                    break;
                case "Root":
                    store = new byte[] { 0x00, 0x8B, 0x94, 0x20, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1E, 0x07, 0x00, 0x00, 0x00, 0x00, 0x3F, 0x00, 0x00, 0x00, 0x3F, 0x00, 0x00, 0x00, 0x3F, 0x80, 0x00, 0x00, 0x3F, 0x80, 0x00, 0x00, 0x3B, 0x80, 0x00, 0x00, 0x3B, 0x9D, 0x89, 0xD9, 0x3F, 0x80, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x40, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3F, 0x80, 0x00, 0x00, 0x3F, 0x80, 0x00, 0x00 };

                    Array.Copy(UIntToBA(h.ObjectID), 0, store,              0x00, 4);
                    Array.Copy(new byte[] { 0, 0, 0, 1 }, 0, store,         0x04, 4);
                    Array.Copy(UIntToBA(h.Alignment), 0, store,             0x0C, 4);
                    Array.Copy(FloatToHex(h.XOff), 0, store,                0x14, 4);
                    Array.Copy(FloatToHex(h.YOff), 0, store,                0x18, 4);
                    Array.Copy(FloatToHex(h.Width), 0, store,               0x1C, 4);
                    Array.Copy(FloatToHex(h.Height), 0, store,              0x24, 4);
                    if (h.Enabled)
                        Array.Copy(FloatToHex(h.Visibility), 0, store, 0x2C, 4);
                    else
                        Array.Copy(new byte[4], 0, store, 0x2C, 4);

                    if (rootNode != null)
                    {
                        int size = 0;
                        int x = 0;
                        int y = 0;

                        for (x = 0; x < rootNode.Nodes.Count; x++) {
                            HUDObject ho = FindObjectWithTag(rootNode.Nodes[x].Tag.ToString());
                            if (ho.Type != "Root")
                            {
                                //if (ho.Name != null)
                                {
                                    size++;
                                    y++;

                                    byte[] addr = UIntToBA((uint)ho.Address);
                                    Array.Copy(addr, 0, store, 0x40 + (4 * (y - 1)), 4);
                                }
                            }
                        }

                        if (h.CurrentRootIndex >= 0 && h.CurrentRootIndex < h.subItems.Length)
                        {
                            ctlMain.HUDObject subRoot = FindObjectWithName(h.subItems[h.CurrentRootIndex]);
                            if (subRoot.Address != 0)
                            {
                                size++;
                                byte[] addr = UIntToBA((uint)subRoot.Address);
                                Array.Copy(addr, 0, store, 0x40 + (4 * y), 4);
                            }
                        }

                        Array.Copy(UIntToBA((uint)size), 0, store, 0x1E8, 4);
                    }

                    NCInterface.SetMemory(h.Address, store);

                    break;
            }

        }

        #endregion

        #region Misc

        void BuildAll()
        {
            foreach (RootHUDObject r in rootList)
            {
                r.inputH.BuildCode();
            }

            foreach (TextHUDObject t in textList)
            {
                t.inputH.BuildCode();
            }

            foreach (BoxHUDObject b in boxList)
            {
                b.inputH.BuildCode();
            }
        }

        static byte[] UIntToBA(uint val)
        {
            byte[] b = BitConverter.GetBytes(val);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(b);

            return b;
        }

        static public byte[] FloatToHex(float floatVal)
        {
            byte[] flt = BitConverter.GetBytes(Single.Parse(floatVal.ToString()));

            if (BitConverter.IsLittleEndian)
                Array.Reverse(flt);

            return flt;
            //return BitConverter.ToUInt32(flt, 0).ToString("X8");
        }

        #endregion

        #region Find Objects and Nodes

        public static void RemoveHUDWithTag(string tag)
        {
            if (tag.IndexOf("Text") >= 0)
            {
                foreach (TextHUDObject obj in textList)
                {
                    if (obj.obj.Name == tag)
                    {
                        textList.Remove(obj);
                        break;
                    }
                }
            }
            else if (tag.IndexOf("Root") >= 0)
            {
                foreach (RootHUDObject obj in rootList)
                {
                    if (obj.obj.Name == tag)
                    {
                        rootList.Remove(obj);
                        break;
                    }
                }
            }
            else if (tag.IndexOf("Box") >= 0)
            {
                foreach (BoxHUDObject obj in boxList)
                {
                    if (obj.obj.Name == tag)
                    {
                        boxList.Remove(obj);
                        break;
                    }
                }
            }
        }

        public static HUDObject FindObjectWithTag(string tag)
        {
            if (tag.IndexOf("Text") >= 0)
            {
                foreach (TextHUDObject obj in textList)
                {
                    if (obj.obj.Name == tag)
                        return obj.obj;
                }
            }
            else if (tag.IndexOf("Root") >= 0)
            {
                foreach (RootHUDObject obj in rootList)
                {
                    if (obj.obj.Name == tag)
                        return obj.obj;
                }
            }
            else if (tag.IndexOf("Box") >= 0)
            {
                foreach (BoxHUDObject obj in boxList)
                {
                    if (obj.obj.Name == tag)
                        return obj.obj;
                }
            }

            return new HUDObject();
        }

        public static HUDObject FindObjectWithName(string name)
        {
            foreach (TextHUDObject obj in textList)
            {
                if (obj.obj.CustomName == name)
                    return obj.obj;
            }
            foreach (RootHUDObject obj in rootList)
            {
                if (obj.obj.CustomName == name)
                    return obj.obj;
            }
            foreach (BoxHUDObject obj in boxList)
            {
                if (obj.obj.CustomName == name)
                    return obj.obj;
            }

            return new HUDObject();
        }

        public object FindHUDObjectUControlFromName(string name)
        {
            return FindHUDObjectUControlFromNameX(name);
        }

        public static object FindHUDObjectUControlFromNameX(string name)
        {
            foreach (TextHUDObject tobj in textList)
            {
                if (tobj.hudCustomName == name)
                    return tobj;
            }

            foreach (RootHUDObject robj in rootList)
            {
                if (robj.hudCustomName == name)
                    return robj;
            }

            foreach (BoxHUDObject bobj in boxList)
            {
                if (bobj.hudCustomName == name)
                    return bobj;
            }

            return null;
        }

        public object FindHUDObjectUControlFromTag(string tag)
        {
            return FindHUDObjectUControlFromTagX(tag);
        }

        public static object FindHUDObjectUControlFromTagX(string tag)
        {
            foreach (TextHUDObject tobj in textList)
            {
                if (tobj.hudName == tag)
                    return tobj;
            }

            foreach (RootHUDObject robj in rootList)
            {
                if (robj.hudName == tag)
                    return robj;
            }

            foreach (BoxHUDObject bobj in boxList)
            {
                if (bobj.hudName == tag)
                    return bobj;
            }

            return null;
        }

        public TreeNode FindTNodeWithNameFromTopInvoke(string name)
        {
            TreeNode top = null;
            Invoke((MethodInvoker)delegate
            {
                top = Instance.modMenuLayout.TopNode;
            });
            return FindNodeWithNameInvoke(top, name);
        }

        public TreeNode FindNodeWithNameInvoke(TreeNode curNode, string name)
        {
            if (curNode.Text == name)
                return curNode;

            TreeNode retNode = null;

            Invoke((MethodInvoker)delegate
            {
                foreach (TreeNode node in curNode.Nodes)
                {
                    if (node.Text == name)
                        retNode = node;
                    else
                    {
                        TreeNode ret = FindNodeWithNameInvoke(node, name);
                        if (ret != null)
                            retNode = ret;
                    }

                    if (retNode != null)
                        break;
                }
            });

            return retNode;
        }

        public static TreeNode FindTNodeWithNameFromTop(string name)
        {
            return FindNodeWithName(Instance.modMenuLayout.TopNode, name);
        }

        public static TreeNode FindNodeWithName(TreeNode curNode, string name)
        {
            if (curNode.Text == name)
                return curNode;

            foreach (TreeNode node in curNode.Nodes)
            {
                if (node.Text == name)
                    return node;
                else
                {
                    TreeNode ret = FindTNodeWithTag(node, name);
                    if (ret != null)
                        return ret;
                }
            }

            return null;
        }

        public static TextHUDObject FindTHUDOWithTag(string tag)
        {
            foreach (TextHUDObject obj in textList)
            {
                if (obj.obj.Name == tag)
                    return obj;
            }

            return new TextHUDObject();
        }

        public static BoxHUDObject FindBHUDOWithTag(string tag)
        {
            foreach (BoxHUDObject obj in boxList)
            {
                if (obj.obj.Name == tag)
                    return obj;
            }

            return new BoxHUDObject();
        }

        public static RootHUDObject FindRHUDOWithTag(string tag)
        {
            foreach (RootHUDObject obj in rootList)
            {
                if (obj.obj.Name == tag)
                    return obj;
            }

            return new RootHUDObject();
        }

        public static TreeNode FindTNodeWithTagFromTop(string tag)
        {
            return FindTNodeWithTag(Instance.modMenuLayout.TopNode, tag);
        }

        public static TreeNode FindTNodeWithTag(TreeNode curNode, string tag)
        {
            if (curNode == null)
                return null;

            if (curNode.Tag.ToString() == tag)
                return curNode;

            foreach (TreeNode node in curNode.Nodes)
            {
                if (node.Tag.ToString() == tag)
                    return node;
                else
                {
                    TreeNode ret = FindTNodeWithTag(node, tag);
                    if (ret != null)
                        return ret;
                }
            }

            return null;
        }

        #endregion

    }
}
