using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
namespace JScript_Runtime_Environment
{
    /// <summary>
    /// TODO Make this actually work. It's supposed to show a list of the libraries
    /// installed in the user's global assembly cache so that they may select one
    /// that they want to include in their script. It doesn't really do that right now.
    /// </summary>
	public partial class LibraryView : Form
	{
		public LibraryView()
		{
			InitializeComponent();
		}
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			this.Search();
		}

		void Search()
		{
			Type[] types = Assembly.GetAssembly(typeof(Int32)).GetTypes();
			treeView1.BeginUpdate();
			treeView1.Nodes.Clear();
			foreach (Type type in types)
			{
				treeView1.Nodes.Add(type.Name);
			}
			treeView1.EndUpdate();
		}
	}
}