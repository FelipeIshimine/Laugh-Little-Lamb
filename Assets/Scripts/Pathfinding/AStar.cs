using System;
using System.Collections.Generic;
using Models;

namespace Pathfinding
{
	public static class AStar
	{
		public static bool TryFindPath(int start, int destination, Func<int,int[]> getAdjacencies, Func<int,float> getScore, int count, ref List<int> path, out int pathCost)
		{
			path.Clear();
			//Debug.Log($"Find Path {start} => {destination}");
			int[] previous = new int[count];
			int[] bestCost = new int[count];
			
			for (int i = 0; i < count; i++)
			{
				bestCost[i] = int.MaxValue;
				previous[i] = -1;
			}
			
			PriorityQueue<int> next = new MinPriorityQueue<int>(count);
			bestCost[start] = 0;
			next.Enqueue(0, start);

			int closestIndex = start;
			
			while (next.Count > 0)
			{
				var (cost, index) = next.Dequeue();

				if (index == destination)
				{
					closestIndex = index;
					//Debug.Log("FOUND");
					break;
				}
				
				var neighbours = getAdjacencies(index);

				//Debug.Log($"{index} neighbours {neighbours.values.Count}");
				foreach (int neighbour in neighbours)
				{
					var score = getScore(neighbour);
					var nCost = cost + 1 + (int)(score);
					if (nCost < bestCost[neighbour])
					{
						if (score < getScore(closestIndex))
						{
							closestIndex = neighbour;
						}
						
						bestCost[neighbour] = nCost;
						previous[neighbour] = index;
						next.Enqueue(nCost, neighbour);
					}
				}
			}


			path = new List<int>();
			bool success = closestIndex == destination;

			if (closestIndex != -1)
			{
				//Debug.Log($"Backtrack Start {closestIndex}");
				pathCost = bestCost[closestIndex];

				while (closestIndex != -1)
				{
					//Debug.Log($"BTrack {closestIndex}");
					path.Add(closestIndex);
					closestIndex = previous[closestIndex];
				}
			}
			else
			{
				pathCost = -1;
			}
			return success;
		}
	}
}