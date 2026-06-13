#ifndef CRISPSPRITE_SPRITE_INCLUDED
#define CRISPSPRITE_SPRITE_INCLUDED

#include "CrispSprite.cginc"

fixed4 SampleSpriteTexture_CS(float2 uv)
{
    fixed4 color = tex2D_CS(_MainTex, uv);

#if ETC1_EXTERNAL_ALPHA
    fixed4 alpha = tex2D_CS(_AlphaTex, uv);
    color.a = lerp(color.a, alpha.r, _EnableExternalAlpha);
#endif

    return color;
}

#endif // CRISPSPRITE_SPRITE_INCLUDED