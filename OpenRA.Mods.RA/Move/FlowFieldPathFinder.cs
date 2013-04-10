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
using System.Diagnostics;
using System.Linq;
using OpenRA.FileFormats;
using OpenRA.Support;
using OpenRA.Traits;

namespace OpenRA.Mods.RA.Move
{
	public class FlowFieldPathFinderInfo : ITraitInfo
	{
		public object Create(ActorInitializer init) { return new FlowFieldPathFinder(init.world); }
	}

	public class FlowFieldPathFinder
	{
		readonly World world;
		public FlowFieldPathFinder(World world) { this.world = world; }

		/*class CachedPath
		{
			public CPos from;
			public CPos to;
			public List<CPos> result;
			public int tick;
			public Actor actor;
		}

		List<CachedPath> CachedPaths = new List<CachedPath>();
		const int MaxPathAge = 50;	 x 40ms ticks */

		class FieldNode
		{
			public int X;
			public int Y;
			public double Phi;
			public FieldNode() {}
			public FieldNode(int x, int y, double phi)
			{
				X = x;
				Y = y;
				Phi = phi;
			}
		}

		public List<CPos> FindUnitPath(CPos from, CPos target, Actor self)
		{
			var mi = self.Info.Traits.Get<MobileInfo>();
			var mapBounds = world.Map.Bounds;

			CPos[] dirs = {new CPos(-1,0), new CPos(+1,0), new CPos(0,-1), new CPos(0,+1) };


			var length_weigth = 1.0;
			var time_weight = 1.0;
			var avoid_weight = 10.0;



			var behindWavefront = new List<FieldNode>();
			var atWavefront = new List<FieldNode>();
			var field = new FieldNode[world.Map.MapSize.X, world.Map.MapSize.Y];
			for (int x = 0; x < world.Map.MapSize.X; x++)			
				for (int y = 0; y < world.Map.MapSize.Y; y++)
					field[x,y] = new FieldNode(x,y,0);

			Func<CPos,double> v = (_) => 1.0;
			Func<CPos,double> a = (cell) => mi.CanEnterCell(world, self, cell, null, false, false) ? 0 : 1;
			Func<CPos, double> c = (cell) => ( length_weigth*v(cell) + time_weight + avoid_weight*a(cell) ) / v(cell);

			// get potential of a node, if out of bounds -> "inf" potential wall
			Func<int, int, double> phi = (x,y) => mapBounds.Contains(x,y) ? field[x,y].Phi : Double.MaxValue ;

			atWavefront.Add(field[target.X, target.Y]);

			while (atWavefront.Count > 0)
			{
				Console.WriteLine("atWavefront.Count: {0}".F(atWavefront.Count));
				// Node with smallest Phi
				var node = atWavefront.First();
				foreach (var n in atWavefront)
					if (n.Phi < node.Phi)
						node = n;

				Console.WriteLine("Node X:{0} Y:{1} Phi:{2}".F(node.X, node.Y, node.Phi));

				atWavefront.Remove(node);
				behindWavefront.Add(node);

				// Find neighbors, advance wavefront
				foreach (var dir in dirs)
				{
					var x = node.X + dir.X;
					var y = node.Y + dir.Y;
					if (!mapBounds.Contains(x,y)) continue;
					var neighbor = field[x,y];
					if (behindWavefront.Contains(neighbor)) continue;
					if (!atWavefront.Contains(neighbor)) atWavefront.Add(neighbor);

					// Calc neighbor's Phi with FDM

					var phi_ = phi(neighbor.X, neighbor.Y);
					var phi_x_up = phi(neighbor.X+1, neighbor.Y);
					var phi_x_dn = phi(neighbor.X-1, neighbor.Y);
					var phi_y_up = phi(neighbor.X, neighbor.Y+1);
					var phi_y_dn = phi(neighbor.X, neighbor.Y-1);

					// actually this gives Phi^2, but doesn't matter....
					neighbor.Phi = 
						Math.Pow(
									Math.Max( 
						         				Math.Max(phi_ - phi_x_dn,0),
						         				-Math.Min(phi_x_up - phi_,0)
						         			)
							,2) + 
						Math.Pow(
									Math.Max(
												Math.Max(phi_ - phi_y_dn,0),
												-Math.Min(phi_y_up - phi_,0)
											)
							,2);

				}

			}







			var dbg = new double[world.Map.MapSize.X, world.Map.MapSize.Y];
			var max = 0d;

			for (int x = 0; x < world.Map.MapSize.X; x++)
				for (int y = 0; y < world.Map.MapSize.Y; y++)
					dbg[x,y] = field[x,y].Phi;


			for (int x = 0; x < world.Map.MapSize.X; x++)			
				for (int y = 0; y < world.Map.MapSize.Y; y++)
					dbg[x,y] /= max;



			var res = new List<CPos>();

			var overlay = world.WorldActor.TraitOrDefault<FlowFieldOverlay>();
			if (overlay != null)
			{
				overlay.AddFlowField(self, dbg);
			}
//			res.Reverse();
			return res;
		}
	}
	
}
