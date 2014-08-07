﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ReliefProModel;
using ReliefProMain.Commands;
using ReliefProDAL;
using ReliefProBLL.Common;
using NHibernate;
using ReliefProMain.View;
using UOMLib;
using ReliefProModel.Compressors;
using ReliefProDAL.Compressors;

namespace ReliefProMain.ViewModel
{
    public class CompressorVM:ViewModelBase
    {
        private ISession SessionPlant { set; get; }
        private ISession SessionProtectedSystem { set; get; }
        public string DirPlant { set; get; }
        public string DirProtectedSystem { set; get; }
        public SourceFile SourceFileInfo { set; get; }
        public string FileName { set; get; }
        private ProIIEqData ProIICompressor;

        private ObservableCollection<string> _CompressorTypes;
        public ObservableCollection<string> CompressorTypes
        {
            get
            {
                return this._CompressorTypes;
            }
            set
            {
                this._CompressorTypes = value;
                OnPropertyChanged("CompressorTypes");
            }
        }

        private string _CompressorName;
        public string CompressorName
        {
            get
            {
                return this._CompressorName;
            }
            set
            {
                this._CompressorName = value;
                OnPropertyChanged("CompressorName");
            }
        }
        private string _CompressorType;
        public string CompressorType
        {
            get
            {
                return this._CompressorType;
            }
            set
            {
                this._CompressorType = value;
                OnPropertyChanged("CompressorType");
            }
        }
        public Compressor CurrentCompressor { get; set; }
        private string _Duty;
        public string Duty
        {
            get
            {
                return this._Duty;
            }
            set
            {
                this._Duty = value;
                OnPropertyChanged("Duty");
            }
        }

       
        

        private ObservableCollection<CustomStream> _Feeds;
        public ObservableCollection<CustomStream> Feeds
        {
            get { return _Feeds; }
            set
            {
                _Feeds = value;
                OnPropertyChanged("Feeds");
            }
        }
        private ObservableCollection<CustomStream> _Products;
        public ObservableCollection<CustomStream> Products
        {
            get { return _Products; }
            set
            {
                _Products = value;
                OnPropertyChanged("Products");
            }
        }
        List<string> dicFeeds = new List<string>();
        List<string> dicProducts = new List<string>();
        List<string> dicProductTypes = new List<string>();
        public CompressorVM(string CompressorName, ISession sessionPlant, ISession sessionProtectedSystem, string dirPlant, string dirProtectedSystem)
        {
            SessionPlant = sessionPlant;
            SessionProtectedSystem = sessionProtectedSystem;
            DirPlant = dirPlant;
            DirProtectedSystem = dirProtectedSystem;
            CompressorTypes = GetCompressorTypes();
            this.CompressorName = CompressorName;
            if (!string.IsNullOrEmpty(CompressorName))
            {
                CompressorType = CompressorTypes[0];
                Feeds = GetStreams(SessionProtectedSystem, false);
                Products = GetStreams(SessionProtectedSystem, true);

                CompressorDAL dbCompressor = new CompressorDAL();
                CurrentCompressor = dbCompressor.GetModel(SessionProtectedSystem);
                if (CurrentCompressor != null)
                {
                    CompressorName = CurrentCompressor.CompressorName;                  
                    CompressorType = CurrentCompressor.CompressorType;
                    SourceFileDAL sfdal = new SourceFileDAL();
                    SourceFileInfo = sfdal.GetModel(CurrentCompressor.SourceFile, SessionPlant);
                    FileName = CurrentCompressor.SourceFile;
                }
            }
            else
            {

            }
        }
        private ICommand _ImportCommand;
        public ICommand ImportCommand
        {
            get
            {
                if (_ImportCommand == null)
                {
                    _ImportCommand = new RelayCommand(Import);

                }
                return _ImportCommand;
            }
        }
        private void Import(object obj)
        {
            SelectEquipmentView v = new SelectEquipmentView();
            SelectEquipmentVM vm = new SelectEquipmentVM("Compressor",  SessionPlant);
            v.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            v.DataContext = vm;
            if (v.ShowDialog() == true)
            {
                if (!string.IsNullOrEmpty(vm.SelectedEq))
                {
                    //根据设该设备名称来获取对应的物流线信息和其他信息。
                    ProIIEqDataDAL dbEq = new ProIIEqDataDAL();
                    FileName = vm.SelectedFile;
                    ProIICompressor = dbEq.GetModel(SessionPlant, FileName, vm.SelectedEq, "Compressor");
                    CompressorType = CompressorTypes[0];
                    CompressorName = ProIICompressor.EqName;
                    dicFeeds = new List<string>();
                    dicProducts = new List<string>();
                    dicProductTypes = new List<string>();
                    Feeds = new ObservableCollection<CustomStream>();
                    Products = new ObservableCollection<CustomStream>();
                    GetEqFeedProduct(ProIICompressor, ref dicFeeds, ref dicProducts, ref dicProductTypes);
                    ProIIStreamDataDAL dbStreamData = new ProIIStreamDataDAL();

                    foreach (string k in dicFeeds)
                    {
                        ProIIStreamData d = dbStreamData.GetModel(SessionPlant, k, FileName);
                        CustomStream cstream = ProIIToDefault.ConvertProIIStreamToCustomStream(d);
                        cstream.IsProduct = false;
                        Feeds.Add(cstream);
                    }
                    for (int i = 0; i < dicProducts.Count; i++)
                    {
                        string k = dicProducts[i];
                        ProIIStreamData d = dbStreamData.GetModel(SessionPlant, k, FileName);
                        CustomStream cstream = ProIIToDefault.ConvertProIIStreamToCustomStream(d);
                        cstream.IsProduct = true;
                        cstream.ProdType = dicProductTypes[i];
                        Products.Add(cstream);
                    }

                }
            }
        }

        public void GetEqFeedProduct(ProIIEqData data, ref List<string> dicFeeds, ref List<string> dicProducts,ref List<string> dicProductTypes)
        {
            string feeddata = data.FeedData;
            string productdata = data.ProductData;
            string producttype = data.ProductStoreData;
            string[] arrFeeds = feeddata.Split(',');
            string[] arrProducts = productdata.Split(',');
            string[] arrProductTypes = producttype.Split(',');
            for (int i = 0; i < arrFeeds.Length; i++)
            {
                dicFeeds.Add(arrFeeds[i]);
            }
            for (int i = 0; i < arrProducts.Length; i++)
            {
                dicProducts.Add(arrProducts[i]);
            }
            for (int i = 0; i < arrProductTypes.Length; i++)
            {
                dicProductTypes.Add(arrProductTypes[i]);
            }

        }

        
        private ObservableCollection<CustomStream> GetStreams(ISession Session, bool IsProduct)
        {
            ObservableCollection<CustomStream> list = new ObservableCollection<CustomStream>();
            CustomStreamDAL db = new CustomStreamDAL();
            IList<CustomStream> lt = db.GetAllList(Session, IsProduct);
            foreach (CustomStream c in lt)
            {
                list.Add(c);
            }

            return list;
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
        public void Save(object obj)
        {
            
                CustomStreamDAL dbCS = new CustomStreamDAL();
                SourceDAL dbsr = new SourceDAL();
                foreach (CustomStream cs in Feeds)
                {
                    Source sr = new Source();
                    sr.MaxPossiblePressure = cs.Pressure;
                    sr.StreamName = cs.StreamName;
                    sr.SourceType = "Compressor(Motor)";
                    dbsr.Add(sr, SessionProtectedSystem);


                    dbCS.Add(cs, SessionProtectedSystem);
                }


                foreach (CustomStream cs in Products)
                {
                    dbCS.Add(cs, SessionProtectedSystem);
                }

                CompressorDAL dbCompressor = new CompressorDAL();
                Compressor compressor = new Compressor();
                compressor.CompressorName = CompressorName;
                //Compressor.Duty = Duty;
                compressor.CompressorType = CompressorType;
                compressor.SourceFile = FileName;
               
                dbCompressor.Add(compressor, SessionProtectedSystem);

                ProtectedSystemDAL psDAL = new ProtectedSystemDAL();
                ProtectedSystem ps = new ProtectedSystem();
                ps.PSType = 3;
                psDAL.Add(ps, SessionProtectedSystem);

                SessionProtectedSystem.Flush();
            

            System.Windows.Window wd = obj as System.Windows.Window;

            if (wd != null)
            {
                wd.DialogResult = true;
            }
        }

        private ObservableCollection<string> GetCompressorTypes()
        {
            ObservableCollection<string> list = new ObservableCollection<string>();
            list.Add("Centrifugal");
            list.Add("Piston");
            return list;
        }
    }
}
