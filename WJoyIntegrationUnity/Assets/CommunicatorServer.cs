using System.Collections.Generic;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Threading;
using UnityEngine;

public class CommunicatorServer {

	public class AsyncReadState {
		public Socket _socket = null;
		public const int BUFFER_SIZE = 8191;
		public byte[] _buffer = new byte[BUFFER_SIZE];
		public StringBuilder _msg = new StringBuilder();
	}

	private ManualResetEvent _accept_thread_block = new ManualResetEvent(false);
	private Socket _connection_socket;
	private Thread _accept_thread;

	private int _port;
	public const char MSG_TERMINATOR = '\0';

	private System.Action<string> _msg_recieved_callback = null;
	private System.Action _on_connected;

	public CommunicatorServer(int port, int queue_size, System.Action<string> msg_recieved, System.Action on_connected = null){
		_port = port;
		_msg_recieved_callback = msg_recieved;
		_on_connected = on_connected;

		_connection_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		_connection_socket.Bind(new IPEndPoint ( IPAddress.Any , _port));
		_connection_socket.Listen(queue_size);
		
		_accept_thread = new Thread(new ThreadStart(()=>{
			while (true) {
				_accept_thread_block.Reset();
				_connection_socket.BeginAccept(new AsyncCallback(accept_callback),_connection_socket);
				_accept_thread_block.WaitOne();
			}
		}));
		_accept_thread.Start();
	}

	public void OnApplicationQuit() {
		_accept_thread.Abort();
	}

	public void accept_callback(IAsyncResult res) {
		Debug.Log(string.Format("connection on port {0} opened",_port));
		if (_on_connected != null) _on_connected();

		Socket listener = (Socket) res.AsyncState;
		Socket handler = listener.EndAccept(res);
		
		_accept_thread_block.Set();
		
		AsyncReadState state = new AsyncReadState();
		state._socket = handler;
		
		AsyncCallback receive_callback = null;
		receive_callback = new AsyncCallback((IAsyncResult rec_res) => {
			AsyncReadState rec_state = (AsyncReadState) rec_res.AsyncState;
			Socket rec_handler = rec_state._socket;
			try {
				int read = rec_handler.EndReceive(rec_res);
				if (read > 0) {
					int start = 0;
					int i = 0;
					for (; i < read; i++) {
						if (rec_state._buffer[i] == (byte)CommunicatorServer.MSG_TERMINATOR) {
							try {
								rec_state._msg.Append(Encoding.ASCII.GetString(rec_state._buffer,start,i));
							} catch (Exception e) {}
							string stv = rec_state._msg.ToString();
							if (stv.Trim() != "") {
								msg_recieved(stv);
							}
							rec_state._msg.Remove(0,rec_state._msg.Length);
							start = i + 1;
						}
					}
					try {
						rec_state._msg.Append(Encoding.ASCII.GetString(rec_state._buffer,start,read-start));
					} catch (Exception e) {}
					
					rec_handler.BeginReceive(rec_state._buffer,0,AsyncReadState.BUFFER_SIZE,0,receive_callback,rec_state);	
					
				} else {
					rec_handler.Close();
					Debug.Log ("connection closed");
				}
				
			} catch (SocketException e) {
				rec_handler.Close();
				
			} catch (Exception e) {
				rec_handler.Close();
				Debug.Log("exception:"+e.GetType()+" msg:"+e.Message+" stack:"+e.StackTrace);
			}
		});
		
		handler.BeginReceive(state._buffer,0,AsyncReadState.BUFFER_SIZE,0,receive_callback,state);
		
		Thread send_thread = new Thread(new ThreadStart(()=>{
			try {
				while (true) {
					if (!handler.Connected) break;
					Thread.Sleep(20);
					string msg_to_send = this.get_msg_send();
					if (msg_to_send == null) continue;
					byte[] msg_bytes = Encoding.ASCII.GetBytes(msg_to_send+CommunicatorServer.MSG_TERMINATOR);
					
					state._socket.BeginSend(msg_bytes,0,msg_bytes.Length,0,new AsyncCallback((IAsyncResult send_res) => {
						Socket send_listener = (Socket) send_res.AsyncState;
						try {
							send_listener.EndSend(send_res);
						} catch (Exception e) {
							send_listener.Close();
						}
					}),state._socket);
				}
			} catch (Exception e) {}
		}));
		send_thread.Start();
	}

	long _last = 0;
	public long _disp = -1;
	private void msg_recieved(string val) {
		_disp = DateTime.Now.ToFileTimeUtc() - _last;
		_last = DateTime.Now.ToFileTimeUtc();
		if (_msg_recieved_callback != null) _msg_recieved_callback(val);
	}

	private Queue<string> _msg_to_send = new Queue<string>();
	public void enqueue_msg_to_send(string msg) { _msg_to_send.Enqueue(msg); }
	private string get_msg_send() {
		return _msg_to_send.Count > 0 ? _msg_to_send.Dequeue() : null;
	}
}
