using CNCMaps.Engine.Generator;
using CNCMaps.Engine.Generator.Map;
using CNCMaps.Shared.Generator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TestRandomMapGenerator
{
    [TestClass]
    public class UnitTest1
    {
		private GeneratorEngineYR NewTestGeneratorEngineYR(int x, int y) {
			var settings = new Settings();
			var newEngine = new GeneratorEngineYR(settings);
			newEngine.Width = (ushort)x;
			newEngine.Height = (ushort)y;
			newEngine.InitialiseMapLayer(0);
			return newEngine;
		}

		[TestMethod]
		public void TestMethod1() {
			var newEngine = NewTestGeneratorEngineYR(3, 3);
			newEngine.TileLayer[0, 0].Z = 1;
			newEngine.TileLayer[1, 0].Z = 1;
			newEngine.TileLayer[2, 0].Z = 1;
			newEngine.TileLayer[0, 1].Z = 0;
			newEngine.TileLayer[1, 1].Z = 3;
			newEngine.TileLayer[2, 1].Z = 0;
			newEngine.TileLayer[0, 2].Z = 0;
			newEngine.TileLayer[1, 2].Z = 0;
			newEngine.TileLayer[2, 2].Z = 0;
			Assert.AreEqual(newEngine.TileLayer[1, 1].Z, 3);
			newEngine.LevelOutHeight();
			Assert.AreEqual(newEngine.TileLayer[1, 1].Z, 0);
		}
	}
}
