﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ReliefProMain.CustomControl
{
    public class UnitConvertTextBox : TextBox
    {
        public enum typeUnit
        {
            T,
            P,
            W,
            M,
            SVR,
            V,
            HC,
            TC,
            HTC,
            ST,
            C,
            MS,
            VOL,
            LEN,
            A,
            E,
            TM,
            FC
        }
        public typeUnit UnitType { get; set; }
        public string UnitOrigin
        {
            get;
            set;
        }
        public UnitConvertTextBox()
        {
        }
        protected override void OnMouseDoubleClick(System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!string.IsNullOrEmpty(UnitOrigin))
            {
                double UnitValue;
                if (double.TryParse(this.Text.Trim(), out UnitValue))
                {
                    UnitConvertCommonView unitConvertCommonView = new UnitConvertCommonView(UnitType.ToString(), UnitOrigin, UnitValue);
                    if (unitConvertCommonView.ShowDialog() == true)
                    {
                        this.UnitOrigin = unitConvertCommonView.TargetUnit;
                        this.Text = unitConvertCommonView.ResultValue.ToString();
                    }
                }
            }
            return;
        }

    }
}
