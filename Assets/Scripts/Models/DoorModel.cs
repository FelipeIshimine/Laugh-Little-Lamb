namespace Models
{
	[System.Serializable]
	public class DoorModel : EntityModel
	{
		public readonly Observable<bool> IsOpen = new Observable<bool>(false);
		public DoorModel(int index) : base(index)
		{
		}
	}
}