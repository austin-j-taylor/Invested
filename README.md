# Invested
![](demoImage.png)
Invested is a physics simulation and game made in the Unity engine. It is a personal interpretation and emulation of the physics of Allomancy, a magic system from the novel "Mistborn" by Brandon Sanderson.

Download the current build from [here.](https://www.dropbox.com/s/6o152qparaoede7/Invested.zip?dl=1)

Here are some short videos of the game:
- [Pushing and Pulling on metal targets](https://gfycat.com/PowerfulPaleAuk)
- [Pull targets vs. Push targets](https://gfycat.com/FoolishUnderstatedBackswimmer)
- [Some recent developments to the HUD](https://gfycat.com/ChubbySelfishBoutu)

This Unity project makes use of [Volumetric Lines by Johannes Unterguggenberger](https://assetstore.unity.com/packages/tools/particles-effects/volumetric-lines-29160) and [Cakeslice's Outline Effect](https://github.com/cakeslice/Outline-Effect).


### Recent changes:

Version 1.1.2

- Changed how coins are thrown
	- When holding down Jump key: coins are thrown beneath you, slightly opposing your directional movement
	- When not holding down Jump key: coins are thrown towards crosshair
- Coins have more realistic friction when pushed against a surface
- Improved camera
	- Fixed bug where camera moves strangely when player is against a wall
	- Added option for removing vertical rotations limits on camera (can now vertically wrap around freely)
- When pushing on multiple targets, your individual pushes on each target are no longer decreased
	- For example: previously, if you had 3 push targets, your push on each was divided by 3. This kept the net force on you from increasing with multiple targets. Now, no alterations are made to each push.
- Burn Rate can now be controlled with precision from 0% to 10%
- Fixed bug with Magnitude Push Control Style when targeting a force of 0.\
- Added Experimental scene

Version 1.1.1

- Improved blue metal lines
	- More massive targets can be detected and targeted from farther away than less massive targets
	- Lines are thinner for less visual clutter
	- The higher the burn rate, the greater the range for detecting targets
- Improved camera
	- No longer clips through walls
	- Added option for first person perspective
- Coins no longer clip through the ground
	- Coins also return to the player's coin pouch much more smoothly when pulled
- Scrolling to a burn rate of 0% now extinguishes metals
- Setting the Force Display Units to be G's now scales the acceleration with the target's mass, not the player's
- Performance improvements
	- Added option to disable rendering of blue lines for lower-end computers
	- Blue lines are now a bit less intensive to calculate
- Increased default Allomantic Constant
