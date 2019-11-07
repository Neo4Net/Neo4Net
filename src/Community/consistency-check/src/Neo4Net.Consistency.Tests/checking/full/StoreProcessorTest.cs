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
namespace Neo4Net.Consistency.checking.full
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Neo4Net.Consistency.checking;
	using CacheAccess = Neo4Net.Consistency.checking.cache.CacheAccess;
	using ConsistencyReport = Neo4Net.Consistency.report.ConsistencyReport;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using InvalidRecordException = Neo4Net.Kernel.impl.store.InvalidRecordException;
	using Neo4Net.Kernel.impl.store;
	using StoreType = Neo4Net.Kernel.impl.store.StoreType;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using RecordLoad = Neo4Net.Kernel.Impl.Store.Records.RecordLoad;
	using NeoStoresRule = Neo4Net.Test.rule.NeoStoresRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;

	public class StoreProcessorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.NeoStoresRule stores = new Neo4Net.test.rule.NeoStoresRule(getClass(), Neo4Net.kernel.impl.store.StoreType.NODE, Neo4Net.kernel.impl.store.StoreType.NODE_LABEL);
		 public readonly NeoStoresRule Stores = new NeoStoresRule( this.GetType(), StoreType.NODE, StoreType.NODE_LABEL );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test public void shouldProcessAllTheRecordsInAStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProcessAllTheRecordsInAStore()
		 {
			  // given
			  RecordStore<NodeRecord> nodeStore = Stores.builder().build().NodeStore;
			  Neo4Net.Consistency.report.ConsistencyReport_Reporter reporter = mock( typeof( Neo4Net.Consistency.report.ConsistencyReport_Reporter ) );
			  StoreProcessor processor = new StoreProcessor( Neo4Net.Consistency.checking.CheckDecorator_Fields.None, reporter, Stage_Fields.SequentialForward, CacheAccess.EMPTY );
			  nodeStore.UpdateRecord( Node( 0, false, 0, 0 ) );
			  nodeStore.UpdateRecord( Node( 1, false, 0, 0 ) );
			  nodeStore.UpdateRecord( Node( 2, false, 0, 0 ) );
			  nodeStore.HighestPossibleIdInUse = 2;

			  // when
			  processor.ApplyFiltered( nodeStore );

			  // then
			  verify( reporter, times( 3 ) ).forNode( any( typeof( NodeRecord ) ), any( typeof( RecordCheck ) ) );
		 }

		 private NodeRecord Node( long id, bool dense, long nextRel, long nextProp )
		 {
			  return ( new NodeRecord( id ) ).initialize( true, nextProp, dense, nextRel, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test public void shouldStopProcessingRecordsWhenSignalledToStop() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldStopProcessingRecordsWhenSignalledToStop()
		 {
			  // given
			  Neo4Net.Consistency.report.ConsistencyReport_Reporter reporter = mock( typeof( Neo4Net.Consistency.report.ConsistencyReport_Reporter ) );
			  StoreProcessor processor = new StoreProcessor( Neo4Net.Consistency.checking.CheckDecorator_Fields.None, reporter, Stage_Fields.SequentialForward, CacheAccess.EMPTY );
			  RecordStore<NodeRecord> nodeStore = new RecordStore_DelegatorAnonymousInnerClass( this, processor );
			  nodeStore.UpdateRecord( Node( 0, false, 0, 0 ) );
			  nodeStore.UpdateRecord( Node( 1, false, 0, 0 ) );
			  nodeStore.UpdateRecord( Node( 2, false, 0, 0 ) );
			  nodeStore.UpdateRecord( Node( 3, false, 0, 0 ) );
			  nodeStore.UpdateRecord( Node( 4, false, 0, 0 ) );
			  nodeStore.HighestPossibleIdInUse = 4;

			  // when
			  processor.ApplyFiltered( nodeStore );

			  // then
			  verify( reporter, times( 3 ) ).forNode( any( typeof( NodeRecord ) ), any( typeof( RecordCheck ) ) );
		 }

		 private class RecordStore_DelegatorAnonymousInnerClass : Neo4Net.Kernel.impl.store.RecordStore_Delegator<NodeRecord>
		 {
			 private readonly StoreProcessorTest _outerInstance;

			 private Neo4Net.Consistency.checking.full.StoreProcessor _processor;

			 public RecordStore_DelegatorAnonymousInnerClass( StoreProcessorTest outerInstance, Neo4Net.Consistency.checking.full.StoreProcessor processor ) : base( outerInstance.Stores.builder().build().NodeStore )
			 {
				 this.outerInstance = outerInstance;
				 this._processor = processor;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void getRecordByCursor(long id, Neo4Net.kernel.impl.store.record.NodeRecord target, Neo4Net.kernel.impl.store.record.RecordLoad mode, Neo4Net.io.pagecache.PageCursor cursor) throws Neo4Net.kernel.impl.store.InvalidRecordException
			 public override void getRecordByCursor( long id, NodeRecord target, RecordLoad mode, PageCursor cursor )
			 {
				  if ( id == 3 )
				  {
						_processor.stop();
				  }
				  base.getRecordByCursor( id, target, mode, cursor );
			 }
		 }
	}

}