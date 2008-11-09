
#region ================== Copyright (c) 2007 Pascal vd Heiden

/*
 * Copyright (c) 2007 Pascal vd Heiden, www.codeimp.com
 * This program is released under GNU General Public License
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 */

#endregion

#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Types;
using CodeImp.DoomBuilder.IO;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	internal sealed class ScriptLumpDocumentTab : ScriptDocumentTab
	{
		#region ================== Constants
		
		#endregion
		
		#region ================== Variables
		
		private string lumpname;
		
		#endregion
		
		#region ================== Properties
		
		public override bool ExplicitSave { get { return false; } }
		public override bool IsClosable { get { return false; } }
		public override bool IsReconfigurable { get { return false; } }
		
		#endregion
		
		#region ================== Constructor / Disposer
		
		// Constructor
		public ScriptLumpDocumentTab(string lumpname)
		{
			// Initialize
			this.lumpname = lumpname;
			this.config = new ScriptConfiguration(); // TODO: Figure out script config
			
			SetTitle(lumpname.ToUpper());
		}
		
		// Disposer
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}
		
		#endregion
		
		#region ================== Methods
		
		#endregion
		
		#region ================== Events
		
		#endregion
	}
}