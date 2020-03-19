Shader "Custom/ToonShadingSimple_v4_SimpleCelluloid"
{
	Properties
	{
		[Header(Main)]
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_ShadowColor("ShadowColor", Color) = (1.0, 1.0, 1.0, 1.0)
		_RimColor("RimColor", Color) = (1.0, 1.0, 1.0, 1.0)
		_ShadowThreshold("ShadowThreshold", Range(-1.0, 1.0)) = 0.2
			//赛璐珞风格通常情况不使用边缘光，RimThreshold 可默认为1
			_RimThreshold("RimThreshold", Range(0.0, 1.0)) = 1
			_RimPower("RimPower", Range(0.0, 16)) = 4.0
			_Specular("Specular", Color) = (1, 1, 1, 1)
			_SpecularScale("Specular Scale", Range(0, 0.1)) = 0.02
			_EdgeSmoothness("Edge Smoothness", Range(0,2)) = 2
			_Outline("Outline",Range(0,1)) = 0.1
			_OutlineColor("OutlineColor",Color) = (0,0,0,1)
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 100



			Pass
			{
				Cull Back
				Tags { "LightMode" = "ForwardBase" }
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
				fixed4 color : COLOR;

			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float3 worldNormal : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				UNITY_FOG_COORDS(3)
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			fixed4 _ShadowColor;
			fixed4 _RimColor;

			fixed _ShadowThreshold;
			fixed _RimThreshold;
			half _RimPower;
			half _EdgeSmoothness;
			fixed4 _Specular;
			fixed _SpecularScale;


			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldNormal = mul(v.normal, (float3x3)unity_WorldToObject);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.color = v.color;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//return i.color;
				fixed3 worldNormal = normalize(i.worldNormal); //法线 N
				fixed3 worldLightDir = normalize(UnityWorldSpaceLightDir(i.worldPos)); //光照方向 L
				fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.worldPos)); //视角方向 V
				fixed3 worldHalfDir = normalize(worldLightDir + worldViewDir); //高光计算用

				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed spec = dot(worldNormal, worldHalfDir);
				// w值也可用一个较小的值代替，效果差别不大
				fixed w = fwidth(spec)*_EdgeSmoothness;
				fixed4 specular = _Specular * lerp(0,1,smoothstep(-w, w, spec + _SpecularScale - 1)) * step(0.001, _SpecularScale);
				fixed diffValue = dot(worldNormal, worldLightDir);
				fixed diffStep = smoothstep(-w + _ShadowThreshold, w + _ShadowThreshold, diffValue);
				fixed4 light = _LightColor0 * 0.5 + 0.5;
				fixed4 diffuse = light * col * (diffStep + (1 - diffStep) * _ShadowColor) * _Color;

				// 模仿参考文章的方法，感觉效果不是太好
				// fixed rimValue = 1 - dot(worldNormal, worldViewDir);
				// fixed rimStep = step(_RimThreshold, rimValue * pow(dot(worldNormal,worldLightDir), _RimPower));

				fixed rimValue = pow(1 - dot(worldNormal, worldViewDir), _RimPower);
				fixed rimStep = smoothstep(-w + _RimThreshold, w + _RimThreshold, rimValue);

				fixed4 rim = light * rimStep * 0.5 * diffStep * _RimColor;
				fixed4 final = diffuse + rim + specular;

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, final);

				return  final;
			}
			ENDCG
		}

			//注意，描边 Pass 放后边，可享受 Early-Z 优化
			Pass
			{
				Cull Front
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
					float3 normal : NORMAL;
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					float4 vertex : SV_POSITION;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float _Outline;
				fixed4 _OutlineColor;

				v2f vert(appdata v)
				{
					v2f o;
					//计算与相机的距离，用来保持描边粗细程度
					float3 posView = mul(UNITY_MATRIX_MV,v.vertex).xyz;
					float dis = length(posView);
					float3 normal = v.normal;
					//顶点沿法线挤出
					v.vertex = v.vertex + float4(normalize(normal), 0) * _Outline * dis * 0.01;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					// apply fog
					UNITY_APPLY_FOG(i.fogCoord, col);
					return _OutlineColor;
				}
				ENDCG
			}
		}
}