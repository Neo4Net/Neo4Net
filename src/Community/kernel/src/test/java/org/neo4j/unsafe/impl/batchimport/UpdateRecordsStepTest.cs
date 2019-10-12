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
namespace Neo4Net.@unsafe.Impl.Batchimport
{
	using Test = org.junit.Test;

	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using Neo4Net.Kernel.impl.store;
	using IdGeneratorImpl = Neo4Net.Kernel.impl.store.id.IdGeneratorImpl;
	using IdSequence = Neo4Net.Kernel.impl.store.id.IdSequence;
	using NodeRecord = Neo4Net.Kernel.impl.store.record.NodeRecord;
	using BatchSender = Neo4Net.@unsafe.Impl.Batchimport.staging.BatchSender;
	using StageControl = Neo4Net.@unsafe.Impl.Batchimport.staging.StageControl;
	using Keys = Neo4Net.@unsafe.Impl.Batchimport.stats.Keys;
	using Stat = Neo4Net.@unsafe.Impl.Batchimport.stats.Stat;
	using StorePrepareIdSequence = Neo4Net.@unsafe.Impl.Batchimport.store.StorePrepareIdSequence;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Matchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Matchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class UpdateRecordsStepTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void ioThroughputStatDoesNotOverflow()
		 public virtual void IoThroughputStatDoesNotOverflow()
		 {
			  // store with huge record size to force overflow and not create huge batch of records
			  RecordStore<NodeRecord> store = mock( typeof( RecordStore ) );
			  when( store.RecordSize ).thenReturn( int.MaxValue / 2 );

			  Configuration configuration = mock( typeof( Configuration ) );
			  StageControl stageControl = mock( typeof( StageControl ) );
			  UpdateRecordsStep<NodeRecord> step = new UpdateRecordsStep<NodeRecord>( stageControl, configuration, store, new StorePrepareIdSequence() );

			  NodeRecord record = new NodeRecord( 1 );
			  record.InUse = true;
			  NodeRecord[] batch = new NodeRecord[11];
			  Arrays.fill( batch, record );

			  step.Process( batch, mock( typeof( BatchSender ) ) );

			  Stat stat = step.Stat( Keys.io_throughput );

			  assertThat( stat.AsLong(), greaterThan(0L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void recordWithReservedIdIsSkipped()
		 public virtual void RecordWithReservedIdIsSkipped()
		 {
			  RecordStore<NodeRecord> store = mock( typeof( NodeStore ) );
			  StageControl stageControl = mock( typeof( StageControl ) );
			  UpdateRecordsStep<NodeRecord> step = new UpdateRecordsStep<NodeRecord>( stageControl, Configuration.DEFAULT, store, new StorePrepareIdSequence() );

			  NodeRecord node1 = new NodeRecord( 1 );
			  node1.InUse = true;
			  NodeRecord node2 = new NodeRecord( 2 );
			  node2.InUse = true;
			  NodeRecord nodeWithReservedId = new NodeRecord( IdGeneratorImpl.INTEGER_MINUS_ONE );
			  NodeRecord[] batch = new NodeRecord[] { node1, node2, nodeWithReservedId };

			  step.Process( batch, mock( typeof( BatchSender ) ) );

			  verify( store ).prepareForCommit( eq( node1 ), any( typeof( IdSequence ) ) );
			  verify( store ).updateRecord( node1 );
			  verify( store ).prepareForCommit( eq( node2 ), any( typeof( IdSequence ) ) );
			  verify( store ).updateRecord( node2 );
			  verify( store, never() ).prepareForCommit(eq(nodeWithReservedId), any(typeof(IdSequence)));
			  verify( store, never() ).updateRecord(nodeWithReservedId);
		 }
	}

}