using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Cfg;

namespace DataAccess
{
    /*
     * Konfigurace NHibernate.
     * Pomůže nám lépe použít NHibernate tak, abychom měli co nejméně práce při psaní DAO tříd v budoucnu.
     */
    public class NHibernateHelper
    {
        private static ISessionFactory _factory;

        public static ISession Session
        {
            get
            {
                if (_factory == null)
                {
                    var cfg = new Configuration();
                    _factory = cfg.Configure(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "hibernate.cfg.xml")).BuildSessionFactory();     // snažíme se obecně říct, kde se nachází konf. soubor hibernate.cfg.xml
                    // volání BuildSessionFactory je procesorově nejnáročnější, proto se snažíme volat ji jednou - použijeme singleton
                }

                return _factory.OpenSession();
            }
        }
    }
}
