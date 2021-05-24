using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CNCMaps.Engine.Game;
using CNCMaps.Engine.Generator;
using CNCMaps.Engine.Generator.Map;
using CNCMaps.FileFormats;
using CNCMaps.FileFormats.VirtualFileSystem;
using CNCMaps.Shared;
using CNCMaps.Shared.Generator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestRandomMapGenerator {

	[TestClass]	
	public class TestGeneratorEngineYR {
		public Settings TestSettings { get; set; }
		public Theater TestTheater { get; set; }

		private GeneratorEngineYR NewTestGeneratorEngineYR(int x, int y) {
			var newEngine = new GeneratorEngineYR(new Settings());
			newEngine.Width = (ushort)x;
			newEngine.Height = (ushort)y;
			newEngine.InitialiseMapLayer(0);
			return newEngine;
		}

		[TestMethod]
		public void TestRampA() {
			var te = NewTestGeneratorEngineYR(3, 3);
			int x = 2;
			int y = 1;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft).Z = 1;
			te.DefineRampTile(x, y);
			Assert.AreEqual(GeneratorEngineYR.RampTL1BR0, te.TileLayer[x, y].TileNum);
			te.TileLayer[x, y].TileNum = 0;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft).Z = 0;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight).Z = 1;
			te.DefineRampTile(x, y);
			Assert.AreEqual(GeneratorEngineYR.RampTR1BL0, te.TileLayer[x, y].TileNum);
			te.TileLayer[x, y].TileNum = 0;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight).Z = 0;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomRight).Z = 1;
			te.DefineRampTile(x, y);
			Assert.AreEqual(GeneratorEngineYR.RampTL0BR1, te.TileLayer[x, y].TileNum);
			te.TileLayer[x, y].TileNum = 0;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomRight).Z = 0;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomLeft).Z = 1;
			te.DefineRampTile(x, y);
			Assert.AreEqual(GeneratorEngineYR.RampTR0BL1, te.TileLayer[x, y].TileNum);
		}

		[TestMethod]
		public void TestRampB() {
			var te = NewTestGeneratorEngineYR(3, 3);
			int x = 2;
			int y = 1;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft).Z = 1;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomLeft).Z = 1;
			te.DefineRampTile(x, y);
			Assert.AreEqual(GeneratorEngineYR.RampL1RC0, te.TileLayer[x, y].TileNum);
			te.TileLayer[x, y].TileNum = 0;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomLeft).Z = 0;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight).Z = 1;
			te.DefineRampTile(x, y);
			Assert.AreEqual(GeneratorEngineYR.RampT1BC0, te.TileLayer[x, y].TileNum);
			te.TileLayer[x, y].TileNum = 0;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft).Z = 0;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomRight).Z = 1;
			te.DefineRampTile(x, y);
			Assert.AreEqual(GeneratorEngineYR.RampLC0R1, te.TileLayer[x, y].TileNum);
			te.TileLayer[x, y].TileNum = 0;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight).Z = 0;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomLeft).Z = 1;
			te.DefineRampTile(x, y);
			Assert.AreEqual(GeneratorEngineYR.RampTC0B1, te.TileLayer[x, y].TileNum);
		}

		[TestMethod]
		public void TestRampC() {
			var te = NewTestGeneratorEngineYR(3, 3);
			int x = 2;
			int y = 1;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Left).Z = 1;
			te.DefineRampTile(x, y);
			Assert.AreEqual(GeneratorEngineYR.RampLC1R0, te.TileLayer[x, y].TileNum);
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Left).Z = 0;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Top).Z = 1;
			te.DefineRampTile(x, y);
			Assert.AreEqual(GeneratorEngineYR.RampTC1B0, te.TileLayer[x, y].TileNum);
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Top).Z = 0;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Right).Z = 1;
			te.DefineRampTile(x, y);
			Assert.AreEqual(GeneratorEngineYR.RampL0RC1, te.TileLayer[x, y].TileNum);
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Right).Z = 0;
			te.TileLayer.GridTile(x, y, TileLayer.TileDirection.Bottom).Z = 1;
			te.DefineRampTile(x, y);
			Assert.AreEqual(GeneratorEngineYR.RampT0BC1, te.TileLayer[x, y].TileNum);
		}

	}
}
