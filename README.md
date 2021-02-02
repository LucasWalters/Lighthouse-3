# Lighthouse-3

Created by Lucas Walters (5037883) and Kwinten Jacobs (7006233)

=======
List of features:
- BVH implementation of the teapot scene with ~6k triangles
- Teapot scene renders at least 6x faster with our binary BVH
- OBJ scene renders at least 4x faster with our binary BVH
- Binning over any number of split planes (currently set to 8) + optimal split plane selection using SAH
- 4-way BVH by collapsing the 2-way BVH (but with reduced performance)
- BVH generation of 1 million triangles in 8s
- Next event estimation for the path tracer
- Multiple importance sampling for (in)direct illumination
- Cosine weighted diffuse random reflections
- Russian roulette for path tracer rays

========
Important classes:
- Scene.cs : Contains the BVH and the root node construction.
- BVHNode.cs : Contains the BVH subdivide logic and the BVH collapse into 4-way BVH logic.
- AABB.cs : Contains the logic for bounding box calculations.
- PathTracer.cs : The new and improved path tracer (Kajya.cs is just a simple path tracer implementation for comparisons)
- StandardScenes.cs : Current scene can be selected here and the scenes can be changed here. Whether BVH is used can also be toggled here.

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
To change the screen size/aspect ratio, go to line 17 & 18 of game.cs
	(Note that a bug exists where high resolutions cause a grey horizontal line to appear sometimes)
To load in a different scene:
- Open StandardScenes.cs and go to line 12
- Choose between: 
   * public static Scene CURRENT_SCENE = TeapotScene(); //Teapot with ~6k trigs and BVH
   * public static Scene CURRENT_SCENE = KnightScene(); //Knight with 1M trigs and BVH
   * public static Scene CURRENT_SCENE = SimpleOBJScene(); //Tiny OBJ without BVH
   * public static Scene CURRENT_SCENE = WhittedScene();
   * public static Scene CURRENT_SCENE = KajiyaScene();

If anything is missing in one of these scenes, changes can be made in the scene descriptions listed in Scene.cs