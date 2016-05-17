using OpenTK;
using System;
using System.IO;
using template;

namespace Template {

class Game
{
	// member variables
	public Surface screen;
    public Camera camera;
	// initialize
	public void Init()
	{
        camera = new Camera(Vector3.Zero, Vector3.UnitZ);
        camera.UpdatePlane();
	}
	// tick: renders one frame
	public void Tick()
	{
		//screen.Clear( 0 );
		//screen.Print( "hello world", 2, 2, 0xffffff );
        
	}

    public void RenderGL()
    {
        camera.debugOutput();
    }
}

} // namespace Template