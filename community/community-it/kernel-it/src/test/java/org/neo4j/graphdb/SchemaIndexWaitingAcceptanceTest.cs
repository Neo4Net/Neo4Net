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
namespace Org.Neo4j.Graphdb
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseFactory = Org.Neo4j.Graphdb.factory.GraphDatabaseFactory;
	using IndexDefinition = Org.Neo4j.Graphdb.schema.IndexDefinition;
	using Org.Neo4j.Kernel.extension;
	using ControlledPopulationIndexProvider = Org.Neo4j.Kernel.Impl.Api.index.ControlledPopulationIndexProvider;
	using DoubleLatch = Org.Neo4j.Test.DoubleLatch;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.default_schema_provider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.api.index.SchemaIndexTestHelper.singleInstanceIndexProviderFactory;

	public class SchemaIndexWaitingAcceptanceTest
	{
		 private readonly ControlledPopulationIndexProvider _provider = new ControlledPopulationIndexProvider();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule rule = new org.neo4j.test.rule.ImpermanentDatabaseRule()
		 public final DatabaseRule rule = new ImpermanentDatabaseRuleAnonymousInnerClass()
		 .withSetting( default_schema_provider, _provider.ProviderDescriptor.name() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTimeoutWaitingForIndexToComeOnline()
		 public void shouldTimeoutWaitingForIndexToComeOnline()
		 {
			  // given
			  GraphDatabaseService db = rule.GraphDatabaseAPI;
			  DoubleLatch latch = _provider.installPopulationJobCompletionLatch();

			  IndexDefinition index;
			  using ( Transaction tx = Db.beginTx() )
			  {
					index = Db.schema().indexFor(Label.label("Person")).on("name").create();
					tx.Success();
			  }

			  latch.WaitForAllToStart();

			  // when
			  try
			  {
					  using ( Transaction tx = Db.beginTx() )
					  {
						// then
						Db.schema().awaitIndexOnline(index, 1, TimeUnit.MILLISECONDS);
      
						fail( "Expected IllegalStateException to be thrown" );
					  }
			  }
			  catch ( System.InvalidOperationException e )
			  {
					// good
					assertThat( e.Message, containsString( "come online" ) );
			  }
			  finally
			  {
					latch.Finish();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTimeoutWaitingForAllIndexesToComeOnline()
		 public void shouldTimeoutWaitingForAllIndexesToComeOnline()
		 {
			  // given
			  GraphDatabaseService db = rule.GraphDatabaseAPI;
			  DoubleLatch latch = _provider.installPopulationJobCompletionLatch();

			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().indexFor(Label.label("Person")).on("name").create();
					tx.Success();
			  }

			  latch.WaitForAllToStart();

			  // when
			  try
			  {
					  using ( Transaction tx = Db.beginTx() )
					  {
						// then
						Db.schema().awaitIndexesOnline(1, TimeUnit.MILLISECONDS);
      
						fail( "Expected IllegalStateException to be thrown" );
					  }
			  }
			  catch ( System.InvalidOperationException e )
			  {
					// good
					assertThat( e.Message, containsString( "come online" ) );
			  }
			  finally
			  {
					latch.Finish();
			  }
		 }
	}

}