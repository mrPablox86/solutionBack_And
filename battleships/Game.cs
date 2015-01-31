using System;
using System.Linq;
using NLog;

namespace battleships
{
	public class ShootInfo
	{
		public ShtEffct Hit;
		public Vector Target;
	}

	public class Game
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();
		public Vector LastTarget;
		private readonly Ai ai;

		public Game(Map map, Ai ai)
		{
			Map = map;
			this.ai = ai;
			TurnsCount = 0;
			BadShots = 0;
		}

		public int TurnsCount { get; private set; }
		public int BadShots { get; private set; }
		public Map Map { get; private set; }
		public ShootInfo LastShootInfo { get; private set; }
		public bool AiCrashed { get; private set; }
		public Exception LastError { get; private set; }

		public bool IsOver()
		{
			return !Map.HasAliveShips() || AiCrashed;
		}

		public void MakeStep()
		{
			if (IsOver()) throw new InvalidOperationException("Game is Over");
			if (!UpdateLastTarget()) return;
			if (IsBadShot(LastTarget)) BadShots++;
			var hit = Map.Badaboom(LastTarget);
			LastShootInfo = new ShootInfo {Target = LastTarget, Hit = hit};
			if (hit == ShtEffct.Miss)
				TurnsCount++;
		}

		private bool UpdateLastTarget()
		{
			try
			{
				LastTarget = LastTarget == null
					? ai.Init(Map.Width, Map.Height, Map.Ships.Select(s => s.Size).ToArray())
					: ai.GetNextShoot(LastShootInfo.Target, LastShootInfo.Hit);
				return true;
			}
			catch (Exception e)
			{
				AiCrashed = true;
				log.Info("Ai {0} crashed", ai.Name);
				log.Error(e);
				LastError = e;
				return false;
			}
		}

		private bool IsBadShot(Vector target)
		{
			var corners = new[] {new Vector(-1, -1), new Vector(-1, 1), new Vector(1, -1), new Vector(1, 1)};
			return
				Map[target] != MapCell.Empty && Map[target] != MapCell.Ship
				|| corners.Any(d => Map[target.Add(d)] == MapCell.DeadShip)
				|| Map.Near(target).Any(c => Map.shipsMap[c.X, c.Y] != null && !Map.shipsMap[c.X, c.Y].Alive);
		}
	}
}