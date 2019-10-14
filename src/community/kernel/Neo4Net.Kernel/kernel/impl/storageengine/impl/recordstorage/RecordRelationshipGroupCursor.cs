using System.Diagnostics;

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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{
	using MutableIntObjectMap = org.eclipse.collections.api.map.primitive.MutableIntObjectMap;
	using IntObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.IntObjectHashMap;

	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using RelationshipReferenceEncoding = Neo4Net.Kernel.Impl.Newapi.RelationshipReferenceEncoding;
	using Record = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordRelationshipTraversalCursor.Record;
	using RelationshipGroupStore = Neo4Net.Kernel.impl.store.RelationshipGroupStore;
	using RelationshipStore = Neo4Net.Kernel.impl.store.RelationshipStore;
	using RecordLoad = Neo4Net.Kernel.Impl.Store.Records.RecordLoad;
	using RelationshipGroupRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipGroupRecord;
	using RelationshipRecord = Neo4Net.Kernel.Impl.Store.Records.RelationshipRecord;
	using StorageRelationshipGroupCursor = Neo4Net.Storageengine.Api.StorageRelationshipGroupCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.References.clearEncoding;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.RelationshipReferenceEncoding.encodeForFiltering;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.newapi.RelationshipReferenceEncoding.encodeForTxStateFiltering;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.storageengine.impl.recordstorage.GroupReferenceEncoding.isRelationship;

	internal class RecordRelationshipGroupCursor : RelationshipGroupRecord, StorageRelationshipGroupCursor
	{
		 private readonly RelationshipStore _relationshipStore;
		 private readonly RelationshipGroupStore _groupStore;
		 private readonly RelationshipRecord _edge = new RelationshipRecord( NO_ID );

		 private BufferedGroup _bufferedGroup;
		 private PageCursor _page;
		 private PageCursor _edgePage;
		 private bool _open;

		 internal RecordRelationshipGroupCursor( RelationshipStore relationshipStore, RelationshipGroupStore groupStore ) : base( NO_ID )
		 {
			  this._relationshipStore = relationshipStore;
			  this._groupStore = groupStore;
		 }

		 public override void Init( long nodeReference, long reference )
		 {
			  // the relationships for this node are not grouped in the store
			  if ( reference != NO_ID && isRelationship( reference ) )
			  {
					Buffer( nodeReference, clearEncoding( reference ) );
			  }
			  else // this is a normal group reference.
			  {
					Direct( nodeReference, reference );
			  }
			  _open = true;
		 }

		 /// <summary>
		 /// Sparse node, i.e. fake groups by reading the whole chain and buffering it.
		 /// </summary>
		 private void Buffer( long nodeReference, long relationshipReference )
		 {
			  OwningNode = nodeReference;
			  Id = NO_ID;
			  Next = NO_ID;

			  using ( PageCursor edgePage = _relationshipStore.openPageCursorForReading( relationshipReference ) )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.eclipse.collections.api.map.primitive.MutableIntObjectMap<BufferedGroup> buffer = new org.eclipse.collections.impl.map.mutable.primitive.IntObjectHashMap<>();
					MutableIntObjectMap<BufferedGroup> buffer = new IntObjectHashMap<BufferedGroup>();
					BufferedGroup current = null;
					while ( relationshipReference != NO_ID )
					{
						 _relationshipStore.getRecordByCursor( relationshipReference, _edge, RecordLoad.FORCE, edgePage );
						 // find the group
						 BufferedGroup group = buffer.get( _edge.Type );
						 if ( group == null )
						 {
							  buffer.put( _edge.Type, current = group = new BufferedGroup( _edge, current ) );
						 }
						 // buffer the relationship into the group
						 if ( _edge.FirstNode == nodeReference ) // outgoing or loop
						 {
							  if ( _edge.SecondNode == nodeReference ) // loop
							  {
									group.Loop( _edge );
							  }
							  else // outgoing
							  {
									group.Outgoing( _edge );
							  }
							  relationshipReference = _edge.FirstNextRel;
						 }
						 else if ( _edge.SecondNode == nodeReference ) // incoming
						 {
							  group.Incoming( _edge );
							  relationshipReference = _edge.SecondNextRel;
						 }
						 else
						 {
							  throw new System.InvalidOperationException( "not a part of the chain! TODO: better exception" );
						 }
					}
					this._bufferedGroup = new BufferedGroup( _edge, current ); // we need a dummy before the first to denote the initial pos
			  }
		 }

		 /// <summary>
		 /// Dense node, real groups iterated with every call to next.
		 /// </summary>
		 internal virtual void Direct( long nodeReference, long reference )
		 {
			  _bufferedGroup = null;
			  Clear();
			  OwningNode = nodeReference;
			  Next = reference;
			  if ( _page == null )
			  {
					_page = GroupPage( reference );
			  }
		 }

		 public override bool Next()
		 {
			  if ( Buffered )
			  {
					_bufferedGroup = _bufferedGroup.next;
					if ( _bufferedGroup != null )
					{
						 LoadFromBuffer();
						 return true;
					}
			  }

			  do
			  {
					if ( Next == NO_ID )
					{
						 //We have now run out of groups from the store, however there may still
						 //be new types that was added in the transaction that we haven't visited yet.
						 return false;
					}
					Group( this, Next, _page );
			  } while ( !InUse() );

			  return true;
		 }

		 public override void SetCurrent( int groupReference, int firstOut, int firstIn, int firstLoop )
		 {
			  Type = groupReference;
			  FirstOut = firstOut;
			  FirstIn = firstIn;
			  FirstLoop = firstLoop;
		 }

		 private void LoadFromBuffer()
		 {
			  Type = _bufferedGroup.label;
			  FirstOut = _bufferedGroup.outgoing();
			  FirstIn = _bufferedGroup.incoming();
			  FirstLoop = _bufferedGroup.loops();
		 }

		 public override void Reset()
		 {
			  if ( _open )
			  {
					_open = false;
					_bufferedGroup = null;
					Id = NO_ID;
					Clear();
			  }
		 }

		 public override int Type()
		 {
			  return Type;
		 }

		 public override int OutgoingCount()
		 {
			  return Buffered ? _bufferedGroup.outgoingCount : Count( OutgoingRawId() );
		 }

		 public override int IncomingCount()
		 {
			  return Buffered ? _bufferedGroup.incomingCount : Count( IncomingRawId() );
		 }

		 public override int LoopCount()
		 {
			  return Buffered ? _bufferedGroup.loopsCount : Count( LoopsRawId() );
		 }

		 private int Count( long reference )
		 {
			  if ( reference == NO_ID )
			  {
					return 0;
			  }
			  if ( _edgePage == null )
			  {
					_edgePage = _relationshipStore.openPageCursorForReading( reference );
			  }
			  _relationshipStore.getRecordByCursor( reference, _edge, RecordLoad.FORCE, _edgePage );
			  if ( _edge.FirstNode == OwningNode )
			  {
					return ( int ) _edge.FirstPrevRel;
			  }
			  else
			  {
					return ( int ) _edge.SecondPrevRel;
			  }
		 }

		 /// <summary>
		 /// If the returned reference points to a chain of relationships that aren't physically filtered by direction and type then
		 /// a flag in this reference can be set so that external filtering will be performed as the cursor progresses.
		 /// See <seealso cref="RelationshipReferenceEncoding.encodeForFiltering(long)"/>.
		 /// </summary>
		 public override long OutgoingReference()
		 {
			  long outgoing = FirstOut;
			  return outgoing == NO_ID ? NO_ID : EncodeRelationshipReference( outgoing );
		 }

		 /// <summary>
		 /// If the returned reference points to a chain of relationships that aren't physically filtered by direction and type then
		 /// a flag in this reference can be set so that external filtering will be performed as the cursor progresses.
		 /// See <seealso cref="RelationshipReferenceEncoding.encodeForFiltering(long)"/>.
		 /// </summary>
		 public override long IncomingReference()
		 {
			  long incoming = FirstIn;
			  return incoming == NO_ID ? NO_ID : EncodeRelationshipReference( incoming );
		 }

		 /// <summary>
		 /// If the returned reference points to a chain of relationships that aren't physically filtered by direction and type then
		 /// a flag in this reference can be set so that external filtering will be performed as the cursor progresses.
		 /// See <seealso cref="RelationshipReferenceEncoding.encodeForFiltering(long)"/>.
		 /// </summary>
		 public override long LoopsReference()
		 {
			  long loops = FirstLoop;
			  return loops == NO_ID ? NO_ID : EncodeRelationshipReference( loops );
		 }

		 public override string ToString()
		 {
			  if ( !_open )
			  {
					return "RelationshipGroupCursor[closed state]";
			  }
			  else
			  {
					string mode = "mode=";
					if ( Buffered )
					{
						 mode = mode + "group";
					}
					else
					{
						 mode = mode + "direct";
					}
					return "RelationshipGroupCursor[id=" + Id + ", open state with: " + mode + ", underlying record=" + base.ToString() + "]";
			  }
		 }

		 /// <summary>
		 /// Implementation detail which provides the raw non-encoded outgoing relationship id
		 /// </summary>
		 internal virtual long OutgoingRawId()
		 {
			  return FirstOut;
		 }

		 /// <summary>
		 /// Implementation detail which provides the raw non-encoded incoming relationship id
		 /// </summary>
		 internal virtual long IncomingRawId()
		 {
			  return FirstIn;
		 }

		 /// <summary>
		 /// Implementation detail which provides the raw non-encoded loops relationship id
		 /// </summary>
		 internal virtual long LoopsRawId()
		 {
			  return FirstLoop;
		 }

		 private long EncodeRelationshipReference( long relationshipId )
		 {
			  Debug.Assert( relationshipId != NO_ID );
			  return Buffered ? encodeForFiltering( relationshipId ) : encodeForTxStateFiltering( relationshipId );
		 }

		 private bool Buffered
		 {
			 get
			 {
				  return _bufferedGroup != null;
			 }
		 }

		 public override void Close()
		 {
			  if ( _edgePage != null )
			  {
					_edgePage.close();
					_edgePage = null;
			  }

			  if ( _page != null )
			  {
					_page.close();
					_page = null;
			  }
		 }

		 public override long GroupReference()
		 {
			  return Id;
		 }

		 internal class BufferedGroup
		 {
			  internal readonly int Label;
			  internal readonly BufferedGroup Next;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal Record OutgoingConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal Record IncomingConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal Record LoopsConflict;
			  internal long FirstOut = NO_ID;
			  internal long FirstIn = NO_ID;
			  internal long FirstLoop = NO_ID;
			  internal int OutgoingCount;
			  internal int IncomingCount;
			  internal int LoopsCount;

			  internal BufferedGroup( RelationshipRecord edge, BufferedGroup next )
			  {
					this.Label = edge.Type;
					this.Next = next;
			  }

			  internal virtual void Outgoing( RelationshipRecord edge )
			  {
					if ( OutgoingConflict == null )
					{
						 FirstOut = edge.Id;
					}
					OutgoingConflict = new Record( edge, OutgoingConflict );
					OutgoingCount++;
			  }

			  internal virtual void Incoming( RelationshipRecord edge )
			  {
					if ( IncomingConflict == null )
					{
						 FirstIn = edge.Id;
					}
					IncomingConflict = new Record( edge, IncomingConflict );
					IncomingCount++;
			  }

			  internal virtual void Loop( RelationshipRecord edge )
			  {
					if ( LoopsConflict == null )
					{
						 FirstLoop = edge.Id;
					}
					LoopsConflict = new Record( edge, LoopsConflict );
					LoopsCount++;
			  }

			  internal virtual long Outgoing()
			  {
					return FirstOut;
			  }

			  internal virtual long Incoming()
			  {
					return FirstIn;
			  }

			  internal virtual long Loops()
			  {
					return FirstLoop;
			  }
		 }

		 private PageCursor GroupPage( long reference )
		 {
			  return _groupStore.openPageCursorForReading( reference );
		 }

		 private void Group( RelationshipGroupRecord record, long reference, PageCursor page )
		 {
			  // We need to load forcefully here since otherwise we cannot traverse over groups
			  // records which have been concurrently deleted (flagged as inUse = false).
			  // @see #org.neo4j.kernel.impl.store.RelationshipChainPointerChasingTest
			  _groupStore.getRecordByCursor( reference, record, RecordLoad.FORCE, page );
		 }
	}

}