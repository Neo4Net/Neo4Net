﻿using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{

	using Org.Neo4j.Cursor;
	using Org.Neo4j.Index.@internal.gbptree;

	public class NativeDistinctValuesProgressor<KEY, VALUE> : NativeIndexProgressor<KEY, VALUE> where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
	{
		 private readonly IndexLayout<KEY, VALUE> _layout;
		 private readonly KEY _prev;
		 private readonly IComparer<KEY> _comparator;
		 private bool _first = true;
		 private long _countForCurrentValue;
		 private bool _last;

		 internal NativeDistinctValuesProgressor( RawCursor<Hit<KEY, VALUE>, IOException> seeker, Org.Neo4j.Storageengine.Api.schema.IndexProgressor_NodeValueClient client, ICollection<RawCursor<Hit<KEY, VALUE>, IOException>> toRemoveFromOnClose, IndexLayout<KEY, VALUE> layout, IComparer<KEY> comparator ) : base( seeker, client, toRemoveFromOnClose )
		 {
			  this._layout = layout;
			  _prev = layout.newKey();
			  this._comparator = comparator;
		 }

		 public override bool Next()
		 {
			  try
			  {
					while ( seeker.next() )
					{
						 KEY key = seeker.get().key();
						 if ( _first )
						 {
							  _first = false;
							  _countForCurrentValue = 1;
							  _layout.copyKey( key, _prev );
						 }
						 else if ( _comparator.Compare( _prev, key ) == 0 )
						 {
							  // same as previous
							  _countForCurrentValue++;
						 }
						 else
						 {
							  // different from previous
							  bool accepted = client.acceptNode( _countForCurrentValue, extractValues( _prev ) );
							  _countForCurrentValue = 1;
							  _layout.copyKey( key, _prev );
							  if ( accepted )
							  {
									return true;
							  }
						 }
					}
					bool finalResult = !_first && !_last && client.acceptNode( _countForCurrentValue, extractValues( _prev ) );
					_last = true;
					return finalResult;
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }
	}

}