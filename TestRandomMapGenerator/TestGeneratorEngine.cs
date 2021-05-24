using CNCMaps.Engine.Generator;
using CNCMaps.Engine.Generator.Map;
using CNCMaps.Engine.Utility;
using CNCMaps.Shared.Generator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestRandomMapGenerator {
	
	[TestClass]
    public class TestGeneratorEngine
    {
		private GeneratorEngineGeneral NewTestGeneratorEngine(int x, int y, int perlinNoiseSeed = 222) {
			var newEngine = new GeneratorEngineGeneral(new Settings(), perlinNoiseSeed);
			newEngine.Width = (ushort)x;
			newEngine.Height = (ushort)y;
			newEngine.InitialiseMapLayer(0);
			return newEngine;
		}

		[TestMethod]
		public void TestLevelOutLowTop() {
			var te = NewTestGeneratorEngine(3, 3);
			int x = 1;
			int y = 1;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft).Z = 1;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Top).Z = 1;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight).Z = 1;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Left).Z = 1;
			te.TileLayer[x, y].Z = 3;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Right).Z = 1;
			te.CheckLevel(x, y);
			Assert.AreEqual(2, te.TileLayer[x, y].Z);
		}

		[TestMethod]
		public void TestLevelOutHighTop() {
			var te = NewTestGeneratorEngine(3, 3);
			int x = 1;
			int y = 1;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft).Z = 4;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Top).Z = 4;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight).Z = 4;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Left).Z = 4;
			te.TileLayer[x, y].Z = 0;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Right).Z = 4;
			te.CheckLevel(x, y);
			Assert.AreEqual(3, te.TileLayer[x, y].Z);
		}

		[TestMethod]
		public void TestLevelOutLowLeft() {
			var te = NewTestGeneratorEngine(3, 3);
			int x = 1;
			int y = 1;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft).Z = 6;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Top).Z = 6;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight).Z = 6;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Left).Z = 6;
			te.TileLayer[x, y].Z = 9;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Right).Z = 6;
			te.CheckLevel(x, y);
			Assert.AreEqual(7, te.TileLayer[x, y].Z);
		}

		[TestMethod]
		public void TestLevelOutHighLeft() {
			var te = NewTestGeneratorEngine(3, 3);
			int x = 1;
			int y = 1;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft).Z = 6;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Top).Z = 6;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight).Z = 6;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Left).Z = 6;
			te.TileLayer[x, y].Z = 1;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Right).Z = 6;
			te.CheckLevel(x, y);
			Assert.AreEqual(5, te.TileLayer[x, y].Z);
		}


		[TestMethod]
		public void TestLevelOutTopLeftBottomRight() {
			var te = NewTestGeneratorEngine(3, 3);
			int x = 2;
			int y = 0;
			te.TileLayer[0, 0].Z = 1;
			te.TileLayer[1, 0].Z = 2;
			te.TileLayer[2, 0].Z = 1;
			te.TileLayer[3, 0].Z = 3;   // = 2
			te.TileLayer[4, 0].Z = 2;
			te.TileLayer[0, 1].Z = 2;
			te.TileLayer[1, 1].Z = 2;
			te.TileLayer[2, 1].Z = 2;
			te.TileLayer[3, 1].Z = 1;
			te.TileLayer[4, 1].Z = 2;
			te.TileLayer[0, 2].Z = 3;
			te.TileLayer[1, 2].Z = 2;
			te.TileLayer[2, 2].Z = 2;
			te.TileLayer[3, 2].Z = 2;
			te.TileLayer[4, 2].Z = 2;
			te.CheckLevel(x, y);
			x = 3;
			te.CheckLevel(x, y);
			TestNeighborLevel(te, x, y);
		}

		[TestMethod]
		public void TestDefineGroundTypeFromHeightLayout() {
			var te = NewTestGeneratorEngine(3, 3);
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
			var te = NewTestGeneratorEngine(3, 3);
			int x = 1;
			int y = 1;
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
			Assert.AreEqual(6, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft).Z);
			Assert.AreEqual(2, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Top).Z);
			Assert.AreEqual(8, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight).Z);
			Assert.AreEqual(-1, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Left).TileNum);	// Does not exist from 1,1
			Assert.AreEqual(9, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Right).Z);
			Assert.AreEqual(11, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomLeft).Z);
			Assert.AreEqual(12, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Bottom).Z);
			Assert.AreEqual(13, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomRight).Z);
			x = 2;
			Assert.AreEqual(2, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft).Z);
			Assert.AreEqual(3, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Top).Z);
			Assert.AreEqual(4, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight).Z);
			Assert.AreEqual(6, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Left).Z);
			Assert.AreEqual(10, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Right).Z);
			Assert.AreEqual(7, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomLeft).Z);
			Assert.AreEqual(13, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Bottom).Z);
			Assert.AreEqual(9, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomRight).Z);
		}

		[TestMethod]
		public void TestGridInvalidBoundsTopLeft() {
			var te = NewTestGeneratorEngine(3, 3);
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
			Assert.AreEqual(3, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Right).Z);
			Assert.AreEqual(-1, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomLeft).TileNum);
			Assert.AreEqual(6, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Bottom).Z);
			Assert.AreEqual(2, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomRight).Z);
		}

		[TestMethod]
		public void TestGridInvalidBoundsBottomRight() {
			var te = NewTestGeneratorEngine(3, 3);
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
			Assert.AreEqual(13, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Left).Z);
			Assert.AreEqual(-1, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Right).TileNum);
			Assert.AreEqual(14, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomLeft).Z);
			Assert.AreEqual(-1, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Bottom).TileNum);
			Assert.AreEqual(-1, te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomRight).TileNum);
		}

		[TestMethod]
		public void TestCheckPit() {
			var te = NewTestGeneratorEngine(3, 3);
			int x = 2;
			int y = 1;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft).Z = 1;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Top).Z = 1;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight).Z = 1;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Left).Z = 1;
			te.TileLayer[x, y].Z = 0;
			te.TileLayer[x, y].Ground = IsoTile.GroundType.Water;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Right).Z = 1;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomLeft).Z = 1;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Bottom).Z = 1;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomRight).Z = 1;
			te.CheckPit(x, y);
			Assert.AreEqual(1, te.TileLayer[x, y].Z);
			Assert.AreEqual(IsoTile.GroundType.Ground, te.TileLayer[x, y].Ground);
			te.TileLayer[x, y].Z = 0;
			te.TileLayer[x, y].Ground = IsoTile.GroundType.Water;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomRight).Z = 0;
			te.CheckPit(x, y);
			Assert.AreEqual(0, te.TileLayer[x, y].Z);
			Assert.AreEqual(IsoTile.GroundType.Water, te.TileLayer[x, y].Ground);
		}

		[TestMethod]
		public void TestCheckSpike() {
			var te = NewTestGeneratorEngine(3, 3);
			int x = 2;
			int y = 1;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft).Z = 1;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Top).Z = 1;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight).Z = 1;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Left).Z = 1;
			te.TileLayer[x, y].Z = 2;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Right).Z = 1;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomLeft).Z = 1;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Bottom).Z = 1;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomRight).Z = 1;
			te.CheckSpike(x, y);
			Assert.AreEqual(1, te.TileLayer[x, y].Z);
			Assert.AreEqual(IsoTile.GroundType.Ground, te.TileLayer[x, y].Ground);
			te.TileLayer[x, y].Z = 2;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomRight).Z = 2;
			te.CheckSpike(x, y);
			Assert.AreEqual(2, te.TileLayer[x, y].Z);
			Assert.AreEqual(IsoTile.GroundType.Ground, te.TileLayer[x, y].Ground);
		}

		// Test that the all grid neighbor tiles do not differ more than +/-1.
		// This generate a random map with the same seed. This gives the same result for each test.
		// Many hills. Requires 3 runs of CheckLevel corrections.
		[TestMethod]
		public void TestRepeatCheckLevel() {
			var te = NewTestGeneratorEngine(10, 10);
			te.GenerateHeightLayout(0.70d, false);	
			te.DefineZFromHeightLayout();
			Assert.AreNotEqual(3, te.TileLayer.GridTile(5, 0, TileLayer.TileDirection.Bottom).Z);
			for (int y = 0; y < te.Height; y++) {
				for (int x = 0; x < te.Width * 2 - 1; x++) {
					te.CheckLevel(x, y);
				}
			}
			Assert.AreEqual(3, te.TileLayer.GridTile(5, 0, TileLayer.TileDirection.Bottom).Z);
			Assert.AreNotEqual(3, te.TileLayer.GridTile(1, 5, TileLayer.TileDirection.Bottom).Z);
			for (int y = 0; y < te.Height; y++) {
				for (int x = 0; x < te.Width * 2 - 1; x++) {
					te.CheckLevel(x, y);
				}
			}
			Assert.AreEqual(3, te.TileLayer.GridTile(1, 5, TileLayer.TileDirection.Bottom).Z);
			for (int y = 0; y < te.Height; y++) {
				for (int x = 0; x < te.Width * 2 - 1; x++) {
					te.CheckLevel(x, y);
				}
			}
			for (int y = 0; y < te.Height; y++) {
				for (int x = 0; x < te.Width * 2 - 1; x++) {
					TestNeighborLevel(te, x, y);
				}
			}
		}

		// Test that the all grid neighbor tiles do not differ more than +/-1.
		// Use a very big map 222x444. Would max out the preview generator with 2GB use.
		// This generate a random map with the same seed. This gives the same result for each test.
		// Many hills.
		[TestMethod]
		public void TestLevelOut() {
			var te = NewTestGeneratorEngine(222, 444);
			te.GenerateHeightLayout(0.20d, false);
			te.DefineZFromHeightLayout();
			te.LevelOut();
			// te.TileLayer.DumpToFile();
			for (int y = 0; y < te.Height; y++) {
				for (int x = 0; x < te.Width * 2 - 1; x++) {
					TestNeighborLevel(te, x, y);
				}
			}
		}

		private void TestNeighborLevel(GeneratorEngineGeneral te, int x, int y) {
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
			t = te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft);
			var b = te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomRight);
			Assert.IsFalse(te.CheckValleyLevel(ct, t, b, correctLevel: false), $"Map layout valley check failed on [{x},{y}]. Current Z:{ct.Z}, TopLeft Z:{t.Z}, BottomRight Z:{b.Z}");
			t = te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight);
			b = te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomLeft);
			Assert.IsFalse(te.CheckValleyLevel(ct, t, b, correctLevel: false), $"Map layout valley check failed on [{x},{y}]. Current Z:{ct.Z}, TopRight Z:{t.Z}, BottomLeft Z:{b.Z}");
			t = te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Top);
			b = te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Bottom);
			Assert.IsFalse(te.CheckValleyLevel(ct, t, b, correctLevel: false), $"Map layout valley check failed on [{x},{y}]. Current Z:{ct.Z}, Top Z:{t.Z}, Bottom Z:{b.Z}");
			t = te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Left);
			b = te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Right);
			Assert.IsFalse(te.CheckValleyLevel(ct, t, b, correctLevel: false), $"Map layout valley check failed on [{x},{y}]. Current Z:{ct.Z}, Left Z:{t.Z}, Right Z:{b.Z}");
			t = te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft);
			b = te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Bottom);
			var v = te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomLeft);
			Assert.IsFalse(te.CheckValleyLevel(ct, t, b, v, correctLevel: false), $"Map layout valley check failed on [{x},{y}]. Current Z:{ct.Z}, TopLeft Z:{t.Z}, Bottom Z:{b.Z}, BottomLeft Z:{v.Z}");
			t = te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Top);
			b = te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomLeft);
			v = te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft);
			Assert.IsFalse(te.CheckValleyLevel(ct, t, b, v, correctLevel: false), $"Map layout valley check failed on [{x},{y}]. Current Z:{ct.Z}, Top Z:{t.Z}, BottomLeft Z:{b.Z}, TopLeft Z:{v.Z}");
			t = te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight);
			b = te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Left);
			v = te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft);
			Assert.IsFalse(te.CheckValleyLevel(ct, t, b, v, correctLevel: false), $"Map layout valley check failed on [{x},{y}]. Current Z:{ct.Z}, TopRight Z:{t.Z}, Left Z:{b.Z}, TopLeft Z:{v.Z}");
			t = te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft);
			b = te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Right);
			v = te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight);
			Assert.IsFalse(te.CheckValleyLevel(ct, t, b, v, correctLevel: false), $"Map layout valley check failed on [{x},{y}]. Current Z:{ct.Z}, TopLeft Z:{t.Z}, Right Z:{b.Z}, TopRight Z:{v.Z}");
			if (ct.Ground == IsoTile.GroundType.Sand)
				Assert.AreEqual(0, ct.Z, $"Map layout failed on [{x},{y}]. Ground type \"Sand\" should have Z = 0.");
			if (ct.Ground == IsoTile.GroundType.Water)
				Assert.AreEqual(0, ct.Z, $"Map layout failed on [{x},{y}]. Ground type \"Water\" should have Z = 0.");
		}

		[TestMethod]
		public void TestWaterToWaterConnection() {
			var te = NewTestGeneratorEngine(3, 3);
			int x = 2;
			int y = 1;
			te.TileLayer[x, y].Ground = IsoTile.GroundType.Water;
			te.CheckSingleWaterSpots(x, y);
			Assert.AreEqual(IsoTile.GroundType.Sand, te.TileLayer[x, y].Ground);
			te.TileLayer[x, y].Ground = IsoTile.GroundType.Water;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Top).Ground = IsoTile.GroundType.Water;
			te.CheckSingleWaterSpots(x, y);
			Assert.AreEqual(IsoTile.GroundType.Sand, te.TileLayer[x, y].Ground);
			te.TileLayer[x, y].Ground = IsoTile.GroundType.Water;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Top).Ground = IsoTile.GroundType.Ground;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Right).Ground = IsoTile.GroundType.Water;
			te.CheckSingleWaterSpots(x, y);
			Assert.AreEqual(IsoTile.GroundType.Sand, te.TileLayer[x, y].Ground);
			te.TileLayer[x, y].Ground = IsoTile.GroundType.Water;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Right).Ground = IsoTile.GroundType.Ground;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Bottom).Ground = IsoTile.GroundType.Water;
			te.CheckSingleWaterSpots(x, y);
			Assert.AreEqual(IsoTile.GroundType.Sand, te.TileLayer[x, y].Ground);
			te.TileLayer[x, y].Ground = IsoTile.GroundType.Water;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Bottom).Ground = IsoTile.GroundType.Ground;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Left).Ground = IsoTile.GroundType.Water;
			te.CheckSingleWaterSpots(x, y);
			Assert.AreEqual(IsoTile.GroundType.Sand, te.TileLayer[x, y].Ground);
			te.TileLayer[x, y].Ground = IsoTile.GroundType.Water;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Left).Ground = IsoTile.GroundType.Ground;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft).Ground = IsoTile.GroundType.Water;
			te.CheckSingleWaterSpots(x, y);
			Assert.AreEqual(IsoTile.GroundType.Water, te.TileLayer[x, y].Ground);
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft).Ground = IsoTile.GroundType.Ground;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight).Ground = IsoTile.GroundType.Water;
			te.CheckSingleWaterSpots(x, y);
			Assert.AreEqual(IsoTile.GroundType.Water, te.TileLayer[x, y].Ground);
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight).Ground = IsoTile.GroundType.Ground;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomRight).Ground = IsoTile.GroundType.Water;
			te.CheckSingleWaterSpots(x, y);
			Assert.AreEqual(IsoTile.GroundType.Water, te.TileLayer[x, y].Ground);
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomRight).Ground = IsoTile.GroundType.Ground;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomLeft).Ground = IsoTile.GroundType.Water;
			te.CheckSingleWaterSpots(x, y);
			Assert.AreEqual(IsoTile.GroundType.Water, te.TileLayer[x, y].Ground);
		}

		[TestMethod]
		public void TestWaterNextToHighGround() {
			var te = NewTestGeneratorEngine(4, 5);
			int x = 2;
			int y = 2;
			int x2 = 3;	// top right x
			int y2 = 1; // top right y
			te.TileLayer[x, y].Ground = IsoTile.GroundType.Water;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight).Ground = IsoTile.GroundType.Water;	// Top right x = 3, Y = 1
			te.CheckSingleWaterNextToHighGround(x, y);
			Assert.AreEqual(IsoTile.GroundType.Water, te.TileLayer[x, y].Ground);
			Assert.AreEqual(IsoTile.GroundType.Water, te.TileLayer[x2, y2].Ground);
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Top).Z = 1;
			te.CheckSingleWaterNextToHighGround(x, y);
			Assert.AreEqual(IsoTile.GroundType.Sand, te.TileLayer[x, y].Ground);
			Assert.AreEqual(IsoTile.GroundType.Water, te.TileLayer[x2, y2].Ground);
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Top).Z = 0;
			te.TileLayer[x, y].Ground = IsoTile.GroundType.Water;
			te.TileLayer.GridTile(x2, y2, TileLayer.TileDirection.TopRight).Z = 1;
			te.CheckSingleWaterNextToHighGround(x2, y2);
			Assert.AreEqual(IsoTile.GroundType.Water, te.TileLayer[x, y].Ground);
			Assert.AreEqual(IsoTile.GroundType.Sand, te.TileLayer[x2, y2].Ground);
			te.TileLayer[x2, y2].Ground = IsoTile.GroundType.Water;
			te.TileLayer.GridTile(x2, y2, TileLayer.TileDirection.TopRight).Z = 0;
			te.TileLayer.GridTile(x2, y2, TileLayer.TileDirection.Right).Z = 1;
			te.CheckSingleWaterNextToHighGround(x2, y2);
			Assert.AreEqual(IsoTile.GroundType.Water, te.TileLayer[x, y].Ground);
			Assert.AreEqual(IsoTile.GroundType.Sand, te.TileLayer[x2, y2].Ground);
			te.TileLayer[x2, y2].Ground = IsoTile.GroundType.Water;
			te.TileLayer.GridTile(x2, y2, TileLayer.TileDirection.Right).Z = 0;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomRight).Z = 1;
			te.CheckSingleWaterNextToHighGround(x, y);
			Assert.AreEqual(IsoTile.GroundType.Sand, te.TileLayer[x, y].Ground);
			Assert.AreEqual(IsoTile.GroundType.Water, te.TileLayer[x2, y2].Ground);
			te.TileLayer[x, y].Ground = IsoTile.GroundType.Water;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomRight).Z = 0;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Bottom).Z = 1;
			te.CheckSingleWaterNextToHighGround(x, y);
			Assert.AreEqual(IsoTile.GroundType.Sand, te.TileLayer[x, y].Ground);
			Assert.AreEqual(IsoTile.GroundType.Water, te.TileLayer[x2, y2].Ground);
			te.TileLayer[x, y].Ground = IsoTile.GroundType.Water;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Bottom).Z = 0;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomLeft).Z = 1;
			te.CheckSingleWaterNextToHighGround(x, y);
			Assert.AreEqual(IsoTile.GroundType.Sand, te.TileLayer[x, y].Ground);
			Assert.AreEqual(IsoTile.GroundType.Water, te.TileLayer[x2, y2].Ground);
			te.TileLayer[x, y].Ground = IsoTile.GroundType.Water;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomLeft).Z = 0;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Left).Z = 1;
			te.CheckSingleWaterNextToHighGround(x, y);
			Assert.AreEqual(IsoTile.GroundType.Sand, te.TileLayer[x, y].Ground);
			Assert.AreEqual(IsoTile.GroundType.Water, te.TileLayer[x2, y2].Ground);
			te.TileLayer[x, y].Ground = IsoTile.GroundType.Water;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Left).Z = 0;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft).Z = 1;
			te.CheckSingleWaterNextToHighGround(x, y);
			Assert.AreEqual(IsoTile.GroundType.Sand, te.TileLayer[x, y].Ground);
			Assert.AreEqual(IsoTile.GroundType.Water, te.TileLayer[x2, y2].Ground);
		}

	}
}
