﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TheGrid.Model
{
    public class Channel
    {
        public Color Color { get; set; }
        public List<Musician> ListMusician { get; set; }
        public List<Sample> ListSample { get; set; }
    }
}