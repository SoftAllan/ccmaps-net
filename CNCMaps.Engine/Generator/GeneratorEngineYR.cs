using CNCMaps.Shared.Generator;
using CNCMaps.Shared;
using NLog;
using CNCMaps.FileFormats.VirtualFileSystem;
using CNCMaps.FileFormats;
using CNCMaps.Engine.Game;
using CNCMaps.FileFormats.Map;
using CNCMaps.Engine.Drawables;

namespace CNCMaps.Engine.Generator {
	internal class GeneratorEngineYR : GeneratorEngine {
		
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		public GeneratorEngineYR(Settings settings) : base(settings, _logger) { }

		const int ClearTile = 0;
		const int WaterTileSingle = 322;
		const int WaterTileLarge = 314; // 4 subtiles.
		
		const byte SeaLevel = 100 ;
		const byte SandLevel = 110;
		const byte HeightInterval = (256 - SandLevel) / 14;


		// Shore Pieces (Set 12) filename: Shore
		// - 42 tiles

		// LAT Grass, thick (Set 13) filename: Ruff
		// - 1 tile
		// GrassThick Idividual (Set 14) filenane: clat
		// - 16 tiles

		// Water (Set 21) filename: Water
		// - 14 tiles

		// LAT Grass Rough (set 33) filename: Sandy
		// - 1 tile
		// GrassRough Individual (set 34) filename: dlat
		// - 16 tiles

		// Pavement (set 38) filename: Pave
		// - 14 tiles
		// Pavement Individual (set 39) filename: plat
		// - 16 tiles

		// LAT Sand (set 41) filename: Green
		// - 1 tile
		// Sand Individual (set 42) filename: glat
		// - 16 tiles

		// subtiles    2 0
		//              3 1
		public override bool GenerateMap() {
			_logger.Info("Starting Yuri's Revenge Map Generator.");

			ParseMapSize(Settings.MapSize);
			_logger.Debug($"Map width={Width} and hight={Height}.");

			/* If using the Theater to define the correct tiles. But for now this is hardcoded to specific values.
			 * Not sure if it is possible to define this dynamically. Or if it is worth the efford.
			 */
			using (var vfs = new VirtualFileSystem()) {
				vfs.Add(VirtualFileSystem.RA2InstallDir);
				vfs.LoadMixes(EngineType.YurisRevenge);
				var modConfig = ModConfig.GetDefaultConfig(EngineType.YurisRevenge);
				var theater = new Theater(Settings.TheaterType, modConfig, vfs, vfs.Open<IniFile>("rulesmd.ini"), vfs.Open<IniFile>("artmd.ini"));
				theater.Initialize();

				InitialiseMapLayer(ClearTile);
				GenerateHeightLayout();
				DefineMapTilesFromHeightLayout(theater);
				DefineWaterSubtiles(theater);
				DefineShoreTiles(theater);
				DefineHillSlope(theater);

				// FillMapTest(theater);

				// TestPerlin();

				// todo: Maybe use the MapFile to wrap the tilelayer into.
				// todo: MapFile can also save.
				// var mapFile = new MapFile()

			}



			return true;
		}


		private void FillMapTest(Theater theater) {
			var cl = theater.GetTileCollection();
			var t = TileLayer.GetTile(5,5);
			t.TileNum = cl.GetTileNumFromSet(13);
			for (int i = 0; i < 16; i++) {
				var x = TileLayer.GetTile(5 + (i * 2), 7);
				x.TileNum = cl.GetTileNumFromSet(14, (byte)i);
			}
			t = TileLayer.GetTile(5,9);
			t.TileNum = cl.GetTileNumFromSet(33);
			for (int i = 0; i < 16; i++) {
				var x = TileLayer.GetTile(5 + (i * 2), 11);
				x.TileNum = cl.GetTileNumFromSet(34, (byte)i);
			}
			t = TileLayer.GetTile(5, 13);
			t.TileNum = cl.GetTileNumFromSet(38);
			for (int i = 0; i < 16; i++) {
				var x = TileLayer.GetTile(5 + (i * 2), 15);
				x.TileNum = cl.GetTileNumFromSet(39, (byte)i);
			}
			t = TileLayer.GetTile(5, 17);
			t.TileNum = cl.GetTileNumFromSet(41);
			for (int i = 0; i < 16; i++) {
				var x = TileLayer.GetTile(5 + (i * 2), 19);
				x.TileNum = cl.GetTileNumFromSet(42, (byte)i);
			}
			t = TileLayer.GetTile(5, 21);
			for (int i = 0; i < 14; i++ ) {
				var x = TileLayer.GetTile(5 + (i * 2), 21);
				x.TileNum = cl.GetTileNumFromSet(21, (byte)i);
			}
		}

		// todo: Maybe just define some constants for the known tiles and subtiles that is going to be used.

		// Shore pieces: 42 tiles
		// #1: Shore TopRight, size 2x2, variant 1 of 3
		// #2: Shore TopRight, size 2x2, variant 2 of 3
		// #3: Shore TopRight, size 2x2, variant 3 of 3
		// #4: Shore TopRight, size 1x2,

		private void DefineMapTilesFromHeightLayout(Theater theater) {
		    IsoTile currentTile;
			_logger.Debug("Defining map tiles from height layout.");
			var cl = theater.GetTileCollection();
			var water = cl.GetTileNumFromSet(cl.WaterSet, 0);
			var sand = cl.GetTileNumFromSet(cl.SandTile, 0);
			for (int y = 0; y < TileLayer.Height; y++) {
				for (int x = 0; x < TileLayer.Width; x++) {
					var h = HeightLayout[y, x];
					currentTile = TileLayer.GetTile(x, y);
					if (h < SeaLevel) {
						currentTile.TileNum = water;
					}
					else if (h < SandLevel) {
						currentTile.TileNum = sand;
					}
					else {
						var hLevel = (byte)((h - SandLevel) / HeightInterval);
						currentTile.Z = hLevel;
					}
				}
			}
		}
		private void DefineWaterSubtiles(Theater theater) {
			// todo: Make 2x2 tiles where possible 
			// WaterTileLarge
		}

		// Make shore tiles instead of sand tiles.
		private void DefineShoreTiles(Theater theater) {
			

		}

		// Make slopes where Z changes.
		private void DefineHillSlope(Theater theater) {
			for (int y = 0; y < TileLayer.Height; y++) {
				for (int x = 0; x < TileLayer.Width; x++) {
					var currentTile = TileLayer.GetTile(x, y);


				}
			}

		}

	}
}
