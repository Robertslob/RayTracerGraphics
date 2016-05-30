# RayTracerGraphics
Ari Saadon: 5495741
Robert Slob: 5496578 
Zino Onomiw: 4221095

Bonus:
* Refraction with Fresnel
* Skydome using HDR
* Basic OBJ support (triangles)
* Acceleration structure
* Textures on all

##Deadline Tuesday, May 31, 2016, 23:59h

For this assignment you will implement a Whitted-style ray tracer. This is a
recursive rendering algorithm for determining light transport between one or
more light sources and a camera, via scene surfaces, by tracing rays backwards
into the scene, starting at the camera.

A Whitted-style ray tracer requires a number of basic ingredients:
* A camera, representing the position and direction of the observer in the virtual world;
* A screen plane floating in front of the camera, which will be used to fire rays at;
* A number of primitives that will be intersected by the camera rays;
* A number of light sources that provide the energy that will be transported back to the
camera;
* A renderer that serves as the access point from the main application. The renderer
‘owns’ the camera and scene, generates the rays, intersects them with the scene,
determines the nearest intersection, and plots pixels based on calculated light
transport.

Optionally, we can define materials for the primitives. A material stores information about color
or texture, reflectivity, refractivity and absorption.

The remainder of this document describes the implementation of such a ray tracer. You are free
to ignore these steps and go straight to the required feature list. Note that the debug view is a
mandatory feature.

##Architecture
To start this project, create classes for the fundamental elements of the ray tracer:

**Camera**, with data members position and direction. The camera also stores the screen plane,
specified by its four corners, which are updated whenever camera position and/or direction is
modified. Hardcoded coordinates and directions allow for an easy start. Use e.g. (0,0,0) as the
camera origin, and (0,0,1) as the direction; this way the screen corners can also be hardcoded
for the time being. Once the basic setup works, you can make this more flexible.

**Primitive**, which encapsulates the ray/primitive intersection functionality. Two classes should
be derived from the base primitive: Sphere and Plane. A sphere is defined by a position and a
radius; a plane is defined by a normal and a distance to the origin. Initially (until you implement
materials) it may also be useful to add a color to the primitive class.

>Important: colors should be stored as floating point vectors. We will convert the final
transported light quantities to integer color as a final step; keeping everything in floats is
accurate, more natural, and easier.

**Light**, which stores the location and intensity of a light source. For a Whitted-style ray tracer,
this will be a point light. Intensity should be stored using float values for red, green and blue.
Scene, which stores a list of primitives and light sources. It implements a scene-level Intersect
method, which loops over the primitives and returns the closest intersection.
Intersection, which stores the result of an intersection. Apart from the intersection distance,
you will at least want to store the nearest primitive, but perhaps also the normal at the
intersection point.

**Raytracer**, which owns the scene, camera and the display surface. The Raytracer implements a
method Render, which uses the camera to loop over the pixels of the screen plane and to
generate a ray for each pixel, which is then used to find the nearest intersection. The result is
then visualized by plotting a pixel. For one line of pixels (typically line 256 for a 512x512
window), it generates debug output by visualizing every Nth ray (where N is e.g. 10).
Application, which calls the Render method of the Raytracer. The application is responsible for
handling keyboard and/or mouse input.

##First Steps - Details

With the basic structure of the application in place, it is time to implement the functionality. It
helps to get to something that produces as quickly as possible.

1. Prepare the scene.
A good scene to start with is a floor plane with three spheres on it. Keep everything within a
10x10x10 cube, and position the spheres so that the default camera can easily ‘see’ them. Make
sure their centers are at y=0.
2. Prepare the debug output.
Draw the scene to the debug output. Use a dot (or 2x2 dots) to visualize
the position of the camera. Use a line to visualize the screen plane. Draw
~100 line segments to approximate the spheres. Use the coordinate system
translation from the tutorial to get a view where camera and spheres fit in
the 512x512 debug window. Skip the plane in the visualization.
3. Generate primary rays.
Use the loop in the Raytracer.Render method to produce the primary rays. Note that you can
use a single ray here: it can be reused once the pixel color has been found. In the debug window,
draw a red line for the normalized ray direction. Verify that the generated rays form an arc with
radius 1. Verify that no rays miss the screen plane.
4. Intersect the scene.
Use the primary rays to intersect the scene. Now the full rays can be displayed in the debug
output. If you visualize rays for y=0 (i.e., line 256 of the 3D view), these rays should exactly end
at the sphere boundaries.
5. Visualize the intersections.

Once you have an intersection for a primary ray, you can use its data to
make up a pixel color. Plot a black pixel if the ray didn’t hit anything.
Otherwise, take the intersection distance, and scale it to a suitable range
(i.e., the scaled distances should be in the range 0..1). Now you can use
this value to plot a greyscale value. This should yield a ‘depth map’ of
your scene.

From here on you can slowly implement the remaining features, but you will never have to work
without visual feedback. E.g., normals can be visualized as little lines pointing away from
intersection points, and shadow rays can be colored based on whether they hit an obstruction
or not. Use the debug view extensively to verify your code.

##The Full Thing

To pass this assignment, implement the following features:

Camera:
* Your camera must support arbitrary field of view. It must be possible to set FOV from
the application, using an angle in degrees.
* The camera must support arbitrary positions and orientations. It must be possible to
aim the camera based on an arbitrary 3D position and target.

Primitives:
* Your ray tracer must at least support planes and spheres. The scene definition must be
flexible, i.e. your ray tracer must be able to handle arbitrary sets of planes and spheres.
Lights:
* Your ray tracer must be able to correctly handle an arbitrary number of point lights and
their shadows.
Materials:
* Your ray tracer must support at least diffuse materials and colored specular materials
(mirrors). The mirrors must be recursive, with an adjustable cap on recursion depth.
There must at least be texturing for a single plane (e.g., the floor plane) to make correct
reflections visible (and verifiable).
Application:
* The application must support keyboard and/or mouse handling to control the camera.
Debug output:
* The debug output must be implemented. It must at least show the primitives (except
for infinite planes), primary rays, shadow rays and secondary rays.

>Note that there is no performance requirement for this application.

##A Bit Extra

Meeting the minimum requirements earns you a 6 (assuming practical details are all in order).
An additional four points can be earned by implementing optional features. An incomplete list
of options, with an indication of the difficulty level:
* [EASY] Add triangle support (1 pt) with optional normal interpolation (1 pt)
* [EASY] Add spotlights (1 pt)
* [EASY] Add stochastic sampling of glossy reflections (1 pt)
* [EASY] Add anti-aliasing (1 pt)
* [MEDIUM] Add textures to all primitives (1 pt) with optional normal maps (1 pt)
* [MEDIUM] Add a textured skydome (1 pt), make it HDR for an additional 1 pt
* [MEDIUM] Add refraction (1 pt) and absorption (1 pt)
* [MEDIUM] Add stochastic sampling of area lights (1 pt)
* [HARD] Add an acceleration structure (2 pts)
* [HARD] Implement the ray tracer on a GPU (2 pt)
