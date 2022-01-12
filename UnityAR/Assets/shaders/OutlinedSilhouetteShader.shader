// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Outlined/SilhouetteShader" {
	Properties{
		 _MainTex("Main Color", 2d) = "white" {}
		 _Color("Main Color", Color) = (0.5,0.5,0.5,1)
		_Textures("Textures", 2DArray) = "" {}
		_TextureIndex("Texture Index", Range(0, 32)) = 0
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_Outline("Outline width", Range(0.0, 100.00)) = 50
		_SecondOutlineColor("Second Outline Color", Color) = (1,1,1,1)
		_SecondOutline("Second Outline width", Range(0.0, 100.00)) = 25
		// Left Hand
		_LeftHandCenter("Left Hand Center", Vector) = (.0, .0, .0, .0)
		_LeftHandRadius("Left Hand Radius", Range(0,100)) = 0
		// Left Arm
		_LeftBackArmCenter("Left Back Arm Center", Vector) = (.0, .0, .0, .0)
		_LeftFrontArmCenter("Left Front Arm Center", Vector) = (.0, .0, .0, .0)
		_LeftArmRadius("Left Arm Radius", Range(0,100)) = 0
		// Right Hand
		_RightHandCenter("Right Hand Center", Vector) = (.0, .0, .0, .0)
		_RightHandRadius("Right Hand Radius", Range(0,100)) = 0
		// Right Arm
		_RightBackArmCenter("Right Back Arm Center", Vector) = (.0, .0, .0, .0)
		_RightFrontArmCenter("Right Front Arm Center", Vector) = (.0, .0, .0, .0)
		_RightArmRadius("Right Arm Radius", Range(0,100)) = 0
		
	}
		CGINCLUDE
		#include "UnityCG.cginc"
		#pragma multi_compile_instancing

		float3 ProjectPointLine(float3 pointTarget, float3 lineStart, float3 lineEnd)
		{
			float3 relativePoint = pointTarget - lineStart;
			float3 lineDirection = lineEnd - lineStart;
			float directionLength = length(lineDirection);
			float3 normalizedLineDirection = lineDirection;
			if (directionLength > .000001f)
				normalizedLineDirection /= directionLength;

			float projection = dot(normalizedLineDirection, relativePoint);
			projection = clamp(projection, 0.0F, directionLength);

			return lineStart + normalizedLineDirection * projection;
		}

		// Calculate distance between a point and a line.
		float DistancePointLine(float3 pointTarget, float3 lineStart, float3 lineEnd)
		{
			return length(ProjectPointLine(pointTarget, lineStart, lineEnd) - pointTarget);
		}

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
			float3 worldPos : TEXCOORD1;

			UNITY_VERTEX_INPUT_INSTANCE_ID
		};
		uniform sampler2D _MainTex;
		float4 _MainTex_ST;
		// sampler2D _MainTex;
		// sampler2D _TextureIndex_ST;
		//sampler2D _TextureIndex_ST;
		//float4 _TextureIndex_ST;

		UNITY_DECLARE_TEX2DARRAY(_Textures);

		UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(float, _TextureIndex)
			UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
			UNITY_DEFINE_INSTANCED_PROP(float, _Outline)
			UNITY_DEFINE_INSTANCED_PROP(float4, _OutlineColor)
			UNITY_DEFINE_INSTANCED_PROP(float, _SecondOutline)
			UNITY_DEFINE_INSTANCED_PROP(float4, _SecondOutlineColor)
			UNITY_DEFINE_INSTANCED_PROP(float, _ThirdOutline)
			UNITY_DEFINE_INSTANCED_PROP(float4, _ThirdOutlineColor)
			// Left Hand
			UNITY_DEFINE_INSTANCED_PROP(float4, _LeftHandCenter)
			UNITY_DEFINE_INSTANCED_PROP(float, _LeftHandRadius)
			// Left Arm
			UNITY_DEFINE_INSTANCED_PROP(float4, _LeftBackArmCenter)
			UNITY_DEFINE_INSTANCED_PROP(float4, _LeftFrontArmCenter)
			UNITY_DEFINE_INSTANCED_PROP(float, _LeftArmRadius)
			// Left Hand
			UNITY_DEFINE_INSTANCED_PROP(float4, _RightHandCenter)
			UNITY_DEFINE_INSTANCED_PROP(float, _RightHandRadius)
			// Right Arm
			UNITY_DEFINE_INSTANCED_PROP(float4, _RightBackArmCenter)
			UNITY_DEFINE_INSTANCED_PROP(float4, _RightFrontArmCenter)
			UNITY_DEFINE_INSTANCED_PROP(float, _RightArmRadius)
		UNITY_INSTANCING_BUFFER_END(Props)
		ENDCG

		SubShader{

		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent"}

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
			#pragma target 3.0

			v2f vert(appdata IN) {
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(IN);

				OUT.worldPos = mul(unity_ObjectToWorld, IN.vertex);
				OUT.pos = UnityObjectToClipPos(IN.vertex);
				OUT.uv = IN.uv;

				UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
				//fixed sliceIdx = 3;

				//_MainTex = UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(v.uv, UNITY_ACCESS_INSTANCED_PROP(Props, _TextureIndex)));
				//fixed3 uv = fixed3(v.uv.x, v.uv.y, UNITY_ACCESS_INSTANCED_PROP(Props, _TextureIndex));
				//o.uv = TRANSFORM_TEX(v.uv, _MainTex);  

				//fixed4 col = UNITY_SAMPLE_TEX2DARRAY(_MainTex, uv);

				//o.uv = v.uv;
				//o.uv = UNITY_SAMPLE_TEX2DARRAY(_Textures, v.uv);
				//o.uv = TRANSFORM_TEX(v.uv, UNITY_ACCESS_INSTANCED_PROP(_TextureIndex_arr, _TextureIndex));
				//UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(v.uv, UNITY_ACCESS_INSTANCED_PROP(_TextureIndex_arr, _TextureIndex)));
				return OUT;
			}

			fixed4 frag(v2f IN) : SV_TARGET{
				UNITY_SETUP_INSTANCE_ID(IN);
				//fixed4 col = tex2D(_MainTex, i.uv) * UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
				//fixed4 col = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
				fixed4 col = UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(IN.uv, UNITY_ACCESS_INSTANCED_PROP(Props, _TextureIndex))) * UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
				//fixed4 col = UNITY_SAMPLE_TEX2DARRAY(_Textures, float3(IN.uv, UNITY_ACCESS_INSTANCED_PROP(Props, _TextureIndex))) * UNITY_ACCESS_INSTANCED_PROP(Props, _Color);

				bool toShow = false;
				// compute distance to both hand
				float3 leftHandPos = UNITY_ACCESS_INSTANCED_PROP(Props, _LeftHandCenter);
				float leftHandRadius = UNITY_ACCESS_INSTANCED_PROP(Props, _LeftHandRadius);
				float3 rightHandPos = UNITY_ACCESS_INSTANCED_PROP(Props, _RightHandCenter);
				float rightHandRadius = UNITY_ACCESS_INSTANCED_PROP(Props, _RightHandRadius);
				float distanceToLeftHand = DistancePointLine(IN.worldPos, leftHandPos - float3(0, 100, 0), leftHandPos + float3(0, 100, 0));
				float distanceToRightHand = DistancePointLine(IN.worldPos, rightHandPos - float3(0, 100, 0), rightHandPos + float3(0, 100, 0));
				toShow = toShow || distanceToLeftHand < leftHandRadius || distanceToRightHand < rightHandRadius;
				// compute distance to both arm
				float3 leftBackArmPos = UNITY_ACCESS_INSTANCED_PROP(Props, _LeftBackArmCenter);
				float3 leftFrontArmPos = UNITY_ACCESS_INSTANCED_PROP(Props, _LeftFrontArmCenter);
				float leftArmRadius = UNITY_ACCESS_INSTANCED_PROP(Props, _LeftArmRadius);
				float3 rightBackArmPos = UNITY_ACCESS_INSTANCED_PROP(Props, _RightBackArmCenter);
				float3 rightFrontArmPos = UNITY_ACCESS_INSTANCED_PROP(Props, _RightFrontArmCenter);
				float rightArmRadius = UNITY_ACCESS_INSTANCED_PROP(Props, _RightArmRadius);

				leftBackArmPos.y = leftFrontArmPos.y = rightBackArmPos.y = rightFrontArmPos.y = IN.worldPos.y;

				float distanceToLeftArm = DistancePointLine(IN.worldPos, leftBackArmPos, leftFrontArmPos);
				float distanceToRightArm = DistancePointLine(IN.worldPos, rightBackArmPos, rightFrontArmPos);
				toShow = toShow || distanceToLeftArm < leftArmRadius || distanceToRightArm < rightArmRadius;
				col.a = (toShow)? col.a : toShow;
				return col;
			}
		ENDCG
			/*SetTexture[_OutlineColor] {
				ConstantColor(0,0,0,0)
				Combine constant
			}*/
		}
		/*Pass {
			Name "BASE"
			Cull back
			Blend Zero One


			SetTexture[_OutlineColor] {
				ConstantColor(0,0,0,0)
				Combine constant
			}
		}*/

		// note that a vertex shader is specified here but its using the one above
		Pass {
			Name "OUTLINE 1"
			Tags { "LightMode" = "Always" }
			Cull Front
			Offset 0,0
			// you can choose what kind of blending mode you want for the outline
			Blend SrcAlpha OneMinusSrcAlpha // Normal
			//Blend One One // Additive
			//Blend One OneMinusDstColor // Soft Additive
			//Blend DstColor Zero // Multiplicative
			//Blend DstColor SrcColor // 2x Multiplicative

			CGPROGRAM
			#pragma multi_compile_instancing
			#pragma vertex vert
			#pragma fragment frag		
			#pragma target 3.0	

			v2f vert(appdata v) {
				// just make a copy of incoming vertex data but scaled according to normal direction
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.pos = UnityObjectToClipPos(v.vertex);

				float3 norm = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
				float2 offset = TransformViewToProjection(norm.xy);

				o.pos.xy += offset * o.pos.z *  UNITY_ACCESS_INSTANCED_PROP(Props, _Outline);
				o.color = UNITY_ACCESS_INSTANCED_PROP(Props, _OutlineColor);
				return o;
			}
			half4 frag(v2f i) :COLOR {
				UNITY_SETUP_INSTANCE_ID(i);
				
				bool toShow = false;
				// compute distance to both hand
				float3 leftHandPos = UNITY_ACCESS_INSTANCED_PROP(Props, _LeftHandCenter);
				float leftHandRadius = UNITY_ACCESS_INSTANCED_PROP(Props, _LeftHandRadius);
				float3 rightHandPos = UNITY_ACCESS_INSTANCED_PROP(Props, _RightHandCenter);
				float rightHandRadius = UNITY_ACCESS_INSTANCED_PROP(Props, _RightHandRadius);
				float distanceToLeftHand = DistancePointLine(i.worldPos, leftHandPos - float3(0, 100, 0), leftHandPos + float3(0, 100, 0));
				float distanceToRightHand = DistancePointLine(i.worldPos, rightHandPos - float3(0, 100, 0), rightHandPos + float3(0, 100, 0));
				toShow = toShow || distanceToLeftHand < leftHandRadius || distanceToRightHand < rightHandRadius;
				// compute distance to both arm
				float3 leftBackArmPos = UNITY_ACCESS_INSTANCED_PROP(Props, _LeftBackArmCenter);
				float3 leftFrontArmPos = UNITY_ACCESS_INSTANCED_PROP(Props, _LeftFrontArmCenter);
				float leftArmRadius = UNITY_ACCESS_INSTANCED_PROP(Props, _LeftArmRadius);
				float3 rightBackArmPos = UNITY_ACCESS_INSTANCED_PROP(Props, _RightBackArmCenter);
				float3 rightFrontArmPos = UNITY_ACCESS_INSTANCED_PROP(Props, _RightFrontArmCenter);
				float rightArmRadius = UNITY_ACCESS_INSTANCED_PROP(Props, _RightArmRadius);

				leftBackArmPos.y = leftFrontArmPos.y = rightBackArmPos.y = rightFrontArmPos.y = i.worldPos.y;

				float distanceToLeftArm = DistancePointLine(i.worldPos, leftBackArmPos, leftFrontArmPos);
				float distanceToRightArm = DistancePointLine(i.worldPos, rightBackArmPos, rightFrontArmPos);
				toShow = toShow || distanceToLeftArm < leftArmRadius || distanceToRightArm < rightArmRadius;

				i.color.a = toShow;
				
				return i.color;
			}
			ENDCG
		}
			// note that a vertex shader is specified here but its using the one above
		Pass {
				Name "OUTLINE 2"
				Tags { "LightMode" = "Always" }
				Cull Front
				Offset 0, 0

				// you can choose what kind of blending mode you want for the outline
				Blend SrcAlpha OneMinusSrcAlpha // Normal
				//Blend One One // Additive
				//Blend One OneMinusDstColor // Soft Additive
				//Blend DstColor Zero // Multiplicative
				//Blend DstColor SrcColor // 2x Multiplicative

				CGPROGRAM
				#pragma multi_compile_instancing
				#pragma vertex vert
				#pragma fragment frag		
				#pragma target 3.0	

				v2f vert(appdata v) {
				// just make a copy of incoming vertex data but scaled according to normal direction
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.pos = UnityObjectToClipPos(v.vertex);

				float3 norm = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
				float2 offset = TransformViewToProjection(norm.xy);
				
				o.pos.xy += offset * o.pos.z * UNITY_ACCESS_INSTANCED_PROP(Props,_SecondOutline);
				o.color = UNITY_ACCESS_INSTANCED_PROP(Props,_SecondOutlineColor);
				return o;
			}
			half4 frag(v2f i) :COLOR {
				UNITY_SETUP_INSTANCE_ID(i);
				
				bool toShow = false;
				// compute distance to both hand
				float3 leftHandPos = UNITY_ACCESS_INSTANCED_PROP(Props, _LeftHandCenter);
				float leftHandRadius = UNITY_ACCESS_INSTANCED_PROP(Props, _LeftHandRadius);
				float3 rightHandPos = UNITY_ACCESS_INSTANCED_PROP(Props, _RightHandCenter);
				float rightHandRadius = UNITY_ACCESS_INSTANCED_PROP(Props, _RightHandRadius);
				float distanceToLeftHand = DistancePointLine(i.worldPos, leftHandPos - float3(0, 100, 0), leftHandPos + float3(0, 100, 0));
				float distanceToRightHand = DistancePointLine(i.worldPos, rightHandPos - float3(0, 100, 0), rightHandPos + float3(0, 100, 0));
				toShow = toShow || distanceToLeftHand < leftHandRadius || distanceToRightHand < rightHandRadius;
				// compute distance to both arm
				float3 leftBackArmPos = UNITY_ACCESS_INSTANCED_PROP(Props, _LeftBackArmCenter);
				float3 leftFrontArmPos = UNITY_ACCESS_INSTANCED_PROP(Props, _LeftFrontArmCenter);
				float leftArmRadius = UNITY_ACCESS_INSTANCED_PROP(Props, _LeftArmRadius);
				float3 rightBackArmPos = UNITY_ACCESS_INSTANCED_PROP(Props, _RightBackArmCenter);
				float3 rightFrontArmPos = UNITY_ACCESS_INSTANCED_PROP(Props, _RightFrontArmCenter);
				float rightArmRadius = UNITY_ACCESS_INSTANCED_PROP(Props, _RightArmRadius);

				leftBackArmPos.y = leftFrontArmPos.y = rightBackArmPos.y = rightFrontArmPos.y = i.worldPos.y;

				float distanceToLeftArm = DistancePointLine(i.worldPos, leftBackArmPos, leftFrontArmPos);
				float distanceToRightArm = DistancePointLine(i.worldPos, rightBackArmPos, rightFrontArmPos);
				toShow = toShow || distanceToLeftArm < leftArmRadius || distanceToRightArm < rightArmRadius;

				i.color.a = toShow;
				
				return i.color;
			}
			ENDCG
		}
	}

		Fallback "Diffuse"
}