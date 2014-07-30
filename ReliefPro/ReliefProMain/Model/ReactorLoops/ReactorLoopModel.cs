﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ReliefProModel.ReactorLoops;

namespace ReliefProMain.Model.ReactorLoops
{
    public class ReactorLoopModel : ModelBase
    {
        public ReactorLoop dbModel { get; set; }

        private ObservableCollection<string> _EffluentStreamSource;
        public ObservableCollection<string> EffluentStreamSource
        {
            get { return _EffluentStreamSource; }
            set
            {
                _EffluentStreamSource = value;
                NotifyPropertyChanged("EffluentStreamSource");
            }
        }
        private ObservableCollection<string> _ColdReactorFeedStreamSource;
        public ObservableCollection<string> ColdReactorFeedStreamSource
        {
            get { return _ColdReactorFeedStreamSource; }
            set
            {
                _ColdReactorFeedStreamSource = value;
                NotifyPropertyChanged("ColdReactorFeedStreamSource");
            }
        }
        private ObservableCollection<string> _HotHighPressureSeparatorSource;
        public ObservableCollection<string> HotHighPressureSeparatorSource
        {
            get { return _HotHighPressureSeparatorSource; }
            set
            {
                _HotHighPressureSeparatorSource = value;
                NotifyPropertyChanged("HotHighPressureSeparatorSource");
            }
        }
        private ObservableCollection<string> _ColdHighPressureSeparatorSource;
        public ObservableCollection<string> ColdHighPressureSeparatorSource
        {
            get { return _ColdHighPressureSeparatorSource; }
            set
            {
                _ColdHighPressureSeparatorSource = value;
                NotifyPropertyChanged("ColdHighPressureSeparatorSource");
            }
        }
        public ObservableCollection<string> _HXNetworkColdStreamSource;
        public ObservableCollection<string> HXNetworkColdStreamSource
        {
            get { return _HXNetworkColdStreamSource; }
            set
            {
                _HXNetworkColdStreamSource = value;
                NotifyPropertyChanged("HXNetworkColdStreamSource");
            }
        }
        public ObservableCollection<string> _InjectionWaterStreamSource;
        public ObservableCollection<string> InjectionWaterStreamSource
        {
            get { return _InjectionWaterStreamSource; }
            set
            {
                _InjectionWaterStreamSource = value;
                NotifyPropertyChanged("InjectionWaterStreamSource");
            }
        }

        public string EffluentStream
        {
            get { return dbModel.EffluentStream; }
            set
            {
                dbModel.EffluentStream = value;
                NotifyPropertyChanged("EffluentStream");
            }
        }
        public string ColdReactorFeedStream
        {
            get { return dbModel.ColdReactorFeedStream; }
            set
            {
                dbModel.ColdReactorFeedStream = value;
                NotifyPropertyChanged("ColdReactorFeedStream");
            }
        }
        public string HotHighPressureSeparator
        {
            get { return dbModel.HotHighPressureSeparator; }
            set
            {
                dbModel.HotHighPressureSeparator = value;
                NotifyPropertyChanged("HotHighPressureSeparator");
            }
        }
        public string ColdHighPressureSeparator
        {
            get { return dbModel.ColdHighPressureSeparator; }
            set
            {
                dbModel.ColdHighPressureSeparator = value;
                NotifyPropertyChanged("ColdHighPressureSeparator");
            }
        }
        public string HXNetworkColdStream
        {
            get { return dbModel.HXNetworkColdStream; }
            set
            {
                dbModel.HXNetworkColdStream = value;
                NotifyPropertyChanged("HXNetworkColdStream");
            }
        }
        public string InjectionWaterStream
        {
            get { return dbModel.InjectionWaterStream; }
            set
            {
                dbModel.InjectionWaterStream = value;
                NotifyPropertyChanged("InjectionWaterStream");
            }
        }

        public string PSFile
        {
            get { return dbModel.PSFile; }
            set
            {
                dbModel.PSFile = value;
                NotifyPropertyChanged("PSFile");
            }
        }

        public string PSVersion
        {
            get { return dbModel.PSVersion; }
            set
            {
                dbModel.PSVersion = value;
                NotifyPropertyChanged("PSVersion");
            }
        }
        private ReactorLoopDetail selectedHXModel;
        public ReactorLoopDetail SelectedHXModel
        {
            get { return selectedHXModel; }
            set
            {
                selectedHXModel = value;
                this.NotifyPropertyChanged("SelectedHXModel");
            }
        }

        public ObservableCollection<ReactorLoopDetail> ObcProcessHX { get; set; }
        public ObservableCollection<ReactorLoopDetail> ObcUtilityHX { get; set; }
        public ObservableCollection<ReactorLoopDetail> ObcMixerSplitter { get; set; }

        public ObservableCollection<ReactorLoopDetail> ObcProcessHXSource { get; set; }
        public ObservableCollection<ReactorLoopDetail> ObcUtilityHXSource { get; set; }
        public ObservableCollection<ReactorLoopDetail> ObcMixerSplitterSource { get; set; }
    }
}
