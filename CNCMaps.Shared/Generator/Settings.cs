using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CNCMaps.Shared.Generator {
	
	// todo: Option to just generate the map without player position and so on.
	// Can be used as base map for the final alert editer.
	public class Settings {
		
		public MapSize MapSize { get; set; }
		public string OutputFile { get; set; }
		public TheaterType TheaterType { get; set; }
	}
}
