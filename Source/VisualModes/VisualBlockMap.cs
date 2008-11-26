
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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using CodeImp.DoomBuilder.Windows;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Actions;
using CodeImp.DoomBuilder.Geometry;
using System.Drawing;
using SlimDX;
using CodeImp.DoomBuilder.Editing;

#endregion

namespace CodeImp.DoomBuilder.VisualModes
{
	public sealed class VisualBlockMap
	{
		#region ================== Constants
		
		public const int BLOCK_SIZE_SHIFT = 7;
		public const int BLOCK_SIZE = 1 << BLOCK_SIZE_SHIFT;
		
		#endregion
		
		#region ================== Variables
		
		// Blocks
		private Dictionary<ulong, VisualBlockEntry> blockmap;
		
		// State
		private bool isdisposed;
		
		#endregion
		
		#region ================== Properties
		
		public bool IsDisposed { get { return isdisposed; } }
		
		#endregion
		
		#region ================== Constructor / Disposer
		
		// Constructor
		internal VisualBlockMap()
		{
			// Initialize
			blockmap = new Dictionary<ulong,VisualBlockEntry>();
		}
		
		// Disposer
		internal void Dispose()
		{
			// Not already disposed?
			if(!isdisposed)
			{
				// Clean up
				blockmap = null;
				
				// Done
				isdisposed = true;
			}
		}
		
		#endregion
		
		#region ================== Basic Methods
		
		// This returns the block coordinates
		public Point GetBlockCoordinates(Vector2D v)
		{
			return new Point((int)v.x >> BLOCK_SIZE_SHIFT,
							 (int)v.y >> BLOCK_SIZE_SHIFT);
		}
		
		// This returns the key for a block at the given coordinates
		private ulong GetBlockKey(Point p)
		{
			return unchecked( ((ulong)(uint)p.X << 32) + (ulong)(uint)p.Y );
		}
		
		// This returns the block with the given coordinates
		// Creates the block if it doesn't exist yet
		public VisualBlockEntry GetBlock(Point p)
		{
			ulong k = GetBlockKey(p);
			if(blockmap.ContainsKey(k))
			{
				return blockmap[k];
			}
			else
			{
				return (blockmap[k] = new VisualBlockEntry());
			}
		}
		
		// This clears the blockmap
		public void Clear()
		{
			blockmap = new Dictionary<ulong,VisualBlockEntry>();
		}
		
		// This returns a range of blocks in a square
		public List<VisualBlockEntry> GetSquareRange(RectangleF rect)
		{
			// Calculate block coordinates
			Point lt = GetBlockCoordinates(new Vector2D(rect.Left, rect.Top));
			Point rb = GetBlockCoordinates(new Vector2D(rect.Right, rect.Bottom));
			
			// Go through the range to make a list
			int entriescount = (rb.X - lt.X) * (rb.Y - lt.Y);
			List<VisualBlockEntry> entries = new List<VisualBlockEntry>(entriescount);
			for(int x = lt.X; x <= rb.X; x++)
			{
				for(int y = lt.Y; y <= rb.Y; y++)
				{
					entries.Add(GetBlock(new Point(x, y)));
				}
			}

			// Return list
			return entries;
		}

		#endregion

		#region ================== Advanced Methods

		// This puts a whole set of linedefs in the blocks they cross
		public void AddLinedefsSet(ICollection<Linedef> lines)
		{
			foreach(Linedef l in lines) AddLinedef(l);
		}
		
		// This puts a single linedef in all blocks it crosses
		public void AddLinedef(Linedef line)
		{
			Vector2D v1, v2;
			float deltax, deltay;
			float posx, posy;
			Point pos, end;
			int dirx, diry;
			
			// Get coordinates
			v1 = line.Start.Position;
			v2 = line.End.Position;
			
			// Find start and end block
			pos = GetBlockCoordinates(v1);
			end = GetBlockCoordinates(v2);
			
			// Add lines to this block
			GetBlock(pos).Lines.Add(line);
			
			// Moving outside the block?
			if(pos != end)
			{
				// Calculate current block edges
				float cl = pos.X * BLOCK_SIZE;
				float cr = (pos.X + 1) * BLOCK_SIZE;
				float ct = pos.Y * BLOCK_SIZE;
				float cb = (pos.Y + 1) * BLOCK_SIZE;
				
				// Line directions
				dirx = Math.Sign(v2.x - v1.x);
				diry = Math.Sign(v2.y - v1.y);
				
				// Calculate offset and delta movement over x
				if(dirx >= 0)
				{
					posx = (cr - v1.x) / (v2.x - v1.x);
					deltax = BLOCK_SIZE / (v2.x - v1.x);
				}
				else
				{
					// Calculate offset and delta movement over x
					posx = (v1.x - cl) / (v1.x - v2.x);
					deltax = BLOCK_SIZE / (v1.x - v2.x);
				}
				
				// Calculate offset and delta movement over y
				if(diry >= 0)
				{
					posy = (cb - v1.y) / (v2.y - v1.y);
					deltay = BLOCK_SIZE / (v2.y - v1.y);
				}
				else
				{
					posy = (v1.y - ct) / (v1.y - v2.y);
					deltay = BLOCK_SIZE / (v1.y - v2.y);
				}
				
				// Continue while not reached the end
				while(pos != end)
				{
					// Check in which direction to move
					if(posx < posy)
					{
						// Move horizontally
						posx += deltax;
						if(pos.X != end.X) pos.X += dirx;
					}
					else
					{
						// Move vertically
						posy += deltay;
						if(pos.Y != end.Y) pos.Y += diry;
					}
					
					// Add lines to this block
					GetBlock(pos).Lines.Add(line);
				}
			}
		}
		
		#endregion
	}
}