using UnityEngine;
using System.Collections.Generic;

public class IRDataVisualizer : MonoBehaviour {

	[SerializeField] private GameObject _point0;
	[SerializeField] private GameObject _point1;
	[SerializeField] private GameObject _point2;
	[SerializeField] private GameObject _point3;

	private GameObject point_for_id(int i) {
		if (i == 0) return _point0;
		if (i == 1) return _point1;
		if (i == 2) return _point2;
		return _point3;
	}

	public static Vector2 CAMERA_SCREEN = new Vector2(1024,767); 
	private Dictionary<int,Vector2> _points = new Dictionary<int, Vector2>();
	private const float scf = 0.001f;
	private Vector2 _last_index_1 = Vector2.zero;
	public void ir_report(int index, bool out_of_view, int x, int y) {
		GameObject tar = point_for_id(index);
		if (out_of_view) {
			tar.SetActive(false);
			if (_points.ContainsKey(index)) _points.Remove(index);

		} else {
			tar.SetActive(true);
			_points[index] = new Vector2(x,y);
			float max_x = CAMERA_SCREEN.x;
			float max_y = CAMERA_SCREEN.y;

			max_x *= scf;
			max_y *= scf;
			float tar_x = x * scf;
			float tar_y = y * scf;

			tar.transform.localPosition = new Vector3(tar_x-max_x/2,tar_y-max_y/2);
		}
	}

	public Vector2 ir_points_center() {
		Vector2 rtv = Vector2.zero;
		foreach(int key in _points.Keys) {
			rtv.x += _points[key].x;
			rtv.y += _points[key].y;
		}
		rtv.x /= _points.Count;
		rtv.y /= _points.Count;
		return rtv;
	}

	public bool any_visible() {
		return _points.Count > 0;
	}

	public int visible_count() {
		return _points.Count;
	}

	public Vector2 camera_center() {
		return new Vector2(IRDataVisualizer.CAMERA_SCREEN.x/2,IRDataVisualizer.CAMERA_SCREEN.y/2);
	}
}
