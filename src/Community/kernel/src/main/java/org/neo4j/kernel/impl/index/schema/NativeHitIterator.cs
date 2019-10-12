using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;


	using PrimitiveLongCollections = Neo4Net.Collection.PrimitiveLongCollections;
	using PrimitiveLongResourceIterator = Neo4Net.Collection.PrimitiveLongResourceIterator;
	using Neo4Net.Cursors;
	using Neo4Net.Index.@internal.gbptree;
	using Value = Neo4Net.Values.Storable.Value;

	/// <summary>
	/// Wraps number key/value results in a <seealso cref="LongIterator"/>.
	/// </summary>
	/// @param <KEY> type of <seealso cref="NumberIndexKey"/>. </param>
	/// @param <VALUE> type of <seealso cref="NativeIndexValue"/>. </param>
	public class NativeHitIterator<KEY, VALUE> : PrimitiveLongCollections.PrimitiveLongBaseIterator, PrimitiveLongResourceIterator where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
	{
		 private readonly RawCursor<Hit<KEY, VALUE>, IOException> _seeker;
		 private readonly ICollection<RawCursor<Hit<KEY, VALUE>, IOException>> _toRemoveFromWhenExhausted;
		 private bool _closed;

		 internal NativeHitIterator( RawCursor<Hit<KEY, VALUE>, IOException> seeker, ICollection<RawCursor<Hit<KEY, VALUE>, IOException>> toRemoveFromWhenExhausted )
		 {
			  this._seeker = seeker;
			  this._toRemoveFromWhenExhausted = toRemoveFromWhenExhausted;
		 }

		 protected internal override bool FetchNext()
		 {
			  try
			  {
					while ( _seeker.next() )
					{
						 KEY key = _seeker.get().key();
						 if ( AcceptValues( key.asValues() ) )
						 {
							  return next( key.EntityId );
						 }
					}
					return false;
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 internal virtual bool AcceptValues( Value[] value )
		 {
			  return true;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void ensureCursorClosed() throws java.io.IOException
		 private void EnsureCursorClosed()
		 {
			  if ( !_closed )
			  {
					_seeker.close();
					_toRemoveFromWhenExhausted.remove( _seeker );
					_closed = true;
			  }
		 }

		 public override void Close()
		 {
			  try
			  {
					EnsureCursorClosed();
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }
	}

}