using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

 /* Copyright (c) 2008,2009 DI Zimmermann Stephan (stefan.zimmermann@tele2.at)
 *   
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated documentation files (the "Software"), to 
 * deal in the Software without restriction, including without limitation the 
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or 
 * sell copies of the Software, and to permit persons to whom the Software is 
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in 
 * all copies or substantial portions of the Software. 
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
 * THE SOFTWARE.
 */

namespace GraphLib
{
    public partial class PlotterGraphPaneEx : UserControl
    {
        //private static ILogger m_logger = MyLogFactory.GetInstance(LoggerType.Log4Net).GetLogger(System.Reflection.Assembly.GetExecutingAssembly(), typeof(PlotterGraphPaneEx));
        #region MEMBERS

        public enum LayoutMode
        {
            NORMAL,
            STACKED,
            VERTICAL_ARRANGED,
            TILES_VER,
            TILES_HOR,
        }

        private BackBuffer memGraphics;
        private int ActiveSources = 0;

        public LayoutMode layout = LayoutMode.NORMAL;
        public int Xco_ordForMiddleLine = 0;
        public Color MajorGridColor = Color.DarkGray;
        public Color MinorGridColor = Color.DarkGray;
        public double maxY = int.MinValue;
        public double minY = int.MaxValue;
        public Color GraphColor = Color.DarkGreen;
        public Color BgndColorTop = Color.White;
        public Color BgndColorBot = Color.White;
        public Color LabelColor = Color.White;
        public Color GraphBoxColor = Color.Black;
        public bool useDoubleBuffer = false;
        public Font legendFont = new Font(FontFamily.GenericSansSerif, 8.25f);
        public string XaxisName { get; set; }
        public string YaxisName { get; set; }
        private List<DataSource> sources = new List<DataSource>();

        public SmoothingMode smoothing = SmoothingMode.None;

        public bool hasMovingGrid = true;
        public bool hasBoundingBox = true;

        private Point mousePos = new Point();
        private bool mouseDown = false;

        public float scroll = ScrollBarMax;
        public double starting_idx = 0;
        public double XD0 = -50;
        public double XD1 = 100;
        public double DX = 0;
        public double off_X = 0;
        public double CurXD0 = 0;
        public double CurXD1 = 0;

        public double grid_distance_x = 200;       // grid distance in samples ( draw a vertical line every 200 samples )
        public double grid_distance_y = 10;
        public double grid_off_x = 0;
        public double GraphCaptionLineHeight = 28;

        public double pad_inter = 4;         // padding between graphs
        public double pad_left = 29;         // left padding
        public double pad_right = 1;        // right padding
        public double pad_top = 1;          // top
        public double pad_bot = 1;          // bottom padding
        public double pad_label = 1;        // y-label area width
        public double pad_xlabel = 1;        // x-label padding ( bottom area left and right were x labels are still visible )

        public float[] MinorGridPattern = new float[] { 2, 4 };
        public float[] MajorGridPattern = new float[] { 2, 2 };

        DashStyle MinorGridDashStyle = DashStyle.Custom;
        DashStyle MajorGridDashStyle = DashStyle.Custom;
        public bool MoveMinorGrid = true;
        public const int ScrollBarMax = 10000;
        double Min_X = 0;
        const float MAX_X = 9999999;
        private object obj = new object();

        #endregion

        #region CONSTRUCTOR
        public PlotterGraphPaneEx()
        {
            memGraphics = new BackBuffer();
            EnableDrawLegend = true;
            InitializeComponent();

            //this.Resize += new System.EventHandler(this.OnResizeForm);
            this.MouseDown += new MouseEventHandler(OnMouseDown);
            this.MouseUp += new MouseEventHandler(OnMouseUp);
            this.MouseMove += new MouseEventHandler(OnMouseMove);
        }

        #endregion
        public bool EnableDrawLegend { get;set;}
        public List<DataSource> Sources
        {
            get
            {
                return sources;
            }
        }

        /// <summary>
        /// To remove screen flickering
        /// </summary>
        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        CreateParams cp = base.CreateParams;
        //        cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
        //        return cp;
        //    }
        //}

        private void OnLoadControl(object sender, EventArgs e)
        {
            memGraphics.Init(this.CreateGraphics(), this.ClientRectangle.Width, this.ClientRectangle.Height);
        }
        
        //protected override void OnPaintBackground(PaintEventArgs e)
        //{
        //    if (ParentForm == null)
        //    {
        //        // paint background when control is used in editor
        //        base.OnPaintBackground(e);
        //    }
        //}

        

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            //if (mouseDown == true)
            //{
            //    mouseDown = false;
            //    Cursor = Cursors.Default;
            //}
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            //if (mouseDown == false)
            //{
            //    mouseDown = true;
            //    mousePos = e.Location;
            //    Cursor = Cursors.Hand;
            //}
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            //if (mouseDown)
            //{
            //    float dx = mousePos.X - e.Location.X;
            //    mousePos = e.Location;

            //    double DX = CurXD1 - CurXD0;

            //    double off_X = dx * DX / this.Width;

            //    if (Math.Abs(off_X) > 0)
            //    {
            //        starting_idx += off_X;
            //    }

            //    Invalidate();
            //}
        }
        public bool isPaintinginProgress;
        
        public void PaintGraphs(Graphics CurGraphics, double CurWidth, double CurHeight, double OFFX, double OFFY)
        {
            isPaintinginProgress = true;
            try
            {


                int CurGraphIdx = 0;
                int VertTileCount = 1;
                int HorTileCount = 1;

                double curOffY = 0;
                double CurOffX = 0;
                Min_X = MAX_X;
                if ((layout == LayoutMode.TILES_VER || layout == LayoutMode.TILES_HOR) && ActiveSources >= 1)
                {
                    // calculate number of tiles                                   
                    if (layout == LayoutMode.TILES_VER)
                    {
                        VertTileCount = 1;
                        HorTileCount = 1;
                        while (true)
                        {
                            if (VertTileCount * HorTileCount >= ActiveSources) break;
                            VertTileCount++;
                            if (VertTileCount * HorTileCount >= ActiveSources) break;
                            HorTileCount++;
                        }
                    }
                    else if (layout == LayoutMode.TILES_HOR)
                    {
                        VertTileCount = 1;
                        HorTileCount = 1;
                        while (true)
                        {
                            if (VertTileCount * HorTileCount >= ActiveSources) break;
                            HorTileCount++;
                            if (VertTileCount * HorTileCount >= ActiveSources) break;
                            VertTileCount++;
                        }
                    }
                }


                foreach (DataSource source in sources)
                {
                    source.CurGraphHeight = CurHeight;
                    source.CurGraphWidth = CurWidth - pad_left /*- pad_label -*/ - pad_right;
                    CurOffX = OFFX + pad_left + pad_label;// *ActiveSources;
                    curOffY = OFFY + pad_top;
                    double tmpmaxY = source.YMax;
                    double tmpminY = source.YMin;
                    if (source.yFlip)
                    {
                        DX = XD1 - XD0;
                    }
                    else
                    {
                        DX = XD1 - XD0;
                    }
                    GetRange(source, CurOffX, curOffY, ref tmpmaxY, ref tmpminY);
                    if (tmpmaxY == int.MinValue && tmpminY == int.MaxValue)
                        return;
                    if (source.AutoScaleY)
                    {
                        source.YD1 = tmpmaxY;
                        source.YD0 = tmpminY;
                    }
                    
                }

                foreach (DataSource source in sources)
                {
                    if (source.YD1 != int.MinValue && source.YD1 > maxY)
                    {
                        //maxY = source.YD1;
                    }
                    if (source.YD0 != int.MaxValue && source.YD0 < minY)
                    {
                        //minY = source.YD0;
                    }
                }
                if (maxY == int.MinValue)
                {
                    maxY = 0;
                    minY = 10;
                }
                int diff = (int)((maxY - minY) / grid_distance_y);
                if (diff <= 0)
                    diff = 1;
                if (diff != (float)((int)diff))
                    diff = (int)diff + 1;

                //minY = (int)(minY - diff);
                ////TODO:[MAITREYEE] chek for eliminating -ve values. remove
                ////if (minY < 0)
                ////    minY = 0;
                //if (maxY <= minY)
                //{
                //    maxY = maxY + diff + diff;
                //}
                //else
                //{
                //    maxY = maxY + diff;
                //}
                foreach (DataSource source in sources)
                {
                    //source.Cur_YD0 = source.YD0;
                    //source.Cur_YD1 = source.YD1;
                    source.Cur_YD0 = minY;
                    source.Cur_YD1 = maxY;
                    //source.YD0 = minY;
                    //source.YD1 = maxY;
                    source.DY = diff;
                    source.grid_distance_y = diff;
                    source.grid_off_y = minY;

                    CurXD0 = XD0;
                    CurXD1 = XD1;

                    if (source.AutoScaleX && source.Samples.Length > 0)
                    {
                        CurXD0 = source.XMin - source.XAutoScaleOffset;
                        CurXD1 = source.XMax + source.XAutoScaleOffset;
                        DX = CurXD1 - CurXD0;
                    }
                    if (source.Active)
                    {
                        int maxYCordinate = (int)(1 + source.CurGraphHeight + GraphCaptionLineHeight / 2 + 0.5f);

                        if (CurGraphIdx == 0)
                        {
                        }
                        if (source.AutoScaleY == true)
                        {
                            int idx_start = -1;
                            int idx_stop = -1;
                            double ymin = 0.0f;
                            double ymax = 0.0f;
                            double ymin_range = 0;
                            double ymax_range = 0;

                            int DownSample = source.Downsampling;
                            cPoint[] data = source.Samples;
                            double mult_y = source.CurGraphHeight / source.DY;
                            double mult_x = (source.CurGraphWidth - pad_label - pad_right - 1) / (DX);
                            if ((source.CurGraphWidth - pad_label - pad_right - 1) % DX != 0 && (source.CurGraphWidth - pad_label - pad_right - 1) % DX < (source.CurGraphWidth - pad_label - pad_right - 1) / DX / 2)
                            {
                                //mult_x = (source.CurGraphWidth - pad_label + pad_right - 1) / (DX + grid_distance_x);
                            }
                            double coff_x = off_X - starting_idx * mult_x;

                            if (source.AutoScaleX)
                            {
                                coff_x = off_X;
                            }

                            for (int i = 0; i < data.Length - 1; i += DownSample)
                            {
                                double x = data[i].x * mult_x + coff_x;

                                if (data[i].y > ymax) ymax = data[i].y;
                                if (data[i].y < ymin) ymin = data[i].y;

                                if (x > 0 && x < (source.CurGraphWidth))
                                {
                                    if (idx_start == -1) idx_start = i;
                                    idx_stop = i;

                                    if (data[i].y > ymax_range) ymax_range = data[i].y;
                                    if (data[i].y < ymin_range) ymin_range = data[i].y;
                                }
                            }

                            if (idx_start >= 0 && idx_stop >= 0)
                            {
                                double data_range = ymax - ymin;              // this is range in the data
                                double delta_range = ymax_range - ymin_range; // this is the visible data range -> might be smaller

                                source.Cur_YD0 = ymin_range;
                                source.Cur_YD1 = ymax_range;
                            }
                        }
                        //Autoscale Y End 
                        if (layout == LayoutMode.VERTICAL_ARRANGED && ActiveSources >= 1)
                        {
                            if (ActiveSources > 1)
                            {
                                source.CurGraphHeight = (float)(CurHeight - pad_top - pad_bot) / ActiveSources - GraphCaptionLineHeight;
                                double Diff = ((ActiveSources - 1) * pad_inter) / ActiveSources;
                                source.CurGraphHeight -= Diff;
                            }
                            else
                            {
                                source.CurGraphHeight = (float)(CurHeight - pad_top - pad_bot) - GraphCaptionLineHeight;
                            }
                        }
                        else if (layout == LayoutMode.STACKED && ActiveSources >= 1)
                        {
                            if (ActiveSources > 1)
                            {
                                source.CurGraphHeight = (float)(CurHeight - pad_top - pad_bot) / ActiveSources - GraphCaptionLineHeight;
                            }
                            else
                            {
                                source.CurGraphHeight = (float)(CurHeight - pad_top - pad_bot) - GraphCaptionLineHeight;
                            }
                        }
                        else if ((layout == LayoutMode.TILES_VER || layout == LayoutMode.TILES_HOR) && ActiveSources >= 1)
                        {
                            if (ActiveSources > 1)
                            {
                                source.CurGraphHeight = (float)(CurHeight - pad_top - pad_bot) / VertTileCount - GraphCaptionLineHeight;
                                double Diff = ((ActiveSources - 1) * pad_inter) / VertTileCount;
                                source.CurGraphHeight -= Diff;
                                source.CurGraphWidth = (float)(CurWidth - pad_left - pad_right) / HorTileCount - pad_label;
                            }
                            else
                            {
                                source.CurGraphHeight = (float)(CurHeight - pad_top - pad_bot) - GraphCaptionLineHeight;
                            }
                        }
                        else
                        {
                            source.CurGraphHeight = (float)(CurHeight - pad_top - pad_bot) - GraphCaptionLineHeight;
                            //source.CurGraphWidth = CurWidth - pad_left - pad_label /* * ActiveSources */ -  pad_right;
                        }

                        if (source.yFlip)
                        {
                            source.DY = source.Cur_YD0 - source.Cur_YD1;

                            if (DX != 0 && source.DY != 0)
                            {
                                source.off_Y = -source.Cur_YD1 * source.CurGraphHeight / source.DY;
                                off_X = -CurXD0 * source.CurGraphWidth / DX;
                            }
                        }
                        else
                        {
                            source.DY = source.Cur_YD1 - source.Cur_YD0;

                            if (DX != 0 && source.DY != 0)
                            {
                                source.off_Y = -source.Cur_YD0 * source.CurGraphHeight / source.DY;
                                off_X = -CurXD0 * source.CurGraphWidth / DX;
                            }
                        }

                        if ((layout == LayoutMode.TILES_VER || layout == LayoutMode.TILES_HOR))
                        {
                            if (ActiveSources > 1)
                            {
                                if (layout == LayoutMode.TILES_VER)
                                {
                                    // TODO: calc curOffX and CurrOffY for CurGraphIdx!!
                                    int CurIdxY = CurGraphIdx % VertTileCount;
                                    int CurIdxX = CurGraphIdx / VertTileCount;

                                    curOffY = OFFY + pad_top + CurIdxY * (source.CurGraphHeight + GraphCaptionLineHeight);
                                    CurOffX = OFFX + pad_label + pad_left + CurIdxX * (pad_label + source.CurGraphWidth);
                                }
                                else
                                {
                                    int CurIdxX = CurGraphIdx % HorTileCount;
                                    int CurIdxY = CurGraphIdx / HorTileCount;

                                    curOffY = OFFY + pad_top + CurIdxY * (source.CurGraphHeight + GraphCaptionLineHeight);
                                    CurOffX = OFFX + pad_label + pad_left + CurIdxX * (pad_label + source.CurGraphWidth);
                                }

                            }
                            else
                            {
                                // one active source
                                curOffY = OFFY + pad_top + CurGraphIdx * (source.CurGraphHeight + GraphCaptionLineHeight);
                                CurOffX = OFFX + pad_left + pad_label;
                            }
                        }
                        else if (layout == LayoutMode.VERTICAL_ARRANGED)
                        {
                            if (ActiveSources > 1)
                            {
                                curOffY = OFFY + pad_top + CurGraphIdx * (source.CurGraphHeight + GraphCaptionLineHeight + pad_inter);
                            }
                            else
                            {
                                curOffY = OFFY + pad_top + CurGraphIdx * (source.CurGraphHeight + GraphCaptionLineHeight);
                            }

                            CurOffX = OFFX + pad_left + pad_label;
                        }
                        else if (layout == LayoutMode.STACKED)
                        {
                            if (ActiveSources > 1)
                            {
                                curOffY = OFFY + pad_top + CurGraphIdx * (source.CurGraphHeight);
                            }
                            else
                            {
                                curOffY = OFFY + pad_top + CurGraphIdx * (source.CurGraphHeight);
                            }

                            CurOffX = OFFX + pad_left + pad_label;
                        }
                        else
                        {
                            CurOffX = OFFX + pad_left + pad_label;// *ActiveSources;
                            curOffY = OFFY + pad_top;
                        }
                        if (layout != LayoutMode.NORMAL || CurGraphIdx == 0)
                        {
                            DrawGrid(CurGraphics, source, (float)CurOffX, (float)(curOffY + GraphCaptionLineHeight / 2));
                        }
                        List<int> marker_pos = DrawGraphCurve(CurGraphics, source, CurOffX, curOffY + GraphCaptionLineHeight / 2);


                        if (layout == LayoutMode.NORMAL)
                        {
                            DrawGraphCaption(CurGraphics, source, marker_pos, (float)(CurOffX + CurGraphIdx * (10 + pad_label)), (float)curOffY);

                            if (CurGraphIdx == 0)
                            {

                                DrawXLabels(CurGraphics, source, marker_pos, (float)CurOffX, (float)curOffY);
                                DrawYLabels(CurGraphics, source, marker_pos, (float)CurOffX /*+ pad_label   * (CurGraphIdx - ActiveSources + 1)*/, (float)curOffY);
                            }
                        }
                        else
                        {
                            DrawGraphCaption(CurGraphics, source, marker_pos, (float)CurOffX, (float)curOffY);

                            DrawXLabels(CurGraphics, source, marker_pos, (float)CurOffX, (float)curOffY);

                            DrawYLabels(CurGraphics, source, marker_pos, (float)CurOffX, (float)curOffY);
                        }
                        if (hasBoundingBox)
                        {
                            DrawGraphBox(CurGraphics, source, (float)CurOffX, (float)curOffY, (float)GraphCaptionLineHeight);
                        }
                        CurGraphIdx++;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                isPaintinginProgress = false;
            }
        }

        private void PaintStackedGraphs(Graphics CurGraphics, float CurWidth, float CurHeigth, float OFFX, float OFFY)
        {
            int CurGraphIdx = 0;
            double curOffY = 0;
            double CurOffX = 0;

            foreach (DataSource source in sources)
            {
                source.Cur_YD0 = source.YD0;
                source.Cur_YD1 = source.YD1;

                source.CurGraphHeight = CurHeigth;
                source.CurGraphWidth = CurWidth - pad_left - pad_label - pad_right;

                DX = XD1 - XD0;

                if (source.AutoScaleX && source.Samples.Length > 0)
                {
                    DX = source.Samples[source.Samples.Length - 1].x;
                }

                CurXD0 = XD0;
                CurXD1 = XD1;

                if (source.Active)
                {
                    if (source.AutoScaleY == true)
                    {
                        int idx_start = -1;
                        int idx_stop = -1;
                        double ymin = 0.0f;
                        double ymax = 0.0f;
                        double ymin_range = 0;
                        double ymax_range = 0;

                        int DownSample = source.Downsampling;
                        cPoint[] data = source.Samples;
                        double mult_y = source.CurGraphHeight / source.DY;
                        double mult_x = (source.CurGraphWidth - pad_label - pad_right - 1) / (DX);
                        if ((source.CurGraphWidth - pad_label - pad_right - 1) % DX != 0 && (source.CurGraphWidth - pad_label - pad_right - 1) % DX < (source.CurGraphWidth - pad_label - pad_right - 1) / DX / 2)
                        {
                            //mult_x = (source.CurGraphWidth - pad_label + pad_right - 1) / (DX + grid_distance_x);
                        }
                        
                        double coff_x = off_X - starting_idx * mult_x;

                        if (source.AutoScaleX)
                        {
                            coff_x = off_X;     // avoid dragging in x-autoscale mode
                        }

                        for (int i = 0; i < data.Length - 1; i += DownSample)
                        {
                            double x = data[i].x * mult_x + coff_x;

                            if (data[i].y > ymax) ymax = data[i].y;
                            if (data[i].y < ymin) ymin = data[i].y;

                            if (x > 0 && x < (source.CurGraphWidth))
                            {
                                if (idx_start == -1) idx_start = i;
                                idx_stop = i;

                                if (data[i].y > ymax_range) ymax_range = data[i].y;
                                if (data[i].y < ymin_range) ymin_range = data[i].y;
                            }
                        }

                        if (idx_start >= 0 && idx_stop >= 0)
                        {
                            double data_range = ymax - ymin;              // this is range in the data
                            double delta_range = ymax_range - ymin_range; // this is the visible data range -> might be smaller

                            source.Cur_YD0 = ymin_range;
                            source.Cur_YD1 = ymax_range;
                        }
                    }

                    if (ActiveSources > 1)
                    {
                        source.CurGraphHeight = (float)(CurHeigth - GraphCaptionLineHeight - pad_top - pad_bot) / ActiveSources;
                    }
                    else
                    {
                        source.CurGraphHeight = (float)(CurHeigth - GraphCaptionLineHeight - pad_top - pad_bot);
                    }

                    if (source.yFlip)
                    {
                        source.DY = source.Cur_YD0 - source.Cur_YD1;

                        if (DX != 0 && source.DY != 0)
                        {
                            source.off_Y = -source.Cur_YD1 * source.CurGraphHeight / source.DY;
                            off_X = -CurXD0 * source.CurGraphWidth / DX;
                        }
                    }
                    else
                    {
                        source.DY = source.Cur_YD1 - source.Cur_YD0;

                        if (DX != 0 && source.DY != 0)
                        {
                            source.off_Y = -source.Cur_YD0 * source.CurGraphHeight / source.DY;
                            off_X = -CurXD0 * source.CurGraphWidth / DX;
                        }
                    }

                    if (ActiveSources > 1)
                    {
                        curOffY = OFFY + pad_top + CurGraphIdx * (source.CurGraphHeight);
                    }
                    else
                    {
                        curOffY = OFFY + pad_top + CurGraphIdx * (source.CurGraphHeight);
                    }

                    CurOffX = OFFX + pad_left + pad_label;

                    DrawGrid(CurGraphics, source, (float)CurOffX, (float)(curOffY + GraphCaptionLineHeight / 2));

                    List<int> marker_pos = DrawGraphCurve(CurGraphics, source, CurOffX, curOffY + GraphCaptionLineHeight / 2);

                    DrawGraphCaption(CurGraphics, source, marker_pos, (float)(CurOffX + CurGraphIdx * (10 + pad_label)), (float)pad_top);

                    DrawYLabels(CurGraphics, source, marker_pos, (float)CurOffX, (float)curOffY);

                    if (hasBoundingBox && CurGraphIdx == ActiveSources - 1)
                    {
                        DrawGraphBox(CurGraphics, (float)source.CurGraphWidth, (float)(CurHeigth - pad_top - GraphCaptionLineHeight), (float)(pad_left + pad_label), (float)pad_top, (float)GraphCaptionLineHeight);
                    }

                    if (CurGraphIdx == ActiveSources - 1)
                    {
                        DrawXLabels(CurGraphics, source, marker_pos, (float)pad_left,(float)( Height - pad_top - GraphCaptionLineHeight - source.CurGraphHeight));
                    }

                    CurGraphIdx++;
                }


            }
        }

        public void PaintControl(Graphics CurGraphics, float CurWidth, float CurHeight, float OffX, float OffY, bool PaintBgnd)
        {
            if (PaintBgnd)
            {
                DrawBackground(CurGraphics, CurWidth, CurHeight, OffX, OffY);
            }

            ActiveSources = 0;

            foreach (DataSource source in sources)
            {
                if (source.Samples != null &&
                    source.Samples.Length > 0 &&
                    source.Active == true)
                {
                    ActiveSources++;
                }
            }

            switch (layout)
            {
                case LayoutMode.NORMAL:

                case LayoutMode.TILES_HOR:
                case LayoutMode.TILES_VER:
                case LayoutMode.VERTICAL_ARRANGED:


                    PaintGraphs(CurGraphics, CurWidth, CurHeight, OffX, OffY);

                    break;

                case LayoutMode.STACKED:


                    PaintStackedGraphs(CurGraphics, CurWidth, CurHeight, OffX, OffY);

                    break;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                if (ParentForm != null)
                {
                    Graphics CurGraphics = e.Graphics;

                    if (memGraphics.g != null && useDoubleBuffer == true)
                    {
                        CurGraphics = memGraphics.g;
                    }

                    CurGraphics.SmoothingMode = smoothing;

                    PaintControl(CurGraphics, this.Width, this.Height, 0, 0, true);

                    if (memGraphics.g != null && useDoubleBuffer == true)
                    {
                        //memGraphics.Render(e.Graphics);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write("exception : " + ex.Message);
            }

            base.OnPaint(e);
        }

        private void DrawBackground(Graphics g, float CurWidth, float CurHeight, float CurOFFX, float CurOFFY)
        {
            Rectangle rbgn = new Rectangle((int)CurOFFX, (int)CurOFFY, (int)CurWidth, (int)CurHeight);

            if (BgndColorTop != BgndColorBot)
            {
                
                using (LinearGradientBrush lb1 = new LinearGradientBrush(new Point((int)0, (int)0),
                                                                         new Point((int)0, (int)(CurHeight)),
                                                                         BgndColorTop,
                                                                         BgndColorBot))
                {
                    g.FillRectangle(lb1, rbgn);
                }
            }
            else
            {
                using (SolidBrush sb1 = new SolidBrush(BgndColorTop))
                {
                    g.FillRectangle(sb1, rbgn);
                }
            }
        }

        private void DrawGrid(Graphics g, DataSource source, float CurrOffX, float CurOffY)
        {
            return;
            double Idx = 0;
            //float mult_x = source.CurGraphWidth / DX;
            //double mult_x = source.CurGraphWidth / (DX );
            double mult_x = (source.CurGraphWidth - pad_label - pad_right - 1) / (DX);
            if ((source.CurGraphWidth - pad_label - pad_right - 1) % DX != 0 && (source.CurGraphWidth - pad_label - pad_right - 1) % DX < (source.CurGraphWidth - pad_label - pad_right - 1) / DX / 2)
            {
                //mult_x = (source.CurGraphWidth - pad_label + pad_right - 1) / (DX + grid_distance_x);
            }
                        
            double coff_x = off_X - starting_idx * mult_x;
            if (source.AutoScaleX)
            {
                coff_x = off_X;     // avoid dragging in x-autoscale mode
            }
            Color CurGridColor = MajorGridColor;
            Color CurMinGridClor = MinorGridColor;
            coff_x = 0;
            if (layout == LayoutMode.NORMAL && source.AutoScaleY)
            {
                CurGridColor = source.GraphColor;
                CurMinGridClor = source.GraphColor;
            }

            using (Pen minorGridPen = new Pen(CurMinGridClor))
            {
                minorGridPen.DashPattern = MinorGridPattern;
                minorGridPen.DashStyle = MinorGridDashStyle;


                using (Pen p2 = new Pen(CurGridColor))
                {
                    p2.DashPattern = MajorGridPattern;
                    p2.DashStyle = MajorGridDashStyle;

                    if (DX != 0)
                    {
                        while (true)
                        {
                            double x = Idx * grid_distance_x * mult_x + grid_off_x * mult_x;

                            if (MoveMinorGrid)
                            {
                                x += coff_x;
                            }

                            if (x > 0 && x < source.CurGraphWidth)
                            {
                                g.DrawLine(minorGridPen, new Point((int)(x + CurrOffX - 0.5f), (int)(CurOffY)),
                                                         new Point((int)(x + CurrOffX - 0.5f), (int)(CurOffY + source.CurGraphHeight)));
                            }
                            if (x > source.CurGraphWidth)
                            {
                                break;
                            }

                            Idx++;
                        }
                    }

                    if (source.DY != 0)
                    {
                        double y0 = (source.grid_off_y * source.CurGraphHeight / source.DY + source.off_Y);

                        // draw horizontal zero grid lines
                        g.DrawLine(p2, new Point((int)CurrOffX, (int)(CurOffY + y0 + 0.5f)), new Point((int)(CurrOffX + source.CurGraphWidth + 0.5f), (int)(CurOffY + y0 + 0.5f)));

                        // draw horizontal grid lines
                        for (Idx = (source.grid_off_y); Idx > (source.YD0); Idx -= source.grid_distance_y)
                        {
                            double y = (Idx * source.CurGraphHeight) / source.DY + source.off_Y;

                            if (y >= 0 && y < source.CurGraphHeight)
                            {
                                g.DrawLine(minorGridPen,
                                            new Point((int)CurrOffX, (int)(CurOffY + y + 0.5f)),
                                            new Point((int)(CurrOffX + source.CurGraphWidth + 0.5f), (int)(0.5f + CurOffY + y)));
                            }
                        }

                        // draw horizontal grid lines
                        for (Idx = (source.grid_off_y); Idx < (source.YD1); Idx += source.grid_distance_y)
                        {
                            double y = Idx * source.CurGraphHeight / source.DY + source.off_Y;

                            if (y >= 0 && y < source.CurGraphHeight)
                            {
                                g.DrawLine(minorGridPen,
                                           new Point((int)CurrOffX, (int)(CurOffY + y + 0.5f)),
                                           new Point((int)(CurrOffX + source.CurGraphWidth + 0.5f), (int)(0.5f + CurOffY + y)));
                            }
                        }
                    }
                }
            }
        }

        void GetRange(DataSource source, double offset_x, double offset_y, ref double yMax, ref double yMin)
        {
            if (source.Samples == null)
            {
            }
            int start = 0;

            yMax=int.MinValue;
            yMin=int.MaxValue;
            
            double mult_x = (source.CurGraphWidth - pad_label - pad_right - 1) / (DX);
            cPoint[] data = source.Samples;
            if (data == null)
                return;
            int nopoints = (int)((source.CurGraphWidth - pad_label + pad_right - 1) / mult_x);
            
            if (scroll >= (ScrollBarMax - 10))
            {
                if (nopoints < data.Length)
                {
                    start = data.Length - nopoints;
                    double Maxx = data[data.Length - 1].x;
                    double MinX = Maxx - DX;
                    if (XD0 > MinX)
                        MinX = XD0;
                    while (start >= 1 && (data[start - 1].x) >= MinX)
                    {
                        start--;
                    }
                }
                else
                    start = 0;
            }
            else
            {
                double xRange = data[data.Length - 1].x - data[0].x;
                double startx = data[0].x + scroll * xRange / ScrollBarMax;
                double Maxx = data[data.Length - 1].x;
                double MinX = Maxx - DX;
                if (XD0 > MinX)
                    MinX = XD0;
                if (startx >= MinX)
                {
                    start = (int)data.Length - 1;
                    while (start >= 1 && (data[start - 1].x) >= MinX)
                    {
                        start--;
                    }
                }
                else
                {
                    while (start <= data.Length - 2 && (data[start + 1].x) < startx)
                    {
                        start++;
                    }
                }
            }
            if (source.AutoScaleY)
            {
                for (int counter = start; counter < data.Length; counter++)
                {
                    double x = ((data[counter].x - data[start].x)) /*counter*/ * mult_x;
                    if (x > source.CurGraphWidth)
                    {
                        break;
                    }
                    if (data[counter].y < yMin)
                        yMin = data[counter].y;
                    if (data[counter].y > yMax)
                        yMax = data[counter].y;
                }
            }
            else
            {
                yMax = source.YMax;
                yMin = source.YMin;
            }
            source.StartIndex = start;
        }
        private List<int> DrawGraphCurve(Graphics g, DataSource source, double offset_x, double offset_y)
        {
            List<int> marker_positions = new List<int>();
            int maxYCordinate = (int)(1 + source.CurGraphHeight + GraphCaptionLineHeight / 2 + 0.5f);
            if (DX != 0 && source.DY != 0)
            {
                List<Point> ps = new List<Point>();
                if (source.Samples != null && source.Samples.Length > 1)
                {
                    int DownSample = source.Downsampling;
                    cPoint[] data = source.Samples;
                    double mult_y = source.CurGraphHeight / source.DY;
                    //float mult_x = source.CurGraphWidth / DX;
                    double mult_x = (source.CurGraphWidth - pad_label - pad_right - 1) / (DX);
                    if ((source.CurGraphWidth - pad_label - pad_right - 1) % DX != 0 && (source.CurGraphWidth - pad_label - pad_right - 1) % DX < (source.CurGraphWidth - pad_label - pad_right - 1) / DX / 2)
                    {
                        //mult_x = (source.CurGraphWidth - pad_label + pad_right - 1) / (DX + grid_distance_x);
                    }
                        
                    double coff_x = off_X - starting_idx * mult_x;
                    if (source.AutoScaleX)
                    {
                        coff_x = off_X;     // avoid dragging in x-autoscale mode
                    }
                    else
                        coff_x = 0;
                    
                    int counter = 0;
                    double modulo = 0;
                    
                    Pen pen12 = new Pen(Color.Black,1.0f);
                    g.DrawLine(pen12, new PointF((int)offset_x, (int)(source.off_Y + offset_y)), new PointF((int)(offset_x + source.CurGraphWidth - pad_label + pad_right - 0.5f), (int)(source.off_Y+offset_y)));
                    for (int i =source.StartIndex; i < data.Length; i += DownSample)
                    {
                        if (Min_X >= MAX_X)
                            Min_X = data[i].x;
                        double x = ((data[i].x - Min_X)) /*counter*/ * mult_x + coff_x;
                        double y = data[i].y * mult_y + source.off_Y;
                        double xi = (data[i].x);

                        modulo =(xi % grid_distance_x);

                        if (modulo <= 0.00001 || (xi > grid_distance_x && Convert.ToDecimal(((int)Convert.ToDecimal(xi / grid_distance_x)) * grid_distance_x) == Convert.ToDecimal(xi)))
                        {
                            if (x >= 0 && x <= (source.CurGraphWidth))
                            {
                                marker_positions.Add(i);
                            }
                        }
                        
                        
                        if (x >= 0 && x < (source.CurGraphWidth))
                        {
                            ps.Add(new Point((int)(x + offset_x + 0f), (int)(y + offset_y + 0f)));
                        }
                        else if (x > source.CurGraphWidth)
                        {
                            break;
                        }
                        else
                        {
                        }
                        counter++;
                    }
                    
                    using (Pen pen = new Pen(source.GraphColor,1.0f))
                    {
                        if (ps.Count > 1)
                        {
                            Point[] gPoints = ps.ToArray();
                            for (int iterator = 0; iterator < gPoints.Length - 1; iterator++)
                            {
                                Point point1 = gPoints[iterator];
                                Point point2 = gPoints[iterator + 1];
                                float slope = 0;
                                if ((maxYCordinate < point1.Y))
                                {
                                    if ((point2.X - point1.X) == 0)
                                        slope = 0;
                                    else
                                        slope = (point2.Y - point1.Y) / (point2.X - point1.X);
                                    float c = point2.Y - (slope * point2.X);
                                    float reqX = (maxYCordinate - c) / slope;
                                    point1.X = (int)reqX;
                                    point1.Y = maxYCordinate;
                                }
                                if ((maxYCordinate < point2.Y))
                                {
                                    if ((point2.X - point1.X) == 0)
                                        slope = 0;
                                    else
                                        slope = (point2.Y - point1.Y) / (point2.X - point1.X);
                                    float c = point2.Y - (slope * point2.X);
                                    float reqX = (maxYCordinate - c) / slope;
                                    point2.X = (int)reqX;
                                    point2.Y = maxYCordinate;
                                }
                                if (point1.X < 0 || point2.X < 0)
                                    continue;
                                if (!(maxYCordinate < point1.Y || maxYCordinate < point2.Y))
                                   g.DrawLine(pen, point1, point2);
                            }
                            // g.DrawLines(p, ps.ToArray());
                        }
                    }
                }
            }
            
            return marker_positions;
        }

        private void DrawGraphCaption(Graphics g, DataSource source, List<int> marker_pos, float offset_x, float offset_y)
        {
            if (EnableDrawLegend)
            {
                Color color = this.Enabled ? source.GraphColor : Color.Gray;
                using (Brush b = new SolidBrush(color))
                {
                    
                    using (Pen pen = new Pen(b))
                    {
                        pen.DashPattern = new float[] { 2, 2 };

                        g.DrawString(source.Name, legendFont, b, new PointF(offset_x + 12, offset_y + 2));

                    }
                }
            }
        }

        private void DrawXLabels(Graphics g, DataSource source, List<int> marker_pos, float offset_x, float offset_y)
        {
            lock (obj)
            {
                if (!this.Enabled)
                    return;
                SizeF dim = new SizeF();
                Color XLabColor = source.GraphColor;
                if (layout == LayoutMode.NORMAL || layout == LayoutMode.STACKED)
                {
                    XLabColor = GraphBoxColor;
                }
                //using (Brush b = new SolidBrush(XLabColor))
                Color color = this.Enabled ? Color.Black : Color.Gray;
                using (Brush b = new SolidBrush(color))
                {
                    using (Pen pen = new Pen(b))
                    {
                        pen.DashPattern = new float[] { 2, 2 };
                        using (Font axisFont = new Font(FontFamily.GenericSansSerif, 10.0f, FontStyle.Bold))
                        {
                            dim = g.MeasureString(XaxisName, axisFont);
                            g.DrawString(XaxisName, axisFont, b, new PointF((float)(0.5f + source.CurGraphWidth + pad_left + pad_label - dim.Width) / 2,
                                (float)(this.Height - dim.Height)));
                        }
                        if (DX != 0 && source.DY != 0)
                        {
                            if (source.Samples != null && source.Samples.Length > 1)
                            {
                                cPoint[] data = source.Samples;
                                double mult_y = source.CurGraphHeight / source.DY;
                                double mult_x = (source.CurGraphWidth - pad_label - pad_right - 1) / (DX);
                                if ((source.CurGraphWidth - pad_label - pad_right - 1) % DX != 0 && (source.CurGraphWidth - pad_label - pad_right - 1) % DX < (source.CurGraphWidth - pad_label - pad_right - 1) / DX / 2)
                                {
                                    //mult_x = (source.CurGraphWidth - pad_label + pad_right - 1) / (DX + grid_distance_x);
                                }

                                double coff_x = off_X - starting_idx * mult_x;
                                if (source.AutoScaleX)
                                {
                                    coff_x = off_X;     // avoid dragging in x-autoscale mode
                                }
                                double counter = XD0;
                                coff_x = 0;
                                
                                double xmin = ((int)(Min_X / grid_distance_x)) * grid_distance_x;
                                while (counter<XD1)
                                {
                                    {
                                        double x = ((xmin - Min_X)) /*counter*/ * mult_x + coff_x;
                                        /*if (scroll > 9990)
                                        {
                                        
                                            x = counter * mult_x;
                                        } */
                                        String value = "" + string.Format("{0:0.#}",xmin);
                                        if (source.OnRenderXAxisLabel != null)
                                        {
                                            //value = source.OnRenderXAxisLabel(source, i);
                                            value = xmin.ToString();
                                        }
                                        //Console.WriteLine("Min:" + Min_X + " Value: " + value + " x:" + x + " Counter:" + counter);
                                        if (MoveMinorGrid == false)
                                        {
                                            g.DrawLine(pen, (float)x, (float)(GraphCaptionLineHeight + offset_y + source.CurGraphHeight - 14), (float)x, (float)(GraphCaptionLineHeight + offset_y + source.CurGraphHeight));
                                            g.DrawString(value, legendFont, b, new PointF((int)(0.5f + x + offset_x + 4), (float)(GraphCaptionLineHeight + offset_y + source.CurGraphHeight - 14)));
                                        }
                                        else
                                        {
                                            dim = g.MeasureString(value, legendFont);
                                            if (counter >= 0 && value!="0")
                                                g.DrawString(value, legendFont, b, new PointF((int)(0.5f + x + offset_x - dim.Width / 2), (float)(GraphCaptionLineHeight + offset_y + source.CurGraphHeight - 14)));
                                            else
                                                if (value != "0")
                                                {
                                                    g.DrawString(value, legendFont, b, new PointF((int)(x + offset_x), (float)(GraphCaptionLineHeight + offset_y + source.CurGraphHeight - 14)));
                                                }
                                                    //if (int.Parse(value) == (int)XD1)
                                            //{
                                            //    Xco_ordForMiddleLine = ((int)(x + offset_x));
                                            //}
                                        }
                                    }
                                    xmin += grid_distance_x;
                                    counter += grid_distance_x;
                                }

                                }
#if TEST
                                foreach (int i in marker_pos)
                                {
                                    double xi = (data[i].x);
                                    double modulo = xi % grid_distance_x;
                                    if (modulo <= 0.000001f || (xi / grid_distance_x * grid_distance_x) == xi)
                                    {
                                        double x = data[i].x * mult_x + coff_x;
                                        x = ((data[i].x - Min_X)) /*counter*/ * mult_x + coff_x;
                                        /*if (scroll > 9990)
                                        {
                                        
                                            x = counter * mult_x;
                                        } */
                                        String value = "" + data[i].x;
                                        if (source.OnRenderXAxisLabel != null)
                                        {
                                            value = source.OnRenderXAxisLabel(source, i);
                                        }
                                        Console.WriteLine("Value: " + value + " x:" + x + " grid_distance_x:" + grid_distance_x);
                                        if (MoveMinorGrid == false)
                                        {
                                            g.DrawLine(pen, (float)x, (float)(GraphCaptionLineHeight + offset_y + source.CurGraphHeight - 14), (float)x, (float)(GraphCaptionLineHeight + offset_y + source.CurGraphHeight));
                                            g.DrawString(value, legendFont, b, new PointF((int)(0.5f + x + offset_x + 4), (float)(GraphCaptionLineHeight + offset_y + source.CurGraphHeight - 14)));
                                        }
                                        else
                                        {
                                            dim = g.MeasureString(value, legendFont);
                                            if (counter > 0)
                                                g.DrawString(value, legendFont, b, new PointF((int)(0.5f + x + offset_x - dim.Width / 2), (float)(GraphCaptionLineHeight + offset_y + source.CurGraphHeight - 14)));
                                            else
                                                g.DrawString(value, legendFont, b, new PointF((int)(x + offset_x), (float)(GraphCaptionLineHeight + offset_y + source.CurGraphHeight - 14)));
                                        }
                                        counter++;
                                    }
                                }
                            }
#endif

                        }
                    }
                }

            }
        }

        private void DrawYLabels(Graphics g, DataSource source, List<int> marker_pos, float offset_x, float offset_y)
        {
           
            if (!this.Enabled)
                return;
            SizeF dim = new SizeF();
            //using (Brush b = new SolidBrush(source.GraphColor))
            Color color = this.Enabled ? Color.Black : Color.Gray;
            using (Brush b = new SolidBrush(color))
            {
                using (Pen pen = new Pen(b))
                {
                    pen.DashPattern = new float[] { 2, 2 };
                    StringFormat format = new StringFormat();
                    format.FormatFlags = StringFormatFlags.DirectionVertical;
                    using (Font axisFont = new Font(FontFamily.GenericSansSerif, 11.0f, FontStyle.Bold))
                    {
                        dim = g.MeasureString(YaxisName, axisFont);
                        float ypos=(float)(this.Height - dim.Width-pad_bot-pad_top - 0.5) / 2;
                        if(ypos<0)
                            ypos=0;
                        g.DrawString(YaxisName, axisFont, b, new PointF((float)-0.5,
                           ypos), format);
                    }

                    // draw labels for horizontal lines
                    if (source.DY != 0)
                    {
                        double Idx = 0;
                        //float y0 = (float)(source.grid_off_y * source.CurGraphHeight / source.DY + source.off_Y);
                        //dim = g.MeasureString(source.grid_off_y.ToString(), legendFont);
                        //g.DrawString(source.grid_off_y.ToString(), legendFont, b, new PointF((int)offset_x - dim.Width, (int)(offset_y + y0 + 0.5f + dim.Height / 2)));
                        String value = "" + Idx;

                        if (source.OnRenderYAxisLabel != null)
                        {
                            value = source.OnRenderYAxisLabel(source, Idx);
                        }

                        dim = g.MeasureString(value, legendFont);
                        //g.DrawString(value, legendFont, b, new PointF((int)offset_x - dim.Width, (int)(offset_y + y0 + 0.5f + dim.Height / 2)));

                        double GridDistY = source.grid_distance_y;

                        if (source.AutoScaleY)
                        {
                            // calculate a matching grid distance                            
                            GridDistY = -Utilities.MostSignificantDigit(source.DY);

                            if (GridDistY == 0)
                            {
                                GridDistY = source.grid_distance_y;
                            }
                        }
                        int counter = 0;
                        float yPos = 0f;
                        for (Idx = (source.grid_off_y); Idx > (source.Cur_YD0); Idx -= GridDistY)
                        {
                            //if (Idx != 0)
                            {
                                Idx = Math.Round(Idx,1, MidpointRounding.ToEven);
                                double y1 = (float)((Idx) * source.CurGraphHeight) / source.DY + source.off_Y;

                                value = "" + (Idx);

                                if (source.OnRenderYAxisLabel != null)
                                {
                                    value = source.OnRenderYAxisLabel(source, Idx);
                                }

                                dim = g.MeasureString(value, legendFont);
                                //if (counter == 0)
                                //    yPos = (int)(offset_y + y1 - 0.5f - dim.Height);
                                //else
                                    yPos = (int)(offset_y + y1 - 0.5f + dim.Height / 2);
                                g.DrawString(value, legendFont, b, new PointF((int)offset_x - dim.Width, yPos));
                                counter++;
                            }
                        }

                        for (Idx = (source.grid_off_y); Idx < (source.Cur_YD1); Idx += GridDistY)
                        {
                            //if (Idx != 0)
                            {
                                Idx = Math.Round(Idx, 1, MidpointRounding.ToEven);
                                double y2 = (float)((Idx) * source.CurGraphHeight) / source.DY + source.off_Y;

                                value = "" + (Idx);

                                if (source.OnRenderYAxisLabel != null)
                                {
                                    value = source.OnRenderYAxisLabel(source, Idx);
                                }
                                //if (counter == 0)
                                    //yPos = (int)(offset_y + y2 - 0.5f - dim.Height);
                                //else
                                    yPos = (int)(offset_y + y2 - 0.5f + dim.Height / 2);
                                dim = g.MeasureString(value, legendFont);
                                g.DrawString(value, legendFont, b, new PointF((int)offset_x - dim.Width, (int)yPos));
                                //if (value =="0")
                                //{

                                //    g.DrawLine(pen, new PointF((int)offset_x, (int)yPos), new PointF((int)(offset_x + source.CurGraphWidth - pad_label + pad_right - 0.5f), (int)yPos));
                                //}
                                counter++;
                            }
                        }
                        

                    }
                }
            }
           
           
        }

        private void DrawGraphBox(Graphics g, float w, float h,
                                    float offset_x,
                                     float offset_y,
                                    float GraphCaptionLineHeight)
        {
            using (Pen p2 = new Pen(GraphBoxColor))
            {
                g.DrawLine(p2, new Point((int)(offset_x + 0.5f), (int)(offset_y + 0.5f)),
                              new Point((int)(offset_x + w - 0.5f), (int)(offset_y + 0.5f)));

                g.DrawLine(p2, new Point((int)(offset_x + w - 0.5f), (int)(offset_y + 0.5f)),
                             new Point((int)(offset_x + w - 0.5f), (int)(offset_y + h + 0.5f)));

                g.DrawLine(p2, new Point((int)(offset_x + w - 0.5f), (int)(offset_y + h + 0.5f)),
                           new Point((int)(offset_x + 0.5f), (int)(offset_y + h + 0.5f)));

                g.DrawLine(p2, new Point((int)(offset_x + 0.5f), (int)(offset_y + h + 0.5f)),
                          new Point((int)(offset_x + 0.5f), (int)(offset_y + 0.5f)));
            }
        }

        private void DrawGraphBox(Graphics g, DataSource source,
                                    float offset_x,
                                     float offset_y,
                                    float GraphCaptionLineHeight)
        {
            int maxYCordinate = (int)(offset_y + source.CurGraphHeight + GraphCaptionLineHeight / 2 + 0.5f);
            Color color = this.Enabled ? GraphBoxColor : Color.Gray;
            using (Pen p2 = new Pen(color))
            {
                //offset_x -=30;
                g.DrawLine(p2, new Point((int)(offset_x + 0.5f), (int)(offset_y + 0.5f)),
                              new Point((int)(offset_x + source.CurGraphWidth - pad_label + pad_right - 0.5f), (int)(offset_y + 0.5f)));

                g.DrawLine(p2, new Point((int)(offset_x + source.CurGraphWidth - pad_label + pad_right - 0.5f), (int)(offset_y + 0.5f)),
                             new Point((int)(offset_x + source.CurGraphWidth - pad_label + pad_right - 0.5f), maxYCordinate));

                g.DrawLine(p2, new Point((int)(offset_x + source.CurGraphWidth - pad_label + pad_right - 0.5f), maxYCordinate),
                           new Point((int)(offset_x + 0.5f), maxYCordinate));

                g.DrawLine(p2, new Point((int)(offset_x + 0.5f), maxYCordinate),
                          new Point((int)(offset_x + 0.5f), (int)(offset_y + 0.5f)));
            }
        }
    }
}
