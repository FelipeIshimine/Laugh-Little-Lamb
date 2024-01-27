using System;
using UnityEngine;

namespace Models
{
	[System.Serializable]
	public abstract class EntityModel : DataModel<EntityModel>
	{
		public event Action<EntityModel,int> OnPositionChange;
		private int positionIndex;
		public int PositionIndex
		{
			get => positionIndex;
			internal set
			{
				if (positionIndex != value)
				{
					positionIndex = value;
					OnPositionChange?.Invoke(this, value);
				}
			}
		}
		protected EntityModel(int index)
		{
			positionIndex = index;
		}
	}
}


