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
namespace Neo4Net.Jmx.impl
{
	using AfterClass = org.junit.AfterClass;
	using BeforeClass = org.junit.BeforeClass;
	using Test = org.junit.Test;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class ConfigurationBeanIT
	{
		 private static GraphDatabaseService _graphdb;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void startDb()
		 public static void StartDb()
		 {
			  _graphdb = ( new TestGraphDatabaseFactory() ).newImpermanentDatabaseBuilder().newGraphDatabase();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void stopDb()
		 public static void StopDb()
		 {
			  if ( _graphdb != null )
			  {
					_graphdb.shutdown();
			  }
			  _graphdb = null;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void durationListedWithUnit()
		 public virtual void DurationListedWithUnit()
		 {
			  ConfigurationBean configurationBean = ( ( GraphDatabaseAPI ) _graphdb ).DependencyResolver.resolveDependency( typeof( JmxKernelExtension ) ).getSingleManagementBean( typeof( ConfigurationBean ) );

			  object v = configurationBean.GetAttribute( GraphDatabaseSettings.log_queries_threshold.name() );
			  assertEquals( "0ms", v );

			  AttributeList attrs = configurationBean.GetAttributes( new string[]{ GraphDatabaseSettings.log_queries_threshold.name() } );

			  assertEquals( 1, attrs.size() );

			  Attribute attr = ( Attribute ) attrs.get( 0 );

			  assertEquals( "0ms", attr.Value );
			  assertEquals( "dbms.logs.query.threshold = 0ms", attr.ToString() );
		 }
	}

}