using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using dhx;
using System.Text.RegularExpressions;
using System.Reflection;
using FastColoredTextBoxNS;

namespace DLModMenu
{
    public partial class InputHandler : UserControl
    {
        bool builtProperly = false;
        int shouldBuild = 0;

        public ICompiledCode CompiledCode = null;
        private ctlMain.HUDObject _hudObject;
        public ctlMain.HUDObject hudObject
        {
            get { return _hudObject; }
            set
            {
                _hudObject = value;
                inputBox.Text = _hudObject.InputCode;
                compileButt_Click(null, null);
                hCustomName = _hudObject.CustomName;
            }
        }

        public string hTag
        {
            get { return hudObject.Name; }
        }

        public string hCustomName = "";

        public InputHandler()
        {
            InitializeComponent();
        }

        CodeCompiler cc = new CodeCompiler();

        private void compileButt_Click(object sender, EventArgs e)
        {
            //Get rid of the changed line color...
            string tempFile = System.IO.Path.GetTempFileName();
            inputBox.SaveToFile(tempFile, Encoding.ASCII);
            System.IO.File.Delete(tempFile);
        }

        private void buildButt_Click(object sender, EventArgs e)
        {
            builtProperly = BuildCode();
            if (builtProperly)
                shouldBuild = 0;
            else
                shouldBuild = 1;
        }

        private void execButt_Click(object sender, EventArgs e)
        {
            statusLabel.Text = "Status: Executing...";
            int ret = RunCode(false);

            if (ret == -0x6556)
                statusLabel.Text = "Status: Failed to execute, code not yet built!";
            else
                statusLabel.Text = "Status: Executed returned: " + ret.ToString();
        }

        public int RunCode(bool forceBuild)
        {
            if (CompiledCode != null)
            {
                return CompiledCode.Start();
            }
            else if (forceBuild && shouldBuild == 0)
            {
                builtProperly = BuildCode();
                if (builtProperly)
                    RunCode(false);
                else
                    shouldBuild = 1;
            }

            return -0x6556;
        }

        public int RaiseEvent(string eventName, object[] args)
        {
            if (CompiledCode != null)
            {
                try
                {
                    Invoke((MethodInvoker)delegate
                    {
                        MethodInfo mi = CompiledCode.GetType().GetMethod(eventName);
                        if (mi != null)
                            mi.Invoke(CompiledCode, args);
                    });
                }
                catch (Exception) {

                }
            }

            return -0x6556;
        }

        public bool BuildCode()
        {
            if (inputBox.Text != "")
            {
                statusLabel.Text = "Status: Building...";
                Application.DoEvents();

                string[] refs;
                string code = ParseCode(inputBox.Text, out refs);
                if (code == null || code == "")
                    return false;

                try
                {
                    string input = "DLModMenu " + hTag.Replace(" ", "") + "  ";
                    string[] args = input.Split(' ');
                    //namespace, class, function, static, params
                    CompiledCode = cc.ExecuteCode(ctlMain.AssemblyDirectory, code, args[0], args[1], args[2], ctlMain.cch, refs);
                    statusLabel.Text = "Status: Built";
                    return true;
                }
                catch (Exception ex)
                {
                    //Console.WriteLine("   " + ex.Message);
                    MessageBox.Show(ex.Message);
                    statusLabel.Text = "Status: Error in Code";
                    return false;
                }
            }

            return false;
        }

        private string ParseCode(string code, out string[] references)
        {
            references = new string[0];
            int y = 0;

            string[] lines = code.Split(new char[] { '\n' });
            for (int x = 0; x < lines.Length; x++)
            {
                lines[x] = lines[x].Replace("\r", "");
                string[] words = lines[x].Split(' ');
                if (lines[x].IndexOf("Family.") >= 0)
                {
                    bool hasSemicolon = false;
                    string[] split = lines[x].Split(new string[] { "Family." }, 100, StringSplitOptions.RemoveEmptyEntries);
                    string[] sect = lines[x].Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                    string name = "";
                    for (y = 0; y < sect.Length; y++)
                        if (sect[y].IndexOf("Family") >= 0)
                        {
                            name = sect[y + 1].Replace(")", "");
                            if (name.IndexOf(";") >= 0)
                            {
                                hasSemicolon = true;
                                name = name.Replace(";", "");
                            }
                            break;
                        }

                    string newLine = "";

                    if (name == "this")
                        name = hCustomName;

                    object tempUC = ctlMain.FindHUDObjectUControlFromNameX(name);
                    if (tempUC == null)
                    {
                        MessageBox.Show(name + " is not a real TreeNode!");
                        return null;
                    }
                    newLine = split[0] + "((" + tempUC.GetType().Name + ")host.FindObj(\"" + name + "\"))";

                    for (y = (y + 2); y < sect.Length; y++)
                    {
                        newLine += "." + sect[y];
                    }
                    if (hasSemicolon)
                        newLine += ";";

                    lines[x] = newLine;

                    //MessageBox.Show(((TextHUDObject)ctlMain.FindHUDObjectUControlFromName(name)).hudDataValue.ToString());
                }
                else if (lines[x].IndexOf("AddRef(") >= 0)
                {
                    Array.Resize(ref references, references.Length + 1);
                    references[references.Length - 1] = lines[x].Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries)[1].Replace(" ", "").Replace("\"", "");
                    lines[x] = "//" + lines[x];
                }


            }

            return String.Join("\r\n", lines);
        }

        private void InputHandler_Load(object sender, EventArgs e)
        {

        }

        private void InputHandler_Resize(object sender, EventArgs e)
        {
            inputBox.Width = Width - 10;
            inputBox.Height = Height - 30;
            inputBox.Location = new Point(5, 5);

            execButt.Top = Height - 25;
            statusLabel.Top = Height - 20;
            compileButt.Top = Height - 25;
            buildButt.Top = Height - 25;
        }

        #region Syntax Highlighter

        TextStyle BlueStyle = new TextStyle(Brushes.LightBlue, null, FontStyle.Regular);
        TextStyle BoldStyle = new TextStyle(null, null, FontStyle.Bold | FontStyle.Underline);
        TextStyle GrayStyle = new TextStyle(Brushes.LightGray, null, FontStyle.Regular);
        TextStyle MagentaStyle = new TextStyle(Brushes.Magenta, null, FontStyle.Regular);
        TextStyle GreenStyle = new TextStyle(Brushes.LimeGreen, null, FontStyle.Italic);
        TextStyle BrownStyle = new TextStyle(Brushes.BurlyWood, null, FontStyle.Italic);
        TextStyle MaroonStyle = new TextStyle(Brushes.LightYellow, null, FontStyle.Regular);
        MarkerStyle SameWordsStyle = new MarkerStyle(new SolidBrush(Color.FromArgb(40, Color.Gray)));

        private void inputBox_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            "".Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries
            CSharpSyntaxHighlight(e);
            statusLabel.Text = "Status: Not Built";
        }

        private void CSharpSyntaxHighlight(FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            inputBox.LeftBracket = '(';
            inputBox.RightBracket = ')';
            inputBox.LeftBracket2 = '\x0';
            inputBox.RightBracket2 = '\x0';
            //clear style of changed range
            e.ChangedRange.ClearStyle(BlueStyle, BoldStyle, GrayStyle, MagentaStyle, GreenStyle, BrownStyle);

            //string highlighting
            e.ChangedRange.SetStyle(BrownStyle, @"""""|@""""|''|@"".*?""|(?<!@)(?<range>"".*?[^\\]"")|'.*?[^\\]'");
            //comment highlighting
            e.ChangedRange.SetStyle(GreenStyle, @"//.*$", RegexOptions.Multiline);
            e.ChangedRange.SetStyle(GreenStyle, @"(/\*.*?\*/)|(/\*.*)", RegexOptions.Singleline);
            e.ChangedRange.SetStyle(GreenStyle, @"(/\*.*?\*/)|(.*\*/)", RegexOptions.Singleline | RegexOptions.RightToLeft);
            //number highlighting
            e.ChangedRange.SetStyle(MagentaStyle, @"\b\d+[\.]?\d*([eE]\-?\d+)?[lLdDfF]?\b|\b0x[a-fA-F\d]+\b");
            //attribute highlighting
            e.ChangedRange.SetStyle(GrayStyle, @"^\s*(?<range>\[.+?\])\s*$", RegexOptions.Multiline);
            //class name highlighting
            e.ChangedRange.SetStyle(BoldStyle, @"\b(class|struct|enum|interface)\s+(?<range>\w+?)\b");
            //keyword highlighting
            e.ChangedRange.SetStyle(BlueStyle, @"\b(AddRef|TextHUDObject|RootHUDObject|BoxHUDObject|Random|abstract|as|base|bool|break|byte|case|catch|char|checked|class|const|continue|decimal|default|delegate|do|double|else|enum|event|explicit|extern|false|Family|finally|fixed|float|for|foreach|goto|if|implicit|in|int|interface|internal|is|lock|long|namespace|new|null|object|operator|out|override|params|private|protected|public|readonly|ref|return|sbyte|sealed|short|sizeof|stackalloc|static|string|struct|switch|this|throw|true|try|typeof|uint|ulong|unchecked|unsafe|ushort|using|virtual|void|volatile|while|add|alias|ascending|descending|dynamic|from|get|global|group|into|join|let|orderby|partial|remove|select|set|value|var|where|yield)\b|#region\b|#endregion\b");

            //clear folding markers
            e.ChangedRange.ClearFoldingMarkers();

            //set folding markers
            e.ChangedRange.SetFoldingMarkers("{", "}");//allow to collapse brackets block
            e.ChangedRange.SetFoldingMarkers(@"#region\b", @"#endregion\b");//allow to collapse #region blocks
            e.ChangedRange.SetFoldingMarkers(@"/\*", @"\*/");//allow to collapse comment block
        }

        #endregion

        private void inputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.S && e.Control)
            {
                compileButt.PerformClick();
            }
        }

    }
}
