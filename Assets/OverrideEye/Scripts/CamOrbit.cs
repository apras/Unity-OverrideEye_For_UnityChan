using UnityEngine;
using System.Collections;

public class CamOrbit : MonoBehaviour
{
	public Texture LicenseLogo;
	public float Radius = 1;
	public float UpdateInterval = 0.1f;

	private Camera m_camera;
	private int m_angle = 0;

	// Use this for initialization
	void Start()
	{
		this.m_camera = this.GetComponent<Camera>();
		this.m_angle = 0;

	    this.StartCoroutine(this.updateCamPosition());
	}
	
	IEnumerator updateCamPosition()
	{
		while(true)
		{
			float _x = -Mathf.Sin( Mathf.Deg2Rad * this.m_angle) * this.Radius;
			float _y = this.m_camera.transform.position.y;
			float _z = Mathf.Cos( Mathf.Deg2Rad * this.m_angle) * this.Radius;

			this.m_camera.transform.position = new Vector3(_x, _y, _z);

			_x = 0;
			_y = -this.m_angle + 180;
			_z = 0;

			this.m_camera.transform.rotation = Quaternion.Euler( new Vector3(_x, _y, _z) );

			this.m_angle += 1;

			yield return new WaitForSeconds( this.UpdateInterval );
		}

		yield return null;
	}

	protected virtual void OnGUI()
	{
		if (this.LicenseLogo != null)
		{
			GUI.DrawTexture(new Rect(0, Screen.height - 127, 147, 127), this.LicenseLogo, ScaleMode.StretchToFill, true);
		}
	}
}
