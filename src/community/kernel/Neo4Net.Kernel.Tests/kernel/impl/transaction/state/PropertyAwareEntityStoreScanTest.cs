using System;

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
namespace Neo4Net.Kernel.impl.transaction.state
{
	using IntPredicates = org.eclipse.collections.impl.block.factory.primitive.IntPredicates;
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using Neo4Net.Kernel.Impl.Api.index;
	using LockService = Neo4Net.Kernel.impl.locking.LockService;
	using RecordStorageReader = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageReader;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using NodeStore = Neo4Net.Kernel.impl.store.NodeStore;
	using PropertyStore = Neo4Net.Kernel.impl.store.PropertyStore;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;
	using RecordLoad = Neo4Net.Kernel.Impl.Store.Records.RecordLoad;
	using Neo4Net.Kernel.impl.transaction.state.storeview;
	using StorageNodeCursor = Neo4Net.Storageengine.Api.StorageNodeCursor;
	using StorageReader = Neo4Net.Storageengine.Api.StorageReader;
	using PopulationProgress = Neo4Net.Storageengine.Api.schema.PopulationProgress;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.RETURNS_MOCKS;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class PropertyAwareEntityStoreScanTest
	{
		 private readonly LockService _locks = mock( typeof( LockService ), RETURNS_MOCKS );
		 private readonly NodeStore _nodeStore = mock( typeof( NodeStore ) );
		 private readonly PropertyStore _propertyStore = mock( typeof( PropertyStore ) );
		 private readonly NeoStores _neoStores = mock( typeof( NeoStores ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  when( _neoStores.NodeStore ).thenReturn( _nodeStore );
			  when( _neoStores.PropertyStore ).thenReturn( _propertyStore );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveBackCompletionPercentage()
		 public virtual void ShouldGiveBackCompletionPercentage()
		 {
			  // given
			  long total = 10;
			  when( _nodeStore.HighId ).thenReturn( total );
			  NodeRecord emptyRecord = new NodeRecord( 0 );
			  NodeRecord inUseRecord = new NodeRecord( 42 );
			  inUseRecord.InUse = true;
			  when( _nodeStore.newRecord() ).thenReturn(emptyRecord);
			  when( _nodeStore.getRecord( anyLong(), any(typeof(NodeRecord)), any(typeof(RecordLoad)) ) ).thenReturn(inUseRecord, inUseRecord, inUseRecord, inUseRecord, inUseRecord, inUseRecord, inUseRecord, inUseRecord, inUseRecord, inUseRecord);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final PercentageSupplier percentageSupplier = new PercentageSupplier();
			  PercentageSupplier percentageSupplier = new PercentageSupplier();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.kernel.impl.transaction.state.storeview.PropertyAwareEntityStoreScan<org.neo4j.storageengine.api.StorageNodeCursor,RuntimeException> scan = new org.neo4j.kernel.impl.transaction.state.storeview.PropertyAwareEntityStoreScan<org.neo4j.storageengine.api.StorageNodeCursor,RuntimeException>(new org.neo4j.kernel.impl.storageengine.impl.recordstorage.RecordStorageReader(neoStores), total, org.eclipse.collections.impl.block.factory.primitive.IntPredicates.alwaysTrue(), id -> locks.acquireNodeLock(id, org.neo4j.kernel.impl.locking.LockService_LockType.READ_LOCK))
			  PropertyAwareEntityStoreScan<StorageNodeCursor, Exception> scan = new PropertyAwareEntityStoreScanAnonymousInnerClass( this, total, IntPredicates.alwaysTrue(), percentageSupplier );
			  percentageSupplier.StoreScan = scan;

			  // when
			  scan.Run();
		 }

		 private class PropertyAwareEntityStoreScanAnonymousInnerClass : PropertyAwareEntityStoreScan<StorageNodeCursor, Exception>
		 {
			 private readonly PropertyAwareEntityStoreScanTest _outerInstance;

			 private long _total;
			 private Neo4Net.Kernel.impl.transaction.state.PropertyAwareEntityStoreScanTest.PercentageSupplier _percentageSupplier;

			 public PropertyAwareEntityStoreScanAnonymousInnerClass( PropertyAwareEntityStoreScanTest outerInstance, long total, UnknownType alwaysTrue, Neo4Net.Kernel.impl.transaction.state.PropertyAwareEntityStoreScanTest.PercentageSupplier percentageSupplier ) : base( new RecordStorageReader( outerInstance.neoStores ), total, alwaysTrue, id -> outerInstance.locks.AcquireNodeLock( id, org.neo4j.kernel.impl.locking.LockService_LockType.ReadLock ) )
			 {
				 this.outerInstance = outerInstance;
				 this._total = total;
				 this._percentageSupplier = percentageSupplier;
			 }

			 private int read;

			 public override bool process( StorageNodeCursor node )
			 {
				  // then
				  read++;
				  float expected = ( float ) read / _total;
				  float actual = _percentageSupplier.get();
				  assertEquals( string.Format( "{0:F}=={1:F}", expected, actual ), expected, actual, 0.0 );
				  return false;
			 }

			 protected internal override StorageNodeCursor allocateCursor( StorageReader storageReader )
			 {
				  return storageReader.AllocateNodeCursor();
			 }
		 }

		 private class PercentageSupplier : System.Func<float>
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.kernel.impl.api.index.StoreScan<?> storeScan;
			  internal StoreScan<object> StoreScanConflict;

			  public override float? Get()
			  {
					assertNotNull( StoreScanConflict );
					PopulationProgress progress = StoreScanConflict.Progress;
					return ( float ) progress.Completed / ( float ) progress.Total;
			  }

			  internal virtual StoreScan<T1> StoreScan<T1>
			  {
				  set
				  {
						this.StoreScanConflict = value;
				  }
			  }
		 }
	}

}