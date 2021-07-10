using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CNCMaps.Engine.Generator {
	public class Player {
		
		public struct PlayerPos {
			public int dx;
			public int dy;
		}

		private GeneratorEngine _generatorEngine;
		private Random _random;
		private const int BorderOffset = 15;
		private int _number;
		
		public Player(GeneratorEngine generatorEngine, Random random, int number) {
			_generatorEngine = generatorEngine;
			_random = random;
			_number = number;
		}

		public int Number { get => _number; 
			set {
				if (value < 0 || value > 9) {
					// todo: Test < 0
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
		public void SetRandomPosition() {
			var xMax = _generatorEngine.Width * 2 - 1;
			if (xMax - BorderOffset < 0) {
				// todo: Test
				throw new Exception($"Random position for player {Number} could not be created. Size of map is too small.");
			}
			// Make sure that the random position is not too close to another player.
			bool posOk;
			do {
				var x = _random.Next(BorderOffset, xMax - BorderOffset);
				var y = _random.Next(BorderOffset, _generatorEngine.Height - BorderOffset);
				// todo: Make sure that it can break out after a number of times without success.
				posOk = CheckPositionToOtherPlayers();
			} while (posOk == false);


		}

		private bool CheckPositionToOtherPlayers() {
			return true;
		}

	}
}
