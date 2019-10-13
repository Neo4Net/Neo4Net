using System.Collections.Generic;

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
namespace Neo4Net.@unsafe.Impl.Batchimport
{
	using Test = org.junit.Test;


	using RelationshipGroupRecord = Neo4Net.Kernel.impl.store.record.RelationshipGroupRecord;
	using BatchSender = Neo4Net.@unsafe.Impl.Batchimport.staging.BatchSender;
	using Neo4Net.@unsafe.Impl.Batchimport.staging;
	using Stage = Neo4Net.@unsafe.Impl.Batchimport.staging.Stage;
	using StageControl = Neo4Net.@unsafe.Impl.Batchimport.staging.StageControl;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.Record.NULL_REFERENCE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.Configuration.DEFAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.staging.ExecutionSupervisors.superviseDynamicExecution;

	public class ReadGroupsFromCacheStepTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProduceCompleteBatchesPerOwner()
		 public virtual void ShouldProduceCompleteBatchesPerOwner()
		 {
			  // GIVEN
			  Configuration config = Configuration.withBatchSize( DEFAULT, 10 );
			  IEnumerator<RelationshipGroupRecord> groups = groups( new Group( 1, 3 ), new Group( 2, 3 ), new Group( 3, 4 ), new Group( 4, 2 ), new Group( 5, 10 ), new Group( 6, 35 ), new Group( 7, 2 ) ).GetEnumerator();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger processCounter = new java.util.concurrent.atomic.AtomicInteger();
			  AtomicInteger processCounter = new AtomicInteger();
			  Stage stage = new StageAnonymousInnerClass( this, config, groups, processCounter );

			  // WHEN processing the data
			  superviseDynamicExecution( stage );

			  // THEN
			  assertEquals( 4, processCounter.get() );
		 }

		 private class StageAnonymousInnerClass : Stage
		 {
			 private readonly ReadGroupsFromCacheStepTest _outerInstance;

			 private Neo4Net.@unsafe.Impl.Batchimport.Configuration _config;
			 private IEnumerator<RelationshipGroupRecord> _groups;
			 private AtomicInteger _processCounter;

			 public StageAnonymousInnerClass( ReadGroupsFromCacheStepTest outerInstance, Neo4Net.@unsafe.Impl.Batchimport.Configuration config, IEnumerator<RelationshipGroupRecord> groups, AtomicInteger processCounter ) : base( this.GetType().Name, null, config, 0 )
			 {
				 this.outerInstance = outerInstance;
				 this._config = config;
				 this._groups = groups;
				 this._processCounter = processCounter;

				 add( new ReadGroupsFromCacheStep( control(), config, groups, 1 ) );
				 add( new VerifierStep( control(), config, processCounter ) );
			 }

		 }

		 protected internal static IList<RelationshipGroupRecord> Groups( params Group[] groups )
		 {
			  IList<RelationshipGroupRecord> records = new List<RelationshipGroupRecord>();
			  foreach ( Group group in groups )
			  {
					for ( int i = 0; i < group.Count; i++ )
					{
						 RelationshipGroupRecord record = new RelationshipGroupRecord( NULL_REFERENCE.longValue() );
						 record.OwningNode = group.OwningNode;
						 record.Next = group.Count - i - 1; // count: how many come after it (importer does this)
						 records.Add( record );
					}
			  }
			  return records;
		 }

		 protected internal class Group
		 {
			  internal readonly long OwningNode;
			  internal readonly int Count;

			  public Group( long owningNode, int count )
			  {
					this.OwningNode = owningNode;
					this.Count = count;
			  }
		 }

		 private class VerifierStep : ProcessorStep<RelationshipGroupRecord[]>
		 {
			  internal long LastBatchLastOwningNode = -1;
			  internal readonly AtomicInteger ProcessCounter;

			  internal VerifierStep( StageControl control, Configuration config, AtomicInteger processCounter ) : base( control, "Verifier", config, 1 )
			  {
					this.ProcessCounter = processCounter;
			  }

			  protected internal override void Process( RelationshipGroupRecord[] batch, BatchSender sender )
			  {
					long lastOwningNode = LastBatchLastOwningNode;
					foreach ( RelationshipGroupRecord record in batch )
					{
						 assertTrue( record.OwningNode >= lastOwningNode );
						 assertTrue( record.OwningNode > LastBatchLastOwningNode );
					}
					ProcessCounter.incrementAndGet();
					if ( batch.Length > 0 )
					{
						 LastBatchLastOwningNode = batch[batch.Length - 1].OwningNode;
					}
			  }
		 }
	}

}