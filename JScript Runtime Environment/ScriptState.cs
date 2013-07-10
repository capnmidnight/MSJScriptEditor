using System;
//using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace JScript_Runtime_Environment
{
    /// <summary>
    /// Editor state object, tracking undo and redo history, needs-saving state, etc.
    /// </summary>
    public class ScriptState
    {
		Stack<string> past;
		Stack<string> future;
		string currentText;
		string fileName;
        TabButton tab;
        public ScriptState()
        {
			this.past = new Stack<string>();
			this.future = new Stack<string>();
			this.currentText = "";
			this.fileName = "new script";
            this.tab = new TabButton(this);
            this.tab.Text = this.fileName;
        }

        /// <summary>
        /// The tab the script is associated with
        /// </summary>
        public TabButton TabControl
        {
            get
            {
                return this.tab;
            }
        }

        /// <summary>
        /// move forward in the edit stack
        /// </summary>
        public void Redo()
        {
            if (future.Count > 0)
            {
				this.past.Push(this.currentText);
				this.currentText = future.Pop();
            }
        }

        /// <summary>
        /// move back in the edit stack
        /// </summary>
        public void Undo()
        {
            if (past.Count > 0)
            {
				this.future.Push(this.currentText);
				this.currentText = past.Pop();
            }
        }

        /// <summary>
        /// set the edit text explicitly, which clears the redo stack
        /// </summary>
		public string ScriptText
		{
			set
			{
				this.future.Clear();
				this.past.Push(this.currentText);
				this.currentText = value;
			}
			get
			{
				return this.currentText;
			}
		}


        /// <summary>
        /// give this script a filename where it will eventually be saved.
        /// </summary>
		public string FileName
		{
			get
			{
				return this.fileName;
			}
			set
			{
                this.fileName = value;
                this.tab.Text = value;
			}
		}
    }
}