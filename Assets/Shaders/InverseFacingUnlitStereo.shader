// Based on Unlit shader, but culls the front faces instead of the back

Shader "Unlit/InverseFacingUnlitStereo" {
	Properties {
		_MainTex ("Base Left (RGB)", 2D) = "white" {}
		_MainTexRight ("Base Right (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		Cull front    // ADDED BY BERNIE, TO FLIP THE SURFACES
		LOD 100
		
		Pass {  
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				
				#include "UnityCG.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f {
					float4 vertex : SV_POSITION;
					half2 texcoord : TEXCOORD0;
				};

				sampler2D _MainTex;
				sampler2D _MainTexRight;
				float4 _MainTex_ST;

				v2f vert (appdata_t v) {
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					// ADDED BY BERNIE:
					v.texcoord.x = 1 - v.texcoord.x;				
					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
					return o;
				}
				
				fixed4 frag (v2f i) : SV_Target	{	
					fixed4 col;
					if (unity_StereoEyeIndex == 0) {
						return tex2D(_MainTex, UnityStereoScreenSpaceUVAdjust(i.texcoord, _MainTex_ST));
					} else {
						return tex2D(_MainTexRight, UnityStereoScreenSpaceUVAdjust(i.texcoord, _MainTex_ST));
					}
					return col;
				}
			ENDCG
		}
	}

}