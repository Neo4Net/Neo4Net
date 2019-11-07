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
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using ImmutableEmptyLongIterator = org.eclipse.collections.impl.iterator.ImmutableEmptyLongIterator;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;

	using RelationshipScanCursor = Neo4Net.Kernel.Api.Internal.RelationshipScanCursor;
	using StorageRelationshipScanCursor = Neo4Net.Kernel.Api.StorageEngine.StorageRelationshipScanCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.record.AbstractBaseRecord.NO_ID;

	internal class DefaultRelationshipScanCursor : DefaultRelationshipCursor<StorageRelationshipScanCursor>, RelationshipScanCursor
	{
		 private int _type;
		 private long _single;
		 private LongIterator _addedRelationships;

		 internal DefaultRelationshipScanCursor( DefaultCursors pool, StorageRelationshipScanCursor storeCursor ) : base( pool, storeCursor )
		 {
		 }

		 internal virtual void Scan( int type, Read read )
		 {
			  StoreCursor.scan( type );
			  this._type = type;
			  this._single = NO_ID;
			  Init( read );
			  this._addedRelationships = ImmutableEmptyLongIterator.INSTANCE;
		 }

		 internal virtual void Single( long reference, Read read )
		 {
			  StoreCursor.single( reference );
			  _type = -1;
			  this._single = reference;
			  Init( read );
			  this._addedRelationships = ImmutableEmptyLongIterator.INSTANCE;
		 }

		 public override bool Next()
		 {
			  // Check tx state
			  bool hasChanges = hasChanges();

			  if ( hasChanges && _addedRelationships.hasNext() )
			  {
					Read.txState().relationshipVisit(_addedRelationships.next(), StoreCursor);
					return true;
			  }

			  while ( StoreCursor.next() )
			  {
					if ( !hasChanges || !Read.txState().relationshipIsDeletedInThisTx(StoreCursor.entityReference()) )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 public override void Close()
		 {
			  if ( !Closed )
			  {
					Read = null;
					StoreCursor.close();

					Pool.accept( this );
			  }
		 }

		 public virtual bool Closed
		 {
			 get
			 {
				  return Read == null;
			 }
		 }

		 public override string ToString()
		 {
			  if ( Closed )
			  {
					return "RelationshipScanCursor[closed state]";
			  }
			  else
			  {
					return "RelationshipScanCursor[id=" + StoreCursor.entityReference() +
							  ", open state with: single=" + _single +
							  ", type=" + _type +
							  ", " + StoreCursor.ToString() + "]";
			  }
		 }

		 protected internal override void CollectAddedTxStateSnapshot()
		 {
			  if ( Single )
			  {
					_addedRelationships = Read.txState().relationshipIsAddedInThisTx(_single) ? LongHashSet.newSetWith(_single).longIterator() : ImmutableEmptyLongIterator.INSTANCE;
			  }
			  else
			  {
					_addedRelationships = Read.txState().addedAndRemovedRelationships().Added.longIterator();
			  }
		 }

		 private bool Single
		 {
			 get
			 {
				  return _single != NO_ID;
			 }
		 }

		 public virtual void Release()
		 {
			  StoreCursor.close();
		 }
	}

}