using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CNCMaps.Engine.Generator {
	
	interface IGeneratorEngine {
		bool GenerateMap();
		bool SaveMap();
	}
}
