Shader "OverrideEye/OverrideEyeForward"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_alpha ("Blend Alpha", Range(0.0,1.0)) = 1.0
	}
	
	CGINCLUDE
	#include "UnityCG.cginc"
	sampler2D _MainTex;
	sampler2D _ForwardMaskTex;
	sampler2D _ForwardClipTex;
	float _alpha;
	
	struct BuildInGBufferOutput
	{
		float4 Col0 : COLOR0;
		float4 Col1 : COLOR1;
		float4 Col2 : COLOR2;
		float4 Col3 : COLOR3;
	};	
	
	struct v2f
	{
		float4 pos			: SV_POSITION;
	};
	
	v2f vert (appdata_full v)
	{
		v2f o;
		o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
		return o;
	}		
	
	float4 fragPass0 (v2f i) : COLOR
	{
		return 0;			
	}
	
	float4 fragPass1(v2f_img i) : COLOR
	{
		float4 _tex = tex2Dlod(_MainTex, float4(i.uv, 0, 0));
		float _mask = tex2Dlod(_ForwardMaskTex, float4(i.uv, 0, 0)).x;
		float _clip = tex2Dlod(_ForwardClipTex, float4(i.uv, 0, 0)).x;
		//return float4(_clip.xxx, 1);
		return float4(_tex.xyz, _mask * _clip * _alpha);
	}		
	
	float fragPass2 (v2f i) : COLOR
	{
		return 1;
	}		
	
	ENDCG	
	
	SubShader
	{
		//0 Mask
		Pass
		{
			Cull off
				
			CGPROGRAM
			#pragma target 3.0
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert
			#pragma fragment fragPass0
			ENDCG			
		}
		
	
		//1 Composit 0
		Pass
		{
			ZTest Always Cull Off ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
				
			CGPROGRAM
			#pragma target 3.0
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert_img
			#pragma fragment fragPass1
			ENDCG			
		}		
		
		//2 Render Clip
		Pass
		{
			Cull back
				
			CGPROGRAM
			#pragma target 3.0
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert_img
			#pragma fragment fragPass2
			ENDCG			
		}		
	} 
	FallBack Off
}