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
			var x = from.X;
			var y = from.Y;
			var res = new List<CPos>();

			res.Add(new CPos(from.X,from.Y));
			while (x != target.X || y != target.Y)
			{
				if (x > target.X) x--;
				if (x < target.X) x++;

				if (y > target.Y) y--;
				if (y < target.Y) y++;
				res.Add(new CPos(x,y));

			}




			res.Reverse();
			return res;
		}
	}
	
}
