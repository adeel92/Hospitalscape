#ifndef CRISPSPRITE_URP_INCLUDED
#define CRISPSPRITE_URP_INCLUDED

#ifdef SAMPLE_TEXTURE2D

half4 SAMPLE_TEXTURE2D_CS(Texture2D tex, SamplerState samp, float2 uv)
{			
	float2 dx = ddx(uv);
	float2 dy = ddy(uv);				
	float2 offset = float2(0.125, 0.375);		
	
	half4 c = SAMPLE_TEXTURE2D(tex, samp, uv + dx * offset.x + dy * offset.y);
	c += SAMPLE_TEXTURE2D(tex, samp, uv - dx * offset.x - dy * offset.y);
	c += SAMPLE_TEXTURE2D(tex, samp, uv + dx * offset.y - dy * offset.x);
	c += SAMPLE_TEXTURE2D(tex, samp, uv - dx * offset.y + dy * offset.x);
	c *= 0.25;
	return c;
}

#endif

#endif // CRISPSPRITE_URP_INCLUDED