﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ReliefProMain.Commands;
using System.IO;
namespace ReliefProMain.ViewModel
{
    public class CreateUnitVM : ViewModelBase
    {
       
        public string dirPlant { get; set; }
        public string dirUnit { get; set; }

        public string dirProtectedSystem { get; set; }
        public string visioProtectedSystem { get; set; }
        public string dbProtectedSystemFile { get; set; } 


        private string _UnitName;
        [ReliefProMain.Util.Required(ErrorMessage = "NotEmpty")]
        public string UnitName
        {
            get
            {
                return this._UnitName;
            }
            set
            {
                this._UnitName = value;

                OnPropertyChanged("UnitName");
            }
        }

        private string _UnitName_Color;
        public string UnitName_Color
        {
            get
            {
                return this._UnitName_Color;
            }
            set
            {
                this._UnitName_Color = value;
                OnPropertyChanged("UnitName");
            }
        }

        public CreateUnitVM(string dir )
        {
            dirPlant = dir;
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
            if (!CheckData()) return;
            string dirUnit = dirPlant + @"\" + UnitName;
            Directory.CreateDirectory(dirUnit);
            string protectedsystem1 = dirUnit + @"\ProtectedSystem1";
            Directory.CreateDirectory(protectedsystem1);
            string dbProtectedSystem = AppDomain.CurrentDomain.BaseDirectory.ToString() + @"template\protectedsystem.mdb";
            string dbProtectedSystem_target = protectedsystem1 + @"\protectedsystem.mdb";
            System.IO.File.Copy(dbProtectedSystem, dbProtectedSystem_target, true);
            string visioProtectedSystem = AppDomain.CurrentDomain.BaseDirectory.ToString() + @"template\protectedsystem.vsd";
            string visioProtectedSystem_target = protectedsystem1 + @"\design.vsd";
            System.IO.File.Copy(visioProtectedSystem, visioProtectedSystem_target, true);
            dirProtectedSystem = protectedsystem1;
            visioProtectedSystem = visioProtectedSystem_target;
            dbProtectedSystemFile = dbProtectedSystem_target;

            
            System.Windows.Window wd = window as System.Windows.Window;
            if (wd != null)
            {
                wd.DialogResult = true;
            }
        }

    }
}
