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
namespace Org.Neo4j.Consistency.checking.full
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Org.Neo4j.Consistency.checking;
	using CacheAccess = Org.Neo4j.Consistency.checking.cache.CacheAccess;
	using ConsistencyReport = Org.Neo4j.Consistency.report.ConsistencyReport;
	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;
	using InvalidRecordException = Org.Neo4j.Kernel.impl.store.InvalidRecordException;
	using Org.Neo4j.Kernel.impl.store;
	using StoreType = Org.Neo4j.Kernel.impl.store.StoreType;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;
	using RecordLoad = Org.Neo4j.Kernel.impl.store.record.RecordLoad;
	using NeoStoresRule = Org.Neo4j.Test.rule.NeoStoresRule;

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
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.NeoStoresRule stores = new org.neo4j.test.rule.NeoStoresRule(getClass(), org.neo4j.kernel.impl.store.StoreType.NODE, org.neo4j.kernel.impl.store.StoreType.NODE_LABEL);
		 public readonly NeoStoresRule Stores = new NeoStoresRule( this.GetType(), StoreType.NODE, StoreType.NODE_LABEL );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Test public void shouldProcessAllTheRecordsInAStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProcessAllTheRecordsInAStore()
		 {
			  // given
			  RecordStore<NodeRecord> nodeStore = Stores.builder().build().NodeStore;
			  Org.Neo4j.Consistency.report.ConsistencyReport_Reporter reporter = mock( typeof( Org.Neo4j.Consistency.report.ConsistencyReport_Reporter ) );
			  StoreProcessor processor = new StoreProcessor( Org.Neo4j.Consistency.checking.CheckDecorator_Fields.None, reporter, Stage_Fields.SequentialForward, CacheAccess.EMPTY );
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
			  Org.Neo4j.Consistency.report.ConsistencyReport_Reporter reporter = mock( typeof( Org.Neo4j.Consistency.report.ConsistencyReport_Reporter ) );
			  StoreProcessor processor = new StoreProcessor( Org.Neo4j.Consistency.checking.CheckDecorator_Fields.None, reporter, Stage_Fields.SequentialForward, CacheAccess.EMPTY );
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

		 private class RecordStore_DelegatorAnonymousInnerClass : Org.Neo4j.Kernel.impl.store.RecordStore_Delegator<NodeRecord>
		 {
			 private readonly StoreProcessorTest _outerInstance;

			 private Org.Neo4j.Consistency.checking.full.StoreProcessor _processor;

			 public RecordStore_DelegatorAnonymousInnerClass( StoreProcessorTest outerInstance, Org.Neo4j.Consistency.checking.full.StoreProcessor processor ) : base( outerInstance.Stores.builder().build().NodeStore )
			 {
				 this.outerInstance = outerInstance;
				 this._processor = processor;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void getRecordByCursor(long id, org.neo4j.kernel.impl.store.record.NodeRecord target, org.neo4j.kernel.impl.store.record.RecordLoad mode, org.neo4j.io.pagecache.PageCursor cursor) throws org.neo4j.kernel.impl.store.InvalidRecordException
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