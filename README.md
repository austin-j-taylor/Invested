# Invested
![](demoImage.png)
Invested is a physics simulation and game made in the Unity engine. It is a personal interpretation and emulation of the physics of Allomancy, a magic system from the novel "Mistborn" by Brandon Sanderson.

Download the current build from [here.](https://www.dropbox.com/s/6o152qparaoede7/Invested.zip?dl=1)

Here are some short videos of the game:
- ["Centrifuge" Pulling](https://gfycat.com/BlackandwhiteEllipticalAfricanaugurbuzzard)
- [Pushing and Pulling on metal targets](https://gfycat.com/PowerfulPaleAuk)
- [Pull targets vs. Push targets](https://gfycat.com/FoolishUnderstatedBackswimmer)
- [Some recent developments to the HUD](https://gfycat.com/ChubbySelfishBoutu)

This Unity project makes use of:
- [Volumetric Lines by Johannes Unterguggenberger](https://assetstore.unity.com/packages/tools/particles-effects/volumetric-lines-29160)
- [Cakeslice's Outline Effect](https://github.com/cakeslice/Outline-Effect)
- [The Heebo font](https://fonts.google.com/specimen/Heebo)


### Recent changes:

Version 1.1.5.1
- Added "Coinshot mode"
	- In Coinshot mode, throwing coins works more like a conventional first-person shooter. Pressing the "Pull" button while holding the "Push" button throws coins. By default, this is left-clicking while holding right click.
	- Press the 'C' key to toggle Coinshot mode on/off.
- Renamed Tutorial scene to Shooting Grounds
	- Added several stationary targets to Shooting Grounds
- Renamed Experimental scene to Southern Mountains

Version 1.1.5
- Implemented iron and steel reserves
	- Pushing/Pulling now consumes iron and steel
		- the mass of the iron/steel consumed is proportional to the force of the Push/Pull
		- passively burning iron/steel without Pushing/Pulling requires a slow burn of iron or steel to see the blue lines to metals
- Improved coin friction against large static objects
