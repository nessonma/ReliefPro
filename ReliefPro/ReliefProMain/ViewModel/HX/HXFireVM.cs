﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using NHibernate;
using ReliefProLL;
using ReliefProMain.Model.HX;
using UOMLib;

namespace ReliefProMain.ViewModel.HX
{
    public class HXFireVM : ViewModelBase
    {
        public ICommand OKCMD { get; set; }
        private ISession SessionPS;
        private ISession SessionPF;
        public HXFireModel model { get; set; }
        private HXBLL hxBLL;
        public HXFireVM(int ScenarioID, ISession SessionPS, ISession SessionPF)
        {
            this.SessionPS = SessionPS;
            this.SessionPF = SessionPF;
            OKCMD = new DelegateCommand<object>(Save);

            hxBLL = new HXBLL(SessionPS, SessionPF);
            var fireModel = hxBLL.GetHXFireModel(ScenarioID);
            fireModel = hxBLL.ReadConvertHXFireModel(fireModel);

            model = new HXFireModel(fireModel);
            model.dbmodel.ScenarioID = ScenarioID;


            UOMLib.UOMEnum uomEnum = new UOMLib.UOMEnum(SessionPF);
            model.ODUnit = uomEnum.UserLength;
            model.LengthUnit = uomEnum.UserLength;
            model.ElevationUnit = uomEnum.UserLength;
        }
        private void WriteConvertModel()
        {
            UnitConvert uc = new UnitConvert();
            model.dbmodel.ExposedToFire = model.ExposedToFire;
            model.dbmodel.Type = model.Type;
            model.dbmodel.OD = uc.Convert(model.ODUnit, UOMLib.UOMEnum.Length.ToString(), model.OD);
            model.dbmodel.Length = uc.Convert(model.LengthUnit, UOMLib.UOMEnum.Length.ToString(), model.Length);
            model.dbmodel.Elevation = uc.Convert(model.ElevationUnit, UOMLib.UOMEnum.Length.ToString(), model.Elevation);
            model.dbmodel.PipingContingency = model.PipingContingency;
        }

        private void Save(object obj)
        {
            if (obj != null)
            {
                System.Windows.Window wd = obj as System.Windows.Window;
                if (wd != null)
                {
                    WriteConvertModel();
                    hxBLL.SaveHXFire(model.dbmodel);
                    wd.DialogResult = true;
                }
            }
        }
    }
}