using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MacroResolver
{
	public class MacroHelper
	{
		private Dictionary<String, String> LUT;
		public MacroHelper(Type MacroClass, IEnumerable<String> headerFiles)
		{
			DateTime t = DateTime.Now;
			List<String> fieldNames = new List<string> ();

			foreach (var prop in MacroClass.GetProperties())
				fieldNames.Add (prop.Name);

			MacroResolver mr = new MacroResolver (headerFiles, fieldNames);
			LUT = mr.Values;
			var s = DateTime.Now - t;
		}

		public int GetMacro(String propertyName)
		{			
			if (LUT.ContainsKey (propertyName))
				return int.Parse(LUT[propertyName]);
			throw new ArgumentNullException (String.Format("{0} is not defined", propertyName));
		}

		public int GetMacro()
		{
			return GetMacro(new StackFrame(1).GetMethod().Name.Remove(0,4));
		}
	}
}

