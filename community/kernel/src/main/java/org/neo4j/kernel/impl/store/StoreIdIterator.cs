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
namespace Org.Neo4j.Kernel.impl.store
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;


	public class StoreIdIterator : LongIterator
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final RecordStore<?> store;
		 private readonly RecordStore<object> _store;
		 private long _targetId;
		 private long _id;
		 private readonly bool _forward;

		 public StoreIdIterator<T1>( RecordStore<T1> store ) : this( store, true )
		 {
		 }

		 public StoreIdIterator<T1>( RecordStore<T1> store, bool forward ) : this( store, forward, forward ? store.NumberOfReservedLowIds : store.HighestPossibleIdInUse )
		 {
		 }

		 public StoreIdIterator<T1>( RecordStore<T1> store, bool forward, long initialId )
		 {
			  this._store = store;
			  this._id = initialId;
			  this._forward = forward;
		 }

		 public override string ToString()
		 {
			  return format( "%s[id=%s/%s; store=%s]", this.GetType().Name, _id, _targetId, _store );
		 }

		 public override bool HasNext()
		 {
			  if ( _forward )
			  {
					if ( _id < _targetId )
					{
						 return true;
					}
					_targetId = _store.HighId;
					return _id < _targetId;
			  }

			  return _id > 0;
		 }

		 public override long Next()
		 {
			  if ( !HasNext() )
			  {
					throw new NoSuchElementException( _forward ? format( "ID [%s] has exceeded the high ID [%s] of %s.", _id, _targetId, _store ) : format( "ID [%s] has exceeded the low ID [%s] of %s.", _id, _targetId, _store ) );
			  }
			  try
			  {
					return _id;
			  }
			  finally
			  {
					_id += _forward ? 1 : -1;
			  }
		 }
	}

}