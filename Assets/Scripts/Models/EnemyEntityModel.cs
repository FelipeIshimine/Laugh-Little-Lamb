using UnityEngine;

namespace Models
{
	[System.Serializable]
	public class EnemyEntityModel : EntityModel, IMove
	{
		public readonly Observable<bool> IsLookingRight = new(true);
		public EnemyEntityModel(int index) : base(index)
		{
		}
	}
}