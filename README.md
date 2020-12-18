# Lighthouse-3
(Note: Name can be changed if requested)

Created by Lucas Walters (5037883) and Kwinten Jacobs (7006233)

=======
List of features:
- BVH implementation of the teapot scene with >10k triangles
- Teapot scene renders at least 6x faster with our binary BVH
- OBJ scene renders at least 4x faster with our binary BVH
- Binning over 8 split planes + optimal split plane selection using SAH
- 4-way BVH by collapsing the 2-way BVH (but with reduced performance)
- BVH generation of 1 million triangles in 8s

========
Important classes:
- Scene.cs : Contains the BVH and the root node construction. On line 51, uncommenting the line enables 4-way BVH generation.
- BVHNode.cs : Contains the BVH subdivide logic and the BVH collapse into 4-way BVH logic
- AABB.cs : Contains the logic for bounding box calculation

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