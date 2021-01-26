using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CNCMaps.Shared.Generator;
using NLog;

namespace CNCMaps.Engine.Generator {
	internal class GeneratorEngineYR : IGeneratorEngine {
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		public GeneratorEngineYR(Settings settings) {
			Settings = settings;
		}

		private Settings Settings { get; }

		public bool GenerateMap() {
			_logger.Info("Starting Yuri's Revenge Map Generator.");
			return false;
		}

		public bool SaveMap() {
			throw new NotImplementedException();
		}
	}
}
