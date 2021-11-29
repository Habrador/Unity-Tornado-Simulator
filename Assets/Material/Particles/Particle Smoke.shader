// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Erik/Particle Smoke Volume" 
{
	Properties 
	{
		_TintColor ("Tint Color", Color) = (1,1,1,1)
		_InvFade("Soft Particles Factor", Range(0.01, 3.0)) = 1.0
		[NoScaleOffset] _MainTex ("Particle Texture", 2D) = "white" {}
		[NoScaleOffset] _NoiseTex("Noise Texture", 2D) = "white" {}
		[NoScaleOffset] _NoiseTex2("Noise Texture 2", 2D) = "white" {}
		[NoScaleOffset] _NoiseTexRound("Noise Texture Round", 2D) = "white" {}
		_Noise1Speed("Noise 1 speed", Range(0.0, 10.0)) = 1.0
		_Noise2Speed("Noise 2 speed", Range(0.0, 10.0)) = 1.0
	}

	Category 
	{
		//"Queue"="Transparent" so it renders after whats not transparent
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	
		//When graphics are rendered, after all shaders have executed and all textures have been applied, 
		//the pixels are written to the screen. How they are combined with what is already there is controlled by the Blend command.
		//http://docs.unity3d.com/Manual/SL-Blend.html
		//Remove it to get no transparency
		//Blend SrcAlpha One
		Blend SrcAlpha OneMinusSrcAlpha //Traditional transparency
		//Blend One One
	
		ColorMask RGB
		Cull Off Lighting Off ZWrite Off
	
		SubShader 
		{
			Pass 
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_particles
				#pragma multi_compile_fog

				#include "UnityCG.cginc"

				sampler2D _MainTex;
				sampler2D _NoiseTex;
				sampler2D _NoiseTex2;
				sampler2D _NoiseTexRound;
				fixed4 _TintColor;
				float _Noise1Speed;
				float _Noise2Speed;
			
				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD2;
					#endif
				};
			
				float4 _MainTex_ST;

				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					#ifdef SOFTPARTICLES_ON
					o.projPos = ComputeScreenPos (o.vertex);
					COMPUTE_EYEDEPTH(o.projPos.z);
					#endif
					o.color = v.color;
					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				sampler2D_float _CameraDepthTexture;
				float _InvFade;

				//The function frag has a return type of fixed4 (low precision RGBA color). 
				//As it only returns a single value, the semantic is indicated on the function itself, : SV_Target.
				fixed4 frag (v2f i) : SV_Target
				{
					#ifdef SOFTPARTICLES_ON
					float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
					float partZ = i.projPos.z;
					float fade = saturate (_InvFade * (sceneZ-partZ));
					i.color.a *= fade;
					#endif

					//Scale: textcoord is uv, just multiply it to scale float2(i.texcoord.x, i.texcoord.y) * 0.1
					//Move: to move the texture multiply with a time: i.texcoord.y -= _Time[0] * 5
					//This depends on the rotation of the particle, so if the particle is rotating, the texture on all
					//particles will not always move in the same direction
					//tex2d - performs a texture lookup in a given 2D sampler

					//Get the rgba before we modify it, so this is the only line needed if we are not modifying the alpha
					fixed4 col = 2.0f * i.color * _TintColor * tex2D(_MainTex, i.texcoord);


					//Change the alpha with the noise textures
					//Has to begin with col.a so the particle can fade away as we deside in the particle system
					float alpha = col.a;

					//First make it seamless by multiplying it with an "island" texture
					alpha *= tex2D(_NoiseTexRound, i.texcoord).a;

					alpha *= tex2D(
						_NoiseTex, 
						float2(i.texcoord.x -= _Time[0] * 0, i.texcoord.y -= _Time[0] * _Noise1Speed) * 0.1).a;

					alpha *= tex2D(
						_NoiseTex2, 
						float2(i.texcoord.x -= _Time[0] * _Noise2Speed, i.texcoord.y -= _Time[0] * _Noise2Speed) * 1.0).a;

					//Dont forget to multiply by 2 (a higher value makes it thicker)
					alpha *= 3.0;

					//Save the new alpha
					col.a = alpha;

					//Is this only used if we have a fog in the scene?
					UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(0,0,0,0)); // fog towards black due to our blend mode
				

					return col;
				}
				ENDCG 
			}
		}	
	}
}
