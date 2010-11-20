using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchInterface
{
    [System.Runtime.InteropServices.ComVisible(true)]
    public class JsMarshal
    {
        private Form1 _parent;

        public JsMarshal() { }
        public JsMarshal(Form1 theForm) { _parent = theForm; }

        public void LikeButtonPressed(string filename)
        {
            _parent.SetStaus("User liked " + filename);
        }

        public void HateButtonPressed(string filename)
        {
            _parent.SetStaus("User hated " + filename);
        }
    }
}
