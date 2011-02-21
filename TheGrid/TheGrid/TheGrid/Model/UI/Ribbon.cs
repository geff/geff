﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TheGrid.Logic.UI;
using TheGrid.Common;
using Microsoft.Xna.Framework.Input;
using System.Windows.Forms;
using System.IO;

namespace TheGrid.Model.UI
{
    public class Ribbon : UIComponent
    {
        public const int MARGE = 5;
        public static int HEIGHT = 0;

        public Partition Partition;
        private ClickableImage imgPlay;
        private ClickableImage imgPause;
        private ClickableImage imgStop;
        private ClickableText txtNewMap;
        private ClickableText txtLoadMap;
        private ClickableText txtSaveMap;

        public Ribbon(UILogic uiLogic, TimeSpan creationTime)
            : base(uiLogic, creationTime)
        {
            CreationTime = creationTime;

            Visible = true;
            Alive = true;
            this.ListUIChildren = new List<UIComponent>();

            Ribbon.HEIGHT = (int)(0.2f * Render.ScreenHeight);
            Rec = new Rectangle(0, 0, (int)Render.ScreenWidth, Ribbon.HEIGHT);

            Partition = new Partition(this, uiLogic, GetNewTimeSpan());
            this.ListUIChildren.Add(Partition);

            imgPlay = new ClickableImage(this.UI, GetNewTimeSpan(), "Play", Render.texPlay, Render.texPlay, new Vector2(Partition.Rec.Right + 30 + MARGE * 2, Partition.Rec.Y));
            imgPause = new ClickableImage(this.UI, GetNewTimeSpan(), "Pause", Render.texPause, Render.texPause, new Vector2(Partition.Rec.Right + 30 + MARGE * 2, Partition.Rec.Y));
            imgPause.Visible = false;
            imgStop = new ClickableImage(this.UI, GetNewTimeSpan(), "Stop", Render.texStop, Render.texStop, new Vector2(Partition.Rec.Right + 30 + MARGE * 3 + Render.texPlay.Width, Partition.Rec.Y));

            imgPlay.ClickImage += new ClickableImage.ClickImageHandler(imgPlay_ClickImage);
            imgPause.ClickImage += new ClickableImage.ClickImageHandler(imgPause_ClickImage);
            imgStop.ClickImage += new ClickableImage.ClickImageHandler(imgStop_ClickImage);

            txtNewMap = new ClickableText(this.UI, GetNewTimeSpan(), "FontMenu", "New", new Vector2(imgStop.Rec.Right + MARGE, Partition.Rec.Y), VisualStyle.ForeColor, VisualStyle.ForeColor, VisualStyle.Transparent, VisualStyle.BackForeColorMouseOver, false);
            txtLoadMap = new ClickableText(this.UI, GetNewTimeSpan(), "FontMenu", "Load", new Vector2(imgStop.Rec.Right + MARGE, txtNewMap.Rec.Bottom + MARGE), VisualStyle.ForeColor, VisualStyle.ForeColor, VisualStyle.Transparent, VisualStyle.BackForeColorMouseOver, false);
            txtSaveMap = new ClickableText(this.UI, GetNewTimeSpan(), "FontMenu", "Save", new Vector2(imgStop.Rec.Right + MARGE, txtLoadMap.Rec.Bottom + MARGE), VisualStyle.ForeColor, VisualStyle.ForeColor, VisualStyle.Transparent, VisualStyle.BackForeColorMouseOver, false);

            txtNewMap.ClickText += new ClickableText.ClickTextHandler(txtNewMap_ClickText);
            txtLoadMap.ClickText += new ClickableText.ClickTextHandler(txtLoadMap_ClickText);
            txtSaveMap.ClickText += new ClickableText.ClickTextHandler(txtSaveMap_ClickText);

            this.ListUIChildren.Add(txtNewMap);
            this.ListUIChildren.Add(txtLoadMap);
            this.ListUIChildren.Add(txtSaveMap);

            this.ListUIChildren.Add(imgPlay);
            this.ListUIChildren.Add(imgPause);
            this.ListUIChildren.Add(imgStop);
        }

        void txtNewMap_ClickText(ClickableText clickableText, MouseState mouseState, GameTime gameTime)
        {
            ListLibrary listLibrary = new ListLibrary(this.UI, gameTime.TotalGameTime);
            this.ListUIChildren.Add(listLibrary);
        }

        void txtLoadMap_ClickText(ClickableText clickableText, MouseState mouseState, GameTime gameTime)
        {
            ListFile listFile = new ListFile(this.UI, gameTime.TotalGameTime, Path.Combine(Directory.GetParent(Application.ExecutablePath).FullName, @"Level\"));
            this.ListUIChildren.Add(listFile);
        }

        void txtSaveMap_ClickText(ClickableText clickableText, MouseState mouseState, GameTime gameTime)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.InitialDirectory = Path.Combine(Directory.GetParent(Application.ExecutablePath).FullName, @"Level\");
            dlg.Filter = "Niveau The Grid (*.xml)|*.xml";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                FileSystem.SaveLevel(Context.Map, Path.GetFileNameWithoutExtension(dlg.FileName));
            }
        }

        void imgPlay_ClickImage(ClickableImage image, MouseState mouseState, GameTime gameTime)
        {
            GamePlay.Play();
        }

        void imgPause_ClickImage(ClickableImage image, MouseState mouseState, GameTime gameTime)
        {
            GamePlay.Pause();
        }

        void imgStop_ClickImage(ClickableImage image, MouseState mouseState, GameTime gameTime)
        {
            GamePlay.Stop();
        }

        public override void Update(GameTime gameTime)
        {
            imgPlay.Visible = !Context.IsPlaying;
            imgPause.Visible = Context.IsPlaying;

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            Rectangle recManualSpeed = new Rectangle(Partition.Rec.Right + MARGE, Partition.Rec.Y, 30, (int)((float)Partition.Rec.Height * Context.Map.SpeedFactor / 2f));
            Rectangle rec = new Rectangle(Rec.X, Rec.Y, Rec.Width, (int)((float)Rec.Height * 1.3f));

            Render.SpriteBatch.Draw(Render.texEmptyGradient, rec, VisualStyle.BackColorDark);

            Render.SpriteBatch.Draw(Render.texEmptyGradient, recManualSpeed, Color.White);

            Render.SpriteBatch.DrawString(Render.FontTextSmall, Context.Map.BPM.ToString(), new Vector2(Partition.Rec.Right + MARGE, Partition.Rec.Y + MARGE), Color.DarkGray);

            base.Draw(gameTime);
        }
    }
}
