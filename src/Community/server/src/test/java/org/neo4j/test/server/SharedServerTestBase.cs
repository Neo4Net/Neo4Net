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
namespace Neo4Net.Test.server
{
	using AfterClass = org.junit.AfterClass;
	using BeforeClass = org.junit.BeforeClass;
	using Rule = org.junit.Rule;

	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using NeoServer = Neo4Net.Server.NeoServer;
	using ServerSettings = Neo4Net.Server.configuration.ServerSettings;
	using ServerHelper = Neo4Net.Server.helpers.ServerHelper;
	using SuppressOutput = Neo4Net.Test.rule.SuppressOutput;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.SuppressOutput.suppressAll;

	public class SharedServerTestBase
	{
		 protected internal static NeoServer Server()
		 {
			  return _server;
		 }

		 private static NeoServer _server;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.SuppressOutput suppressOutput = suppressAll();
		 public SuppressOutput SuppressOutput = suppressAll();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void allocateServer() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public static void AllocateServer()
		 {
			  System.setProperty( "org.neo4j.useInsecureCertificateGeneration", "true" );
			  suppressAll().call((Callable<Void>)() =>
			  {
				ServerHolder.SetServerBuilderProperty( GraphDatabaseSettings.cypher_hints_error.name(), "true" );
				ServerHolder.SetServerBuilderProperty( GraphDatabaseSettings.transaction_timeout.name(), "300s" );
				ServerHolder.SetServerBuilderProperty( ServerSettings.transaction_idle_timeout.name(), "300s" );
				_server = ServerHolder.Allocate();
				ServerHelper.cleanTheDatabase( _server );
				return null;
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void releaseServer() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public static void ReleaseServer()
		 {
			  try
			  {
					suppressAll().call((Callable<Void>)() =>
					{
					 ServerHolder.Release( _server );
					 return null;
					});
			  }
			  finally
			  {
					_server = null;
			  }
		 }
	}

}