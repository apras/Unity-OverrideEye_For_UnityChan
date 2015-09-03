Shader "OverrideEye/OverrideEyeDeferred"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_alpha ("Blend Alpha", Range(0.0,1.0)) = 1.0
	}
	
	CGINCLUDE
	#include "UnityCG.cginc"
	sampler2D _MainTex;
	sampler2D _DeferredMaskTex;
	sampler2D _DeferredClipTex;
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
	
	BuildInGBufferOutput fragPass0 (v2f i)
	{
		BuildInGBufferOutput o;
				
		o.Col0 = float4(0, 0, 0, 0);
		o.Col1 = float4(0, 0, 0, 0);
		o.Col2 = float4(0, 0, 0, 0);
		o.Col3 = float4(0, 0, 0, 0);
					
		return o;			
	}
	
	float4 fragPass1(v2f_img i) : COLOR
	{
		float4 _tex = tex2Dlod(_MainTex, float4(i.uv, 0, 0));
		float _mask = tex2Dlod(_DeferredMaskTex, float4(i.uv, 0, 0)).x;
		float _clip = tex2Dlod(_DeferredClipTex, float4(i.uv, 0, 0)).x;
		return float4(_tex.xyz, _mask * _clip * _alpha);
	}		
	
	float4 fragPass2(v2f_img i) : COLOR
	{
		float4 _tex = tex2Dlod(_MainTex, float4(i.uv, 0, 0));
		float _mask = tex2Dlod(_DeferredMaskTex, float4(i.uv, 0, 0)).x;
		float _clip = tex2Dlod(_DeferredClipTex, float4(i.uv, 0, 0)).x;
		clip( _mask * _clip - 0.0001f);
		return _tex;
	}	
	
	float4 fragPass3(v2f_img i) : COLOR
	{
		float4 _tex = tex2Dlod(_MainTex, float4(i.uv, 0, 0));
		float _mask = tex2Dlod(_DeferredMaskTex, float4(i.uv, 0, 0)).x;
		float _clip = tex2Dlod(_DeferredClipTex, float4(i.uv, 0, 0)).x;
		clip( _mask * _clip - 0.0001f);
		return _tex;
	}	
	
	float4 fragPass4(v2f_img i) : COLOR
	{
		float4 _tex = tex2Dlod(_MainTex, float4(i.uv, 0, 0));
		float _mask = tex2Dlod(_DeferredMaskTex, float4(i.uv, 0, 0)).x;
		float _clip = tex2Dlod(_DeferredClipTex, float4(i.uv, 0, 0)).x;
		clip( _mask * _clip - 0.0001f);
		return _tex;
	}	
	
	float fragPass5 (v2f i) : COLOR
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
		
		//2 Composit 1
		Pass
		{
			ZTest Always Cull Off ZWrite Off
			
			CGPROGRAM
			#pragma target 3.0
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert_img
			#pragma fragment fragPass2
			ENDCG			
		}
		
		//3 Composit 2
		Pass
		{
			ZTest Always Cull Off ZWrite Off
				
			
			CGPROGRAM
			#pragma target 3.0
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert_img
			#pragma fragment fragPass3
			ENDCG			
		}
		
		//4 Composit 3
		Pass
		{
			ZTest Always Cull Off ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
				
			CGPROGRAM
			#pragma target 3.0
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert_img
			#pragma fragment fragPass4
			ENDCG			
		}
		
		//5 Render Clip
		Pass
		{
			Cull back
				
			CGPROGRAM
			#pragma target 3.0
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert_img
			#pragma fragment fragPass5
			ENDCG			
		}		
	} 
	FallBack Off
}