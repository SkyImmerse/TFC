// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Custom/Item" {
	Properties{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
	_Layer1Tex("Layer 1 (RGB) Trans (A)", 2D) = "white" {}
	_TilePosition("TilePosition", Vector) = (0,0,0,0)
		_TilePosition1("_TilePosition1", Vector) = (0,0,0,0)
	}

		SubShader{
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

		CGPROGRAM
#pragma surface surf Lambert alpha:fade

#pragma multi_compile_instancing

		sampler2D _MainTex;
	sampler2D _Layer1Tex;
	fixed4 _Color;

	struct Input {
		float2 uv_MainTex;
		float2 uv_Layer1Tex;
	};

	UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_DEFINE_INSTANCED_PROP(fixed4, _TilePosition)	// Make _Color an instanced property (i.e. an array)
		UNITY_INSTANCING_BUFFER_END(Props)
		UNITY_INSTANCING_BUFFER_START(Props1)
		UNITY_DEFINE_INSTANCED_PROP(fixed4, _TilePosition1)	// Make _Color an instanced property (i.e. an array)
		UNITY_INSTANCING_BUFFER_END(Props1)

	void surf(Input IN, inout SurfaceOutput o) {
		// main
		float4 c0 = tex2D(_MainTex, IN.uv_MainTex* UNITY_ACCESS_INSTANCED_PROP(Props, _TilePosition).zw + UNITY_ACCESS_INSTANCED_PROP(Props, _TilePosition).xy);
		// layer1
		float4 c1 = tex2D(_Layer1Tex, IN.uv_Layer1Tex* UNITY_ACCESS_INSTANCED_PROP(Props1, _TilePosition1).zw + UNITY_ACCESS_INSTANCED_PROP(Props1, _TilePosition1).xy);

		if (c1.a > 0.5f && c1.r != 1 && c1.g != 1 && c1.b != 1) {
			c0.rgb = c1.rgb;
			c0.a = c1.a;
		}
		o.Alpha = c0.a;
		o.Albedo = c0.rgb * _Color.rgb;
	}
	ENDCG
	}

		Fallback "Legacy Shaders/Transparent/VertexLit"
}
