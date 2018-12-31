Shader "Unlit/LightSurfaceShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_AmbientColor("AmbientColor", Color) = (0, 0, 0, 0)
		_Layer("Layer", Int) = 7
    }
    SubShader
    {
		Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
        LOD 100
		ZWrite Off
		Lighting Off
		Fog { Mode Off }
			
		BlendOp Add
		Blend DstColor SrcColor
        Pass
        {
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _AmbientColor;
			int _LightCounts;
			int _Layer;
			float4 _LightsPositions[300];
			float4 _LightColors[300];

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
			float quarticOut(float t) {
				return pow(t - 1.0, 3.0) * (1.0 - t) + 1.0;
			}

			float map(float s, float a1, float a2, float b1, float b2)
			{
				return b1 + (s - a1)*(b2 - b1) / (a2 - a1);
			}


			float4 getLightColor(int totalColors, v2f i, fixed2 lightPos, float intensity, float4 lightColor) {
				float lightDistance = 5.3;
				float diff = quarticOut(map(clamp(length(i.uv - lightPos), 0.02, lightDistance), 0, lightDistance, 0.0, 1));
				return float4(lerp(_AmbientColor.rgb, lightColor.rgb * map(clamp(1, 0.01, 1), 0, 1, 0, 5), (1 - diff)), diff);
			}

            fixed4 frag (v2f input) : SV_Target
            {
				fixed4 col = _AmbientColor;


				float cof =  float(1) / _LightCounts;
				float4 final = col;

				for (int i = 0; i < _LightCounts; i++) {
					float4 result = getLightColor(_LightCounts, input, float2(_LightsPositions[i].x+1-0.5, _LightsPositions[i].y-1.25+0.5), _LightsPositions[i].w, _LightColors[i]);
					final = (result + final);
				}

				final /= (_LightCounts+1)*2;


				/*float2 pos = float2(0.048, 0.0);
				float2 size = float2(0.190, 0.9);*/
/*
				float2 pos = float2(0.6, 0.0);
				float2 size = float2(0.3, 0.55);

				if (input.uv.x > pos.x && input.uv.x < pos.x + size.x &&
				   input.uv.y > pos.y && input.uv.y < pos.y + size.y) {
					col.rgb = _AmbientColor.rgb;
				}
				else {
					col.rgba = final;
				}

				pos = float2(0.8, 0.15);
				size = float2(0.1, 1);

				if (input.uv.x > pos.x && input.uv.x < pos.x + size.x &&
					input.uv.y > pos.y && input.uv.y < pos.y + size.y) {
					col.rgb = _AmbientColor.rgb;
				}

				pos = float2(0.6, 0.0);
				size = float2(0.3, 0.55);

				if (input.uv.x > pos.x && input.uv.x < pos.x + size.x &&
					input.uv.y > pos.y && input.uv.y < pos.y + size.y) {
					col.rgb = _AmbientColor.rgb;
				}

				pos = float2(0.438, 0.5);
				size = float2(0.15, 0.1);

				if (input.uv.x > pos.x && input.uv.x < pos.x + size.x &&
					input.uv.y > pos.y && input.uv.y < pos.y + size.y) {
					col.rgb = lerp(_AmbientColor.rgb, final, 0.6);
				}

				pos = float2(0.438, 0.65);
				size = float2(0.15, 0.15);

				if (input.uv.x > pos.x && input.uv.x < pos.x + size.x &&
					input.uv.y > pos.y && input.uv.y < pos.y + size.y) {
					col.rgb = lerp(_AmbientColor.rgb, final, 0.6);
				}

				pos = float2(0.438, 0.75);
				size = float2(0.15, 0.1);

				if (input.uv.x > pos.x && input.uv.x < pos.x + size.x &&
					input.uv.y > pos.y && input.uv.y < pos.y + size.y) {
					col.rgb = lerp(_AmbientColor.rgb, final, 0);
				}
*/
				/*pos = float2(0.487, 0.93);
				size = float2(0.1, 0.18);

				if (input.uv.x > pos.x && input.uv.x < pos.x + size.x &&
					input.uv.y > pos.y && input.uv.y < pos.y + size.y) {
					col.rgb = _AmbientColor.rgb;
				}*/
				/*pos = float2(0.0, 0.0);
				size = float2(0.490, 0.9);

				if (input.uv.x > pos.x && input.uv.x < pos.x + size.x &&
					input.uv.y > pos.y && input.uv.y < pos.y + size.y) {
					col.rgb = _AmbientColor.rgb;
				}
				else {
					col.rgba = final;
				}*/
				return final;
            }
            ENDCG
        }
    }
}
