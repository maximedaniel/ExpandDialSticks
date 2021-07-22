// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'

Shader "Custom/MyProjectorShader"
{
    Properties
    {
        _Color("Main Color", Color) = (1,0,0,1)
        _MainTex("Cookie", 2D) = "white" { TexGen ObjectLinear }
    }

        Subshader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+100"}
        Pass
        {
            ZWrite Off
            Offset -1, -1

            Fog { Mode Off }

            ColorMask RGB
            Blend OneMinusSrcAlpha SrcAlpha
            //Blend SrcColor DstColor 



            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_fog_exp2
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float4 uv       : TEXCOORD0;
                float4 uvFalloff : TEXCOORD1;
            };

                uniform sampler2D _MainTex;
                uniform float4x4 unity_Projector;
                uniform float4x4 unity_ProjectorClip;
                uniform float4 _Color;

            v2f vert(appdata_tan v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = mul(unity_Projector, v.vertex);
                o.uvFalloff = mul(unity_ProjectorClip, v.vertex);
                return o;
            }

            half4 frag(v2f i) : COLOR
            {
                float4 tex = tex2Dproj(_MainTex, UNITY_PROJ_COORD(i.uv))  *  _Color;
                tex.a = 1 - tex.a;
                if (i.uv.w < 0)
                {
                    tex = float4(0,0,0,1);
                }

               /* fixed4 texS = tex2Dproj(_MainTex, UNITY_PROJ_COORD(i.uv));
                texS.a = 1 - texS.a;

                fixed4 res = float4(_Color.r, _Color.g, _Color.b, _Color.a) * texS;*/
                return tex;
            }
            ENDCG

        }
    }
}