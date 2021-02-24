using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CNCMaps.Shared.Generator;
using CNCMaps.Shared;
using NLog;
using CNCMaps.FileFormats.Map;
using CNCMaps.FileFormats.VirtualFileSystem;
using System.IO;
using CNCMaps.FileFormats;
using CNCMaps.Engine.Game;
using System.Drawing;
using CNCMaps.Engine.Utility;

namespace CNCMaps.Engine.Generator {
	internal class GeneratorEngineYR : GeneratorEngine {
		
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		public GeneratorEngineYR(Settings settings) : base(settings) { }

		public override bool GenerateMap() {
			_logger.Info("Starting Yuri's Revenge Map Generator.");

			ParseMapSize(Settings.MapSize);
			_logger.Debug($"Map width={Width} and hight={Height}.");

			using (var vfs = new VirtualFileSystem()) {
				vfs.Add(VirtualFileSystem.RA2InstallDir);
				vfs.LoadMixes(EngineType.YurisRevenge);
				var modConfig = ModConfig.GetDefaultConfig(EngineType.YurisRevenge);
				var theater = new Theater(Settings.TheaterType, modConfig, vfs, vfs.Open<IniFile>("rulesmd.ini"), vfs.Open<IniFile>("artmd.ini"));
				theater.Initialize();

				InitialiseMapLayer(theater.GetTileCollection().ClearTile);


				// todo: Maybe use the MapFile to wrap the tilelayer into.
				// todo: MapFile can also save.
				// var mapFile = new MapFile()

			}


			Bitmap bitmap = new Bitmap(300, 300);

			var noise = new PerlinNoise(222);
			var c = Color.Empty;
			var nv = 0d;
			var h = 0;

			for (int x = 0; x < bitmap.Width; x++) {
				for (int y = 0; y < bitmap.Height; y++) {
					nv = noise.Noise(x * 0.006d, y * 0.006d, 0d) + 1d;
					h = (int)(nv * 128);
					if (h < 100)
						c = Color.FromArgb(0, 0, h + 80);   // water
					else if (h < 120)
						c = Color.FromArgb(300 - h, 300 - h, 0);		// sand
					else 
						c = Color.FromArgb(0, h, 0);		// grass.
					bitmap.SetPixel(x, y, c);
				}
			}

			var debugView = new DebugGeneratorEngine();
			debugView.Canvas.Image = bitmap;
			debugView.ShowDialog();


			return true;
		}

	}
}
