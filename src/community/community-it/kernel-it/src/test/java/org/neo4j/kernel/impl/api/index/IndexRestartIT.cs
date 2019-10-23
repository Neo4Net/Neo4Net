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
namespace Neo4Net.Kernel.Impl.Api.index
{
	using CoreMatchers = org.hamcrest.CoreMatchers;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using NotFoundException = Neo4Net.GraphDb.NotFoundException;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using UncloseableDelegatingFileSystemAbstraction = Neo4Net.GraphDb.mockfs.UncloseableDelegatingFileSystemAbstraction;
	using IndexDefinition = Neo4Net.GraphDb.Schema.IndexDefinition;
	using Schema = Neo4Net.GraphDb.Schema.Schema;
	using DoubleLatch = Neo4Net.Test.DoubleLatch;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.factory.GraphDatabaseSettings.default_schema_provider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.InternalIndexState.ONLINE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.InternalIndexState.POPULATING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.api.index.SchemaIndexTestHelper.singleInstanceIndexProviderFactory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.Neo4NetMatchers.getIndexState;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.Neo4NetMatchers.getIndexes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.Neo4NetMatchers.hasSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.Neo4NetMatchers.haveState;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.Neo4NetMatchers.inTx;

	public class IndexRestartIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.fs.EphemeralFileSystemRule fs = new org.Neo4Net.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule Fs = new EphemeralFileSystemRule();

		 private IGraphDatabaseService _db;
		 private TestGraphDatabaseFactory _factory;
		 private readonly ControlledPopulationIndexProvider _provider = new ControlledPopulationIndexProvider();
		 private readonly Label _myLabel = label( "MyLabel" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  _factory = new TestGraphDatabaseFactory();
			  _factory.FileSystem = new UncloseableDelegatingFileSystemAbstraction( Fs.get() );
			  _factory.KernelExtensions = Collections.singletonList( singleInstanceIndexProviderFactory( "test", _provider ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void after()
		 public virtual void After()
		 {
			  _db.shutdown();
		 }

		 /* This is somewhat difficult to test since dropping an index while it's populating forces it to be cancelled
		  * first (and also awaiting cancellation to complete). So this is a best-effort to have the timing as close
		  * as possible. If this proves to be flaky, remove it right away.
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToDropIndexWhileItIsPopulating()
		 public virtual void ShouldBeAbleToDropIndexWhileItIsPopulating()
		 {
			  // GIVEN
			  StartDb();
			  DoubleLatch populationCompletionLatch = _provider.installPopulationJobCompletionLatch();
			  IndexDefinition index = CreateIndex();
			  populationCompletionLatch.WaitForAllToStart(); // await population job to start

			  // WHEN
			  DropIndex( index, populationCompletionLatch );

			  // THEN
			  assertThat( getIndexes( _db, _myLabel ), inTx( _db, hasSize( 0 ) ) );
			  try
			  {
					getIndexState( _db, index );
					fail( "This index should have been deleted" );
			  }
			  catch ( NotFoundException e )
			  {
					assertThat( e.Message, CoreMatchers.containsString( _myLabel.name() ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleRestartOfOnlineIndex()
		 public virtual void ShouldHandleRestartOfOnlineIndex()
		 {
			  // Given
			  StartDb();
			  CreateIndex();
			  _provider.awaitFullyPopulated();

			  // And Given
			  StopDb();
			  _provider.InitialIndexState = ONLINE;

			  // When
			  StartDb();

			  // Then
			  assertThat( getIndexes( _db, _myLabel ), inTx( _db, haveState( _db, Neo4Net.GraphDb.Schema.Schema_IndexState.Online ) ) );
			  assertEquals( 1, _provider.populatorCallCount.get() );
			  assertEquals( 2, _provider.writerCallCount.get() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleRestartIndexThatHasNotComeOnlineYet()
		 public virtual void ShouldHandleRestartIndexThatHasNotComeOnlineYet()
		 {
			  // Given
			  StartDb();
			  CreateIndex();

			  // And Given
			  StopDb();
			  _provider.InitialIndexState = POPULATING;

			  // When
			  StartDb();

			  assertThat( getIndexes( _db, _myLabel ), inTx( _db, not( haveState( _db, Neo4Net.GraphDb.Schema.Schema_IndexState.Failed ) ) ) );
			  assertEquals( 2, _provider.populatorCallCount.get() );
		 }

		 private IndexDefinition CreateIndex()
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					IndexDefinition index = _db.schema().indexFor(_myLabel).on("number_of_bananas_owned").create();
					tx.Success();
					return index;
			  }
		 }

		 private void DropIndex( IndexDefinition index, DoubleLatch populationCompletionLatch )
		 {
			  using ( Transaction tx = _db.beginTx() )
			  {
					index.Drop();
					populationCompletionLatch.Finish();
					tx.Success();
			  }
		 }

		 private void StartDb()
		 {
			  if ( _db != null )
			  {
					_db.shutdown();
			  }

			  _db = _factory.newImpermanentDatabaseBuilder().setConfig(default_schema_provider, _provider.ProviderDescriptor.name()).newGraphDatabase();
		 }

		 private void StopDb()
		 {
			  if ( _db != null )
			  {
					_db.shutdown();
			  }
		 }
	}

}