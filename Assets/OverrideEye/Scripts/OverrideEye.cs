using UnityEngine;
using System.Collections;

public class OverrideEye : MonoBehaviour
{
	public enum RenderType
	{
		OVERRIDE = 0,
		MASK,
		CLIP
	}

	public RenderType Type = RenderType.OVERRIDE;
	public int Priority = 0;
	public Material[] Materials;
	public MeshRenderer Mr = null;
	public SkinnedMeshRenderer SkinMr = null;

	public void OnEnable ()
	{
		OverrideEyeSystem.instance.Add (this);
	}
	
	public void Start ()
	{
		OverrideEyeSystem.instance.Add (this);
		//
		MeshRenderer _meshRenderer = this.GetComponent<MeshRenderer> ();
		if (_meshRenderer != null) {
			this.Mr = _meshRenderer;
			this.Materials = this.Mr.sharedMaterials;
		} else {
			SkinnedMeshRenderer _skinMeshRenderer = this.GetComponent<SkinnedMeshRenderer> ();

			if (_skinMeshRenderer != null) {
				this.SkinMr = _skinMeshRenderer;
				this.Materials = this.SkinMr.sharedMaterials;
			}
		}
	}
	
	public void OnDisable ()
	{
		OverrideEyeSystem.instance.Remove (this);
	}
	
	void Update ()
	{
		
	}
}

