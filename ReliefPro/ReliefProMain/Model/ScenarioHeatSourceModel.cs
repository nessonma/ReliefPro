﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReliefProModel;
using NHibernate;
using ReliefProBLL.Common;
using ReliefProDAL;

namespace ReliefProMain.Model
{     
    public class ScenarioHeatSourceModel : ModelBase
    {
        public ScenarioHeatSource model;

        
        public int ID
        {
            get
            {
                return model.ID;
            }
            set
            {
                if (model.ID != value)
                {
                    model.ID = value;
                    NotifyPropertyChanged("ID");
                }
            }
        }
        public int HeatSourceID
        {
            get
            {
                return model.HeatSourceID;
            }
            set
            {
                if (model.HeatSourceID != value)
                {
                    model.HeatSourceID = value;
                    NotifyPropertyChanged("HeatSourceID");
                }
            }
        }
        public string DutyFactor
        {
            get
            {
                return model.DutyFactor;
            }
            set
            {
                if (model.DutyFactor != value)
                {
                    model.DutyFactor = value;
                    NotifyPropertyChanged("DutyFactor");
                }
            }
        }
        private string _HeatSourceName;
        public string HeatSourceName
        {
            get
            {
                return _HeatSourceName;
            }
            set
            {
                if (_HeatSourceName != value)
                {
                    _HeatSourceName = value;
                    NotifyPropertyChanged("HeatSourceName");
                }
            }
        }

        private string _HeatSourceType;
        public string HeatSourceType
        {
            get
            {
                return _HeatSourceType;
            }
            set
            {
                if (_HeatSourceType != value)
                {
                    _HeatSourceType = value;
                    NotifyPropertyChanged("HeatSourceType");
                }
            }
        }
        private string _Duty;
        public string Duty
        {
            get
            {
                return _Duty;
            }
            set
            {
                if (_Duty != value)
                {
                    _Duty = value;
                    NotifyPropertyChanged("Duty");
                }
            }
        }

        public ScenarioHeatSourceModel(ScenarioHeatSource m)
        {
            model = m;
        }
       
    }
}