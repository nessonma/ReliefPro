﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NHibernate;
using ReliefProBLL.Common;
using ReliefProDAL;
using ReliefProDAL.GlobalDefault;
using ReliefProModel;
using ReliefProModel.GlobalDefault;
using ReliefProModel.Reports;
using UOMLib;

namespace ReliefProLL
{
    public class ReportBLL
    {
        private GlobalDefaultDAL globalDefaultDAL = new GlobalDefaultDAL();
        private PSVDAL psvDAL = new PSVDAL();
        private ScenarioDAL scenarioDAL = new ScenarioDAL();
        public ConcurrentBag<PSV> PSVBag;
        public ConcurrentBag<Scenario> ScenarioBag;
        public List<string> ProcessUnitReportPath;
        private PUsummaryGridDS SumDs = new PUsummaryGridDS();
        public string[] ScenarioName = new string[] {"Controlling Single Scenario", "General Electric Power Failure",
            "General Cooling Water Failure","General Instument Air Failure","Steam Failure","Fire" };

        List<string> listScenario = new List<string> { "PowerDS", "WaterDS", "AirDS", "SteamDS", "FireDS" };
        List<string> listProperty = new List<string> { "ReliefLoad", "ReliefMW", "ReliefTemperature", "ReliefZ" };
        List<PUsummaryGridDS> listGrid;
        List<double> tmpResult = new List<double>();
        public ReportBLL()
        { }
        public ReportBLL(List<string> ProcessUnitReportPath)
        {
            PSVBag = new ConcurrentBag<PSV>();
            ScenarioBag = new ConcurrentBag<Scenario>();
            this.ProcessUnitReportPath = ProcessUnitReportPath;
            GenerateDbSession();
        }
        public List<FlareSystem> GetDisChargeTo()
        {
            return globalDefaultDAL.GetFlareSystem(TempleSession.Session).ToList();
        }

        #region PlantSummary
        public List<PlantSummaryGridDS> CalcPlantSummary(List<PlantSummaryGridDS> listPlant)
        {
            List<string> listPlantCalc = new List<string> { "Direct Summation", "100-30-30", "Sum of first 2 Max", "100-50-50" };
            int CalcType = 0;
            listPlantCalc.ForEach(p =>
            {
                PlantSummaryGridDS plant = new PlantSummaryGridDS();
                plant.ControllingDS = new Scenario();
                plant.PowerDS = new Scenario();
                plant.WaterDS = new Scenario();
                plant.AirDS = new Scenario();
                plant.ProcessUnit = p;

                plant.ControllingDS.ReliefLoad = GetPlantSumResult(listPlant, CalcType, "ControllingDS", "ReliefLoad");
                plant.ControllingDS.ReliefMW = GetPlantSumResult(listPlant, CalcType, "ControllingDS", "ReliefMW");
                plant.ControllingDS.ReliefTemperature = GetPlantSumResult(listPlant, CalcType, "ControllingDS", "ReliefTemperature");

                plant.PowerDS.ReliefLoad = GetPlantSumResult(listPlant, CalcType, "PowerDS", "ReliefLoad");
                plant.PowerDS.ReliefMW = GetPlantSumResult(listPlant, CalcType, "PowerDS", "ReliefMW");
                plant.PowerDS.ReliefTemperature = GetPlantSumResult(listPlant, CalcType, "PowerDS", "ReliefTemperature");

                plant.WaterDS.ReliefLoad = GetPlantSumResult(listPlant, CalcType, "WaterDS", "ReliefLoad");
                plant.WaterDS.ReliefMW = GetPlantSumResult(listPlant, CalcType, "WaterDS", "ReliefMW");
                plant.WaterDS.ReliefTemperature = GetPlantSumResult(listPlant, CalcType, "WaterDS", "ReliefTemperature");

                plant.AirDS.ReliefLoad = GetPlantSumResult(listPlant, CalcType, "AirDS", "ReliefLoad");
                plant.AirDS.ReliefMW = GetPlantSumResult(listPlant, CalcType, "AirDS", "ReliefMW");
                plant.AirDS.ReliefTemperature = GetPlantSumResult(listPlant, CalcType, "AirDS", "ReliefTemperature");
                CalcType++;
                listPlant.Add(plant);
            });

            return listPlant;
        }

        public PlantSummaryGridDS GetPlantReprotDS(List<PUsummaryGridDS> ProcessUnitReprotDS, int CalcType)
        {
            PlantSummaryGridDS plant = new PlantSummaryGridDS();
            plant.ControllingDS = ProcessUnitReprotDS.OrderByDescending(p => p.SingleDS.ReliefLoad).FirstOrDefault().SingleDS;
            plant.PowerDS.ReliefLoad = GetPlantSumResult(ProcessUnitReprotDS, CalcType, "PowerDS", "ReliefLoad");
            plant.PowerDS.ReliefMW = GetPlantSumResult(ProcessUnitReprotDS, CalcType, "PowerDS", "ReliefMW");
            plant.PowerDS.ReliefTemperature = GetPlantSumResult(ProcessUnitReprotDS, CalcType, "PowerDS", "ReliefTemperature");

            plant.WaterDS.ReliefLoad = GetPlantSumResult(ProcessUnitReprotDS, CalcType, "WaterDS", "ReliefLoad");
            plant.WaterDS.ReliefMW = GetPlantSumResult(ProcessUnitReprotDS, CalcType, "WaterDS", "ReliefMW");
            plant.WaterDS.ReliefTemperature = GetPlantSumResult(ProcessUnitReprotDS, CalcType, "WaterDS", "ReliefTemperature");

            plant.AirDS.ReliefLoad = GetPlantSumResult(ProcessUnitReprotDS, CalcType, "AirDS", "ReliefLoad");
            plant.AirDS.ReliefMW = GetPlantSumResult(ProcessUnitReprotDS, CalcType, "AirDS", "ReliefMW");
            plant.AirDS.ReliefTemperature = GetPlantSumResult(ProcessUnitReprotDS, CalcType, "AirDS", "ReliefTemperature");

            return plant;
        }
        private string GetPlantSumResult<T>(List<T> ProcessUnitReprotDS, int CalcType, string ScenarioType, string ScenarioProperty)
        {
            if (CalcType == 2)
            {
                SumOfFirst2Max(ProcessUnitReprotDS, ScenarioType, ScenarioProperty);
            }
            double maxValue = ProcessUnitReprotDS.Max(p =>
            {
                Scenario scenario = (Scenario)p.GetType().GetProperty(ScenarioType).GetValue(p, null);
                if (scenario == null) return 0;
                var obj = scenario.GetType().GetProperty(ScenarioProperty).GetValue(scenario, null);
                if (obj == null) return 0;
                return double.Parse(obj.ToString());
            });
            ProcessUnitReprotDS.ForEach(p =>
             {
                 Scenario scenario = (Scenario)p.GetType().GetProperty(ScenarioType).GetValue(p, null);
                 if (scenario == null) return;
                 var obj = scenario.GetType().GetProperty(ScenarioProperty).GetValue(scenario, null);
                 if (obj == null) return;
                 double value = double.Parse(obj.ToString());
                 switch (CalcType)
                 {
                     case 0: DirectSummation(value); break;
                     case 1: Calc100_30_30(value, maxValue); break;
                     case 3: Calc100_50_50(value, maxValue); break;
                     default: break;
                 }
             });
            return CalcPlantSumResult();
        }

        private string CalcPlantSumResult()
        {
            double Result = tmpResult.Sum();
            tmpResult.Clear();
            return Result.ToString();
        }

        private void DirectSummation(double value)
        {
            tmpResult.Add(value);
        }
        private void SumOfFirst2Max<T>(List<T> ProcessUnitReprotDS, string ScenarioType, string ScenarioProperty)
        {
            //ProcessUnitReprotDS.OrderBy(p => { return p.PowerDS.ReliefLoad; }).Take(2);
            var query = ProcessUnitReprotDS.OrderByDescending(p =>
             {
                 //return p.GetType().GetProperty(ScenarioType).GetValue(p, null).GetType().GetProperty(ScenarioProperty);
                 Scenario scenario = (Scenario)p.GetType().GetProperty(ScenarioType).GetValue(p, null);
                 return scenario.GetType().GetProperty(ScenarioProperty).GetValue(scenario, null); ;
             }).Take(2);
            foreach (var result in query)
            {
                Scenario scenario = (Scenario)result.GetType().GetProperty(ScenarioType).GetValue(result, null);
                if (scenario == null) continue;
                object obj = scenario.GetType().GetProperty(ScenarioProperty).GetValue(scenario, null);
                if (obj != null)
                {
                    tmpResult.Add(double.Parse(obj.ToString()));
                }
            }
        }
        private void Calc100_30_30(double value, double maxValue)
        {
            CalcFun(value, maxValue, 0.3);
        }
        private void Calc100_50_50(double value, double maxValue)
        {
            CalcFun(value, maxValue, 0.5);
        }
        private void CalcFun(double value, double maxValue, double Scale)
        {
            if (value == maxValue)
            {
                if (tmpResult.Contains(value))
                {
                    tmpResult.Add(value * Scale);
                }
                else
                {
                    tmpResult.Add(value);
                }
            }
            else
            {
                tmpResult.Add(value * Scale);
            }
        }
        #endregion

        #region PUsummary


        public List<PUsummaryGridDS> GetPuReprotDS(List<PSV> listPSV)
        {
            var listGrid = new List<PUsummaryGridDS>();
            foreach (var PSV in listPSV)
            {
                PUsummaryGridDS gridDs = new PUsummaryGridDS();
                gridDs.psv = PSV;
                gridDs.PowerDS = this.ScenarioBag.FirstOrDefault(p => p.ScenarioName == this.ScenarioName[1] && p.dbPath == PSV.dbPath);
                gridDs.WaterDS = this.ScenarioBag.FirstOrDefault(p => p.ScenarioName == this.ScenarioName[2] && p.dbPath == PSV.dbPath);
                gridDs.AirDS = this.ScenarioBag.FirstOrDefault(p => p.ScenarioName == this.ScenarioName[3] && p.dbPath == PSV.dbPath);
                gridDs.SteamDS = this.ScenarioBag.FirstOrDefault(p => p.ScenarioName == this.ScenarioName[4] && p.dbPath == PSV.dbPath);
                gridDs.FireDS = this.ScenarioBag.FirstOrDefault(p => p.ScenarioName == this.ScenarioName[5] && p.dbPath == PSV.dbPath);
                InitControllingSingleScenarioDS(ref gridDs);
                listGrid.Add(gridDs);
            }
            return listGrid;
        }

        #region  ControllingSingleScenarioDS
        private void InitControllingSingleScenarioDS(ref PUsummaryGridDS gridDs)
        {
            double maxPowerDS = !string.IsNullOrEmpty(gridDs.PowerDS.ReliefLoad) ? double.Parse(gridDs.PowerDS.ReliefLoad) : 0;
            double maxWaterDS = !string.IsNullOrEmpty(gridDs.WaterDS.ReliefLoad) ? double.Parse(gridDs.WaterDS.ReliefLoad) : 0;
            double maxAirDS = !string.IsNullOrEmpty(gridDs.AirDS.ReliefLoad) ? double.Parse(gridDs.AirDS.ReliefLoad) : 0;
            double maxSteamDS = !string.IsNullOrEmpty(gridDs.SteamDS.ReliefLoad) ? double.Parse(gridDs.SteamDS.ReliefLoad) : 0;
            double maxFireDS = !string.IsNullOrEmpty(gridDs.FireDS.ReliefLoad) ? double.Parse(gridDs.FireDS.ReliefLoad) : 0;
            List<double> MaxList = new List<double> { maxPowerDS, maxWaterDS, maxAirDS, maxSteamDS, maxFireDS };
            var v = MaxList.Select((m, index) => new { index, m }).OrderByDescending(n => n.m).Take(1);
            int Index = 0;
            foreach (var t in v)
            {
                Index = t.index; break;
            }
            switch (Index)
            {
                case 0: gridDs.SingleDS = gridDs.PowerDS; break;
                case 1: gridDs.SingleDS = gridDs.WaterDS; break;
                case 2: gridDs.SingleDS = gridDs.AirDS; break;
                case 3: gridDs.SingleDS = gridDs.SteamDS; break;
                case 4: gridDs.SingleDS = gridDs.FireDS; break;
                default: break;
            }
        }
        #endregion

        #region Process Unit ALL Info
        private void GetPSVInfo(ISession SessionPS)
        {
            var PSVInfo = psvDAL.GetAllList(SessionPS).ToList();
            PSVInfo.ForEach(psv => { psv.dbPath = SessionPS.Connection.ConnectionString; PSVBag.Add(psv); });
        }
        private void GetScenarioInfo(ISession SessionPS)
        {
            var ScenarioInfo = scenarioDAL.GetAllList(SessionPS).ToList();
            ScenarioInfo.ForEach(p => { p.dbPath = SessionPS.Connection.ConnectionString; ScenarioBag.Add(p); });
        }
        private void InitInfo()
        {
            PSVBag.AsParallel().ForAll(p =>
            {
                foreach (string scenarioName in ScenarioName)
                {
                    var scenarioInfo = ScenarioBag.FirstOrDefault(s => s.dbPath == p.dbPath && s.ScenarioName == scenarioName);
                    if (scenarioInfo == null || scenarioInfo.ID <= 0)
                    {
                        Scenario scenario = new Scenario();
                        scenario.dbPath = p.dbPath;
                        scenario.ScenarioName = scenarioName;
                        ScenarioBag.Add(scenario);
                    }
                }
            });

            PSVBag.OrderBy(p => p.dbPath);
            ScenarioBag.OrderBy(p => p.dbPath).ThenBy(p => p.ScenarioName);
        }
        private void GenerateDbSession()
        {
            ProcessUnitReportPath.AsParallel().ForAll(p =>
            {
                NHibernateHelper helperProtectedSystem = new NHibernateHelper(p);
                var tmpSession = helperProtectedSystem.GetCurrentSession();
                GetPSVInfo(tmpSession);
                GetScenarioInfo(tmpSession);
            });
            //Parallel.ForEach(ReportPath, (p, loopState) =>
            //{
            //    countdown.AddCount();
            //    NHibernateHelper helperProtectedSystem = new NHibernateHelper(p);
            //    var tmpSession = helperProtectedSystem.GetCurrentSession();
            //    GetPSVInfo(tmpSession);
            //    GetScenarioInfo(tmpSession);
            //    countdown.Signal();
            //});
            //countdown.Signal();
            //countdown.Wait();
            InitInfo();
        }
        #endregion

        #region PUsummary Subtotal
        public List<PUsummaryGridDS> CalcMaxSum(List<PUsummaryGridDS> lstGrid)
        {
            this.listGrid = lstGrid;
            SumDs.psv = new PSV();
            SumDs.psv.ValveType = "Summation";
            SumDs.SingleDS = new Scenario();
            Calc();
            listGrid.Add(SumDs);

            PUsummaryGridDS MaxDs = new PUsummaryGridDS();
            MaxDs.psv = new PSV();
            MaxDs.psv.ValveType = "Max";
            MaxDs.SingleDS = new Scenario();
            MaxDs.SingleDS.Phase = string.Empty;
            MaxDs.SingleDS.ReliefLoad = listGrid.Max(p =>
            {
                if (p.SingleDS == null) return null;
                if (!string.IsNullOrEmpty(p.SingleDS.ReliefLoad))
                    return double.Parse(p.SingleDS.ReliefLoad);
                else return null;
            }).ToString();
            listGrid.Insert(listGrid.Count - 1, MaxDs);
            return listGrid;
        }
        private void Calc()
        {
            SumDs.PowerDS = new Scenario();
            SumDs.WaterDS = new Scenario();
            SumDs.AirDS = new Scenario();
            SumDs.SteamDS = new Scenario();
            SumDs.FireDS = new Scenario();
            listScenario.ForEach(p =>
            {
                CalcSum(p);
            });
            SumDs.SingleDS.Phase = string.Empty;
            SumDs.PowerDS.Phase = string.Empty;
            SumDs.WaterDS.Phase = string.Empty;
            SumDs.AirDS.Phase = string.Empty;
            SumDs.SteamDS.Phase = string.Empty;
            SumDs.FireDS.Phase = string.Empty;
        }
        private void CalcSum(string ScenarioType)
        {
            PropertyInfo pInfo = SumDs.GetType().GetProperty(ScenarioType);
            Scenario scenario = (Scenario)SumDs.GetType().GetProperty(ScenarioType).GetValue(SumDs, null);

            listProperty.ForEach(p =>
            {
                PropertyInfo pInfoScenario = scenario.GetType().GetProperty(p);
                string result = GetSumResult(ScenarioType, p);
                pInfoScenario.SetValue(scenario, result, null);
            });

            pInfo.SetValue(SumDs, scenario, null);
        }
        private string GetSumResult(string ScenarioType, string ScenarioProperty)
        {
            object obj;
            double? a, b;
            a = listGrid.Sum(p =>
            {
                Scenario scenario = (Scenario)p.GetType().GetProperty(ScenarioType).GetValue(p, null);
                if (scenario == null) return null;
                obj = scenario.GetType().GetProperty(ScenarioProperty).GetValue(scenario, null);
                if (obj == null) return null;
                else return double.Parse(obj.ToString());
            });
            a = a ?? 0;
            if (ScenarioProperty.Equals("ReliefLoad"))
            {
                return a == 0 ? "" : a.ToString();
            }
            if (ScenarioProperty.Equals("ReliefMW"))
            {
                GetSumResult(ScenarioType, "ReliefLoad");
            }

            b = listGrid.Sum(p =>
            {
                Scenario scenario = (Scenario)p.GetType().GetProperty(ScenarioType).GetValue(p, null);
                if (scenario == null) return null;
                obj = scenario.GetType().GetProperty(ScenarioProperty).GetValue(scenario, null);
                if (obj == null) return null;
                if (!string.IsNullOrEmpty(scenario.ReliefMW))
                {
                    double mw = double.Parse(scenario.ReliefMW);
                    if (mw != 0)
                        return double.Parse(obj.ToString()) / mw;
                }
                return null;
            });
            b = b == 0 ? null : b;

            double? reslut = a / b;
            reslut = reslut ?? 0;
            return reslut == 0 ? "" : reslut.ToString();
        }
        #endregion

        #endregion
    }
}
