using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;

namespace JScript_Runtime_Environment
{
    public partial class MainForm : Form
    {
        private int childFormNumber = 0;
        List<ScriptState> scripts;
        ScriptState currentScript;
        private ScriptHost scriptHost;
        Font font;
        int totalLineCount;
        string[] lines;
        bool scriptLocked;
        public MainForm()
        {
            this.InitializeComponent();
            this.scripts = new List<ScriptState>();
            this.scriptHost = new ScriptHost();
            this.scriptHost.OnUpdateOutput += new ScriptHost.UpdateOutputDelegate(scriptHost_UpdateOutput);
            this.font = new Font("Courier New", 20);
            this.ShowNewEditor(null, null);
            this.scriptLocked = false;
        }

        ScriptState ActiveScript
        {
            get
            {
                return this.currentScript;
            }
            set
            {
                this.currentScript = value; foreach (TabButton button in this.flowPanTabs.Controls)
                    button.BackColor = Color.LightGray;
                if (value != null)
                {
                    this.txtScript.Text = value.ScriptText;
                    value.TabControl.BackColor = Color.LightBlue;
                }
                else
                {
                    this.txtScript.Text = "";
                }
            }
        }

        void scriptHost_UpdateOutput(string line)
        {
            this.txtOutput.Text += line;
        }

        private void ShowNewEditor(object sender, EventArgs e)
        {
            if (this.scripts.Count == 0)
            {
                this.txtScript.Enabled = true;
                this.btnClose.Visible = true;
            }
            ScriptState state = new ScriptState();
            this.scripts.Add(state);
            state.FileName += " " + childFormNumber++;
            this.flowPanTabs.Controls.Add(state.TabControl);
            state.TabControl.Click += new EventHandler(TabControl_Click);
            this.ActiveScript = state;
        }

        void TabControl_Click(object sender, EventArgs e)
        {
            TabButton button = sender as TabButton;
            this.ActiveScript = button.Script;
        }

        private void OpenFile(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            openFileDialog.Filter = "JavaScript files (*.js)|*.js|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string FileName = openFileDialog.FileName;
                ShowNewEditor(sender, e);
                ScriptState state = new ScriptState();
                state.FileName = FileName;
                state.ScriptText = File.ReadAllText(FileName);
                this.ActiveScript = state;
            }
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            saveFileDialog.Filter = "JavaScript Files (*.js)|*.js|All Files (*.*)|*.*";
            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string FileName = saveFileDialog.FileName;
                File.WriteAllText(FileName, this.ActiveScript.ScriptText);
                this.ActiveScript.FileName = FileName;
            }
        }

        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void CutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyToolStripMenuItem_Click(sender, e);
            if (this.txtScript.SelectedText != null && this.txtScript.SelectedText.Length > 0)
                this.txtScript.SelectedText = "";
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.txtScript.SelectedText != null && this.txtScript.SelectedText.Length > 0)
                Clipboard.SetText(this.txtScript.SelectedText);
        }

        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.txtScript.SelectedText = Clipboard.GetText();
        }

        private void ToolBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStrip.Visible = toolBarToolStripMenuItem.Checked;
        }

        private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.scripts.Clear();
            this.flowPanTabs.Controls.Clear();
            this.ShowNewEditor(null, null);
            this.ActiveScript = null;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.ActiveScript.FileName.IndexOf("source window") == 0)
                SaveAsToolStripMenuItem_Click(sender, e);
            else
                File.WriteAllText(this.ActiveScript.FileName, this.ActiveScript.ScriptText);
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                using (PrintDocument doc = CreatePrintDocument())
                {
                    doc.PrinterSettings = printDialog.PrinterSettings;
                    doc.Print();
                }
            }
        }

        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (PrintDocument doc = CreatePrintDocument())
            {
                printPreview.Document = doc;
                printPreview.ShowDialog();
            }
        }

        private PrintDocument CreatePrintDocument()
        {
            PrintDocument doc = new PrintDocument();

            totalLineCount = 0;
            doc.DocumentName = this.ActiveScript.FileName;
            lines = this.txtScript.Lines;
            doc.PrintPage += new PrintPageEventHandler(doc_PrintPage);
            return doc;
        }

        private void doc_PrintPage(object sender, PrintPageEventArgs e)
        {
            float lineHeight = font.GetHeight(e.Graphics);
            int linesPerPage = (int)(e.MarginBounds.Height / lineHeight);
            int lineCount = 0;
            while (lineCount < linesPerPage
                && totalLineCount < lines.Length)
            {
                int lineLength = 0;
                for (int i = 1; i <= lines[lineCount].Length; ++i)
                    if (e.Graphics.MeasureString(lines[lineCount], font).Width <= e.MarginBounds.Width)
                        lineLength = i;
                e.Graphics.DrawString(lines[lineCount].Substring(0, lineLength), font, Brushes.Black,
                    new PointF(e.MarginBounds.X, e.MarginBounds.Y + lineHeight * lineCount));
                lineCount++;
                totalLineCount++;
            }
        }

        private void printSetupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            printDialog.ShowDialog();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.txtScript.SelectAll();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ActiveScript.Undo();
            scriptLocked = true;
            this.txtScript.Text = this.ActiveScript.ScriptText;
            scriptLocked = false;
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ActiveScript.Redo();
            scriptLocked = true;
            this.txtScript.Text = this.ActiveScript.ScriptText;
            scriptLocked = false;
        }

        private void executeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.ActiveScript != null)
            {
                this.txtOutput.Text = "";
                Application.DoEvents();
                scriptHost.ExecuteFullScript(this.ActiveScript.ScriptText);
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.scripts.Count > 0)
            {
                this.flowPanTabs.Controls.Remove(this.ActiveScript.TabControl);
                this.scripts.Remove(this.ActiveScript);
                if (this.scripts.Count > 0)
                    this.ActiveScript = this.scripts[0];
            }
            if (this.scripts.Count == 0)
            {
                this.ActiveScript = null;
                this.txtScript.Enabled = false;
                this.btnClose.Visible = false;
            }
        }

        private void libraryViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new LibraryView().Show();
        }

        private void txtScript_TextChanged(object sender, EventArgs e)
        {
            if (!scriptLocked)
                this.ActiveScript.ScriptText = this.txtScript.Text;
        }

        private void txtExecute_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string msg = scriptHost.ExecuteFullScriptReturn(this.txtScript.Text, this.txtExecute.Text);
                txtOutput.Text = string.Format("\"{0}\": {1}\n", txtExecute.Text, msg);
            }
        }

        protected void UpdateCursorLocation()
        {
            int column = this.txtScript.SelectionStart;
            int row = 1;
            for (int i = 0; i < this.txtScript.Lines.Length; ++i)
            {
                if (column > this.txtScript.Lines[i].Length)
                {
                    column -= this.txtScript.Lines[i].Length + 2;
                    row++;
                }
                else
                {
                    break;
                }
            }
            this.lblCusorStatus.Text = string.Format("Row: {0}, Column: {1}", row, column + 1);
        }

        private void txtScript_KeyUp(object sender, KeyEventArgs e)
        {
            UpdateCursorLocation();
        }

        private void txtScript_MouseClick(object sender, MouseEventArgs e)
        {
            UpdateCursorLocation();
        }
    }
}
