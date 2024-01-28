using Controllers.Commands;
using Models;

namespace Controllers.Entities
{
	public class WaitCommand : Command<WaitCommand>
	{
		public readonly EntityModel SourceEntity;

		public WaitCommand(EntityModel sourceEntity)
		{
			SourceEntity = sourceEntity;
		}

		protected override void DoAction() { }
		protected override void UndoAction() { }
	}
}