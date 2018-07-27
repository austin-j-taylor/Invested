# Invested
Invested is a physics simulation and game made in the Unity engine. It is a personal interpretation and emulation of the physics of Allomancy, a magic system from the novel "Mistborn" by Brandon Sanderson.

Download the current build from [here.](https://www.dropbox.com/s/j9jscf4xf8pf04i/Invested%201.0.4.zip?dl=1)

Here are some short videos of the game:
- [Pushing and Pulling on metal targets](https://gfycat.com/PowerfulPaleAuk)
- [Pull targets vs. Push targets](https://gfycat.com/FoolishUnderstatedBackswimmer)
- [Flying around the scene](https://gfycat.com/SpecificInferiorCaracal)

This Unity project makes use of [Volumetric Lines by Johannes Unterguggenberger](https://assetstore.unity.com/packages/tools/particles-effects/volumetric-lines-29160) and a modified shader from [Shrimpy's Outlined Diffuse Shader Fixed](https://github.com/Shrimpey/Outlined-Diffuse-Shader-Fixed).

Changelog:

--Version 1.0.5--

-Added option to enable/disable Allomantic Normal Force
-Added new Force-Distance Relationship: Exponential
-Altered Settings menu to distinguish Gameplay and Physics settings
-Added a few new blocks to the Tutorial
-Added a zero-grav zone to the Sandbox, just for fun.
-Added version number on title screen
-Fixed bug in which the ANF was calculated wrong when quickly pulling and pushing in sequence
	
--Version 1.0.4--

-Added Main menu, title screen, scene selection menu, and discrete settings menu
-Added basics for Tutorial, Sandbox, and Luthadel scenes
	-Added fog/mists to Luthadel

--Version 1.0.3--

-Added option to view force in User Interface in either Newtons or G's (acceleration)

--Version 1.0.2--

-Added new Push Control Style: Player can choose one of two methods to control their push:
	-by the percentage of the maximum possible force
	-by a constant target force magnitude.
-Added new Force-Distance Relationship: Player can choose one of two possible formulas for the Allomantic Force:
	-Inverse square relationship between Force and Distance
	-Linear relationship between Force and Distance
-Added option to control Allomantic Force Constant
-Added option to control Max Range

--Version 1.0.1--

-Fixed bug in which thrown coins were pushed above crosshairs