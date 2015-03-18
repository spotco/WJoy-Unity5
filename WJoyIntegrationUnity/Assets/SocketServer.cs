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

public class SocketServer : MonoBehaviour {
	[SerializeField] private WiiModel _wii_model;
	private CommunicatorServer _socket;
	private OnNextUpdater _on_next_update = new OnNextUpdater();

	public void Start() {
		_socket = new CommunicatorServer(7001,100,(string val)=>{
			try {
				JSONObject jason = JSONObject.Parse(val);
				string type = jason.GetString("t");
				_on_next_update.CallOnNextUpdate(()=>{
					if (type == "b") {
						string button = jason.GetString("b");
						int id = Convert.ToInt32(jason.GetNumber("id"));
						if (button == "B") {
							Debug.Log ("B Released");
							JSONObject obj = new JSONObject();
							obj.Add("action","rumble");
							obj.Add("id",Convert.ToInt32(jason.GetNumber("id")));
							obj.Add("duration",0.001f);
							enqueue_msg_to_send(obj.ToString());
						}

					} else if (type == "bp") {
						string button = jason.GetString("b");
						int id = Convert.ToInt32(jason.GetNumber("id"));
						if (button == "B") {
							Debug.Log ("B Pressed");
						}

					} else if (type == "m") {
						_wii_model.wmp_report(jason);
					} else if (type == "c") {
						_wii_model.wiimote_connect(jason);
					} else if (type == "d") {
						_wii_model.wiimote_disconnect(jason);
					} else if (type == "a") {
						_wii_model.accel_report(jason);
					} else if (type == "i") {
						_wii_model.ir_report(jason);
					}
				});
			} catch {
				Debug.LogError("MALFORMED JSON");
			}
		}, ()=> {
			Debug.Log ("unity connect message sent");
			JSONObject obj = new JSONObject();
			obj.Add("action","unity_connect");
			enqueue_msg_to_send(obj.ToString());
		});
	}

	public void Update() {
		_on_next_update.UpdateTick();
	}
	
	void OnApplicationQuit() {
		JSONObject obj = new JSONObject();
		obj.Add("action","unity_disconnect");
		enqueue_msg_to_send(obj.ToString());
		_socket.OnApplicationQuit();
	}
	
	public void enqueue_msg_to_send(string msg) { 
		_socket.enqueue_msg_to_send(msg);
	}
}
