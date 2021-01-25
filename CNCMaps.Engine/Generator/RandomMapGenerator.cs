using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CNCMaps.Shared.Generator;
using NLog;

namespace CNCMaps.Engine.Generator {

	public class RandomMapGenerator {
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		public RandomMapGenerator(Settings generatorSettings ) {
			Settings = generatorSettings;
		}

		private Settings Settings { get; }

		internal void GenerateMap() {
			DumpPropertiesToLog();
		}

		private void DumpPropertiesToLog() {
			_logger.Debug($"Random map generator map size: {Settings.MapSize}");
		}
	}
}
