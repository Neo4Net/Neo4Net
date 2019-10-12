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
namespace Org.Neo4j.@unsafe.Impl.Batchimport
{
	using Test = org.junit.Test;


	using Org.Neo4j.Kernel.impl.store;
	using Record = Org.Neo4j.Kernel.impl.store.record.Record;
	using RelationshipGroupRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipGroupRecord;
	using Group = Org.Neo4j.@unsafe.Impl.Batchimport.ReadGroupsFromCacheStepTest.Group;
	using BatchSender = Org.Neo4j.@unsafe.Impl.Batchimport.staging.BatchSender;
	using StageControl = Org.Neo4j.@unsafe.Impl.Batchimport.staging.StageControl;
	using Org.Neo4j.@unsafe.Impl.Batchimport.staging;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.Configuration.DEFAULT;

	public class EncodeGroupsStepTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test public void shouldEncodeGroupChains() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldEncodeGroupChains()
		 {
			  // GIVEN
			  StageControl control = mock( typeof( StageControl ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicLong nextId = new java.util.concurrent.atomic.AtomicLong();
			  AtomicLong nextId = new AtomicLong();
			  RecordStore<RelationshipGroupRecord> store = mock( typeof( RecordStore ) );
			  when( store.nextId() ).thenAnswer(invocation => nextId.incrementAndGet());
			  doAnswer(invocation =>
			  {
				// our own way of marking that this has record been prepared (firstOut=1)
				invocation.getArgument<RelationshipGroupRecord>( 0 ).FirstOut = 1;
				return null;
			  }).when( store ).prepareForCommit( any( typeof( RelationshipGroupRecord ) ) );
			  Configuration config = Configuration.withBatchSize( DEFAULT, 10 );
			  EncodeGroupsStep encoder = new EncodeGroupsStep( control, config, store );

			  // WHEN
			  encoder.Start( Org.Neo4j.@unsafe.Impl.Batchimport.staging.Step_Fields.ORDER_SEND_DOWNSTREAM );
			  Catcher catcher = new Catcher();
			  encoder.Process( Batch( new Group( 1, 3 ), new Group( 2, 3 ), new Group( 3, 4 ) ), catcher );
			  encoder.Process( Batch( new Group( 4, 2 ), new Group( 5, 10 ) ), catcher );
			  encoder.Process( Batch( new Group( 6, 35 ) ), catcher );
			  encoder.Process( Batch( new Group( 7, 2 ) ), catcher );
			  encoder.endOfUpstream();
			  encoder.awaitCompleted();
			  encoder.Close();

			  // THEN
			  assertEquals( 4, catcher.Batches.Count );
			  long lastOwningNodeLastBatch = -1;
			  foreach ( RelationshipGroupRecord[] batch in catcher.Batches )
			  {
					AssertBatch( batch, lastOwningNodeLastBatch );
					lastOwningNodeLastBatch = batch[batch.Length - 1].OwningNode;
			  }
		 }

		 private void AssertBatch( RelationshipGroupRecord[] batch, long lastOwningNodeLastBatch )
		 {
			  for ( int i = 0; i < batch.Length; i++ )
			  {
					RelationshipGroupRecord record = batch[i];
					assertTrue( record.Id > Record.NULL_REFERENCE.longValue() );
					assertTrue( record.OwningNode > lastOwningNodeLastBatch );
					assertEquals( 1, record.FirstOut ); // the mark our store mock sets when preparing
					if ( record.Next == Record.NULL_REFERENCE.longValue() )
					{
						 // This is the last in the chain, verify that this is either:
						 assertTrue( i == batch.Length - 1 || batch[i + 1].OwningNode > record.OwningNode );
					}
			  }
		 }

		 private RelationshipGroupRecord[] Batch( params Group[] groups )
		 {
			  return ReadGroupsFromCacheStepTest.Groups( groups ).ToArray();
		 }

		 private class Catcher : BatchSender
		 {
			  internal readonly IList<RelationshipGroupRecord[]> Batches = new List<RelationshipGroupRecord[]>();

			  public override void Send( object batch )
			  {
					Batches.Add( ( RelationshipGroupRecord[] ) batch );
			  }
		 }
	}

}