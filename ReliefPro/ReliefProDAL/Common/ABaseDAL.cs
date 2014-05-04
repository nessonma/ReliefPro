﻿using System;
using NHibernate;


namespace ReliefProDAL.Common
{
    public abstract class ABaseDAL<A> : IBase<A>
    {

        public virtual bool Exists(Object mainId, NHibernate.ISession session)
        {
            A t = session.Get<A>(mainId);
            return t == null ? false : true;
        }

        public virtual Object Add(A model, NHibernate.ISession session)
        {
            using (ITransaction transaction = session.BeginTransaction())
            {

                Object o = session.Save(model);
                transaction.Commit();
                return o;
            }

        }

        public virtual void Update(A model, NHibernate.ISession session)
        {
            session.Update(model);
            
        }

        public virtual void Delete(A model, NHibernate.ISession session)
        {
            session.Delete(model);
            
        }

        public virtual A GetModel(Object mainId, NHibernate.ISession session)
        {
            return session.Get<A>(mainId);
        }

        public virtual void AddOrUpdate(A model, ISession session)
        {
            session.SaveOrUpdate(model);
        }

        public abstract bool reg();



    }

}
