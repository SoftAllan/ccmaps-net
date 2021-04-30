using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using CNCMaps.Engine.Game;
using CNCMaps.Engine.Generator.Map;
using CNCMaps.Engine.Utility;
using CNCMaps.FileFormats;
using CNCMaps.Shared;
using CNCMaps.Shared.Generator;
using NLog;

namespace CNCMaps.Engine.Generator {
	public abstract class GeneratorEngine : IGeneratorEngine {

		/* Notes about the generator:
		 * Regardless of the size of the map the size of lakes, rivers and the like has to be set to the same scale.
		 * 
		 */

		public GeneratorEngine(Settings settings, Logger logger) {
			Settings = settings;
			Noise = new PerlinNoise(222);   // todo: set seed to Environment.TickCount
			_logger = logger;
		}

		private const double NoiseOffset = 0.04d;

		internal Settings Settings { get; }
		public TileLayer TileLayer { get ; set ; }
		public ushort Height;
		public ushort Width;
		public Theater Theater { get; set; }
		internal PerlinNoise Noise { get; }
		public byte[,] HeightLayout { get; set; }
		private Logger _logger { get; }

		public virtual bool GenerateMap() {
			throw new NotImplementedException();
		}

		public bool SaveMap() {
			_logger.Debug("Creating map file.");
			IniFile iniFile;
			using (var mapStream = File.Create(Settings.OutputFile)) {
				iniFile = new IniFile(mapStream, Settings.OutputFile, 0, 0);
			}
			/* todo: Add comments
			 * ; Map created with Random Map Generator.
			 * ; Coded by Allan Greis Eriksen.
			 * ; Time and date for creation.
			 */
			_logger.Debug("Adding sections to map file.");
			AddPreviewSection(iniFile);
			AddPreviewPackSection(iniFile);
			AddBasicSection(iniFile);
			AddIsoMapPack5Section(iniFile);
			AddMapSection(iniFile);
			AddWaypointsSection(iniFile);
			_logger.Debug("Saving map file.");
			iniFile.Save(Settings.OutputFile);
			_logger.Debug("Map file saved.");
			return true;
		}

		// todo: Add a random map size.
		// todo: Define the min, max range for each map size.
		internal virtual void ParseMapSize(MapSize mapSize) {
			_logger.Debug("Parsing map size.");
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

		// Crate a new tilelayer, fill tiles for all array values and set coordinates.
		// (Code from MapFile.cs)
		public void InitialiseMapLayer(int tilenumber) {
			_logger.Debug("Initializing map layer.");
			TileLayer = new TileLayer(Width, Height);
			for (ushort y = 0; y < Height; y++) {
				for (ushort x = 0; x <= Width * 2 - 2; x++) {
					ushort dx = (ushort)(x);
					ushort dy = (ushort)(y * 2 + x % 2);
					ushort rx = (ushort)((dx + dy) / 2 + 1);
					ushort ry = (ushort)(dy - rx + Width + 1);
					TileLayer[x, y] = new IsoTile(dx, dy, rx, ry, 0, tilenumber, 0, 0);
				}
			}
		}

		internal void GenerateHeightLayout() {
			_logger.Debug("Generating height layout");
			HeightLayout = new byte[TileLayer.Height, TileLayer.Width];
			var nv = 0d;
			for (int y = 0; y < TileLayer.Height; y++) {
				for (int x = 0; x < TileLayer.Width; x++) {
					nv = Noise.Noise(x * NoiseOffset, y * NoiseOffset, 0d) + 1d;
					HeightLayout[y,x] = (byte)(nv * 128);
				}
			}
			DebugLayoutHeight();
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
			TileLayer.SerializeIsoMapPack5(isoMapPack5);
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

		internal void TestPerlin() {
			Bitmap bitmap = new Bitmap(300, 300);

			var noise = new PerlinNoise(222);
			var c = Color.Empty;
			var nv = 0d;
			var h = 0;

			for (int x = 0; x < bitmap.Width; x++) {
				for (int y = 0; y < bitmap.Height; y++) {
					nv = noise.Noise(x * 0.006d, y * 0.006d, 0d) + 1d;
					h = (int)(nv * 128);
					if (h < 100)
						c = Color.FromArgb(0, 0, h + 80);   // water
					else if (h < 110)
						c = Color.FromArgb(300 - h, 300 - h, 0);        // sand
					else
						c = Color.FromArgb(0, h, 0);        // grass.
					bitmap.SetPixel(x, y, c);
				}
			}

			var debugView = new DebugGeneratorEngine();
			debugView.Canvas.Image = bitmap;
			debugView.ShowDialog();
		}

		internal void DebugLayoutHeight() {
			Bitmap bitmap = new Bitmap(TileLayer.Width, TileLayer.Height);
			var c = Color.Empty;
			var h = 0;
			for (int y = 0; y < TileLayer.Height; y++) {
				for (int x = 0; x < TileLayer.Width; x++) {
					h = HeightLayout[y, x];
					if (h < 100)
						c = Color.FromArgb(0, 0, h + 80);   // water
					else if (h < 110)
						c = Color.FromArgb(300 - h, 300 - h, 0);        // sand
					else
						c = Color.FromArgb(0, h, 0);        // grass.
					bitmap.SetPixel(x, y, c);
				}
			}

			var debugView = new DebugGeneratorEngine();
			debugView.Canvas.Image = bitmap;
			debugView.ShowDialog();
		}
	}
}
