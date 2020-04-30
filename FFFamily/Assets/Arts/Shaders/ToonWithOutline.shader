Shader "ToonWithOutline"
{
	// Properties are like public variables in C#.
	// The same variables are declared again below.
	Properties
	{
		_Color("Color", Color) = (0.5, 0.65, 1, 1)
		_MainTex("Main Texture", 2D) = "white" {}

	_OutlineColor("OutlineColor",Color) = (0,0,0,0)
	_OutlineSize("Size",float) = 0
		// we are storing HDR ambient color, which
		// offers greater range and accuracy.
		[HDR]
		_AmbientColor("Ambient Color", Color) = (0.4, 0.4, 0.4, 1)
		[HDR]
		_SpecularColor("Specular Color", Color) = (0.9, 0.9, 0.9, 1)
		_Glossiness("Glossiness", Float) = 1024
		[HDR]
		_RimColor("Rim Color", Color) = (1,1,1,1)
		_RimAmount("Rim Amount", Range(0, 1)) = 0.716
			// Control how far the rim extends along the lit surface
			_RimThreshold("Rim Threshold", Range(0, 1)) = 0.1
	}

		// Container for the shader code.
			SubShader
		{
			Pass
	{
		Blend SrcAlpha OneMinusSrcAlpha
		cull front
		Zwrite off
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal:NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;

				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _OutlineSize;
			float4 _OutlineColor;
			v2f vert(appdata v)
			{
				v2f o;
				float4 Vpos = mul(UNITY_MATRIX_MV,v.vertex);
				float3 Vnormal = mul(UNITY_MATRIX_IT_MV,v.normal);
				Vnormal.z = -0.05;
				o.vertex = mul(UNITY_MATRIX_P,Vpos + float4(Vnormal,0)*_OutlineSize / 10);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{

fixed4 col = tex2D(_MainTex, i.uv);
			// apply fog
			UNITY_APPLY_FOG(i.fogCoord, col);
			return _OutlineColor * col;
			}
			ENDCG
		}


		Pass
			{
				Blend SrcAlpha OneMinusSrcAlpha
				Zwrite off
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				// make fog work
				#pragma multi_compile_fog

				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					float3 normal:NORMAL;
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					float4 vertex : SV_POSITION;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float4 _OutlineColor;


				v2f vert(appdata v)
				{
					v2f o;

					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					// sample the texture
					fixed4 col = tex2D(_MainTex, i.uv);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return _OutlineColor * col;
			}
			ENDCG
			}
			Pass
			{
				// Tags specify properties of the shader.
				// Setup forward rendering to receive only directional light data.
				Tags
				{
					"LightMode" = "ForwardBase"
					"PassFlags" = "OnlyDirectional"
				}

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				// Compile multiple versions of this shader depending on lighting settings.
				#pragma multi_compile_fwdbase
				#pragma target 3.0

				#include "UnityCG.cginc"
				#include "Lighting.cginc"
				#include "AutoLight.cginc"

				// Vertex shader input
				struct appdata
				{
					float4 vertex : POSITION;
					float4 uv : TEXCOORD0;
					float3 normal : NORMAL;
				};

			// Vertex shader output, also Fragment shader input
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 worldNormal: NORMAL;
				float3 viewDir: TEXCOORD1;
				// Macro found in Autolight.cginc. Declares a vector4
				// into the TEXCOORD2 semantic with varying precision 
				// depending on platform target.
				SHADOW_COORDS(2)
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.viewDir = WorldSpaceViewDir(v.vertex);
				// Defined in Autolight.cginc. Assigns the above shadow coordinate
				// by transforming the vertex from world space to shadow-map space.
				TRANSFER_SHADOW(o)
				return o;
			}

			float4 _Color;
			float4 _AmbientColor;
			float4 _SpecularColor;
			float _Glossiness;
			float4 _RimColor;
			float _RimAmount;
			float _RimThreshold;

			float4 frag(v2f i) : SV_Target
			{
				// Diffuse light
				float3 normal = normalize(i.worldNormal);
				float NdotL = dot(_WorldSpaceLightPos0, normal);

				// Cast and Receive Shadows
				// Samples the shadow map and returns a value between 0 and 1,
				// where 0 is no shadow and 1 is fully covered by shadow
				float shadow = SHADOW_ATTENUATION(i);

				// Using smoothstep softens the edge between light and dark while
				// clamping lightIntensity. The effect of this is explained in the
				// project writeup.
				float lightIntensity;
				if (NdotL < 0.5) {
					lightIntensity = smoothstep(0, 0.01, NdotL * shadow) / 2.0;
				}
		else {
		 lightIntensity = smoothstep(0.5, 0.51, NdotL * shadow) / 2.0 + 0.5;
		}


				// factor in the color of the main directional light.
				// _LightColor 0 is declared in Lighting.cginc.
				float4 diffLight = lightIntensity * _LightColor0;


				// Specular Light
				float3 viewDir = normalize(i.viewDir);
				float3 halfVec = normalize(_WorldSpaceLightPos0 + viewDir);
				float NdotH = dot(normal, halfVec);
				// multiply NdotH by lightIntensity achieves a sim
				float specIntensity = pow(NdotH * lightIntensity, _Glossiness);
				// Again, smoothstep clamps values between 0 and 1 to achieve
				// toonified look, while softening the edges of the highlight
				specIntensity = smoothstep(0.005, 0.01, specIntensity);
				float4 specLight = specIntensity * _SpecularColor;

				// Rim Light: illuminates edge of an object
				// The less the angle between normal and view direction,
				// the closer the fragment is to the edge, and the stronger
				// the rim illumination.
				float4 rimDot = 1 - dot(viewDir, normal);
				// Smoothstep similar to before. We multiply rimDot by NdotL
				// to ensure only illuminated surfaces of the object has rimLight
				float rimIntensity = rimDot * pow(NdotL, _RimThreshold);
				rimIntensity = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimIntensity);
				float4 rimLight = rimIntensity * _RimColor;

				float4 sample = tex2D(_MainTex, i.uv);

				return _Color * sample * (_AmbientColor + diffLight + specLight + rimLight);
			}
			ENDCG
		}

				// Use Unity's shadow casting shader
				UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
		}
}