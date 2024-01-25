using System;
using System.Collections;
using System.Collections.Generic;
using Models;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class TilemapController : MonoBehaviour
{
   private static readonly TileBase[] AllTiles = new TileBase[1024];
   
   [SerializeField] private List<Tile> floorTiles;
   [SerializeField] private List<Tile> wallTiles;
   
   [SerializeField] private Tile playerTile;
   [SerializeField] private Tile exitTile;
   
   [SerializeField] private Tilemap tilemap;

   [SerializeField] public TilemapModel tilemapModel = new TilemapModel();

   [Button]
   public void ScanTilemap()
   {
	   tilemap.CompressBounds();

	   
	   var count = tilemap.GetTilesBlockNonAlloc(tilemap.cellBounds, AllTiles);
	   var bounds = tilemap.cellBounds;

	   List<int> floor = new List<int>();
	   
	   for (var index = 0; index < count; index++)
	   {
		   //var coordinate = bounds.min + new Vector3Int(index % bounds.size.x, index / bounds.size.x);
		   var tile = AllTiles[index];
		   if (floorTiles.Exists( x => x == tile))
		   {
			   floor.Add(index);
		   }
	   }

	   tilemapModel = new TilemapModel(bounds, floor);
   }

   private void OnDrawGizmosSelected()
   {
	   if (tilemap)
	   {
		   Gizmos.color = Color.magenta;
		   tilemap.CompressBounds();
		   var bounds = tilemap.cellBounds;
		   Gizmos.DrawLineStrip(new[]
		   {
			   tilemap.CellToWorld(new Vector3Int(bounds.min.x,bounds.min.y)),
			   tilemap.CellToWorld(new Vector3Int(bounds.min.x,bounds.max.y)),
			   tilemap.CellToWorld(new Vector3Int(bounds.max.x,bounds.max.y)),
			   tilemap.CellToWorld(new Vector3Int(bounds.max.x,bounds.min.y)),
		   }, true);

		   var count = tilemap.GetTilesBlockNonAlloc(tilemap.cellBounds, AllTiles);
		   for (var index = 0; index < count; index++)
		   {
			   var coordinate = bounds.min + new Vector3Int(index % bounds.size.x, index / bounds.size.x);
			   var center = tilemap.GetCellCenterWorld(coordinate);
			   
			   #if UNITY_EDITOR
			   UnityEditor.Handles.Label(center, $"{coordinate.x},{coordinate.y}");
			   #endif
		   }
		   
	   }
   }
}
