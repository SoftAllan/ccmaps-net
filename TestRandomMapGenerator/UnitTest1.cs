using CNCMaps.Engine.Game;
using CNCMaps.Engine.Generator;
using CNCMaps.Engine.Generator.Map;
using CNCMaps.Engine.Utility;
using CNCMaps.FileFormats;
using CNCMaps.FileFormats.VirtualFileSystem;
using CNCMaps.Shared;
using CNCMaps.Shared.Generator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TestRandomMapGenerator
{
    [TestClass]
    public class UnitTest1
    {
		public Settings TestSettings { get; set; }
		public Theater TestTheater { get; set; }

		[TestInitialize]
		public void Setup() {
			// A test theater is setup for all testing methods as this takes a long time to do.
			TestSettings = new Settings();
			TestSettings.TheaterType = TheaterType.Temperate;
			TestTheater = CreateTheater();
		}
		
		private Theater CreateTheater() {
			var vfs = new VirtualFileSystem();
			vfs.Add(VirtualFileSystem.RA2InstallDir);
			vfs.LoadMixes(EngineType.YurisRevenge);
			var modConfig = ModConfig.GetDefaultConfig(EngineType.YurisRevenge);
			var theater = new Theater(TestSettings.TheaterType, modConfig, vfs, vfs.Open<IniFile>("rulesmd.ini"), vfs.Open<IniFile>("artmd.ini"));
			theater.Initialize();
			return theater;
		}

		private GeneratorEngineYR NewTestGeneratorEngineYR(int x, int y) {
			var newEngine = new GeneratorEngineYR(TestSettings);
			newEngine.Width = (ushort)x;
			newEngine.Height = (ushort)y;
			newEngine.InitialiseMapLayer(0);
			return newEngine;
		}

		[TestMethod]
		public void TestLevelOutLowTop() {
			var te = NewTestGeneratorEngineYR(3, 3);
			int x = 1;
			int y = 1;
			te.TileLayer[0, 0].Z = 1;
			te.TileLayer[1, 0].Z = 1;
			te.TileLayer[2, 0].Z = 1;
			te.TileLayer[0, 1].Z = 1;
			te.TileLayer[1, 1].Z = 3;
			te.TileLayer[2, 1].Z = 1;
			// [3, y] and [4, y] is not used in this test.
			// [x, 2] is not used in this test.
			te.CheckLevel(x, y);
			Assert.AreEqual(2, te.TileLayer[x, y].Z);
		}

		[TestMethod]
		public void TestLevelOutHighTop() {
			var te = NewTestGeneratorEngineYR(3, 3);
			int x = 1;
			int y = 1;
			te.TileLayer[0, 0].Z = 4;
			te.TileLayer[1, 0].Z = 4;
			te.TileLayer[2, 0].Z = 4;
			te.TileLayer[0, 1].Z = 4;
			te.TileLayer[1, 1].Z = 0;
			te.TileLayer[2, 1].Z = 4;
			// [3, y] and [4, y] is not used in this test.
			// [x, 2] is not used in this test.
			te.CheckLevel(x, y);
			Assert.AreEqual(3, te.TileLayer[x, y].Z);
		}

		[TestMethod]
		public void TestLevelOutLowLeft() {
			var te = NewTestGeneratorEngineYR(3, 3);
			int x = 1;
			int y = 1;
			te.TileLayer[0, 0].Z = 6;
			te.TileLayer[1, 0].Z = 6;
			te.TileLayer[2, 0].Z = 6;
			te.TileLayer[0, 1].Z = 6;
			te.TileLayer[1, 1].Z = 9;
			te.TileLayer[2, 1].Z = 6;
			// [3, y] and [4, y] is not used in this test.
			// [x, 2] is not used in this test.
			te.CheckLevel(x, y);
			Assert.AreEqual(7, te.TileLayer[x, y].Z);
		}

		[TestMethod]
		public void TestLevelOutHighLeft() {
			var te = NewTestGeneratorEngineYR(3, 3);
			int x = 1;
			int y = 1;
			te.TileLayer[0, 0].Z = 6;
			te.TileLayer[1, 0].Z = 6;
			te.TileLayer[2, 0].Z = 6;
			te.TileLayer[0, 1].Z = 6;
			te.TileLayer[1, 1].Z = 1;
			te.TileLayer[2, 1].Z = 6;
			// [3, y] and [4, y] is not used in this test.
			// [x, 2] is not used in this test.
			te.CheckLevel(x, y);
			Assert.AreEqual(5, te.TileLayer[x, y].Z);
		}

		[TestMethod]
		public void TestLevelWater() {
			var te = NewTestGeneratorEngineYR(5, 5);
			int x = 3;
			int y = 3;
			te.TileLayer[2, 1].Z = 4;
			te.TileLayer[3, 1].Z = 3;
			te.TileLayer[4, 1].Z = 4;
			te.TileLayer[2, 2].Z = 2;
			te.TileLayer[3, 2].Ground = IsoTile.GroundType.Water;
			te.TileLayer[4, 2].Z = 1;
			te.TileLayer[2, 3].Z = 1;
			te.TileLayer[3, 3].Z = 1;
			te.TileLayer[4, 3].Z = 1;
			te.TileLayer.DumpZToFile();
			
		}

		//[TestMethod]
		//public void TestLevelWaterLeft() {
		//	var te = NewTestGeneratorEngineYR(3, 3);
		//	int x = 1;
		//	int y = 1;
		//	te.TileLayer[0, 0].Z = 1;
		//	te.TileLayer[1, 0].Z = 1;
		//	te.TileLayer[2, 0].Z = 1;
		//	te.TileLayer[0, 1].Z = 1;
		//	te.TileLayer[0, 1].TileNum = GeneratorEngineYR.WaterTileSingle;
		//	te.TileLayer[1, 1].Z = 0;
		//	te.TileLayer[1, 1].TileNum = GeneratorEngineYR.WaterTileSingle;
		//	te.TileLayer[2, 1].Z = 0;
		//	// [3, y] and [4, y] is not used in this test.
		//	// [x, 2] is not used in this test.
		//	te.CheckWaterOrSandLevel(x, y);
		//	Assert.AreEqual(1, te.TileLayer[x, y].Z);
		//	te.TileLayer[0, 1].Z = 1;
		//	te.TileLayer[0, 1].TileNum = GeneratorEngineYR.WaterTileSingle;
		//	te.TileLayer[1, 1].Z = 0;
		//	te.TileLayer[1, 1].TileNum = 0;
		//	te.CheckWaterOrSandLevel(x, y);
		//	Assert.AreEqual(0, te.TileLayer[x, y].Z);
		//	te.TileLayer[0, 1].Z = 1;
		//	te.TileLayer[0, 1].TileNum = 0;
		//	te.TileLayer[1, 1].Z = 0;
		//	te.TileLayer[1, 1].TileNum = GeneratorEngineYR.WaterTileSingle;
		//	te.CheckWaterOrSandLevel(x, y);
		//	Assert.AreEqual(0, te.TileLayer[x, y].Z);
		//}

		//[TestMethod]
		//public void TestLevelSandTop() {
		//	var te = NewTestGeneratorEngineYR(3, 3);
		//	int x = 1;
		//	int y = 1;
		//	te.TileLayer[0, 0].Z = 1;
		//	te.TileLayer[1, 0].Z = 1;
		//	te.TileLayer[1, 0].TileNum = GeneratorEngineYR.SandTileSingle;
		//	te.TileLayer[2, 0].Z = 1;
		//	te.TileLayer[0, 1].Z = 0;
		//	te.TileLayer[1, 1].Z = 0;
		//	te.TileLayer[1, 1].TileNum = GeneratorEngineYR.SandTileSingle;
		//	te.TileLayer[2, 1].Z = 0;
		//	// [3, y] and [4, y] is not used in this test.
		//	// [x, 2] is not used in this test.
		//	te.CheckWaterOrSandLevel(x, y);
		//	Assert.AreEqual(1, te.TileLayer[x, y].Z);
		//	te.TileLayer[1, 0].Z = 1;
		//	te.TileLayer[1, 0].TileNum = GeneratorEngineYR.SandTileSingle;
		//	te.TileLayer[1, 1].Z = 0;
		//	te.TileLayer[1, 1].TileNum = 0;
		//	te.CheckWaterOrSandLevel(x, y);
		//	Assert.AreEqual(0, te.TileLayer[x, y].Z);
		//	te.TileLayer[1, 0].Z = 1;
		//	te.TileLayer[1, 0].TileNum = 0;
		//	te.TileLayer[1, 1].Z = 0;
		//	te.TileLayer[1, 1].TileNum = GeneratorEngineYR.SandTileSingle;
		//	te.CheckWaterOrSandLevel(x, y);
		//	Assert.AreEqual(0, te.TileLayer[x, y].Z);
		//}

		//[TestMethod]
		//public void TestLevelSandLeft() {
		//	var te = NewTestGeneratorEngineYR(3, 3);
		//	int x = 1;
		//	int y = 1;
		//	te.TileLayer[0, 0].Z = 1;
		//	te.TileLayer[1, 0].Z = 1;
		//	te.TileLayer[2, 0].Z = 1;
		//	te.TileLayer[0, 1].Z = 1;
		//	te.TileLayer[0, 1].TileNum = GeneratorEngineYR.SandTileSingle;
		//	te.TileLayer[1, 1].Z = 0;
		//	te.TileLayer[1, 1].TileNum = GeneratorEngineYR.WaterTileSingle;
		//	te.TileLayer[2, 1].Z = 0;
		//	// [3, y] and [4, y] is not used in this test.
		//	// [x, 2] is not used in this test.
		//	te.CheckWaterOrSandLevel(x, y);
		//	Assert.AreEqual(1, te.TileLayer[x, y].Z);
		//	te.TileLayer[0, 1].Z = 1;
		//	te.TileLayer[0, 1].TileNum = GeneratorEngineYR.SandTileSingle;
		//	te.TileLayer[1, 1].Z = 0;
		//	te.TileLayer[1, 1].TileNum = 0;
		//	te.CheckWaterOrSandLevel(x, y);
		//	Assert.AreEqual(0, te.TileLayer[x, y].Z);
		//	te.TileLayer[0, 1].Z = 1;
		//	te.TileLayer[0, 1].TileNum = 0;
		//	te.TileLayer[1, 1].Z = 0;
		//	te.TileLayer[1, 1].TileNum = GeneratorEngineYR.SandTileSingle;
		//	te.CheckWaterOrSandLevel(x, y);
		//	Assert.AreEqual(0, te.TileLayer[x, y].Z);
		//}

		//[TestMethod]
		//public void TestLevelSandOrWater() {
		//	var te = NewTestGeneratorEngineYR(3, 3);
		//	int x = 1;
		//	int y = 1;
		//	te.TileLayer[0, 0].Z = 1;
		//	te.TileLayer[1, 0].Z = 1;
		//	te.TileLayer[1, 0].TileNum = GeneratorEngineYR.WaterTileSingle;
		//	te.TileLayer[2, 0].Z = 1;
		//	te.TileLayer[0, 1].Z = 1;
		//	te.TileLayer[1, 1].Z = 0;
		//	te.TileLayer[1, 1].TileNum = GeneratorEngineYR.SandTileSingle;
		//	te.TileLayer[2, 1].Z = 0;
		//	// [3, y] and [4, y] is not used in this test.
		//	// [x, 2] is not used in this test.
		//	te.CheckWaterOrSandLevel(x, y);
		//	Assert.AreEqual(1, te.TileLayer[x, y].Z);
		//	te.TileLayer[1, 0].TileNum = GeneratorEngineYR.SandTileSingle;
		//	te.TileLayer[1, 1].TileNum = GeneratorEngineYR.WaterTileSingle;
		//	te.TileLayer[1, 1].Z = 0;
		//	te.CheckWaterOrSandLevel(x, y);
		//	Assert.AreEqual(1, te.TileLayer[x, y].Z);
		//}


		[TestMethod]
		public void TestDefineMapTilesFromHeightLayout() {
			var te = NewTestGeneratorEngineYR(3, 3);
			te.HeightLayout = new byte[te.TileLayer.Width * 2 - 1, te.TileLayer.Height];
			te.TileLayer[0, 0].Z = 0;
			te.HeightLayout[0, 0] = GeneratorEngineYR.SeaLevel;
			te.HeightLayout[1, 0] = GeneratorEngineYR.SandLevel;
			te.HeightLayout[0, 1] = 119;
			te.HeightLayout[1, 1] = 120;
			te.HeightLayout[1, 2] = 200;
			te.HeightLayout[2, 2] = 255;
			// [3, y] and [4, y] is not used in this test.
			te.DefineZFromHeightLayout();
			Assert.AreEqual<byte>(0, te.TileLayer[0, 0].Z);
			Assert.AreEqual(IsoTile.GroundType.Water, te.TileLayer[0, 0].Ground);
			Assert.AreEqual<byte>(0, te.TileLayer[1, 0].Z);
			Assert.AreEqual(IsoTile.GroundType.Sand, te.TileLayer[1, 0].Ground);
			Assert.AreEqual<byte>(0, te.TileLayer[0, 1].Z);
			Assert.AreEqual<byte>(1, te.TileLayer[1, 1].Z);
			Assert.AreEqual<byte>(9, te.TileLayer[1, 2].Z);
			Assert.AreEqual<byte>(14, te.TileLayer[2, 2].Z);
		}

		[TestMethod]
		public void TestGridDirection() {
			var te = NewTestGeneratorEngineYR(3, 3);
			int x = 1;
			int y = 1;
			te.TileLayer[0, 0].Z = 1;
			te.TileLayer[1, 0].Z = 2;
			te.TileLayer[2, 0].Z = 3;
			te.TileLayer[0, 1].Z = 4;
			te.TileLayer[1, 1].Z = 5;
			te.TileLayer[2, 1].Z = 6;
			te.TileLayer[0, 2].Z = 7;
			te.TileLayer[1, 2].Z = 8;
			te.TileLayer[2, 2].Z = 9;
			// [3, y] and [4, y] is not used in this test.
			Assert.AreEqual(1, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft).Z);
			Assert.AreEqual(2,te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Top).Z);
			Assert.AreEqual(3,te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight).Z);
			Assert.AreEqual(4,te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Left).Z);
			Assert.AreEqual(6,te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Right).Z);
			Assert.AreEqual(7,te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomLeft).Z);
			Assert.AreEqual(8,te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Bottom).Z);
			Assert.AreEqual(9,te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomRight).Z);
		}

		[TestMethod]
		public void TestGridInvalidBoundsTopLeft() {
			var te = NewTestGeneratorEngineYR(3, 3);
			int x = 0;
			int	y = 0;
			te.TileLayer[0, 0].Z = 1;
			te.TileLayer[1, 0].Z = 2;
			te.TileLayer[2, 0].Z = 3;
			te.TileLayer[3, 0].Z = 4;
			te.TileLayer[4, 0].Z = 5;
			te.TileLayer[0, 1].Z = 6;
			te.TileLayer[1, 1].Z = 7;
			te.TileLayer[2, 1].Z = 8;
			te.TileLayer[3, 1].Z = 9;
			te.TileLayer[4, 1].Z = 10;
			te.TileLayer[0, 2].Z = 11;
			te.TileLayer[1, 2].Z = 12;
			te.TileLayer[2, 2].Z = 13;
			te.TileLayer[3, 2].Z = 14;
			te.TileLayer[4, 2].Z = 15;
			Assert.AreEqual(-1, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft).TileNum);
			Assert.AreEqual(-1, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Top).TileNum);
			Assert.AreEqual(-1, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight).TileNum);
			Assert.AreEqual(-1, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Left).TileNum);
			Assert.AreEqual(2, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Right).Z);
			Assert.AreEqual(-1, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomLeft).TileNum);
			Assert.AreEqual(6, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Bottom).Z);
			Assert.AreEqual(7, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomRight).Z);
		}

		[TestMethod]
		public void TestGridInvalidBoundsBottomRight() {
			var te = NewTestGeneratorEngineYR(3, 3);
			int x = te.Width * 2 - 2;
			int y = te.Width - 1;
			te.TileLayer[0, 0].Z = 1;
			te.TileLayer[1, 0].Z = 2;
			te.TileLayer[2, 0].Z = 3;
			te.TileLayer[3, 0].Z = 4;
			te.TileLayer[4, 0].Z = 5;
			te.TileLayer[0, 1].Z = 6;
			te.TileLayer[1, 1].Z = 7;
			te.TileLayer[2, 1].Z = 8;
			te.TileLayer[3, 1].Z = 9;
			te.TileLayer[4, 1].Z = 10;
			te.TileLayer[0, 2].Z = 11;
			te.TileLayer[1, 2].Z = 12;
			te.TileLayer[2, 2].Z = 13;
			te.TileLayer[3, 2].Z = 14;
			te.TileLayer[4, 2].Z = 15;
			Assert.AreEqual(9, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft).Z);
			Assert.AreEqual(10, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Top).Z);
			Assert.AreEqual(-1, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight).TileNum);
			Assert.AreEqual(14, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Left).Z);
			Assert.AreEqual(-1, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Right).TileNum);
			Assert.AreEqual(-1, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomLeft).TileNum);
			Assert.AreEqual(-1, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Bottom).TileNum);
			Assert.AreEqual(-1, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomRight).TileNum);
		}

		[TestMethod]
		public void TestCheckPit() {
			var te = NewTestGeneratorEngineYR(3, 3);
			te.TileLayer[0, 0].Z = 1;
			te.TileLayer[1, 0].Z = 1;
			te.TileLayer[2, 0].Z = 1;
			te.TileLayer[0, 1].Z = 1;
			te.TileLayer[1, 1].Z = 0;
			te.TileLayer[2, 1].Z = 1;
			te.TileLayer[0, 2].Z = 1;
			te.TileLayer[1, 2].Z = 1;
			te.TileLayer[2, 2].Z = 1;
			// [3, y] and [4, y] is not used in this test.
			te.CheckPit(1, 1);
			Assert.AreEqual(1, te.TileLayer[1, 1].Z);
			te.TileLayer[1, 1].Z = 0;
			te.TileLayer[2, 2].Z = 0;
			te.CheckPit(1, 1);
			Assert.AreEqual(0, te.TileLayer[1, 1].Z);
		}

		[TestMethod]
		public void TestCheckSpike() {
			var te = NewTestGeneratorEngineYR(3, 3);
			te.TileLayer[0, 0].Z = 1;
			te.TileLayer[1, 0].Z = 1;
			te.TileLayer[2, 0].Z = 1;
			te.TileLayer[0, 1].Z = 1;
			te.TileLayer[1, 1].Z = 2;
			te.TileLayer[2, 1].Z = 1;
			te.TileLayer[0, 2].Z = 1;
			te.TileLayer[1, 2].Z = 1;
			te.TileLayer[2, 2].Z = 1;
			// [3, y] and [4, y] is not used in this test.
			te.CheckSpike(1, 1);
			Assert.AreEqual(1, te.TileLayer[1, 1].Z);
			te.TileLayer[1, 1].Z = 2;
			te.TileLayer[2, 2].Z = 2;
			te.CheckSpike(1, 1);
			Assert.AreEqual(2, te.TileLayer[1, 1].Z);
		}

		// Test that the all grid neighbor tiles do not differ more than +/-1.
		// This generate a random map with the same seed. This gives the same result for each test.
		[TestMethod]
		public void TestCompleteLevel() {
			var te = NewTestGeneratorEngineYR(50, 50);
			var noise = new PerlinNoise(222);
			te.GenerateHeightLayout(0.9d, false);	// Many hills.
			te.DefineZFromHeightLayout();
			te.LevelOut();
			te.TileLayer.DumpZToFile();
			for (int y = 0; y < te.Height; y++) {
				for (int x = 0; x < te.Width * 2 - 1; x++) {
					var ct = te.TileLayer[x, y];
					var t = te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft);
					Assert.IsFalse(te.CheckTileLevel(ct, t, correctLevel: false), $"Map layout failed on [{x},{y}]. Current Z:{ct.Z}, TopLeft Z:{t.Z}");
					t = te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Top);
					Assert.IsFalse(te.CheckTileLevel(ct, t, correctLevel: false), $"Map layout failed on [{x},{y}]. Current Z:{ct.Z}, Top Z:{t.Z}");
					t = te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight);
					Assert.IsFalse(te.CheckTileLevel(ct, t, correctLevel: false), $"Map layout failed on [{x},{y}]. Current Z:{ct.Z}, TopRight Z:{t.Z}");
					t = te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Left);
					Assert.IsFalse(te.CheckTileLevel(ct, t, correctLevel: false), $"Map layout failed on [{x},{y}]. Current Z:{ct.Z}, Left Z:{t.Z}");
					t = te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Right);
					Assert.IsFalse(te.CheckTileLevel(ct, t, correctLevel: false), $"Map layout failed on [{x},{y}]. Current Z:{ct.Z}, Right Z:{t.Z}");
					t = te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomLeft);
					Assert.IsFalse(te.CheckTileLevel(ct, t, correctLevel: false), $"Map layout failed on [{x},{y}]. Current Z:{ct.Z}, BottomLeft Z:{t.Z}");
					t = te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Bottom);
					Assert.IsFalse(te.CheckTileLevel(ct, t, correctLevel: false), $"Map layout failed on [{x},{y}]. Current Z:{ct.Z}, Bottom Z:{t.Z}");
					t = te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomRight);
					Assert.IsFalse(te.CheckTileLevel(ct, t, correctLevel: false), $"Map layout failed on [{x},{y}]. Current Z:{ct.Z}, BottomRight Z:{t.Z}");
					if (ct.Ground == IsoTile.GroundType.Sand)
						Assert.AreEqual(0, ct.Z, $"Map layout failed on [{x},{y}]. Ground type \"Sand\" should have Z = 0.");
					if (ct.Ground == IsoTile.GroundType.Water)
						Assert.AreEqual(0, ct.Z, $"Map layout failed on [{x},{y}]. Ground type \"Water\" should have Z = 0.");
				}
			}
		}

	}
}
