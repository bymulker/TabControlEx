using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ToolbarEx
{
    public class PageRenamingEventArgs : EventArgs
    {
        private string m_name = "";
        public string NewName
        {
            get { return m_name; }
        }

        private PageRenamingEventArgs() { }

        public PageRenamingEventArgs(string name)
        {
            m_name = name;
        }

    }

    public delegate void PageRenamingEventHandler(object sender, string newName);

    class EditBox : IDisposable
    {
        public event PageRenamingEventHandler PageRenaming;

        /// <summary>
        /// cursor character
        /// </summary>
        string _c = char.ConvertFromUtf32(124);

        private TextArray m_textArr;

        private TabPageEx m_parent;
        
        private int m_selStart = 0;

        private int m_selEnd = 0;

        private Pen m_cursPen = new Pen(Color.Black, 2);

        public EditBox(TabPageEx parent)
        {
            m_parent = parent;
            m_textArr = new TextArray(m_parent.Font);
        }

        public EditBox(TabPageEx parent, string text)
        {
            m_parent = parent;
            Graphics g = parent.CreateGraphics();
            m_textArr = new TextArray(m_parent.Font, text, g);
            g.Dispose();
        }

        public string Text
        {            
            get { return m_textArr.getText(); }            
        }

        public void SetText(string txt, Graphics g = null)
        {
            bool destroyGr = false;
            if(g == null)
            {
                g = m_parent.CreateGraphics();
                destroyGr = true;
            }
                
            m_textArr.setText(txt, g);

            if (destroyGr)
                g.Dispose(); 
        }

        public void ClearText()
        {
            m_textArr.Clear();
        }

        private Rectangle m_bounds = Rectangle.Empty;
        public Rectangle Bounds
        {
            get { return m_bounds; }
            set { m_bounds = value; }
        }
                
        private Color m_selBackColor = Color.Black;

        public Color SelBackColor
        {
            get { return m_selBackColor; }
            set { m_selBackColor = value; }
        }

        private Color m_selForeColor = Color.White;

        public Color SelForeColor
        {
            get { return m_selForeColor; }
            set { m_selForeColor = value; }
        }

        private int m_cursPos = 0;

        public int CursorPosition
        {
            get { return m_cursPos; }
            set
            {
                if(value <= m_textArr.Count && value >= 0)
                    m_cursPos = value;
            }
        }

        public void Draw(Graphics g)
        {
            Render(g);
        }

        public void selectAll()
        {
            setSelection(0, m_textArr.Count);
        }
                
        public void setSelection(int _start, int _end)
        {
            if (_start == _end)
                return;

            if (_start < 0 || _end < 0)
                return;
            
            if(_end < _start)
            {
                int temp = _start;
                _start = _end;
                _end = temp;
            }

            m_selStart = _start;
            m_selEnd = _end;
            m_cursPos = _end;
        }

        public void selectOffset(int offset)
        {
            if (offset == 0)
                return;

            //if selection is empty, start selection
            if(m_selEnd < 0)
            {
                m_selStart = m_cursPos;
                m_selEnd = m_cursPos;
            }
                       
            int selTarget = m_cursPos + offset;

            if(selTarget < m_selStart)
            {
                //going back <-
                if(selTarget < 0)
                {
                    selTarget = 0;
                }

                m_selStart = selTarget;
                
            }
            else if(selTarget > m_selEnd)
            {
                //going forth ->
                if(selTarget > m_textArr.Count)
                {
                    selTarget = m_textArr.Count;
                }

                m_selEnd = selTarget;
                
            }
            else
            {
                //cursor is moving inside selection.
                if(m_cursPos == m_selStart)
                {
                    m_selStart = selTarget; 
                }
                else if(m_cursPos == m_selEnd)
                {
                    m_selEnd = selTarget;
                }
            }

            m_cursPos = selTarget;

        }

        public void deselect()
        {
            m_selStart = -1;
            m_selEnd = -1;
        }

        public bool delSelection()
        {
            if (m_selStart >= 0 && m_selEnd > m_selStart)
            {
                //we have a selection, put cursor at the beginning
                m_cursPos = m_selStart;

                //now delete the selected characters
                m_textArr.RemoveRange(m_selStart, m_selEnd);
                deselect();
                return true;
            }

            return false;
        }

        public int getHitPos(Point mousePt)
        {
            CharRecord ch;
            RectangleF chR;
            int indexPos = 0;
            double physPos = 0;
            IEnumerator<CharRecord> cEn = m_textArr.GetEnumerator();
            while (cEn.MoveNext())
            {
                ch = cEn.Current;
                chR = new RectangleF((float)(m_bounds.Location.X + physPos), m_bounds.Y,
                                    ch.sz.Width, ch.sz.Height);
                if (chR.Contains(mousePt))
                    return indexPos;
                indexPos++;
                physPos += ch.sz.Width;
            }

            if (mousePt.X > physPos)
                return indexPos;

            return -1;

        }
        
        protected void Render(Graphics g)
        {
            CharRecord ch;
            RectangleF chR = RectangleF.Empty;
            Brush backBr;
            Brush foreBr;
            int indexPos = 0;
            double physPos = 0;
            float cursPos;
                        
            Brush normBackBrush = new SolidBrush(Color.White);
            Brush normForeBrush = new SolidBrush(Color.Black);

            Brush selBackBrush = new SolidBrush(SelBackColor);
            Brush selForeBrush = new SolidBrush(SelForeColor);            

            SizeF cursSize = g.MeasureString(_c, m_parent.Font, 0,
                   StringFormat.GenericTypographic);

            PointF chrLoc;
            Size offset = new Size(0, 4);

            IEnumerator<CharRecord> cEn = m_textArr.GetEnumerator();
            while (cEn.MoveNext())
            {
                ch = cEn.Current;
                cursPos = (float)(m_bounds.Location.X + physPos);
                chR = new RectangleF(cursPos, m_bounds.Y,
                                    ch.sz.Width, ch.sz.Height + 4);
                chrLoc = chR.Location + offset;

                if (indexPos >= m_selStart && indexPos <= m_selEnd)
                {
                    backBr = selBackBrush;
                    foreBr = selForeBrush;
                }
                else
                {
                    backBr = normBackBrush;
                    foreBr = normForeBrush;
                }

                g.FillRectangle(backBr, chR);
                g.DrawString(ch.c.ToString(), m_parent.Font, foreBr,
                    chrLoc.X, chrLoc.Y,
                    StringFormat.GenericTypographic);

                if (indexPos == CursorPosition)
                {
                    //draw cursor
                    chR = new RectangleF(chrLoc, cursSize);
                    g.DrawLine(m_cursPen, chR.Location,
                        new Point((int)chR.Location.X, (int)chR.Bottom));                   
                }
                                
                indexPos++;
                physPos += ch.sz.Width;

            }

            if (indexPos == CursorPosition)
            {
                //draw cursor at the end..
                chR = new RectangleF(new Point((int)(m_bounds.Location.X + physPos), 
                                                m_bounds.Y), cursSize);
                chrLoc = chR.Location + offset;
                g.DrawLine(m_cursPen, chrLoc,
                    new Point((int)chR.Location.X, (int)chR.Bottom+4));
            }

            normBackBrush.Dispose();
            normForeBrush.Dispose();
            selBackBrush.Dispose();
            selForeBrush.Dispose();

        }

        public void MouseIsDown(MouseEventArgs e)
        {
            int hitPos = getHitPos(e.Location);

            if(hitPos >= 0)
            {
                deselect();
                m_selStart = hitPos;
                m_cursPos = hitPos;
                m_parent.Invalidate();
            }
        }

        public void MouseIsMoving(MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                int hitPos = getHitPos(e.Location);

                if (hitPos >= 0 && hitPos > m_selStart)
                {
                    m_selEnd = hitPos;
                    m_parent.Invalidate();
                }
            }            

        }

        public void MouseIsDoubleClicked(MouseEventArgs e)
        {
            selectAll();
            m_parent.Invalidate();           
        }

        public void KeyIsDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right)
            {
                if (e.Shift)
                {
                    selectOffset(1);
                }
                else
                {
                    deselect();
                    if(m_cursPos < m_textArr.Count)
                        m_cursPos++;
                }
               
            }

            else if (e.KeyCode == Keys.Left)
            {
                if (e.Shift)
                {
                    selectOffset(-1);
                }
                else
                {
                    deselect();
                    if(m_cursPos > 0)
                        m_cursPos--;
                }

            }

            else if(e.KeyCode == Keys.Back)
            {
                if(m_cursPos > 0)
                {
                    deselect();
                    m_textArr.RemoveAt(m_cursPos - 1);
                    m_cursPos--;
                }

            }

            else if(e.KeyCode == Keys.Delete)
            {
                if (delSelection())
                    return;

                //we dont have a selection..delete the next cursor position
                int cnt = m_textArr.Count;
                if(cnt > 0 && m_cursPos < cnt)
                {
                    m_textArr.RemoveAt(m_cursPos);
                }
            }

            else if(e.KeyCode == Keys.Enter)
            {
                PageRenaming?.Invoke(this, m_textArr.getText());
            }

        }

        protected void typeChar(Char ch)
        {
            Graphics g = m_parent.CreateGraphics();

            delSelection();

            if(m_cursPos > m_textArr.Count)
            {
                //we are at the end so append
                m_textArr.Add(ch, g);               
            }
            else
            {
                m_textArr.Insert(m_cursPos, ch, g);
            }

            m_cursPos++;            

        }

        public void KeyIsPressed(KeyPressEventArgs e)
        {
            if(e.KeyChar == (char)Keys.Back || 
                e.KeyChar == (char)Keys.Delete||
                e.KeyChar == (char)Keys.Enter) 
            {
                e.Handled = true;
                return;
            }

            typeChar(e.KeyChar);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    m_cursPen.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~EditBox() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion


    }
}
