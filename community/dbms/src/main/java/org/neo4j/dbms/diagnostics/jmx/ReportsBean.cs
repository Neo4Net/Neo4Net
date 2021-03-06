﻿using System.Collections.Generic;
using System.Text;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Dbms.diagnostics.jmx
{

	using QueryExecutionException = Org.Neo4j.Graphdb.QueryExecutionException;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using ManagementBeanProvider = Org.Neo4j.Jmx.impl.ManagementBeanProvider;
	using ManagementData = Org.Neo4j.Jmx.impl.ManagementData;
	using Neo4jMBean = Org.Neo4j.Jmx.impl.Neo4jMBean;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;

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