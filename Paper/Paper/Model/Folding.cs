﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Paper.Model
{
    public class Folding : ComponentBase, IResizableWidth, IResizableHeight
    {
        int _height = 0;
        public int Height 
        {
            get
            {
                return _height;
            }
            set
            {
                _height = (int)Math.Ceiling((double)value / (double)Common.depthUnity);

                if (_height > 6)
                    _height = 6;
                if (_height < 0)
                    _height = 0;
            }
        }
        public int Width { get; set; }

        public List<Rectangle> ListCutting { get; set; }

        public Folding(int x, int y, int width, int height)
            : base(x, y)
        {
            this.Width = width;
            this.Height = height;
            this.ListCutting = new List<Rectangle>();
        }

        public Rectangle RecTop
        {
            get
            {
                return new Rectangle(Location.X, Location.Y - Height * Common.depthUnity, Width, Height * Common.depthUnity);
            }
        }

        public Rectangle RecFace
        {
            get
            {
                return new Rectangle(Location.X, Location.Y, Width, Common.lineMidScreen.P1.Y - Location.Y + Height * Common.depthUnity);
            }
        }

        public List<Line> LineResizableWidth
        {
            get
            {
                List<Line> _lineResizeable = new List<Line>();

                Line line = new Line(Location.X + Width, Location.Y - Height * Common.depthUnity, Location.X + Width, Common.lineMidScreen.P1.Y + (Height * Common.depthUnity));

                _lineResizeable.Add(line);

                return _lineResizeable;
            }
        }

        public List<Line> LineResizableHeight
        {
            get
            {
                List<Line> _lineResizeable = new List<Line>();

                Line line1 = new Line(Location.X, Location.Y, Location.X + Width, Location.Y);
                Line line2 = new Line(Location.X, Common.lineMidScreen.P1.Y + Height * Common.depthUnity, Location.X + Width, Common.lineMidScreen.P1.Y + Height * Common.depthUnity);

                _lineResizeable.Add(line1);
                _lineResizeable.Add(line2);

                return _lineResizeable;
            }
        }

        public override System.Drawing.Rectangle RectangleSelection
        {
            get
            {
                return new System.Drawing.Rectangle(Location.X, Location.Y - Height * Common.depthUnity, Width, Common.lineMidScreen.P1.Y - Location.Y + 2*(Height * Common.depthUnity));
            }
        }
    }

    public class CuboidComparerWithSelection : IComparer<ComponentBase>
    {
        public int Compare(ComponentBase x, ComponentBase y)
        {
            int ret = 0;

            if (x.ModeSelection != ModeSelection.None && y.ModeSelection == ModeSelection.None)
                ret = 1;
            else if (x.ModeSelection == ModeSelection.None && y.ModeSelection != ModeSelection.None)
                ret = -1;

            IResizableHeight xh = x as IResizableHeight;
            IResizableHeight yh = x as IResizableHeight;

            if (ret == 0 && xh != null && yh != null)
                ret = xh.Height - yh.Height;

            if (ret == 0)
                ret = x.CreationDate.CompareTo(y.CreationDate);

            return ret;
        }
    }

    public class CuboidComparer : IComparer<ComponentBase>
    {
        public int Compare(ComponentBase x, ComponentBase y)
        {
            IResizableHeight xh = x as IResizableHeight;
            IResizableHeight yh = x as IResizableHeight;

            int ret = 0;

            if (xh != null && yh != null)
                ret = xh.Height - yh.Height;

            if (ret == 0)
                ret = x.CreationDate.CompareTo(y.CreationDate);

            return ret;
        }
    }


    public enum ModeSelection
    {
        None,
        NearMove,
        NearResizeWidth,
        NearResizeHeight,
        SelectedMove,
        SelectedResizeWidth,
        SelectedResizeHeight
    }
}
