using CNCMaps.Shared.Generator;
using CNCMaps.Shared;
using NLog;
using CNCMaps.FileFormats.VirtualFileSystem;
using CNCMaps.FileFormats;
using CNCMaps.Engine.Game;
using CNCMaps.Engine.Generator.Map;

namespace CNCMaps.Engine.Generator {
	public class GeneratorEngineYR : GeneratorEngine {

		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		public GeneratorEngineYR(Settings settings) : base(settings, _logger) { }

		public const int ClearTile = 0;
		public const int WaterTileSingle = 322;
		public const int WaterTileLarge = 314; // 4 subtiles.
		public const int SandTileSingle = 418;

		public const byte SeaLevel = 100 ;
		public const byte SandLevel = 110;
		public const byte HeightInterval = (256 - SandLevel) / 14;


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
				// 0.04 large hills
				// 0.20 many hills
				GenerateHeightLayout(0.04d, debug: true); 
				DefineMapTilesFromHeightLayout(theater);
				LevelOut();
				DefineWaterSubtiles(theater);
				DefineShoreTiles(theater);
				CheckForPitAndSpike(theater);



				TileLayer.DumpZToFile();

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
			for (int i = 0; i < 14; i++) {
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

		public void DefineMapTilesFromHeightLayout(Theater theater) {
			IsoTile currentTile;
			_logger.Debug("Defining map tiles from height layout.");
			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width * 2 - 1; x++) {
					var h = HeightLayout[x, y];
					currentTile = TileLayer[x, y];
					if (h < SeaLevel) {
						currentTile.TileNum = WaterTileSingle;	// todo: change to single. Large tiles are fixed later.
						currentTile.Z = 0;
					}
					else if (h < SandLevel) {
						currentTile.TileNum = SandTileSingle;
						currentTile.Z = 0;
					}
					else {
						var hLevel = (byte)((h - SandLevel) / HeightInterval);
						currentTile.Z = hLevel;
					}
				}
			}
		}

		// Make sure that neighbour z is not jumping more than 1.
		// Control against top, then left.
		// Sea and sand level might be raised.
		public void LevelOut() {
			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width * 2 - 1; x++) {
					CheckLevel(x, y);
					CheckWaterOrSandLevel(x, y);
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

		// Check for pits and spikes.
		private void CheckForPitAndSpike(Theater theater) {
			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width * 2 - 1; x++) {
					CheckPit(x, y);
					CheckSpike(x, y);
				}
			}
		}

		// Remove small holes.
		public void CheckPit(int x, int y) {
			var ct = TileLayer[x, y];
			if (TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft).Z - 1 == ct.Z &&
				TileLayer.GridTile(x, y, TileLayer.TileDirection.Top).Z - 1 == ct.Z && 
				TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight).Z - 1 == ct.Z &&
				TileLayer.GridTile(x, y, TileLayer.TileDirection.Left).Z - 1 == ct.Z &&
				TileLayer.GridTile(x, y, TileLayer.TileDirection.Right).Z - 1 == ct.Z &&
				TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomLeft).Z - 1 == ct.Z &&
				TileLayer.GridTile(x, y, TileLayer.TileDirection.Bottom).Z - 1 == ct.Z &&
				TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomRight).Z - 1 == ct.Z) {
					ct.Z++;
			}
		}

		// Removes small spikes
		public void CheckSpike(int x, int y) {
			var ct = TileLayer[x, y];
			if (TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft).Z + 1 == ct.Z &&
				TileLayer.GridTile(x, y, TileLayer.TileDirection.Top).Z + 1 == ct.Z &&
				TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight).Z + 1 == ct.Z &&
				TileLayer.GridTile(x, y, TileLayer.TileDirection.Left).Z + 1 == ct.Z &&
				TileLayer.GridTile(x, y, TileLayer.TileDirection.Right).Z + 1 == ct.Z &&
				TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomLeft).Z + 1 == ct.Z &&
				TileLayer.GridTile(x, y, TileLayer.TileDirection.Bottom).Z + 1 == ct.Z &&
				TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomRight).Z + 1 == ct.Z) {
				ct.Z--;
			}
		}

		// Check the level of the tile[x, y].
		// If it is more that +/1 against either the topleft, top or the left tile it is corrected.
		public void CheckLevel(int x, int y) {
			var ct = TileLayer[x, y];
			CheckTileLevel(ct, TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft));
			CheckTileLevel(ct, TileLayer.GridTile(x, y, TileLayer.TileDirection.Top));
			CheckTileLevel(ct, TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight));
			CheckTileLevel(ct, TileLayer.GridTile(x, y, TileLayer.TileDirection.Left));
		}

		// Check the current tile against the validated tile.
		// Returns true if the current tile has been altered.
		public bool CheckTileLevel(IsoTile current, IsoTile validated, bool correctLevel = true) {
			if (validated.TileNum != -1) {
				if (validated.Z - 1 > current.Z) {
					if (correctLevel) 
						current.Z = (byte)(validated.Z - 1);
					return true;
				}
				if (validated.Z + 1 < current.Z) {
					if (correctLevel)
						current.Z = (byte)(validated.Z + 1);
					return true;
				}
			}
			return false;
		}

		public void CheckWaterOrSandLevel(int x, int y) {
			var ct = TileLayer[x, y];
			if (CheckTileWaterlineLevel(ct, TileLayer.GridTile(x, y, TileLayer.TileDirection.Top))) return;
			CheckTileWaterlineLevel(ct, TileLayer.GridTile(x, y, TileLayer.TileDirection.Left));
		}

		// Checks the current tile against the validated waterline. If the validated tile is a water or sand tile
		// the level is ajusted.
		// Returns true if the current tile has been altered.
		private bool CheckTileWaterlineLevel(IsoTile current, IsoTile validated) {
			if (current.TileNum == WaterTileSingle || current.TileNum == SandTileSingle) {
				if (validated.TileNum == WaterTileSingle || validated.TileNum == SandTileSingle) {
					if (validated.Z != current.Z) {
						current.Z = validated.Z;
						return true;
					}
				}
			}
			return false;
		}

	}
}
