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
	using Test = org.junit.Test;

	using TxState = Neo4Net.Kernel.Impl.Api.state.TxState;
	using StorageRelationshipTraversalCursor = Neo4Net.Storageengine.Api.StorageRelationshipTraversalCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class DefaultRelationshipTraversalCursorTest
	{
		private bool InstanceFieldsInitialized = false;

		public DefaultRelationshipTraversalCursorTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_noRel = Rel( -1L, -1L, -1L, -1 );
		}

		 private DefaultCursors _pool = mock( typeof( DefaultCursors ) );
		 private long _node = 42;
		 private int _type = 9999;
		 private int _type2 = 9998;
		 private long _relationship = 100;
		 private long _relationshipGroup = 313;

		 // Regular traversal of a sparse chain

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void regularSparseTraversal()
		 public virtual void RegularSparseTraversal()
		 {
			  RegularTraversal( _relationship );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void regularSparseTraversalWithTxState()
		 public virtual void RegularSparseTraversalWithTxState()
		 {
			  RegularTraversalWithTxState( _relationship );
		 }

		 // Dense traversal is just like regular for this class, denseness is handled by the store

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void regularDenseTraversal()
		 public virtual void RegularDenseTraversal()
		 {
			  RegularTraversal( RelationshipReferenceEncoding.encodeGroup( _relationshipGroup ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void regularDenseTraversalWithTxState()
		 public virtual void RegularDenseTraversalWithTxState()
		 {
			  RegularTraversalWithTxState( RelationshipReferenceEncoding.encodeGroup( _relationshipGroup ) );
		 }

		 // Sparse traversal but with tx-state filtering

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sparseTraversalWithTxStateFiltering()
		 public virtual void SparseTraversalWithTxStateFiltering()
		 {
			  // given
			  StorageRelationshipTraversalCursor storeCursor = storeCursor( Rel( 100, _node, 50, _type ), Rel( 102, _node, 51, _type ), Rel( 104, _node, 52, _type ) );

			  DefaultRelationshipTraversalCursor cursor = new DefaultRelationshipTraversalCursor( _pool, storeCursor );
			  Read read = TxState( Rel( 3, _node, 50, _type ), Rel( 4, 50, _node, _type ), Rel( 5, _node, 50, _type2 ), Rel( 6, _node, _node, _type ), Rel( 7, _node, 52, _type ) );

			  // when
			  cursor.Init( _node, RelationshipReferenceEncoding.encodeForTxStateFiltering( _relationship ), read );

			  // then
			  AssertRelationships( cursor, 100, 3, 7, 102, 104 );
		 }

		 // Sparse traversal but with filtering both of store and tx-state

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void sparseTraversalWithFiltering()
		 public virtual void SparseTraversalWithFiltering()
		 {
			  // given
			  StorageRelationshipTraversalCursor storeCursor = storeCursor( Rel( 100, 50, _node, _type ), Rel( 101, _node, 50, _type ), Rel( 102, 50, _node, _type2 ), Rel( 103, 51, _node, _type ), Rel( 104, _node, _node, _type ) );

			  DefaultRelationshipTraversalCursor cursor = new DefaultRelationshipTraversalCursor( _pool, storeCursor );
			  Read read = TxState( Rel( 3, _node, 50, _type ), Rel( 4, 50, _node, _type ), Rel( 5, _node, 50, _type2 ), Rel( 6, _node, _node, _type ), Rel( 7, _node, 52, _type ) );

			  // when
			  cursor.Init( _node, RelationshipReferenceEncoding.encodeForFiltering( _relationship ), read );

			  // then
			  AssertRelationships( cursor, 100, 4, 103 );
		 }

		 // Empty store, but filter tx-state

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void emptyStoreOutgoingOfType()
		 public virtual void EmptyStoreOutgoingOfType()
		 {
			  // given
			  StorageRelationshipTraversalCursor storeCursor = EmptyStoreCursor();

			  DefaultRelationshipTraversalCursor cursor = new DefaultRelationshipTraversalCursor( _pool, storeCursor );
			  Read read = TxState( Rel( 3, _node, 50, _type ), Rel( 4, 50, _node, _type ), Rel( 5, _node, 50, _type2 ), Rel( 6, _node, _node, _type ), Rel( 7, _node, 52, _type ) );

			  // when
			  cursor.Init( _node, RelationshipReferenceEncoding.encodeNoOutgoingRels( _type ), read );

			  // then
			  AssertRelationships( cursor, 3, 7 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void emptyStoreIncomingOfType()
		 public virtual void EmptyStoreIncomingOfType()
		 {
			  // given
			  StorageRelationshipTraversalCursor storeCursor = EmptyStoreCursor();

			  DefaultRelationshipTraversalCursor cursor = new DefaultRelationshipTraversalCursor( _pool, storeCursor );
			  Read read = TxState( Rel( 3, _node, 50, _type ), Rel( 4, 50, _node, _type ), Rel( 5, 50, _node, _type2 ), Rel( 6, _node, _node, _type ), Rel( 7, 56, _node, _type ), Rel( 8, _node, 52, _type ) );

			  // when
			  cursor.Init( _node, RelationshipReferenceEncoding.encodeNoIncomingRels( _type ), read );

			  // then
			  AssertRelationships( cursor, 4, 7 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void emptyStoreLoopsOfType()
		 public virtual void EmptyStoreLoopsOfType()
		 {
			  // given
			  StorageRelationshipTraversalCursor storeCursor = EmptyStoreCursor();

			  DefaultRelationshipTraversalCursor cursor = new DefaultRelationshipTraversalCursor( _pool, storeCursor );
			  Read read = TxState( Rel( 3, _node, 50, _type ), Rel( 2, _node, _node, _type ), Rel( 5, 50, _node, _type2 ), Rel( 6, _node, _node, _type ), Rel( 7, 56, _node, _type ), Rel( 8, _node, 52, _type ) );

			  // when
			  cursor.Init( _node, RelationshipReferenceEncoding.encodeNoLoopRels( _type ), read );

			  // then
			  AssertRelationships( cursor, 2, 6 );
		 }

		 // HELPERS

		 private void RegularTraversal( long reference )
		 {
			  // given
			  StorageRelationshipTraversalCursor storeCursor = storeCursor( 100, 102, 104 );
			  DefaultRelationshipTraversalCursor cursor = new DefaultRelationshipTraversalCursor( _pool, storeCursor );
			  Read read = EmptyTxState();

			  // when
			  cursor.Init( _node, reference, read );

			  // then
			  AssertRelationships( cursor, 100, 102, 104 );
		 }

		 private void RegularTraversalWithTxState( long reference )
		 {
			  // given
			  StorageRelationshipTraversalCursor storeCursor = storeCursor( 100, 102, 104 );
			  DefaultRelationshipTraversalCursor cursor = new DefaultRelationshipTraversalCursor( _pool, storeCursor );
			  Read read = TxState( 3, 4 );

			  // when
			  cursor.Init( _node, reference, read );

			  // then
			  AssertRelationships( cursor, 3, 4, 100, 102, 104 );
		 }

		 private Read EmptyTxState()
		 {
			  return mock( typeof( Read ) );
		 }

		 private Read TxState( params long[] ids )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return TxState( LongStream.of( ids ).mapToObj( id => Rel( id, _node, _node, _type ) ).toArray( Rel[]::new ) );
		 }

		 private Read TxState( params Rel[] rels )
		 {
			  Read read = mock( typeof( Read ) );
			  if ( rels.Length > 0 )
			  {
					TxState txState = new TxState();
					foreach ( Rel rel in rels )
					{
						 txState.RelationshipDoCreate( rel.RelId, rel.Type, rel.SourceId, rel.TargetId );
					}
					when( read.HasTxStateWithChanges() ).thenReturn(true);
					when( read.TxState() ).thenReturn(txState);
			  }
			  return read;
		 }

		 private void AssertRelationships( DefaultRelationshipTraversalCursor cursor, params long[] expected )
		 {
			  foreach ( long expectedId in expected )
			  {
					assertTrue( cursor.Next(), "Expected relationship " + expectedId + " but got none" );
					assertEquals( expectedId, cursor.RelationshipReference(), "Expected relationship " + expectedId + " got " + cursor.RelationshipReference() );
			  }
			  assertFalse( cursor.Next(), "Expected no more relationships, but got " + cursor.RelationshipReference() );
		 }

		 private Rel Rel( long relId, long startId, long endId, int type )
		 {
			  return new Rel( this, relId, startId, endId, type );
		 }

		 private Rel _noRel;

		 private class Rel
		 {
			 private readonly DefaultRelationshipTraversalCursorTest _outerInstance;

			  internal readonly long RelId;
			  internal readonly long SourceId;
			  internal readonly long TargetId;
			  internal readonly int Type;

			  internal Rel( DefaultRelationshipTraversalCursorTest outerInstance, long relId, long sourceId, long targetId, int type )
			  {
				  this._outerInstance = outerInstance;
					this.RelId = relId;
					this.SourceId = sourceId;
					this.TargetId = targetId;
					this.Type = type;
			  }
		 }

		 private StorageRelationshipTraversalCursor EmptyStoreCursor( params long[] ids )
		 {
			  return StoreCursor( new Rel[0] );
		 }

		 private StorageRelationshipTraversalCursor StoreCursor( params long[] ids )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return StoreCursor( LongStream.of( ids ).mapToObj( id => Rel( id, -1L, -1L, -1 ) ).toArray( Rel[]::new ) );
		 }

		 private StorageRelationshipTraversalCursor StoreCursor( params Rel[] rels )
		 {
			  return new StorageRelationshipTraversalCursorAnonymousInnerClass( this, rels );
		 }

		 private class StorageRelationshipTraversalCursorAnonymousInnerClass : StorageRelationshipTraversalCursor
		 {
			 private readonly DefaultRelationshipTraversalCursorTest _outerInstance;

			 private Neo4Net.Kernel.Impl.Newapi.DefaultRelationshipTraversalCursorTest.Rel[] _rels;

			 public StorageRelationshipTraversalCursorAnonymousInnerClass( DefaultRelationshipTraversalCursorTest outerInstance, Neo4Net.Kernel.Impl.Newapi.DefaultRelationshipTraversalCursorTest.Rel[] rels )
			 {
				 this.outerInstance = outerInstance;
				 this._rels = rels;
				 i = -1;
				 rel = outerInstance.NO_REL;
			 }

			 private int i;
			 private Rel rel;

			 public long neighbourNodeReference()
			 {
				  return rel.sourceId == _outerInstance.node ? rel.targetId : rel.sourceId;
			 }

			 public long originNodeReference()
			 {
				  return _outerInstance.node;
			 }

			 public void init( long nodeReference, long reference )
			 {
			 }

			 public int type()
			 {
				  return rel.type;
			 }

			 public long sourceNodeReference()
			 {
				  return rel.sourceId;
			 }

			 public long targetNodeReference()
			 {
				  return rel.targetId;
			 }

			 public void visit( long relationshipId, int typeId, long startNodeId, long endNodeId )
			 {
				  Rel = outerInstance.rel( relationshipId, startNodeId, endNodeId, typeId );
			 }

			 public bool hasProperties()
			 {
				  throw new System.NotSupportedException( "not implemented" );
			 }

			 public long propertiesReference()
			 {
				  throw new System.NotSupportedException( "not implemented" );
			 }

			 public long entityReference()
			 {
				  return rel.relId;
			 }

			 public bool next()
			 {
				  i++;
				  if ( i < 0 || i >= _rels.Length )
				  {
						rel = _outerInstance.NO_REL;
						return false;
				  }
				  else
				  {
						rel = _rels[i];
						return true;
				  }
			 }

			 public void reset()
			 {
			 }

			 public void close()
			 {
			 }
		 }
	}

}