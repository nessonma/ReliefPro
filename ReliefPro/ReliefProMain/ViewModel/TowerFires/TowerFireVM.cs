﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ReliefProModel;
using ReliefProMain.Commands;
using ReliefProDAL;
using ReliefProBLL.Common;
using ReliefProMain.Interface;
using ReliefProMain.Service;
using ReliefProMain.View.TowerFires;
using ReliefProMain.Model;
using UOMLib;
using ReliefProMain.ViewModel;
using NHibernate;
using System.IO;
using ProII;
using ReliefProCommon.CommonLib;

namespace ReliefProMain.ViewModel.TowerFires
{
    public class TowerFireVM : ViewModelBase
    {
        private ISession SessionPlant { set; get; }
        private ISession SessionProtectedSystem { set; get; }
        private string DirPlant { set; get; }
        private string DirProtectedSystem { set; get; }
        private string PrzFile;
        private string PrzVersion;
        private string EqName;
        public ObservableCollection<string> HeatInputModels { get; set; }
        private Latent latent;
        private int ScenarioID;
        public TowerFireModel MainModel { get; set; }
        //public ReliefProModel.TowerFire CurrentModel { get; set; }
        private ObservableCollection<TowerFireEq> _EqList;
        public ObservableCollection<TowerFireEq> EqList
        {
            get
            {
                return this._EqList;
            }
            set
            {
                this._EqList = value;
                OnPropertyChanged("EqList");
            }
        }
        UnitConvert unitConvert;
        UOMLib.UOMEnum uomEnum;

        public TowerFireVM(int ScenarioID,string EqName,string PrzFile,string PrzVersion, ISession sessionPlant, ISession sessionProtectedSystem,string DirPlant, string DirProtectedSystem)
        {
            unitConvert = new UnitConvert();
            uomEnum = new UOMLib.UOMEnum(sessionPlant);
            InitUnit();

            SessionPlant = sessionPlant;
            SessionProtectedSystem = sessionProtectedSystem;
            HeatInputModels = GetHeatInputModels();

            this.PrzFile = PrzFile;
            this.PrzVersion = PrzVersion;
            this.DirPlant = DirPlant;
            this.DirProtectedSystem = DirProtectedSystem;
            this.EqName = EqName;
            this.ScenarioID = ScenarioID;

            UnitConvert uc = new UnitConvert();
            TowerFireDAL db = new TowerFireDAL();
            ReliefProModel.TowerFire model = db.GetModel(SessionProtectedSystem, ScenarioID);
            MainModel = new TowerFireModel(model);
            ReadConvert();
            TowerFireEqDAL dbtfeq = new TowerFireEqDAL();
            IList<TowerFireEq> list = dbtfeq.GetAllList(SessionProtectedSystem, MainModel.ID);
            EqList = new ObservableCollection<TowerFireEq>();
            foreach (TowerFireEq eq in list)
            {
                EqList.Add(eq);
            }

            LatentDAL dblatent = new LatentDAL();
            latent = dblatent.GetModel(SessionProtectedSystem);

        }

        private ICommand _OKClick;
        public ICommand OKClick
        {
            get
            {
                if (_OKClick == null)
                {
                    _OKClick = new RelayCommand(Update);

                }
                return _OKClick;
            }
        }

        private void Update(object window)
        {

            TowerFireDAL towerFireDAL = new TowerFireDAL();
            //ReliefProModel.TowerFire m = db.GetModel(CurrentModel.ID, SessionProtectedSystem);
            //m.HeatInputModel = CurrentModel.HeatInputModel;
            //m.IsExist = CurrentModel.IsExist;
            //m.ReliefCpCv = CurrentModel.ReliefCpCv;
            //m.ReliefLoad = CurrentModel.ReliefLoad;
            //m.ReliefMW = CurrentModel.ReliefMW;
            //m.ReliefPressure = CurrentModel.ReliefPressure;
            //m.ReliefTemperature = CurrentModel.ReliefTemperature;
            //m.ReliefZ = CurrentModel.ReliefZ;
WriteConvert();
            towerFireDAL.Update(MainModel.model, SessionProtectedSystem);

            ScenarioDAL scenarioDAL = new ScenarioDAL();
            Scenario sc = scenarioDAL.GetModel(ScenarioID, SessionProtectedSystem);
            sc.ReliefLoad = MainModel.ReliefLoad;
            sc.ReliefMW = MainModel.ReliefMW;
            sc.ReliefPressure = MainModel.ReliefPressure;
            sc.ReliefTemperature = MainModel.ReliefTemperature;
            scenarioDAL.Update(sc, SessionProtectedSystem);
            SessionProtectedSystem.Flush();


            System.Windows.Window wd = window as System.Windows.Window;

            if (wd != null)
            {
                wd.DialogResult = true;
            }
        }

        public ObservableCollection<string> GetHeatInputModels()
        {
            ObservableCollection<string> list = new ObservableCollection<string>();
            list.Add("API 521");
            return list;
        }

        private ICommand _EditClick;
        public ICommand EditClick
        {
            get
            {
                if (_EditClick == null)
                {
                    _EditClick = new RelayCommand(Edit);

                }
                return _EditClick;
            }
        }
        private void Edit(object obj)
        {

            double C1 = 0;
            if (MainModel.IsExist)
            {
                C1 = 43200;
            }
            else
            {
                C1 = 70900;
            }
            int id = int.Parse(obj.ToString());

            TowerFireEqDAL db = new TowerFireEqDAL();
            TowerFireEq eq = db.GetModel(id, SessionProtectedSystem);

            double latentEnthalpy = double.Parse(latent.LatentEnthalpy);
            if (eq.Type == "Column" || eq.Type == "Side Column")
            {
                TowerFireColumnView v = new TowerFireColumnView();
                TowerFireColumnVM vm = new TowerFireColumnVM(eq.ID, SessionPlant, SessionProtectedSystem);
                v.DataContext = vm;
                v.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                if (v.ShowDialog() == true)
                {
                    EqList.Clear();
                    eq.WettedArea = vm.Area.ToString(); ;
                    eq.HeatInput = Algorithm.GetQ(C1, double.Parse(eq.FFactor), double.Parse(eq.WettedArea)).ToString();
                    eq.ReliefLoad = (double.Parse(eq.HeatInput) / latentEnthalpy).ToString();
                    db.Update(eq, SessionProtectedSystem);
                    IList<TowerFireEq> list = db.GetAllList(SessionProtectedSystem, MainModel.ID);
                    foreach (TowerFireEq q in list)
                    {
                        EqList.Add(q);
                    }
                    SessionProtectedSystem.Flush();
                }
            }
            else if (eq.Type == "Drum")
            {
                TowerFireDrumView v = new TowerFireDrumView();
                TowerFireDrumVM vm = new TowerFireDrumVM(eq.ID, SessionPlant, SessionProtectedSystem);
                v.DataContext = vm;
                v.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                if (v.ShowDialog() == true)
                {
                    EqList.Clear();
                    eq.WettedArea = vm.Area.ToString();
                    eq.HeatInput = Algorithm.GetQ(C1, double.Parse(eq.FFactor), double.Parse(eq.WettedArea)).ToString();
                    eq.ReliefLoad = (double.Parse(eq.HeatInput) / latentEnthalpy).ToString();
                    db.Update(eq, SessionProtectedSystem);
                    IList<TowerFireEq> list = db.GetAllList(SessionProtectedSystem, MainModel.ID);
                    foreach (TowerFireEq q in list)
                    {
                        EqList.Add(q);
                    }
                    SessionProtectedSystem.Flush();
                }
            }
            else if (eq.Type == "Shell-Tube HX")
            {
                TowerFireHXView v = new TowerFireHXView();
                TowerFireHXVM vm = new TowerFireHXVM(eq.ID, SessionPlant, SessionProtectedSystem);
                v.DataContext = vm;
                v.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                if (v.ShowDialog() == true)
                {
                    EqList.Clear();
                    eq.WettedArea = vm.Area.ToString();
                    eq.HeatInput = Algorithm.GetQ(C1, double.Parse(eq.FFactor), double.Parse(eq.WettedArea)).ToString();
                    eq.ReliefLoad = (double.Parse(eq.HeatInput) / latentEnthalpy).ToString();
                    db.Update(eq, SessionProtectedSystem);
                    IList<TowerFireEq> list = db.GetAllList(SessionProtectedSystem, MainModel.ID);
                    foreach (TowerFireEq q in list)
                    {
                        EqList.Add(q);
                    }
                    SessionProtectedSystem.Flush();
                }
            }
            else if (eq.Type == "Air Cooler")
            {
                TowerFireCoolerView v = new TowerFireCoolerView();
                TowerFireCoolerVM vm = new TowerFireCoolerVM(eq.ID, SessionPlant, SessionProtectedSystem);
                v.DataContext = vm;
                v.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                if (v.ShowDialog() == true)
                {
                    EqList.Clear();
                    eq.WettedArea = vm.Area.ToString();
                    eq.HeatInput = Algorithm.GetQ(C1, double.Parse(eq.FFactor), double.Parse(eq.WettedArea)).ToString();
                    eq.ReliefLoad = (double.Parse(eq.HeatInput) / latentEnthalpy).ToString();
                    db.Update(eq, SessionProtectedSystem);
                    IList<TowerFireEq> list = db.GetAllList(SessionProtectedSystem, MainModel.ID);
                    foreach (TowerFireEq q in list)
                    {
                        EqList.Add(q);
                    }
                    SessionProtectedSystem.Flush();
                }
            }
            else if (eq.Type == "Other HX")
            {
                TowerFireOtherView v = new TowerFireOtherView();
                TowerFireOtherVM vm = new TowerFireOtherVM(eq.ID, SessionPlant, SessionProtectedSystem);
                v.DataContext = vm;
                v.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                if (v.ShowDialog() == true)
                {
                    EqList.Clear();
                    eq.WettedArea = vm.Area.ToString();
                    eq.HeatInput = Algorithm.GetQ(C1, double.Parse(eq.FFactor), double.Parse(eq.WettedArea)).ToString();
                    eq.ReliefLoad = (double.Parse(eq.HeatInput) / latentEnthalpy).ToString();
                    db.Update(eq, SessionProtectedSystem);
                    IList<TowerFireEq> list = db.GetAllList(SessionProtectedSystem, MainModel.ID);
                    foreach (TowerFireEq q in list)
                    {
                        EqList.Add(q);
                    }
                    SessionProtectedSystem.Flush();
                }
            }


        }


        private ICommand _TotalClick;
        public ICommand TotalClick
        {
            get
            {
                if (_TotalClick == null)
                {
                    _TotalClick = new RelayCommand(Run);

                }
                return _TotalClick;
            }
        }

        private void Run(object obj)
        {
            double reliefload = 0;
            foreach (TowerFireEq eq in EqList)
            {
                if (eq.FireZone)
                {
                    reliefload = reliefload + double.Parse(eq.ReliefLoad ?? "0");
                }
            }
            MainModel.ReliefLoad = reliefload.ToString();


            PSVDAL psvDAL=new PSVDAL();
            PSV psv=psvDAL.GetModel(SessionProtectedSystem);
            double pressure=double.Parse(psv.Pressure);

            double reliefFirePressure = pressure * 1.21;
            string tempdir = DirProtectedSystem + @"\temp\";
            string dirLatent = tempdir + "TowerFire";
            if (!Directory.Exists(dirLatent))
                Directory.CreateDirectory(dirLatent);

            IProIIReader reader = ProIIFactory.CreateReader(PrzVersion);
            reader.InitProIIReader(PrzFile);
            ProIIStreamData proIITray1StreamData = reader.CopyStream(EqName, 1, 2, 1);
            reader.ReleaseProIIReader();
            CustomStream stream = ProIIToDefault.ConvertProIIStreamToCustomStream(proIITray1StreamData);


            string gd = Guid.NewGuid().ToString();
            string vapor = "S_" + gd.Substring(0, 5).ToUpper();
            string liquid = "S_" + gd.Substring(gd.Length - 5, 5).ToUpper();
            int ImportResult = 0;
            int RunResult = 0;
            PROIIFileOperator.DecompressProIIFile(PrzFile, tempdir);
            string content = PROIIFileOperator.getUsableContent(stream.StreamName, tempdir);
            IFlashCalculate fcalc = ProIIFactory.CreateFlashCalculate(PrzVersion);
            string tray1_f = fcalc.Calculate(content, 1, reliefFirePressure.ToString(), 4, "", stream, vapor, liquid, dirLatent, ref ImportResult, ref RunResult);
            if (ImportResult == 1 || ImportResult == 2)
            {
                if (RunResult == 1 || RunResult == 2)
                {
                    reader = ProIIFactory.CreateReader(PrzVersion);
                    reader.InitProIIReader(tray1_f);
                    ProIIStreamData proIIVapor = reader.GetSteamInfo(vapor);
                    ProIIStreamData proIILiquid = reader.GetSteamInfo(liquid);
                    reader.ReleaseProIIReader();
                    CustomStream vaporFire = ProIIToDefault.ConvertProIIStreamToCustomStream(proIIVapor);
                    MainModel.ReliefLoad = vaporFire.WeightFlow;
                    MainModel.ReliefMW = vaporFire.BulkMwOfPhase;
                    MainModel.ReliefPressure = reliefFirePressure.ToString();
                    MainModel.ReliefTemperature = vaporFire.Temperature;
                    MainModel.ReliefCpCv = vaporFire.BulkCPCVRatio;
                    MainModel.ReliefZ = vaporFire.VaporZFmKVal;

                }

                else
                {
                    MessageBox.Show("Prz file is error", "Message Box");
                    return;
                }
            }
            else
            {
                MessageBox.Show("inp file is error", "Message Box");
                return;
            }

        }

        private void ReadConvert()
        {
            if (!string.IsNullOrEmpty(MainModel.ReliefLoad))
                MainModel.ReliefLoad = unitConvert.Convert(UOMEnum.MassRate, reliefloadUnit, double.Parse(MainModel.ReliefLoad)).ToString();
            if (!string.IsNullOrEmpty(MainModel.ReliefPressure))
                MainModel.ReliefPressure = unitConvert.Convert(UOMEnum.Pressure, reliefPressureUnit, double.Parse(MainModel.ReliefPressure)).ToString();
            if (!string.IsNullOrEmpty(MainModel.ReliefTemperature))
                MainModel.ReliefTemperature = unitConvert.Convert(UOMEnum.Temperature, reliefTemperatureUnit, double.Parse(MainModel.ReliefTemperature)).ToString();
        }
        private void WriteConvert()
        {
            if (!string.IsNullOrEmpty(MainModel.ReliefLoad))
                MainModel.ReliefLoad = unitConvert.Convert(reliefloadUnit, UOMEnum.MassRate, double.Parse(MainModel.ReliefLoad)).ToString();
            if (!string.IsNullOrEmpty(MainModel.ReliefPressure))
                MainModel.ReliefPressure = unitConvert.Convert(reliefPressureUnit, UOMEnum.Pressure, double.Parse(MainModel.ReliefPressure)).ToString();
            if (!string.IsNullOrEmpty(MainModel.ReliefTemperature))
                MainModel.ReliefTemperature = unitConvert.Convert(reliefTemperatureUnit, UOMEnum.Temperature, double.Parse(MainModel.ReliefTemperature)).ToString();
        }
        private void InitUnit()
        {
            this.reliefloadUnit = uomEnum.UserMassRate;
            this.reliefPressureUnit = uomEnum.UserPressure;
            this.reliefTemperatureUnit = uomEnum.UserTemperature;
        }
        #region 单位-字段
        private string reliefloadUnit;
        public string ReliefLoadUnit
        {
            get { return reliefloadUnit; }
            set
            {
                reliefloadUnit = value;
                this.OnPropertyChanged("ReliefLoadUnit");
            }
        }

        private string reliefTemperatureUnit;
        public string ReliefTemperatureUnit
        {
            get { return reliefTemperatureUnit; }
            set
            {
                reliefTemperatureUnit = value;
                this.OnPropertyChanged("ReliefTemperatureUnit");
            }
        }

        private string reliefPressureUnit;
        public string ReliefPressureUnit
        {
            get { return reliefPressureUnit; }
            set
            {
                reliefPressureUnit = value;
                this.OnPropertyChanged("ReliefPressureUnit");
            }
        }
        #endregion
    }
}