﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReliefProModel.Reports
{
    public class PUsummary
    {
        public virtual int ID { get; set; }
        public virtual int UnitID { get; set; }
        public virtual string PlantName { get; set; }
        public virtual string ProcessUnitName { get; set; }
        public virtual string Description { get; set; }
        public virtual string Remark { get; set; }
    }
}