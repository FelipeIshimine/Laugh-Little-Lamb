using System;
using UnityEngine;

namespace Models
{
	public struct EntityModel
	{
		public Vector2Int Coordinate;
		public Orientation LookDirection;

		public EntityModel(Vector2Int coordinate, Orientation lookDirection)
		{
			Coordinate = coordinate;
			LookDirection = lookDirection;
		}
	}
}