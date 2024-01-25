using System.Collections.Generic;
using Model;
using UnityEngine;

namespace Models
{
	[System.Serializable]
	public class BoardModel : DataModel<BoardModel>
	{
		private int Count => nodes.Count;
		[SerializeField] private List<int> nodes = new List<int>();
	}
}