using System.Collections.Generic;
using System.Text;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Dbms.diagnostics.jmx
{

	using QueryExecutionException = Neo4Net.Graphdb.QueryExecutionException;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using ManagementBeanProvider = Neo4Net.Jmx.impl.ManagementBeanProvider;
	using ManagementData = Neo4Net.Jmx.impl.ManagementData;
	using Neo4jMBean = Neo4Net.Jmx.impl.Neo4jMBean;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;

	public class ReportsBean : ManagementBeanProvider
	{
		 public ReportsBean() : base(typeof(Reports))
		 {
		 }

		 protected internal override Neo4jMBean CreateMBean( ManagementData management )
		 {
			  return new ReportsImpl( management, false );
		 }

		 protected internal override Neo4jMBean CreateMXBean( ManagementData management )
		 {
			  return new ReportsImpl( management, true );
		 }

		 private class ReportsImpl : Neo4jMBean, Reports
		 {
			  internal readonly GraphDatabaseAPI GraphDatabaseAPI;

			  internal ReportsImpl( ManagementData management, bool isMXBean ) : base( management, isMXBean )
			  {
					GraphDatabaseAPI = management.ResolveDependency( typeof( GraphDatabaseAPI ) );
			  }

			  public override string ListTransactions()
			  {
					string res;
					try
					{
							using ( Transaction tx = GraphDatabaseAPI.beginTx() )
							{
							 res = GraphDatabaseAPI.execute( "CALL dbms.listTransactions()" ).resultAsString();
      
							 tx.Success();
							}
					}
					catch ( QueryExecutionException )
					{
						 res = "dbms.listTransactions() is not available";
					}
					return res;
			  }

			  public virtual string EnvironmentVariables
			  {
				  get
				  {
						StringBuilder sb = new StringBuilder();
						foreach ( KeyValuePair<string, string> env in System.getenv().entrySet() )
						{
							 sb.Append( env.Key ).Append( '=' ).Append( env.Value ).Append( '\n' );
						}
						return sb.ToString();
				  }
			  }
		 }
	}

}