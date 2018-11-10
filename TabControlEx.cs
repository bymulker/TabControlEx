using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms.VisualStyles;

namespace ToolbarEx
{
    [Flags]
    public enum TabExMode
    {
        TM_IDLE,
        TM_HOT,
        TM_TEXTEDIT,
        TM_DRAG
    };
       
    public partial class TabControlEx: TabControl
    {
        public event TabPageRenamingEventHandler TabPageRenaming;

        private TabExMode mode = TabExMode.TM_IDLE;

        public TabExMode Mode { get { return mode; } }
               
        private int activeIx = -1;
             
        public TabControlEx()
        {
            InitializeComponent();            

            //this.DrawMode = TabDrawMode.OwnerDrawFixed;

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            
        }

        public bool isTabPageEx(int index)
        {
            //string typeName = this.TabPages[index].GetType().Name;
            if (index < 0)
                return false;
            return this.TabPages[index] is TabPageEx;
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {           
            if(e.Control is TabPageEx)
            {
                TabPageEx page = e.Control as TabPageEx;
                page.PageRenaming += Page_PageRenaming;
                page.EditStarted += Page_EditStarted;
                page.EditFinished += Page_EditFinished;
            }
           
            base.OnControlAdded(e);
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            if (e.Control is TabPageEx)
            {
                TabPageEx page = e.Control as TabPageEx;
                page.PageRenaming -= Page_PageRenaming;
                page.EditStarted -= Page_EditStarted;
                page.EditFinished -= Page_EditFinished;
            }
            
            base.OnControlRemoved(e);
        }
        
        private void Page_EditFinished(object sender, EventArgs e)
        {
            mode = TabExMode.TM_IDLE;
            this.Cursor = Cursors.Default;
        }

        private void Page_EditStarted(object sender, EventArgs e)
        {
            mode = TabExMode.TM_TEXTEDIT;
            this.Cursor = Cursors.IBeam;
        }
       
        private void Page_PageRenaming(TabPageEx page, ref bool cancel, ref TabPageRenamingEventArgs args)
        {
            TabPageRenaming?.Invoke(page, ref cancel, ref args);            
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            DrawControl(e.Graphics);
            base.OnPaint(e);            
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            activeIx = getActiveIndex();

            if(mode == TabExMode.TM_IDLE)
            {
                mode = TabExMode.TM_HOT;
            }

            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            activeIx = -1;
            
            if(mode == TabExMode.TM_HOT)
            {
                mode = TabExMode.TM_IDLE;
            }

            if (activeIx >= 0 && mode != TabExMode.TM_TEXTEDIT)
            {
                activeIx = -1;               
            }

            Invalidate();

            base.OnMouseLeave(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            activeIx = getActiveIndex();

            if (activeIx >= 0 && isTabPageEx(activeIx))
            {
                TabPageEx tab = this.TabPages[activeIx] as TabPageEx;
                tab.MouseIsMoving(e);
               
            }

            Invalidate();

        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (activeIx >= 0 && isTabPageEx(activeIx))
            {
                TabPageEx tab = this.TabPages[activeIx] as TabPageEx;
                tab.MouseIsDown(e);
                Invalidate();
            }

            base.OnMouseDown(e);
        }   

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if(mode == TabExMode.TM_HOT || mode == TabExMode.TM_IDLE)
            {
                if (activeIx >= 0 && isTabPageEx(activeIx))
                {
                    TabPageEx tab = this.TabPages[activeIx] as TabPageEx;
                    tab.StartEditMode();
                    Invalidate();
                    return;
                }
            }
            else if(mode == TabExMode.TM_TEXTEDIT)
            {
                if (activeIx >= 0 && isTabPageEx(activeIx))
                {
                    TabPageEx tab = this.TabPages[activeIx] as TabPageEx;
                    tab.MouseIsDoubleClicked(e);
                    Invalidate();
                    return;
                }                
            }

            base.OnMouseDoubleClick(e);
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            e.IsInputKey = true;
            base.OnPreviewKeyDown(e);
        }

        protected override void OnKeyDown(KeyEventArgs ke)
        {
            if (mode == TabExMode.TM_TEXTEDIT)
            {
                TabPageEx tab = this.TabPages[SelectedIndex] as TabPageEx;
                tab.KeyIsDown(ke);
                Invalidate();
            }

            base.OnKeyDown(ke);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (mode == TabExMode.TM_TEXTEDIT)
            {
                TabPageEx tab = this.TabPages[SelectedIndex] as TabPageEx;
                tab.KeyIsPressed(e);
                Invalidate();
            }

            base.OnKeyPress(e);
        }
                
        protected void DrawControl(Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (!this.Visible)
                return;

            for (int i = (TabCount-1); i > (-1) ; i--)
            {
                if(i != this.SelectedIndex)
                    DrawTab(g, TabPages[i], i);
            }

            if(SelectedIndex >= 0 && SelectedIndex < this.TabCount)
                DrawTab(g, SelectedTab, SelectedIndex);

        }

        protected void DrawTab(Graphics g, TabPage tabPage, int nIndex)
        {
            if(tabPage is TabPageEx)
            {
                TabPageEx tab = tabPage as TabPageEx;

                tab.Draw(g, nIndex);
            } 
            else
            {
                _drawTab(g, nIndex);
            }                

        }           

        protected override bool ProcessDialogChar(char charCode)
        {
            foreach (TabPage page in this.TabPages)
            {
                if(Control.IsMnemonic(charCode, page.Text))
                {
                    this.SelectedTab = page;
                    this.Focus();
                    return true;
                }
            }

            return base.ProcessDialogChar(charCode);
        }
              
        public new Point MousePosition
        {
            get
            {
                Point loc = this.PointToClient(Control.MousePosition);
                if (this.RightToLeftLayout)
                {
                    loc.X = (this.Width - loc.X);
                }
                return loc;
            }
        }

        protected int getActiveIndex()
        {
            Rectangle rect;
            TabPage pg;

            Point mpt = this.MousePosition;
            for (int i = 0; i < this.TabCount; i++)
            {
                pg = this.TabPages[i];
                if (pg.Enabled)
                {
                    rect = GetTabRect(i);
                    if (rect.Contains(mpt))
                        return i;
                }

            }

            return -1;
        }

        [Browsable(false)]
        public int ActiveIndex
        {
            get
            {
                return activeIx;                            
            }
        }

        public  void _drawTab(Graphics g, int index)
        {
            TabPage tab = this.TabPages[index];

            Rectangle recBounds = getTabBounds(index);
            RectangleF tabTextArea = (RectangleF)recBounds;
           
            Brush fillBr = getTabBackgroundBrush(index, recBounds);

            GraphicsPath pthTab = new GraphicsPath();
                        
            getTabShapePath(index, pthTab, recBounds);

            g.DrawPath(Pens.Black, pthTab);

            g.FillPath(fillBr, pthTab);

            TextRenderer.DrawText(g, tab.Text,
                   tab.Font, recBounds, Color.Black);

            fillBr.Dispose();
            pthTab.Dispose();
        }

        public void getTabShapePath(int index, GraphicsPath path, Rectangle tabBounds)
        {

            int spread;
            int eigth;
            int sixth;
            int quarter;

            int leftOffY = index !=this.SelectedIndex ? 2 : 0;           

            if (this.Alignment <= TabAlignment.Bottom)
            {
                spread = (int)Math.Floor((decimal)tabBounds.Height * 2 / 3);
                eigth = (int)Math.Floor((decimal)tabBounds.Height * 1 / 8);
                sixth = (int)Math.Floor((decimal)tabBounds.Height * 1 / 6);
                quarter = (int)Math.Floor((decimal)tabBounds.Height * 1 / 4);
            }
            else
            {
                spread = (int)Math.Floor((decimal)tabBounds.Width * 2 / 3);
                eigth = (int)Math.Floor((decimal)tabBounds.Width * 1 / 8);
                sixth = (int)Math.Floor((decimal)tabBounds.Width * 1 / 6);
                quarter = (int)Math.Floor((decimal)tabBounds.Width * 1 / 4);
            }

            switch (this.Alignment)
            {
                case TabAlignment.Top:

                    path.AddCurve(new Point[] {  new Point(tabBounds.X, tabBounds.Bottom-leftOffY)
                                          ,new Point(tabBounds.X + sixth, tabBounds.Bottom - eigth)
                                          ,new Point(tabBounds.X + spread - quarter, tabBounds.Y + eigth)
                                          ,new Point(tabBounds.X + spread, tabBounds.Y)});
                    path.AddLine(tabBounds.X + spread, tabBounds.Y, tabBounds.Right - spread, tabBounds.Y);
                    path.AddCurve(new Point[] {  new Point(tabBounds.Right - spread, tabBounds.Y)
                                          ,new Point(tabBounds.Right - spread + quarter, tabBounds.Y + eigth)
                                          ,new Point(tabBounds.Right - sixth, tabBounds.Bottom - eigth)
                                          ,new Point(tabBounds.Right, tabBounds.Bottom+leftOffY)});
                    break;
                case TabAlignment.Bottom:
                    path.AddCurve(new Point[] {  new Point(tabBounds.Right, tabBounds.Y)
                                          ,new Point(tabBounds.Right - sixth, tabBounds.Y + eigth)
                                          ,new Point(tabBounds.Right - spread + quarter, tabBounds.Bottom - eigth)
                                          ,new Point(tabBounds.Right - spread, tabBounds.Bottom)});
                    path.AddLine(tabBounds.Right - spread, tabBounds.Bottom, tabBounds.X + spread, tabBounds.Bottom);
                    path.AddCurve(new Point[] {  new Point(tabBounds.X + spread, tabBounds.Bottom)
                                          ,new Point(tabBounds.X + spread - quarter, tabBounds.Bottom - eigth)
                                          ,new Point(tabBounds.X + sixth, tabBounds.Y + eigth)
                                          ,new Point(tabBounds.X, tabBounds.Y+leftOffY)});
                    break;
                case TabAlignment.Left:
                    path.AddCurve(new Point[] {  new Point(tabBounds.Right, tabBounds.Bottom)
                                          ,new Point(tabBounds.Right - eigth, tabBounds.Bottom - sixth)
                                          ,new Point(tabBounds.X + eigth, tabBounds.Bottom - spread + quarter)
                                          ,new Point(tabBounds.X, tabBounds.Bottom - spread)});
                    path.AddLine(tabBounds.X, tabBounds.Bottom - spread, tabBounds.X, tabBounds.Y + spread);
                    path.AddCurve(new Point[] {  new Point(tabBounds.X, tabBounds.Y + spread)
                                          ,new Point(tabBounds.X + eigth, tabBounds.Y + spread - quarter)
                                          ,new Point(tabBounds.Right - eigth, tabBounds.Y + sixth)
                                          ,new Point(tabBounds.Right, tabBounds.Y)});

                    break;
                case TabAlignment.Right:
                    path.AddCurve(new Point[] {  new Point(tabBounds.X, tabBounds.Y)
                                          ,new Point(tabBounds.X + eigth, tabBounds.Y + sixth)
                                          ,new Point(tabBounds.Right - eigth, tabBounds.Y + spread - quarter)
                                          ,new Point(tabBounds.Right, tabBounds.Y + spread)});
                    path.AddLine(tabBounds.Right, tabBounds.Y + spread, tabBounds.Right, tabBounds.Bottom - spread);
                    path.AddCurve(new Point[] {  new Point(tabBounds.Right, tabBounds.Bottom - spread)
                                          ,new Point(tabBounds.Right - eigth, tabBounds.Bottom - spread + quarter)
                                          ,new Point(tabBounds.X + eigth, tabBounds.Bottom - sixth)
                                          ,new Point(tabBounds.X, tabBounds.Bottom)});
                    break;
            }
        }

        public virtual Brush getTabBackgroundBrush(int index, Rectangle tabBounds)
        {            
            LinearGradientBrush fillBrush = null;

            //	Capture the colours dependant on selection state of the tab
            Color dark, light;

            if (index == this.ActiveIndex)
            {
                dark = Color.FromArgb(167, 217, 245);
                light = Color.FromArgb(234, 246, 253);
            }
            else
            {
                dark = SystemColors.ControlLight;
                light = SystemColors.Window;
            }

            fillBrush = new LinearGradientBrush(tabBounds, light, dark, LinearGradientMode.Vertical);

            return fillBrush;

        }

        public Rectangle getTabBounds(int index)
        {
            Point offset = Point.Empty;
            int w = 10;

            Rectangle rect = GetTabRect(index);

            if(index == 0)
            {
                rect.Width += w;
            }
            else
            {
                rect.Offset(new Point(-w/2, 0));
                rect.Width += w;
            }

            if(index != SelectedIndex)
            {
                rect.Offset(0, 1);
                rect.Inflate(0, -2);
            }

            rect.Height += 1;

            return rect;

        }

    }
}
