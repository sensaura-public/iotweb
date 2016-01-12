namespace IotWeb.Common
{
	public delegate void ServerStoppedHandler(IServer server);

	public interface IServer
	{
		event ServerStoppedHandler ServerStopped;

		bool Running { get; }

		void Start();

		void Stop();
	}
}
