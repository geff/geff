﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TheGrid.Logic.UI;
using Microsoft.Xna.Framework.Graphics;
using TheGrid.Common;
using Microsoft.Xna.Framework.Input;
using TheGrid.Model.Instrument;
using TheGrid.Logic.Controller;

namespace TheGrid.Model.UI
{
    public class Partition : UIComponent
    {
        private Ribbon ribbon;
        private List<float> listTimeSegment;
        private Color colorTimeSegment = new Color(0.3f, 0.3f, 0.3f, 0.4f);
        private float prevPartitionRatio = 0f;
        private TimeSpan prevTime = TimeSpan.Zero;
        private float measureBarWidth = 5f;
        private float timelineDuration = 1000f * 10;
        private float timeWidth = 20f;
        private Rectangle recPartition;

        public Partition(Ribbon ribbon, UILogic uiLogic, TimeSpan creationTime)
            : base(uiLogic, creationTime)
        {
            this.ribbon = ribbon;
            Visible = true;
            Alive = true;

            Rec = new Rectangle(Ribbon.MARGE * 2, Ribbon.MARGE * 2, (int)(0.7f * Render.ScreenWidth), ribbon.Rec.Height - Ribbon.MARGE * 3);

            recPartition = new Rectangle(Rec.X + Ribbon.MARGE + 100, Rec.Y, Rec.Width - 100 - Ribbon.MARGE, Rec.Height);

            MouseManager mouseLeftButton = AddMouse(MouseButtons.LeftButton);
            mouseLeftButton.MouseFirstPressed += new MouseManager.MouseFirstPressedHandler(mouseLeftButton_MouseFirstPressed);
            mouseLeftButton.MousePressed += new MouseManager.MousePressedHandler(mouseLeftButton_MousePressed);
            mouseLeftButton.MouseReleased += new MouseManager.MouseReleasedHandler(mouseLeftButton_MouseReleased);
        }

        void mouseLeftButton_MouseReleased(MouseButtons mouseButton, MouseState mouseState, GameTime gameTime, Point distance)
        {
            prevPartitionRatio = -1f;
        }

        void mouseLeftButton_MouseFirstPressed(MouseButtons mouseButton, MouseState mouseState, GameTime gameTime)
        {
            if (recPartition.Contains(UI.GameEngine.Controller.mousePositionPoint))
            {
                prevPartitionRatio = Context.PartitionRatio;
                prevTime = Context.Time;
            }
        }

        void mouseLeftButton_MousePressed(MouseButtons mouseButton, MouseState mouseState, GameTime gameTime, Point distance)
        {
            if (prevPartitionRatio > -1f)
            {
                if (UI.GameEngine.Controller.keyBoardState.IsKeyDown(Keys.LeftControl))
                {
                    Context.PartitionRatio = prevPartitionRatio + (float)distance.X / 200f;

                    Context.PartitionRatio = MathHelper.Clamp(Context.PartitionRatio, 0.05f, 10f);

                    this.InitSegment();
                }
                else
                {
                    Context.Time = prevTime + TimeSpan.FromMilliseconds(timelineDuration / (float)recPartition.Width * (float)distance.X);

                    if (Context.Time < TimeSpan.Zero)
                        Context.Time = TimeSpan.Zero;
                    if (Context.Time > Context.Map.PartitionDuration)
                        Context.Time = Context.Map.PartitionDuration;
                }

                if (mouseState.X > recPartition.Right)
                {
                    Mouse.SetPosition(recPartition.Left, mouseState.Y);

                    if (UI.GameEngine.Controller.keyBoardState.IsKeyDown(Keys.LeftControl))
                        prevPartitionRatio -= recPartition.Width/200f;
                    else
                        prevTime = prevTime.Subtract(TimeSpan.FromMilliseconds(timelineDuration));
                }
                else if (mouseState.X < recPartition.Left)
                {
                    Mouse.SetPosition(recPartition.Right, mouseState.Y);

                    if (UI.GameEngine.Controller.keyBoardState.IsKeyDown(Keys.LeftControl))
                        prevPartitionRatio += recPartition.Width / 200f;
                    else
                        prevTime = prevTime.Add(TimeSpan.FromMilliseconds(timelineDuration));
                }

                if (mouseState.Y > recPartition.Bottom)
                {
                    Mouse.SetPosition(mouseState.X, recPartition.Bottom);
                }
                else if (mouseState.Y < recPartition.Top)
                {
                    Mouse.SetPosition(mouseState.X, recPartition.Top);
                }

                prevPartitionRatio = MathHelper.Clamp(prevPartitionRatio, 0.05f, 10f);

                if (prevTime < TimeSpan.Zero - TimeSpan.FromMilliseconds(timelineDuration))
                    prevTime = TimeSpan.Zero - TimeSpan.FromMilliseconds(timelineDuration);
                if (prevTime > Context.Map.PartitionDuration + TimeSpan.FromMilliseconds(timelineDuration))
                    prevTime = Context.Map.PartitionDuration + TimeSpan.FromMilliseconds(timelineDuration);
            }
        }

        public void Init()
        {
            ListUIChildren = new List<UIComponent>();
            countChildren = 0;

            float channelX = Rec.X + Ribbon.MARGE + 100;
            float channelHeight = (float)Rec.Height / ((float)Context.Map.Channels.Count - 1);
            float channelWidth = Rec.Width - 100 - Ribbon.MARGE;

            for (int i = 1; i < Context.Map.Channels.Count; i++)
            {
                float fi = (float)(i - 1);

                float channelY = (float)Rec.Y + channelHeight * fi;

                ClickableImage imgMuteChannel = new ClickableImage(UI, GetNewTimeSpan(), "MuteChannel" + i.ToString(), Render.texMuteChannel, Render.texMuteChannel, new Vector2(Rec.X + Ribbon.MARGE * 2 + Render.FontMenu.MeasureString("0").X, channelY + channelHeight / 2 - Render.texSoloChannel.Height / 2));
                ClickableImage imgSoloChannel = new ClickableImage(UI, GetNewTimeSpan(), "SoloChannel" + i.ToString(), Render.texSoloChannel, Render.texSoloChannel, new Vector2(Rec.X + Ribbon.MARGE * 3 + Render.FontMenu.MeasureString("0").X + Render.texSoloChannel.Width, channelY + channelHeight / 2 - Render.texMuteChannel.Height / 2));

                imgMuteChannel.Tag = i;
                imgSoloChannel.Tag = i;

                imgMuteChannel.ClickImage += new ClickableImage.ClickImageHandler(imgMuteChannel_ClickImage);
                imgSoloChannel.ClickImage += new ClickableImage.ClickImageHandler(imgSoloChannel_ClickImage);

                ListUIChildren.Add(imgMuteChannel);
                ListUIChildren.Add(imgSoloChannel);

                for (int j = 0; j < Context.Map.Channels[i].ListMusician.Count; j++)
                {
                    float fj = (float)j;

                }
            }

            InitSegment();
        }

        private void InitSegment()
        {
            //--- Calcul des segments
            timelineDuration = (float)recPartition.Width / timeWidth * Context.Map.TimeDuration * Context.PartitionRatio;

            listTimeSegment = new List<float>();

            int nbSeg = (int)(Context.Map.PartitionDuration.TotalMilliseconds / Context.Map.TimeDuration);

            for (int s = 0; s < nbSeg; s++)
            {
                float x = recPartition.Left + (float)s * ((Context.Map.TimeDuration * (float)recPartition.Width) / timelineDuration);

                listTimeSegment.Add((int)x);
            }
            //---
        }

        private void UpdateButtonState()
        {
            for (int i = 1; i < Context.Map.Channels.Count; i++)
            {
                ClickableImage imgMuteChannel = (ClickableImage)ListUIChildren.Find(ui => ui is ClickableImage && ((ClickableImage)ui).Name == "MuteChannel" + i.ToString());
                ClickableImage imgSoloChannel = (ClickableImage)ListUIChildren.Find(ui => ui is ClickableImage && ((ClickableImage)ui).Name == "SoloChannel" + i.ToString());

                TypePlaying typePlaying = Context.Map.Channels[i].TypePlaying;

                if (typePlaying == TypePlaying.Muted)
                {
                    imgMuteChannel.Color = Color.Blue;
                    imgSoloChannel.Color = Color.White;
                }
                else if (typePlaying == TypePlaying.Solo)
                {
                    imgMuteChannel.Color = Color.White;
                    imgSoloChannel.Color = Color.Blue;
                }
                else if (typePlaying == TypePlaying.Playing)
                {
                    imgMuteChannel.Color = Color.White;
                    imgSoloChannel.Color = Color.White;
                }
            }
        }

        void imgMuteChannel_ClickImage(ClickableImage image, MouseState mouseState, GameTime gameTime)
        {
            GamePlay.MuteChannel((int)image.Tag);
            UpdateButtonState();
        }

        void imgSoloChannel_ClickImage(ClickableImage image, MouseState mouseState, GameTime gameTime)
        {
            GamePlay.SoloChannel((int)image.Tag);
            UpdateButtonState();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            Rectangle rec = new Rectangle(Rec.X, Rec.Y, Rec.Width, (int)((float)Rec.Height * 1.2f));

            Render.SpriteBatch.Draw(Render.texEmptyGradient, rec, VisualStyle.BackColorLight);

            float channelHeight = (float)Rec.Height / ((float)Context.Map.Channels.Count - 1);
            float channelWidth = Rec.Width - 100 - Ribbon.MARGE;
            float posPartBegin = (Rec.Width - 100 - Ribbon.MARGE) * 0.1f;
            float timePartBegin = (posPartBegin / channelWidth) * timelineDuration;

            float channelX = Rec.X + Ribbon.MARGE + 100;
            int elapsedTimeWidth = (int)(((float)Context.Time.TotalMilliseconds * channelWidth) / timelineDuration);

            for (int i = 1; i < Context.Map.Channels.Count; i++)
            {
                float fi = (float)(i - 1);

                float channelY = (float)Rec.Y + channelHeight * fi;

                Rectangle recChannel = new Rectangle(
                    (int)channelX,
                    (int)channelY,
                    (int)channelWidth,
                    (int)channelHeight);

                //--- Nombre de musiciens en cours
                Render.SpriteBatch.DrawString(Render.FontMenu, Context.Map.Channels[i].ListMusician.Count(m => m.IsPlaying).ToString(), new Vector2(Rec.X + Ribbon.MARGE, channelY + channelHeight / 2 - +Render.FontMenu.MeasureString("0").Y / 2), Color.White);
                //---

                //--- Channel
                Render.SpriteBatch.Draw(Render.texEmptyGradient, recChannel, Context.Map.Channels[i].Color);
                //Render.SpriteBatch.Draw(Render.texEmptyGradient, recChannel, new Color(0.1f,0.1f,0.1f));
                //---

                //--- Affichage des ségments de temps
                for (int s = 0; s < listTimeSegment.Count; s++)
                {
                    float x = listTimeSegment[s] - elapsedTimeWidth;
                    int width = 1;

                    if (s % 4 == 0)
                        width = (int)measureBarWidth;

                    if (x + posPartBegin > channelX && x + posPartBegin < (channelX + channelWidth))
                        Render.SpriteBatch.Draw(Render.texEmptyGradient, new Rectangle((int)(x + posPartBegin), (int)channelY, width, (int)channelHeight), colorTimeSegment);
                }
                //---

                if (Context.Map.Channels[i].ListMusician == null || Context.Map.Channels[i].ListMusician.Count == 0)
                    continue;

                float heightPerMusician = channelHeight / (float)Context.Map.Channels[i].ListMusician.Count;
                for (int j = 0; j < Context.Map.Channels[i].ListMusician.Count; j++)
                {
                    float fj = (float)j;

                    for (int k = 0; k < Context.Map.Channels[i].ListMusician[j].Partition.Count; k++)
                    {
                        TimeValue<Cell> part = Context.Map.Channels[i].ListMusician[j].Partition[k];
                        //if (Context.Map.Channels[i].ListMusician[j].Partition[k].Instrument != null)

                        if (part.Value.Clip != null)
                        {
                            if (part.Value.Clip.Instrument != null &&
                                part.Value.Clip.Instrument is InstrumentSample &&
                                Context.Map.Channels[i].ListMusician[j].Channel == part.Value.Channel)
                            {
                                float totalMs = (float)part.Time.Subtract(Context.Time).TotalMilliseconds;
                                float duration = (float)((InstrumentSample)part.Value.Clip.Instrument).Sample.Duration.TotalMilliseconds/2f;

                                if (totalMs > -timePartBegin - duration && totalMs < timelineDuration)
                                {
                                    float sizeClip = 1f;

                                    float partPos = (float)part.Time.TotalMilliseconds * channelWidth / timelineDuration;
                                    float timePos = (float)Context.Time.TotalMilliseconds * channelWidth / timelineDuration;

                                    float musicianX = (int)channelX + (int)partPos - (int)timePos + (int)posPartBegin;
                                    float musicianWidth = duration / timelineDuration * channelWidth;

                                    if (totalMs < -timePartBegin)
                                    {
                                        sizeClip = 1f + (totalMs + timePartBegin) / duration;
                                        musicianX = (int)channelX + (int)(((1f - sizeClip) * duration + totalMs) / timelineDuration * channelWidth) + (int)posPartBegin;
                                        musicianWidth = sizeClip * duration / timelineDuration * channelWidth;
                                    }
                                    else if (totalMs + timePartBegin > timelineDuration - duration)
                                    {
                                        musicianWidth = channelWidth - musicianX + channelX;
                                    }

                                    if ((part.Time.TotalMilliseconds / duration) % 4 == 0)
                                    {
                                        musicianX += measureBarWidth;
                                        musicianWidth -= measureBarWidth;
                                    }

                                    Render.SpriteBatch.Draw(Render.texEmpty, new Rectangle(
                                       (int)(musicianX + 1),
                                       (int)(channelY + heightPerMusician * fj),
                                       (int)(musicianWidth - 1),
                                       (int)(heightPerMusician - 1)), new Color(0f, 0f, 0f, 0.5f));
                                }
                            }
                        }
                    }
                }
            }

            Render.SpriteBatch.Draw(Render.texTimeMarker, new Vector2(Rec.X + Ribbon.MARGE + 100 - Render.texTimeMarker.Width / 2 + posPartBegin, Rec.Y), Color.White);


            base.Draw(gameTime);
        }
    }
}