using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

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
namespace Neo4Net.Kernel.Api.Impl.Fulltext
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using Neo4Net.Functions;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using IndexReference = Neo4Net.Kernel.Api.Internal.IndexReference;
	using SchemaWrite = Neo4Net.Kernel.Api.Internal.SchemaWrite;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor;
	using KernelTransactionImplementation = Neo4Net.Kernel.Impl.Api.KernelTransactionImplementation;
	using IndexDescriptorFactory = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptorFactory;
	using Race = Neo4Net.Test.Race;
	using RepeatRule = Neo4Net.Test.rule.RepeatRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.Kernel.Api.StorageEngine.EntityType.NODE;

	/// <summary>
	/// Concurrent updates and index changes should result in valid state, and not create conflicts or exceptions during
	/// commit.
	/// </summary>
	public class ConcurrentLuceneFulltextUpdaterTest : LuceneFulltextTestSupport
	{
		 private readonly int _aliceThreads = 1;
		 private readonly int _bobThreads = 1;
		 private readonly int _nodesCreatedPerThread = 500;
		 private Race _race;
		 private System.Threading.CountdownEvent _aliceLatch = new System.Threading.CountdownEvent( 2 );
		 private System.Threading.CountdownEvent _bobLatch = new System.Threading.CountdownEvent( 2 );

		 protected internal override RepeatRule CreateRepeatRule()
		 {
			  return new RepeatRule( false, 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void createRace()
		 public virtual void CreateRace()
		 {
			  _race = new Race();
		 }

		 private SchemaDescriptor GetNewDescriptor( string[] IEntityTokens )
		 {
			  return FulltextAdapter.schemaFor( NODE, IEntityTokens, Settings, "otherProp" );
		 }

		 private SchemaDescriptor GetExistingDescriptor( string[] IEntityTokens )
		 {
			  return FulltextAdapter.schemaFor( NODE, IEntityTokens, Settings, PROP );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Neo4Net.Kernel.Api.Internal.IndexReference createInitialIndex(Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor descriptor) throws Exception
		 private IndexReference CreateInitialIndex( SchemaDescriptor descriptor )
		 {
			  IndexReference index;
			  using ( KernelTransactionImplementation transaction = KernelTransaction )
			  {
					SchemaWrite schemaWrite = transaction.SchemaWrite();
					index = schemaWrite.IndexCreate( descriptor, FulltextIndexProviderFactory.Descriptor.name(), "nodes" );
					transaction.Success();
			  }
			  Await( index );
			  return index;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void raceContestantsAndVerifyResults(Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor newDescriptor, Runnable aliceWork, Runnable changeConfig, Runnable bobWork) throws Throwable
		 private void RaceContestantsAndVerifyResults( SchemaDescriptor newDescriptor, ThreadStart aliceWork, ThreadStart changeConfig, ThreadStart bobWork )
		 {
			  _race.addContestants( _aliceThreads, aliceWork );
			  _race.addContestant( changeConfig );
			  _race.addContestants( _bobThreads, bobWork );
			  _race.go();
			  Await( IndexDescriptorFactory.forSchema( newDescriptor, "nodes", FulltextIndexProviderFactory.Descriptor ) );
			  using ( Transaction tx = Db.beginTx() )
			  {
					KernelTransaction ktx = KernelTransaction( tx );
					ScoreEntityIterator bob = FulltextAdapter.query( ktx, "nodes", "bob" );
					IList<ScoreEntityIterator.ScoreEntry> list = bob.ToList();
					try
					{
						 assertEquals( _bobThreads * _nodesCreatedPerThread, list.Count );
					}
					catch ( Exception e )
					{
						 StringBuilder sb = ( new StringBuilder( e.Message ) ).Append( Environment.NewLine ).Append( "Nodes found in query for bob:" );
						 foreach ( ScoreEntityIterator.ScoreEntry entry in list )
						 {
							  sb.Append( Environment.NewLine ).Append( "\t" ).Append( Db.getNodeById( entry.EntityId() ) );
						 }
						 throw e;
					}
					ScoreEntityIterator alice = FulltextAdapter.query( ktx, "nodes", "alice" );
					assertEquals( 0, alice.Count() );
			  }
		 }

		 private ThreadStart Work( int iterations, ThrowingAction<Exception> work )
		 {
			  return () =>
			  {
				try
				{
					 for ( int i = 0; i < iterations; i++ )
					 {
						  Thread.yield();
						  using ( Transaction tx = Db.beginTx() )
						  {
								Thread.yield();
								work.Apply();
								Thread.yield();
								tx.success();
						  }
					 }
				}
				catch ( Exception e )
				{
					 throw new AssertionError( e );
				}
			  };
		 }

		 private ThrowingAction<Exception> DropAndReCreateIndex( IndexReference descriptor, SchemaDescriptor newDescriptor )
		 {
			  return () =>
			  {
				_aliceLatch.await();
				_bobLatch.await();
				using ( KernelTransactionImplementation transaction = KernelTransaction )
				{
					 SchemaWrite schemaWrite = transaction.schemaWrite();
					 schemaWrite.indexDrop( descriptor );
					 schemaWrite.indexCreate( newDescriptor, FulltextIndexProviderFactory.Descriptor.name(), "nodes" );
					 transaction.success();
				}
			  };
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void labelledNodesCoreAPI() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LabelledNodesCoreAPI()
		 {
			  string[] IEntityTokens = new string[] { Label.name() };
			  SchemaDescriptor descriptor = GetExistingDescriptor( IEntityTokens );
			  SchemaDescriptor newDescriptor = GetNewDescriptor( IEntityTokens );
			  IndexReference initialIndex = CreateInitialIndex( descriptor );

			  ThreadStart aliceWork = Work(_nodesCreatedPerThread, () =>
			  {
				Db.getNodeById( CreateNodeIndexableByPropertyValue( Label, "alice" ) );
				_aliceLatch.Signal();
			  });
			  ThreadStart bobWork = Work(_nodesCreatedPerThread, () =>
			  {
				Db.getNodeById( CreateNodeWithProperty( Label, "otherProp", "bob" ) );
				_bobLatch.Signal();
			  });
			  ThreadStart changeConfig = Work( 1, DropAndReCreateIndex( initialIndex, newDescriptor ) );
			  RaceContestantsAndVerifyResults( newDescriptor, aliceWork, changeConfig, bobWork );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void labelledNodesCypherCurrent() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LabelledNodesCypherCurrent()
		 {
			  string[] IEntityTokens = new string[] { Label.name() };
			  SchemaDescriptor descriptor = GetExistingDescriptor( IEntityTokens );
			  SchemaDescriptor newDescriptor = GetNewDescriptor( IEntityTokens );
			  IndexReference initialIndex = CreateInitialIndex( descriptor );

			  ThreadStart aliceWork = Work(_nodesCreatedPerThread, () =>
			  {
				Db.execute( "create (:LABEL {" + PROP + ": \"alice\"})" ).close();
				_aliceLatch.Signal();
			  });
			  ThreadStart bobWork = Work(_nodesCreatedPerThread, () =>
			  {
				Db.execute( "create (:LABEL {otherProp: \"bob\"})" ).close();
				_bobLatch.Signal();
			  });
			  ThreadStart changeConfig = Work( 1, DropAndReCreateIndex( initialIndex, newDescriptor ) );
			  RaceContestantsAndVerifyResults( newDescriptor, aliceWork, changeConfig, bobWork );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void labelledNodesCypher31() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LabelledNodesCypher31()
		 {
			  string[] IEntityTokens = new string[] { Label.name() };
			  SchemaDescriptor descriptor = GetExistingDescriptor( IEntityTokens );
			  SchemaDescriptor newDescriptor = GetNewDescriptor( IEntityTokens );
			  IndexReference initialIndex = CreateInitialIndex( descriptor );

			  ThreadStart aliceWork = Work(_nodesCreatedPerThread, () =>
			  {
				Db.execute( "CYPHER 3.1 create (:LABEL {" + PROP + ": \"alice\"})" ).close();
				_aliceLatch.Signal();
			  });
			  ThreadStart bobWork = Work(_nodesCreatedPerThread, () =>
			  {
				Db.execute( "CYPHER 3.1 create (:LABEL {otherProp: \"bob\"})" ).close();
				_bobLatch.Signal();
			  });
			  ThreadStart changeConfig = Work( 1, DropAndReCreateIndex( initialIndex, newDescriptor ) );
			  RaceContestantsAndVerifyResults( newDescriptor, aliceWork, changeConfig, bobWork );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void labelledNodesCypher23() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LabelledNodesCypher23()
		 {
			  string[] IEntityTokens = new string[] { Label.name() };
			  SchemaDescriptor descriptor = GetExistingDescriptor( IEntityTokens );
			  SchemaDescriptor newDescriptor = GetNewDescriptor( IEntityTokens );
			  IndexReference initialIndex = CreateInitialIndex( descriptor );

			  ThreadStart aliceWork = Work(_nodesCreatedPerThread, () =>
			  {
				Db.execute( "CYPHER 2.3 create (:LABEL {" + PROP + ": \"alice\"})" ).close();
				_aliceLatch.Signal();
			  });
			  ThreadStart bobWork = Work(_nodesCreatedPerThread, () =>
			  {
				Db.execute( "CYPHER 2.3 create (:LABEL {otherProp: \"bob\"})" ).close();
				_bobLatch.Signal();
			  });
			  ThreadStart changeConfig = Work( 1, DropAndReCreateIndex( initialIndex, newDescriptor ) );
			  RaceContestantsAndVerifyResults( newDescriptor, aliceWork, changeConfig, bobWork );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void labelledNodesCypherRule() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void LabelledNodesCypherRule()
		 {
			  string[] IEntityTokens = new string[] { Label.name() };
			  SchemaDescriptor descriptor = GetExistingDescriptor( IEntityTokens );
			  SchemaDescriptor newDescriptor = GetNewDescriptor( IEntityTokens );
			  IndexReference initialIndex = CreateInitialIndex( descriptor );

			  ThreadStart aliceWork = Work(_nodesCreatedPerThread, () =>
			  {
				Db.execute( "CYPHER planner=rule create (:LABEL {" + PROP + ": \"alice\"})" ).close();
				_aliceLatch.Signal();
			  });
			  ThreadStart bobWork = Work(_nodesCreatedPerThread, () =>
			  {
				Db.execute( "CYPHER planner=rule create (:LABEL {otherProp: \"bob\"})" ).close();
				_bobLatch.Signal();
			  });
			  ThreadStart changeConfig = Work( 1, DropAndReCreateIndex( initialIndex, newDescriptor ) );
			  RaceContestantsAndVerifyResults( newDescriptor, aliceWork, changeConfig, bobWork );
		 }
	}

}