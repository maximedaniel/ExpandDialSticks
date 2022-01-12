// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'

Shader "Custom/MyProjectorShader"
{
    Properties
    {
        _Color("Main Color", Color) = (1,0,0,1)
        _MainTex("Main Texture", 2D) = "white" { TexGen ObjectLinear }
    }

        Subshader
    {
        Tags { "RenderType" = "Transparent"}
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
                float2 uvFalloff : TEXCOORD1;
            };

                uniform float4 _Color;
                uniform sampler2D _MainTex;
                uniform float4x4 unity_Projector;
                uniform float4x4 unity_ProjectorClip;

            v2f vert(appdata_tan v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = mul(unity_Projector, v.vertex);
               // _MainTex_ST = float4(0.70, 0.70, 0.23, 1);
               // V1
                //float coeff = 1 - _OrthographicSize /2;
                // V2
                /*float width = 0.3; // 10% pour 1 scale
                float rate = width / _OrthographicSize;
                float coeff = 1 - rate/2;*/
                float coeff = 0;
                //o.uvFalloff = (o.uv - 0.5) *  coeff  + 0.5;
                o.uvFalloff = mul(unity_ProjectorClip, v.vertex);

                return o;
            }

            half4 frag(v2f i) : COLOR
            {
                float4 tex = tex2D(_MainTex, i.uv) * _Color;
                tex.a = 1 - tex.a;
                if (i.uv.w < 0)
                {
                    tex = float4(0,0,0,1);
                }
                return tex;
            }
            ENDCG

        }
    }
}