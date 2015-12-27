namespace IotWeb.Common
{
	public interface IServer
	{
		int Port { get; }

		void Start(int port);

		void Stop();
	}
}
