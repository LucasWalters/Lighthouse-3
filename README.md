# Lighthouse-3

Created by Lucas Walters and Kwinten Jacobs
=======
List of features:
- Whitted style ray tracer
- Kajiya style ray tracer
- Direct illumination distance attenuation and light angle on diffuse surfaces
- A controllable camera with configurable position, orientation, FOV and aspect ratio (controls below)
- Planes, spheres and triangles
- Glossy, mirror, glass and 'regular' materials
- Multithreaded ray tracing
- Continually sending of rays each frame, not only on camera change
- Vignetting, barrel distortion and pincushion distortion
- Anti-aliasing
========
Important classes:
- Scene.cs : Contains all basic scenes (e.g Mirror scene, sphere scene).
- ObjectLoader.cs : Loads and triangulates .obj files. Note that only the colour of the material class and not the properties are loaded.
- Ray.cs : The logic for sending rays and finding intersections with primitives.
- Kajiya.cs : The logic for the Kayija style ray tracer
- Whitted.cs : The logic for the whitted style ray tracer
- Camera.cs : The logic for the camera and the multithreading.
- game.cs : Contains the logic for user input and where a scene is selected to be loaded in.
========
Controls:

W & S: Move camera up and down
A & D: Move camera sideways
Q & E: Zoom camera in & out
F & G: Increase and decrease FOV

Up & down arrows: Rotate camera up and down
Left & Right arrows: Rotate camera left and right
H & J: Show & hide the information box in the top left corner

=======
To load in a different scene:
- Open game.cs and go to line 36
- Choose between: 
   * scene = Scene.MirrorScene()
   * scene = Scene.SphereScene()

If anything is missing in one of these scenes, changes can be made in Scene.cs