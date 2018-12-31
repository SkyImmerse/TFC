// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Custom/Creature"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Layer1Tex("Texture", 2D) = "white" {}
		_MaskTex("Mask (RGBA)", 2D) = "white" {}
		 _Addon1Tex("Addon 1 (RGBA)", 2D) = "white" {}
		 _Addon2Tex("Addon 2 (RGBA)", 2D) = "white" {}
		 _AddonMask1Tex("Addon Mask 1 (RGBA)", 2D) = "white" {}
		 _AddonMask2Tex("Addon Mask 2 (RGBA)", 2D) = "white" {}
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
		Blend One  OneMinusSrcAlpha

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
		float2 uv_MaskTex : TEXCOORD1;
		float2 uv_Addon1Tex : TEXCOORD2;
		float2 uv_Addon2Tex : TEXCOORD3;
		float2 uv_AddonMask1Tex : TEXCOORD4;
		float2 uv_AddonMask2Tex : TEXCOORD5;
	};

	struct v2f
	{
		float2 uv : TEXCOORD0;
		float2 uv_MaskTex : TEXCOORD1;
		float2 uv_Addon1Tex : TEXCOORD2;
		float2 uv_Addon2Tex : TEXCOORD3;
		float2 uv_AddonMask1Tex : TEXCOORD4;
		float2 uv_AddonMask2Tex : TEXCOORD5;
		float4 vertex : SV_POSITION;
		UNITY_VERTEX_INPUT_INSTANCE_ID // necessary only if you want to access instanced properties in fragment Shader.
	};

	sampler2D _MainTex;
	float4 _MainTex_ST;
	sampler2D _Layer1Tex;
	float4 _Layer1_ST;
	sampler2D _MaskTex;
	float4 _MaskTex_ST;
	sampler2D _Addon1Tex;
	float4 _Addon1Tex_ST;
	sampler2D _Addon2Tex;
	float4 _Addon2Tex_ST;
	sampler2D _AddonMask1Tex;
	float4 _AddonMask1Tex_ST;
	sampler2D _AddonMask2Tex;
	float4 _AddonMask2Tex_ST;

		UNITY_INSTANCING_BUFFER_START(Orders)
		UNITY_DEFINE_INSTANCED_PROP(fixed4, _Orders)
		UNITY_INSTANCING_BUFFER_END(Orders)

		UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_DEFINE_INSTANCED_PROP(fixed4, _TilePosition)	// Make _Color an instanced property (i.e. an array)
		UNITY_INSTANCING_BUFFER_END(Props)


		UNITY_INSTANCING_BUFFER_START(Props2)
		UNITY_DEFINE_INSTANCED_PROP(fixed4, _MaskTex_Positions)	// Make _Color an instanced property (i.e. an array)
		UNITY_INSTANCING_BUFFER_END(Props2)
		
		UNITY_INSTANCING_BUFFER_START(Props3)
		UNITY_DEFINE_INSTANCED_PROP(fixed4, _AddonMask1Tex_Positions)	// Make _Color an instanced property (i.e. an array)
		UNITY_INSTANCING_BUFFER_END(Props3)
		
		UNITY_INSTANCING_BUFFER_START(Props4)
		UNITY_DEFINE_INSTANCED_PROP(fixed4, _AddonMask2Tex_Positions)	// Make _Color an instanced property (i.e. an array)
		UNITY_INSTANCING_BUFFER_END(Props4)
		
		UNITY_INSTANCING_BUFFER_START(Props5)
		UNITY_DEFINE_INSTANCED_PROP(fixed4, _Addon1Tex_Positions)	// Make _Color an instanced property (i.e. an array)
		UNITY_INSTANCING_BUFFER_END(Props5)

		UNITY_INSTANCING_BUFFER_START(Props6)
		UNITY_DEFINE_INSTANCED_PROP(fixed4, _Addon2Tex_Positions)	// Make _Color an instanced property (i.e. an array)
		UNITY_INSTANCING_BUFFER_END(Props6)


		UNITY_INSTANCING_BUFFER_START(Props7)
		UNITY_DEFINE_INSTANCED_PROP(fixed4, _HeadColor)	// Make _Color an instanced property (i.e. an array)
		UNITY_INSTANCING_BUFFER_END(Props7)

		UNITY_INSTANCING_BUFFER_START(Props8)
		UNITY_DEFINE_INSTANCED_PROP(fixed4, _BodyColor)	// Make _Color an instanced property (i.e. an array)
		UNITY_INSTANCING_BUFFER_END(Props8)

		UNITY_INSTANCING_BUFFER_START(Props9)
		UNITY_DEFINE_INSTANCED_PROP(fixed4, _LegsColor)	// Make _Color an instanced property (i.e. an array)
		UNITY_INSTANCING_BUFFER_END(Props9)

		UNITY_INSTANCING_BUFFER_START(Props10)
		UNITY_DEFINE_INSTANCED_PROP(fixed4, _FeetColor)	// Make _Color an instanced property (i.e. an array)
		UNITY_INSTANCING_BUFFER_END(Props10)

		v2f vert(appdata v)
	{
		v2f o;
		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_TRANSFER_INSTANCE_ID(v, o); // necessary only if you want to access instanced properties in the fragment Shader.
		v.vertex.z = (100 * UNITY_ACCESS_INSTANCED_PROP(Orders, _Orders).y) - (UNITY_ACCESS_INSTANCED_PROP(Orders, _Orders).x)/10;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = TRANSFORM_TEX(v.uv* UNITY_ACCESS_INSTANCED_PROP(Props, _TilePosition).zw + UNITY_ACCESS_INSTANCED_PROP(Props, _TilePosition).xy, _MainTex);
	
		o.uv_MaskTex = TRANSFORM_TEX(v.uv_MaskTex* UNITY_ACCESS_INSTANCED_PROP(Props2, _MaskTex_Positions).zw + UNITY_ACCESS_INSTANCED_PROP(Props2, _MaskTex_Positions).xy, _MaskTex);

		o.uv_Addon1Tex = TRANSFORM_TEX(v.uv_MaskTex* UNITY_ACCESS_INSTANCED_PROP(Props5, _Addon1Tex_Positions).zw + UNITY_ACCESS_INSTANCED_PROP(Props5, _Addon1Tex_Positions).xy, _MainTex);

		o.uv_Addon2Tex = TRANSFORM_TEX(v.uv_MaskTex* UNITY_ACCESS_INSTANCED_PROP(Props6, _Addon2Tex_Positions).zw + UNITY_ACCESS_INSTANCED_PROP(Props6, _Addon2Tex_Positions).xy, _MainTex);

		o.uv_AddonMask1Tex = TRANSFORM_TEX(v.uv_MaskTex* UNITY_ACCESS_INSTANCED_PROP(Props3, _AddonMask1Tex_Positions).zw + UNITY_ACCESS_INSTANCED_PROP(Props3, _AddonMask1Tex_Positions).xy, _MainTex);

		o.uv_AddonMask2Tex = TRANSFORM_TEX(v.uv_MaskTex* UNITY_ACCESS_INSTANCED_PROP(Props4, _AddonMask2Tex_Positions).zw + UNITY_ACCESS_INSTANCED_PROP(Props4, _AddonMask2Tex_Positions).xy, _MainTex);

		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		// sample the texture
		float4 c0 = tex2D(_MainTex, i.uv);

		// mask
		float4 c1 = tex2D(_MaskTex, i.uv_MaskTex);

		// addons
		float4 a1 = tex2D(_Addon1Tex, i.uv_Addon1Tex);
		float4 a2 = tex2D(_Addon2Tex, i.uv_Addon2Tex);

		// addons mask
		float4 am1 = tex2D(_AddonMask1Tex, i.uv_AddonMask1Tex);
		float4 am2 = tex2D(_AddonMask2Tex, i.uv_AddonMask2Tex);

		// filter mask
		if (c1.r > 0.5f && c1.g < 0.5f && c1.b < 0.5f) {
			c1.rgb = UNITY_ACCESS_INSTANCED_PROP(Props8, _BodyColor)* c0.rgb;
		}
		else if (c1.r > 0.5f && c1.g > 0.5f && c1.b < 0.5f) {
			c1.rgb = UNITY_ACCESS_INSTANCED_PROP(Props7, _HeadColor)* c0.rgb;
		}
		else if (c1.r < 0.5f && c1.g > 0.5f && c1.b < 0.5f) {
			c1.rgb = UNITY_ACCESS_INSTANCED_PROP(Props9, _LegsColor) * c0.rgb;
		}
		else if (c1.r < 0.5f && c1.g < 0.5f && c1.b > 0.5f) {
			c1.rgb = UNITY_ACCESS_INSTANCED_PROP(Props10, _FeetColor) * c0.rgb;
		}

		float Alpha = c0.a;
		c1.rgb = c0.rgb;

		// filter addon1 mask
		if (am1.r > 0.5f && am1.g < 0.5f && am1.a > 0.5f) {
			c1.rgb = UNITY_ACCESS_INSTANCED_PROP(Props8, _BodyColor) * a1.rgb;
			Alpha = a1.a;
		}
		if (am1.r < 0.5f && am1.g > 0.5f && am1.a > 0.5f) {
			c1.rgb = UNITY_ACCESS_INSTANCED_PROP(Props9, _LegsColor) * a1.rgb;
			Alpha = a1.a;
		}

		// filter addon1 mask
		if (am2.r > 0.5f && am2.g < 0.5f && am2.a > 0.5f) {
			c1.rgb = UNITY_ACCESS_INSTANCED_PROP(Props7, _HeadColor) * a2.rgb;
			Alpha = a2.a;
		}
		if (am2.r < 0.5f && am2.g > 0.5f && am2.a > 0.5f) {
			c1.rgb = UNITY_ACCESS_INSTANCED_PROP(Props7, _HeadColor) * a2.rgb;
			Alpha = a2.a;
		}
		if (c0.a < 0.1f) discard;
		return float4(c1.rgb, Alpha);
	}
		ENDCG
	}
	}
}