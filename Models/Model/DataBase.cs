using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Models
{
    public class DataBase<TEntity> where TEntity : class
    {
        LiteDatabase m_db;
        LiteCollection<TEntity> m_collection;

        public DataBase(string tableName = "")
        {
            // Open database (or create if not exits)
            m_db = new LiteDatabase(System.AppDomain.CurrentDomain.BaseDirectory + @"LiBingHui&LiXingMan.db");

            // Get table collection
            if (string.IsNullOrEmpty(tableName))
                m_collection = m_db.GetCollection<TEntity>();
            else
                m_collection = m_db.GetCollection<TEntity>(tableName);

            //var testCollection = m_db.GetCollection<tbl_QuestionBank>();
            //testCollection.Max(a => a.id);
        }

        public bool Insert(TEntity record)
        {
            try
            {
                m_collection.Insert(record);
                return true;
            }
            catch { return false; }
        }

        public bool Insert(IEnumerable<TEntity> records)
        {
            try
            {
                m_collection.Insert(records);
                return true;
            }
            catch { return false; }
        }

        //.Select(a=>a.id>1);
        public IEnumerable<TEntity> Select(Expression<Func<TEntity, bool>> predicate, int skip = 0, int limit = int.MaxValue)
        {
            try
            {
                return m_collection.Find(predicate, skip, limit);
            }
            catch { return null; }
        }

        public IEnumerable<TEntity> SelectAll()
        {
            try
            {
                return m_collection.FindAll();
            }
            catch { return null; }
        }

        //.SelectOne(a=>a.id==1);
        public TEntity SelectOne(Expression<Func<TEntity, bool>> predicate)
        {
            try
            {
                return m_collection.FindOne(predicate);
            }
            catch { return null; }
        }

        public TEntity Select(int id)
        {
            try
            {
                return m_collection.FindById(id);
            }
            catch { return null; }
        }

        public bool Update(TEntity record)
        {
            try
            {
                return m_collection.Update(record);
            }
            catch { return false; }
        }

        public bool Update(IEnumerable<TEntity> records)
        {
            try
            {
                m_collection.Update(records);
                return true;
            }
            catch { return false; }
        }

        public bool UpdateOrInsert(TEntity record)
        {
            try
            {
                return m_collection.Upsert(record);
            }
            catch { return false; }
        }

        public bool UpdateOrInsert(IEnumerable<TEntity> records)
        {
            try
            {
                m_collection.Upsert(records);
                return true;
            }
            catch { return false; }
        }

        //.Delete(a=>a.id==1);
        public bool Delete(Expression<Func<TEntity, bool>> predicate)
        {
            try
            {
                m_collection.Delete(predicate);
                return true;
            }
            catch { return false; }
        }

        public int Count()
        {
            try
            {
                return m_collection.Count();
            }
            catch { return -1; }
        }

        //.Count(a=>a.id==1);
        public int Count(Expression<Func<TEntity, bool>> predicate)
        {
            try
            {
                return m_collection.Count(predicate);
            }
            catch { return -1; }
        }

        //.Exist(a=>a.id==1);
        public bool Exist(Expression<Func<TEntity, bool>> predicate)
        {
            try
            {
                return m_collection.Exists(predicate);
            }
            catch { return false; }
        }

        //.Max(a=>a.id);
        public object Max<K>(Expression<Func<TEntity, K>> property)
        {
            try
            {
                var result = m_collection.Max(property);
                return result.RawValue;
            }
            catch { return null; }
        }

        //.Min(a=>a.id);
        public object Min<K>(Expression<Func<TEntity, K>> property)
        {
            try
            {
                var result = m_collection.Min(property);
                return result.RawValue;
            }
            catch { return null; }
        }
    }
}
