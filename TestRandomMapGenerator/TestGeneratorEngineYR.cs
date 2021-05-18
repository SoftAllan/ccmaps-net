﻿using System;
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

		[TestInitialize]
		public void Setup() {
			// A test theater is setup for all testing methods as this takes a long time to do.
			// todo: It is still called for each test method. Maybe use some kind of stub if possible.
			//		 This would however require that the constructor of Theater can use interfaces for dependency injection. Maybe.
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
	}
}
