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
namespace Neo4Net.Server.rest
{
	using Test = org.junit.Test;

	using Neo4Net.Collections.Helpers;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using CypherExecutor = Neo4Net.Server.database.CypherExecutor;
	using Database = Neo4Net.Server.database.Database;
	using WrappedDatabase = Neo4Net.Server.database.WrappedDatabase;
	using CypherSession = Neo4Net.Server.rest.management.console.CypherSession;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class CypherSessionTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnASingleNode()
		 public virtual void ShouldReturnASingleNode()
		 {
			  GraphDatabaseFacade graphdb = ( GraphDatabaseFacade ) ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
			  Database database = new WrappedDatabase( graphdb );
			  CypherExecutor executor = new CypherExecutor( database, NullLogProvider.Instance );
			  executor.Start();
			  HttpServletRequest request = mock( typeof( HttpServletRequest ) );
			  when( request.Scheme ).thenReturn( "http" );
			  when( request.RemoteAddr ).thenReturn( "127.0.0.1" );
			  when( request.RemotePort ).thenReturn( 5678 );
			  when( request.ServerName ).thenReturn( "127.0.0.1" );
			  when( request.ServerPort ).thenReturn( 7474 );
			  when( request.RequestURI ).thenReturn( "/" );
			  try
			  {
					CypherSession session = new CypherSession( executor, NullLogProvider.Instance, request );
					Pair<string, string> result = session.Evaluate( "create (a) return a" );
					assertThat( result.First(), containsString("Node[0]") );
			  }
			  finally
			  {
					graphdb.Shutdown();
			  }
		 }
	}

}