Shader "EGA/Particles/SmoothSmoke"
{
	Properties
	{
		_MainTex("Main Tex", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Emission("Emission", Float) = 2
		[MaterialToggle] _Useblack("Use black", Float) = 0
		[MaterialToggle] _Usedepth ("Use depth?", Float ) = 0
		_Depthpower("Depth power", Float) = 1
		[HideInInspector] _tex4coord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#include "UnityCG.cginc"
		//#pragma target 3.0
		#pragma surface surf Unlit alpha:fade keepalpha noshadow 
		#undef TRANSFORM_TEX
		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
		#pragma multi_compile _ SOFTPARTICLES_ON
		struct Input
		{
			float4 uv_tex4coord;
			float4 vertexColor : COLOR;
			float4 screenPos;
		};

		uniform float4 _Color;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float _Emission;
		uniform sampler2D _CameraDepthTexture;
		uniform float _Depthpower;
		uniform fixed _Usedepth;
		uniform fixed _Useblack;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float4 uv_MainTex = i.uv_tex4coord;
			uv_MainTex.xy = i.uv_tex4coord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
			float smoothstepResult1 = smoothstep( uv_MainTex.z , ( uv_MainTex.z + uv_MainTex.w ) , tex2D( _MainTex, uv_MainTex.xy ).a);
			float clampResult11 = clamp( smoothstepResult1 , 0.0 , 1.0 );
			float staticSwitch13 = lerp(1.0, clampResult11, _Useblack);
			o.Emission = ( _Color * staticSwitch13 * i.vertexColor * _Emission ).rgb;
			float temp_output_9_0 = ( _Color.a * clampResult11 * i.vertexColor.a );
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth17 = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture,UNITY_PROJ_COORD( ase_screenPos ))));
			float distanceDepth17 = abs( ( screenDepth17 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _Depthpower ) );
			float clampResult18 = clamp( distanceDepth17 , 0.0 , 1.0 );
			float4 staticSwitch20 = temp_output_9_0;
			#ifdef SOFTPARTICLES_ON
				staticSwitch20 *= lerp(1, clampResult18, _Usedepth);
			#endif
			o.Alpha = staticSwitch20;
		}
	ENDCG
	}
}