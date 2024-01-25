using System.Collections.Generic;
using Model;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Models
{
	[System.Serializable]
	public class TilemapModel : DataModel<TilemapModel>
	{
		public BoundsInt Bounds;
		public List<int> FloorTiles = new List<int>();

		public TilemapModel(BoundsInt bounds, List<int> floorTiles)
		{
			this.Bounds = bounds;
			this.FloorTiles = floorTiles;
		}
	}
}