using System.Threading;
using Cysharp.Threading.Tasks;

namespace Controllers
{
	public interface IPlayer
	{
		public UniTask TakeTurnAsync(CancellationToken token);
	}
}