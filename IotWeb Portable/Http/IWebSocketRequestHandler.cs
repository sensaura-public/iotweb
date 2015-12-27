namespace IotWeb.Common.Http
{
    public interface IWebSocketRequestHandler
    {
        bool WillAcceptRequest(string uri, string protocol);

		void Connected(WebSocket socket);
    }
}
