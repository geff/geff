﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheGrid.Model;
using TheGrid.Model.UI.Menu;
using Microsoft.Xna.Framework;
using TheGrid.Common;
using TheGrid.Model.Instrument;
using TheGrid.Model.UI;
using Microsoft.Xna.Framework.Graphics;
using TheGrid.Model.UI.Effect;
using TheGrid.Model.Effect;
using TheGrid.Model.UI.Note;
using System.IO;
using System.Windows.Forms;

namespace TheGrid.Logic.UI
{
    public class UILogic
    {
        public GameEngine GameEngine { get; set; }

        public List<UIComponent> ListUIComponent { get; set; }
        public Ribbon Ribbon;
        public CircularMenu Menu;

        public UILogic(GameEngine gameEngine)
        {
            this.GameEngine = gameEngine;
            this.ListUIComponent = new List<UIComponent>();

            Ribbon = new Ribbon(this, null, TimeSpan.FromDays(1));
            ListUIComponent.Add(Ribbon);

            CreateCircularMenu();
            Menu.Enter += new CircularMenu.EnterHandler(menu_Enter);
            Menu.Leave += new CircularMenu.LeaveHandler(menu_Leave);
        }

        #region Menu
        private void CreateCircularMenu()
        {
            Menu = new CircularMenu(this, null, TimeSpan.FromDays(2), null, null, null, false, true);
            Menu.Visible = true;
            Menu.LocalSize = 110;

            Item itemNew = new Item(Menu, "New");
            itemNew.Selected += new Item.SelectedHandler(itemNew_Selected);
            Menu.Items.Add(itemNew);

            Item itemLoad = new Item(Menu, "Load");
            itemLoad.Selected += new Item.SelectedHandler(itemLoad_Selected);
            Menu.Items.Add(itemLoad);

            Item itemSave = new Item(Menu, "Save");
            itemSave.Selected += new Item.SelectedHandler(itemSave_Selected);
            Menu.Items.Add(itemSave);

            Item itemExit = new Item(Menu, "Exit");
            itemExit.Selected += new Item.SelectedHandler(itemExit_Selected);
            Menu.Items.Add(itemExit);

            Item itemMenu = new Item(Menu, "Menu");
            Menu.Items.Add(itemMenu);

            Menu.Location = new Vector2(0f, 0f);
            Menu.nbVertex = Menu.Items.Count * 4;
            Menu.PercentVisibility = 0f;
            Menu.State = ComponentState.Closed;
            Menu.EffectVertex = GameEngine.Render.effectUI;
            Menu.EffectSprite = GameEngine.Render.effectUISprite;
            Menu.IsUI = true;
            Menu.IsTurnMode = true;

            double angleItem = MathHelper.PiOver2 / ((double)Menu.Items.Count - 1);

            Menu.MaxAngle = ((double)Menu.Items.Count) * angleItem + 0.03;
            Menu.MinAngleDelta = -MathHelper.PiOver2 + MathHelper.Pi / 12;
            Menu.MaxAngleDelta = angleItem - 0.01;

            Menu.AngleDelta = Menu.MinAngleDelta;

            Menu.CreateVertex();

            ListUIComponent.Add(Menu);
        }

        void menu_Leave(GameTime gameTime)
        {
            Menu.Close(gameTime);
        }

        void menu_Enter(GameTime gameTime)
        {
            Menu.Open(gameTime);
        }

        void itemNew_Selected(Item item, GameTime gameTime)
        {
            ListLibrary listLibrary = new ListLibrary(
                this,
                null,
                gameTime.TotalGameTime,
                new Rectangle(
                    (int)(0.25f * GameEngine.Render.ScreenWidth),
                    (int)(0.25f * GameEngine.Render.ScreenHeight),
                    (int)(0.5f * GameEngine.Render.ScreenWidth),
                    (int)(0.73f * GameEngine.Render.ScreenHeight)),
                GameEngine.Render.FontText, false);

            ListUIComponent.Add(listLibrary);
        }

        void itemLoad_Selected(Item item, GameTime gameTime)
        {
            ListFile listFile = new ListFile(
                this,
                null,
                gameTime.TotalGameTime,
                Path.Combine(Directory.GetParent(Application.ExecutablePath).FullName, @"Files\Level\"),
                new Rectangle(
                    (int)(0.25f * GameEngine.Render.ScreenWidth),
                    (int)(0.25f * GameEngine.Render.ScreenHeight),
                    (int)(0.5f * GameEngine.Render.ScreenWidth),
                    (int)(0.73f * GameEngine.Render.ScreenHeight)),
                GameEngine.Render.FontText);

            ListUIComponent.Add(listFile);
        }

        void itemSave_Selected(Item item, GameTime gameTime)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.InitialDirectory = Path.Combine(Directory.GetParent(Application.ExecutablePath).FullName, @"Files\Level\");
            dlg.Filter = "Niveau The Grid (*.xml)|*.xml";

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                FileSystem.SaveLevel(Context.Map, Path.GetFileNameWithoutExtension(dlg.FileName));
            }
        }

        void itemExit_Selected(Item item, GameTime gameTime)
        {
            GameEngine.Exit();
        }
        #endregion

        public void Update(GameTime gameTime)
        {
            ListUIComponent.Sort(new ComparerUIComponent());

            for (int i = 0; i < ListUIComponent.Count; i++)
            {
                if (ListUIComponent[i].Alive)
                {
                    ListUIComponent[i].Update(gameTime);
                }

                ListUIComponent[i].UpdateUIDependency(gameTime);
            }

            ListUIComponent.RemoveAll(ui => !ui.Alive);
        }

        public void Draw(GameTime gameTime)
        {
            GameEngine.Render.SpriteBatch.Begin();

            ListUIComponent.Sort(new ComparerUIComponent());
            ListUIComponent.Reverse();

            foreach (UIComponent uiComponent in ListUIComponent)
            {
                if (uiComponent.Visible)
                {
                    uiComponent.Draw(gameTime);
                }
            }

            GameEngine.Render.SpriteBatch.End();
        }

        public bool IsUIModalActive()
        {
            bool isUIModalActive = false;

            foreach (UIComponent ui in ListUIComponent)
            {
                if (ui.Alive && ui.Visible)
                {
                    isUIModalActive |= ui.IsUIModalActive();
                }
            }

            return isUIModalActive;
        }

        public bool IsMouseHandled()
        {
            bool isMouseHandled = false;

            if (ListUIComponent != null)
            {
                foreach (UIComponent ui in ListUIComponent)
                {
                    if (ui.Alive && ui.Visible)
                    {
                        if (ui is NotePanel && ui.MouseHandled)
                        {
                            int a = 0;
                        }

                        isMouseHandled |= ui.IsMouseHandled();
                    }
                }
            }

            return isMouseHandled;
        }

        public void OpenPannelEffect(GameTime gameTime, ChannelEffect channelEffect, Cell cell)
        {
            EffectPanel effectPanel = new EffectPanel(this, null, gameTime.TotalGameTime, channelEffect, cell);
            this.ListUIComponent.Add(effectPanel);
        }

        public void OpenListSample(GameTime gameTime, Cell cell)
        {
            ListSample listSample = new ListSample(this, null, gameTime.TotalGameTime, cell, cell.Channel, new Rectangle(GameEngine.GraphicsDevice.Viewport.Width / 2 - 125, (int)(GameEngine.GraphicsDevice.Viewport.Height * 0.25), 250, (int)(GameEngine.GraphicsDevice.Viewport.Height * 0.6)), GameEngine.Render.FontText, false);
            listSample.Modal = true;
            this.ListUIComponent.Add(listSample);
        }

        public void OpenKeyboard(GameTime gameTime, Cell cell)
        {
            NotePanel notePanel = new NotePanel(this, TimeSpan.FromDays(1));
            this.ListUIComponent.Add(notePanel);
        }

        public CircularMenu GetCurrentMenu()
        {
            List<UIComponent> menus = ListUIComponent.FindAll(ui => ui is CircularMenu && !((CircularMenu)ui).IsUI);
            menus.Sort((x, y) => x.CreationTime.CompareTo(y.CreationTime));

            return GetCurrentMenu(menus);
        }

        public CircularMenu GetCurrentMenu(List<UIComponent> menus)
        {
            CircularMenu currentMenu = null;
            if (menus != null && menus.Count > 0)
                currentMenu = (CircularMenu)menus[0];

            return currentMenu;
        }

        public void CloseMenu(GameTime gameTime)
        {
            List<UIComponent> menus = ListUIComponent.FindAll(ui => ui is CircularMenu && !((CircularMenu)ui).IsUI);
            menus.Sort((x, y) => x.CreationTime.CompareTo(y.CreationTime));

            CircularMenu currentMenu = GetCurrentMenu(menus);

            //--- Ferme le précédent menu
            if (menus.Count > 0)
            {
                foreach (UIComponent menu in menus)
                {
                    CircularMenu m = (CircularMenu)menu;
                    if (m.State == ComponentState.Opened || m.State == ComponentState.Opening || m.State == ComponentState.WaitDependency)
                    {
                        m.Close(gameTime);

                        //---> Supprime le menu dépendant du menu courant
                        GameEngine.UI.ListUIComponent.RemoveAll(ui => ui is CircularMenu && ui.UIDependency != null && ui.UIDependency == m);
                    }
                }
            }
        }

        public void SwitchMenu(GameTime gameTime)
        {
            List<UIComponent> menus = ListUIComponent.FindAll(ui => ui is CircularMenu && !((CircularMenu)ui).IsUI);
            menus.Sort((x, y) => x.CreationTime.CompareTo(y.CreationTime));

            CircularMenu currentMenu = GetCurrentMenu(menus);

            //--- Ferme le précédent menu
            if (menus.Count > 0)
            {
                foreach (UIComponent menu in menus)
                {
                    ((CircularMenu)menu).Close(gameTime);

                    //---> Supprime le menu dépendant du menu courant
                    GameEngine.UI.ListUIComponent.RemoveAll(ui => ui is CircularMenu && ui.UIDependency != null && ui.UIDependency == menu);
                }

                //--- Créé un nouveau menu ayant comme dépendance le menu courant
                CircularMenu newMenu = GameEngine.UI.CreateMenu(Context.SelectedCell, gameTime.TotalGameTime);
                newMenu.Alive = true;
                newMenu.UIDependency = currentMenu;
                newMenu.State = ComponentState.WaitDependency;

                GameEngine.UI.ListUIComponent.Add(newMenu);
                //---

                //--- Met la lecture de la partition en pause
                if (Context.StatePlaying == StatePlaying.Playing)
                    Context.StatePlaying = StatePlaying.Waiting;
                //---
            }
            else if (currentMenu == null)// || currentMenu.State == ComponentState.Closing || currentMenu.State == ComponentState.Closed)
            {
                //---> Ouvre le nouveau menu
                CircularMenu newMenu = GameEngine.UI.CreateMenu(Context.SelectedCell, gameTime.TotalGameTime);
                newMenu.Alive = true;
                GameEngine.UI.ListUIComponent.Add(newMenu);

                newMenu.Open(gameTime);

                //--- Met la lecture de la partition en pause
                if (Context.StatePlaying == StatePlaying.Playing)
                    Context.StatePlaying = StatePlaying.Waiting;
                //---
            }
            //---
        }

        #region Menu
        private void NextMenu(CircularMenu previousMenu, CircularMenu nextMenu)
        {
            previousMenu.UIDependency = null;
            nextMenu.UIDependency = previousMenu;
            nextMenu.Alive = true;
            nextMenu.Visible = false;
            this.ListUIComponent.Add(nextMenu);
        }

        public CircularMenu CreateMenu(Cell cell, TimeSpan creationTime)
        {
            //---
            CircularMenu menuRoot = new CircularMenu(this, null, creationTime, cell, null, null, true);

            Item itemReset = new Item(menuRoot, "Reset");
            itemReset.Selected += new Item.SelectedHandler(itemReset_Selected);
            menuRoot.Items.Add(itemReset);

            Item itemMenuDirection = new Item(menuRoot, "Direction");
            itemMenuDirection.Selected += new Item.SelectedHandler(itemMenuDirection_Selected);
            menuRoot.Items.Add(itemMenuDirection);

            Item itemMenuRepeater = new Item(menuRoot, "Repeater");
            itemMenuRepeater.Selected += new Item.SelectedHandler(itemMenuRepeater_Selected);
            menuRoot.Items.Add(itemMenuRepeater);

            //Item itemMenuSpeed = new Item(menuRoot, "Speed");
            //itemMenuSpeed.Selected += new Item.SelectedHandler(itemMenuSpeed_Selected);
            //menuRoot.Items.Add(itemMenuSpeed);

            Item itemMenuDuration = new Item(menuRoot, "Duration");
            itemMenuDuration.Selected += new Item.SelectedHandler(itemMenuDuration_Selected);
            menuRoot.Items.Add(itemMenuDuration);

            Item itemMenuInstrument = new Item(menuRoot, "Instrument");
            itemMenuInstrument.Selected += new Item.SelectedHandler(itemMenuInstrument_Selected);
            menuRoot.Items.Add(itemMenuInstrument);

            Item itemMenuChannel = new Item(menuRoot, "Channel");
            itemMenuChannel.Selected += new Item.SelectedHandler(itemMenuChannel_Selected);
            menuRoot.Items.Add(itemMenuChannel);
            //---

            return menuRoot;
        }

        #region Menu Channel
        void itemMenuChannel_Selected(Item item, GameTime gameTime)
        {
            item.ParentMenu.Close(gameTime);

            CircularMenu menuChannel = new CircularMenu(this, null, gameTime.TotalGameTime, item.ParentMenu.ParentCell, item.ParentMenu, item, false);

            for (int i = 0; i < Context.Map.Channels.Count; i++)
            {
                Item itemChannel = new Item(menuChannel, Context.Map.Channels[i].Name, i);
                itemChannel.Color = Context.Map.Channels[i].Color;

                if (item.ParentMenu.ParentCell.Channel != null && item.ParentMenu.ParentCell.Channel == Context.Map.Channels[i])
                    itemChannel.Checked = true;

                itemChannel.Selected += new Item.SelectedHandler(itemChannel_Selected);
                menuChannel.Items.Add(itemChannel);
            }

            menuChannel.nbVertex = menuChannel.Items.Count * 4;

            NextMenu(item.ParentMenu, menuChannel);
        }

        void itemChannel_Selected(Item item, GameTime gameTime)
        {
            item.ParentMenu.Close(gameTime);

            item.ParentMenu.ParentCell.Channel = Context.Map.Channels[item.Value];

            if (item.ParentMenu.ParentItem.Name == "Sample")
                itemSample_Selected(item, gameTime);
            else if (item.ParentMenu.ParentItem.Name == "MusicianStart")
                itemMusicianStart_Selected(item, gameTime);
            else if (item.ParentMenu.ParentItem.Name == "Effect")
                itemEffect_Selected(item, gameTime);
            else
                NextMenu(item.ParentMenu, item.ParentMenu.ParentMenu);

            GameEngine.GamePlay.EvaluateMuscianGrid();
        }
        #endregion

        #region Menu Instrument
        void itemMenuInstrument_Selected(Item item, GameTime gameTime)
        {
            item.ParentMenu.Close(gameTime);

            CircularMenu menuInstrument = new CircularMenu(this, null, gameTime.TotalGameTime, item.ParentMenu.ParentCell, item.ParentMenu, item, true);

            Item itemReset = new Item(menuInstrument, "Reset");
            itemReset.Selected += new Item.SelectedHandler(itemInstrumentReset_Selected);
            menuInstrument.Items.Add(itemReset);

            Item itemSample = new Item(menuInstrument, "Sample");
            itemSample.Selected += new Item.SelectedHandler(itemSample_Selected);
            menuInstrument.Items.Add(itemSample);

            Item itemNote = new Item(menuInstrument, "Note");
            itemNote.Selected += new Item.SelectedHandler(itemNote_Selected);
            menuInstrument.Items.Add(itemNote);

            Item itemEffect = new Item(menuInstrument, "Effect");
            itemEffect.Selected += new Item.SelectedHandler(itemEffect_Selected);
            menuInstrument.Items.Add(itemEffect);

            Item itemMusicianStart = new Item(menuInstrument, "MusicianStart");
            itemMusicianStart.Selected += new Item.SelectedHandler(itemMusicianStart_Selected);
            menuInstrument.Items.Add(itemMusicianStart);

            Item itemMusicianStop = new Item(menuInstrument, "MusicianStop");
            itemMusicianStop.Selected += new Item.SelectedHandler(itemMusicianStop_Selected);
            menuInstrument.Items.Add(itemMusicianStop);

            NextMenu(item.ParentMenu, menuInstrument);
        }

        void itemInstrumentReset_Selected(Item item, GameTime gameTime)
        {
            item.ParentMenu.Close(gameTime);

            item.ParentMenu.ParentCell.InitClip();
            item.ParentMenu.ParentCell.Clip.Instrument = null;

            GameEngine.GamePlay.EvaluateMuscianGrid();
            NextMenu(item.ParentMenu, item.ParentMenu.ParentMenu);
        }

        void itemSample_Selected(Item item, GameTime gameTime)
        {
            item.ParentMenu.Close(gameTime);

            if (item.ParentMenu.ParentCell.Channel == null || item.ParentMenu.ParentCell.Channel.Name == "Empty")
            {
                itemMenuChannel_Selected(item, gameTime);
            }
            else
            {
                item.ParentMenu.Alive = false;

                OpenListSample(gameTime, item.ParentMenu.ParentCell);
            }
        }

        void itemNote_Selected(Item item, GameTime gameTime)
        {
            item.ParentMenu.Close(gameTime);
            OpenKeyboard(gameTime, item.ParentMenu.ParentCell);
        }

        void itemEffect_Selected(Item item, GameTime gameTime)
        {
            item.ParentMenu.Close(gameTime);

            if (item.ParentMenu.ParentCell.Channel == null || item.ParentMenu.ParentCell.Channel.Name == "Empty")
            {
                itemMenuChannel_Selected(item, gameTime);
            }
            else
            {
                CircularMenu menuChannelEffect = new CircularMenu(this, null, gameTime.TotalGameTime, item.ParentMenu.ParentCell, item.ParentMenu, item, false, true);

                //--- Création des items liés aux effets du channel
                for (int i = 0; i < item.ParentMenu.ParentCell.Channel.ListEffect.Count; i++)
                {
                    Item itemChannelEffect = new Item(menuChannelEffect, item.ParentMenu.ParentCell.Channel.ListEffect[i].Name);
                    itemChannelEffect.Value = i;
                    itemChannelEffect.Selected += new Item.SelectedHandler(itemChannelEffect_Selected);
                    menuChannelEffect.Items.Add(itemChannelEffect);
                }
                //---

                NextMenu(item.ParentMenu, menuChannelEffect);
            }
        }

        void itemChannelEffect_Selected(Item item, GameTime gameTime)
        {
            item.ParentMenu.Close(gameTime);
            item.ParentMenu.Alive = false;

            OpenPannelEffect(gameTime, item.ParentMenu.ParentCell.Channel.ListEffect[item.Value], item.ParentMenu.ParentCell);
        }

        void itemMusicianStart_Selected(Item item, GameTime gameTime)
        {
            if (item.ParentMenu.ParentCell.Channel == null || item.ParentMenu.ParentCell.Channel.Name == "Empty")
            {
                itemMenuChannel_Selected(item, gameTime);
            }
            else
            {
                if (item.ParentMenu.ParentCell.Channel.CellStart != null)
                {
                    item.ParentMenu.ParentCell.Channel.CellStart.InitClip();
                    item.ParentMenu.ParentCell.Channel.CellStart.Clip.Instrument = null;
                }

                item.ParentMenu.ParentCell.Channel.CellStart = item.ParentMenu.ParentCell;
                item.ParentMenu.ParentCell.Channel.CellStart.InitClip();
                item.ParentMenu.ParentCell.Channel.CellStart.Clip.Instrument = new InstrumentStart();

                item.ParentMenu.Close(gameTime);

                GameEngine.GamePlay.EvaluateMuscianGrid();
                NextMenu(item.ParentMenu, item.ParentMenu.ParentMenu);
            }
        }

        void itemMusicianStop_Selected(Item item, GameTime gameTime)
        {
            item.ParentMenu.Close(gameTime);

            item.ParentMenu.ParentCell.InitClip();
            item.ParentMenu.ParentCell.Clip.Instrument = new InstrumentStop();

            GameEngine.GamePlay.EvaluateMuscianGrid();
            NextMenu(item.ParentMenu, item.ParentMenu.ParentMenu);
        }
        #endregion

        #region Menu Speed
        void itemMenuSpeed_Selected(Item item, GameTime gameTime)
        {
            item.ParentMenu.Close(gameTime);

            //---
            CircularMenu menuSpeed = new CircularMenu(this, null, gameTime.TotalGameTime, item.ParentMenu.ParentCell, item.ParentMenu, item, true);

            for (int i = 0; i < 9; i++)
            {
                Item itemSpeed;

                if (i == 0)
                    itemSpeed = new Item(menuSpeed, "Reset", i);
                else if (i < 5)
                    itemSpeed = new Item(menuSpeed, "SpeedH" + i.ToString(), i);
                else
                    itemSpeed = new Item(menuSpeed, "SpeedL" + (9 - i).ToString(), i - 9);

                itemSpeed.Selected += new Item.SelectedHandler(itemSpeed_Selected);
                menuSpeed.Items.Add(itemSpeed);
            }
            //---

            NextMenu(item.ParentMenu, menuSpeed);
        }

        void itemSpeed_Selected(Item item, GameTime gameTime)
        {
            item.ParentMenu.Close(gameTime);

            item.ParentMenu.ParentCell.InitClip();

            bool isChecked = !item.Checked;

            for (int i = 0; i < 9; i++)
            {
                item.ParentMenu.Items[i].Checked = false;
            }

            item.Checked = isChecked;

            item.ParentMenu.ParentCell.Clip.Speed = item.Value;

            GameEngine.GamePlay.EvaluateMuscianGrid();
            NextMenu(item.ParentMenu, item.ParentMenu.ParentMenu);
        }
        #endregion

        #region Menu Duration
        void itemMenuDuration_Selected(Item item, GameTime gameTime)
        {
            item.ParentMenu.Close(gameTime);

            //---
            CircularMenu menuDuration = new CircularMenu(this, null, gameTime.TotalGameTime, item.ParentMenu.ParentCell, item.ParentMenu, item, false);

            Item itemDurationReset = new Item(menuDuration, "Reset", 1);
            itemDurationReset.Selected += new Item.SelectedHandler(itemDuration_Selected);
            menuDuration.Items.Add(itemDurationReset);

            Item itemDurationQuarterNoteSubPart = new Item(menuDuration, "QuarterNoteSubPart");
            itemDurationQuarterNoteSubPart.Selected += new Item.SelectedHandler(itemDurationQuarterNoteSubPart_Selected);
            menuDuration.Items.Add(itemDurationQuarterNoteSubPart); 
            
            Item itemDurationQuarterNotePart = new Item(menuDuration, "QuarterNotePart");
            itemDurationQuarterNotePart.Selected += new Item.SelectedHandler(itemDurationQuarterNotePart_Selected);
            menuDuration.Items.Add(itemDurationQuarterNotePart);
            //---

            NextMenu(item.ParentMenu, menuDuration);
        }

        void itemDurationQuarterNoteSubPart_Selected(Item item, GameTime gameTime)
        {
            item.ParentMenu.Close(gameTime);

            //---
            item.ParentMenu.ParentCell.InitClip();
            float prevDuration = item.ParentMenu.ParentCell.Clip.Duration;

            CircularMenu menuDurationQuarterNoteSubPart = new CircularMenu(this, null, gameTime.TotalGameTime, item.ParentMenu.ParentCell, item.ParentMenu, item, false, false);

            for (int i = 0; i < 32; i++)
            {
                Item itemDurationQuarterNoteSubPartItem = new Item(menuDurationQuarterNoteSubPart, (i + 1).ToString(), i + 1);
                itemDurationQuarterNoteSubPartItem.Selected += new Item.SelectedHandler(itemDurationQuarterNoteSubPartItem_Selected);
                menuDurationQuarterNoteSubPart.Items.Add(itemDurationQuarterNoteSubPartItem);

                if (i % 4 == 0)
                {
                    itemDurationQuarterNoteSubPartItem.Color = VisualStyle.BackForeColorChecked;
                    itemDurationQuarterNoteSubPartItem.ShowName = true;
                }

                if (prevDuration - (int)prevDuration > (float)i / 32f  )
                    itemDurationQuarterNoteSubPartItem.Checked = true;
            }
            //---

            NextMenu(item.ParentMenu, menuDurationQuarterNoteSubPart);
        }

        void itemDurationQuarterNoteSubPartItem_Selected(Item item, GameTime gameTime)
        {
            item.ParentMenu.Close(gameTime);

            item.ParentMenu.ParentCell.InitClip();

            float prevDuration = item.ParentMenu.ParentCell.Clip.Duration;

            float newValue = (float)item.Value /32f + (int)prevDuration;
            if (prevDuration - (int)prevDuration == (float)item.Value/32f)
                newValue = 1f;

            foreach (Item itemMenu in item.ParentMenu.Items)
            {
                if(newValue ==-1)
                    itemMenu.Checked = false;
                else
                    itemMenu.Checked = itemMenu.Value <= item.Value;
            }

            item.ParentMenu.ParentCell.Clip.Duration = newValue;

            GameEngine.GamePlay.EvaluateMuscianGrid();
            NextMenu(item.ParentMenu, item.ParentMenu.ParentMenu);
        }

        void itemDurationQuarterNotePart_Selected(Item item, GameTime gameTime)
        {
            item.ParentMenu.Close(gameTime);

            //---
            CircularMenu menuDurationQuarterNotePart = new CircularMenu(this, null, gameTime.TotalGameTime, item.ParentMenu.ParentCell, item.ParentMenu, item, false, true);

            for (int i = 0; i < 8; i++)
            {
                Item itemDurationQuarterNotePartItem = new Item(menuDurationQuarterNotePart, (i + 1).ToString(), i + 1);
                itemDurationQuarterNotePartItem.Selected += new Item.SelectedHandler(itemDurationQuarterNotePartItem_Selected);
                menuDurationQuarterNotePart.Items.Add(itemDurationQuarterNotePartItem);
            }
            //---

            NextMenu(item.ParentMenu, menuDurationQuarterNotePart);
        }

        void itemDurationQuarterNotePartItem_Selected(Item item, GameTime gameTime)
        {
            item.ParentMenu.Close(gameTime);

            item.ParentMenu.ParentCell.InitClip();

            float prevDuration = item.ParentMenu.ParentCell.Clip.Duration;

            item.ParentMenu.ParentCell.Clip.Duration = (float)item.Value + prevDuration - (int)prevDuration;

            GameEngine.GamePlay.EvaluateMuscianGrid();
            NextMenu(item.ParentMenu, item.ParentMenu.ParentMenu);
        }

        private void itemDuration_Selected(Item item, GameTime gameTime)
        {
            item.ParentMenu.Close(gameTime);

            item.ParentMenu.ParentCell.InitClip();

            item.ParentMenu.ParentCell.Clip.Duration = 1f / (float)item.Value;

            GameEngine.GamePlay.EvaluateMuscianGrid();
            NextMenu(item.ParentMenu, item.ParentMenu.ParentMenu);
        }
        #endregion

        #region Menu Repeater
        void itemMenuRepeater_Selected(Item item, GameTime gameTime)
        {
            item.ParentMenu.Close(gameTime);

            //---
            CircularMenu menuRepeater = new CircularMenu(this, null, gameTime.TotalGameTime, item.ParentMenu.ParentCell, item.ParentMenu, item, true);
            menuRepeater.AngleDelta = -MathHelper.TwoPi / 12;

            for (int i = 0; i < 6; i++)
            {
                Item itemRepeater = new Item(menuRepeater, "Repeater" + (i + 1).ToString(), i);

                if (item.ParentMenu.ParentCell.Clip != null && item.ParentMenu.ParentCell.Clip.Repeater.HasValue && i <= item.ParentMenu.ParentCell.Clip.Repeater.Value)
                    itemRepeater.Checked = true;

                itemRepeater.Selected += new Item.SelectedHandler(itemRepeater_Selected);
                menuRepeater.Items.Add(itemRepeater);
            }
            //---

            NextMenu(item.ParentMenu, menuRepeater);
        }

        void itemRepeater_Selected(Item item, GameTime gameTime)
        {
            item.ParentMenu.Close(gameTime);

            item.ParentMenu.ParentCell.InitClip();
            int newValue = item.Value;

            if (item.ParentMenu.ParentCell.Clip.Repeater == item.Value)
                newValue = -1;

            for (int i = 0; i < 6; i++)
            {
                item.ParentMenu.Items[i].Checked = i <= newValue;
            }

            if (newValue == -1)
                item.ParentMenu.ParentCell.Clip.Repeater = null;
            else
                item.ParentMenu.ParentCell.Clip.Repeater = newValue;

            GameEngine.GamePlay.EvaluateMuscianGrid();
            NextMenu(item.ParentMenu, item.ParentMenu.ParentMenu);
        }
        #endregion

        #region Menu Direction
        void itemMenuDirection_Selected(Item item, GameTime gameTime)
        {
            item.ParentMenu.Close(gameTime);

            //---
            CircularMenu menuDirection = new CircularMenu(this, null, gameTime.TotalGameTime, item.ParentMenu.ParentCell, item.ParentMenu, item, true);

            for (int i = 0; i < 6; i++)
            {
                Item itemDirection = new Item(menuDirection, "Direction" + (i + 1).ToString(), i);

                if (item.ParentMenu.ParentCell.Clip != null && item.ParentMenu.ParentCell.Clip.Directions[i])
                    itemDirection.Checked = true;

                itemDirection.Selected += new Item.SelectedHandler(itemDirection_Selected);
                menuDirection.Items.Add(itemDirection);
            }
            //---

            NextMenu(item.ParentMenu, menuDirection);
        }

        void itemDirection_Selected(Item item, GameTime gameTime)
        {
            item.Checked = !item.Checked;

            item.ParentMenu.ParentCell.InitClip();
            item.ParentMenu.ParentCell.Clip.Directions[item.Value] = item.Checked;

            GameEngine.GamePlay.EvaluateMuscianGrid();
        }
        #endregion

        void itemReset_Selected(Item item, GameTime gameTime)
        {
            item.ParentMenu.Close(gameTime);

            if (item.ParentMenu.ParentCell.Clip != null &&
                item.ParentMenu.ParentCell.Clip.Instrument != null &&
                item.ParentMenu.ParentCell.Channel != null &&
                item.ParentMenu.ParentCell == item.ParentMenu.ParentCell.Channel.CellStart)
            {
                item.ParentMenu.ParentCell.Clip.Instrument = null;
                item.ParentMenu.ParentCell.Channel.CellStart = null;
            }

            item.ParentMenu.ParentCell.Clip = null;
            item.ParentMenu.ParentCell.Channel = null;

            GameEngine.GamePlay.EvaluateMuscianGrid();

            if (Context.StatePlaying == StatePlaying.Waiting)
                Context.StatePlaying = StatePlaying.Playing;
        }
        #endregion
    }
}
