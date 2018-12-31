// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Instanced/TileShader" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
	_Layer1Tex("Layer 1 (RGB) Trans (A)", 2D) = "white" {}
		_TilePosition("TilePosition", Vector) = (0,0,0,0)
		_TilePosition1("TilePosition1", Vector) = (0,0,0,0)
	}
		SubShader{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 200

			CGPROGRAM
		#pragma surface surf Lambert alpha:fade

		// Enable instancing for this shader
		#pragma multi_compile_instancing

		// Config maxcount. See manual page.
		// #pragma instancing_options

		sampler2D _MainTex;
		sampler2D _Layer1Tex;

	struct Input {
		float2 uv_MainTex;
		float2 uv_Layer1Tex;
	};

	half _Glossiness;
	half _Metallic;

	struct appdata
	{
		float4 vertex : POSITION;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct v2f
	{
		float4 vertex : SV_POSITION;
		UNITY_VERTEX_INPUT_INSTANCE_ID // necessary only if you want to access instanced properties in fragment Shader.
	};


	v2f vert(appdata v)
	{
		v2f o;

		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_TRANSFER_INSTANCE_ID(v, o); // necessary only if you want to access instanced properties in the fragment Shader.

		o.vertex = UnityObjectToClipPos(v.vertex);
		return o;
	}
	// Declare instanced properties inside a cbuffer.
	// Each instanced property is an array of by default 500(D3D)/128(GL) elements. Since D3D and GL imposes a certain limitation
	// of 64KB and 16KB respectively on the size of a cubffer, the default array size thus allows two matrix arrays in one cbuffer.
	// Use maxcount option on #pragma instancing_options directive to specify array size other than default (divided by 4 when used
	// for GL).
	UNITY_INSTANCING_BUFFER_START(Props)
		UNITY_DEFINE_INSTANCED_PROP(fixed4, _TilePosition)	// Make _Color an instanced property (i.e. an array)
		UNITY_INSTANCING_BUFFER_END(Props)
		UNITY_INSTANCING_BUFFER_START(Props1)
		UNITY_DEFINE_INSTANCED_PROP(fixed4, _TilePosition1)	// Make _Color an instanced property (i.e. an array)
		UNITY_INSTANCING_BUFFER_END(Props1)

		void surf(Input IN, inout SurfaceOutput o) {
		// Albedo comes from a texture tinted by color
		fixed4 c = tex2D(_MainTex, IN.uv_MainTex * UNITY_ACCESS_INSTANCED_PROP(Props, _TilePosition).zw + UNITY_ACCESS_INSTANCED_PROP(Props, _TilePosition).xy);
		// layer1
		fixed4 c1 = tex2D(_Layer1Tex, IN.uv_Layer1Tex* UNITY_ACCESS_INSTANCED_PROP(Props1, _TilePosition1).zw + UNITY_ACCESS_INSTANCED_PROP(Props1, _TilePosition1).xy);

		if (c1.a > 0.5f && c1.r != 1 && c1.g != 1 && c1.b != 1) {
			c0.rgb = c1.rgb;
			c0.a = c1.a;
		}

		o.Albedo = c0.rgb;
		o.Alpha = c0.a;
	}
	ENDCG
	}
		FallBack "VertexLit" // Diffuse
}