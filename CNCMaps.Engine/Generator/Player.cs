using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CNCMaps.Engine.Generator.Map;

namespace CNCMaps.Engine.Generator {
	public class Player {
		
		public struct PlayerPos {
			public int dx;
			public int dy;
		}

		private GeneratorEngine _generatorEngine;
		private Random _random;
		
		// How many positions from the border of the map that the player position may not be placed
		private const int BorderOffset = 15;
		
		// How many positions from a water tile the player position may not be placed.
		private const int WaterOffset = 3;

		// Player zone radius from Position.
		private const int PlayerZoneRadius = 8;
		
		private int _number;

		// Used by the delegate for define circle to indicate if the current check method has raised a flag.
		private bool _checkFlag;
		
		public Player(GeneratorEngine generatorEngine, Random random, int number) {
			_generatorEngine = generatorEngine;
			_random = random;
			_number = number;
		}

		public int Number { get => _number; 
			set {
				if (value < 1 || value > 9) {
					// todo: Test < 1
					// todo: Test > 9
					throw new Exception($"Player number {value} is invalid.");
				}
				_number = value;
			} 
		}

		public PlayerPos Position { get; set; }

		public int rx => (Position.dx + Position.dy) / 2 + 1;

		public int ry => Position.dy - rx + _generatorEngine.Width + 1;

		// Place the player at a random position within the map size.
		// Returns true if it did not succeed.
		public bool SetRandomPosition() {
		    var xMax = _generatorEngine.Width * 2 - 1;
			if (xMax - BorderOffset < 0) {
				// todo: Test
				throw new Exception($"Random position for player {Number} could not be created. Size of map is too small.");
			}
			var checkError = false;
			var x = 0;
			var y = 0;
			var attempt = 50;
			do {
				attempt--;
				x = _random.Next(BorderOffset, xMax - BorderOffset);
				y = _random.Next(BorderOffset, _generatorEngine.Height - BorderOffset);
				Position = new PlayerPos() { dx = x, dy = y };
				checkError = CheckPositionToWater();
				if (!checkError) checkError = CheckPositionToOtherPlayers();
			} while (checkError && attempt > 0);
			if (checkError) {
				Position = new PlayerPos() { dx = 0, dy = 0 };
			}
			return checkError;
		}

		// Check if the players position is in the water or too close to water.
		public bool CheckPositionToWater() {
			_checkFlag = false;
			_generatorEngine.TileLayer.DefineCircle(Position.dx, Position.dy, WaterOffset, IsIsoMapWater);
			return _checkFlag;
		}

		private void IsIsoMapWater(IsoTile tile) {
			if (tile.Ground == IsoTile.GroundType.Water) _checkFlag = true;
		}

		// Check if the players position is too close to another player.
		// Returns true if it is too close.
		public bool CheckPositionToOtherPlayers() {
			_checkFlag = false;
			_generatorEngine.TileLayer.DefineCircle(Position.dx, Position.dy, PlayerZoneRadius, IsIsoTilePlayerZoneDifferent);
			return _checkFlag;
		}

		private void IsIsoTilePlayerZoneDifferent(IsoTile isoTile) {
			if (isoTile.PlayerZone != 0 && isoTile.PlayerZone != Number) _checkFlag = true;
		}

		// Creates a player zone with the specified player number.
		public void MakePlayerZone() {
			_generatorEngine.TileLayer.DefineCircle(Position.dx, Position.dy, PlayerZoneRadius, ChangeIsoTilePlayerZone);
		}

		private void ChangeIsoTilePlayerZone(IsoTile isoTile) {
			isoTile.PlayerZone = Number;
		}


	}
}
