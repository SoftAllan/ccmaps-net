using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CNCMaps.Shared.Generator;
using CNCMaps.Shared;
using NLog;
using CNCMaps.FileFormats.Map;
using CNCMaps.FileFormats.VirtualFileSystem;
using System.IO;
using CNCMaps.FileFormats;

namespace CNCMaps.Engine.Generator {
	internal class GeneratorEngineYR : GeneratorEngine {
		
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		public GeneratorEngineYR(Settings settings) : base(settings) { }

		public override bool GenerateMap() {
			_logger.Info("Starting Yuri's Revenge Map Generator.");

			ParseMapSize(Settings.MapSize);
			_logger.Debug($"Map width={Width} and hight={Height}.");

			InitialiseMapLayer();

			// todo: Maybe use the MapFile to wrap the tilelayer into.
			// todo: MapFile can also save.
			// var mapFile = new MapFile()

			return true;
		}

	}
}
