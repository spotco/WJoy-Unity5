using UnityEngine;
using System.Collections;
using System;

public class WiiModel : MonoBehaviour {

	[SerializeField] public WiiModelHand _left_hand;
	[SerializeField] public WiiModelHand _right_hand;

	public void i_initialize() {
		_left_hand.i_initialize();
		_right_hand.i_initialize();
	}

	public void wmp_report(JSONObject jason) {
		int id = Convert.ToInt32(jason.GetNumber("id"));
		int vz = Convert.ToInt32(jason.GetNumber("r"));
		bool vz_slow = jason.GetBoolean("rs");

		int vx = Convert.ToInt32(jason.GetNumber("p"));
		bool vx_slow = jason.GetBoolean("ps");
		int vy = -Convert.ToInt32(jason.GetNumber("y"));
		bool vy_slow = jason.GetBoolean("ys");

		if (_left_hand._wiimote_id == id) _left_hand.wmp_report(vx,vx_slow,vy,vy_slow,vz,vz_slow);
		if (_right_hand._wiimote_id == id) _right_hand.wmp_report(vx,vx_slow,vy,vy_slow,vz,vz_slow);
	}

	public void accel_report(JSONObject jason) {
		int id = Convert.ToInt32(jason.GetNumber("id"));
		int rx = Convert.ToInt32(jason.GetNumber("p"));
		int rz = -Convert.ToInt32(jason.GetNumber("r"));

		if (_left_hand._wiimote_id == id) _left_hand.accel_report(rx,rz);
		if (_right_hand._wiimote_id == id) _right_hand.accel_report(rx,rz);
	}

	public void ir_report(JSONObject jason) {
		int id = Convert.ToInt32(jason.GetNumber("id"));
		int px = Convert.ToInt32(jason.GetNumber("px"));
		int py = Convert.ToInt32(jason.GetNumber("py"));
		bool out_of_view = jason.GetBoolean("ov");
		int index = Convert.ToInt32(jason.GetNumber("in"));

		if (_left_hand._wiimote_id == id) _left_hand.ir_report(index,out_of_view,px,py);
		if (_right_hand._wiimote_id == id) _right_hand.ir_report(index,out_of_view,px,py);
	}

	public void wiimote_connect(JSONObject jason) {
		int id = Convert.ToInt32(jason.GetNumber("id"));
		if (!_left_hand._wiimote_found) {
			_left_hand.associate_with_id(id);
			Debug.Log (string.Format("Link LEFT hand with id({0})",id));
		} else if (!_right_hand._wiimote_found) {
			_right_hand.associate_with_id(id);
			Debug.Log (string.Format("Link RIGHT hand with id({0})",id));
		}
	}

	public void wiimote_disconnect(JSONObject jason) {
		int id = Convert.ToInt32(jason.GetNumber("id"));
		if (_left_hand._wiimote_id == id) _left_hand._wiimote_found = false;
		if (_right_hand._wiimote_id == id) _right_hand._wiimote_found = false;
	}

	public bool is_left_hand_id(int id) {
		return id == _left_hand._wiimote_id;
	}
	public bool is_right_hand_id(int id) {
		return id == _right_hand._wiimote_id;
	}

	public int connected_count() {
		return (_left_hand._wiimote_found?1:0) + (_right_hand._wiimote_found?1:0);
	}
	public int calibrated_count() {
		return ((_left_hand._wiimote_found && _left_hand._calibrated)?1:0) + ((_right_hand._wiimote_found && _right_hand._calibrated)?1:0);
	}

}


