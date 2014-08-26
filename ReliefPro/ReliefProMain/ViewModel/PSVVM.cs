﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.IO;
using System.Windows;
using ReliefProModel;
using ReliefProMain.Commands;
using ReliefProDAL;
using ReliefProBLL.Common;
using ReliefProMain.Interface;
using ReliefProMain.Service;
using ReliefProMain.Models;
using UOMLib;
using NHibernate;
using ProII;
using ReliefProCommon.CommonLib;
using ReliefProDAL.GlobalDefault;
using ReliefProModel.GlobalDefault;
using System.Threading;
using System.Collections.ObjectModel;

namespace ReliefProMain.ViewModel
{
    public class PSVVM : ViewModelBase
    {
        private ISession SessionPlant { set; get; }
        private ISession SessionProtectedSystem { set; get; }
        private string DirPlant { set; get; }
        private string DirProtectedSystem { set; get; }
        public SourceFile SourceFileInfo { set; get; }
        public string EqName { get; set; }
        public string EqType { get; set; }
        public string FileFullPath { get; set; }
        private string psvPressureUnit;
        public string PSVPressureUnit
        {
            get { return psvPressureUnit; }
            set
            {
                psvPressureUnit = value;
                OnPropertyChanged("PSVPressureUnit");
            }
        }

        private string drumPSVPressureUnit;
        public string DrumPressureUnit
        {
            get { return drumPSVPressureUnit; }
            set
            {
                drumPSVPressureUnit = value;
                OnPropertyChanged("DrumPressureUnit");
            }
        }


        private string _CriticalPressureUnit;
        public string CriticalPressureUnit
        {
            get { return _CriticalPressureUnit; }
            set
            {
                _CriticalPressureUnit = value;
                OnPropertyChanged("CriticalPressureUnit");
            }
        }
        private double _CriticalPressure;
        public double CriticalPressure
        {
            get { return _CriticalPressure; }
            set
            {
                _CriticalPressure = value;
                OnPropertyChanged("CriticalPressure");
            }
        }
        private string _ReflexDrumVisible;
        public string ReflexDrumVisible
        {
            get { return _ReflexDrumVisible; }
            set
            {
                _ReflexDrumVisible = value;
                OnPropertyChanged("ReflexDrumVisible");
            }
        }
        public ObservableCollection<string> ValveTypes { get; set; }
        public PSVModel CurrentModel { get; set; }

        public ObservableCollection<string> DischargeTos { get; set; }
        public ObservableCollection<string> Locations { get; set; }
        private bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }
        public PSV psv;
        PSVDAL dbpsv = new PSVDAL();
        UOMLib.UOMEnum uomEnum;
        public ObservableCollection<string> GetValveTypes()
        {
            ObservableCollection<string> list = new ObservableCollection<string>();
            list.Add("Modulation");
            list.Add("Pop Action");
            list.Add("Rupture Disk");
            list.Add("Temperature Actuated");
            return list;
        }
        public ObservableCollection<string> GetDischargeTos()
        {
            ObservableCollection<string> list = new ObservableCollection<string>();
            GlobalDefaultDAL gdDAL = new GlobalDefaultDAL();
            IList<FlareSystem> fs = gdDAL.GetFlareSystem(SessionPlant);
            foreach (FlareSystem m in fs)
            {
                list.Add(m.FlareName);
            }

            return list;
        }
        public ObservableCollection<string> GetLocations()
        {
            ObservableCollection<string> list = new ObservableCollection<string>();
            ProIIEqDataDAL gdDAL = new ProIIEqDataDAL();
            IList<ProIIEqData> fs = gdDAL.GetAllList(SessionPlant);
            foreach (ProIIEqData m in fs)
            {
                list.Add(m.EqName);
            }
            return list;
        }



        public PSVVM(string eqName, string eqType, SourceFile sourceFileInfo, ISession sessionPlant, ISession sessionProtectedSystem, string dirPlant, string dirProtectedSystem)
        {
            EqName = eqName;
            EqType = eqType;
            SessionPlant = sessionPlant;
            SessionProtectedSystem = sessionProtectedSystem;
            DirPlant = dirPlant;
            DirProtectedSystem = dirProtectedSystem;
            SourceFileInfo = sourceFileInfo;
            ValveTypes = GetValveTypes();
            DischargeTos = GetDischargeTos();
            Locations = GetLocations();
            uomEnum = new UOMLib.UOMEnum(sessionPlant);
            this.psvPressureUnit = uomEnum.UserPressure;
            this.drumPSVPressureUnit = uomEnum.UserPressure;
            if (eqType == "Tower")
            {
                ReflexDrumVisible = "Visible";
            }
            else
            {
                ReflexDrumVisible = "Hidden";
            }

            psv = dbpsv.GetModel(sessionProtectedSystem);
            if (psv != null)
            {
                CurrentModel = ConvertModel(psv);
            }
            else
            {
                psv = new PSV();
                CurrentModel = new PSVModel();
                CurrentModel.ValveType = ValveTypes[0];
                if (DischargeTos.Count > 0)
                {
                    CurrentModel.DischargeTo = DischargeTos[0];
                }
                if (eqType != "StorageTank")
                {
                    CurrentModel.Location = eqName;
                }
                
            }
        }
        private PSVModel ConvertModel(PSV m)
        {
            PSVModel model = new PSVModel();
            model.ID = psv.ID;
            model.PSVName = m.PSVName;
            model.Pressure = UnitConvert.Convert(UOMEnum.Pressure, uomEnum.UserPressure, m.Pressure);
            model.ReliefPressureFactor = m.ReliefPressureFactor;
            model.ValveNumber = m.ValveNumber;
            model.ValveType = m.ValveType;
            model.DrumPSVName = m.DrumPSVName;
            model.Location = m.Location;
            if (m.DrumPressure != null)
            {
                model.DrumPressure = UnitConvert.Convert(UOMEnum.Pressure, uomEnum.UserPressure, m.DrumPressure);
            }
            model.Description = m.Description;
            model.LocationDescription = m.LocationDescription;
            model.DischargeTo = m.DischargeTo;
            return model;
        }
        private void ConvertModel(PSVModel m, ref PSV model)
        {
            model.PSVName = m.PSVName;
            model.Pressure = UnitConvert.Convert(psvPressureUnit, UOMEnum.Pressure, m.Pressure);
            model.ReliefPressureFactor = m.ReliefPressureFactor;
            model.ValveNumber = m.ValveNumber;
            model.ValveType = m.ValveType;
            model.DrumPSVName = m.DrumPSVName;
            model.Location = m.Location;
            if (m.DrumPressure != null)
            {
                model.DrumPressure = UnitConvert.Convert(DrumPressureUnit, UOMEnum.Pressure, m.DrumPressure);
            }
            model.Description = m.Description;
            model.LocationDescription = m.LocationDescription;
            model.DischargeTo = m.DischargeTo;
        }


        private ICommand _SaveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (_SaveCommand == null)
                {
                    _SaveCommand = new RelayCommand(Save);

                }
                return _SaveCommand;
            }
        }

        private void Save(object window)
        {
            if (string.IsNullOrEmpty(CurrentModel.PSVName))
            {
                MessageBox.Show("PSV Name can't be empty.", "Message Box");
                return;
            }
            if (CurrentModel.Pressure <= 0)
            {
                MessageBox.Show("PSV Pressure can't be empty.", "Message Box");
                return;
            }
            if (CurrentModel.ReliefPressureFactor <= 0)
            {
                MessageBox.Show("Relief Pressure Factor can't be empty.", "Message Box");
                return;
            }
            if (string.IsNullOrEmpty(CurrentModel.Location))
            {
                MessageBox.Show("Location must be selected.", "Message Box");
                return;
            }
            if (string.IsNullOrEmpty(CurrentModel.DischargeTo))
            {
                MessageBox.Show("Discharge To can't be empty.", "Message Box");
                return;
            }
            if (string.IsNullOrEmpty(CurrentModel.ValveType))
            {
                MessageBox.Show("Valve Type can't be empty.", "Message Box");
                return;
            }
            if (CurrentModel.ValveNumber<=0)
            {
                MessageBox.Show("Valve Number can't be zero.", "Message Box");
                return;
            }
            try
            {

                Task.Factory.StartNew(() =>
                {

                });

                this.IsBusy = true;
                var t1 = Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(3000);

                    if (CurrentModel.ID == 0)
                    {
                        if (EqType == "Tower")
                        {
                            CreateTowerPSV();
                        }
                        ConvertModel(CurrentModel, ref psv);
                        dbpsv.Add(psv, SessionProtectedSystem);
                    }
                    else if (psv.ReliefPressureFactor == CurrentModel.ReliefPressureFactor && psv.Pressure == CurrentModel.Pressure)
                    {
                        ConvertModel(CurrentModel, ref psv);
                        dbpsv.Update(psv, SessionProtectedSystem);
                        SessionProtectedSystem.Flush();
                    }
                    else
                    {
                        //需要重新复制一份ProtectedSystem.mdb，相当于重新分析。----error
                        //string dbProtectedSystem = AppDomain.CurrentDomain.BaseDirectory.ToString() + @"template\protectedsystem.mdb";
                        //string dbProtectedSystem_target = DirProtectedSystem + @"\protectedsystem.mdb";
                        //System.IO.File.Copy(dbProtectedSystem, dbProtectedSystem_target, true);
                        //NHibernateHelper helperProtectedSystem = new NHibernateHelper(dbProtectedSystem_target);
                        //SessionProtectedSystem = helperProtectedSystem.GetCurrentSession();

                        //应该是删除所有的和定压相关的表的数据

                        ClearData();
                        if (EqType == "Tower")
                        {
                            CreateTowerPSV();
                        }
                        ConvertModel(CurrentModel, ref psv);
                        dbpsv.Add(psv, SessionProtectedSystem);
                    }
                    Thread.Sleep(3000);
                }).ContinueWith((t) => { });

                Task.WaitAll(t1);
                this.IsBusy = false;

                System.Windows.Window wd = window as System.Windows.Window;
                if (wd != null)
                {
                    wd.DialogResult = true;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }


        }
        public void CreateTowerPSV()
        {
            //判断压力是否更改，relief pressure 是否更改。  （drum的是否修改，会影响到火灾的计算）
            string FileFullPath = DirPlant + @"\" + SourceFileInfo.FileNameNoExt + @"\" + SourceFileInfo.FileName;
            string tempdir = DirProtectedSystem + @"\temp\";
            if (!Directory.Exists(tempdir))
                Directory.CreateDirectory(tempdir);

            string dirPhase = tempdir + "Phase";
            if (!Directory.Exists(dirPhase))
                Directory.CreateDirectory(dirPhase);
            string dirLatent = tempdir + "Latent";
            if (!Directory.Exists(dirLatent))
                Directory.CreateDirectory(dirLatent);
            string dirCopyStream = tempdir + "CopyStream";
            if (!Directory.Exists(dirCopyStream))
                Directory.CreateDirectory(dirCopyStream);

            string copyFile = dirCopyStream + @"\" + SourceFileInfo.FileName;
            File.Copy(FileFullPath, copyFile, true);
            CustomStream stream = CopyTop1Liquid(copyFile);
            double internPressure = UnitConvert.Convert("MPAG", "KPA", stream.Pressure);
            if (internPressure == 0)
            {
                MessageBox.Show("Please Rerun this ProII file and save it.", "Message Box");
                return;
            }
            PROIIFileOperator.DecompressProIIFile(FileFullPath, tempdir);

            string phasecontent = PROIIFileOperator.getUsablePhaseContent(stream.StreamName, tempdir);
            double ReliefPressure = CurrentModel.ReliefPressureFactor * CurrentModel.Pressure;

            double criticalPressure = 0;
            bool b = CalcCriticalPressure(phasecontent, ReliefPressure, stream, dirPhase, ref criticalPressure);
            if (b == false)
                return;
            double latentEnthalpy = 0;
            double ReliefTemperature = 0;
            LatentProduct latentVapor = new LatentProduct();
            LatentProduct latentLiquid = new LatentProduct();
            string content = PROIIFileOperator.getUsableContent(stream.StreamName, tempdir);
            if (criticalPressure > ReliefPressure)
            {
                b = CalcLatent(content, ReliefPressure, stream, dirLatent, ref latentVapor, ref latentLiquid);
                if (b == false)
                    return;
                latentEnthalpy = latentVapor.SpEnthalpy - latentLiquid.SpEnthalpy;
                ReliefTemperature = latentVapor.Temperature;
            }
            else
            {
                latentEnthalpy = 116.3152;
                ReliefTemperature = stream.Temperature;
            }
            IList<CustomStream> products = null;
            CustomStreamDAL dbstream = new CustomStreamDAL();
            products = dbstream.GetAllList(SessionProtectedSystem, true);
            double overHeadPressure = GetTowerOverHeadPressure(products);

            int ImportResult = 0;
            int RunResult = 0;
            List<FlashResult> listFlashResult = new List<FlashResult>();
            int count = products.Count;
            for (int i = 1; i <= count; i++)
            {
                CustomStream p = products[i - 1];
                if (p.TotalMolarRate != null && p.TotalMolarRate > 0)
                {
                    IFlashCalculate fc = ProIIFactory.CreateFlashCalculate(SourceFileInfo.FileVersion);
                    string gd = Guid.NewGuid().ToString();
                    string vapor = "S_" + gd.Substring(0, 5).ToUpper();
                    string liquid = "S_" + gd.Substring(gd.Length - 5, 5).ToUpper();
                    string prodtype = p.ProdType;
                    int tray = p.Tray;
                    string streamname = p.StreamName;
                    double strPressure = p.Pressure;
                    double strTemperature = p.Temperature;
                    string f = string.Empty;


                    string dirflash = tempdir + p.StreamName;
                    if (!Directory.Exists(dirflash))
                        Directory.CreateDirectory(dirflash);
                    double prodpressure = 0;
                    if (p.Pressure != null)
                    {
                        prodpressure = p.Pressure;
                    }
                    string usablecontent = PROIIFileOperator.getUsableContent(p.StreamName, tempdir);

                    if (prodtype == "4" || (prodtype == "2" && tray == 1)) // 2个条件是等同含义，后者是有气有液
                    {
                        f = fc.Calculate(usablecontent, 1, ReliefPressure.ToString(), 4, "", p, vapor, liquid, dirflash, ref ImportResult, ref RunResult);
                    }

                    else if (prodtype == "6" || prodtype == "3") //3 气相  6 沉积水 
                    {
                        f = fc.Calculate(usablecontent, 2, ReliefTemperature.ToString(), 4, "", p, vapor, liquid, dirflash, ref ImportResult, ref RunResult);
                    }
                    else
                    {
                        double press = ReliefPressure + (p.Pressure - overHeadPressure);
                        f = fc.Calculate(usablecontent, 1, press.ToString(), 3, "", p, vapor, liquid, dirflash, ref ImportResult, ref RunResult);
                    }
                    if (ImportResult == 1 || ImportResult == 2)
                    {
                        if (RunResult == 1 || RunResult == 2)
                        {
                            FlashResult fr = new FlashResult();
                            fr.LiquidName = liquid;
                            fr.VaporName = vapor;
                            fr.StreamName = streamname;
                            fr.PrzFile = f;
                            fr.Tray = tray;
                            fr.ProdType = prodtype;
                            listFlashResult.Add(fr);
                        }
                        else
                        {
                            MessageBoxResult r = MessageBox.Show("Prz file is error", "Message Box", MessageBoxButton.OKCancel);
                            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe", f);

                            if (r == MessageBoxResult.Yes)
                            {
                                FlashResult fr = new FlashResult();
                                fr.LiquidName = liquid;
                                fr.VaporName = vapor;
                                fr.StreamName = streamname;
                                fr.PrzFile = f;
                                fr.Tray = tray;
                                fr.ProdType = prodtype;
                                listFlashResult.Add(fr);
                            }
                            return;
                        }
                    }
                    else
                    {
                        MessageBoxResult r = MessageBox.Show("inp file is error", "Message Box", MessageBoxButton.OKCancel);
                        if (r == MessageBoxResult.Yes)
                        {
                            FlashResult fr = new FlashResult();
                            fr.LiquidName = liquid;
                            fr.VaporName = vapor;
                            fr.StreamName = streamname;
                            fr.PrzFile = f;
                            fr.Tray = tray;
                            fr.ProdType = prodtype;
                            listFlashResult.Add(fr);
                        }
                        return;
                    }
                }
            }

            List<TowerFlashProduct> listFlashProduct = new List<TowerFlashProduct>();
            count = listFlashResult.Count;
            for (int i = 1; i <= count; i++)
            {
                FlashResult fr = listFlashResult[i - 1];
                string prodtype = fr.ProdType;
                int tray = fr.Tray;
                if (fr.PrzFile != "")
                {
                    CustomStream cs = null;
                    IProIIReader reader = ProIIFactory.CreateReader(SourceFileInfo.FileVersion);
                    reader.InitProIIReader(fr.PrzFile);
                    TowerFlashProduct product = new TowerFlashProduct();
                    if (prodtype == "4" || (prodtype == "2" && tray == 1) || prodtype == "3" || prodtype == "6")
                    {
                        ProIIStreamData data = reader.GetSteamInfo(fr.VaporName);
                        cs = ProIIToDefault.ConvertProIIStreamToCustomStream(data);
                    }
                    else
                    {
                        ProIIStreamData data = reader.GetSteamInfo(fr.LiquidName);
                        cs = ProIIToDefault.ConvertProIIStreamToCustomStream(data);

                    }
                    reader.ReleaseProIIReader();
                    product.SpEnthalpy = cs.SpEnthalpy;
                    product.StreamName = fr.StreamName;
                    product.WeightFlow = cs.WeightFlow;
                    product.ProdType = fr.ProdType;
                    product.Tray = tray;
                    product.Temperature = cs.Temperature;

                    listFlashProduct.Add(product);
                }
            }



            LatentDAL dblatent = new LatentDAL();
            //dbLatentProduct dblatentproduct = new dbLatentProduct();
            TowerFlashProductDAL dbFlashProduct = new TowerFlashProductDAL();


            Critical c = new Critical();
            c.CriticalPressure = criticalPressure;
            CriticalDAL dbcritical = new CriticalDAL();
            dbcritical.Add(c, SessionProtectedSystem);



            Latent latent = new Latent();
            latent.LatentEnthalpy = latentEnthalpy;
            latent.ReliefTemperature = ReliefTemperature;
            latent.ReliefOHWeightFlow = latentVapor.BulkMwOfPhase;
            latent.ReliefPressure = CurrentModel.Pressure * CurrentModel.ReliefPressureFactor;
            dblatent.Add(latent, SessionProtectedSystem);

            foreach (TowerFlashProduct p in listFlashProduct)
            {
                dbFlashProduct.Add(p, SessionProtectedSystem);
            }


        }


        private CustomStream CopyTop1Liquid(string copyPrzFile)
        {
            CustomStream cs = new CustomStream();
            IProIIReader reader = ProIIFactory.CreateReader(SourceFileInfo.FileVersion);
            reader.InitProIIReader(copyPrzFile);
            ProIIStreamData proIITray1StreamData = reader.CopyStream(EqName, 1, 2, 1);
            reader.ReleaseProIIReader();
            cs = ProIIToDefault.ConvertProIIStreamToCustomStream(proIITray1StreamData);
            return cs;
        }


        private double GetTowerOverHeadPressure(IList<CustomStream> products)
        {
            double overHeadPressure = 0;
            foreach (CustomStream cs in products)
            {
                if (cs.ProdType == "3" || cs.ProdType == "4")
                {
                    overHeadPressure = cs.Pressure;
                    break;
                }
            }
            return overHeadPressure;

        }

        private bool CalcCriticalPressure(string content, double ReliefPressure, CustomStream stream, string dirPhase, ref double criticalPressure)
        {
            int ImportResult = 0;
            int RunResult = 0;
            criticalPressure = 0;
            IPHASECalculate PhaseCalc = ProIIFactory.CreatePHASECalculate(SourceFileInfo.FileVersion);
            string PH = "PH" + Guid.NewGuid().ToString().Substring(0, 4);
            string criticalPress = string.Empty;
            string phasef = PhaseCalc.Calculate(content, 1, ReliefPressure.ToString(), 4, "", stream, PH, dirPhase, ref ImportResult, ref RunResult);
            if (ImportResult == 1 || ImportResult == 2)
            {
                if (RunResult == 1 || RunResult == 2)
                {
                    IProIIReader reader = ProIIFactory.CreateReader(SourceFileInfo.FileVersion);
                    reader.InitProIIReader(phasef);
                    criticalPress = reader.GetCriticalPressure(PH);
                    reader.ReleaseProIIReader();
                    criticalPress = UnitConvert.Convert("KPA", "MPAG", double.Parse(criticalPress)).ToString();
                    criticalPressure = double.Parse(criticalPress);
                    return true;
                }
                else
                {
                    MessageBox.Show("Prz file is error", "Message Box");
                    return false;
                }
            }
            else
            {
                MessageBox.Show("inp file is error", "Message Box");
                return false;
            }

        }

        private bool CalcLatent(string content, double ReliefPressure, CustomStream stream, string dirLatent, ref LatentProduct latentVapor, ref LatentProduct latentLiquid)
        {
            LatentProductDAL dblp = new LatentProductDAL();
            int ImportResult = 0;
            int RunResult = 0;
            string gd = Guid.NewGuid().ToString();
            string vapor = "S_" + gd.Substring(0, 5).ToUpper();
            string liquid = "S_" + gd.Substring(gd.Length - 5, 5).ToUpper();
            IFlashCalculate fcalc = ProIIFactory.CreateFlashCalculate(SourceFileInfo.FileVersion);
            string tray1_f = fcalc.Calculate(content, 1, ReliefPressure.ToString(), 4, "", stream, vapor, liquid, dirLatent, ref ImportResult, ref RunResult);
            if (ImportResult == 1 || ImportResult == 2)
            {
                if (RunResult == 1 || RunResult == 2)
                {
                    IProIIReader reader = ProIIFactory.CreateReader(SourceFileInfo.FileVersion);
                    reader.InitProIIReader(tray1_f);
                    ProIIStreamData proIIVapor = reader.GetSteamInfo(vapor);
                    ProIIStreamData proIILiquid = reader.GetSteamInfo(liquid);
                    reader.ReleaseProIIReader();
                    latentVapor = ProIIToDefault.ConvertProIIStreamToLatentProduct(proIIVapor);
                    latentVapor.ProdType = "1";
                    latentLiquid = ProIIToDefault.ConvertProIIStreamToLatentProduct(proIILiquid);
                    latentVapor.ProdType = "2";

                    dblp.Add(latentVapor, SessionProtectedSystem);
                    dblp.Add(latentLiquid, SessionProtectedSystem);
                    return true;
                }

                else
                {
                    MessageBox.Show("Prz file is error", "Message Box");
                    return false;
                }
            }
            else
            {
                MessageBox.Show("inp file is error", "Message Box");
                return false;
            }
        }


        private void ClearData()
        {
            LatentDAL ldal = new LatentDAL();          
            ldal.RemoveALL( SessionProtectedSystem);

            LatentProductDAL lpdal = new LatentProductDAL();
            lpdal.RemoveALL(SessionProtectedSystem);

            PSVDAL psvdal = new PSVDAL();
            psvdal.RemoveALL(SessionProtectedSystem);

        }

        

    }
}
