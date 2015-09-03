using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;


public class OverrideEyeSystem
{
	static OverrideEyeSystem m_Instance;

	static public OverrideEyeSystem instance
	{
		get
		{
			if(m_Instance == null)
			{
				m_Instance = new OverrideEyeSystem();
			}
			return m_Instance;
		}
	}

	internal List<OverrideEye> m_eyeObjs = new List<OverrideEye>();

	public void Add(OverrideEye eyeObj)
	{
		this.Remove(eyeObj);
		this.m_eyeObjs.Add(eyeObj);
		this.Sort();
	}

	public void Remove(OverrideEye eyeObj)
	{
		this.m_eyeObjs.Remove(eyeObj);
		this.Sort();
	}

	public void Sort()
	{
		this.m_eyeObjs.Sort((a, b) => a.Priority - b.Priority);
	}
}

[RequireComponent(typeof (Camera))]
public class RenderOverrideEye : MonoBehaviour
{
	public float BlendAlpha = 1.0f;
	private Camera m_camera;
	private Dictionary<CameraEvent, CommandBuffer> m_cameraCommandBuffers = new Dictionary<CameraEvent, CommandBuffer>();
	private Material m_overrideEyeMat;
	private RenderTexture m_deferredMask;

	void Start()
	{
		this.m_camera = this.gameObject.GetComponent<Camera>();
		//
		if (this.m_camera.actualRenderingPath == RenderingPath.Forward)
		{
			this.m_overrideEyeMat = new Material(Shader.Find("OverrideEye/OverrideEyeForward"));
		}
		else
		{
			this.m_overrideEyeMat = new Material(Shader.Find("OverrideEye/OverrideEyeDeferred"));
		}
	}

	public void OnDisable()
	{
		foreach(KeyValuePair<CameraEvent, CommandBuffer> buffer in this.m_cameraCommandBuffers)
		{
			this.m_camera.RemoveCommandBuffer(buffer.Key, buffer.Value);
		}
	}

	public void OnPreRender()
	{
		this.m_overrideEyeMat.SetFloat("_alpha", this.BlendAlpha);

		bool _flag = this.gameObject.activeInHierarchy && this.enabled;
		if(!_flag)
		{
			this.OnDisable();
			return;
		}


		if (this.m_camera.actualRenderingPath == RenderingPath.Forward)
			this.renderForward();
		else
			this.renderDeferredShading();
	}

	private void renderForward()
	{
		CommandBuffer _bufA = null;
		CameraEvent _evA = CameraEvent.AfterForwardOpaque;
		
		if(this.m_cameraCommandBuffers.ContainsKey(_evA))
		{
			_bufA = this.m_cameraCommandBuffers[_evA];
			_bufA.Clear();
		}
		else
		{
			_bufA = new CommandBuffer();
			_bufA.name = "Override Eye";
			this.m_cameraCommandBuffers.Add(_evA, _bufA);
			this.m_camera.AddCommandBuffer(_evA, _bufA);
		}
		
		OverrideEyeSystem system = OverrideEyeSystem.instance;
		
		int _rt0 = Shader.PropertyToID("_rt0");
		_bufA.GetTemporaryRT(_rt0, -1, -1, 24, FilterMode.Point, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
		_bufA.SetRenderTarget(_rt0, _rt0);
		_bufA.ClearRenderTarget(true, true, Color.clear);
		
		foreach(OverrideEye eyeObj in system.m_eyeObjs)
		{
			if(eyeObj.Type == OverrideEye.RenderType.MASK)
			{
				this.drawOverrideEyeMat(_bufA, eyeObj, 0);
			}
			else if (eyeObj.Type == OverrideEye.RenderType.OVERRIDE)
			{
				this.drawSlefMat(_bufA, eyeObj, 0);
			}
		}


		// Clip
		int _rt4 = Shader.PropertyToID("_rt4");
		_bufA.GetTemporaryRT(_rt4, -1, -1, 24, FilterMode.Point, RenderTextureFormat.R8, RenderTextureReadWrite.Default);
		_bufA.SetRenderTarget(_rt4,  _rt4);
		_bufA.ClearRenderTarget(true, true, Color.clear);
		
		foreach(OverrideEye eyeObj in system.m_eyeObjs)
		{
			if(eyeObj.Type == OverrideEye.RenderType.CLIP)
			{
				this.drawOverrideEyeMat(_bufA, eyeObj, 2);
			}
			else
			{
				this.drawOverrideEyeMat(_bufA, eyeObj, 0);
			}
		}

		_bufA.SetGlobalTexture("_ForwardClipTex", _rt4);

		// Mask
		int _rt5 = Shader.PropertyToID("_rt5");
		_bufA.GetTemporaryRT(_rt5, -1, -1, 24, FilterMode.Point, RenderTextureFormat.R8, RenderTextureReadWrite.Default);
		_bufA.SetRenderTarget(_rt5,  _rt5);
		_bufA.ClearRenderTarget(true, true, Color.clear);
		
		foreach(OverrideEye eyeObj in system.m_eyeObjs)
		{
			if(eyeObj.Type == OverrideEye.RenderType.MASK)
			{
				this.drawOverrideEyeMat(_bufA, eyeObj, 0);
			}
			else if(eyeObj.Type == OverrideEye.RenderType.OVERRIDE)
			{
				this.drawOverrideEyeMat(_bufA, eyeObj, 2);
			}
		}
		
		_bufA.SetGlobalTexture("_ForwardMaskTex", _rt5);

		_bufA.Blit(_rt0, BuiltinRenderTextureType.CameraTarget, this.m_overrideEyeMat, 1);

		_bufA.ReleaseTemporaryRT(_rt0);
		_bufA.ReleaseTemporaryRT(_rt4);
		_bufA.ReleaseTemporaryRT(_rt5);
	}

	private void renderDeferredShading()
	{
		CommandBuffer _bufA = null;
		CameraEvent _evA = CameraEvent.AfterForwardOpaque;
		
		if(this.m_cameraCommandBuffers.ContainsKey(_evA))
		{
			_bufA = this.m_cameraCommandBuffers[_evA];
			_bufA.Clear();
		}
		else
		{
			_bufA = new CommandBuffer();
			_bufA.name = "Override Eye";
			this.m_cameraCommandBuffers.Add(_evA, _bufA);
			this.m_camera.AddCommandBuffer(_evA, _bufA);
		}
		
		OverrideEyeSystem system = OverrideEyeSystem.instance;
		
		int	_rt0 = Shader.PropertyToID("_rt0");
		int	_rt1 = Shader.PropertyToID("_rt1");
		int	_rt2 = Shader.PropertyToID("_rt2");
		int	_rt3 = Shader.PropertyToID("_rt3");
			
		_bufA.GetTemporaryRT(_rt0, -1, -1, 24, FilterMode.Point, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
		_bufA.GetTemporaryRT(_rt1, -1, -1, 0, FilterMode.Point, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
		_bufA.GetTemporaryRT(_rt2, -1, -1, 0, FilterMode.Point, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
		_bufA.GetTemporaryRT(_rt3, -1, -1, 0, FilterMode.Point, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
		
		
		RenderTargetIdentifier[] _rts = new RenderTargetIdentifier[4]
		{
			_rt0,
			_rt1,
			_rt2,
			_rt3
		};
		_bufA.SetRenderTarget(_rts, new RenderTargetIdentifier(_rt0));
		_bufA.ClearRenderTarget(true, true, Color.clear);
		
		foreach(OverrideEye eyeObj in system.m_eyeObjs)
		{
			if(eyeObj.Type == OverrideEye.RenderType.MASK)
			{
				this.drawOverrideEyeMat(_bufA, eyeObj, 0);
			}
			else if(eyeObj.Type == OverrideEye.RenderType.OVERRIDE)
			{
				this.drawSlefMat(_bufA, eyeObj, 0);
			}
		}

		// Clip
		int _rt4 = Shader.PropertyToID("_rt4");
		_bufA.GetTemporaryRT(_rt4, -1, -1, 24, FilterMode.Point, RenderTextureFormat.R8, RenderTextureReadWrite.Default);
		_bufA.SetRenderTarget(_rt4,  _rt4);
		_bufA.ClearRenderTarget(true, true, Color.clear);

		foreach(OverrideEye eyeObj in system.m_eyeObjs)
		{
			if(eyeObj.Type == OverrideEye.RenderType.CLIP)
			{
				this.drawOverrideEyeMat(_bufA, eyeObj, 5);
			}
			else
			{
				this.drawOverrideEyeMat(_bufA, eyeObj, 0);
			}
		}

		_bufA.SetGlobalTexture("_DeferredClipTex", _rt4);


		// Mask
		int _rt5 = Shader.PropertyToID("_rt5");
		_bufA.GetTemporaryRT(_rt5, -1, -1, 24, FilterMode.Point, RenderTextureFormat.R8, RenderTextureReadWrite.Default);
		_bufA.SetRenderTarget(_rt5,  _rt5);
		_bufA.ClearRenderTarget(true, true, Color.clear);
		
		foreach(OverrideEye eyeObj in system.m_eyeObjs)
		{
			if(eyeObj.Type == OverrideEye.RenderType.MASK)
			{
				drawOverrideEyeMat(_bufA, eyeObj, 0);
			}
			else if(eyeObj.Type == OverrideEye.RenderType.OVERRIDE)
			{
				drawOverrideEyeMat(_bufA, eyeObj, 5);
			}
		}
		
		_bufA.SetGlobalTexture("_DeferredMaskTex", _rt5);


		// for StandardShader
		//_bufA.Blit(_rt0, BuiltinRenderTextureType.GBuffer0, this.m_overrideEyeMat, 1);
		//_bufA.Blit(_rt1, BuiltinRenderTextureType.GBuffer1, this.m_overrideEyeMat, 2);
		//_bufA.Blit(_rt2, BuiltinRenderTextureType.GBuffer2, this.m_overrideEyeMat, 3);
		//_bufA.Blit(_rt3, BuiltinRenderTextureType.CameraTarget, this.m_overrideEyeMat, 4);

		// for unitychanShader
		_bufA.Blit(_rt0, BuiltinRenderTextureType.CameraTarget, this.m_overrideEyeMat, 1);


		_bufA.ReleaseTemporaryRT(_rt0);
		_bufA.ReleaseTemporaryRT(_rt1);
		_bufA.ReleaseTemporaryRT(_rt2);
		_bufA.ReleaseTemporaryRT(_rt3);
		_bufA.ReleaseTemporaryRT(_rt4);
		_bufA.ReleaseTemporaryRT(_rt5);
	}

	private void drawOverrideEyeMat(CommandBuffer buf, OverrideEye eyeObj, int pass)
	{
		if(eyeObj.Mr != null)
		{
			MeshFilter _mf = eyeObj.GetComponent<MeshFilter>();
			
			for(int _i = 0; _i < _mf.sharedMesh.subMeshCount; ++_i)
			{
				buf.DrawMesh(_mf.sharedMesh, eyeObj.gameObject.transform.localToWorldMatrix, this.m_overrideEyeMat, _i, pass);
			}
		}
		else if(eyeObj.SkinMr != null)
		{
			for(int _i = 0; _i < eyeObj.SkinMr.sharedMesh.subMeshCount; ++_i)
			{
				buf.DrawRenderer(eyeObj.SkinMr, this.m_overrideEyeMat, _i, pass);
			}
		}
	}

	private void drawSlefMat(CommandBuffer buf, OverrideEye eyeObj, int pass)
	{
		if(eyeObj.Mr != null)
		{
			MeshFilter _mf = eyeObj.GetComponent<MeshFilter>();
			
			for(int _i = 0; _i < _mf.sharedMesh.subMeshCount; ++_i)
			{
				buf.DrawMesh(_mf.sharedMesh, eyeObj.gameObject.transform.localToWorldMatrix, eyeObj.Materials[_i], _i, pass);
			}
		}
		else if(eyeObj.SkinMr != null)
		{
			for(int _i = 0; _i < eyeObj.SkinMr.sharedMesh.subMeshCount; ++_i)
			{
				eyeObj.Materials[_i].EnableKeyword("DIRLIGHTMAP_OFF");
				eyeObj.Materials[_i].EnableKeyword("DYNAMICLIGHTMAP_OFF");
				buf.DrawRenderer(eyeObj.SkinMr, eyeObj.Materials[_i], _i, pass);
			}
		}
	}
}

