using UnityEngine;

namespace Models
{
	[System.Serializable]
	public class SheepEntityModel : EntityModel, IMove
	{
		public readonly Observable<Orientation> LookDirection = new(Orientation.Down);
		
		public SheepEntityModel(int index) : base(index)
		{
		}

	}
}