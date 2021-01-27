using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CNCMaps.Shared.Generator;
using CNCMaps.Shared;
using NLog;
using CNCMaps.FileFormats.Map;
using CNCMaps.FileFormats.VirtualFileSystem;
using System.IO;
using CNCMaps.FileFormats;

namespace CNCMaps.Engine.Generator {
	internal class GeneratorEngineYR : IGeneratorEngine {
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		public GeneratorEngineYR(Settings settings) {
			Settings = settings;
		}

		private Settings Settings { get; }
		private TileLayer _tileLayer;
		private ushort Height;
		private ushort Width;

		public bool GenerateMap() {
			_logger.Info("Starting Yuri's Revenge Map Generator.");

			ParseMapSize(Settings.MapSize);

			// testing
			_tileLayer = new TileLayer(Width, Height);

			// Fill level 0 clear tiles for all array values
			// (Code from MapFile.cs)
			// Todo: Maybe move the code in both to a common file/class or something.
			for (ushort y = 0; y < Height; y++) {
				for (ushort x = 0; x <= Width * 2 - 2; x++) {
					ushort dx = (ushort)(x);
					ushort dy = (ushort)(y * 2 + x % 2);
					ushort rx = (ushort)((dx + dy) / 2 + 1);
					ushort ry = (ushort)(dy - rx + Width + 1);
					_tileLayer[x, y] = new IsoTile(dx, dy, rx, ry, 0, 0, 0, 0);
				}
			}

			// todo: Maybe use the MapFile to wrap the tilelayer into.
			// todo: MapFile can also save.
			// var mapFile = new MapFile()

			return true;
		}

		private void ParseMapSize(MapSize mapSize) {
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
			_logger.Debug($"Map width={Width} and hight={Height}.");
		}

		// todo: Possible move to an abstract class (GeneratorEngine) as this might be the same for the other generator engines.
		public bool SaveMap() {
			IniFile iniFile;
			using (var mapStream = File.Create(Settings.OutputFile)) {
				iniFile = new IniFile(mapStream, Settings.OutputFile, 0, 0);
			}
			AddIsoMapPack5Section(iniFile);
			AddMapSection(iniFile);
			iniFile.Save(Settings.OutputFile);
			return true;
		}

		private void AddIsoMapPack5Section(IniFile iniFile) {
			var isoMapPack5 = iniFile.GetOrCreateSection("IsoMapPack5");
			_tileLayer.SerializeIsoMapPack5(isoMapPack5);
		}

		private void AddMapSection(IniFile iniFile) {
			var map = iniFile.GetOrCreateSection("Map");
			map.SetValue("Size", $"0,0,{Width},{Height}");
			map.SetValue("Theater", "TEMPERATE");   // todo: Temporary value
			map.SetValue("LocalSize", $"2,4,{Width - 4},{Height - 6}"); // todo: Test with medium, large and vlarge values.
		}
	}
}
