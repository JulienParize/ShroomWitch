// Shader des fleurs du champ procédural (composant FlowerField).
// - La couleur vient des sommets du maillage : pas besoin de texture.
// - Le vent plie chaque fleur autour de son pied, directement sur la carte graphique.
// Les réglages du vent sont envoyés par FlowerField : inutile de créer un matériau à la main.
Shader "ShroomWitch/FlowerWind"
{
    Properties
    {
        [Header(Vent)]
        _WindDirection ("Direction du vent (X et Z)", Vector) = (0.5, 0, 0.866, 0)
        _WindStrength ("Force du vent", Range(0, 2)) = 0.6
        _WindSpeed ("Vitesse des rafales (m/s)", Float) = 3
        _GustFrequency ("Frequence des rafales (1 / taille en m)", Float) = 0.167
        _FlutterStrength ("Fremissement", Range(0, 1)) = 0.2
        _FlutterSpeed ("Vitesse du fremissement", Float) = 4
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        // Pétales et feuilles sont de simples triangles sans épaisseur : on dessine leurs deux faces.
        Cull Off

        // Code commun à toutes les passes (couleur, ombre, profondeur) : le vent doit être identique partout.
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        // Inclinaison maximale d'une fleur, en radians (environ 70 degrés).
        #define FLOWER_MAX_BEND 1.2

        CBUFFER_START(UnityPerMaterial)
            float4 _WindDirection;
            float _WindStrength;
            float _WindSpeed;
            float _GustFrequency;
            float _FlutterStrength;
            float _FlutterSpeed;
        CBUFFER_END

        struct FlowerAttributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            half4 color : COLOR;
            // xyz = pied de la fleur (espace objet), w = nombre au hasard propre à la fleur (0 à 1).
            float4 pivotAndRandom : TEXCOORD0;
        };

        struct FlowerVertex
        {
            float3 positionWS;
            float3 normalWS;
        };

        // Nombre pseudo-aléatoire entre 0 et 1 pour une case de grille.
        float FlowerHash(float2 cell)
        {
            float3 p = frac(float3(cell.x, cell.y, cell.x) * float3(0.1031, 0.1030, 0.0973));
            p += dot(p, p.yzx + 33.33);
            return frac((p.x + p.y) * p.z);
        }

        // Bruit lisse entre 0 et 1 : dessine les "nappes" de vent qui traversent le champ.
        float FlowerNoise(float2 position)
        {
            float2 cell = floor(position);
            float2 blend = frac(position);
            blend = blend * blend * (3.0 - 2.0 * blend);

            float a = FlowerHash(cell);
            float b = FlowerHash(cell + float2(1.0, 0.0));
            float c = FlowerHash(cell + float2(0.0, 1.0));
            float d = FlowerHash(cell + float2(1.0, 1.0));
            return lerp(lerp(a, b, blend.x), lerp(c, d, blend.x), blend.y);
        }

        // Flexion d'une fleur : le vecteur donne la direction (x, z) où elle se couche,
        // sa longueur donne l'inclinaison en radians par mètre de hauteur.
        float2 FlowerWindBend(float3 pivotWS, float random01)
        {
            float2 windDir = _WindDirection.xz;
            windDir /= max(length(windDir), 0.0001);
            float2 sideDir = float2(-windDir.y, windDir.x);
            float time = _Time.y;

            // Rafales : un bruit qui glisse sur le champ dans le sens du vent.
            float2 gustPosition = (pivotWS.xz - windDir * (time * _WindSpeed)) * _GustFrequency;
            float gust = FlowerNoise(gustPosition) * 0.65 + FlowerNoise(gustPosition * 2.3 + 31.7) * 0.35;

            // Frémissement : chaque fleur oscille à son propre rythme, y compris un peu de côté.
            float phase = random01 * 6.2831853;
            float rhythm = _FlutterSpeed * (0.75 + 0.5 * random01);
            float flutterAlong = sin(time * rhythm + phase);
            float flutterSide = sin(time * rhythm * 0.71 + phase * 1.7);

            float along = lerp(0.15, 1.0, gust) + flutterAlong * _FlutterStrength;
            float side = flutterSide * _FlutterStrength;

            // Certaines fleurs sont plus souples que d'autres.
            float suppleness = lerp(0.75, 1.25, frac(random01 * 7.13));

            return (windDir * along + sideDir * side) * (_WindStrength * suppleness);
        }

        // Fait tourner un vecteur autour d'un axe (sinus et cosinus de l'angle déjà calculés).
        float3 FlowerRotate(float3 value, float3 axis, float sinAngle, float cosAngle)
        {
            return value * cosAngle + cross(axis, value) * sinAngle + axis * (dot(axis, value) * (1.0 - cosAngle));
        }

        // Plie la fleur autour de son pied : plus un sommet est haut, plus il tourne.
        // Une rotation (et non un simple décalage) garde la longueur de la tige et incline aussi la tête.
        void ApplyFlowerWind(inout float3 positionWS, inout float3 normalWS, float3 pivotWS, float random01)
        {
            float2 bend = FlowerWindBend(pivotWS, random01);
            float strength = length(bend);
            float2 bendDir = bend / max(strength, 0.0001);

            float height = max(positionWS.y - pivotWS.y, 0.0);
            float angle = min(strength * height, FLOWER_MAX_BEND);

            // Axe horizontal, perpendiculaire à la direction où la fleur se couche.
            float3 axis = float3(bendDir.y, 0.0, -bendDir.x);
            float sinAngle, cosAngle;
            sincos(angle, sinAngle, cosAngle);

            positionWS = pivotWS + FlowerRotate(positionWS - pivotWS, axis, sinAngle, cosAngle);
            normalWS = FlowerRotate(normalWS, axis, sinAngle, cosAngle);
        }

        // Position et normale du sommet dans le monde, vent compris.
        FlowerVertex GetFlowerVertex(FlowerAttributes input)
        {
            FlowerVertex vertex;
            vertex.positionWS = TransformObjectToWorld(input.positionOS.xyz);
            vertex.normalWS = TransformObjectToWorldNormal(input.normalOS);

            float3 pivotWS = TransformObjectToWorld(input.pivotAndRandom.xyz);
            ApplyFlowerWind(vertex.positionWS, vertex.normalWS, pivotWS, input.pivotAndRandom.w);
            return vertex;
        }

        // Les couleurs des sommets sont enregistrées "comme à l'écran" (sRGB) ;
        // l'éclairage, lui, se calcule en linéaire. Formule de conversion approchée.
        half3 FlowerVertexColor(half3 color)
        {
        #ifdef UNITY_COLORSPACE_GAMMA
            return color;
        #else
            return color * (color * (color * 0.305306011 + 0.682171111) + 0.012522878);
        #endif
        }
        ENDHLSL

        // ---------------------------------------------------------------
        // Passe principale : couleur, lumière du soleil, ombres reçues, brouillard.
        // ---------------------------------------------------------------
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForwardOnly" }

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex ForwardVertex
            #pragma fragment ForwardFragment

            // Variantes compilées selon les réglages d'ombres et de brouillard du projet.
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct ForwardVaryings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                half3 color : TEXCOORD2;
                half fogFactor : TEXCOORD3;
            };

            ForwardVaryings ForwardVertex(FlowerAttributes input)
            {
                FlowerVertex vertex = GetFlowerVertex(input);

                ForwardVaryings output;
                output.positionCS = TransformWorldToHClip(vertex.positionWS);
                output.positionWS = vertex.positionWS;
                output.normalWS = vertex.normalWS;
                output.color = FlowerVertexColor(input.color.rgb);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 ForwardFragment(ForwardVaryings input, FRONT_FACE_TYPE isFrontFace : FRONT_FACE_SEMANTIC) : SV_Target
            {
                // Dos d'un pétale : on retourne la normale pour qu'il soit éclairé correctement.
                half3 normalWS = normalize(input.normalWS);
                normalWS = IS_FRONT_VFACE(isFrontFace, normalWS, -normalWS);

                // Soleil (lumière principale) avec son ombre.
                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord, input.positionWS, half4(1.0, 1.0, 1.0, 1.0));
                half diffuse = saturate(dot(normalWS, mainLight.direction));
                half3 lighting = mainLight.color * (diffuse * mainLight.distanceAttenuation * mainLight.shadowAttenuation);

                // Lumière ambiante du ciel.
                lighting += SampleSH(normalWS);

                half3 color = MixFog(input.color * lighting, input.fogFactor);
                return half4(color, 1.0);
            }
            ENDHLSL
        }

        // ---------------------------------------------------------------
        // Ombre projetée par les fleurs (elle suit le vent).
        // ---------------------------------------------------------------
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex ShadowVertex
            #pragma fragment ShadowFragment
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            // Renseignés par URP : direction du soleil, ou position d'une lampe (point, spot).
            float3 _LightDirection;
            float3 _LightPosition;

            struct ShadowVaryings
            {
                float4 positionCS : SV_POSITION;
            };

            ShadowVaryings ShadowVertex(FlowerAttributes input)
            {
                FlowerVertex vertex = GetFlowerVertex(input);

            #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 lightDirectionWS = normalize(_LightPosition - vertex.positionWS);
            #else
                float3 lightDirectionWS = _LightDirection;
            #endif

                ShadowVaryings output;
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(vertex.positionWS, vertex.normalWS, lightDirectionWS));
                output.positionCS = ApplyShadowClamping(positionCS);
                return output;
            }

            half4 ShadowFragment(ShadowVaryings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        // ---------------------------------------------------------------
        // Profondeur seule (utilisée par URP avant le rendu principal).
        // ---------------------------------------------------------------
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthVertex
            #pragma fragment DepthFragment

            struct DepthVaryings
            {
                float4 positionCS : SV_POSITION;
            };

            DepthVaryings DepthVertex(FlowerAttributes input)
            {
                FlowerVertex vertex = GetFlowerVertex(input);

                DepthVaryings output;
                output.positionCS = TransformWorldToHClip(vertex.positionWS);
                return output;
            }

            half DepthFragment(DepthVaryings input) : SV_Target
            {
                return input.positionCS.z;
            }
            ENDHLSL
        }

        // ---------------------------------------------------------------
        // Profondeur + normales (utilisée par l'occlusion ambiante, SSAO).
        // ---------------------------------------------------------------
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }

            ZWrite On

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            struct DepthNormalsVaryings
            {
                float4 positionCS : SV_POSITION;
                half3 normalWS : TEXCOORD0;
            };

            DepthNormalsVaryings DepthNormalsVertex(FlowerAttributes input)
            {
                FlowerVertex vertex = GetFlowerVertex(input);

                DepthNormalsVaryings output;
                output.positionCS = TransformWorldToHClip(vertex.positionWS);
                output.normalWS = vertex.normalWS;
                return output;
            }

            half4 DepthNormalsFragment(DepthNormalsVaryings input, FRONT_FACE_TYPE isFrontFace : FRONT_FACE_SEMANTIC) : SV_Target
            {
                half3 normalWS = normalize(input.normalWS);
                normalWS = IS_FRONT_VFACE(isFrontFace, normalWS, -normalWS);
                return half4(normalWS, 0.0);
            }
            ENDHLSL
        }
    }
}
