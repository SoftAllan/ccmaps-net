using CNCMaps.Shared.Generator;
using CNCMaps.Shared;
using NLog;
using CNCMaps.FileFormats.VirtualFileSystem;
using CNCMaps.FileFormats;
using CNCMaps.Engine.Game;
using CNCMaps.Engine.Generator.Map;
using System;

namespace CNCMaps.Engine.Generator {
	public class GeneratorEngineYR : GeneratorEngine {

		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		public GeneratorEngineYR(Settings settings) : base(settings, _logger) { }

		/* Should be using the Theater to define the correct tiles. But for now this is hardcoded to specific values.
		 * Not sure if it is possible to define this dynamically. Or if it is worth the efford.
		 */
		public const ushort ClearTile = 0;
		public const ushort WaterTileSingle = 322;
		public const ushort WaterTileLarge = 314; // 4 subtiles.
		// Center = Subtile 0
		// BottomRight = Subtile 1
		// BottomLeft = Subtile 2
		// Bottom = Subtile 3

		public const ushort SandTileSingle = 418;

		/* Ramps (all 1 subtile = 0)
		 * TL = Top Left
		 * BR = Bottom Right
		 * TR = Top Right
		 * BL = Bottom Left
		 * T = Top
		 * B = Bottom
		 * L = Left
		 * R = Right
		 * 0 = Level 0
		 * 1 = Level 1
		 * C = Connection side
		 * RampTL0BR1 => Ramp has Top Left at level 0 and Bottom Right at level 1
		 */

		// 1 level ramps
		public const ushort RampTL0BR1 = 29;
		public const ushort RampTR0BL1 = 30;
		public const ushort RampTL1BR0 = 31;
		public const ushort RampTR1BL0 = 32;

		public const ushort RampT0BC1 = 33;
		public const ushort RampLC1R0 = 34;
		public const ushort RampTC1B0 = 35;
		public const ushort RampL0RC1 = 36;
		
		public const ushort RampTC0B1 = 37;
		public const ushort RampL1RC0 = 38;
		public const ushort RampT1BC0 = 39;
		public const ushort RampLC0R1 = 40;

		// the other ramps are mostly for rasing 2 levels.
		

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

			InitialiseMapLayer(ClearTile);
			// 0.04 large hills
			// 0.20 many hills
			GenerateHeightLayout(0.04d, debug: false);
			DefineZFromHeightLayout();
			LevelOut();
			DefineWaterSubtiles();
			DefineSandTiles();
			DefineShoreTiles();
			DefineRampTiles();

			return true;
		}


		/*
		private void FillMapTest() {
			//using (var vfs = new VirtualFileSystem()) {
			//vfs.Add(VirtualFileSystem.RA2InstallDir);
			//vfs.LoadMixes(EngineType.YurisRevenge);
			//var modConfig = ModConfig.GetDefaultConfig(EngineType.YurisRevenge);
			//var theater = new Theater(Settings.TheaterType, modConfig, vfs, vfs.Open<IniFile>("rulesmd.ini"), vfs.Open<IniFile>("artmd.ini"));
			//theater.Initialize();




			// TestPerlin();

			// todo: Maybe use the MapFile to wrap the tilelayer into.
			// todo: MapFile can also save.
			// var mapFile = new MapFile()

			//}
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
		*/


		// Shore pieces: 42 tiles
		// #1: Shore TopRight, size 2x2, variant 1 of 3
		// #2: Shore TopRight, size 2x2, variant 2 of 3
		// #3: Shore TopRight, size 2x2, variant 3 of 3
		// #4: Shore TopRight, size 1x2,


		private void DefineWaterSubtiles() {
			_logger.Debug("Defining water tiles.");
			// todo: Make 2x2 tiles where possible 
			// WaterTileLarge
			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width * 2 - 1; x++) {
					if (TileLayer[x, y].Ground == IsoTile.GroundType.Water)
						TileLayer[x, y].TileNum = WaterTileSingle;
				}
			}
		}

		private void DefineSandTiles() {
			_logger.Debug("Defining sand tiles.");
			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width * 2 - 1; x++) {
					if (TileLayer[x, y].Ground == IsoTile.GroundType.Sand)
						TileLayer[x, y].TileNum = SandTileSingle;
				}
			}
		}

		// Make shore tiles instead of sand tiles.
		private void DefineShoreTiles() {
			_logger.Debug("Defining shore tiles.");
		}

		private void DefineRampTiles() {
			_logger.Debug("Defining sand tiles.");
			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width * 2 - 1; x++) {
					DefineRampTile(x, y);
				}
			}
		}

		public void DefineRampTile(int x, int y) {
			var ct = TileLayer[x, y];
			if (TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft).Z > ct.Z)
				if (TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomLeft).Z > ct.Z) {
					ct.TileNum = RampL1RC0;
					return;
				}
			if (TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft).Z > ct.Z)
				if (TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight).Z > ct.Z) {
					ct.TileNum = RampT1BC0;
					return;
				}
			if (TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomRight).Z > ct.Z)
				if (TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight).Z > ct.Z) {
					ct.TileNum = RampLC0R1;
					return;
				}
			if (TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomRight).Z > ct.Z)
				if (TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomLeft).Z > ct.Z) {
					ct.TileNum = RampTC0B1;
					return;
				}
			if (TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft).Z > ct.Z) {
				ct.TileNum = RampTL1BR0;
				return;
			}
			if (TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight).Z > ct.Z) {
				ct.TileNum = RampTR1BL0;
				return;
			}
			if (TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomRight).Z > ct.Z) {
				ct.TileNum = RampTL0BR1;
				return;
			}
			if (TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomLeft).Z > ct.Z) {
				ct.TileNum = RampTR0BL1;
				return;
			}
			if (TileLayer.GridTile(x, y, TileLayer.TileDirection.Left).Z > ct.Z) {
				ct.TileNum = RampLC1R0;
				return;
			}
			if (TileLayer.GridTile(x, y, TileLayer.TileDirection.Top).Z > ct.Z) {
				ct.TileNum = RampTC1B0;
				return;
			}
			if (TileLayer.GridTile(x, y, TileLayer.TileDirection.Right).Z > ct.Z) {
				ct.TileNum = RampL0RC1;
				return;
			}
			if (TileLayer.GridTile(x, y, TileLayer.TileDirection.Bottom).Z > ct.Z) {
				ct.TileNum = RampT0BC1;
				return;
			}
		}
	}
}
