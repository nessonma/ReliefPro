﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReliefProModel.ReactorLoops
{
    public class ReactorLoopDetail
    {
        public virtual int ID { get; set; }
        //0-ProcessHX,1-UtilityHX,2-Mixer/Splitter
        public virtual int ReactorLoopID { get; set; }
        public virtual string ReactorType { get; set; }
        public virtual string DetailInfo { get; set; }
    }
}