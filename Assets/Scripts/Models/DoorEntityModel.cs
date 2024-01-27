namespace Models
{
	[System.Serializable]
	public class DoorEntityModel : EntityModel
	{
		public readonly Observable<bool> IsOpen = new(true);
		public DoorEntityModel(int index) : base(index)
		{
		}
	}
}