﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CNCMaps.FileFormats.Encodings;
using CNCMaps.Shared;
using CNCMaps.Shared.Utility;
using NLog;

namespace CNCMaps.Engine.Generator.Map {

	public delegate void IsoTileChange(IsoTile isoTile);
	
	public class TileLayer {

		static Logger logger = LogManager.GetCurrentClassLogger();

		private IsoTile[,] isoTiles;
		private Size fullSize;

		public TileLayer(int w, int h)
			: this(new Size(w, h)) {
		}

		public TileLayer(Size fullSize) {
			this.fullSize = fullSize;
			isoTiles = new IsoTile[fullSize.Width * 2 - 1, fullSize.Height];
		}

		public int Width {
			get { return fullSize.Width; }
		}

		public int Height {
			get { return fullSize.Height; }
		}

		public virtual IsoTile this[int x, int y] {
			get {
				if (0 <= x && x < isoTiles.GetLength(0) && 0 <= y && y < isoTiles.GetLength(1))
					return isoTiles[x, y];
				else
					return null;
			}
			set {
				isoTiles[x, y] = value;
			}
		}

		/// <summary>Gets a tile at display coordinates.</summary>
		/// <param name="dx">The dx.</param>
		/// <param name="dy">The dy.</param>
		/// <returns>The tile.</returns>
		public IsoTile GetTile(int dx, int dy) {
			return isoTiles[dx, dy];
		}

		public enum TileDirection {
			Top,
			TopLeft,
			TopRight,
			Left,
			Right,
			BottomLeft,
			Bottom,
			BottomRight
		}

		public void SerializeIsoMapPack5(FileFormats.IniFile.IniSection isoMapPack5) {
			List<IsoTile> tileSet = new List<IsoTile>();
			byte[] encoded;

			foreach (var isoTile in this.isoTiles) {
				tileSet.Add(isoTile);
			}

			encoded = GetEncoded(tileSet);

			string compressed64 = Convert.ToBase64String(encoded, Base64FormattingOptions.None);

			int i = 1;
			int idx = 0;
			isoMapPack5.Clear();
			while (idx < compressed64.Length) {
				int adv = Math.Min(74, compressed64.Length - idx);
				isoMapPack5.SetValue(i++.ToString(), compressed64.Substring(idx, adv));
				idx += adv;
			}
		}

		private byte[] GetEncoded(List<IsoTile> tileSetParam) {
			// A tile is of 11 bytes. Last 4 bytes of padding is used for termination
			byte[] isoMapPack = new byte[tileSetParam.Count * 11 + 4];

			long di = 0;
			foreach (var tile in tileSetParam) {
				var bs = tile.ToMapPack5Entry().ToArray();
				Array.Copy(bs, 0, isoMapPack, di, 11);
				di += 11;
			}

			return Format5.Encode(isoMapPack, 5);
		}

		public IsoTile GridTile(int x, int y, TileLayer.TileDirection direction) {
			switch (direction) {
				case TileDirection.Top: 
					return y - 1 >= 0 ? isoTiles[x, y - 1] : GetInvalidTile();
				case TileDirection.TopLeft:
					y += x % 2;
					return y - 1 >= 0 && x - 1 >= 0 ? isoTiles[x - 1, y - 1] : GetInvalidTile();
				case TileDirection.TopRight:
					y += x % 2;
					return y - 1 >= 0 && x + 1 < Width * 2 - 1 ? isoTiles[x + 1, y - 1] : GetInvalidTile();
				case TileDirection.Left:
					return x - 2 >= 0 ? isoTiles[x - 2, y] : GetInvalidTile();
				case TileDirection.Right:
					return x + 2 < Width * 2 - 1 ? isoTiles[x + 2, y] : GetInvalidTile();
				case TileDirection.BottomLeft:
					y += x % 2;
					return y < Height && x - 1 >= 0 ? isoTiles[x - 1, y] : GetInvalidTile();
				case TileDirection.Bottom:
					return y + 1 < Height ? isoTiles[x, y + 1] : GetInvalidTile();
				case TileDirection.BottomRight:
					y += x % 2;
					return y < Height && x + 1 < Width * 2 - 1 ? isoTiles[x + 1, y] : GetInvalidTile();
				default:
					return GetInvalidTile();
			}
		}

		private IsoTile GetInvalidTile() {
			return new IsoTile(0, 0, 0, 0, 0, -1, 0, 0);
		}

		private const string _debugMapFile = "DebugMapFile.txt";
		private string DebugMapFile() {
			var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var file = Path.Combine(dir, _debugMapFile);
			return file;
		}

		public void DumpZToFile() {
			var file = DebugMapFile();
			logger.Debug($"Dumping Z order to {file}");
			var sb = new StringBuilder();
			sb.Append("    ");
			for (int x = 0; x < Width * 2 - 1; x++) {
				sb.Append($"{x,3} ");
			}
			sb.AppendLine();
			for (int y = 0; y < Height; y++) {
				sb.Append($"{y,4}");
				for (int x = 0; x < Width * 2 - 1; x++) {
					var h = $"{isoTiles[x, y].Z,0:X}";
					char t;
					switch (isoTiles[x, y].Ground) {
						case IsoTile.GroundType.Water:
							t = 'w';
							break;
						case IsoTile.GroundType.Sand:
							t = 's';
							break;
						default:
							t = ' ';
							break;
					}
					sb.Append($"[{t}{h}]");
				}
				sb.AppendLine();
			}
			using (var writeFile = new StreamWriter(file)) {
				writeFile.Write(sb);
			}
		}

		public void DumpPlayerZoneToFile() {
			var file = DebugMapFile();
			logger.Debug($"Dumping Player zone to {file}");
			var sb = new StringBuilder();
			sb.Append("   ");
			for (int x = 0; x < Width * 2 - 1; x++) {
				sb.Append($"{x,2} ");
			}
			sb.AppendLine();
			for (int y = 0; y < Height; y++) {
				sb.Append($"{y,3}");
				for (int x = 0; x < Width * 2 - 1; x++) {
					var h = $"{isoTiles[x, y].PlayerZone}";
					sb.Append($"[{h}]");
				}
				sb.AppendLine();
			}
			using (var writeFile = new StreamWriter(file)) {
				writeFile.Write(sb);
			}
		}


		public void DumpToFile() {
			var file = DebugMapFile();
			logger.Debug($"Dumping to {file}");
			var sb = new StringBuilder();
			sb.Append("     ");
			for (int x = 0; x < Width * 2 - 1; x++) {
				sb.Append($"{x,9} ");
			}
			sb.AppendLine();
			for (int y = 0; y < Height; y++) {
				sb.Append($"{y,4}");
				for (int x = 0; x < Width * 2 - 1; x++) {
					var h = $"{isoTiles[x, y].Z,0:X}";
					var n = $"{isoTiles[x, y].TileNum:D3}";
					var s = $"{isoTiles[x, y].SubTile}";
					char t;
					switch (isoTiles[x, y].Ground) {
						case IsoTile.GroundType.Water:
							t = 'w';
							break;
						case IsoTile.GroundType.Sand:
							t = 's';
							break;
						default:
							t = ' ';
							break;
					}
					sb.Append($"[{t}{h}#{n}:{s}]");
				}
				sb.AppendLine();
			}
			using (var writeFile = new StreamWriter(file)) {
				writeFile.Write(sb);
			}
		}

		// Define a circle and calls the IsoTileChange delegate for each isoTile in the defined circle.
		public void DefineCircle(int centerX, int centerY, int radius, IsoTileChange isoTileChange) {
			int xFrom = centerX - radius;
			if (xFrom < 0) xFrom = 0;
			int yFrom = centerY - radius;
			if (yFrom < 0) yFrom = 0;
			int diameter = (radius * 2) + 1;
			int xTo = xFrom + diameter;
			if (xTo > Width * 2 - 1) xTo = Width * 2 - 1;
			int yTo = yFrom + diameter;
			if (yTo > Height) yTo = Height;
			int radiusSq = (diameter * diameter) / 4;
			for (int y = yFrom; y < yTo; y++) {
				int yDiff = y - centerY;
				int threshold = radiusSq - (yDiff * yDiff);
				for (int x = xFrom; x < xTo; x++) {
					int d = x - centerX;
					if ((d * d) <= threshold) isoTileChange(this[x, y]);
				}
			}
		}

		public void ResetPlayerZone() {
			for (int x = 0; x < Width * 2 - 1; x++) {
				for (int y = 0; y < Height; y++) {
					this[x, y].PlayerZone = 0;
				}
			}
		}
	}
}
