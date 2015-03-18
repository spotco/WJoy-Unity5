using System;
using UnityEngine;
using System.Collections.Generic;

public class Util {

	public static System.Random rand = new System.Random(69);
	
	public static float rand_range(float min, float max) {
		float r = (float)rand.NextDouble();
		return (max-min)*r + min;
	}

	public static int int_random(int min, int max) {
		return Mathf.FloorToInt(rand_range(min,max));
	}
	
	public static float vec_dist(Vector3 a, Vector3 b) {
		return (float)Math.Abs(Math.Sqrt(Math.Pow(a.x-b.x,2)+Math.Pow(a.y-b.y,2)+Math.Pow(a.z-b.z,2)));
	}
	
	public static Vector3 vec_drp(Vector3 from, Vector3 to, float mlt) {
		return new Vector3(
			drp(from.x,to.x,mlt),
			drp(from.y,to.y,mlt),
			drp(from.z,to.z,mlt)
		);
	}

	public static Vector3 vec_invert(Vector3 vec) {
		return new Vector3(-vec.x,-vec.y,-vec.z);
	}

	public static Vector3 vec_delta(Vector3 vec, float jx = 0, float jy = 0, float jz = 0) {
		return new Vector3(vec.x+jx,vec.y+jy,vec.z+jz);
	}

	public static Vector3 vec_any_normal(Vector3 ivec) {
		Vector3 vec = ivec.normalized;
		Vector3 add = Vector3.up;
		if (vec.y == add.y) add = Util.vec_delta(add,0.5f).normalized;
		return Util.vec_cross(vec,add).normalized;
	}
	
	public static GameObject proto_clone(GameObject proto) {
		GameObject rtv = ((GameObject)UnityEngine.Object.Instantiate(proto));
		rtv.transform.SetParent(proto.transform.parent);
		rtv.transform.localScale = proto.transform.localScale;
		rtv.transform.localPosition = proto.transform.localPosition;
		rtv.transform.localRotation = proto.transform.localRotation;
		rtv.SetActive(true);
		return rtv;
	}
	
	public static Vector3 sin_lerp_vec(Vector3 a, Vector3 b, float t) {
		return new Vector3(sin_lerp(a.x,b.x,t),sin_lerp(a.y,b.y,t),sin_lerp(a.z,b.z,t));
	}
	
	public static Vector3 lerp_vec(Vector3 a, Vector3 b, float t) {
		return new Vector3(lerp(a.x,b.x,t),lerp(a.y,b.y,t),lerp(a.z,b.z,t));
	}
	
	public static float sin_lerp(float min, float max, float t) {
		t = t > 1 ? 1 : t;
		t = t < 0 ? 0 : t;
		return Util.lerp(min,max, Mathf.Sin(t*Mathf.PI/2)/Mathf.Sin(Mathf.PI/2));
	}
	
	public static float lerp(float a, float b, float t) {
		return a + (b - a) * t;
	}
	
	public static float drp(float from, float to, float mlt) {
		
		if (Math.Abs(to-from) < 0.01f) return to;
		return from + (to-from)*mlt;
	}
	
	public static void transform_set_euler_world(Transform t,Vector3 tar) {
		Quaternion q = t.rotation;
		q.eulerAngles = tar;
		t.rotation = q;
	}

	public static void transform_set_euler_local(Transform t,Vector3 tar) {
		Quaternion q = t.localRotation;
		q.eulerAngles = tar;
		t.localRotation = q;
	}

	public static void transform_position_delta(Transform t, Vector3 delta) {
		Vector3 pos = t.transform.position;
		pos.x += delta.x;
		pos.y += delta.y;
		pos.z += delta.z;
		t.transform.position = pos;
	}
	
	public static string vec_to_s(Vector3 v) {
		return string.Format("({0},{1},{2})",v.x,v.y,v.z);
	}
	
	public static Vector3 vec_sub(Vector3 a, Vector3 b) {
		return new Vector3(a.x-b.x,a.y-b.y,a.z-b.z);
	}
	
	public static Vector3 vec_cross(Vector3 v1,Vector3 a) {
		float x1, y1, z1;
		x1 = (v1.y*a.z) - (a.y*v1.z);
		y1 = -((v1.x*a.z) - (v1.z*a.x));
		z1 = (v1.x*a.y) - (a.x*v1.y);
		return new Vector3(x1,y1,z1);
	}
	
	public static Vector3 valv(float x) {
		return new Vector3(x,x,x);
	}
	
	public static Vector3 vec_add(Vector3 a, Vector3 b) {
		Vector3 v = new Vector3();
		v.x = a.x + b.x;
		v.y = a.y + b.y;
		v.z = a.z + b.z;
		return v;
	}
	
	public static Vector3 vec_scale(Vector3 v,float f) {
		v.x *= f;
		v.y *= f;
		v.z *= f;
		return v;
	}
	
	public static float clampf(float val, float min, float max) {
		if (val > max) {
			return max;
		} else if (val < min) {
			return min;
		} else {
			return val;
		}
	}
	
	public static int sig(float n) {
		if (n > 0) {
			return 1;
		} else if (n < 0) {
			return -1;
		} else {
			return 0;
		}
	}
	
	public static float rad2deg = 57.29f;
	public static float deg2rad = 0.017f;
}


