// �����: ����� ������
// ����: 28.12.2015

using System;
using System.Collections.Generic;
using System.Linq;

namespace battleships
{
	///<summary>��������� ������ ����</summary>
	public enum MapCell
	{
		Empty = 0,

		Ship,

		DeadShip,
	
		Miss
	}

	///<summary>��������� ��������</summary>
	public enum ShtEffct
	{
		
		Miss,
		
		Wound,
		
		Kill,
	}

	///<summary>�������</summary>
	public class Ship
	{
		///<summary>�����������</summary>
		public Ship(Vector location, int size, bool direction)
		{
			Location = location;
			Size = size;
			Direction = direction;
			AliveCells = new HashSet<Vector>(GetShipCells());
		}




		///<summary>��� �� �������</summary>
		public bool Alive { get { return AliveCells.Any(); } }
		///<summary>������� ������� �� �����</summary>
		public Vector Location { get; private set; }


		
		
		
		///<summary>������ �������</summary>
		public List<Vector> GetShipCells()
		{
			var d = Direction ? new Vector(1, 0) : new Vector(0, 1);
			var list1 = new List<Vector>();
			for (int i = 0; i < Size; i++)
			{
				var shipCell = d.Mult(i).Add(Location);
				list1.Add(shipCell);
			}
			return list1;
		}


		///<summary>������ �������</summary>
		public int Size { get; private set; }
		///<summary>����������� �������. True � ��������������. False � ������������</summary>
		public bool Direction { get; private set; }
		///<summary>����� ������ �������</summary>
		public HashSet<Vector> AliveCells;
	}


	/////////////////////////////////////////////////////////////////////////////////////////////////
	/// �����
	/////////////////////////////////////////////////////////////////////////////////////////////////


	///<summary>�����</summary>
	public class Map
	{
		private static MapCell[,] cells;
		public static Ship[,] shipsMap;

		///<summary>�����������</summary>
		public Map(int width, int height)
		{
			Width = width;
			Height = height;
			cells = new MapCell[width, height];
			shipsMap = new Ship[width, height];
		}

		///<summary>������� �� ����</summary>
		public List<Ship> Ships = new List<Ship>();

		///<summary>������ ����</summary>
		public int Width { get; private set; }
		///<summary>������ ����</summary>
		public int Height { get; private set; }

		public MapCell this[Vector p]
		{
			get
			{
				return CheckBounds(p) ? cells[p.X, p.Y] : MapCell.Empty; // ��������� ����� ����� ������ ����� ����� �� ��������� �� ����� �� ������� ����. 
			}
			private set
			{
				if (!CheckBounds(p))
					throw new IndexOutOfRangeException(p + " is not in the map borders"); // ������� ����������� ������ � ����.
				cells[p.X, p.Y] = value;
			}
		}

		///<summary>�������� ������� ������� i � ����� v, ��������� � ����������� d</summary>
		public bool Set(Vector v, int n, bool direction)
		{
			var ship = new Ship(v, n, direction);
			var shipCells = ship.GetShipCells();
			//���� ����� ���� �������� ������, �� ��������� ������� ������!
			if (shipCells.SelectMany(Near).Any(c => this[c] != MapCell.Empty)) return false;
			//���� ������� �� ���������� � ���� ������
			if (!shipCells.All(CheckBounds)) return false;

			// �����, ������ �������
			foreach (var cell in shipCells)
				{
					this[cell] = MapCell.Ship;
					shipsMap[cell.X, cell.Y] = ship;
				}
			Ships.Add(ship);
			return true;
		}

		///<summary>������� ���!!!</summary>
		public ShtEffct Badaboom(Vector target)
		{
			var hit = CheckBounds(target) && this[target] == MapCell.Ship;
			
			
			if (hit)
			{
				var ship = shipsMap[target.X, target.Y];
				ship.AliveCells.Remove(target);
				this[target] = MapCell.DeadShip;
				return ship.Alive ? ShtEffct.Wound : ShtEffct.Kill;
			}


			if (this[target] == MapCell.Empty) this[target] = MapCell.Miss;
			return ShtEffct.Miss;
		}

		///<summary>����������� ������</summary>
		public IEnumerable<Vector> Near(Vector cell)
		{
			return
				from i in new[] {-1, 0, 1} //x
				from j in new[] {-1, 0, 1} //y
				let c = cell.Add(new Vector(i, j))
				where CheckBounds(c)
				select c;
		}

		///<summary>�������� �� ����� �� �������</summary>
		public bool CheckBounds(Vector p)
		{
			return p.X >= 0 && p.X < Width && p.Y >= 0 && p.Y < Height;
		}
		
		///<summary>���� �� ���� ���� ����� ������</summary>
		public bool HasAliveShips()
		{
			for (int index = 0; index < Ships.Count; index++)
			{
				var s = Ships[index];
				if (s.Alive) return true;
			}
			return false;
		}
	}
}