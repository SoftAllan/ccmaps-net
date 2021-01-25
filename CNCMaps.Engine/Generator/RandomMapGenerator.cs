using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CNCMaps.Shared.Generator;

namespace CNCMaps.Engine.Generator {

	public class RandomMapGenerator {

		public RandomMapGenerator(Settings generatorSettings ) {
			Settings = generatorSettings;
		}

		private Settings Settings { get; }


	}
}
