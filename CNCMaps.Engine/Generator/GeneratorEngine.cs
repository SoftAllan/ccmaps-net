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
			AddIsoMapPack5Section(iniFile);
			AddMapSection(iniFile);
			iniFile.Save(Settings.OutputFile);
			return true;
		}

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
		public void InitialiseMapLayer() {
			_tileLayer = new TileLayer(Width, Height);
			for (ushort y = 0; y < Height; y++) {
				for (ushort x = 0; x <= Width * 2 - 2; x++) {
					ushort dx = (ushort)(x);
					ushort dy = (ushort)(y * 2 + x % 2);
					ushort rx = (ushort)((dx + dy) / 2 + 1);
					ushort ry = (ushort)(dy - rx + Width + 1);
					_tileLayer[x, y] = new IsoTile(dx, dy, rx, ry, 0, 0, 0, 0);
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

		internal void AddIsoMapPack5Section(IniFile iniFile) {
			var isoMapPack5 = iniFile.GetOrCreateSection("IsoMapPack5");
			_tileLayer.SerializeIsoMapPack5(isoMapPack5);
		}

		internal void AddMapSection(IniFile iniFile) {
			var map = iniFile.GetOrCreateSection("Map");
			map.SetValue("Size", $"0,0,{Width},{Height}");
			map.SetValue("Theater", TheaterType(Settings.TheaterType).ToUpper());   // todo: Test if uppercase is needed.
			map.SetValue("LocalSize", $"2,4,{Width - 4},{Height - 6}"); // todo: Test with medium, large and vlarge values.
		}

	}
}
