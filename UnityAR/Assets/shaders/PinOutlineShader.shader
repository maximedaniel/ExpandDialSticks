// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Outlined/PinSilhouetteShader" {
	Properties{
		 _Color("Main Color", Color) = (0.5,0.5,0.5,1)
		 _MainTex("Texture", 2D) = "white" {}
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_Outline("Outline width", Range(0.0, 100.00)) = 50
		_SecondOutlineColor("Second Outline Color", Color) = (1,1,1,1)
		_SecondOutline("Second Outline width", Range(0.0, 100.00)) = 25
	}
		CGINCLUDE
#include "UnityCG.cginc"
#pragma multi_compile_instancing
			struct appdata {
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float2 uv : TEXCOORD0;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct v2f {
			float4 pos : POSITION;
			float4 color : COLOR;
			float2 uv : TEXCOORD0;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};


		uniform sampler2D _MainTex;
		float4 _MainTex_ST;
		UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
			UNITY_DEFINE_INSTANCED_PROP(float, _Outline)
			UNITY_DEFINE_INSTANCED_PROP(float4, _OutlineColor)
			UNITY_DEFINE_INSTANCED_PROP(float, _SecondOutline)
			UNITY_DEFINE_INSTANCED_PROP(float4, _SecondOutlineColor)
			UNITY_INSTANCING_BUFFER_END(Props)
			ENDCG

			SubShader {

			Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True" }

				//Blend SrcAlpha OneMinusSrcAlpha
				//ZWrite off

				Pass{
					Name "BASE"
					Cull back
					Offset 0, 0
				//Blend Zero One
				Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM

				#include "UnityCG.cginc"

				#pragma multi_compile_instancing
				#pragma vertex vert
				#pragma fragment frag

				v2f vert(appdata v) {
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					return o;
				}

				fixed4 frag(v2f i) : SV_TARGET{
					UNITY_SETUP_INSTANCE_ID(i);
					fixed4 col = tex2D(_MainTex, i.uv);
					col *= UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
					return col;
				}
			ENDCG
			}


				// note that a vertex shader is specified here but its using the one above
					Pass{
						Name "OUTLINE 1"
						Tags { "LightMode" = "Always" }
						Cull Front
					//Offset -2048,32 (for distinct circles)
					Offset 32, 32

					Blend SrcAlpha OneMinusSrcAlpha // Normal

					CGPROGRAM
					#pragma multi_compile_instancing
					#pragma vertex vert
					#pragma fragment frag			

					v2f vert(appdata v) {
					// just make a copy of incoming vertex data but scaled according to normal direction
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					o.pos = UnityObjectToClipPos(v.vertex);

					float3 norm = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
					float2 offset = TransformViewToProjection(norm.xy);

					o.pos.xy += offset * o.pos.z * UNITY_ACCESS_INSTANCED_PROP(Props, _Outline);

					o.color = UNITY_ACCESS_INSTANCED_PROP(Props, _OutlineColor);
					return o;
				}
				half4 frag(v2f i) :COLOR {
					UNITY_SETUP_INSTANCE_ID(i);
					return i.color;
				}
				ENDCG
				}
					// note that a vertex shader is specified here but its using the one above
				/*	Pass{
						Name "OUTLINE 2"
						Tags { "LightMode" = "Always" }
						Cull Front
						Offset 0, 0

					Blend SrcAlpha OneMinusSrcAlpha

					CGPROGRAM
					#pragma multi_compile_instancing
					#pragma vertex vert
					#pragma fragment frag			

					v2f vert(appdata v) {
					// just make a copy of incoming vertex data but scaled according to normal direction
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					o.pos = UnityObjectToClipPos(v.vertex);

					float3 norm = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
					float2 offset = TransformViewToProjection(norm.xy);

					o.pos.xy += offset * o.pos.z * UNITY_ACCESS_INSTANCED_PROP(Props,_SecondOutline);
					o.color = UNITY_ACCESS_INSTANCED_PROP(Props,_SecondOutlineColor);
					return o;
				}
				half4 frag(v2f i) :COLOR {
					UNITY_SETUP_INSTANCE_ID(i);
					return i.color;
				}
				ENDCG
				
				}*/
				

		}

		Fallback "Diffuse"
}