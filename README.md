# Invested
Invested is a physics simulation and game made in the Unity engine. It is a personal interpretation and emulation of the physics of Allomancy, a magic system from the novel "Mistborn" by Brandon Sanderson.

Download the current build from [here.](https://www.dropbox.com/s/6o152qparaoede7/Invested.zip?dl=1)

Here are some short videos of the game:
- [Pushing and Pulling on metal targets](https://gfycat.com/PowerfulPaleAuk)
- [Pull targets vs. Push targets](https://gfycat.com/FoolishUnderstatedBackswimmer)
- [Some recent developments to the HUD](https://gfycat.com/ChubbySelfishBoutu)

This Unity project makes use of [Volumetric Lines by Johannes Unterguggenberger](https://assetstore.unity.com/packages/tools/particles-effects/volumetric-lines-29160) and [Cakeslice's Outline Effect](https://github.com/cakeslice/Outline-Effect).


Recent changes:

Version 1.1.0

- Added Glossary
- Added several new Interface options
- Added settings for disabling gravity and air resistance on the player
- Anchored Push Boosts are now estimated when targeting a metal but not actively pushing on it
	- Effectively, the forces displayed in the HUD assume that a target is perfectly anchored until you push on it.
- Added option to force the ANF to be equal for the Allomancer and target.
	- Whichever target or Allomancer is most anchored determines the ANF on both the target and Allomancer.
- Improved visual effect on text color of force labels in the HUD while pushing
- Changed target force labels to only show the two highest significant figures
- Increased friction acting on player when grounded and not trying to move
- Increased angle of throwing coins when airborne with mouse & keyboard control scheme
- Somewhat fixed bug where coins would occasionally fall through the ground when pushing on them out of a jump

Version 1.0.9

- Added HUD element: Mass, Force, and AF + ANF indicators that follow targets
- Added several physics options for calculation of Anchored Push Boosts
- Changed color of metal lines to Pull targets to be green
- Improved visual highlight of potential targets
- Improved menu navigation with the Escape key
- Improved Tutorial slightly
- Improved Sandbox slightly
- Swapped Jump and Throw-coin controls on gamepad
- Fixed several bugs with the calculation of the ANF
- Fixed visual bug with target metal lines when using gamepad
