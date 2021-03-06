﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchInterface
{
    [System.Runtime.InteropServices.ComVisible(true)]
    public class JsMarshal
    {
        private Form1 _parent;
        private Dictionary<string, int> userFeedback = new Dictionary<string,int>();

        public JsMarshal() { }
        public JsMarshal(Form1 theForm) { _parent = theForm; }

        public void LikeButtonPressed(string filename, Boolean active)
        {
            userFeedback[filename] = active ? 1 : 0;
            _parent.SetStatus("User liked: " + filename + " Status= " + userFeedback[filename]);
        }

        public void HateButtonPressed(string filename, Boolean active)
        {
            userFeedback[filename] = active ? -1 : 0;
            _parent.SetStatus("User hated: " + filename + " Status= " + userFeedback[filename]);
        }

        public Dictionary<string, int> getFeedback()
        {
            return userFeedback;
        }

        public void resetResultSet(string[] results){
            userFeedback.Clear();
            for(int i=0; i<results.Length; i++){
                // Initially there is no feedback on any
                // of the results
                userFeedback[results[i]] = 0;
            }
        }
    }
}
