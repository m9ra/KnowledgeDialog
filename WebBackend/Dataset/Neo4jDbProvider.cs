using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Neo4j.Driver.V1;

namespace WebBackend.Dataset
{
    class Neo4jDbProvider
    {
        ISession _session;

        IDriver _driver;

        internal Neo4jDbProvider()
        {
            _driver = GraphDatabase.Driver("bolt://localhost", AuthTokens.Basic("neo4j", "neo4jj"));
            _session = _driver.Session();
        }

        internal void AddEdge(string idFrom, string edge, string idTo)
        {
            _session.Run("CREATE (a:Node {name:'Arthur', title:'King'})");
        }
    }
}
