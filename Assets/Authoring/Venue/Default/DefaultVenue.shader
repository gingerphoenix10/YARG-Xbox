/*
  This is an approximate recreation of the Menu shader in the ShaderToy template.
   It's not exactly the same as the default venue as the shader used hasn't been
   published, however it gets the job done.

   I'll also come clean and admit that this shader **was** created using gernerative AI.
   I do not know enough about HLSL, nor do I have the time to figure out how the shader
   originally works and recreate it.
*/

Shader "ShaderToy/DefaultVenue"
{
    Properties
    {
        _Color_SideA      ("Color Side A",     Color) = (0.000, 0.859, 0.992, 1)
        _Color_SideB      ("Color Side B",     Color) = (0.929, 0.188, 0.125, 1)
        _Color_Background ("Color Background", Color) = (0.000, 0.043, 0.098, 1)

        _Point_Strength ("Point Strength", float) = 1.5

        [NoScaleOffset] _NoiseTex ("Dither Texture", 2D) = "grey" {}
        _NoiseRange ("Noise Range", float) = 0.1
    }

    SubShader
    {
        Pass
        {
            ColorMask RGB
            Cull Off
            ZWrite On
            ZTest Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #define iResolution _ScreenParams
            #define gl_FragCoord ((_iParam.scrPos.xy/_iParam.scrPos.w) * _ScreenParams.xy)

            sampler2D _NoiseTex;
            float4 _NoiseTex_TexelSize;
            float _NoiseRange;

            float4 _Color_SideA;
            float4 _Color_SideB;
            float4 _Color_Background;
            float _Point_Strength;

            const float EPSILON = 1e-6;

            struct appdata_t
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos    : SV_POSITION;
                float4 scrPos : TEXCOORD0;
            };

            v2f vert(appdata_t v)
            {
                v2f o;

                float4 pos = float4(v.vertex.xy * 2.0, 0.0, 1.0);
                #if UNITY_REVERSED_Z
                    pos.z = 0.000001;
                #else
                    pos.z = 0.999999;
                #endif

                o.pos = pos;
                o.scrPos = ComputeScreenPos(pos);
                return o;
            }

            // ------------------------------------------------------------
            // Gradient helpers (ported 1:1)
            // ------------------------------------------------------------

            void iteration(
                inout float3 color,
                inout float amount,
                float2 uv,
                float2 pPos,
                float4 pColor
            )
            {
                float dist = distance(uv, pPos) / _Point_Strength + EPSILON;
                dist = 1 - exp(-dist * dist * 10);
                float strength = 1 - min(EPSILON, log(dist)) - 1;

                color = (pColor.rgb * strength + color * amount) / (amount + strength);
                amount += strength;
            }

            float2 circle(float radius, float speed, float offset)
            {
                return float2(
                    cos(_Time.y * speed + offset),
                    sin(_Time.y * speed + offset)
                ) * radius;
            }

            // ------------------------------------------------------------
            // ShaderToy-style mainImage
            // ------------------------------------------------------------

            float4 mainImage(float2 fragCoord)
            {
                // Normalized screen UV
                float2 uv = fragCoord.xy / iResolution.xy;
                uv -= 0.5;

                // Pixel-perfect dither sampling
                float2 noiseUV =
                    (fragCoord.xy * _NoiseTex_TexelSize.xy);

                float3 noise = tex2D(_NoiseTex, noiseUV).rgb;
                float2 dither = noise.xy * _NoiseRange - _NoiseRange * 0.5;

                uv += dither;

                // Animated points
                float2 sideA = circle(0.75, 0.2, _SinTime.x);
                float2 sideB = circle(0.75, 0.2, 0);
                float2 bg    = circle(_CosTime.y * 0.2 + 0.2, 0.2, UNITY_HALF_PI);

                float3 color = 0;
                float amount = 0;

                iteration(color, amount, uv,  sideA,  _Color_SideA);
                iteration(color, amount, uv, -sideB,  _Color_SideB);
                iteration(color, amount, uv,  bg,     _Color_Background);
                iteration(color, amount, uv, -bg,     _Color_Background);

                return float4(color, 1.0);
            }

            fixed4 frag(v2f _iParam) : SV_Target
            {
                return mainImage(gl_FragCoord);
            }

            ENDCG
        }
    }
}
