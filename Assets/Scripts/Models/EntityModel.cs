using System;
using UnityEngine;

namespace Models
{
	[System.Serializable]
	public abstract class EntityModel<T> : DataModel<T>, ICoordinate where T : EntityModel<T>
	{
		public event Action<ICoordinate,int> OnPositionChange;
		private int positionIndex;
		public int PositionIndex
		{
			get => positionIndex;
			internal set
			{
				if (positionIndex != value)
				{
					positionIndex = value;
					OnPositionChange?.Invoke((T)this, value);
				}
			}
		}
		protected EntityModel(int index)
		{
			positionIndex = index;
		}
	}

	public interface IMove { }

	public interface ICoordinate
	{
		public event Action<ICoordinate,int> OnPositionChange;
		public int PositionIndex { get; }
	}
}


