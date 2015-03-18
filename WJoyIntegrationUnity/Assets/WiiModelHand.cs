using UnityEngine;
using System.Collections;
using System;


/*
 A single wiimote, if a Wii MotionPlus (WMP) is connected will calculate current rotation and apply to gameobject.
 */
public class WiiModelHand : MonoBehaviour {
	public int _wiimote_id = -1; //
	public bool _wiimote_found = false; //wiimote has been found for this hand

	//calibrated accelerometer voltage values, will set _current_value to this frame's rotation value
	public WiiModelHandCalibratedValue _val_x = new WiiModelHandCalibratedValue();
	public WiiModelHandCalibratedValue _val_y = new WiiModelHandCalibratedValue();
	public WiiModelHandCalibratedValue _val_z = new WiiModelHandCalibratedValue();

	public void i_initialize() {
		_wiimote_id = -1;
		_wiimote_found = false;
		_val_x._inst_scale = 1.0f;
		_val_x.set_clamp(-80.0f,80.0f);

		_val_y._inst_scale = -1.0f;
		_val_y.set_clamp(-55.0f,55.0f);

		_val_z._inst_scale = 1.0f;
		_val_z._accel_avg_frames = 4.0f;
		_val_z.set_clamp(-55.0f,55.0f);
	}

	public void associate_with_id(int id) {
		_wiimote_found = true;
		_wiimote_id = id;
		_calibrated = false;
	}

	private uint _wmp_should_recenter_xyz_ct = 0;
	private bool should_recenter() { return _wmp_should_recenter_xyz_ct > 15; }

	private ulong _wmp_report_recieved_ct = 0;
	public void wmp_report(int vx, bool vx_slow, int vy, bool vy_slow, int vz, bool vz_slow) {
		_wmp_report_recieved_ct++;

		if (_calibrate_mode) {
			//calibrate average voltage values (10 bit integer)
			_val_x.wmp_report_calibrate(vx);
			_val_y.wmp_report_calibrate(vy);
			_val_z.wmp_report_calibrate(vz);
			_calibration_length++;

		} else if (_calibrated) {
			if (_val_x.should_recenter_from_wmp_val(vx) && _val_y.should_recenter_from_wmp_val(vy) &&_val_z.should_recenter_from_wmp_val(vz)) {
				//if average value moving slow enough, recenter axis based on gyroscope values (if x/z) or if IR centered (if y)
				_wmp_should_recenter_xyz_ct++;
			} else {
				_wmp_should_recenter_xyz_ct = 0;
			}

			_val_x.wmp_report(vx,vx_slow,this.should_recenter());
			_val_z.wmp_report(vz,vz_slow,this.should_recenter());
			_val_y.wmp_report(vy,vy_slow,this.should_recenter());
		}

		if (_wmp_report_recieved_ct > 50 && !_calibrated) {
			//automatically start calibration after 50 reports recieved
			float spd_from_avg = Vector3.Distance(new Vector3(vx,vy,vz),new Vector3(_val_x._all_avg,_val_y._all_avg,_val_z._all_avg));
			bool slow_enough = spd_from_avg < 30;
			if (_calibrate_mode) {
				if (_calibration_length > 150) {
					_calibrate_mode = false;
					_calibrated = true;
					Debug.Log ("auto calibrated device "+_wiimote_id);
				} else if (!slow_enough) {
					//too much movement, retry calibration
					Debug.Log ("calibration failed (Too Much Movement)");
					_calibrate_mode = false;
				}
			} else if (!_calibrated && slow_enough) {
				Debug.Log ("attempting auto calibration (Keep the Wiimote motionless)");
				_calibrate_mode = true;
				_calibration_length = 0;
			}
		}

		_val_x.report_all_wmp_avg(vx);
		_val_y.report_all_wmp_avg(vy);
		_val_z.report_all_wmp_avg(vz);
	}

	public void accel_report(int rx, int rz) {
		_val_x.accel_report(rx);
		_val_z.accel_report(rz);
	}

	private bool _calibrate_mode = false; //is doing calibration
	public bool _calibrated = false; //has been calibrated
	private int _calibration_length = 0; //calibration duration

	private bool _wmp_interpolate_mode = false; //no packets seen recently, interpolate movement based on previous packets
	private Vector3 _wmp_interpolate_delta_euler = Vector3.zero; //how much (in terms of euler angle) interpolation has moved the current rotation
	private int _wmp_interpolate_mode_delay_ct = 0; //time until interpolation mode start

	private Vector3 _wmp_interpolate_d_euler = Vector3.zero; //delta per frame (in terms of euler angle) to interpolate
	private Vector3 _wmp_last_euler = Vector3.zero; //last frame's rotation euler

	private Vector3 _current_euler = Vector3.zero; //current frame's rotation euler
	private bool _has_current_euler = false; //not first frame
	private int _drp_to_target_euler_ct = 0; //interpolate to target position count

	public void Update() {
		if (_calibrated) {
			if (_wmp_report_recieved_ct == 0) {
				_wmp_interpolate_mode_delay_ct++;
				if (_wmp_interpolate_mode_delay_ct >= 4) {
					//Haven't recieved a packet in 4 updates, start interpolating WMP movement from previous values
					if (!_wmp_interpolate_mode) {
						_wmp_interpolate_delta_euler = Vector3.zero;
					}
					_wmp_interpolate_mode = true;
				}
			} else {
				
				_wmp_interpolate_d_euler.x -= _wmp_interpolate_d_euler.x/10.0f;
				_wmp_interpolate_d_euler.x += _val_x._current_value - _wmp_last_euler.x;
				_wmp_interpolate_d_euler.y -= _wmp_interpolate_d_euler.y/10.0f;
				_wmp_interpolate_d_euler.y += _val_y._current_value - _wmp_last_euler.y;
				_wmp_interpolate_d_euler.z -= _wmp_interpolate_d_euler.z/10.0f;
				_wmp_interpolate_d_euler.z += _val_z._current_value - _wmp_last_euler.z;

				_wmp_last_euler = new Vector3(_val_x._current_value,_val_y._current_value,_val_z._current_value);

				_wmp_interpolate_mode_delay_ct = 0;
				_wmp_interpolate_mode = false;
			}
			_wmp_report_recieved_ct = 0;

			Vector3 target_current_euler = _current_euler;
			if (_wmp_interpolate_mode) {
				if (_wmp_interpolate_d_euler.magnitude > 1) {
					_wmp_interpolate_d_euler.x *= 0.5f;
					_wmp_interpolate_d_euler.y *= 0.5f;
					_wmp_interpolate_d_euler.z *= 0.5f;
				} else {
					_wmp_interpolate_d_euler = Vector3.zero;
				}
				_wmp_interpolate_delta_euler = Util.vec_add(_wmp_interpolate_delta_euler,_wmp_interpolate_d_euler);
				target_current_euler = Util.vec_add(
					_wmp_interpolate_delta_euler,
					new Vector3(_val_x._current_value,_val_y._current_value,_val_z._current_value)
				);
				_drp_to_target_euler_ct = 6;

			} else {
				target_current_euler = new Vector3(_val_x._current_value,_val_y._current_value,_val_z._current_value);
			}

			if (!_has_current_euler) {
				_current_euler = target_current_euler;
				_has_current_euler = true;
			} else {
				if (_drp_to_target_euler_ct > 0) {
					_drp_to_target_euler_ct--;
					_current_euler = Util.vec_drp(_current_euler,target_current_euler,1/3.0f);
				} else {
					_current_euler = target_current_euler;
				}
			}
			Util.transform_set_euler_world(this.transform,_current_euler);
		}


		if (Input.GetKeyUp(KeyCode.P) && _wiimote_found) {
			_calibrate_mode = !_calibrate_mode;
			if (!_calibrate_mode) {
				_calibrated = true;
				Debug.Log ("manual calibrated device "+_wiimote_id);
			} else {
				Debug.Log ("MANUAL CALIBRATION");
				_calibration_length = 0;
			}
		}

		_val_x.i_update(this.should_recenter());
		_val_y.i_update(this.should_recenter());
		_val_z.i_update(this.should_recenter());
	}

	[SerializeField] private IRDataVisualizer _ir_data_visualizer;
	public void ir_report(int index, bool out_of_view, int x, int y) {
		_ir_data_visualizer.ir_report(index,out_of_view,x,y);

		if (_ir_data_visualizer.any_visible() && Mathf.Abs(_ir_data_visualizer.camera_center().x-_ir_data_visualizer.ir_points_center().x) < 80) {
			_val_y.accel_report(0);
		} else {
			_val_y._accel_exists = false;
		}
		//test_ir_position();
	}
	
	public class WiiModelHandCalibratedValue {
		public const float SLOW_MULT = 1.0f/13.768f * (1.0f/150.0f);
		public const float FAST_MULT = SLOW_MULT * (2000.0f / 400.0f);

		public float _inst_scale = 1.0f;
		public float _calibrated_zero = 7797.988f;
		public float _current_value = 0;

		public float slow_rotation_speed_from_wmp_val(int val) {
			return (val-_calibrated_zero)*SLOW_MULT*_inst_scale;
		}
		public float fast_rotation_speed_from_wmp_val(int val) {
			return (val-_calibrated_zero)*FAST_MULT*_inst_scale;
		}
		public bool should_recenter_from_wmp_val(int val) {
			return this.slow_rotation_speed_from_wmp_val(val) < 0.15f;
		}

		public void wmp_report(int val, bool slow, bool do_recenter) {
			if (slow) {
				if (_accel_exists && do_recenter) {
					//_current_value = Util.drp(_current_value,_accel_val,1/4.0f);
				} else {
					_current_value += slow_rotation_speed_from_wmp_val(val);
				}

			} else {
				_current_value += fast_rotation_speed_from_wmp_val(val);
			}

			if (_clamp_exists) _current_value = Mathf.Clamp(_current_value,_clamp.x,_clamp.y);
		}

		public void wmp_report_calibrate(int val) {
			_calibrated_zero -= _calibrated_zero / 30.0f;
			_calibrated_zero += val / 30.0f;
		}

		public bool _accel_exists = false;
		private float _last_reported_accel_val = 0;
		private float _accel_val = 0;
		public void accel_report(float val) {
			if (!_accel_exists) {
				_accel_exists = true;
				_accel_val = val;
			}
			_last_reported_accel_val = val;
		}

		public float _accel_avg_frames = 1.0f;
		public void i_update(bool should_recenter) {
			_accel_val -= _accel_val / _accel_avg_frames;
			_accel_val += _last_reported_accel_val / _accel_avg_frames;

			if (should_recenter && _accel_exists) {
				_current_value = Util.drp(_current_value,_accel_val,1/4.0f);
			}
		}

		private bool _clamp_exists = false;
		private Vector2 _clamp = Vector2.zero;
		public void set_clamp(float min, float max) {
			_clamp.x = min;
			_clamp.y = max;
			_clamp_exists = true;
		}

		private bool _has_all_avg = false;
		public float _all_avg = 0;
		public void report_all_wmp_avg(float val) {
			if (!_has_all_avg) _all_avg = val;
			_all_avg -= _all_avg / 30.0f;
			_all_avg += val / 30.0f;
		}
	}
	/*
	//This is some almost-working code to be able to calculate wiimote position based on IR data
	//using the inverse projective transform and some constants found on http://wiibrew.org/wiki/Wiimote

	private Vector2 _last_index_1 = Vector2.zero;
	private void test_ir_position(int index, int x, int y) {
		if (_ir_data_visualizer.visible_count() == 2) {
			if (index == 1) {
				_last_index_1.x = x;
				_last_index_1.y = y;
			}
			if (index == 0) {
				Vector3 world_position = ir_screen_to_world_position(
					new Vector3(
						x,
						y,
						Vector3.Distance(
							new Vector3(x,y),
							new Vector3(_last_index_1.x,_last_index_1.y)
						)
					)
				);
				world_position.x *= -1;
				world_position.y *= -1;
				world_position.z *= -150;
				world_position.z -= 3.5f;
				this.gameObject.transform.localPosition = world_position;
			}
		}
	}
	
	public Vector3 ir_screen_to_world_position(Vector3 ir_point) {
		Matrix4x4 inverse_camera_rotation = new Matrix4x4();
		Quaternion q_camera_rotation = new Quaternion();
		q_camera_rotation.eulerAngles = new Vector3(_val_x._current_value,_val_y._current_value,_val_z._current_value);
		inverse_camera_rotation.SetTRS(Vector3.zero,q_camera_rotation,Util.valv(1));
		inverse_camera_rotation = inverse_camera_rotation.inverse;
		Matrix4x4 projection = projection_matrix(0.1f,100.0f,33.0f*Mathf.Deg2Rad,23.0f*Mathf.Rad2Deg);
		Matrix4x4 inverse_view_projection = (inverse_camera_rotation * projection).inverse;
		return inverse_view_projection.MultiplyPoint(ir_point);
	}
	
	public Matrix4x4 projection_matrix(float near_plane, float far_plane, float fov_horiz, float fov_vert) {
		float    h, w, Q;
		w = (float)1/Mathf.Tan(fov_horiz*0.5f);  // 1/tan(x) == cot(x)
		h = (float)1/Mathf.Tan(fov_vert*0.5f);   // 1/tan(x) == cot(x)
		Q = far_plane/(far_plane - near_plane);
		Matrix4x4 rtv = Matrix4x4.zero;
		rtv.m00 = w;
		rtv.m11 = h;
		rtv.m22 = Q;
		rtv.m32 = -Q*near_plane;
		rtv.m23 = 1;
		return rtv;
	}
	*/
}
