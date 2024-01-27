using System;
using UnityEngine;

namespace Models
{
	[System.Serializable]
	public enum Orientation
	{
		Up = 0,
		Right = 1,
		Down = 2,
		Left = 3
	}

	public static class OrientationExtensions
	{
		public static Vector2Int ToVector2Int(this Orientation @this)
		{
			switch (@this)
			{
				case Orientation.Up:
					return Vector2Int.up;
				case Orientation.Right:
					return Vector2Int.right;
				case Orientation.Down:
					return Vector2Int.down;
				case Orientation.Left:
					return Vector2Int.left;
				default:
					throw new ArgumentOutOfRangeException(nameof(@this), @this, null);
			}
		}
	}
}