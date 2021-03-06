﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TheGrid.Model.UI.Menu
{
    public class Item
    {
        public CircularMenu ParentMenu { get; set; }
        public bool Checked { get; set; }
        public int Value { get; set; }
        public String Name { get; set; }
        public Color Color { get; set; }
        public bool MouseOver { get; set; }
        public bool ShowName { get; set; }

        public delegate void SelectedHandler(Item item, GameTime gameTime);
        public event SelectedHandler Selected; 
        
        public Item(CircularMenu parentMenu, string name) : this(parentMenu, name, 0) { }

        public Item(CircularMenu parentMenu, string name, int value)
        {
            ParentMenu = parentMenu;
            Name = name;
            Value = value;
        }

        public virtual void OnSelected(GameTime gameTime)
        {
            if(Selected != null)
                Selected(this, gameTime);
        }
    }
}