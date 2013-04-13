#region Copyright & License Information
/*
 * Copyright 2013 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion


using System;
using System.Collections.Generic;

namespace OpenRA.Mods.RA
{
	public class FastMarchingSolver
	{
		public FieldNode[,] Field;
	
		int2[] dirs = {new int2(-1,0), new int2(+1,0), new int2(0,-1), new int2(0,+1) };

		int Width;
		int Height;

		internal List<FieldNode> narrowList;

		public Func<CPos,bool> CanEnter = (_) => true;

		public FastMarchingSolver(int width, int height)
		{
			Width = width;
			Height = height;
			narrowList = new List<FieldNode>();

			Field = new FieldNode[Width, Height];

			for (int x = 0; x < Width; x++)			
				for (int y = 0; y < Height; y++)
					Field[x,y] = new FieldNode(x,y);


		}

		public void init(int2 seed)
		{
			FieldNode node = Field[seed.X,seed.Y];
			node.Phi = 0;
			node.State = NodeState.frozen;
			calcNeighbors(node);
		}

		void putNarrow(FieldNode node)
		{
			for (var i = 0; i < narrowList.Count; i++)
			{
				var item = narrowList[i];
			
				if (node.Phi < item.Phi)
				{
					narrowList.Insert(i, node);
					return;
				}
			}

			narrowList.Add(node);
		}

		FieldNode popNarrow()
		{
			var node = narrowList[0];
			narrowList.RemoveAt(0);	
			return node;
		}

		void removeNarrow(FieldNode node)
		{
			narrowList.Remove(node);
		}

		void calcNeighbors(FieldNode node)
		{
			foreach (int2 dir in dirs)
			{
				var x = node.X + dir.X;
				var y = node.Y + dir.Y;
				if (x < 0 || y < 0 || x >= Width || y >= Height)
					continue;

				var neighbor = Field[x,y];
				if (neighbor.IsFrozen) continue;

				if (neighbor.State == NodeState.narrow)
					removeNarrow(neighbor);
				else
					neighbor.State = NodeState.narrow;

				updateNode(neighbor);
				putNarrow(neighbor);
			}
		}

		internal double F(FieldNode node)
		{
			return CanEnter(new CPos(node.X, node.Y)) ? 1 : double.MaxValue;
		}
		void updateNode(FieldNode node)
		{
			var w = getFrozenNode(node.X-1, node.Y);
			var e = getFrozenNode(node.X+1, node.Y);
			var n = getFrozenNode(node.X, node.Y+1);
			var s = getFrozenNode(node.X, node.Y-1);

			var row = -1d;
			if (w != null && e != null) row = Math.Min(w.Phi, e.Phi);
			else if (w != null) row = w.Phi;
			else if (e != null) row = e.Phi;

			var col = -1d;
			if (n != null && s != null) col = Math.Min(n.Phi, s.Phi);
			else if (n != null) col = n.Phi;
			else if (s != null) col = s.Phi;

			var a = 0d;
			var b = 0d;
			var c = 0d;

			if (row != -1)
			{
				a += 1;
				b -= 2*row;
				c += row*row;
			}

			if (col != -1)
			{
				a += 1;
				b -= 2*col;
				c += col*col;
			}

			var f = F(node);
			c -= f*f;

			var D = Math.Sqrt(b*b - 4*a*c);
			a *= 2;
			var u1 = (-b +D) / a;
			if (D == 0)
				node.Phi = u1;
			else
			{
				var u2 = (-b -D) / a;
				node.Phi = Math.Max(u1, u2);
			}

		}

		public void solve()
		{
			while (narrowList.Count > 0)
			{
				FieldNode node = popNarrow();
				node.State = NodeState.frozen;
				calcNeighbors(node);
			}
		}

		FieldNode getNode(int x, int y)
		{
			if (x < 0 || x >= Width || y < 0 || y >= Height)
				return null;
			return Field[x,y];
		}

		FieldNode getFrozenNode(int x, int y)
		{
			var node = getNode(x,y);
			if (node != null && !node.IsFrozen)
				node = null;
			return node;
		}


		public enum NodeState {frozen, narrow, far}; 
		public class FieldNode
		{
			public int X;
			public int Y;
			public double Phi;
			public NodeState State;
			public FieldNode() {}
			public FieldNode(int x, int y)
			{
				X = x;
				Y = y;
				Phi = float.NaN;
				State = NodeState.far;
			}
			public bool IsFrozen { get { return State == NodeState.frozen; } }
		}
	}



}

