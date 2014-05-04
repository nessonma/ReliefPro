﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReliefProMain.Interface
{
    public interface IServiceLocator
    {
        void Register<TInterface, TImplementation>() where TImplementation : TInterface;

        TInterface Get<TInterface>();
    }
}
