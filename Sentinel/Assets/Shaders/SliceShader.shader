Shader "Custom/SliceShader" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_SliceGuide ("Slice Guide (RGB)", 2D) = "white" {}
      	_SliceAmount ("Slice Amount", Range(0.0, 1.0)) = 0.5
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		Cull Off
		
		CGPROGRAM
		#pragma surface surf Lambert addshadow

		sampler2D _MainTex;
      	sampler2D _SliceGuide;
     	float _SliceAmount;

		struct Input {
			float2 uv_MainTex;
			float2 uv_SliceGuide;
          	float _SliceAmount;
		};

		void surf (Input IN, inout SurfaceOutput o) {
          clip(tex2D (_SliceGuide, IN.uv_SliceGuide).rgb - _SliceAmount);
          o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
