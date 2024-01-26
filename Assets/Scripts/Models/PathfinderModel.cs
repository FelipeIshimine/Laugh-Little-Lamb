using System;
using System.Collections;
using System.Collections.Generic;

namespace Models
{
	[System.Serializable]
	public class PathfinderModel
	{
		public Neighbours[] adjacencies;
		public Distances[] distances;
		
		public PathfinderModel(int count, Func<int,int[]> getNeighbours, Predicate<int> isWalkable, Func<int,int,float> getDistance)
		{
			adjacencies = new Neighbours[count];
			for (int i = 0; i < count; i++)
			{
				adjacencies[i] = new Neighbours(Array.FindAll(getNeighbours(i), isWalkable));
			}

			distances = new Distances[count];
			for (int i = 0; i < adjacencies.Length; i++)
			{
				distances[i] = new Distances(adjacencies.Length);
			}
			
			for (var x = 0; x < count; x++)
			{
				for (int y = x+1; y < count; y++)
				{
					var distance = getDistance(x,y);
					distances[y][x] = distances[x][y] = distance;
				}
			}
		}

		public int[] GetNeighbours(int index) => adjacencies[index];
		
		[System.Serializable]
		public struct Neighbours : IEnumerable<int>
		{
			public int[] values;

			public Neighbours(int[] toArray)
			{
				values = toArray;
			}

			public IEnumerator<int> GetEnumerator()
			{
				foreach (int value in values)
					yield return value;
			}
			IEnumerator IEnumerable.GetEnumerator() => values.GetEnumerator();

			public static implicit operator int[](Neighbours neighbours) => neighbours.values;
		}

		[System.Serializable]
		public struct Distances
		{
			public float[] values;

			public Distances(int count)
			{
				values = new float[count];
			}

			public float this[int i] { get => values[i]; set => values[i] = value; }

			public static implicit operator float[](Distances distances) => distances.values;
		}
	}
}