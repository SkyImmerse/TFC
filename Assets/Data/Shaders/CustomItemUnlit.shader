// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Custom/ItemUnlit"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Layer1Tex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags
	{
		"Queue" = "Transparent"
		"IgnoreProjector" = "True"
		"RenderType" = "Transparent"
		"PreviewType" = "Plane"
		"CanUseSpriteAtlas" = "True"
	}

		Cull Off
		Lighting Off
		Fog{ Mode Off }
		Blend One OneMinusSrcAlpha

		Pass
	{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile DUMMY PIXELSNAP_ON

#pragma multi_compile_instancing


#include "UnityCG.cginc"

	struct appdata
	{
		float4 vertex : POSITION;
		UNITY_VERTEX_INPUT_INSTANCE_ID
		float2 uv : TEXCOORD0;
		float2 uv1 : TEXCOORD1;
	};

	struct v2f
	{
		float2 uv : TEXCOORD0;
		float2 uv1 : TEXCOORD1;
		float4 vertex : SV_POSITION;
		UNITY_VERTEX_INPUT_INSTANCE_ID // necessary only if you want to access instanced properties in fragment Shader.
	};

	sampler2D _MainTex;
	float4 _MainTex_ST;

	sampler2D _Layer1Tex;
	float4 _Layer1Tex_ST;

	UNITY_INSTANCING_BUFFER_START(Orders)
		UNITY_DEFINE_INSTANCED_PROP(fixed4, _Orders)
		UNITY_INSTANCING_BUFFER_END(Orders)


	UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_DEFINE_INSTANCED_PROP(fixed4, _TilePosition)	// Make _Color an instanced property (i.e. an array)
		UNITY_INSTANCING_BUFFER_END(Props)


		UNITY_INSTANCING_BUFFER_START(Props1)
		UNITY_DEFINE_INSTANCED_PROP(fixed4, _TilePosition1)	// Make _Color an instanced property (i.e. an array)
		UNITY_INSTANCING_BUFFER_END(Props1)

		UNITY_INSTANCING_BUFFER_START(Props2)
		UNITY_DEFINE_INSTANCED_PROP(fixed4, _SelectedColor)	// Make _Color an instanced property (i.e. an array)
		UNITY_INSTANCING_BUFFER_END(Props2)

		v2f vert(appdata v)
	{
		v2f o;
		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_TRANSFER_INSTANCE_ID(v, o); // necessary only if you want to access instanced properties in the fragment Shader.
		v.vertex.z = (100 * UNITY_ACCESS_INSTANCED_PROP(Orders, _Orders).y) - UNITY_ACCESS_INSTANCED_PROP(Orders, _Orders).x/10;
		o.vertex = UnityObjectToClipPos(v.vertex);
		
		o.uv = TRANSFORM_TEX(v.uv* UNITY_ACCESS_INSTANCED_PROP(Props, _TilePosition).zw + UNITY_ACCESS_INSTANCED_PROP(Props, _TilePosition).xy, _MainTex);
		o.uv1 = TRANSFORM_TEX(v.uv1* UNITY_ACCESS_INSTANCED_PROP(Props1, _TilePosition1).zw + UNITY_ACCESS_INSTANCED_PROP(Props1, _TilePosition1).xy, _Layer1Tex);
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		// sample the texture
		fixed4 c0 = tex2D(_MainTex, i.uv);

		/*fixed4 c1 = tex2D(_Layer1Tex, i.uv1);

		if (c1.a > 0.9f && c1.r != 1 && c1.g != 1 && c1.b != 1) {
			c0.rgb = c1.rgb;
			c0.a = c1.a;
		}*/

		if (c0.a < 0.1f) discard;

		return c0;
	}
		ENDCG
	}
	}
}