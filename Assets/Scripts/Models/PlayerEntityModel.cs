using UnityEngine;

namespace Models
{
	[System.Serializable]
	public class PlayerEntityModel : EntityModel
	{
		public readonly Observable<Orientation> LookDirection = new(Orientation.Down);
		public PlayerEntityModel(int index) : base(index)
		{
		}
	}
}