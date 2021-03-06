using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using Paper.Properties;
using Paper.Model;
using System.Drawing.Imaging;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace Paper
{
    public partial class FrmPapierEditor : Form
    {
        Graphics g;
        Graphics gBmp;
        Bitmap bmp;
        Pen penDotFar;
        Pen penDotNear;

        Scene scene = new Scene();

        ComponentBase nearestCuboid = null;
        ComponentBase _curComponent = null;

        int valMax = 0;
        int prevVScrollValue = 0;
        int prevHScrollValue = 0;

        ComponentBase curComponent
        {
            get
            {
                return _curComponent;
            }
            set
            {
                _curComponent = value;

                curResizeableWidthComponent = _curComponent as IResizableWidth;
                curResizeableHeightComponent = _curComponent as IResizableHeight;
            }
        }

        IResizableWidth curResizeableWidthComponent = null;
        IResizableHeight curResizeableHeightComponent = null;

        int prevWidth = 0;
        int prevHeight = 0;
        Point prevLocation;
        Point initialPointMouse;
        Point initialPointMouseWithoutDelta;
        Point pointMouse;
        int curLevelSearch = 0;

        public FrmPapierEditor()
        {
            InitializeComponent();

            Form1_Resize(null, null);

            InitForm();
        }

        private void InitForm()
        {
            btnFolding.Tag = Tools.Folding;
            btnLink.Tag = Tools.Link;
            btnMovingZoneH.Tag = Tools.ZoneMovingH;
            btnMovingzoneV.Tag = Tools.ZoneMovingV;
            btnPlatform.Tag = Tools.Platform;
            btnSensorButton.Tag = Tools.SensorButton;
            btnSensorCamera.Tag = Tools.SensorCamera;
            btnSensorNearness.Tag = Tools.SensorNearness;
            btnSensorRemoteControl.Tag = Tools.SensorRemoteControl;
            btnZoneFoldingH.Tag = Tools.ZoneFoldingH;
            btnZoneFoldingV.Tag = Tools.ZoneFoldingV;
            btnHole.Tag = Tools.Hole;

            valMax = vScrollBar.Maximum - vScrollBar.LargeChange + 1; ;

            vScrollBar.Value = valMax;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (pic.Width == 0 || pic.Height == 0)
                return;

            g = pic.CreateGraphics();
            bmp = new Bitmap(pic.Width, pic.Height);
            gBmp = Graphics.FromImage(bmp);

            Common.Bottom = Common.MaxDepth * 2 * Common.depthUnity;

            Common.ScreenSize = new Size(pic.Width, pic.Height);

            Common.lineMidScreen = new Line();
            Common.lineMidScreen.P1 = new Point(0, Common.ScreenSize.Height - Common.Bottom);
            Common.lineMidScreen.P2 = new Point(pic.Width, Common.ScreenSize.Height - Common.Bottom);

            Common.Delta = new Point(-hScrollBar.Value, valMax - vScrollBar.Value + Common.lineMidScreen.P1.Y);

            penDotFar = new Pen(Color.White, 2f);
            penDotFar.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            penDotNear = new Pen(Color.White, 2f);
            penDotNear.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

            DrawScene();
        }

        private void pic_MouseDown(object sender, MouseEventArgs e)
        {
            initialPointMouseWithoutDelta = e.Location;
            initialPointMouse = e.Location;
            initialPointMouse.Offset(-Common.Delta.X, -Common.Delta.Y);
            prevVScrollValue = vScrollBar.Value;
            prevHScrollValue = hScrollBar.Value;


            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (curComponent != null && curComponent != nearestCuboid)
                {
                    curComponent.ModeSelection = ModeSelection.None;
                    curComponent = null;
                }

                //--- Sélection des éléments pour le déplacement et le redimensionnement
                foreach (ComponentBase component in scene.listComponent)
                {
                    if (component == nearestCuboid && component.ModeSelection == ModeSelection.NearMove)
                    {
                        component.ModeSelection = ModeSelection.SelectedMove;
                        curComponent = component;
                        prevLocation = ((IMoveable)component).Location;
                    }
                    else if (component == nearestCuboid && component.ModeSelection == ModeSelection.NearResizeWidth)
                    {
                        component.ModeSelection = ModeSelection.SelectedResizeWidth;
                        curComponent = component;
                        prevWidth = ((IResizableWidth)component).Width;
                    }
                    else if (component == nearestCuboid && component.ModeSelection == ModeSelection.NearResizeHeight)
                    {
                        component.ModeSelection = ModeSelection.SelectedResizeHeight;
                        curComponent = component;
                        prevHeight = ((IResizableHeight)component).Height;
                    }
                }
                //---

                int gridWidth = int.Parse(txtGridWidth.Text);
                int gridHeight = int.Parse(txtGridHeight.Text);

                //---> Création du pliage
                if (Common.CurrentTool == Tools.Folding && curComponent == null)
                {
                    curComponent = new Folding(initialPointMouse.X, initialPointMouse.Y, gridWidth * 2, 6);
                }

                //---> Création de la zone de pliage H
                if (Common.CurrentTool == Tools.ZoneFoldingH && curComponent == null)
                {
                    curComponent = new ZoneFoldingH(initialPointMouse.X, initialPointMouse.Y, gridHeight * 2);
                }

                //---> Création de la zone de pliage V
                if (Common.CurrentTool == Tools.ZoneFoldingV && curComponent == null)
                {
                    curComponent = new ZoneFoldingV(initialPointMouse.X, initialPointMouse.Y, gridWidth * 2);
                }

                //---> Création de la zone de déplacement H
                if (Common.CurrentTool == Tools.ZoneMovingH && curComponent == null)
                {
                    curComponent = new ZoneMovingH(initialPointMouse.X, initialPointMouse.Y, gridWidth * 3, gridHeight);
                }

                //---> Création de la zone de déplacement V
                if (Common.CurrentTool == Tools.ZoneMovingV && curComponent == null)
                {
                    curComponent = new ZoneMovingV(initialPointMouse.X, initialPointMouse.Y, gridWidth, gridHeight * 3);
                }

                //---> Création de la platforme
                if (Common.CurrentTool == Tools.Platform && curComponent == null)
                {
                    curComponent = new Platform(initialPointMouse.X, initialPointMouse.Y, gridWidth * 3, gridHeight * 2);
                }

                if (Common.CurrentTool == Tools.SensorButton && curComponent == null)
                {
                    curComponent = new Sensor(initialPointMouse.X, initialPointMouse.Y, SensorType.Button);
                }

                if (Common.CurrentTool == Tools.SensorCamera && curComponent == null)
                {
                    curComponent = new Sensor(initialPointMouse.X, initialPointMouse.Y, SensorType.Camera);
                }

                if (Common.CurrentTool == Tools.SensorNearness && curComponent == null)
                {
                    curComponent = new Sensor(initialPointMouse.X, initialPointMouse.Y, SensorType.Nearness);
                }

                if (Common.CurrentTool == Tools.SensorRemoteControl && curComponent == null)
                {
                    curComponent = new Sensor(initialPointMouse.X, initialPointMouse.Y, SensorType.RemoteControl);
                }

                if (Common.CurrentTool == Tools.Hole && curComponent == null)
                {
                    curComponent = new Hole(initialPointMouse.X, initialPointMouse.Y, gridWidth * 3, gridHeight * 3);
                }

                //---> Stockage du nouvel élément
                if (Common.CurrentTool != Tools.None && curComponent != null && curComponent.ModeSelection == ModeSelection.None)
                {
                    prevLocation = ((IMoveable)curComponent).Location;
                    curComponent.ModeSelection = ModeSelection.SelectedMove;
                    curComponent.ColorIndex = Common.CurrentColorIndex;
                    scene.listComponent.Add(curComponent);
                    CalcFoldingIntersections();
                }

                DrawScene();
            }
        }

        private void pic_MouseUp(object sender, MouseEventArgs e)
        {
            //---> Suppression du cuboid proche
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                for (int i = 0; i < scene.listComponent.Count; i++)
                {
                    if (scene.listComponent[i].ModeSelection == ModeSelection.NearMove)
                    {
                        if (nearestCuboid == scene.listComponent[i])
                            nearestCuboid = null;
                        if (curComponent == scene.listComponent[i])
                            curComponent = null;

                        this.scene.listComponent.RemoveAt(i);
                        i--;
                    }
                }

                CalcFoldingIntersections();
                DrawScene();
            }
        }

        private void pic_MouseMove(object sender, MouseEventArgs e)
        {
            pointMouse = new Point(e.X, e.Y);

            if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                hScrollBar.Value = Math.Min(hScrollBar.Maximum, Math.Max(hScrollBar.Minimum, prevHScrollValue + (initialPointMouseWithoutDelta.X - pointMouse.X)));
                vScrollBar.Value = Math.Min(valMax, Math.Max(vScrollBar.Minimum, prevVScrollValue + (initialPointMouseWithoutDelta.Y - pointMouse.Y)));

                Common.Delta = new Point(-hScrollBar.Value, valMax - vScrollBar.Value + Common.lineMidScreen.P1.Y);

                DrawScene();
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Left && curComponent != null)
            {
                pointMouse.Offset(-Common.Delta.X, -Common.Delta.Y);


                if (curComponent.ModeSelection == ModeSelection.SelectedMove)
                {
                    int gridWidth = 1;
                    int gridHeight = 1;

                    if (btnGrid.Checked)
                    {
                        gridWidth = int.Parse(txtGridWidth.Text);
                        gridHeight = int.Parse(txtGridHeight.Text);
                    }



                    ((IMoveable)curComponent).Location = new Point((prevLocation.X + pointMouse.X - initialPointMouse.X) / gridWidth * gridWidth,
                                                  (prevLocation.Y + pointMouse.Y - initialPointMouse.Y) / gridHeight * gridHeight);

                }
                else if (curComponent.ModeSelection == ModeSelection.SelectedResizeWidth)
                {
                    if (curResizeableWidthComponent != null)
                    {
                        int gridWidth = 1;

                        if (btnGrid.Checked)
                        {
                            gridWidth = int.Parse(txtGridWidth.Text);
                        }

                        curResizeableWidthComponent.Width = (prevWidth + pointMouse.X - initialPointMouse.X) / gridWidth * gridWidth;

                        if (curResizeableWidthComponent.Width <= 0)
                            curResizeableWidthComponent.Width = 1;
                    }
                }
                else if (curComponent.ModeSelection == ModeSelection.SelectedResizeHeight)
                {
                    if (curResizeableHeightComponent != null)
                    {
                        int gridHeight = 1;

                        if (btnGrid.Checked)
                        {
                            gridHeight = int.Parse(txtGridHeight.Text);
                        }

                        curResizeableHeightComponent.Height = (prevHeight + pointMouse.Y - initialPointMouse.Y) / gridHeight * gridHeight; ;

                        if (curResizeableHeightComponent.Height <= 0)
                            curResizeableHeightComponent.Height = 1;
                    }
                }

                //SortCuboid();

                //CalcCuboidIntersections();

                CalcFoldingIntersections();

                DrawScene();
            }
            //--- Calcul des poignées de redimensionnement
            else if (e.Button == System.Windows.Forms.MouseButtons.None)// && Common.CurrentTool == Tools.None)
            {
                int distanceNearestCuboid = int.MaxValue;
                nearestCuboid = null;
                ModeSelection nearestModeSelection = ModeSelection.None;

                List<ComponentBase> listComponentForMove = new List<ComponentBase>();
                List<ComponentBase> listComponentForHResize = new List<ComponentBase>();
                List<ComponentBase> listComponentForWResize = new List<ComponentBase>();

                foreach (ComponentBase component in scene.listComponent)
                {
                    if (component != curComponent)
                        component.ModeSelection = ModeSelection.None;
                    else
                        component.ModeSelection = ModeSelection.Selected;

                    //--- Distance move
                    if (component.RectangleSelection.Contains(pointMouse))
                        listComponentForMove.Add(component);
                    //---

                    //--- Distance resize
                    IResizableWidth resizeableWComponent = component as IResizableWidth;
                    IResizableHeight resizeableHComponent = component as IResizableHeight;

                    if (resizeableWComponent != null)
                    {
                        float distance = float.MaxValue / 2f;

                        //--- Calcul la distance du point à chaque segment limite de redimensionnement
                        foreach (Line lineResizeLimit in resizeableWComponent.LineResizableWidth)
                        {
                            distance = Utils.DistancePaAB(pointMouse, lineResizeLimit.P1, lineResizeLimit.P2);

                            if (distance < 10 && distance < distanceNearestCuboid)
                                listComponentForWResize.Add(component);
                        }
                    }

                    if (resizeableHComponent != null)
                    {
                        float distance = float.MaxValue / 2f;

                        //--- Calcul la distance du point à chaque segment limite de redimensionnement
                        foreach (Line lineResizeLimit in resizeableHComponent.LineResizableHeight)
                        {
                            distance = Utils.DistancePaAB(pointMouse, lineResizeLimit.P1, lineResizeLimit.P2);

                            if (distance < 10 && distance < distanceNearestCuboid)
                                listComponentForHResize.Add(component);
                        }
                    }
                    //---
                }

                //--- Sélection NearMove
                int localLevelSearch = curLevelSearch;

                if (listComponentForMove.Count - 1 < curLevelSearch && listComponentForMove.Count > 0)
                    localLevelSearch = listComponentForMove.Count - 1;

                if (listComponentForMove.Count > localLevelSearch)
                {
                    nearestCuboid = listComponentForMove[localLevelSearch];
                    nearestModeSelection = ModeSelection.NearMove;
                }
                //---

                //--- Sélection NearResizeWidth
                localLevelSearch = curLevelSearch;

                if (listComponentForWResize.Count - 1 < curLevelSearch && listComponentForWResize.Count > 0)
                    localLevelSearch = listComponentForWResize.Count - 1;

                if (listComponentForWResize.Count > localLevelSearch)
                {
                    nearestCuboid = listComponentForWResize[localLevelSearch];
                    nearestModeSelection = ModeSelection.NearResizeWidth;
                }
                //---

                //--- Sélection NearResizeHeight
                localLevelSearch = curLevelSearch;

                if (listComponentForHResize.Count - 1 < curLevelSearch && listComponentForHResize.Count > 0)
                    localLevelSearch = listComponentForHResize.Count - 1;

                if (listComponentForHResize.Count > localLevelSearch)
                {
                    nearestCuboid = listComponentForHResize[localLevelSearch];
                    nearestModeSelection = ModeSelection.NearResizeHeight;
                }
                //---

                if (nearestCuboid != null)
                    nearestCuboid.ModeSelection = nearestModeSelection;

                //if (nearestModeSelection != ModeSelection.None)
                {
                    SortCuboid();

                    DrawScene();
                }

                if (nearestModeSelection == ModeSelection.None)
                    pic.Cursor = Cursors.Default;
                else if (nearestModeSelection == ModeSelection.NearResizeWidth)
                    pic.Cursor = Cursors.SizeWE;
                else if (nearestModeSelection == ModeSelection.NearResizeHeight)
                    pic.Cursor = Cursors.SizeNS;
                else if (nearestModeSelection == ModeSelection.NearMove)
                    pic.Cursor = Cursors.SizeAll;
            }
            //---
        }

        private void SortCuboid()
        {
            SortCuboid(true);
        }

        private void SortCuboid(bool handleSelection)
        {
            //if (handleSelection)
            //    scene.listComponent.Sort(new CuboidComparerWithSelection());
            //else
            scene.listComponent.Sort(new CuboidComparer());
        }

        private void CalcFoldingIntersections()
        {
            CalcHoleIntersections();

            scene.listComponent.Sort(new CuboidComparer());

            int i = 0;
            foreach (ComponentBase component in scene.listComponent)
            {
                Folding folding = component as Folding;

                if (folding != null)
                {
                    //folding.CuttingFace = new Cutting(folding);
                    //folding.CuttingFace.Rectangle = folding.RecFaceWithoutDelta;
                    //folding.CuttingFace.IsEmpty = false;

                    for (int j = i - 1; j >= 0; j--)
                    {
                        Folding folding2 = scene.listComponent[j] as Folding;

                        if (folding2 != null)
                        {
                            Rectangle recFace2 = new Rectangle(folding2.RecFaceWithoutDelta.Left, folding2.RecFaceWithoutDelta.Top - (folding2.Height - folding.Height), folding2.RecFaceWithoutDelta.Width, folding2.RecFaceWithoutDelta.Height);

                            IntersectCutting(folding.CuttingFace, recFace2);
                        }
                    }

                }
                i++;

            }

            scene.listComponent.Sort(new CuboidComparerTop());

            i = 0;
            foreach (ComponentBase component in scene.listComponent)
            {
                Folding folding = component as Folding;

                if (folding != null)
                {
                    //folding.CuttingTop = new Cutting(folding);
                    //folding.CuttingTop.Rectangle = folding.RecTopWithoutDelta;
                    //folding.CuttingTop.IsEmpty = false;

                    for (int j = i + 1; j < scene.listComponent.Count; j++)
                    {
                        Folding folding2 = scene.listComponent[j] as Folding;

                        if (folding2 != null)
                        {
                            Rectangle recFace2 = new Rectangle(folding2.RecTopWithoutDelta.Left, folding.RecTopWithoutDelta.Top, folding2.RecTopWithoutDelta.Width, folding2.RecTopWithoutDelta.Height);

                            IntersectCutting(folding.CuttingTop, recFace2);
                        }
                    }

                }
                i++;
            }

        }

        private void CalcHoleIntersections()
        {
            scene.listComponent.Sort(new CuboidComparer());
            scene.listComponent.Reverse();

            int i = 0;
            foreach (ComponentBase component in scene.listComponent)
            {
                Folding folding = component as Folding;

                if (folding != null)
                {
                    folding.CuttingFace = new Cutting(folding);
                    folding.CuttingFace.Rectangle = folding.RecFaceWithoutDelta;
                    folding.CuttingFace.IsEmpty = false;

                    for (int j = i - 1; j >= 0; j--)
                    {
                        Hole hole = scene.listComponent[j] as Hole;

                        if (hole != null)
                        {
                            Rectangle recFace2 = hole.RectangleSelection;
                            recFace2.Y -= Common.lineMidScreen.P1.Y;

                            IntersectCutting(folding.CuttingFace, recFace2);
                        }
                    }

                }
                i++;

            }

            scene.listComponent.Sort(new CuboidComparerTop());

            i = 0;
            foreach (ComponentBase component in scene.listComponent)
            {
                Folding folding = component as Folding;

                if (folding != null)
                {
                    folding.CuttingTop = new Cutting(folding);
                    folding.CuttingTop.Rectangle = folding.RecTopWithoutDelta;
                    folding.CuttingTop.IsEmpty = false;

                    for (int j = i + 1; j < scene.listComponent.Count; j++)
                    {
                        Hole hole = scene.listComponent[j] as Hole;

                        if (hole != null)
                        {
                            Rectangle recFace2 = hole.RectangleSelection;
                            recFace2.Y -= Common.lineMidScreen.P1.Y;

                            IntersectCutting(folding.CuttingFace, recFace2);
                        }
                    }

                }
                i++;
            }
        }

        private void IntersectCutting(Cutting parent, Rectangle rec2)
        {
            if (!parent.IsEmpty)
            {
                Rectangle rec = parent.Rectangle;

                if (rec.IntersectsWith(rec2))
                {
                    if (parent.Cuttings.Count > 0)
                    {
                        foreach (Cutting cutting in parent.Cuttings)
                        {
                            IntersectCutting(cutting, rec2);
                        }
                    }
                    else
                    {
                        // Découpage du cutting
                        int[] locW = new int[2] { rec.Left, rec.Right };
                        int[] locH = new int[2] { rec.Top, rec.Bottom };

                        bool[] visibleW = new bool[1] { false };
                        bool[] visibleH = new bool[1] { false };

                        // _____
                        //|.....|
                        //|.....|
                        //|_____|
                        //|     |
                        //|     |
                        //|_____|
                        if (rec2.Top <= rec.Top && rec2.Bottom < rec.Bottom && rec2.Bottom > rec.Top)
                        {
                            locH = new int[3] { rec.Top, rec2.Bottom, rec.Bottom };
                            visibleH = new bool[2] { false, true };
                        }
                        // _____
                        //|     |
                        //|     |
                        //|_____|
                        //|.....|
                        //|.....|
                        //|_____|
                        //|     |
                        //|     |
                        //|_____|
                        else if(rec2.Top > rec.Top && rec2.Bottom < rec.Bottom)
                        {
                            locH = new int[4] { rec.Top, rec2.Top, rec2.Bottom, rec.Bottom };
                            visibleH = new bool[3] { true, false, true };
                        }
                        // _____
                        //|     |
                        //|     |
                        //|_____|
                        //|.....|
                        //|.....|
                        //|_____|
                        else if (rec2.Top > rec.Top && rec2.Top < rec.Bottom && rec2.Bottom >= rec.Bottom)
                        {
                            locH = new int[3] { rec.Top, rec2.Top, rec.Bottom };
                            visibleH = new bool[2] { true, false };
                        }


                        // _____  ____
                        //|.....|     |
                        //|.....|     |
                        //|_____|_____|
                        if (rec2.Left <= rec.Left && rec2.Right < rec.Right && rec2.Right > rec.Left)
                        {
                            locW = new int[3] { rec.Left, rec2.Right, rec.Right };
                            visibleW = new bool[2] { false, true };
                        }
                        // _____ _____ _____
                        //|     |.....|     |
                        //|     |.....|     |
                        //|_____|_____|_____|
                        else if (rec2.Left > rec.Left && rec2.Right < rec.Right)
                        {
                            locW = new int[4] { rec.Left, rec2.Left, rec2.Right, rec.Right };
                            visibleW = new bool[3] { true, false, true };
                        }
                        // _____  ____
                        //|     |.....|
                        //|     |.....|
                        //|_____|_____|
                        else if (rec2.Left > rec.Left && rec2.Left < rec.Right && rec2.Right >= rec.Right)
                        {
                            locW = new int[3] { rec.Left, rec2.Left, rec.Right };
                            visibleW = new bool[2] { true, false };
                        }

                        for (int x = 0; x < locW.Length - 1; x++)
                        {
                            for (int y = 0; y < locH.Length - 1; y++)
                            {
                                Cutting cutting = new Cutting(parent.ParentFolding);
                                cutting.Rectangle = new Rectangle(locW[x], locH[y], locW[x + 1] - locW[x], locH[y + 1] - locH[y]);
                                cutting.IsEmpty = (!visibleW[x] && !visibleH[y]);

                                parent.Cuttings.Add(cutting);
                            }
                        }
                    }
                }
            }
        }
        
        private void IntersectSceneBackground()
        {
            this.scene.CuttingFront = new Cutting(null);
            this.scene.CuttingTop = new Cutting(null);

            int maxX = 0;
            int maxY = int.MaxValue;

            foreach (ComponentBase component in scene.listComponent)
            {
                IMoveable moveable = component as IMoveable;
                IResizableWidth resizeableWidth = component as IResizableWidth;

                if (moveable != null)
                {
                    int x = moveable.Location.X;
                    int y = moveable.Location.Y;

                    if (resizeableWidth != null)
                    {
                        x += resizeableWidth.Width;
                    }

                    if (x > maxX)
                        maxX = x;

                    if (y < maxY)
                        maxY = y;
                }
            }

            maxY -= 5 * Common.depthUnity;
            maxX += 5 * Common.depthUnity;

            this.scene.CuttingFront.Rectangle = new Rectangle(0, maxY, maxX, Math.Abs(maxY));
            this.scene.CuttingTop.Rectangle = new Rectangle(0, 0, maxX, Common.MaxDepth * 2 * Common.depthUnity);

            scene.listComponent.Sort(new CuboidComparer());

            foreach (ComponentBase component in scene.listComponent)
            {
                Folding folding2 = component as Folding;

                if (folding2 != null)
                {
                    Rectangle recFace2 = new Rectangle(folding2.RecFaceWithoutDelta.Left, folding2.RecFaceWithoutDelta.Top - (folding2.Height - 0), folding2.RecFaceWithoutDelta.Width, folding2.RecFaceWithoutDelta.Height);

                    IntersectCutting(this.scene.CuttingFront, recFace2);
                }
            }

            scene.listComponent.Sort(new CuboidComparerTop());

            foreach (ComponentBase component in scene.listComponent)
            {

                Folding folding2 = component as Folding;

                if (folding2 != null)
                {
                    Rectangle recFace2 = new Rectangle(folding2.RecTopWithoutDelta.Left, 0, folding2.RecTopWithoutDelta.Width, folding2.RecTopWithoutDelta.Height);

                    IntersectCutting(this.scene.CuttingTop, recFace2);
                }
            }
        }

        private Color GetColorFromIndex(int index)
        {
            if (index == 1)
                return Color.FromArgb(129, 242, 48);
            if (index == 2)
                return Color.FromArgb(42, 211, 211);
            if (index == 3)
                return Color.FromArgb(255, 143, 51);
            if (index == 4)
                return Color.FromArgb(245, 49, 101);

            else return Color.White;
        }

        private float[][] GetScalingColorFromIndex(int index)
        {
            Color color = GetColorFromIndex(index);

            return new float[][] { 
                       new float[] {(float)color.R/255f,  0,  0,  0, 0},
                       new float[] {0,  (float)color.G/255f,  0,  0, 0},
                       new float[] {0,  0,  (float)color.B/255f,  0, 0},
                       new float[] {0,  0,  0,  1f, 0},
                       new float[] {0,  0,  0,  0f, 0},};
        }

        private void DrawCutting(Cutting cutting, Brush brush)
        {
            if (cutting.Cuttings.Count == 0)
            {
               /* 
                Rectangle rec2 = cutting.Rectangle;
                rec2.X += 1;
                rec2.Y += 1+Common.lineMidScreen.P1.Y;
                rec2.Width -= 2;
                rec2.Height -= 2;
                */

                if (!cutting.IsEmpty)
                {
                    gBmp.FillRectangle(brush, cutting.Rectangle.Left + Common.Delta.X, cutting.Rectangle.Top + Common.Delta.Y, cutting.Rectangle.Width, cutting.Rectangle.Height);
                    //gBmp.DrawRectangle(Pens.Red, rec2);
                }
                else
                {
                    //gBmp.DrawRectangle(Pens.Yellow, rec2);
                }
            }
            else
            {
                foreach (Cutting cuttingChild in cutting.Cuttings)
                {
                    DrawCutting(cuttingChild, brush);
                }
            }
        }

        private void DrawCuttingTop(Cutting cutting, Rectangle rec, Brush brush)
        {
            if (cutting.Cuttings.Count == 0)
            {
                Rectangle rec2 = cutting.Rectangle;

                if (!cutting.IsEmpty)
                {
                    rec2.Intersect(rec);

                    if (!rec2.IsEmpty)
                        gBmp.FillRectangle(brush, rec2.Left + Common.Delta.X, rec2.Top + Common.Delta.Y, rec2.Width, rec2.Height);

                    //gBmp.DrawRectangle(Pens.Red, rec2);
                }
                else
                {
                    //gBmp.DrawRectangle(Pens.Yellow, rec2);
                }
            }
            else
            {
                foreach (Cutting cuttingChild in cutting.Cuttings)
                {
                    DrawCuttingTop(cuttingChild, rec, brush);
                }
            }
        }

        private void DrawScene()
        {
            gBmp.Clear(Color.FromArgb(50, 50, 50));

            SolidBrush brush = null;

            for (int i = 0; i < Common.MaxDepth; i++)
            {
                brush = new SolidBrush(Color.FromArgb(35 + 10 * i, 35 + 10 * i, 35 + 10 * i));
                gBmp.FillRectangle(brush, 0f, 2 * i * Common.depthUnity + Common.Delta.Y, Common.ScreenSize.Width, 2 * Common.depthUnity);
            }

            Pen pen = Pens.Black;

            Rectangle recScreen = new Rectangle(-Common.Delta.X, -Common.Delta.Y, Common.ScreenSize.Width, Common.ScreenSize.Height);

            if (btnGrid.Checked)
            {
                int gridWidth = int.Parse(txtGridWidth.Text);
                int gridHeight = int.Parse(txtGridHeight.Text);
                int dw = Common.Delta.X % gridWidth;
                int dh = Common.Delta.Y % gridHeight;

                pen = new Pen(Color.FromArgb(60, 60, 60), 1f);

                for (int x = 0; x < Common.ScreenSize.Width / gridWidth + 1; x++)
                {
                    gBmp.DrawLine(pen, x * gridWidth + dw, 0, x * gridWidth + dw, Common.ScreenSize.Height);
                }

                for (int y = 0; y < Common.ScreenSize.Height / gridHeight + 1; y++)
                {
                    gBmp.DrawLine(pen, 0, y * gridHeight + dh, Common.ScreenSize.Width, y * gridHeight + dh);
                }
            }

            foreach (ComponentBase component in scene.listComponent)
            {
                //if (!component.RectangleSelection.IntersectsWith(recScreen) && !recScreen.Contains(component.RectangleSelection))
                //    continue;

                Folding folding = component as Folding;
                ZoneFoldingV zoneFoldingV = component as ZoneFoldingV;
                ZoneFoldingH zoneFoldingH = component as ZoneFoldingH;
                ZoneMovingV zoneMovingV = component as ZoneMovingV;
                ZoneMovingH zoneMovingH = component as ZoneMovingH;
                Platform platform = component as Platform;
                Sensor sensor = component as Sensor;
                Hole hole = component as Hole;

                pen = new Pen(GetColorFromIndex(component.ColorIndex), 2f);

                #region Folding
                if (folding != null)
                {
                    for (int i = 0; i < folding.HeightSerializable; i += 2)
                    {
                        brush = new SolidBrush(Color.FromArgb(65 + 10 * i, 65 + 10 * i, 65 + 10 * i));

                        int HeightFactor = folding.Height > (i + 1) ? 2 : 1;

                        Rectangle rec = new Rectangle(folding.Location.X, folding.Location.Y + (i - folding.HeightSerializable) * Common.depthUnity, folding.Width, HeightFactor * Common.depthUnity);

                        DrawCuttingTop(folding.CuttingTop, rec, brush);
                    }


                    brush.Color = Color.FromArgb(brush.Color.R + 30, brush.Color.G + 30, brush.Color.B + 30);

                    DrawCutting(folding.CuttingFace, brush);
                }
                #endregion

                #region ZoneFolding
                if (zoneFoldingV != null)
                {
                    DrawImageInRectangle(gBmp, Resources.Grid, zoneFoldingV.Rectangle, component.ColorIndex, true, false);

                    gBmp.DrawLine(pen, zoneFoldingV.Location.X + Common.Delta.X, 0, zoneFoldingV.Location.X + Common.Delta.X, Common.ScreenSize.Height);
                    gBmp.DrawLine(pen, zoneFoldingV.Location.X + zoneFoldingV.Width + Common.Delta.X, 0, zoneFoldingV.Location.X + zoneFoldingV.Width + Common.Delta.X, Common.ScreenSize.Height);
                }
                else if (zoneFoldingH != null)
                {
                    DrawImageInRectangle(gBmp, Resources.Grid, zoneFoldingH.Rectangle, component.ColorIndex, false, true);

                    gBmp.DrawLine(pen, 0, zoneFoldingH.Location.Y + Common.Delta.Y, Common.ScreenSize.Width, zoneFoldingH.Location.Y + Common.Delta.Y);
                    gBmp.DrawLine(pen, 0, zoneFoldingH.Location.Y + zoneFoldingH.Height + Common.Delta.Y, Common.ScreenSize.Width, zoneFoldingH.Location.Y + zoneFoldingH.Height + Common.Delta.Y);
                }
                #endregion

                #region ZoneMoving
                if (zoneMovingV != null)
                {
                    DrawImageInRectangle(gBmp, Resources.GridV, zoneMovingV.RectangleSelection, component.ColorIndex, true, true);

                    gBmp.DrawRectangle(pen, zoneMovingV.RectangleSelection);
                }
                else if (zoneMovingH != null)
                {
                    DrawImageInRectangle(gBmp, Resources.GridH, zoneMovingH.RectangleSelection, component.ColorIndex, true, true);

                    gBmp.DrawRectangle(pen, zoneMovingH.RectangleSelection);
                }
                #endregion

                #region Platform
                if (platform != null)
                {
                    gBmp.DrawLine(pen, platform.Location.X + Common.Delta.X, platform.Location.Y + Common.Delta.Y, platform.Location.X + Common.Delta.X, platform.Location.Y + platform.Height + Common.Delta.Y);
                    gBmp.DrawLine(pen, platform.Location.X - 1 + Common.Delta.X, platform.Location.Y + platform.Height + Common.Delta.Y, platform.Location.X + platform.Width + 1 + Common.Delta.X, platform.Location.Y + platform.Height + Common.Delta.Y);
                    gBmp.DrawLine(pen, platform.Location.X + platform.Width + Common.Delta.X, platform.Location.Y + Common.Delta.Y, platform.Location.X + platform.Width + Common.Delta.X, platform.Location.Y + platform.Height + Common.Delta.Y);
                }
                #endregion

                #region Sensor
                if (sensor != null)
                {
                    Image sensorImage = null;

                    switch (sensor.SensorType)
                    {
                        case SensorType.Button:
                            sensorImage = Resources.Icon_SensorButton;
                            break;
                        case SensorType.Camera:
                            sensorImage = Resources.Icon_SensorCamera;
                            break;
                        case SensorType.Nearness:
                            sensorImage = Resources.Icon_SensorNearness;
                            break;
                        case SensorType.RemoteControl:
                            sensorImage = Resources.Icon_SensorRemoteControl;
                            break;
                        default:
                            break;
                    }

                    DrawImageInRectangle(gBmp, sensorImage, sensor.RectangleSelection, component.ColorIndex, true, true);
                }
                #endregion

                #region Hole
                if (hole != null)
                {
                    brush = new SolidBrush(Color.FromArgb(40, 40, 40));
                    gBmp.FillRectangle(brush, hole.RectangleSelection);
                }
                #endregion
            }

            ComponentBase componentSelected = null;

            if (nearestCuboid == null && curComponent != null)
                componentSelected = curComponent;
            else if (nearestCuboid != null)
                componentSelected = nearestCuboid;

            if (componentSelected != null && componentSelected.ModeSelection != ModeSelection.None)
            {
                pen.Color = Color.White;
                pen.DashStyle = DashStyle.Dot;
                pen.Width = 3f;

                gBmp.DrawRectangle(pen, componentSelected.RectangleSelection);
            }

            g.DrawImage(bmp, 0, 0);
        }

        private void DrawImageInRectangle(Graphics g, Image image, Rectangle rectangle, int indexColor, bool useScrollingH, bool useScrollingV)
        {
            float[][] colorMatrixElements = GetScalingColorFromIndex(indexColor);

            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);

            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(
               colorMatrix,
               ColorMatrixFlag.Default,
               ColorAdjustType.Bitmap);

            for (int i = 0; i < (int)Math.Ceiling((double)rectangle.Width / (double)image.Width); i++)
            {
                for (int j = 0; j < (int)Math.Ceiling((double)rectangle.Height / (double)image.Height); j++)
                {
                    Rectangle rec = new Rectangle(rectangle.X + i * image.Width, rectangle.Y + j * image.Height, Math.Min(image.Width, rectangle.Width), Math.Min(image.Height, rectangle.Height));

                    if ((i + 1) * rec.Width > rectangle.Width)
                        rec.Width -= (i + 1) * rec.Width - rectangle.Width;

                    if ((j + 1) * rec.Height > rectangle.Height)
                        rec.Height -= (j + 1) * rec.Height - rectangle.Height;

                    g.DrawImage(image, rec, 0f, 0f, rec.Width, rec.Height, GraphicsUnit.Pixel, imageAttributes);
                }
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            //---> Suppression du cuboid courant
            if (e.KeyCode == Keys.Delete && curComponent != null)
            {
                this.scene.listComponent.Remove(curComponent);
                curComponent = null;
                DrawScene();
            }
            else if (e.KeyCode == Keys.X && nearestCuboid != null)
            {
                curLevelSearch++;

                pic_MouseMove(null, new MouseEventArgs(System.Windows.Forms.MouseButtons.None, 0, pointMouse.X, pointMouse.Y, 0));
            }
            else if (e.KeyCode == Keys.W && nearestCuboid != null)
            {
                if (curLevelSearch > 0)
                    curLevelSearch--;

                pic_MouseMove(null, new MouseEventArgs(System.Windows.Forms.MouseButtons.None, 0, pointMouse.X, pointMouse.Y, 0));
            }
        }

        private void UncheckAllButtons()
        {
            btnFolding.Checked = false;
            btnLink.Checked = false;
            btnMovingZoneH.Checked = false;
            btnMovingzoneV.Checked = false;
            btnPlatform.Checked = false;
            btnSensorButton.Checked = false;
            btnSensorCamera.Checked = false;
            btnSensorNearness.Checked = false;
            btnSensorRemoteControl.Checked = false;
            btnZoneFoldingV.Checked = false;
            btnZoneFoldingH.Checked = false;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            ToolStripButton toolStripItem = ((ToolStripButton)sender);
            bool isChecked = toolStripItem.Checked;
            UncheckAllButtons();

            toolStripItem.Checked = isChecked;

            if (isChecked)
            {
                Common.CurrentTool = ((Tools)toolStripItem.Tag);
            }
            else
            {
                Common.CurrentTool = Tools.None;
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            //---> RAZ de la scène
            scene.listComponent = new List<ComponentBase>();
            DrawScene();
        }

        private void btnOuvrir_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "Papier (*.ppr)|*.ppr";

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Scene), new Type[] 
                    { 
                        typeof(ComponentBase), 
                        typeof(Folding),
                        typeof(Link), 
                        typeof(Platform), 
                        typeof(Sensor), 
                        typeof(ZoneFoldingH), 
                        typeof(ZoneFoldingV), 
                        typeof(ZoneMovingH) ,
                        typeof(ZoneMovingV),
                        typeof(Hole)
                    });

                    XmlReader reader = new XmlTextReader(dlg.FileName);

                    scene = (Scene)serializer.Deserialize(reader);
                    reader.Close();

                    CalcFoldingIntersections();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnEnregistrer_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = "Papier (*.ppr)|*.ppr|Papier FBX (*.fbx)|*.fbx|Collada (*.dae)|*.dae";

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string extension = Path.GetExtension(dlg.FileName).ToUpper();

                    if (extension == ".PPR")
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(Scene), new Type[] 
                    { 
                        typeof(ComponentBase), 
                        typeof(Folding), 
                        typeof(Link), 
                        typeof(Platform), 
                        typeof(Sensor), 
                        typeof(ZoneFoldingH), 
                        typeof(ZoneFoldingV), 
                        typeof(ZoneMovingH) , 
                        typeof(ZoneMovingV),
                        typeof(Hole)
                    });

                        XmlWriter writer = new XmlTextWriter(dlg.FileName, Encoding.UTF8);
                        serializer.Serialize(writer, scene);
                        writer.Close();
                    }
                    else if (extension == ".FBX")
                    {
                        Utils.ExportToFBX(scene, dlg.FileName);
                    }
                    else if (extension == ".DAE")
                    {
                        IntersectSceneBackground();
                        Utils.ExportToCOLLADA2(scene, dlg.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Common.CurrentColorIndex = toolStripComboBox1.SelectedIndex + 1;

            ComponentBase component = null;

            if (nearestCuboid == null && curComponent != null)
                component = curComponent;
            else if (nearestCuboid != null)
                component = nearestCuboid;

            if (component != null)
            {
                component.ColorIndex = Common.CurrentColorIndex;
                DrawScene();
            }
        }

        private void vScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            Common.Delta = new Point(-hScrollBar.Value, valMax - vScrollBar.Value + Common.lineMidScreen.P1.Y);
            DrawScene();
        }

        private void hScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            Common.Delta = new Point(-hScrollBar.Value, valMax - vScrollBar.Value + Common.lineMidScreen.P1.Y);
            DrawScene();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (Environment.MachineName.StartsWith("P64"))
                this.Location = new Point(1500, 100);

            Form1_Resize(null, null);
        }

        private void btnGrid_Click(object sender, EventArgs e)
        {
            txtGridWidth.Enabled = btnGrid.Checked;
            txtGridHeight.Enabled = btnGrid.Checked;
        }
    }
}
