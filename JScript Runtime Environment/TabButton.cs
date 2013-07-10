using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace JScript_Runtime_Environment
{
    /// <summary>
    /// A tab is really just a button that does something different than you might expect!
    /// </summary>
    public class TabButton : Button
    {
		private ScriptState parent;
        public TabButton(ScriptState parent)
            : base()
        {
            this.parent = parent;
        }

        // This way, we can reuse all of our GUI controls between scripts
        public ScriptState Script
        {
            get
            {
                return this.parent;
            }
        }
    }
}
