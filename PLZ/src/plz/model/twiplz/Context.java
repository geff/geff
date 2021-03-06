package plz.model.twiplz;

import plz.engine.ContextBase;
import plz.engine.logic.controller.Pointer;
import plz.logic.controller.twiplz.SelectionMode;
import plz.model.*;

public class Context extends ContextBase
{
	public Map Map;
	public int selectionOffsetY = 0;
	public SelectionMode selectionMode = SelectionMode.Screentouch;
	public GameMode gameMode = GameMode.Circular;
	public int Score = 0;
	public int AddedScore = 0;
	public int Combo = 0;
	public GameStateTime gameStateTime;
}
