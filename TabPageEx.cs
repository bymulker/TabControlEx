using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ToolbarEx
{
    public class TabPageRenamingEventArgs : EventArgs
    {
        private TabPageEx m_page = null;
        public TabPageEx Page { get { return m_page; } }

        private string m_name = "";
        public string NewName
        {
            get { return m_name; }
            set { m_name = value; }            
        }

        private bool m_isHandled = false;

        public bool IsHandled
        {
            get { return m_isHandled; }
            set { m_isHandled = value; }
        }
        
        private TabPageRenamingEventArgs() { }

        public TabPageRenamingEventArgs(TabPageEx pg, string name) : base()
        {
            m_page = pg;
            m_name = name;
        }

    }

    public delegate void TabPageRenamingEventHandler(TabPageEx page, ref bool cancel, ref TabPageRenamingEventArgs args);
    
    public class TabPageEx : TabPage
    {
        public event TabPageRenamingEventHandler PageRenaming;
        public event EventHandler EditStarted;
        public event EventHandler EditFinished;

        private EditBox m_editBox;

        private ContextMenuStrip m_menu = null;
        public ContextMenuStrip Menu
        {
            get { return m_menu; }
            set { m_menu = value; }
        }

        public TabPageEx()
            : base()
        {
            initEditBox("");
        }

        public TabPageEx(string txt)
            : base(txt)
        {
            initEditBox(txt);
        }

        protected override void OnCreateControl()
        {
            this.Font = new Font("Arial", 12, GraphicsUnit.Pixel);
            base.OnCreateControl();
        }

        protected void initEditBox(string txt)
        {
            m_editBox = new EditBox(this);
            m_editBox.SetText(txt);
            m_editBox.selectAll();
            m_editBox.PageRenaming += EditBox_PageRenaming;
        }

        private void EditBox_PageRenaming(object sender, string newName)
        {
            bool cancel = false;
            TabPageRenamingEventArgs args = new TabPageRenamingEventArgs(this, newName);
            PageRenaming?.Invoke(this, ref cancel, ref args);

            if(!args.IsHandled)
            {
                if(!cancel)
                {
                    this.Text = newName;
                    FinishEditMode();
                    Invalidate();
                }
            }

        }

        private bool m_inEditMode = false;
        public bool IsInEditMode
        {
            get { return m_inEditMode; }
        }

        public void StartEditMode()
        {
            if (!m_inEditMode)
            {
                m_editBox.SetText(this.Text);
                m_editBox.selectAll();
                m_inEditMode = true;

                if(EditStarted != null)
                {
                    EventArgs args = new EventArgs();
                    EditStarted(this, args);
                }
                              
            }
        }

        protected void FinishEditMode()
        {
            if (m_inEditMode)
            {
                m_inEditMode = false;
                m_editBox.ClearText();

                if(EditFinished != null)
                {
                    EventArgs args = new EventArgs();
                    EditFinished(this, args);
                }
                
            }                           
        }

        public void Draw(Graphics g, int index)
        {
            TabControlEx cntrl = this.Parent as TabControlEx;

            Rectangle recBounds = cntrl.getTabBounds(index);
            RectangleF tabTextArea = (RectangleF)recBounds;            

            Brush fillBr = cntrl.getTabBackgroundBrush(index, recBounds);

            GraphicsPath pthTab = new GraphicsPath();
            
            cntrl.getTabShapePath(index, pthTab, recBounds);
           
            g.DrawPath(Pens.Black, pthTab);

            g.FillPath(fillBr, pthTab);

            if (m_inEditMode)
            {
                recBounds.Location = new Point(recBounds.X + 14,
                                               recBounds.Y);
                m_editBox.Bounds = recBounds;
                m_editBox.Draw(g);
                return;
            }
            else
            {
                TextRenderer.DrawText(g, this.Text,
                   this.Font, recBounds, ForeColor);
            }            

            fillBr.Dispose();
            pthTab.Dispose();
        } 

        public  void KeyIsPressed(KeyPressEventArgs e)
        {
            if(this.IsInEditMode)
            {
                m_editBox.KeyIsPressed(e);
                this.Invalidate();
                e.Handled = true;
                return;
            }

        }

        public void KeyIsDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                FinishEditMode();
                this.Invalidate();
                e.Handled = true;
                return;
            }

            m_editBox.KeyIsDown(e);
            Invalidate();
            e.Handled = true;
            return;            

        }

        public void MouseIsDown(MouseEventArgs e)
        {
            if (IsInEditMode)
            {
                m_editBox.MouseIsDown(e);
                return;
            }

            if (e.Button == MouseButtons.Right)
            {
                if(m_menu != null)
                {
                    TabControlEx cntrl = this.Parent as TabControlEx;
                    Size offset = new Size(0, 0);

                    Point mnuLoc = e.Location;

                    if (cntrl.Alignment == TabAlignment.Top)
                    {
                        offset.Width = -5; offset.Height = -24;
                    }

                    mnuLoc += offset;

                    m_menu.Show(this, mnuLoc);
                }
                
            }

        }

        public void MouseIsMoving(MouseEventArgs e)
        {
            if (IsInEditMode)
            {
                m_editBox.MouseIsMoving(e);
            }

        }

        public void MouseIsDoubleClicked(MouseEventArgs e)
        {

            if(IsInEditMode)
            {
                m_editBox.MouseIsDoubleClicked(e);
            }

        }

        protected override void Dispose(bool disposing)
        {
            if (m_menu != null)
                m_menu.Dispose();
            base.Dispose(disposing);
        }

    }
}
