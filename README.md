# Invested
![](demoImage.png)
Invested is a physics simulation and game made in the Unity engine. It is a personal interpretation and emulation of the physics of Allomancy, a magic system from the novel "Mistborn" by Brandon Sanderson.

Download the current build from [here.](https://www.dropbox.com/s/6o152qparaoede7/Invested.zip?dl=1) (Windows). Let me know if there's a need for other operating systems.

Here are some videos of the game:
- [Overview](https://gfycat.com/boldenchantedamethystgemclam)
- [Traversing Luthadel](https://gfycat.com/insecuredifficultdodo)
- [Aerial combat](https://gfycat.com/soupyelegantlark)
- [Throwing coins](https://gfycat.com/complicatedregularangelwingmussel)

This Unity project makes use of:
- [Volumetric Lines by Johannes Unterguggenberger](https://assetstore.unity.com/packages/tools/particles-effects/volumetric-lines-29160)
- [Cakeslice's Outline Effect](https://github.com/cakeslice/Outline-Effect)
- [The Heebo font](https://fonts.google.com/specimen/Heebo)


### Recent changes:

Version 1.2 (May 2019)
- Added Tutorial
- Expanded Sandbox
- Added player model
- Added Pewter
	- Sprinting and Pewter Jumping/wall jumping
- Changed controls
	- Full gamepad (XBox 360 controller) support for menu navigation and playability in general
	- Added control to move slowly/anchor self
	- Player moves more like a ball should (improved rolling physics)
- Camera improvements
	- Added options for inverting camera (horizontally and vertically)
	- Added option for changing camera distance from player
	- Much less likely to clip through walls
- HUD improvements
	- Added Help Overlay to show controls while playing
	- Burn Percentage Meter now differentiates between Iron and Steel burn percentage (only affects for gamepad users)
	- Added option to hide HUD
- Allomancy changes
	- Added experimental "Distributed Power" Anchored Push Boost.
	- Changed "Exponential w/ Velocity" Anchored Push Boost.
		- Clarified terminology used to describe how velocity affects each subtype of EwV used.
		- EwV is now the default Allomancy model
	- Player model now glows when metals are burned
	- Walls partially interfere with ironsight
- Simulations are now playable through the scene select menu
- Added settings for "inverted" gravity and changing the time scale