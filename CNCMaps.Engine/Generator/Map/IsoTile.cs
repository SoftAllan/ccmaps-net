using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CNCMaps.Engine.Generator.Map {
	
	public class IsoTile : FileFormats.Map.IsoTile {

		public enum GroundType {
			Ground,
			Water,
			Sand
		}

		public bool SlopeChecked { get; set; }
		public GroundType Ground { get; set; }
		public IsoTile(ushort p1, ushort p2, ushort rx, ushort ry, byte z, int tilenum, byte subtile, byte icegrowth) : base(p1, p2, rx, ry, z, tilenum, subtile, icegrowth) {
			SlopeChecked = false;
			Ground = GroundType.Ground;
		}
	}
}
