﻿using System;
using System.Collections.Generic;
using NHibernate;
using ReliefProDAL.Common;
using ReliefProModel;
using System.Data;
using NHibernate.Criterion;

namespace ReliefProDAL
{
    public class dbFeedBottomHX : IBaseDAL<FeedBottomHX>
    {
        public IList<FeedBottomHX> GetAllList(ISession session)
        {
            IList<FeedBottomHX> list = null;
            try
            {
                list = session.CreateCriteria<FeedBottomHX>().List<FeedBottomHX>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return list;
        }
        public FeedBottomHX GetModel(ISession session)
        {
            FeedBottomHX model = null;
            IList<FeedBottomHX> list = null;
            try
            {
                list = session.CreateCriteria<FeedBottomHX>().List<FeedBottomHX>();
                if (list.Count > 0)
                {
                    model = list[0];
                }
                else
                    model = null;
            }
            catch (Exception ex)
            {
                model = null;
                throw ex;
                
            }
            
            return model;
        }
    }
}
