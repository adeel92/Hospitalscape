Most sprites and UI images do not generate mipmaps, resulting in a jagged appearance when scaled down.
Crisp Sprite employs supersampling techniques to enhance image quality.
By making a simple material change, sprites and UI images can achieve a clean and sharp appearance.

Open Assets/CrispSprite/DemoScene/Demo1 to see how it works.
If you have any questions or suggestions, please contact willhongcom@gmail.com.

-------------------------------------------------------------------------
Basic setup:

Set sprite material to "CrispSprite-Sprites-Default" in Assets/CrispSprite/Materials/
or
Set UI Images material to "CrispSprite-UI-Default" in Assets/CrispSprite/Materials/

If you are using URP light sprites.
Set sprite material to "CrispSprite-URP-Sprite-Lit-Default" in Assets/CrispSprite/URP/Materials/

-------------------------------------------------------------------------

-------------------------------------------------------------------------
Reference

1. Sharper Mipmapping using Shader Based Supersampling
   https://bgolus.medium.com/sharper-mipmapping-using-shader-based-supersampling-ed7aadb47bec

-------------------------------------------------------------------------
Version History

1.0.0 Initial release.
1.0.1 Support URP light sprite.
1.0.2 Support Unity 2020 URP light sprite.