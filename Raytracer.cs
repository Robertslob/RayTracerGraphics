using OpenTK;
using System;
using System.IO;

namespace Application {

    //owns the scene, camera and surface. has the render method which is called by the application every frame
    class Raytracer
    {
	    public Surface screen;
        public Camera camera;
        public Scene scene;

	    public Camera Init()
	    {
            camera = new Camera(Vector3.Zero, Vector3.UnitZ);
            camera.UpdatePlane();
            return camera;
	    }

	    // this guy is called once every frame
	    public void Render()
	    {	
            //use camera to display stuff on the screen
	    } 
    }

} 