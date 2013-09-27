﻿using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Xml;
using CNCMaps.FileFormats;
using CNCMaps.Map;
using CNCMaps.Rendering;
using CNCMaps.Utility;
using CNCMaps.VirtualFileSystem;
using System;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace CNCMaps {
	class Program {
		static OptionSet _options;
		static Logger _logger;
		public static RenderSettings Settings;

		public static int Main(string[] args) {
			InitLoggerConfig();
			InitSettings(args);
			if (!ValidateSettings())
				return 2;

			try {
				_logger.Info("Initializing virtual filesystem");

				var mapStream = File.Open(Settings.InputFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				VirtualFile mapFile;
				var mixMap = new MixFile(mapStream, Settings.InputFile, 0, mapStream.Length, false, false);
				if (mixMap.IsValid()) { // input max is a mix
					var mapArchive = new MixFile(mapStream, Path.GetFileName(Settings.InputFile), true);
					// grab the largest file in the archive
					var mixEntry = mapArchive.Index.OrderByDescending(me => me.Value.Length).First();
					mapFile = mapArchive.OpenFile(mixEntry.Key);
				}
				else {
					mapFile = new VirtualFile(mapStream, Path.GetFileName(Settings.InputFile), true);
				}
				var map = new MapFile(mapFile, Path.GetFileName(Settings.InputFile));

				// ---------------------------------------------------------------
				// Code to organize moving of maps in a directory for themselves
				/*map.EngineType = Settings.Engine;
				string mapName = map.DetermineMapName();
				string dir = Path.Combine(Path.GetDirectoryName(Settings.InputFile), mapName);
				if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
				map.Close();
				map.Dispose();
				File.Move(Settings.InputFile, Path.Combine(dir, Path.GetFileName(map.FileName)));
				return 0;*/
				// ---------------------------------------------------------------

				if (!string.IsNullOrEmpty(Settings.ModConfig)) {
					if (File.Exists(Settings.ModConfig)) {
						ModConfig cfg;
						try {
							using (FileStream f = File.OpenRead(Settings.ModConfig))
								cfg = ModConfig.Deserialize(f);
							ModConfig.ActiveConfig = cfg;
							if (Settings.Engine != EngineType.AutoDetect) {
								if (Settings.Engine != cfg.Engine)
									_logger.Warn("Provided engine override does not match mod config.");
							}
							else
								Settings.Engine = ModConfig.ActiveConfig.Engine;
						}
						catch (IOException) {
							_logger.Error("IOException while loading mod config");
						}
						catch (XmlException) {
							_logger.Error("XmlException while loading mod config");
						}
						catch (SerializationException) {
							_logger.Error("Serialization exception while loading mod config");
						}
					}
					else {
						_logger.Error("Invalid mod config file specified");
					}
				}
				if (!map.LoadMap(Settings.Engine)) {
					_logger.Error("Could not successfully load this map. Try specifying the engine type manually.");
					return 1;
				}

				// enginetype is now definitive, load mod config
				if (ModConfig.ActiveConfig == null)
					ModConfig.LoadDefaultConfig(map.Engine);

				foreach (string modDir in ModConfig.ActiveConfig.ExtraDirectories)
					VFS.Add(modDir);
				
				// add mixdir to VFS (if it's not included in the mod config)
				string mixDir = VFS.DetermineMixDir(Settings.MixFilesDirectory, map.Engine);
				if (ModConfig.ActiveConfig.ExtraDirectories.All(d => 0 != String.Compare(Path.GetFullPath(d).TrimEnd('\\'), mixDir.TrimEnd('\\'), StringComparison.InvariantCultureIgnoreCase)))
					VFS.Add(mixDir);

				foreach (string mixFile in ModConfig.ActiveConfig.ExtraMixes)
					VFS.Add(mixFile);

				// now load the stuff the unmodified game would also load
				if (!VFS.GetInstance().ScanMixDir(mixDir, map.Engine)) {
					_logger.Fatal("Scanning for mix files failed. If on Linux, specify the --mixdir command line argument");
					return 2;
				}

				if (!map.LoadTheater()) {
					_logger.Error("Could not successfully load all required components for this map. Aborting.");
					return 1;
				}

				if (Settings.StartPositionMarking == StartPositionMarking.Tiled)
					map.MarkTiledStartPositions();

				if (Settings.MarkOreFields)
					map.MarkOreAndGems();

				map.DrawMap();

#if DEBUG
				// ====================================================================================
				using (var form = new DebugDrawingSurfaceWindow(map.GetDrawingSurface(), map.GetTiles(), map.GetTheater(), map)) {
					form.RequestTileEvaluate += map.DebugDrawTile; form.ShowDialog();
				}
				// ====================================================================================
#endif

				if (Settings.StartPositionMarking == StartPositionMarking.Squared)
					map.DrawSquaredStartPositions();

				if (Settings.OutputFile == "")
					Settings.OutputFile = map.DetermineMapName();

				if (Settings.OutputDir == "")
					Settings.OutputDir = Path.GetDirectoryName(Settings.InputFile);

				// free up as much memory as possible before saving the large images
				Rectangle saveRect = map.GetSizePixels(Settings.SizeMode);
				DrawingSurface ds = map.GetDrawingSurface();
				// if we don't need this data anymore, we can try to save some memory
				if (!Settings.GeneratePreviewPack) {
					ds.FreeNonBitmap();
					map.FreeUseless();
					GC.Collect();
				}

				if (Settings.SaveJPEG)
					ds.SaveJPEG(Path.Combine(Settings.OutputDir, Settings.OutputFile + ".jpg"), Settings.JPEGCompression, saveRect);

				if (Settings.SavePNG)
					ds.SavePNG(Path.Combine(Settings.OutputDir, Settings.OutputFile + ".png"), Settings.PNGQuality, saveRect);

				Regex reThumb = new Regex(@"(\+)?\((\d+),(\d+)\)");
				var match = reThumb.Match(Settings.ThumbnailSettings);
				if (match.Success) {
					Size dimensions = new Size(
						int.Parse(match.Groups[2].Captures[0].Value),
						int.Parse(match.Groups[3].Captures[0].Value));
					var cutRect = map.GetSizePixels(Settings.SizeMode);

					if (match.Groups[1].Captures[0].Value == "+") {
						// + means maintain aspect ratio
						double aspectRatio = cutRect.Width / (double)cutRect.Height;
						if (dimensions.Width / (double)dimensions.Height > aspectRatio) {
							dimensions.Height = (int)(dimensions.Width / aspectRatio);
						}
						else {
							dimensions.Width = (int)(dimensions.Height / aspectRatio);
						}
					}
					_logger.Info("Saving thumbnail with dimensions {0}x{1}", dimensions.Width, dimensions.Height);
					ds.SaveThumb(dimensions, cutRect, Path.Combine(Settings.OutputDir, "thumb_" + Settings.OutputFile + ".jpg"));
				}

				if (Settings.GeneratePreviewPack) {
					if (mapFile.BaseStream is MixFile)
						_logger.Error("Cannot inject thumbnail into an archive (.mmx/.yro/.mix)!");
					else
						map.GeneratePreviewPack(Settings.OmitPreviewPackMarkers);
				}
			}
			catch (Exception exc) {
				_logger.Error(string.Format("An unknown fatal exception occured: {0}", exc), exc);
#if DEBUG
				throw;
#else
				return 1;
#endif
			}

			LogManager.Configuration = null; // required for mono release to flush possible targets
			return 0;
		}


		private static void InitLoggerConfig() {
			// for release, logmanager automatically picks the correct NLog.config file
#if DEBUG
			try {
				LogManager.Configuration = new XmlLoggingConfiguration("NLog.Debug.config");
			}
			catch {
			}
#endif
			if (LogManager.Configuration == null) {
				// init default config
				var target = new ColoredConsoleTarget();
				target.Name = "console";
				target.Layout = "${processtime:format=s\\.ffff} [${level}] ${message}";
				target.RowHighlightingRules.Add(new ConsoleRowHighlightingRule() {
					ForegroundColor = ConsoleOutputColor.Magenta,
					Condition = "level = LogLevel.Fatal"
				});
				target.RowHighlightingRules.Add(new ConsoleRowHighlightingRule() {
					ForegroundColor = ConsoleOutputColor.Red,
					Condition = "level = LogLevel.Error"
				});
				target.RowHighlightingRules.Add(new ConsoleRowHighlightingRule() {
					ForegroundColor = ConsoleOutputColor.Yellow,
					Condition = "level = LogLevel.Warn"
				});
				target.RowHighlightingRules.Add(new ConsoleRowHighlightingRule() {
					ForegroundColor = ConsoleOutputColor.Gray,
					Condition = "level = LogLevel.Info"
				});
				target.RowHighlightingRules.Add(new ConsoleRowHighlightingRule() {
					ForegroundColor = ConsoleOutputColor.DarkGray,
					Condition = "level = LogLevel.Debug"
				});
				target.RowHighlightingRules.Add(new ConsoleRowHighlightingRule() {
					ForegroundColor = ConsoleOutputColor.White,
					Condition = "level = LogLevel.Trace"
				});
				LogManager.Configuration = new LoggingConfiguration();
				LogManager.Configuration.AddTarget("console", target);
#if DEBUG
				LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, target));
#else
				LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, target));

#endif
				LogManager.ReconfigExistingLoggers();
			}
			_logger = LogManager.GetCurrentClassLogger();
		}

		private static void InitSettings(string[] args) {
			Settings = RenderSettings.CreateDefaults();
			_options = new OptionSet {
				{"h|help", "Show this short help text", v => Settings.ShowHelp = true},
				{"i|infile=", "Input file", v => Settings.InputFile = v},
				{"o|outfile=", "Output file, without extension, read from map if not specified.", v => Settings.OutputFile = v},
				{"d|outdir=", "Output directiory", v => Settings.OutputDir = v},
				{"y|force-ra2", "Force using the Red Alert 2 engine for rendering", v => Settings.Engine = EngineType.RedAlert2}, 
				{"Y|force-yr", "Force using the Yuri's Revenge engine for rendering", v => Settings.Engine = EngineType.YurisRevenge},
				{"t|force-ts", "Force using the Tiberian Sun engine for rendering", v => Settings.Engine = EngineType.TiberianSun},
				{"T|force-fs", "Force using the Firestorm engine for rendering", v => Settings.Engine = EngineType.Firestorm},
				{"j|output-jpg", "Output JPEG file", v => Settings.SaveJPEG = true},
				{"q|jpeg-quality=", "Set JPEG quality level (0-100)", (int v) => Settings.JPEGCompression = v},
				{"p|output-png", "Output PNG file", v => Settings.SavePNG = true},
				{"c|png-compression=", "Set PNG compression level (1-9)", (int v) => Settings.PNGQuality = v}, 
				{"m|mixdir=", "Specify location of .mix files, read from registry if not specified (win only)",v => Settings.MixFilesDirectory = v},
				{"M|modconfig=", "Filename of a game configuration specific to your mod (create with GUI)",v => Settings.ModConfig = v},
				{"s|start-pos-tiled", "Mark starting positions in a tiled manner",v => Settings.StartPositionMarking = StartPositionMarking.Tiled},
				{"S|start-pos-squared", "Mark starting positions in a squared manner",v => Settings.StartPositionMarking = StartPositionMarking.Squared}, 
				{"r|mark-ore", "Mark ore and gem fields more explicity, looks good when resizing to a preview",v => Settings.MarkOreFields = true},
				{"F|force-fullmap", "Ignore LocalSize definition and just save the full map", v => Settings.SizeMode = SizeMode.Full},
				{"f|force-localsize", "Use localsize for map dimensions (default)", v => Settings.SizeMode = SizeMode.Local}, 
				{"k|replace-preview", "Update the maps [PreviewPack] data with the rendered image",v => Settings.GeneratePreviewPack = true}, 
				{"n|ignore-lighting", "Ignore all lighting and lamps on the map",v => Settings.IgnoreLighting = true}, 
				{"K|replace-preview-nosquares", "Update the maps [PreviewPack] data with the rendered image, without squares",
					v => {
						Settings.GeneratePreviewPack = true;
						Settings.OmitPreviewPackMarkers = true;
					}
				}, 
				{"G|graphics-winmgr", "Attempt rendering voxels using window manager context first (default)",v => Settings.PreferOSMesa = false},
				{"g|graphics-osmesa", "Attempt rendering voxels using OSMesa context first", v => Settings.PreferOSMesa = true},
				{"z|create-thumbnail=", "Also save a thumbnail along with the fullmap in dimensions (x,y), prefix with + to keep aspect ratio	", v => Settings.ThumbnailSettings = v},
			};

			_options.Parse(args);
		}

		private static bool ValidateSettings() {
			if (Settings.ShowHelp) {
				ShowHelp();
				return false; // not really false :/
			}
			else if (!File.Exists(Settings.InputFile)) {
				_logger.Error("Specified input file does not exist");
				return false;
			}
			else if (!Settings.SaveJPEG && !Settings.SavePNG && !Settings.GeneratePreviewPack) {
				_logger.Error("No output format selected. Either specify -j, -p, -k or a combination");
				return false;
			}
			else if (Settings.OutputDir != "" && !System.IO.Directory.Exists(Settings.OutputDir)) {
				_logger.Error("Specified output directory does not exist.");
				return false;
			}
			return true;
		}

		private static void ShowHelp() {
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.Write("Usage: ");
			Console.WriteLine("");
			var sb = new System.Text.StringBuilder();
			var sw = new StringWriter(sb);
			_options.WriteOptionDescriptions(sw);
			Console.WriteLine(sb.ToString());
		}

		public static bool IsLinux {
			get {
				int p = (int)Environment.OSVersion.Platform;
				return (p == 4) || (p == 6) || (p == 128);
			}
		}
	}
}