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
namespace Neo4Net.Kernel.Impl.Newapi
{
	using Before = org.junit.Before;
	using Test = org.junit.Test;
	using InOrder = org.mockito.InOrder;

	using IndexQuery = Neo4Net.Kernel.Api.Internal.IndexQuery;
	using IndexReference = Neo4Net.Kernel.Api.Internal.IndexReference;
	using NodeValueIndexCursor = Neo4Net.Kernel.Api.Internal.NodeValueIndexCursor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using Locks = Neo4Net.Kernel.impl.locking.Locks;
	using Neo4Net.Kernel.Impl.Newapi.LockingNodeUniqueIndexSeek;
	using LockTracer = Neo4Net.Kernel.Api.StorageEngine.@lock.LockTracer;
	using IndexDescriptorFactory = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptorFactory;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.inOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.IndexQuery.exact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.locking.ResourceTypes.INDEX_ENTRY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.locking.ResourceTypes.indexEntryResourceId;

	public class LockingNodeUniqueIndexSeekTest
	{
		private bool InstanceFieldsInitialized = false;

		public LockingNodeUniqueIndexSeekTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_index = IndexDescriptorFactory.uniqueForSchema( SchemaDescriptorFactory.forLabel( _labelId, _propertyKeyId ) );
			_predicate = exact( _propertyKeyId, _value );
			_resourceId = indexEntryResourceId( _labelId, _predicate );
		}

		 private readonly int _labelId = 1;
		 private readonly int _propertyKeyId = 2;
		 private IndexReference _index;

		 private readonly Value _value = Values.of( "value" );
		 private IndexQuery.ExactPredicate _predicate;
		 private long _resourceId;
		 private UniqueNodeIndexSeeker<NodeValueIndexCursor> _uniqueNodeIndexSeeker = mock( typeof( UniqueNodeIndexSeeker ) );

		 private readonly Neo4Net.Kernel.impl.locking.Locks_Client _locks = mock( typeof( Neo4Net.Kernel.impl.locking.Locks_Client ) );
		 private readonly Read _read = mock( typeof( Read ) );
		 private InOrder _order;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _order = inOrder( _locks );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHoldSharedIndexLockIfNodeIsExists() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHoldSharedIndexLockIfNodeIsExists()
		 {
			  // given
			  NodeValueIndexCursor cursor = mock( typeof( NodeValueIndexCursor ) );
			  when( cursor.Next() ).thenReturn(true);
			  when( cursor.NodeReference() ).thenReturn(42L);

			  // when
			  long nodeId = LockingNodeUniqueIndexSeek.Apply( _locks, LockTracer.NONE, () => cursor, _uniqueNodeIndexSeeker, _read, _index, _predicate );

			  // then
			  assertEquals( 42L, nodeId );
			  verify( _locks ).acquireShared( LockTracer.NONE, INDEX_ENTRY, _resourceId );
			  verifyNoMoreInteractions( _locks );

			  verify( cursor ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHoldSharedIndexLockIfNodeIsConcurrentlyCreated() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHoldSharedIndexLockIfNodeIsConcurrentlyCreated()
		 {
			  // given
			  NodeValueIndexCursor cursor = mock( typeof( NodeValueIndexCursor ) );
			  when( cursor.Next() ).thenReturn(false, true);
			  when( cursor.NodeReference() ).thenReturn(42L);

			  // when
			  long nodeId = LockingNodeUniqueIndexSeek.Apply( _locks, LockTracer.NONE, () => cursor, _uniqueNodeIndexSeeker, _read, _index, _predicate );

			  // then
			  assertEquals( 42L, nodeId );
			  _order.verify( _locks ).acquireShared( LockTracer.NONE, INDEX_ENTRY, _resourceId );
			  _order.verify( _locks ).releaseShared( INDEX_ENTRY, _resourceId );
			  _order.verify( _locks ).acquireExclusive( LockTracer.NONE, INDEX_ENTRY, _resourceId );
			  _order.verify( _locks ).acquireShared( LockTracer.NONE, INDEX_ENTRY, _resourceId );
			  _order.verify( _locks ).releaseExclusive( INDEX_ENTRY, _resourceId );
			  verifyNoMoreInteractions( _locks );

			  verify( cursor ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHoldExclusiveIndexLockIfNodeDoesNotExist() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHoldExclusiveIndexLockIfNodeDoesNotExist()
		 {
			  // given
			  NodeValueIndexCursor cursor = mock( typeof( NodeValueIndexCursor ) );
			  when( cursor.Next() ).thenReturn(false, false);
			  when( cursor.NodeReference() ).thenReturn(-1L);

			  // when
			  long nodeId = LockingNodeUniqueIndexSeek.Apply( _locks, LockTracer.NONE, () => cursor, _uniqueNodeIndexSeeker, _read, _index, _predicate );

			  // then
			  assertEquals( -1L, nodeId );
			  _order.verify( _locks ).acquireShared( LockTracer.NONE, INDEX_ENTRY, _resourceId );
			  _order.verify( _locks ).releaseShared( INDEX_ENTRY, _resourceId );
			  _order.verify( _locks ).acquireExclusive( LockTracer.NONE, INDEX_ENTRY, _resourceId );
			  verifyNoMoreInteractions( _locks );

			  verify( cursor ).close();
		 }
	}

}