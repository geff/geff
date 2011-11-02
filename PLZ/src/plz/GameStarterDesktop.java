package plz;

import plz.logic.controller.twiplz.SelectionMode;

import com.badlogic.gdx.ApplicationListener;
import com.badlogic.gdx.backends.jogl.JoglApplication;

public class GameStarterDesktop
{
	public static void main(String[] argv)
	{
		boolean miniWindow = true;
		
		GameEngine app = new GameEngine();
		JoglApplication window;

		if(miniWindow)
		{
			window = new JoglApplication(app, "plz", 400, 400, false);
			window.getJFrame().setLocation(2100, 550);
		}
		else
		{
			window = new JoglApplication(app, "plz", 1024, 600, false);
		}
	}
}