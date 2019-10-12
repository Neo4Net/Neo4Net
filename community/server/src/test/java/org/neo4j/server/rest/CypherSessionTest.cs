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
namespace Org.Neo4j.Server.rest
{
	using Test = org.junit.Test;

	using Org.Neo4j.Helpers.Collection;
	using GraphDatabaseFacade = Org.Neo4j.Kernel.impl.factory.GraphDatabaseFacade;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using CypherExecutor = Org.Neo4j.Server.database.CypherExecutor;
	using Database = Org.Neo4j.Server.database.Database;
	using WrappedDatabase = Org.Neo4j.Server.database.WrappedDatabase;
	using CypherSession = Org.Neo4j.Server.rest.management.console.CypherSession;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;

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