﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefProModel
{
    public class BasicUnitCurrent
    {
        public virtual int ID { get; set; }
        public virtual int BasicUnitID { get; set; }
        public virtual int UnitTypeID { get; set; }
        public virtual int SystemUnitID { get; set; }
    }
}