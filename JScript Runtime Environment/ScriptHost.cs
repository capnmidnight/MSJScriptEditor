using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;

namespace JScript_Runtime_Environment
{
    /// <summary>
    /// The script host class is the basic foundation under which scripts in the editor are ran. 
    /// It provides all of the "piping" to get inline scripts running.
    /// </summary>
	class ScriptHost : TextWriter
	{
		private CodeDomProvider provider;
		private CompilerParameters parms;
		public delegate void UpdateOutputDelegate(string line);
		public event UpdateOutputDelegate OnUpdateOutput;

		public ScriptHost()
		{
			this.provider = Microsoft.JScript.JScriptCodeProvider.CreateProvider("javascript");

            // TODO assemblies to load. Eventually want to make this configurable. This should be
            // good for the time being.
			this.parms = new CompilerParameters(new string[] { 
                "System.dll",
                "System.Drawing.dll",
                "System.Windows.Forms.dll",
                "System.Xml.dll"});

            // generate an in-memory executable
			this.parms.CompilerOptions = "/t:exe";
			this.parms.GenerateExecutable = true;
			this.parms.GenerateInMemory = true;
		}
        /// <summary>
        /// TODO LIB provides the basic import statements that are being used to execute the script. Also,
        /// defines a few methods that emulate javascript in-the-browser functionality. Need to make this
        /// configurable, with a default list provided to the user.
        /// </summary>
		private static string LIB = @"import System;
import System.Data;
import System.Data.SqlClient;
import System.Data.SqlTypes;
import System.Drawing;
import System.Drawing.Imaging;
import System.IO;
import System.Net;
import System.Net.Sockets;
import System.Text;
import System.Text.RegularExpressions;
import System.Windows.Forms;

function print(txt)
{
    Console.Write(txt);
}

function println(txt)
{
    Console.WriteLine(txt);
}

function alert(txt)
{
    MessageBox.Show(txt, ""JavaScript"", MessageBoxButtons.OK, MessageBoxIcon.Information);
}

try
{
";

        // we have to be proactive about catching errors so that
        // they don't kill the environment application
        private static string ERR = @"
}
catch(exp)
{
    Console.WriteLine(exp.Message);
}
";

        private static int PreLineCount;
        static ScriptHost()
        {
            // Count the number of lines that we're adding to the front
            // of the user's script so we can later give a line count
            // that will make sense to the user.
            PreLineCount = LIB.Split('\n').Length;
        }

        /// <summary>
        /// Takes a script snippet and executes it in a context of the
        /// libraries and methods defined in the ScriptHost
        /// </summary>
        /// <param name="script"></param>
		public void ExecuteFullScript(string script)
		{
			string code = LIB + script + ERR;
			CompilerResults rs = provider.CompileAssemblyFromSource(parms, code);

            //Generate a meaningful error message and send it out to whomever is listening
            // to the ScriptHost as a TextWriter. In this case, our "console" window.
			if (rs.Errors.Count > 0)
				foreach (CompilerError error in rs.Errors)
					this.WriteLine(string.Format("{0}: at {1}:{2}, \"{3}\"\r\n", error.ErrorNumber, error.Line - PreLineCount, error.Column, error.ErrorText));
			else
			{
				try
				{
                    //Capture standard output so we can redirect System.Console.WriteLine calls to our own textbox.
					Console.SetOut(this);

                    // TODO I forget why I'm invoking the script with an "asdf" parameter. I think I just need a non-null
                    // array of objects, which could be an empty array. Need to test later.
					rs.CompiledAssembly.EntryPoint.Invoke(null, new object[] { new string[] { "asdf" } });
					this.WriteLine(">> done");
				}
				catch (Exception exp)
				{
					this.WriteLine(string.Format("Runtime Error: {0}\r\n", exp.StackTrace));
				}
			}
		}

        /// <summary>
        /// A proxy for the Console.WriteLine method, so users can use standard output and
        /// the environment can capture it.
        /// </summary>
        /// <param name="line"></param>
		public override void WriteLine(string line, params object[] p)
		{
			if (this.OnUpdateOutput != null)
				this.OnUpdateOutput(string.Format("{0}\r\n", string.Format(line, p)));
		}

        /// <summary>
        /// A proxy for the Console.WriteLine method, so users can use standard output and
        /// the environment can capture it.
        /// </summary>
        /// <param name="line"></param>
        public override void Write(string line, params object[] p)
		{
			if (this.OnUpdateOutput != null)
				this.OnUpdateOutput(string.Format(line, p));
		}

		public override Encoding Encoding
		{
			get
			{
				return Encoding.ASCII;
			}
		}


        /// <summary>
        /// Takes a script command and executes it as a single line of code as if it
        /// existed within the context of another script. This is sort of equivalent to
        /// a REPL with command history.
        /// </summary>
        /// <param name="script"></param>
		public string ExecuteFullScriptReturn(string context, string command)
		{
			string code = LIB + context +
@"public class JScriptTestBed{
public static function Execute()
{
return eval(""" + command.Replace("\"", "\\\"") + @""");
}
}";

            // TODO refactor all of this code so that the duplicate stuff
            // is removed to another method
			CompilerResults rs = provider.CompileAssemblyFromSource(parms, code);
			string retVal = null;
			if (rs.Errors.Count > 0)
				foreach (CompilerError error in rs.Errors)
					this.WriteLine(string.Format("{0}: at {1}:{2}, \"{3}\"\r\n", error.ErrorNumber, error.Line - 28, error.Column, error.ErrorText));
			else
			{
				try
				{
					Console.SetOut(this);
					rs.CompiledAssembly.EntryPoint.Invoke(null, new object[] { new string[] { "asdf" } });
					object ret = rs.CompiledAssembly.GetType("JScriptTestBed").GetMethod("Execute").Invoke(null, null);
					if (ret != null)
						retVal = ret.ToString();
					this.WriteLine(">> done");
				}
				catch (Exception exp)
				{
					this.WriteLine(string.Format("Runtime Error: {0}\r\n", exp.StackTrace));
				}
			}
			return retVal;
		}
	}
}