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


			Bitmap bitmap = new Bitmap(200, 200);
			/* Test of bitmap creation.
			var rnd = new Random();
			for (int x = 0; x < 100; x++) {
				for (int y = 0; y < 100; y++) {
					bitmap.SetPixel(x, y, Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256)));
				}
			}
			*/

			var noise = new PerlinNoise(1234);

			for (int x = 0; x < 200; x++) {
				for (int y = 0; y < 200; y++) {
					var dx = (double)x;
					var dy = (double)y;
					var n = noise.Noise(dx * 0.05, dy * 0.05, 0);
					var positive = n + 1;
					var large = (int)(positive * 128);
					var c = Color.FromArgb(large, large, large);	// gray scale.
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
