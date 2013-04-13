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

		Dictionary<Actor, double[,]> flowFields;

		public void WorldLoaded(World w)
		{
			this.world = w;
			this.refreshTick = 0;
			this.flowFields = new Dictionary<Actor, double[,]>();

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

		public void AddFlowField(Actor self, double[,] flowField)
		{
			flowFields[self] = flowField;
		}

		public void Render(WorldRenderer wr)
		{
			if (!Visible) return;
			if (world.Selection.Actors.Count() != 1) return;
			if (!flowFields.ContainsKey(world.Selection.Actors.First())) return;

			var flowField = flowFields[world.Selection.Actors.First()];

			var qr = Game.Renderer.WorldQuadRenderer;
			bool doDim = refreshTick - world.FrameNumber <= 0;
			if (doDim) refreshTick = world.FrameNumber + 20;

			var viewBounds = Game.viewport.WorldBounds(world);
			var mapBounds = world.Map.Bounds;


			for (var x = 0; x < flowField.GetLength(0); x++)
			{
				for (var y = 0; y < flowField.GetLength(1); y++)
				{

					var cell = new CPos(x,y);
					if (!viewBounds.Contains(cell.X, cell.Y))
						continue;

					var value = flowField[x,y];
					var w = Math.Max(0, Math.Min((int)(value*200), 200));
					var ploc = cell.ToPPos();
					qr.FillRect(new RectangleF(ploc.X, ploc.Y, Game.CellSize, Game.CellSize), Color.FromArgb(w, Color.Red));				
				}
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
