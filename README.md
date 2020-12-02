# Lighthouse-3

Created by Lucas Walters (5037883) and Kwinten Jacobs (7006233)
=======
List of features:
- Whitted style ray tracer
- Kajiya style path tracer
- A controllable camera with configurable position, orientation, FOV and aspect ratio (controls below)
- Direct illumination with distance attenuation and angle on diffuse surfaces
- Planes, rectangles, spheres and triangles primitives
- (Glossy) mirror, glass, diffuse materials
- Dielectrics with fresnel factor
- Multithreaded ray tracing
- Continually sending of rays each frame and averaging the results
- Vignetting, barrel distortion and pincushion distortion
- Anti-aliasing, depth of field & Gamma correction
- OBJ loader for importing .obj files

========
Important classes:
- Scene.cs : Contains all basic scenes (e.g OBJScene, WhittedScene, KajiyaScene).
- ObjectLoader.cs : Loads and triangulates .obj files.
- Ray.cs : The logic for sending rays and finding intersections with primitives.
- Kajiya.cs : The logic for the Kayija style path tracer
- Whitted.cs : The logic for the whitted style ray tracer
- Camera.cs : The logic for the camera and the multithreading.
- game.cs : Contains the logic for user input and where a scene is selected to be loaded in.
========
Controls:

W & S: Move camera forwards and backwards
A & D: Move camera left and right
Q & E: Zoom camera up and down
F & G: Increase and decrease FOV

Up & down arrows: Rotate camera up and down
Left & Right arrows: Rotate camera left and right
J & H: Show & hide the information box in the top left corner

=======
To change the screen size/aspect ratio, go to line 16 & 17 of game.cs
	(Note that a bug exists where high resolutions cause a grey horizontal line to appear sometimes)
To load in a different scene:
- Open Scene.cs and go to line 22
- Choose between: 
   * public static Scene CURRENT_SCENE = OBJScene();
   * public static Scene CURRENT_SCENE = WhittedScene();
   * public static Scene CURRENT_SCENE = KajiyaScene();

If anything is missing in one of these scenes, changes can be made in the scene descriptions listed in Scene.cs