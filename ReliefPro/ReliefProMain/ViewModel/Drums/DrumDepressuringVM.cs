﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using NHibernate;
using ReliefProLL;
using ReliefProMain.Commands;
using ReliefProMain.Model;
using UOMLib;
using ReliefProMain.View.DrumDepressures;

namespace ReliefProMain.ViewModel.Drums
{
    public class DrumDepressuringVM : ViewModelBase
    {
        public ICommand OKCMD { get; set; }
        public ICommand CalcCMD { get; set; }
        public ICommand DetailedCMD { get; set; }
        public ICommand DepressuringCurveCMD { get; set; }
        private string selectedShotCut = "Shortcut";
        public string SelectedShotCut
        {
            get { return selectedShotCut; }
            set
            {
                selectedShotCut = value;
                if (value == "Shortcut")
                {
                    isEnableFireHeatInput = false;
                }
                else
                {
                    isEnableFireHeatInput = true;
                }
                OnPropertyChanged("SelectedShotCut");
            }
        }
        private bool enableFireHeatInput;
        public bool isEnableFireHeatInput
        {
            get { return enableFireHeatInput; }
            set
            {
                enableFireHeatInput = value;
                OnPropertyChanged("isEnableFireHeatInput");
            }
        }

        public List<string> lstShortCut { get; set; }

        private string selectedDeprRqe = "21bar/min";
        public string SelectedDeprRqe
        {
            get { return selectedDeprRqe; }
            set
            {
                selectedDeprRqe = value;
                OnPropertyChanged("SelectedDeprRqe");
            }
        }
        public List<string> lstDeprRqe { get; set; }

        private string selectedHeatInput = "API 521";
        public string SelectedHeatInput
        {
            get { return selectedHeatInput; }
            set
            {
                selectedHeatInput = value;
                OnPropertyChanged("SelectedHeatInput");
            }
        }
        public List<string> lstHeatInput { get; set; }
        private ISession SessionPS;
        private ISession SessionPF;
        public DrumDepressuringModel model { get; set; }
        private DrumDepressuringBLL drumBLL;
        public DrumDepressuringVM(int ScenarioID, ISession SessionPS, ISession SessionPF)
        {
            this.SessionPS = SessionPS;
            this.SessionPF = SessionPF;
            OKCMD = new DelegateCommand<object>(Save);
            CalcCMD = new DelegateCommand<object>(Calc);
            DetailedCMD = new DelegateCommand<object>(CalcDetailed);
            DepressuringCurveCMD = new DelegateCommand<object>(DepressuringCurve);

            lstShortCut = new List<string>(new[] { "Shortcut", "PROII DEPR Unit" });
            lstDeprRqe = new List<string>{
                "21bar/min","7bar/min","50% Design pressure in 15min","7barg in 15min","Specify"};
            lstHeatInput = new List<string>(new[] { "API 521", "API 521 Scale", "API 2000", "API 2000 Scale" });

            drumBLL = new DrumDepressuringBLL(SessionPS, SessionPF);
            var drumModel = drumBLL.GetDrumPressuring(ScenarioID);
            drumModel = drumBLL.ReadConvertModel(drumModel);
            if (!string.IsNullOrEmpty(drumModel.HeatInputModel))
                selectedHeatInput = drumModel.HeatInputModel;
            if (!string.IsNullOrEmpty(drumModel.DepressuringRequirements))
                selectedDeprRqe = drumModel.DepressuringRequirements;
            if (!string.IsNullOrEmpty(drumModel.HeatInputModel))
                selectedShotCut = drumModel.ShortCut;

            UOMLib.UOMEnum uomEnum = new UOMLib.UOMEnum(SessionPF);
            model = new DrumDepressuringModel(drumModel);
            model.InitialPressureUnit = uomEnum.UserPressure;
            model.VaporDensityUnit = uomEnum.UserDensity;
            model.TotalVaporVolumeUnit = uomEnum.UserVolume;
            model.VesseldesignpressureUnit = uomEnum.UserPressure;
            model.TotalWettedAreaUnit = uomEnum.UserArea;
            model.InitialDepressuringRateUnit = uomEnum.UserMassRate;

            model.TimespecifyUnit = uomEnum.UserTime;
            model.CalculatedVesselPressureUnit = uomEnum.UserPressure;
            model.CalculatedDepressuringRateUnit = uomEnum.UserMassRate;

            model.DetailPUnit = uomEnum.UserPressure;
            model.DetailPTimeUnit = uomEnum.UserTime;
            model.TimeStepUnit = uomEnum.UserTime;
        }
        private void WriteConvertModel()
        {
            model.dbmodel.InitialPressure = UnitConvert.Convert(model.InitialPressureUnit, UOMLib.UOMEnum.Pressure.ToString(), model.InitialPressure);
            model.dbmodel.VaporDensity = UnitConvert.Convert(model.VaporDensityUnit, UOMLib.UOMEnum.Density.ToString(), model.VaporDensity.Value);
            model.dbmodel.TotalVaporVolume = UnitConvert.Convert(model.TotalVaporVolumeUnit, UOMLib.UOMEnum.Volume.ToString(), model.TotalVaporVolume);
            model.dbmodel.Vesseldesignpressure = UnitConvert.Convert(model.VesseldesignpressureUnit, UOMLib.UOMEnum.Pressure.ToString(), model.Vesseldesignpressure);
            model.dbmodel.TotalWettedArea = UnitConvert.Convert(model.TotalWettedAreaUnit, UOMLib.UOMEnum.Area.ToString(), model.TotalWettedArea);
            //model.dbmodel.ValveConstantforSonicFlow = uc.Convert(model.ReliefTemperatureUnit, UOMLib.UOMEnum.Temperature.ToString(), model.ReliefTemperature);
            model.dbmodel.InitialDepressuringRate = UnitConvert.Convert(model.InitialDepressuringRateUnit, UOMLib.UOMEnum.MassRate.ToString(), model.InitialDepressuringRate);
            model.dbmodel.Timespecify = UnitConvert.Convert(model.TimespecifyUnit, UOMLib.UOMEnum.Time.ToString(), model.Timespecify);
            model.dbmodel.CalculatedVesselPressure = UnitConvert.Convert(model.CalculatedVesselPressureUnit, UOMLib.UOMEnum.Pressure.ToString(), model.CalculatedVesselPressure);
            model.dbmodel.CalculatedDepressuringRate = UnitConvert.Convert(model.CalculatedDepressuringRateUnit, UOMLib.UOMEnum.MassRate.ToString(), model.CalculatedDepressuringRate);

            model.dbmodel.DeltaP = UnitConvert.Convert(model.DetailPUnit, UOMLib.UOMEnum.Pressure.ToString(), model.DetailP);
            model.dbmodel.DeltaPTime = UnitConvert.Convert(model.DetailPTimeUnit, UOMLib.UOMEnum.Time.ToString(), model.DetailPTime);
            model.dbmodel.TimeStep = UnitConvert.Convert(model.TimeStepUnit, UOMLib.UOMEnum.Time.ToString(), model.TimeStep);

            model.dbmodel.ShortCut = selectedShotCut;
            model.dbmodel.DepressuringRequirements = selectedDeprRqe;
            model.dbmodel.FireHeatInput = enableFireHeatInput;
            model.dbmodel.HeatInputModel = selectedHeatInput;
        }
        private void Calc(object obj)
        {



        }
        private void CalcDetailed(object obj)
        { 
        }
        private void DepressuringCurve(object obj)
        {
            DeprCurveView v = new DeprCurveView();
            v.ChartSource=new KeyValuePair<int, double>[]{
                 new KeyValuePair<int,double>(1,400),
                 new KeyValuePair<int,double>(2,200),
                 new KeyValuePair<int,double>(3,300),
                 new KeyValuePair<int,double>(4,320),
                 new KeyValuePair<int,double>(5,150)};
            v.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            v.ShowDialog();
        
        }
        private void Save(object obj)
        {
            WriteConvertModel();
            drumBLL.SaveData(model.dbmodel, SessionPS);
            if (obj != null)
            {
                System.Windows.Window wd = obj as System.Windows.Window;
                if (wd != null)
                {
                    wd.DialogResult = true;
                }
            }
        }
    }
}
