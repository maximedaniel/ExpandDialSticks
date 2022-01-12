// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//RadialGradientQuad.shader
Shader "Custom/RadialGradientQuad" {
    Properties{
        _ColorA("Color A", Color) = (1, 1, 1, 1)
        _ColorB("Color B", Color) = (0, 0, 0, 1)
        _Top("_Top", Vector) = (0, 1, 0, 1)
        _Bot("_Bot", Vector) = (0, -1, 0, 1)
        _Radius("Radius", Range(0, 5)) = 1
        _VerticalSlide("VerticalSlide", Range(0, 1)) = 0.5
        _HorizontalSlide("HorizontalSlide", Range(0, 1)) = 0.5
    }

        SubShader{
            Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane"}
            LOD 100

            Pass {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                /*struct appdata_t {
                    float4 vertex : POSITION;
                    float2 texcoord : TEXCOORD0;
                };*/
                bool IsInsideCylinder(float3 bot, float3 top, float radius, float3 target) {
                    float3 botTargetVector = target - bot;
                    float3 botTopVector = top - bot;
                    float3 topTargetVector = target - top;
                    float3 topBotVector = bot - top;
                    float angleRadianA = dot(botTargetVector, botTopVector) / (length(botTargetVector) * length(botTopVector));
                    float angleDegreeA = acos(angleRadianA);
                    float distanceBotLine = distance(target, bot) * sin(angleDegreeA);

                    float angleRadianB = dot(topTargetVector, topBotVector) / (length(topTargetVector) * length(topBotVector));
                    float angleDegreeB = acos(angleRadianB);
                    float distanceTopLine = distance(target, top) * sin(angleDegreeB);

                    return (angleRadianA >= 0.0f && angleRadianB >= 0.0f && distanceBotLine < radius && distanceTopLine < radius); //angleDegreeA >= 0.0f && angleDegreeB >= 0.0f &&

                    //return (angleRadianA >= 0.0f && angleDegreeB >= 0.0f);
                    //float distTargetBot = sqrt(pow(_Top.x - _Bot.x, 2) + pow(_Top.y - _Bot.y, 2) + pow(_Top.z - _Bot.z, 2));
                }

                float CylTest_CapsFirst(float3 pt1, float3 pt2, float lengthsq, float radius_sq, float3 testpt)
                {
                    float dx, dy, dz;	// vector d  from line segment point 1 to point 2
                    float pdx, pdy, pdz;	// vector pd from point 1 to test point
                    float dot, dsq;

                    dx = pt2.x - pt1.x;	// translate so pt1 is origin.  Make vector from
                    dy = pt2.y - pt1.y;     // pt1 to pt2.  Need for this is easily eliminated
                    dz = pt2.z - pt1.z;

                    pdx = testpt.x - pt1.x;		// vector from pt1 to test point.
                    pdy = testpt.y - pt1.y;
                    pdz = testpt.z - pt1.z;

                    // Dot the d and pd vectors to see if point lies behind the 
                    // cylinder cap at pt1.x, pt1.y, pt1.z

                    dot = pdx * dx + pdy * dy + pdz * dz;

                    // If dot is less than zero the point is behind the pt1 cap.
                    // If greater than the cylinder axis line segment length squared
                    // then the point is outside the other end cap at pt2.

                    if (dot < 0.0f || dot > lengthsq)
                    {
                        return(-1.0f);
                    }
                    else
                    {
                        // Point lies within the parallel caps, so find
                        // distance squared from point to line, using the fact that sin^2 + cos^2 = 1
                        // the dot = cos() * |d||pd|, and cross*cross = sin^2 * |d|^2 * |pd|^2
                        // Carefull: '*' means mult for scalars and dotproduct for vectors
                        // In short, where dist is pt distance to cyl axis: 
                        // dist = sin( pd to d ) * |pd|
                        // distsq = dsq = (1 - cos^2( pd to d)) * |pd|^2
                        // dsq = ( 1 - (pd * d)^2 / (|pd|^2 * |d|^2) ) * |pd|^2
                        // dsq = pd * pd - dot * dot / lengthsq
                        //  where lengthsq is d*d or |d|^2 that is passed into this function 

                        // distance squared to the cylinder axis:

                        dsq = (pdx * pdx + pdy * pdy + pdz * pdz) - dot * dot / lengthsq;

                        if (dsq > radius_sq)
                        {
                            return(-1.0f);
                        }
                        else
                        {
                            return(dsq);		// return distance squared to axis
                        }
                    }
                }

                struct v2f {
                    float4  pos : SV_POSITION;
                    float3  worldPos : TEXCOORD0;
                    float3  worldNormal : TEXCOORD1;
                    //float4 vertex : SV_POSITION;
                    //half2 texcoord : TEXCOORD0;
                };

                v2f vert(appdata_base v)
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                    o.worldNormal = mul(unity_ObjectToWorld, float4(v.normal, 0.0)).xyz;

                    //o.vertex = UnityObjectToClipPos(v.vertex); //UnityObjectToClipPos(v.vertex);
                   // o.texcoord = v.texcoord;
                    return o;
                }

                fixed4 _ColorA, _ColorB;
                float _Slide, _Radius, _VerticalSlide, _HorizontalSlide;
                float4 _Top, _Bot;

                fixed4 frag(v2f i) : SV_Target
                {
                    float4 _Height = _Bot + _VerticalSlide * (_Top - _Bot);

                    float4 _Width = _Radius - _HorizontalSlide * _Radius;

                    bool isInsideCylinder = IsInsideCylinder(_Top.xyz, _Height.xyz, _Width, i.worldPos);
                        // CylTest_CapsFirst(_Top, _Bot, sqrt(_Height), sqrt(_Radius), i.worldPos);

                    //float t = length(i.texcoord - float2(0.5, 0.5)) ; // 1.141... = sqrt(2) //* 1.41421356237
                    return (isInsideCylinder) ? _ColorA : _ColorB;
                   // return (t > _Slide) ? _ColorA : _ColorB;
                    //return lerp(_ColorA, _ColorB, t + (_Slide - 0.5) * 2);
                }
                ENDCG
            }
    }
}
