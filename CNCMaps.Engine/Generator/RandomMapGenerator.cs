using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CNCMaps.Shared.Generator;
using NLog;
using CNCMaps.Shared;

namespace CNCMaps.Engine.Generator {

	public class RandomMapGenerator {
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		public RandomMapGenerator(Settings generatorSettings, EngineType engine) {
			Settings = generatorSettings;
			Engine = engine;
		}

		private Settings Settings { get; }
		private EngineType Engine { get; }

		internal bool GenerateMap() {
			DumpPropertiesToLog();
			if (!ValidateParameters()) return false;
			return true;
		}

		private bool ValidateParameters() {
			if (Engine == EngineType.AutoDetect) {
				_logger.Error("Engine type must be defined.");
				return false;
			}
			if (string.IsNullOrEmpty(Settings.OutputFile)) {
				_logger.Error("Output file is not defined.");
				return false;
			}
			return true;
		}

		private void DumpPropertiesToLog() {
			_logger.Debug($"Random map generator map size: {Settings.MapSize}");
			_logger.Debug($"Engine: {Engine}");
		}
	}
}
