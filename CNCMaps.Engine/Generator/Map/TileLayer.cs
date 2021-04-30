using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using CNCMaps.FileFormats.Encodings;
using CNCMaps.Shared;
using CNCMaps.Shared.Utility;
using NLog;

namespace CNCMaps.Engine.Generator.Map {
	public class TileLayer : IEnumerable<IsoTile> {

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

		// Todo: Examine if this is needed.
		/// <summary>Gets a tile at map coordinates.</summary>
		/// <param name="rx">The rx.</param>
		/// <param name="ry">The ry.</param>
		/// <returns>The tile r.</returns>
		public IsoTile GetTileR(int rx, int ry) {
			int dx = (rx - ry + fullSize.Width - 1);
			int dy = rx + ry - fullSize.Width - 1;

			if (dx < 0 || dy < 0 || dx >= isoTiles.GetLength(0) || (dy / 2) >= isoTiles.GetLength(1))
				return null;
			else
				return GetTile(dx, dy / 2);
		}

		// Todo: The enumerator might not be needed.
		#region enumerator stuff
		public IEnumerator<IsoTile> GetEnumerator() {
			return new TwoDimensionalEnumerator<IsoTile>(isoTiles);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return new TwoDimensionalEnumerator<IsoTile>(isoTiles);
		}
		#endregion

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

		public IsoTile GetNeighbourTile(IsoTile t, TileDirection tileDirection) {
			// find index for t
			int x = t.Dx;
			int y = (t.Dy + (t.Dx + 1) % 2) / 2;
			return GetNeighbourTile(x, y, tileDirection);
		}

		public IsoTile GetNeighbourTile(int x, int y, TileDirection direction) {
			switch (direction) {
				// in non-diagonal direction we don't need to check odd/evenness of x
				case TileDirection.Bottom:
					if (y >= isoTiles.GetLength(1)) return null;
					return this[x, y + 1];

				case TileDirection.Top:
					if (y < 2) return null;
					return this[x, y - 1];

				case TileDirection.Left:
					if (x < 2) return null;
					return this[x - 2, y];

				case TileDirection.Right:
					if (x >= isoTiles.GetLength(0) - 1) return null;
					return this[x + 2, y];
			}

			// the horizontally neighbouring tiles have dy' = dy + 1 if x is odd,
			// and the horizontally neighbouring tiles have dy' = dy - 1 if x is even,
			y += x % 2;
			switch (direction) {
				case TileDirection.BottomLeft:
					if (x < 1 || y >= isoTiles.GetLength(1)) return null;
					return this[x - 1, y];

				case TileDirection.BottomRight:
					if (x >= isoTiles.GetLength(0) || y >= isoTiles.GetLength(1)) return null;
					return this[x + 1, y];

				case TileDirection.TopLeft:
					if (x < 1 || y < 1) return null;
					return this[x - 1, y - 1];

				case TileDirection.TopRight:
					if (y < 1 || x >= isoTiles.GetLength(0) - 1) return null;
					return this[x + 1, y - 1];
			}
			throw new InvalidOperationException();
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
	}
}
