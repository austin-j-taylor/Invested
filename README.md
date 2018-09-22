# Invested
![](demoImage.png)
Invested is a physics simulation and game made in the Unity engine. It is a personal interpretation and emulation of the physics of Allomancy, a magic system from the novel "Mistborn" by Brandon Sanderson.

Download the current build from [here.](https://www.dropbox.com/s/6o152qparaoede7/Invested.zip?dl=1)

Here are some short videos of the game:
- [Pushing and Pulling on metal targets](https://gfycat.com/PowerfulPaleAuk)
- [Pull targets vs. Push targets](https://gfycat.com/FoolishUnderstatedBackswimmer)
- [Some recent developments to the HUD](https://gfycat.com/ChubbySelfishBoutu)

This Unity project makes use of:
	- [Volumetric Lines by Johannes Unterguggenberger](https://assetstore.unity.com/packages/tools/particles-effects/volumetric-lines-29160)
	- [Cakeslice's Outline Effect](https://github.com/cakeslice/Outline-Effect)
	- [The Heebo font](https://fonts.google.com/specimen/Heebo)


### Recent changes:

Version 1.1.3

- Settings are now saved to disk
	- Added separate settings for Mouse Sensitivity and Gamepad Sensitivity
	- Added setting to disable/enable FPS counter
- Improvements to Magnitude Push Control Style
	- Magnitude can now be controlled with precision from 0N to 100N
	- Works better when targeting multiple metal targets
	- Burn Rate Meter updates more smoothly
	- Works better with both ANF and EWV Anchored Push Boosts
- Fixed bugs in the calculation of the ANF
- Fixed bug with friction on coins when trying to push and pull on them simultaneously
- Added something special to the Experimental map

Version 1.1.2

- Changed how coins are thrown
	- When holding down Jump key: coins are thrown beneath you, slightly opposing your directional movement
	- When not holding down Jump key: coins are thrown towards crosshair
- Coins have more realistic friction when pushed against a surface
- Improved camera
	- Fixed bug where camera moves strangely when player is against a wall
	- Added option for removing vertical rotation limits on camera (can now vertically wrap around freely)
- When pushing on multiple targets, your individual pushes on each target are no longer decreased
	- For example: previously, if you had 3 push targets, your push on each was divided by 3. This kept the net force on you from increasing with multiple targets. Now, no alterations are made to each push.
- Burn Rate can now be controlled with precision from 0% to 10%
- Fixed bug with Magnitude Push Control Style when targeting a force of 0.
- Added Experimental scene
