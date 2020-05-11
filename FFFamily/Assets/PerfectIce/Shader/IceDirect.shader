// Shader created with Shader Forge v1.37 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.37;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:False,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:0,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:False,igpj:True,qofs:1,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:0,x:34596,y:32383,varname:node_0,prsc:2|diff-396-RGB,diffpow-404-RGB,spec-389-OUT,gloss-404-A,normal-25-RGB,emission-538-OUT,alpha-605-OUT,clip-668-OUT,refract-653-OUT;n:type:ShaderForge.SFN_Tex2d,id:25,x:32949,y:32835,ptovrint:False,ptlb:Refraction,ptin:_Refraction,varname:node_5709,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:f20ef6394a329114c94d93a68ef967f8,ntxv:3,isnm:True;n:type:ShaderForge.SFN_Fresnel,id:217,x:33313,y:33234,varname:node_217,prsc:2|EXP-556-OUT;n:type:ShaderForge.SFN_Cubemap,id:362,x:33480,y:33008,ptovrint:False,ptlb:Cube,ptin:_Cube,varname:node_4146,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,cube:c1ad01f33e5dc4d40ae09cf74ba9d47f,pvfc:0;n:type:ShaderForge.SFN_Slider,id:389,x:34237,y:32326,ptovrint:False,ptlb:Shininess,ptin:_Shininess,varname:node_3287,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0.1,cur:1,max:10;n:type:ShaderForge.SFN_Color,id:396,x:33289,y:32374,ptovrint:False,ptlb:Color,ptin:_Color,varname:node_2832,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:1;n:type:ShaderForge.SFN_Tex2d,id:404,x:33290,y:32544,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:node_3358,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:88ff7575bffeb7e498b8c4ee156fe11a,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Color,id:430,x:34257,y:32960,ptovrint:False,ptlb:ReflectColor,ptin:_ReflectColor,varname:node_7460,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:1,c3:1,c4:0.5;n:type:ShaderForge.SFN_Power,id:464,x:33667,y:33168,varname:node_464,prsc:2|VAL-549-OUT,EXP-591-OUT;n:type:ShaderForge.SFN_Lerp,id:498,x:33936,y:32959,cmnt:RefColor,varname:node_498,prsc:2|A-571-OUT,B-584-OUT,T-464-OUT;n:type:ShaderForge.SFN_Multiply,id:534,x:34104,y:32959,varname:node_534,prsc:2|A-498-OUT,B-536-OUT,C-535-OUT;n:type:ShaderForge.SFN_Slider,id:535,x:33797,y:33122,ptovrint:False,ptlb:RefStrength,ptin:_RefStrength,varname:node_5688,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:20;n:type:ShaderForge.SFN_Slider,id:536,x:33767,y:33229,ptovrint:False,ptlb:LightStrength,ptin:_LightStrength,varname:node_9601,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.1,max:20;n:type:ShaderForge.SFN_Add,id:537,x:34252,y:32813,varname:node_537,prsc:2|A-404-RGB,B-534-OUT;n:type:ShaderForge.SFN_Multiply,id:538,x:34422,y:32813,varname:node_538,prsc:2|A-537-OUT,B-430-RGB;n:type:ShaderForge.SFN_ValueProperty,id:549,x:33482,y:33175,ptovrint:False,ptlb:FrenelPower,ptin:_FrenelPower,varname:node_6236,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.1;n:type:ShaderForge.SFN_Dot,id:556,x:33143,y:33258,varname:node_556,prsc:2,dt:4|A-557-OUT,B-25-RGB;n:type:ShaderForge.SFN_Vector1,id:557,x:32982,y:33258,varname:node_557,prsc:2,v1:1;n:type:ShaderForge.SFN_Multiply,id:571,x:33664,y:32884,varname:node_571,prsc:2|A-404-RGB,B-396-RGB;n:type:ShaderForge.SFN_Multiply,id:584,x:33668,y:33011,varname:node_584,prsc:2|A-362-RGB,B-362-A;n:type:ShaderForge.SFN_ConstantLerp,id:591,x:33486,y:33234,varname:node_591,prsc:2,a:-1,b:0|IN-217-OUT;n:type:ShaderForge.SFN_Add,id:605,x:34430,y:32497,varname:node_605,prsc:2|A-404-A,B-644-OUT;n:type:ShaderForge.SFN_ValueProperty,id:644,x:34276,y:32549,ptovrint:False,ptlb:TexAlphaAdd,ptin:_TexAlphaAdd,varname:node_2326,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.1;n:type:ShaderForge.SFN_Multiply,id:653,x:33996,y:32647,varname:node_653,prsc:2|A-655-OUT,B-654-OUT;n:type:ShaderForge.SFN_Multiply,id:654,x:33850,y:32695,varname:node_654,prsc:2|A-657-OUT,B-681-OUT;n:type:ShaderForge.SFN_ComponentMask,id:655,x:33843,y:32555,varname:node_655,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-25-RGB;n:type:ShaderForge.SFN_Slider,id:657,x:33525,y:32695,ptovrint:False,ptlb:RefractionStrength,ptin:_RefractionStrength,varname:node_21,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-10,cur:0,max:10;n:type:ShaderForge.SFN_Add,id:668,x:34429,y:32624,varname:node_668,prsc:2|A-404-A,B-669-OUT;n:type:ShaderForge.SFN_Slider,id:669,x:34123,y:32738,ptovrint:False,ptlb:Cutoff,ptin:_Cutoff,varname:node_7947,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:-1,cur:1,max:1;n:type:ShaderForge.SFN_Vector1,id:681,x:33681,y:32767,varname:node_681,prsc:2,v1:0.5;proporder:396-389-430-404-25-362-535-536-549-644-657-669;pass:END;sub:END;*/

Shader "IceShader/IceDirect" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _Shininess ("Shininess", Range(0.1, 10)) = 1
        _ReflectColor ("ReflectColor", Color) = (1,1,1,0.5)
        _MainTex ("MainTex", 2D) = "white" {}
        _Refraction ("Refraction", 2D) = "bump" {}
        _Cube ("Cube", Cube) = "_Skybox" {}
        _RefStrength ("RefStrength", Range(0, 20)) = 1
        _LightStrength ("LightStrength", Range(0, 20)) = 0.1
        _FrenelPower ("FrenelPower", Float ) = 0.1
        _TexAlphaAdd ("TexAlphaAdd", Float ) = 0.1
        _RefractionStrength ("RefractionStrength", Range(-10, 10)) = 0
        _Cutoff ("Cutoff", Range(-1, 1)) = 1
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent+1"
            "RenderType"="Transparent"
        }
        LOD 200
        GrabPass{ }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma only_renderers d3d9 d3d11 gles gles3 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _GrabTexture;
            uniform sampler2D _Refraction; uniform float4 _Refraction_ST;
            uniform samplerCUBE _Cube;
            uniform float _Shininess;
            uniform float4 _Color;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float4 _ReflectColor;
            uniform float _RefStrength;
            uniform float _LightStrength;
            uniform float _FrenelPower;
            uniform float _TexAlphaAdd;
            uniform float _RefractionStrength;
            uniform float _Cutoff;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
                float4 screenPos : TEXCOORD5;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                o.screenPos = o.pos;
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                #if UNITY_UV_STARTS_AT_TOP
                    float grabSign = -_ProjectionParams.x;
                #else
                    float grabSign = _ProjectionParams.x;
                #endif
                i.screenPos = float4( i.screenPos.xy / i.screenPos.w, 0, 0 );
                i.screenPos.y *= _ProjectionParams.x;
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 _Refraction_var = UnpackNormal(tex2D(_Refraction,TRANSFORM_TEX(i.uv0, _Refraction)));
                float3 normalLocal = _Refraction_var.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float2 sceneUVs = float2(1,grabSign)*i.screenPos.xy*0.5+0.5 + (_Refraction_var.rgb.rg*(_RefractionStrength*0.5));
                float4 sceneColor = tex2D(_GrabTexture, sceneUVs);
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                clip((_MainTex_var.a+_Cutoff) - 0.5);
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = 1;
                float3 attenColor = attenuation * _LightColor0.xyz;
///////// Gloss:
                float gloss = _MainTex_var.a;
                float specPow = exp2( gloss * 10.0 + 1.0 );
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float3 specularColor = float3(_Shininess,_Shininess,_Shininess);
                float3 directSpecular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),specPow)*specularColor;
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = pow(max( 0.0, NdotL), _MainTex_var.rgb) * attenColor;
                float3 diffuseColor = _Color.rgb;
                float3 diffuse = directDiffuse * diffuseColor;
////// Emissive:
                float4 _Cube_var = texCUBE(_Cube,viewReflectDirection);
                float3 emissive = ((_MainTex_var.rgb+(lerp((_MainTex_var.rgb*_Color.rgb),(_Cube_var.rgb*_Cube_var.a),pow(_FrenelPower,lerp(-1,0,pow(1.0-max(0,dot(normalDirection, viewDirection)),0.5*dot(1.0,_Refraction_var.rgb)+0.5))))*_LightStrength*_RefStrength))*_ReflectColor.rgb);
/// Final Color:
                float3 finalColor = diffuse + specular + emissive;
                return fixed4(lerp(sceneColor.rgb, finalColor,(_MainTex_var.a+_TexAlphaAdd)),1);
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdadd
            #pragma only_renderers d3d9 d3d11 gles gles3 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _GrabTexture;
            uniform sampler2D _Refraction; uniform float4 _Refraction_ST;
            uniform samplerCUBE _Cube;
            uniform float _Shininess;
            uniform float4 _Color;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float4 _ReflectColor;
            uniform float _RefStrength;
            uniform float _LightStrength;
            uniform float _FrenelPower;
            uniform float _TexAlphaAdd;
            uniform float _RefractionStrength;
            uniform float _Cutoff;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
                float4 screenPos : TEXCOORD5;
                LIGHTING_COORDS(6,7)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                o.screenPos = o.pos;
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                #if UNITY_UV_STARTS_AT_TOP
                    float grabSign = -_ProjectionParams.x;
                #else
                    float grabSign = _ProjectionParams.x;
                #endif
                i.screenPos = float4( i.screenPos.xy / i.screenPos.w, 0, 0 );
                i.screenPos.y *= _ProjectionParams.x;
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 _Refraction_var = UnpackNormal(tex2D(_Refraction,TRANSFORM_TEX(i.uv0, _Refraction)));
                float3 normalLocal = _Refraction_var.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float2 sceneUVs = float2(1,grabSign)*i.screenPos.xy*0.5+0.5 + (_Refraction_var.rgb.rg*(_RefractionStrength*0.5));
                float4 sceneColor = tex2D(_GrabTexture, sceneUVs);
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                clip((_MainTex_var.a+_Cutoff) - 0.5);
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
///////// Gloss:
                float gloss = _MainTex_var.a;
                float specPow = exp2( gloss * 10.0 + 1.0 );
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float3 specularColor = float3(_Shininess,_Shininess,_Shininess);
                float3 directSpecular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),specPow)*specularColor;
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = pow(max( 0.0, NdotL), _MainTex_var.rgb) * attenColor;
                float3 diffuseColor = _Color.rgb;
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                return fixed4(finalColor * (_MainTex_var.a+_TexAlphaAdd),0);
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            Cull Back
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma only_renderers d3d9 d3d11 gles gles3 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _Cutoff;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float2 uv0 : TEXCOORD1;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos( v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                clip((_MainTex_var.a+_Cutoff) - 0.5);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
