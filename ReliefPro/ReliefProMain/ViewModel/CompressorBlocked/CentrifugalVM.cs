﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using NHibernate;
using ReliefProLL;
using ReliefProMain.Commands;
using ReliefProMain.Model.CompressorBlocked;
using UOMLib;

namespace ReliefProMain.ViewModel.CompressorBlocked
{
    public class CentrifugalVM : ViewModelBase
    {
        public ICommand CalcCMD { get; set; }
        public ICommand OKCMD { get; set; }
        private ISession SessionPS;
        private ISession SessionPF;
        public CentrifugalModel model { get; set; }
        private CompressorBlockedBLL blockBLL;
        public CentrifugalVM(int ScenarioID, ISession SessionPS, ISession SessionPF)
        {
            this.SessionPS = SessionPS;
            this.SessionPF = SessionPF;
            OKCMD = new DelegateCommand<object>(Save);
            CalcCMD = new DelegateCommand<object>(CalcResult);

            blockBLL = new CompressorBlockedBLL(SessionPS, SessionPF);
            var BlockedModel = blockBLL.GetCentrifugalModel(ScenarioID);
            BlockedModel = blockBLL.ReadConvertCentrifugalModel(BlockedModel);

            model = new CentrifugalModel(BlockedModel);
            model.dbmodel.ScenarioID = ScenarioID;

            UOMLib.UOMEnum uomEnum = new UOMEnum(SessionPF);
            model.ReliefloadUnit = uomEnum.UserMassRate;
            model.ReliefTempUnit = uomEnum.UserTemperature;
            model.ReliefPressureUnit = uomEnum.UserPressure;
        }
        private void WriteConvertModel()
        {
            UnitConvert uc = new UnitConvert();
            model.dbmodel.Scale = model.Scale;
            model.dbmodel.ReliefMW = model.ReliefMW;
            model.dbmodel.Reliefload = uc.Convert(model.ReliefloadUnit, UOMLib.UOMEnum.MassRate.ToString(), model.Reliefload);
            model.dbmodel.ReliefTemp = uc.Convert(model.ReliefTempUnit, UOMLib.UOMEnum.Temperature.ToString(), model.ReliefTemp);
            model.dbmodel.ReliefPressure = uc.Convert(model.ReliefPressureUnit, UOMLib.UOMEnum.Pressure.ToString(), model.ReliefPressure);
        }
        private void CalcResult(object obj)
        {
        }
        private void Save(object obj)
        {
            if (obj != null)
            {
                System.Windows.Window wd = obj as System.Windows.Window;
                if (wd != null)
                {
                    WriteConvertModel();
                    wd.DialogResult = true;
                }
            }
        }
    }
}
