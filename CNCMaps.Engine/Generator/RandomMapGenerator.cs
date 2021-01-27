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
			IGeneratorEngine generatorEngine = CreateGeneratorEngine();
			if (generatorEngine == null) return false;
			if (!generatorEngine.GenerateMap()) return false;
			if (!generatorEngine.SaveMap()) return false;
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

		private IGeneratorEngine CreateGeneratorEngine() {
			IGeneratorEngine newEngine = null;
			switch (Engine) {
				case EngineType.TiberianSun:
					// todo: Test
					_logger.Error("Random generator engine for Tiberian Sun not implemented.");
					break;
				case EngineType.Firestorm:
					// todo: Test
					_logger.Error("Random generator engine for Firestorm not implemented.");
					break;
				case EngineType.RedAlert2:
					// todo: Test
					_logger.Error("Random generator engine for Red Alert 2 not implemented.");
					break;
				case EngineType.YurisRevenge:
					// todo: Test
					newEngine = new GeneratorEngineYR(Settings);
					break;
				default:
					throw new ArgumentException($"Unknown engine type: {Engine}");
			}
			return newEngine;
		}
	}
}
