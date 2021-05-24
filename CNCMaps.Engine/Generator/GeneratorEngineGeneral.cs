using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CNCMaps.Shared.Generator;
using NLog;

namespace CNCMaps.Engine.Generator {
	public class GeneratorEngineGeneral : GeneratorEngine {
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		public GeneratorEngineGeneral(Settings settings, int perlinNoiseSeed) : base(settings, _logger, perlinNoiseSeed) {
		}

	}
}
