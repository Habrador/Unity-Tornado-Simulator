// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//The most common approach to make volumetric clouds is to draw them in a height zone above the camera using fbm 
//fbm - fractal brownian motion = layering Perlin noise of different frequencies
//This noise is the often combines with a gradient to define a change in cloud density over height

Shader "Erik/Cloud"
{
	Properties
	{
		[Header(Clouds)]
		//How dense the clouds should be
		_CloudDensity("Cloud Density", Range(0, 10)) = 0.5
		//How dark the clouds chould be
		_CloudDarkness("Cloud Darkness", Range(0, 5)) = 0.5
		//Cloud color
		_CloudColor("Cloud Color", Color) = (1, 1, 1, 1)
		
		[Header(Noise)]
		//Noise textures
		[NoScaleOffset] _NoiseTex("Noise Texture", 2D) = "white" {}
		[NoScaleOffset] _NoiseTex2("Noise Texture 2", 2D) = "white" {}
		[NoScaleOffset] _HeightGradient("Height Gradient", 2D) = "white" {}
		[NoScaleOffset] _TornadoMarker("Tornado Marker", 2D) = "white" {}
	}
	
	
	SubShader
	{
		Tags
		{
			//"Queue" = "Transparent"
			//"LightMode" = "ForwardBase"
		}

		Pass
		{
			//Traditional transparency
			//Blend SrcAlpha OneMinusSrcAlpha

			//Use this if we want to render full screen
			Blend SrcAlpha Zero


			CGPROGRAM

			//Pragmas
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			#pragma glsl

			#ifndef SHADER_API_D3D11
			#pragma target 3.0
			#else
			#pragma target 4.0
			#endif


			//Structs
			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};


			//Input
			//appdata_base includes position, normal and one texture coordinate
			v2f vert(appdata_base v)
			{
				v2f o;

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;

				return o;
			}



			//
			//User defined functions and variables
			//

			//Camera
			float3 _CamPos;
			float3 _CamRight;
			float3 _CamUp;
			float3 _CamForward;
			//For full screen
			float _AspectRatio;
			float _FieldOfView;
			//Clouds
			float _CloudDensity;
			float _CloudDarkness;
			fixed4 _CloudColor;
			//Where the sky plane begins and ends - have to be static
			static float _SkyStart = 230.0;
			static float _SkyEnd = 260.0;
			static fixed4 _FogColor = fixed4(fixed3(0.7, 0.7, 0.7), 1.0);
			//Noise
			sampler2D _NoiseTex;
			sampler2D _NoiseTex2;
			sampler2D _HeightGradient;
			//Unity specific
			//The color of the main light
			fixed4 _LightColor0;
			//Tornado
			float3 _TornadoPos;
			sampler2D _TornadoMarker;



			//The distance square between 2 vectors 
			float3 distanceSquare(float3 vec1, float3 vec2)
			{
				float x = vec1.x - vec2.x;
				float y = vec1.y - vec2.y;
				float z = vec1.z - vec2.z;
				
				float distSqr = (x * x) + (y * y) + (z * z);

				return distSqr;
			}



			//Get the color by mixing and animating different noise textures to simulate volumetric clouds
			fixed4 getColorFlat(float3 intersection)
			{
				float alpha = tex2D(_NoiseTex2, float2(intersection.x, intersection.z) * 0.0002).a;

				//float alpha = fbm(intersection, 4);

				//Save it or it will overwrite?
				float3 intersectCopy = intersection;

				//Test the Diablo trick to make the clouds look thick
				//alpha *= tex2D(_NoiseTex, float2(intersection.x += _Time[0] * 50.0, intersection.z) * 0.001).a;

				//alpha *= tex2D(_NoiseTex, float2(intersectCopy.x += _Time[0] * 70.0, intersection.z) * 0.005).a;

				//alpha *= 3.0;


				//float3 rgb = tex2D(_NoiseTex, float2(intersection.x, intersection.z) * 0.002).rgb;

				float3 rgb = _CloudColor;

				fixed4 color = fixed4(rgb, alpha);

				return color;
			}



			//Calculate the density at a certain coordinate
			float getDensityVolume(float3 sampleCoordinate)
			{
				//Amount of absorbing material in the sample
				//tex2Dlod gives the same result as tex2D
				//tex2D breaks the shader because of looping?
				//fixed density = tex2Dlod(_NoiseTex2, float4(sampleCoordinate.x, sampleCoordinate.z, 0, 0) * 0.001).a;

				float3 sampleCoordinateCopy = sampleCoordinate;

				fixed density = tex2Dlod(_NoiseTex2, float4(sampleCoordinate.x, sampleCoordinate.z += _Time[1] * 5, 0, 0) * 0.001).a;

				density *= tex2Dlod(_NoiseTex, float4(sampleCoordinateCopy.x += _Time[1] * 5, sampleCoordinateCopy.z, 0, 0) * 0.001).a;

				density *= 2.0;

				//Less thick at the top
				fixed currentHeightPercentage = (sampleCoordinate.y - _SkyStart) / (_SkyEnd - _SkyStart);

				//Reduce density at the bottom of the clouds
				density *= tex2Dlod(_HeightGradient, float4(0.0, currentHeightPercentage, 0.0, 0.0)).a;

				return density;
			}
			


			//Get the value of the phase function
			//It determines how much light traveling through a medium in direction w will, upon scattering, reflect to direction w'
			half getPhaseFunctionValue(fixed3 sunDir, fixed3 viewDir)
			{
				//Henyey-Greenstein
				//The parameter g can be adjusted to control the relative amounts of forward and backward scattering
				//-1 <= g <= 1, g > 0 -> forward scattering more dominant, g = 0 means isotropic (same in all directions)
				//Forward scattering will result in brighter sun, and this is what should happen when you model clouds
				//Clouds = forward scattering
				const fixed g = 0.2;

				const half pi = 3.14;

				const half gSquare = g * g;

				//Theta is the relative angle between w and w'
				half cosTheta = dot(sunDir, viewDir);

				half p_theta = (1.0 / (4.0 * pi)) * ((1 - gSquare) / pow(1.0 + gSquare - 2.0 * g * cosTheta, 3.0 / 2.0));

				return p_theta;
			}



			//Get the lightning at a certain sample by raymarching towards the light from that sample
			float3 getIncidentLightning(float3 startCoordinate, fixed3 ray)
			{
				//Get the direction to the sun (is NOT the direction of the sun)
				fixed3 lightDir = normalize(_WorldSpaceLightPos0.xyz);

				//We need a plane so we can define where the clouds end - use same plane equation as in getColor
				float3 planePos = float3(0.0, _SkyEnd, 0.0);
				//The sky's normal is always pointing up
				fixed3 planeNormal = float3(0.0, 1.0, 0.0);

				//Calculate the denominator once - we need it several times
				fixed denominator = dot(lightDir, planeNormal);

				//Init values
				float transmittance = 1.0;

				//If the ray is pointing at the plane and is not parallell to it
				if (denominator > 0.0001)
				{
					//Find the distance from the sample to the sky plane
					float distanceToPlane = dot(planePos - startCoordinate, planeNormal) / denominator;

					//To sample the cloud volume we divide the distance into x parts
					//So all cloud pixels are sampled equal amount of times
					//The pdf on cloud game uses between 64 and 128 samples depending on the distance
					//the ray will travel, which is not constant since they have a curved earth
					const int samples = 5;

					//The distance the ray will move between each sample
					float distToMove = distanceToPlane / samples;

					//also calculate alpha even though we dont need alpha
					//but if alpha reaches 1, then we can stop looping, so it may save time!
					fixed alpha = 0.0;

					//Sample all points within the cloud layer
					[unroll(samples)] for (int i = 0; i < samples; i++)
					{
						//The coordinate of the current sample
						float3 sampleCoordinate = startCoordinate + (lightDir * distToMove * i);

						//Calculate transmittance
						fixed sigma = 0.1;
						//This density will determine which type of clouds we have, higher means darker
						float density = getDensityVolume(sampleCoordinate) * _CloudDarkness;

						//Calculate the transmittance at this point
						float thisTransmittance = exp(-sigma * density * distToMove);

						//Add the transmittance of this sample to the total transmittance
						transmittance *= thisTransmittance;

						//When we know the transmittance we can get the new alpha
						alpha += ((1.0 - thisTransmittance) * (1.0 - alpha));

						//Stop iterating if alpha is above 1
						if (alpha >= 1.0)
						{
							break;
						}
					}
				}


				//When we know the transmittance at this point from the sun, we can calculate the lightning
				//Is important and cant be a constant, because this function will make the clouds darker at the bottom
				float phaseFunction = getPhaseFunctionValue(lightDir, ray);

				float3 incidentLightning = transmittance * _LightColor0.rgb * phaseFunction;

				return incidentLightning;
			}



			//Get the color from volume rendering
			//startPos - start coordinates of the ray (= where the ray first hit the cloud)
			//ray - direction of the ray we have
			fixed4 getColorVolumeRendering(float3 startPos, fixed3 ray)
			{
				//Init the color
				fixed4 color = 0.0;
				fixed3 rgb = 0.0;

				//We need a plane so we can define where the clouds end - use same plane equation as in getColor
				float3 planePos = float3(0.0, _SkyEnd, 0.0);
				//The sky's normal is always pointing up
				const fixed3 planeNormal = float3(0.0, 1.0, 0.0);

				//Calculate the denominator once - we need it several times
				fixed denominator = dot(ray, planeNormal);

				//If the ray is pointing at the plane and is not parallell to it
				if (denominator > 0.0001)
				{
					//
					//Change the color of the clouds where the tornado is
					//
					float3 intersection = startPos;
					intersection.y = 0.0;
					_TornadoPos.y = 0.0;

					bool isCloseToTornado = false;

					//The distance from the center of the tornado to this coordinate on the plane
					//float distToTornado = length(intersection - _TornadoPos);
					float distToTornadoSqr = distanceSquare(intersection, _TornadoPos);

					//How large piece of the sky around the tornado should we change?
					const float tornadoRadius = 250.0;

					fixed tornadoDensity = 0.0;

					if (distToTornadoSqr < tornadoRadius * tornadoRadius)
					{
						isCloseToTornado = true;

						//From x,z to u,v
						float xMin = _TornadoPos.x - tornadoRadius;
						float xMax = _TornadoPos.x + tornadoRadius;

						float zMin = _TornadoPos.z - tornadoRadius;
						float zMax = _TornadoPos.z + tornadoRadius;

						//(value-min)/(max-min) to get 0 -> 1 range
						fixed2 uv = fixed2((intersection.x - xMin) / (xMax - xMin), (intersection.z - zMin) / (zMax - zMin));

						//This is the round texture
						tornadoDensity = tex2D(_TornadoMarker, uv).a;

						//Add a moving texture to make it look better
						tornadoDensity *= tex2D(_NoiseTex2, float2(intersection.x, intersection.z += _Time[1] * 50) * 0.001).a;

						tornadoDensity *= 2.0;
					}


					//
					//Get the color from volume rendering
					//

					//Find the distance from the bottom sky plane to the top sky plane
					float distanceToPlane = dot(planePos - startPos, planeNormal) / denominator;

					//To sample the cloud volume we divide the distance into x parts
					//So all cloud pixels are sampled equal amount of times
					//The pdf on cloud game uses between 64 and 128 samples depending on the distance
					//the ray will travel, which is not constant since they have a curved earth
					const int samples = 20;

					//The distance the ray will move between each sample
					float distToMove = distanceToPlane / samples;

					//Init values
					fixed alpha = 0.0;
					float transmittance = 1.0;

					//Sample all points within the cloud layer
					//Use the [unroll(n)] attribute to force an exact higher number
					[unroll(samples)] for (int i = 0; i < samples; i++)
					{
						//The coordinate of the current sample
						float3 sampleCoordinate = startPos + (ray * distToMove * i);

						//Calculate transmittance
						const fixed sigma = 0.1;

						//This density will determine how much cloud we have
						float density = getDensityVolume(sampleCoordinate) * _CloudDensity;

						//Add more density closer to the tornado center
						if (isCloseToTornado && i == 0)
						{
							density += tornadoDensity * 1.0;
						}

						//Calculate the transmittance at this point
						float thisTransmittance = exp(-sigma * density * distToMove);

						//Add the transmittance of this sample to the total transmittance
						transmittance *= thisTransmittance;

						//When we know the transmittance we can get the new alpha
						alpha += ((1.0 - thisTransmittance) * (1.0 - alpha));

						//Stop iterating if alpha is above 1
						if (alpha >= 1.0)
						{
							break;
						}

						//Get the incident lightning by using another ray march
						float3 incidentLightning = getIncidentLightning(sampleCoordinate, ray);

						//Change the color - from "production volume rendering fundamentals"
						//rgb += transmittance * incidentLightning * _CloudColor * density * distToMove;

						//Change color - from Tessendorf pdf slides, which will avoid opacity problems
						rgb += ((1.0 - thisTransmittance) / sigma) * _CloudColor * transmittance * incidentLightning;

						//To avoid black clouds
						if (rgb.x < 0.6)
						{
							rgb = 0.6;
						}
					}

					
					//The color of the clouds should be more white the close to the tornado we are to 
					//make the particles merge with the cloud
					if (isCloseToTornado)
					{
						rgb += 0.3 * tornadoDensity;
					}

					//To avoid white clouds
					if (rgb.x > 0.98)
					{
						rgb = 0.98;
					}
					
					//Final color from ray marching
					color = fixed4(rgb, alpha);



					//
					//Add fog to avoid strange black dots where the sky ends
					//
					float distance = length(startPos - _CamPos);
					
					//Fog data
					fixed fogDensity = 0.001;

					//Fog methods
					//Exponential
					float f = exp(-distance * fogDensity);
					//Exponential square
					//float f = exp(-distance * distance * fogDensity * fogDensity);
					//Linear - the first term is where the fog begins
					//float f = fogDensity * (700 - distance);

					color = fixed4((_FogColor * (1.0 - f)) + (rgb * f), alpha);
				}
				

				return color;
			}




			//Get the color where the ray hit something, else return transparent color
			fixed4 getColor(float3 startPos, fixed3 ray)
			{
				//Init data
				//The distance to the horizon so we know when to stop drawing clouds
				//const float distanceToHorizon = 2000.0;
				//Init color is always 0
				fixed4 color = 0;

				//First we need to find where the ray intersects with the sky = where the clouds begin
				//This plane is here our "container", and we have another plane where the clouds end
				//At the same time we will also find the distance to the sky
				//http://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-plane-and-ray-disk-intersection
				//Doesnt matter where the sky begins, just the height is important
				const float3 planePos = float3(0.0, _SkyStart, 0.0);
				//The sky's normal is always pointing up
				const fixed3 planeNormal = float3(0.0, 1.0, 0.0);


				//Calculate the denominator once - we need it several times
				fixed denominator = dot(ray, planeNormal);	

				//Make sure the denominator is above a ceratin small value in case the ray never reaches the plane
				//when the ray and plane are almost parallell or opposite
				if (denominator > 0.0001)
				{
					//The distance from the camera to the sky plane
					float distanceToPlane = dot(planePos - startPos, planeNormal) / denominator;
					
					//Get the point of intersection where the ray hits the plane
					float3 intersection = startPos + distanceToPlane * ray;

					//Alt 1 - Get the color at this point by volume rendering
					color = getColorVolumeRendering(intersection, ray);

					//Alt 2 - Get the color at this point by using a texture on a plane
					//color = getColorDiablo(intersection);

					//color.rgb *= getColorDiablo(intersection).rgb;

					//Blend with fog color;
					//color.rgb *= fixed3(0.7, 0.7, 0.7);

					//If full screen, then alpha always has to be 1 because transparent means black background
					color.a = 1.0;
				}
				else
				{
					//Same as fog color
					color = _FogColor;
				}			

				return color;
			}



			fixed4 frag(v2f i) : SV_Target
			{
				//Transform the uv so they go from -1 to 1 and not 0 to 1, like a normal coordinate system, 
				//which begins at the center
				//float2 uv = i.uv * 2.0 - 1.0;

				//For full screen
				fixed2 uv = (i.uv - 0.5) * _FieldOfView;
				uv.x *= _AspectRatio;


				//Camera
				//Focal length from experimentation by positioning a sphere at the center and compare it with a real sphere
				//Should be 1 and not 0.62 when render full screen because we have already changed the fov above
				const fixed focalLength = 1.0;

				fixed3 ray = normalize(_CamUp * uv.y + _CamRight * uv.x + _CamForward * focalLength);

				//Where the ray starts
				float3 startPos = _CamPos;

				//Get the color at this pixel
				fixed4 color = getColor(startPos, ray);


				return color;
			}

			ENDCG
		}
	}
}
