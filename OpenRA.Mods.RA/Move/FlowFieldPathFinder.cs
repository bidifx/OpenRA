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



		public List<CPos> FindUnitPath(CPos from, CPos target, Actor self)
		{
			var mi = self.Info.Traits.Get<MobileInfo>();
			var mapBounds = world.Map.Bounds;

			var fms = new FastMarchingSolver(world.Map.MapSize.X, world.Map.MapSize.Y);
			fms.CanEnter = (cell) => mi.CanEnterCell(world, self, cell, null, true, false);

			fms.init(target.ToInt2());
			fms.solve();



			/*var length_weigth = 1.0;
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
			Func<CPos, double> c = (cell) => ( length_weigth*v(cell) + time_weight + avoid_weight*a(cell) ) / v(cell);*/

			// get potential of a node, if out of bounds -> "inf" potential wall
			//Func<int, int, double> phi = (x,y) => mapBounds.Contains(x,y) ? field[x,y].Phi : Double.MaxValue ;

		

	
			var dbg = new double[world.Map.MapSize.X, world.Map.MapSize.Y];
			var max = 0d;

			for (int x = 0; x < world.Map.MapSize.X; x++)
				for (int y = 0; y < world.Map.MapSize.Y; y++)
				{					
					dbg[x,y] = fms.Field[x,y].Phi;
					if ( dbg[x,y] < double.MaxValue)
						max = Math.Max(dbg[x,y], max);
				}


			for (int x = 0; x < world.Map.MapSize.X; x++)			
				for (int y = 0; y < world.Map.MapSize.Y; y++)
				{
					if (dbg[x,y] < double.MaxValue)
						dbg[x,y] /= max;										
					else
						dbg[x,y] = 1;
				}

			var overlay = world.WorldActor.TraitOrDefault<FlowFieldOverlay>();
			if (overlay != null)
			{
				overlay.AddFlowField(self, dbg);
			}

			var res = new List<CPos>();


			CPos[] dirs = {new CPos(-1,0), new CPos(+1,0), new CPos(0,-1), new CPos(0,+1),
			new CPos(+1,+1), new CPos(-1,+1), new CPos(-1,-1), new CPos(+1,-1)};
			var pos = new CPos(from.X, from.Y);
			while (pos != target)
			{
				var min = double.MaxValue;
				CPos next;
				foreach (var dir in dirs)
				{
					var tmp = new CPos(pos.X+dir.X, pos.Y+dir.Y);
					if (!world.Map.Bounds.Contains(tmp.ToInt2())) continue;
					if (fms.Field[tmp.X,tmp.Y].Phi < min)
					{
						min = fms.Field[tmp.X,tmp.Y].Phi;
						next = tmp;
					}
				}
				if (res.Contains(next))
				{
					Console.WriteLine("No way found, looping(?)");
					break;
				}

				res.Add(next);
				pos = next;
			}
			res.Reverse();

			return res;
		}
	}
	
}
