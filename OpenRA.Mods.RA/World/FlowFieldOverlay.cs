using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using OpenRA.FileFormats;
using OpenRA.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.RA
{
	class FlowFieldOverlayInfo : Traits.TraitInfo<FlowFieldOverlay>
	{
	}

	class FlowFieldOverlay : IRenderOverlay, IWorldLoaded
	{
		//Dictionary<Player, int[,]> layers;
		int refreshTick;
		World world;
		public bool Visible;


		List<List<CPos>> regions;
		Color[] colors = {Color.Green, Color.Blue, Color.Black, Color.Purple, Color.Yellow, Color.Red, Color.Fuchsia, Color.Azure, Color.White, Color.Coral};
		public void WorldLoaded(World w)
		{
			this.world = w;
			this.refreshTick = 0;
			this.regions = new List<List<CPos>>();

			//this.layers = new Dictionary<Player, int[,]>(8);
			// Enabled via Cheats menu
			this.Visible = false;
		}

		/*public void AddLayer(IEnumerable<Pair<CPos, int>> cellWeights, int maxWeight, Player pl)
		{
			if (maxWeight == 0) return;

			int[,] layer;
			if (!layers.TryGetValue(pl, out layer))
			{
				layer = new int[world.Map.MapSize.X, world.Map.MapSize.Y];
				layers.Add(pl, layer);
			}

			foreach (var p in cellWeights)
				layer[p.First.X, p.First.Y] = Math.Min(128, (layer[p.First.X, p.First.Y]) + ((maxWeight - p.Second) * 64 / maxWeight));
		}*/

		public void AddRegion(List<CPos> region)
		{
			regions.Add(region);
		}

		public void Render(WorldRenderer wr)
		{
			if (!Visible) return;

			var qr = Game.Renderer.WorldQuadRenderer;
			bool doDim = refreshTick - world.FrameNumber <= 0;
			if (doDim) refreshTick = world.FrameNumber + 20;

			var viewBounds = Game.viewport.WorldBounds(world);
			var mapBounds = world.Map.Bounds;

			var col = 0;
			foreach (var region in regions)
			{
				//Console.WriteLine("Render region: {0}".F(region.Count));
				if (col>=colors.Length) col = 0;
				Color color = colors[col];


				foreach (var cell in region)
				{
					if (!viewBounds.Contains(cell.X, cell.Y))
						continue;

					var ploc = cell.ToPPos();
					qr.FillRect(new RectangleF(ploc.X, ploc.Y, Game.CellSize, Game.CellSize), Color.FromArgb(128, color));				
				}

				col++;
			}

			/*foreach (var pair in layers)
			{
				Color c = (pair.Key != null) ? pair.Key.ColorRamp.GetColor(0f) : Color.PaleTurquoise;
				var layer = pair.Value;

				for (int j = mapBounds.Top; j <= mapBounds.Bottom; ++j)
					for (int i = mapBounds.Left; i <= mapBounds.Right; ++i)
					{
						if (layer[i, j] <= 0) continue;

						var w = Math.Max(0, Math.Min(layer[i, j], 128));
						if (doDim)
						{
							layer[i, j] = layer[i, j] * 5 / 6;
						}

						if (!viewBounds.Contains(i, j)) continue;

						// Only render quads in viewing range:
						var ploc = new CPos(i, j).ToPPos();
						qr.FillRect(new RectangleF(ploc.X, ploc.Y, Game.CellSize, Game.CellSize), Color.FromArgb(w, c));
					}
			}*/
		}
	}
}
