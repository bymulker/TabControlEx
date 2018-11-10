using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ToolbarEx
{ 
    struct CharRecord
    {
        public Char c;
        public SizeF sz;
        public bool sel;
    }


    class TextArray
    {
        private Font m_font;

        private List<CharRecord> m_rec = new List<CharRecord>();

        public void SetFont(Font fnt)
        {
            Debug.Assert(fnt != null);
            if(m_font != fnt)
            {
                IEnumerator<CharRecord> cEn = this.GetEnumerator();
                while(cEn.MoveNext())
                {
                    CharRecord rec = cEn.Current;
                    rec.sz = TextRenderer.MeasureText(rec.c.ToString(), fnt);
                }
            }

            m_font = fnt;
        }

        public TextArray(Font fnt)
        {
            m_font = fnt;
        }

        public TextArray(Font fnt, string txt, Graphics g)
        {
            m_font = fnt;
            setText(txt, g);            
        }

        public void setText(string txt, Graphics g)
        {
            this.Clear();
            addText(txt, g);
        }

        public void addText(string txt, Graphics g)
        {
            CharEnumerator cEn = txt.GetEnumerator();
            while (cEn.MoveNext())
            {
                this.Add(cEn.Current, g);
            }
        }

        public string getText()
        {
            string txt = "";
            IEnumerator<CharRecord> cEn = this.GetEnumerator();
            while (cEn.MoveNext())
            {
                CharRecord rec = cEn.Current;
                txt += rec.c.ToString();
            }
            return txt;

        }

        public CharRecord this[int index]
        {
            get
            {
                return m_rec[index];
            }

            set
            {
                m_rec[index] = value;
            }
        }

        public int Count
        {
            get
            {
                return m_rec.Count;
            }
        }

        public static CharRecord createCharRecord(Font f, Char c, Graphics g)
        {
            string str = c.ToString();
            CharRecord item = new CharRecord();
            item.c = c;
            item.sz = g.MeasureString(str, f, 0, 
                   StringFormat.GenericTypographic);
            item.sel = false;
            return item;
        }

        public void Add(Char c, Graphics g)
        {
            m_rec.Add(createCharRecord(m_font, c, g));
        }

        public void Insert(int index, Char c, Graphics g)
        {
            CharRecord item = createCharRecord(m_font, c, g);
            m_rec.Insert(index, item);
        }

        public void Clear()
        {
            m_rec.Clear();
        }

        public void RemoveRange(int n1, int n2)
        {
            if (n1 == n2 || n1 < 0)
                return;            

            if(n1 > n2)
            {
                int temp = n1;
                n1 = n2;
                n2 = temp;
            }

            if(n2 >= this.Count)
            {
                if (n1 == 0)
                {
                    m_rec.Clear();
                    return;
                }
                else
                    n2 = this.Count - 1;
            }
           
            int cnt = (n2 - n1) + 1;

            m_rec.RemoveRange(n1, cnt);

        }

        public void CopyTo(CharRecord[] array, int arrayIndex)
        {
            m_rec.CopyTo(array, arrayIndex);
        }

        public IEnumerator<CharRecord> GetEnumerator()
        {
            return m_rec.GetEnumerator();
        }

        public int IndexOf(CharRecord item)
        {
            return m_rec.IndexOf(item);
        }

        public void RemoveAt(int index)
        {
            m_rec.RemoveAt(index);
        }

       
    }
}
