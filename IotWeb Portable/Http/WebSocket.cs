using System;
using System.IO;
using System.Text;

namespace IotWeb.Common.Http
{
	public class WebSocket
	{
		/// <summary>
		/// Helper class to process a WebSocket frame header.
		/// </summary>
		private class FrameHeader
		{
			// Size of a decoding key
			private const int KeySize = 4;

			// Frame information
			public byte OpCode { get; private set; }
			public long Length { get; private set; }
			public bool Masked { get; private set; }
			public bool Finished { get; private set; }
			public byte[] Key { get; private set; }

			/// <summary>
			/// Parse the header from the input stream.
			/// 
			/// This will read bytes up to the start of the data block.
			/// </summary>
			/// <param name="input"></param>
			/// <returns></returns>
			public static FrameHeader Parse(Stream input)
			{
				byte[] buffer = new byte[MaxHeaderSize];
				// Read the header
				if (!WebSocket.ReadBytes(input, buffer, 0, 2))
					return null;
				// Decode it
				FrameHeader header = new FrameHeader();
				header.Finished = ((buffer[0] & 0x80) == 0x80);
				header.OpCode = (byte)(buffer[0] & 0x0f);
				header.Masked = ((buffer[1] & 0x80) == 0x80);
				// Get the length
				long length = (long)(buffer[1] & 0x7f);
				// Check for extended length
				if (length == 126)
				{
					// 16 bit payload length
					if (!WebSocket.ReadBytes(input, buffer, 0, 2))
						return null;
					length = ConvertBytes(buffer, 2);
				}
				else if (length == 127)
				{
					// 64 bit payload length
					if (!ReadBytes(input, buffer, 0, 8))
						return null;
					length = ConvertBytes(buffer, 8);
				}
				header.Length = length;
				// If the frame is masked we need the key
				if (header.Masked)
				{
					header.Key = new byte[KeySize];
					if (!WebSocket.ReadBytes(input, header.Key, 0, KeySize))
						return null;
				}
				return header;
			}

			/// <summary>
			/// Convert a sequence of bytes (in network byte order) to a long value
			/// </summary>
			/// <param name="data"></param>
			/// <param name="length"></param>
			/// <returns></returns>
			private static long ConvertBytes(byte[] data, int length)
			{
				UInt64 value = 0;
				for (int i = 0; i < length; i++)
					value = (value << 8) + (UInt64)data[i];
				return (long)value;
			}
		}

		/// <summary>
		/// The types of frames we can receive
		/// </summary>
		private enum FrameType
		{
			Unknown,
			Text,
			Binary
		}

		/// <summary>
		/// Event handler for data received events
		/// </summary>
		/// <param name="socket"></param>
		/// <param name="frame"></param>
		public delegate void DataReceivedHandler(WebSocket socket, string frame);

		/// <summary>
		/// Event handler for connection close events.
		/// </summary>
		/// <param name="socket"></param>
		public delegate void ConnectionClosedHandler(WebSocket socket);

		// Constants
		private const long MaxFrameSize = 64 * 1024;
		private const int MaxHeaderSize = 14;

		// Opcodes
		private const byte ContinuationFrame = 0x00;
		private const byte TextFrame = 0x01;
		private const byte BinaryFrame = 0x02;
		private const byte CloseFrame = 0x08;
		private const byte PingFrame = 0x09;
		private const byte PongFrame = 0x0A;

		// Instance variables
		private Stream m_input;
		private Stream m_output;
		private bool m_closed;

		/// <summary>
		/// Event for data reception
		/// </summary>
		public event DataReceivedHandler DataReceived;

		/// <summary>
		/// Event for connection closed
		/// </summary>
		public event ConnectionClosedHandler ConnectionClosed;

		/// <summary>
		/// Internal constructor.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="output"></param>
		internal WebSocket(Stream input, Stream output)
		{
			m_input = input;
			m_output = output;
			m_closed = false;
		}

		/// <summary>
		/// Send a message on the socket
		/// </summary>
		/// <param name="message"></param>
		public void Send(string message)
		{
			if (m_closed)
				return;
			// Convert the message to UTF8
			byte[] data = Encoding.UTF8.GetBytes(message);
			SendFrame(TextFrame, data, 0, data.Length);
		}

		/// <summary>
		/// Send a portion of an array as a binary frame.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		public void Send(byte[] data, int offset, int length)
		{
			if (m_closed)
				return;
			SendFrame(BinaryFrame, data, offset, length);
		}

		/// <summary>
		/// Send an array of bytes as a binary frame.
		/// </summary>
		/// <param name="data"></param>
		public void Send(byte[] data)
		{
			if (m_closed)
				return;
			SendFrame(BinaryFrame, data, 0, data.Length);
		}

		/// <summary>
		/// Close the socket
		/// </summary>
		public void Close()
		{
			if (m_closed)
				return;
			// Send the close frame
			SendFrame(CloseFrame, null, 0, 0);
			m_closed = true;
			// Notify any listeners
			ConnectionClosedHandler handler = ConnectionClosed;
			if (handler != null)
				handler(this);
		}

		/// <summary>
		/// Process messages until the socket is closed.
		/// </summary>
		internal void Run()
		{
			byte[] buffer = new byte[MaxFrameSize];
			long index = 0;
			FrameType lastFrame = FrameType.Unknown;
			while (!m_closed)
			{
				// Read the header
				FrameHeader header = FrameHeader.Parse(m_input);
				if (header == null)
				{
					Close();
					return;
				}
				// Figure out what to do with the frame
				bool readData = true;
				switch (header.OpCode)
				{
					case ContinuationFrame:
						if (lastFrame == FrameType.Unknown)
						{
							// Continuation with no start frame, just drop the connection
							Close();
							return;
						}
						break;
					case TextFrame:
						// Start of a text frame
						index = 0;
						lastFrame = FrameType.Text;
						break;
					case BinaryFrame:
						// Start of a binary frame
						index = 0;
						lastFrame = FrameType.Text;
						break;
					case CloseFrame:
						// Close the connection
						Close();
						return;
					case PingFrame:
						// Request for a Pong response
						SendPong(header);
						continue;
					default:
						// Just ignore it
						readData = false;
						break;
				}
				// If the packet is too large just discard the payload
				if ((index + header.Length) > MaxFrameSize)
				{
					readData = false;
					index = MaxFrameSize;
				}
				// Read or consume the data
				if (readData)
				{
					if (!ReadData(header, buffer, (int)index))
						Close();
					index += header.Length;
				}
				else if (!ConsumeData(header))
					Close();
				// Did we wind up closing the connection?
				if (m_closed)
					return;
				// Is this the end of the data sequence?
				if (header.Finished)
				{
					// If it was too large we ignore it
					if (index >= MaxFrameSize)
						continue;
					// If it wasn't a text frame we ignore it
					if (lastFrame != FrameType.Text)
						continue;
					// TODO: Send out the notification for new data
					string message = Encoding.UTF8.GetString(buffer, 0, (int)index);
					index = 0;
					DataReceivedHandler handler = DataReceived;
					if (handler != null)
						handler(this, message);
				}
			}
		}

		/// <summary>
		/// Read bytes from the input stream.
		/// 
		/// This wraps the read call and ensures the number of bytes requested is
		/// read before returning. Will return 'false' if the connection has
		/// closed during the operation.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="buffer"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		private static bool ReadBytes(Stream input, byte[] buffer, int offset, int length)
		{
			while (length > 0)
			{
				try
				{
					int read = input.Read(buffer, offset, length);
					if (read == 0)
						return false;
					offset += read;
					length -= read;
				}
				catch (Exception)
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Read data from the input stream into the frame buffer
		/// </summary>
		/// <param name="header"></param>
		/// <param name="buffer"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		private bool ReadData(FrameHeader header, byte[] buffer, int offset)
		{
			// Would this overflow the buffer?
			long total = header.Length + (long)offset;
			if (total >= MaxFrameSize)
				return ConsumeData(header);
			// Read into the buffer
			if (!ReadBytes(m_input, buffer, offset, (int)header.Length))
				return false;
			// Do we need to unmask the data ?
			if (header.Masked)
			{
				for(int i=0; i<(int)header.Length; i++)
					buffer[offset + i] = (byte)(buffer[offset + i] ^ header.Key[i % 4]);
			}
			return true;
		}

		/// <summary>
		/// Read, but do not store, the data from the input stream
		/// </summary>
		/// <param name="header"></param>
		/// <returns></returns>
		private bool ConsumeData(FrameHeader header)
		{
			const int DiscardBufferSize = 128;
			byte[] buffer = new byte[DiscardBufferSize];
			long remaining = header.Length;
			while (remaining > 0)
			{
				if (!ReadBytes(m_input, buffer, 0, Math.Min((int)remaining, DiscardBufferSize)))
					return false;
				remaining -= (long)DiscardBufferSize;
			}
			return true;
		}

		/// <summary>
		/// Send a frame
		/// </summary>
		/// <param name="opcode"></param>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		private void SendFrame(byte opcode, byte[] data, int offset, int length)
		{
			// Set up the header
			byte[] header = new byte[MaxHeaderSize];
			int index = 0;
			// Add the opcode and flags
			header[index++] = (byte)(0x80 | opcode); // Always send complete frames
			// Add the length
			if (length < 126)
				header[index++] = (byte)length;
			else if (length < 65536)
			{
				// 16 bit length field
				header[index++] = 126;
				header[index++] = (byte)((length >> 8) & 0xff);
				header[index++] = (byte)(length & 0xff);
			}
			else // We do not support large frames, just ignore them
				return;
			// And finally send it out
			lock (m_output)
			{
				m_output.Write(header, 0, index); // Include key
				if (data!=null)
					m_output.Write(data, offset, length);
				m_output.Flush();
			}
		}

		/// <summary>
		/// Send a 'pong' response
		/// </summary>
		/// <param name="header"></param>
		private void SendPong(FrameHeader header)
		{
			// Get the data we need to echo back
			byte[] buffer = new byte[MaxFrameSize];
			if (!ReadData(header, buffer, 0))
				return;
			SendFrame(PongFrame, buffer, 0, (int)header.Length);
		}
	}
}
