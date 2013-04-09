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

			var field = new double[world.Map.MapSize.X, world.Map.MapSize.Y];


			var length_weigth = 1.0;
			var time_weight = 1.0;
			var avoid_weight = 1.0;

			Func<CPos,double> v = (_) => 1.0;
			Func<CPos,double> a = (cell) => mi.CanEnterCell(world, self, cell, null, false, false) ? 0 : 1;

			var max = 0d;

			for (int x = 0; x < world.Map.MapSize.X; x++)
			{
				for (int y = 0; y < world.Map.MapSize.Y; y++)
				{
					var cell = new CPos(x,y);
					var c = ( length_weigth*v(cell) + time_weight + avoid_weight*a(cell) ) / v(cell);

					c  *= Math.Sqrt(Math.Pow(from.X-x,2) + Math.Pow((from.Y-y),2));
					if (c > max) max = c;
					field[x,y] = c;
				}
			}




			for (int x = 0; x < world.Map.MapSize.X; x++)			
				for (int y = 0; y < world.Map.MapSize.Y; y++)
					field[x,y] /= max;



			var res = new List<CPos>();

			/*
			var x = from.X;
			var y = from.Y;

			res.Add(new CPos(from.X,from.Y));
			while (x != target.X || y != target.Y)
			{
				if (x > target.X) x--;
				if (x < target.X) x++;

				if (y > target.Y) y--;
				if (y < target.Y) y++;
				res.Add(new CPos(x,y));

			}

*/

			var overlay = world.WorldActor.TraitOrDefault<FlowFieldOverlay>();
			if (overlay != null)
			{
				overlay.AddFlowField(self, field);
			}
//			res.Reverse();
			return res;
		}
	}
	
}
