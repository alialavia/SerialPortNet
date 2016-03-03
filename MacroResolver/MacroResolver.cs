using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;


namespace MacroResolver
{
	/// <summary>
	/// Resolve macro value in include files
	/// </summary>
	public class MacroResolver
	{
		//private String code = "int main(int argc, char* argv[]){\tint temp = {0};\n\t#include<stdio.h>\t\n\tprintf(\"%d\", temp); return 0; }";
		/// <summary>
		/// Initializes a new instance of the <see cref="SerialPortNET.MacroResolver"/> class.
		/// </summary>
		/// <param name="includeList">List of include files</param>
		/// <param name="Macro">Macro name</param>
		public MacroResolver (IEnumerable<String> includeList, IEnumerable<String> Macros)
		{			
			// Create the required source
			StreamWriter fileWriter = new StreamWriter ("getMacro.c");
			fileWriter.WriteLine ("#include<stdio.h>");
			foreach (var includeFile in includeList)
				fileWriter.WriteLine (String.Format ("#include<{0}>", includeFile));

			fileWriter.WriteLine ("int main(void){{");
			foreach (var Macro in Macros) 
				fileWriter.WriteLine ("\t#ifdef {0} \t\n\tprintf(\"{0}=%d\\r\\n\", {0});\n\t#endif", Macro);
			
			fileWriter.WriteLine ("\treturn 0; }}\r\n");
			fileWriter.Flush ();
			fileWriter.Close ();

			// Compile it
			Process compileProcess = new Process ();
			compileProcess.EnableRaisingEvents = true;
			compileProcess.StartInfo.FileName = "gcc";
			compileProcess.StartInfo.Arguments = "getMacro.c -o getMacro";
			compileProcess.StartInfo.RedirectStandardError = true;
			compileProcess.StartInfo.UseShellExecute = false;
			compileProcess.Start ();
			compileProcess.WaitForExit ();

			var compileOutput = compileProcess.StandardError.ReadToEnd ();		

			if (compileOutput != "")
				throw new Exception(compileOutput);
			// Run it
			Process executeProcess = new Process ();
			executeProcess.EnableRaisingEvents = true;
			executeProcess.StartInfo.FileName = "getMacro";
			executeProcess.StartInfo.Arguments = "";
			executeProcess.StartInfo.RedirectStandardOutput = true;
			executeProcess.StartInfo.UseShellExecute = false;
			executeProcess.Start ();
			executeProcess.WaitForExit ();

			while (!executeProcess.StandardOutput.EndOfStream) {
				var fields = executeProcess.StandardOutput.ReadLine ().Split(new String[] {"="}, StringSplitOptions.None);
				Values.Add (fields [0], fields [1]);
			}				
		}
		/// <summary>
		/// Value of the macro
		/// </summary>
		public Dictionary<String, String> Values = new Dictionary<string, string>();
	}
}

