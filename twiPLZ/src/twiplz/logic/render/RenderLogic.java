package twiplz.logic.render;

import plz.engine.logic.controller.Pointer;
import plz.engine.logic.controller.PointerUsage;
import plz.engine.logic.ui.components.SensitiveZone;
import twiplz.Context;
import twiplz.GameEngine;
import twiplz.logic.gameplay.GamePlayLogic;
import twiplz.logic.ui.screens.GameScreen;
import twiplz.model.Cell;
import twiplz.model.CellPartType;

import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.Texture;
import com.badlogic.gdx.graphics.glutils.ShaderProgram;
import com.badlogic.gdx.math.Matrix4;
import com.badlogic.gdx.math.Vector2;

public class RenderLogic extends plz.engine.logic.render.RenderLogicBase
{
	Texture texCellForeground;
	Texture texCellBackground;
	Texture texCircle;
	Texture[] texArrowsIn = new Texture[6];
	Texture[] texArrowsOut = new Texture[6];
	Color[] colors;
	ShaderProgram shader;
	
	public Vector2[] PointToDraw = new Vector2[5];

	private boolean showCursor = false;
	private boolean showColor = true;

	private GamePlayLogic GamePlay()
	{
		return (GamePlayLogic) gameEngine.GamePlay;
	}

	private GameScreen GameScreen()
	{
		return (GameScreen) gameEngine.CurrentScreen;
	}

	public RenderLogic(GameEngine gameEngine)
	{
		super(gameEngine);

		LoadTextures();
	}

	private void LoadTextures()
	{
		texCellForeground = new Texture(Gdx.files.internal("data/CellForeground.png"));
		texCellBackground = new Texture(Gdx.files.internal("data/CellBackground.png"));
		texCircle = new Texture(Gdx.files.internal("data/Circle.png"));

//		shader =  new ShaderProgram(Gdx.files.internal("data/shaders/batch.vert").readString(), Gdx.files.internal("data/shaders/batch.frag").readString());
		
		for (int i = 1; i <= 6; i++)
		{
			texArrowsIn[i - 1] = new Texture(Gdx.files.internal("data/ArrowIn" + i + ".png"));
			texArrowsOut[i - 1] = new Texture(Gdx.files.internal("data/ArrowOut" + i + ".png"));
		}

		colors = new Color[7];
		colors[0] = new Color(1f, 0.7f, 0.84f, 1f);
		colors[1] = new Color(0.78f, 0.7f, 1f, 1f);
		colors[2] = new Color(0.68f, 0.90f, 1f, 1f);
		colors[3] = new Color(0.68f, 1f, 0.7f, 1f);
		colors[4] = new Color(0.95f, 1f, 0.66f, 1f);
		colors[5] = new Color(1f, 0.79f, 0.68f, 1f);
		colors[6] = Color.WHITE;

		if (Context.Mini)
		{
			for (int i = 0; i < colors.length; i++)
			{
				colors[i].mul(0.25f);
			}
		}
	}

	@Override
	public void Render(float deltaTime)
	{
		super.Render(deltaTime);

		if (Context.Mini)
			spriteBatch.setColor(Color.BLACK);
		else
			spriteBatch.setColor(Color.WHITE);

		spriteBatch.begin();

		if (showColor)
		{
			// --- Cellules de la map
			for (Cell cell : Context.Map.Cells)
			{
				// if (!cell.Selected)
				DrawCell(cell, false, false);
			}
			// ---
		}

		// --- Tuile sélectionnée
		if (GamePlay().SelectedTile != null)
		{
			DrawCell(GamePlay().SelectedTile.Cells[0], false, true);
			DrawCell(GamePlay().SelectedTile.Cells[1], false, true);

			// spriteBatch.setColor(Color.RED);

			// spriteBatch.draw(texCellForeground,
			// GamePlay().SelectedTile.Location.x - 32,
			// -(GamePlay().SelectedTile.Location.y - 32), 64, 64);
		}
		// ---

		spriteBatch.end();
	}

	@Override
	public void RenderUI(float deltaTime)
	{
		ProjectUI();

		gameEngine.CurrentScreen.render(deltaTime);

		// --- Bouton 'Nouvelle tuile'
		//spriteBatch.setShader(shader);

	
		spriteBatch.begin();
//shader.begin();
		
		DrawCell(GamePlay().Tile.Cells[0], true, false);
		DrawCell(GamePlay().Tile.Cells[1], true, false);

		if (showCursor)
		{

			for (Pointer pointer : Context.pointers)
			{
				if (pointer.Current != null)
				{
					if (pointer.Usage == PointerUsage.SelectTile)
						spriteBatch.setColor(Color.GREEN);
					else if (pointer.Usage == PointerUsage.ButtonTurnTile)
						spriteBatch.setColor(Color.RED);
					else
						spriteBatch.setColor(Color.BLUE);

					spriteBatch.draw(texCellForeground, pointer.Current.x - 20, Gdx.graphics.getHeight() - (pointer.Current.y - 20), 40, 40);
				}
			}
		}
		
		//shader.end();

		spriteBatch.end();
		// ---
		
		//spriteBatch.setShader(null);
	}

	private void DrawCell(Cell cell, boolean isUI, boolean isSelectedCell)
	{
		Vector2 cellLocation = cell.Location.cpy();

		int width = texCellBackground.getWidth();
		int height = texCellBackground.getHeight();

		if (isUI)
		{
			SensitiveZone imgNewTile = ((GameScreen) this.gameEngine.CurrentScreen).imgNewTile;

			height = (int) (imgNewTile.height / 2);
			width = (int) ((2 * height) / Math.sqrt(3f));
		}
		else
		{
			cellLocation.mul(256f);
		}

		if (isSelectedCell)
		{
			if(Context.Mini)
				spriteBatch.setColor(0.3f,0.3f,0.3f, 1f);
			else
				spriteBatch.setColor(Color.GREEN);
		}
		else
			spriteBatch.setColor(Color.WHITE);

		spriteBatch.draw(texCellBackground, cellLocation.x, cellLocation.y, width, height);

		if (!cell.IsEmpty)
		{
			spriteBatch.setColor(colors[cell.ColorType - 1]);
			spriteBatch.draw(texCellForeground, cellLocation.x, cellLocation.y, width, height);
		}

		if (isSelectedCell)
		{
			if (GamePlay().SelectedTile.ActiveCell == cell)
			{
				spriteBatch.setColor(Color.BLACK);
				spriteBatch.draw(texCircle, cellLocation.x - width/4, cellLocation.y - height/4, (int)((float)width*1.5f), (int)((float)height*1.5));
			}
		}

		if (Context.Mini)
			spriteBatch.setColor(Color.BLACK);
		else
			spriteBatch.setColor(Color.WHITE);

		for (int i = 0; i < 6; i++)
		{
			Texture texturePart = null;

			if (cell.Parts[i] == CellPartType.Out)
			{
				texturePart = texArrowsOut[i];
			}
			else if (cell.Parts[i] == CellPartType.In)
			{
				texturePart = texArrowsIn[i];
			}

			if (texturePart != null)
			{
				spriteBatch.draw(texturePart, cellLocation.x, cellLocation.y, width, height);
			}
		}
	}
}
