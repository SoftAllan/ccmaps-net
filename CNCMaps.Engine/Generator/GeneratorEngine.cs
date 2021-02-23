using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CNCMaps.FileFormats;
using CNCMaps.FileFormats.Map;
using CNCMaps.Shared;
using CNCMaps.Shared.Generator;

namespace CNCMaps.Engine.Generator {
	public abstract class GeneratorEngine : IGeneratorEngine {
		
		public GeneratorEngine(Settings settings) {
			Settings = settings;
		}

		internal Settings Settings { get; }
		internal TileLayer _tileLayer;
		internal ushort Height;
		internal ushort Width;

		public virtual bool GenerateMap() {
			throw new NotImplementedException();
		}

		public bool SaveMap() {
			IniFile iniFile;
			using (var mapStream = File.Create(Settings.OutputFile)) {
				iniFile = new IniFile(mapStream, Settings.OutputFile, 0, 0);
			}
			/* todo: Add comments
			 * ; Map created with Random Map Generator.
			 * ; Coded by Allan Greis Eriksen.
			 * ; Time and date for creation.
			 */ 
			AddPreviewSection(iniFile);
			AddPreviewPackSection(iniFile);
			AddBasicSection(iniFile);
			AddIsoMapPack5Section(iniFile);
			AddMapSection(iniFile);
			AddWaypointsSection(iniFile);
			iniFile.Save(Settings.OutputFile);
			return true;
		}

		// todo: Add a random map size.
		public virtual void ParseMapSize(MapSize mapSize) {
			switch (mapSize) {
				case MapSize.Small:
					Height= 50;
					Width = 50;
					break;
				case MapSize.Medium:
					Height = 100;
					Width = 100;
					break;
				case MapSize.Large:
					Height = 150;
					Width = 150;
					break;
				case MapSize.VeryLarge:
					Height = 300;
					Width = 300;
					break;
				default:
					break;
			}
		}

		// Fill level 0 clear tiles for all array values
		// (Code from MapFile.cs)
		// Todo: Temp use.
		public void InitialiseMapLayer(int tilenumber) {
			_tileLayer = new TileLayer(Width, Height);
			for (ushort y = 0; y < Height; y++) {
				for (ushort x = 0; x <= Width * 2 - 2; x++) {
					ushort dx = (ushort)(x);
					ushort dy = (ushort)(y * 2 + x % 2);
					ushort rx = (ushort)((dx + dy) / 2 + 1);
					ushort ry = (ushort)(dy - rx + Width + 1);
					_tileLayer[x, y] = new IsoTile(dx, dy, rx, ry, 0, tilenumber, 0, 0);
				}
			}
		}

		public string TheaterType(TheaterType theaterType) {
			switch (theaterType) {
				case Shared.TheaterType.None:
					throw new InvalidOperationException();
				case Shared.TheaterType.Temperate:
					return "temperate";
				case Shared.TheaterType.Urban:
					return "urban";
				case Shared.TheaterType.Snow:
					return "snow";
				case Shared.TheaterType.Lunar:
					return "lunar";
				case Shared.TheaterType.Desert:
					return "desert";
				case Shared.TheaterType.NewUrban:
					return "newurban";
				case Shared.TheaterType.All:
					throw new InvalidOperationException();
				default:
					throw new InvalidOperationException();
			}
		}

		private void AddIsoMapPack5Section(IniFile iniFile) {
			var isoMapPack5 = iniFile.GetOrCreateSection("IsoMapPack5");
			_tileLayer.SerializeIsoMapPack5(isoMapPack5);
		}

		private void AddMapSection(IniFile iniFile) {
			var map = iniFile.GetOrCreateSection("Map");
			map.SetValue("Size", $"0,0,{Width},{Height}");
			map.SetValue("Theater", TheaterType(Settings.TheaterType).ToUpper());   // todo: Test if uppercase is needed.
			map.SetValue("LocalSize", $"2,4,{Width - 4},{Height - 6}"); // todo: Test with medium, large and vlarge values.
		}

		// Todo: Find the relevant values.
		private void AddBasicSection(IniFile iniFile) {
			var basic = iniFile.GetOrCreateSection("Basic");
			basic.SetValue("Name", "RANDOMMAP");    // todo: name should be RANDOMMAP{date and time}
			basic.SetValue("Author", "Random Map Generator");
			basic.SetValue("Percent", "0");			// todo: What is this?
			basic.SetValue("GameMode", "standard"); // todo: maybe set to Random Map or "randommap"?
			basic.SetValue("HomeCell", "98");           // todo: What is this?
			basic.SetValue("InitTime", "10000");    // todo: What is this?
			basic.SetValue("Official", "no");
			basic.SetValue("EndOfGame", "no");  // todo: What is this?
			basic.SetValue("FreeRadar", "no");  // Minimap not visible
			basic.SetValue("MaxPlayer", "8");   // todo: Change to parameter
			basic.SetValue("MinPlayer", "2");   // No scenario.
			basic.SetValue("SkipScore", "no");
			basic.SetValue("TrainCrate", "no");
			basic.SetValue("TruckCrate", "no");
			basic.SetValue("AltHomeCell", "99");    // todo: what is this?
			basic.SetValue("OneTimeOnly", "no");
			basic.SetValue("CarryOverCap", "0");
			basic.SetValue("NewINIFormat", "4");
			basic.SetValue("NextScenario", "");
			basic.SetValue("SkipMapSelect", "no");
			basic.SetValue("CarryOverMoney", "0.000000");
			basic.SetValue("AltNextScenario", "");
			basic.SetValue("MultiplayerOnly", "1");  // No senario.
			basic.SetValue("IceGrowthEnabled", "yes");
			basic.SetValue("VeinGrowthEnabled", "yes");
			basic.SetValue("TiberiumGrowthEnabled", "yes");
			basic.SetValue("IgnoreGlobalAITriggers", "no");
			basic.SetValue("TiberiumDeathToVisceroid", "no");
		}

		private void AddPreviewSection(IniFile iniFile) {
			var preview = iniFile.GetOrCreateSection("Preview");
		}

		private void AddPreviewPackSection(IniFile iniFile) {
			var previewMap = iniFile.GetOrCreateSection("PreviewPack");
		}

		private void AddWaypointsSection(IniFile iniFile) {
			var waypoints = iniFile.GetOrCreateSection("Waypoints");
			waypoints.SetValue("0", "50050");
			waypoints.SetValue("1", "51050");
			waypoints.SetValue("2", "52050");
			waypoints.SetValue("3", "53050");
			waypoints.SetValue("4", "50051");
			waypoints.SetValue("5", "51051");
			waypoints.SetValue("6", "52051");
			waypoints.SetValue("7", "53051");
		}

	}
}
