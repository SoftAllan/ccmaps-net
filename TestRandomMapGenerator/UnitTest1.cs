using CNCMaps.Engine.Game;
using CNCMaps.Engine.Generator;
using CNCMaps.Engine.Generator.Map;
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
		public void TestLevelOutHeight() {
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
			Assert.AreEqual(newEngine.TileLayer[0, 0].Z, 1);
			Assert.AreEqual(newEngine.TileLayer[1, 1].Z, 0);
			Assert.AreEqual(newEngine.TileLayer[2, 2].Z, 0);
		}

		[TestMethod]
		public void TestDefineMapTilesFromHeightLayout() {
			var te = NewTestGeneratorEngineYR(3, 3);
			te.HeightLayout = new byte[te.TileLayer.Height, te.TileLayer.Width];
			te.TileLayer[0, 0].Z = 0;
			te.HeightLayout[0, 0] = GeneratorEngineYR.SeaLevel;
			te.HeightLayout[1, 0] = GeneratorEngineYR.SandLevel;
			te.HeightLayout[0, 1] = 119;
			te.HeightLayout[1, 1] = 120;
			te.HeightLayout[1, 2] = 200;
			te.HeightLayout[2, 2] = 255;
			te.DefineMapTilesFromHeightLayout(TestTheater);
			Assert.AreEqual<byte>(0, te.TileLayer[0, 0].Z);
			Assert.AreEqual<byte>(0, te.TileLayer[1, 0].Z);
			Assert.AreEqual<byte>(0, te.TileLayer[0, 1].Z);
			Assert.AreEqual<byte>(1, te.TileLayer[1, 1].Z);
			Assert.AreEqual<byte>(9, te.TileLayer[1, 2].Z);
			Assert.AreEqual<byte>(14, te.TileLayer[2, 2].Z);
		}
	}
}
