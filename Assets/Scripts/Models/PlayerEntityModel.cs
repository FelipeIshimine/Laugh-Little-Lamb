using UnityEngine;

namespace Models
{
	[System.Serializable]
	public class PlayerEntityModel : EntityModel<PlayerEntityModel>, IMove
	{
		public readonly Observable<Orientation> LookDirection = new(Orientation.Down);
		
		public PlayerEntityModel(int index) : base(index)
		{
		}

	}
}