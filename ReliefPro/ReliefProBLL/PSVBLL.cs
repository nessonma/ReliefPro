﻿using NHibernate;
using ReliefProDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReliefProBLL
{
    public class PSVBLL
    {
        ISession SessionProtectedSystem;

        public PSVBLL(ISession SessionProtectedSystem)
        {
            this.SessionProtectedSystem = SessionProtectedSystem;
        }
        public void DeletePSVData()
        {
            string sql = " from ReliefProModel.Latent ";
            SessionProtectedSystem.Delete(sql);
            sql = " from ReliefProModel.LatentProduct ";
            SessionProtectedSystem.Delete(sql);
            sql = " from ReliefProModel.PSV ";
            SessionProtectedSystem.Delete(sql);

            sql = " from ReliefProModel.FlashCalcResult ";
            SessionProtectedSystem.Delete(sql);

        }
    }
}
