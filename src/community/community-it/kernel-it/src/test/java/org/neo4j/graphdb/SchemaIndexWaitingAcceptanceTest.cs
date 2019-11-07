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
namespace Neo4Net.GraphDb
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseFactory = Neo4Net.GraphDb.factory.GraphDatabaseFactory;
	using IndexDefinition = Neo4Net.GraphDb.Schema.IndexDefinition;
	using Neo4Net.Kernel.extension;
	using ControlledPopulationIndexProvider = Neo4Net.Kernel.Impl.Api.index.ControlledPopulationIndexProvider;
	using DoubleLatch = Neo4Net.Test.DoubleLatch;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.GraphDatabaseSettings.default_schema_provider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.api.index.SchemaIndexTestHelper.singleInstanceIndexProviderFactory;

	public class SchemaIndexWaitingAcceptanceTest
	{
		 private readonly ControlledPopulationIndexProvider _provider = new ControlledPopulationIndexProvider();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.DatabaseRule rule = new Neo4Net.test.rule.ImpermanentDatabaseRule()
		 public final DatabaseRule rule = new ImpermanentDatabaseRuleAnonymousInnerClass()
		 .withSetting( default_schema_provider, _provider.ProviderDescriptor.name() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTimeoutWaitingForIndexToComeOnline()
		 public void shouldTimeoutWaitingForIndexToComeOnline()
		 {
			  // given
			  IGraphDatabaseService db = rule.GraphDatabaseAPI;
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
			  IGraphDatabaseService db = rule.GraphDatabaseAPI;
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