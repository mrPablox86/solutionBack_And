using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace battleships
{
	public class AiTester
	{
		private static readonly Logger resultsLog = LogManager.GetLogger("results");
		private readonly Settings settings;

		public AiTester(Settings settings)
		{
			this.settings = settings;
		}

		public void TestSingleFile(string exe)
		{
			var gen = new MapGenerator(settings.Width, settings.Height, settings.Ships, new Random(settings.RandomSeed));
			var vis = new GameVisualizer();
			var monitor = new ProcessMonitor(TimeSpan.FromSeconds(settings.TimeLimitSeconds*settings.GamesCount),
				settings.MemoryLimit);
			var badShots = 0;
			var crashes = 0;
			var gamesPlayed = 0;
			var shoots = new List<int>();
			var ai = new Ai(exe, monitor);
			for (var iGame = 0; iGame < settings.GamesCount; iGame++)
			{
				var map = gen.GenerateMap();
				var game = new Game(map, ai);
				RunGameToEnd(game, vis);
				gamesPlayed++;
				badShots += game.BadShots;
				if (game.AiCrashed)
				{
					crashes++;
					if (crashes > settings.CrashLimit) break;
					ai = new Ai(exe, monitor);
				}
				else
					shoots.Add(game.TurnsCount);
				if (settings.Verbose)
				{
					Console.WriteLine(
						"Game #{3,4}: Turns {0,4}, BadShots {1}{2}",
						game.TurnsCount, game.BadShots, game.AiCrashed ? ", Crashed" : "", iGame);
				}
			}
			ai.Dispose();
			WriteTotal(ai, shoots, crashes, badShots, gamesPlayed);
		}

		private void RunGameToEnd(Game game, GameVisualizer vis)
		{
			while (!game.IsOver())
			{
				game.MakeStep();
				if (settings.Interactive)
				{
					vis.Visualize(game);
					if (game.AiCrashed)
						Console.WriteLine(game.LastError.Message);
					Console.ReadKey();
				}
			}
		}

		private static void WriteTotal(Ai ai, List<int> shots, int crashes, int badShots, int gamesPlayed)
		{
			if (shots.Count == 0) shots.Add(1000*1000);
			shots.Sort();
			var median = shots.Count%2 == 1 ? shots[shots.Count/2] : (shots[shots.Count/2] + shots[(shots.Count + 1)/2])/2;
			var mean = shots.Average();
			var sigma = Math.Sqrt(shots.Average(s => (s - mean)*(s - mean)));
			var headers = FormatTableRow(new object[] {"AiName", "Mean", "Sigma", "Median", "Crashes", "BadShots%", "Games"});
			var message = FormatTableRow(new object[] {ai.Name, mean, sigma, median, crashes, (100.0*badShots) / shots.Sum(), gamesPlayed});
			resultsLog.Info(message);
			Console.WriteLine("Score statistics");
			Console.WriteLine("================");
			Console.WriteLine("    " + headers);
			Console.WriteLine("    " + message);
		}

		private static string FormatTableRow(IEnumerable<object> values)
		{
			return string.Join(" ", values.Select(v => v.ToString().Replace("\t", " ").PadRight(9).Substring(0, 9)));
		}
	}
}