using System.Collections.Generic;

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
namespace Neo4Net.Server.plugins
{
	using AfterClass = org.junit.AfterClass;
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using NullFormat = Neo4Net.Server.rest.repr.formats.NullFormat;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;

	public class DefaultPluginManagerTest
	{
		 private static PluginManager _manager;
		 private static GraphDatabaseAPI _graphDb;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void loadExtensionManager()
		 public static void LoadExtensionManager()
		 {
			  _graphDb = ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newImpermanentDatabase();
			  _manager = new DefaultPluginManager( NullLogProvider.Instance );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void destroyExtensionManager()
		 public static void DestroyExtensionManager()
		 {
			  _manager = null;
			  if ( _graphDb != null )
			  {
					_graphDb.shutdown();
			  }
			  _graphDb = null;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canGetUrisForNode()
		 public virtual void CanGetUrisForNode()
		 {
			  IDictionary<string, IList<string>> extensions = _manager.getExensionsFor( typeof( GraphDatabaseService ) );
			  IList<string> methods = extensions[typeof( FunctionalTestPlugin ).Name];
			  assertNotNull( methods );
			  assertThat( methods, hasItem( FunctionalTestPlugin.CREATE_NODE ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canInvokeExtension() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CanInvokeExtension()
		 {
			  _manager.invoke( _graphDb, typeof( FunctionalTestPlugin ).Name, typeof( GraphDatabaseService ), FunctionalTestPlugin.CREATE_NODE, _graphDb, ( new NullFormat( null, ( MediaType[] ) null ) ).readParameterList( "" ) );
		 }
	}

}