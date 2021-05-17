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
		 */
		public const byte SeaLevel = 80 ;
		public const byte SandLevel = 90;
		public const byte GroundLevel = 110;
		public const byte HeightInterval = (256 - GroundLevel) / 14;

		public GeneratorEngine(Settings settings, Logger logger) {
			Settings = settings;
			Noise = new PerlinNoise(222);   // todo: set seed to Environment.TickCount
			_logger = logger;
		}

		internal Settings Settings { get; }
		public TileLayer TileLayer { get; set; }
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
					Height= 50;     // 50 x 400 works. It just a matter of being able to place the players.
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
					Height = 200;
					Width = 200;
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
			basic.SetValue("Percent", "0");         // todo: What is this?
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
			waypoints.SetValue("0", "65055");
			waypoints.SetValue("1", "66055");
			waypoints.SetValue("2", "67055");
			waypoints.SetValue("3", "68055");
			waypoints.SetValue("4", "65056");
			waypoints.SetValue("5", "66056");
			waypoints.SetValue("6", "67056");
			waypoints.SetValue("7", "68056");
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

		public void GenerateHeightLayout(double noiseOffset, bool debug) {
			_logger.Debug("Generating height layout.");
			HeightLayout = new byte[Width * 2 - 1, Height];
			var nv = 0d;
			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width * 2 - 2; x++) {
					nv = Noise.Noise(x * noiseOffset, y * noiseOffset, 0d) + 1d;
					HeightLayout[x, y] = (byte)(nv * 128);
				}
			}
			if (debug)
				DebugLayoutHeight();
		}

		internal void DebugLayoutHeight() {
			Bitmap bitmap = new Bitmap(Width * 2 - 1, Height);
			var c = Color.Empty;
			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width * 2 - 1; x++) {
					var h = HeightLayout[x, y];
					if (h <= SeaLevel)
						c = Color.FromArgb(0, 0, h + 80);   // water
					else if (h <= SandLevel)
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

		public void DefineZFromHeightLayout() {
			IsoTile currentTile;
			_logger.Debug("Defining Z from height layout.");
			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width * 2 - 1; x++) {
					var h = HeightLayout[x, y];
					currentTile = TileLayer[x, y];
					if (h <= SeaLevel) {
						currentTile.Ground = IsoTile.GroundType.Water;
						currentTile.Z = 0;
					}
					else if (h <= SandLevel) {
						currentTile.Ground = IsoTile.GroundType.Sand;
						currentTile.Z = 0;
					}
					else if (h <= GroundLevel) {
						currentTile.Z = 0;
					}
					else {
						var hLevel = (byte)((h - GroundLevel) / HeightInterval);
						currentTile.Z = hLevel;
					}
				}
			}
		}

		// Make sure that neighbour z is not jumping more than 1.
		// Sea and sand level might be raised.
		// Repeat this as many times needed until no changes has been made.
		// Max 15 repeats.
		// Check for Pits and spikes and water connection at the end. Only needed once.
		public void LevelOut() {
			_logger.Debug("Leveling out.");
			int pass = 0;
			bool changed;
			do {
				changed = false;
				for (int y = 0; y < Height; y++) {
					for (int x = 0; x < Width * 2 - 1; x++) {
						changed = CheckLevel(x, y) || changed;
					}
				}
			} while (changed == true && pass++ < 15);
			CheckForPitsAndSpikes();
			CheckForWaterNextToHighGround();	// Check this before water connections.
			CheckForWaterConnections();
		}

		// Check the level of the tile[x, y].
		// If it is more that +/1 against either the topleft, top, topright or the left tile it is corrected.
		public bool CheckLevel(int x, int y) {
			var ct = TileLayer[x, y];
			var changed = false;
			changed = CheckTileLevel(ct, TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft)) || changed;
			changed = CheckTileLevel(ct, TileLayer.GridTile(x, y, TileLayer.TileDirection.Top)) || changed;
			changed = CheckTileLevel(ct, TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight)) || changed;
			changed = CheckTileLevel(ct, TileLayer.GridTile(x, y, TileLayer.TileDirection.Left)) || changed;
			return changed;
		}

		// Check the current tile against the validated tile.
		// If the current tile is altered the ground type is reset to ground (removing water or sand).
		// Returns true if the current tile has been altered.
		public bool CheckTileLevel(IsoTile current, IsoTile validated, bool correctLevel = true) {
			if (validated.TileNum != -1) {
				if (validated.Z - 1 > current.Z) {
					if (correctLevel) {
						current.Z = (byte)(validated.Z - 1);
						current.Ground = IsoTile.GroundType.Ground;
					}
					return true;
				}
				if (validated.Z + 1 < current.Z) {
					if (correctLevel) {
						current.Z = (byte)(validated.Z + 1);
						current.Ground = IsoTile.GroundType.Ground;
					}
					return true;
				}
			}
			return false;
		}

		// Remove small holes and spikes.
		private void CheckForPitsAndSpikes() {
			_logger.Debug("Removing pits and spikes.");
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
				ct.Ground = IsoTile.GroundType.Ground;
			}
		}

		// Removes small spikes.
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
				ct.Ground = IsoTile.GroundType.Ground;
			}
		}

		private void CheckForWaterConnections() {
			_logger.Debug("Ensuring water connections.");
			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width * 2 - 1; x++) {
					CheckSingleWaterSpots(x, y);
				}
			}
		}

		private void CheckForWaterNextToHighGround() {
			_logger.Debug("Removing water next to high ground.");
			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width * 2 - 1; x++) {
					CheckSingleWaterNextToHighGround(x, y);
				}
			}
		}

		// Make sure that a water tile is always connected to at least one other water tile.
		public void CheckSingleWaterSpots(int x, int y) {
			if (TileLayer[x, y].Ground != IsoTile.GroundType.Water) return;
			if (TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft).Ground == IsoTile.GroundType.Water ||
				TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight).Ground == IsoTile.GroundType.Water ||
				TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomRight).Ground == IsoTile.GroundType.Water ||
				TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomLeft).Ground == IsoTile.GroundType.Water)
				return;
			TileLayer[x, y].Ground = IsoTile.GroundType.Sand;
		}

		public void CheckSingleWaterNextToHighGround(int x, int y) {
			var ct = TileLayer[x, y];
			if (TileLayer[x, y].Ground != IsoTile.GroundType.Water) return;
			if (TileLayer.GridTile(x, y, TileLayer.TileDirection.TopLeft).Z - 1 == ct.Z ||
				TileLayer.GridTile(x, y, TileLayer.TileDirection.Top).Z - 1 == ct.Z ||
				TileLayer.GridTile(x, y, TileLayer.TileDirection.TopRight).Z - 1 == ct.Z ||
				TileLayer.GridTile(x, y, TileLayer.TileDirection.Left).Z - 1 == ct.Z ||
				TileLayer.GridTile(x, y, TileLayer.TileDirection.Right).Z - 1 == ct.Z ||
				TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomLeft).Z - 1 == ct.Z ||
				TileLayer.GridTile(x, y, TileLayer.TileDirection.Bottom).Z - 1 == ct.Z ||
				TileLayer.GridTile(x, y, TileLayer.TileDirection.BottomRight).Z - 1 == ct.Z) {
				TileLayer[x, y].Ground = IsoTile.GroundType.Sand;
			}
		}
	}
}
