﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Criterion;
using ReliefProModel.Drum;

namespace ReliefProDAL.Drum
{
    public class dbDrumFire
    {
        public IList<DrumFireCalc> GetAllList(ISession session)
        {
            IList<DrumFireCalc> list = null;
            try
            {
                list = session.CreateCriteria<DrumFireCalc>().List<DrumFireCalc>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return list;
        }
        public DrumFireCalc GetModelByDrumID(ISession session, int drumFireCalcID)
        {
            var list = session.CreateCriteria<DrumFireCalc>().Add(Expression.Eq("ID", drumFireCalcID)).List<DrumFireCalc>();
            if (list.Count() > 0)
            {
                return list[0];
            }
            return null;
        }
        public void SaveDrumFireCalc(ISession session, DrumFireCalc model)
        {
            using (ITransaction tx = session.BeginTransaction())
            {
                try
                {
                    session.SaveOrUpdate(model);
                    session.Flush();
                    tx.Commit();
                }
                catch (HibernateException)
                {
                    tx.Rollback();
                    throw;
                }
            }
        }
    }
}