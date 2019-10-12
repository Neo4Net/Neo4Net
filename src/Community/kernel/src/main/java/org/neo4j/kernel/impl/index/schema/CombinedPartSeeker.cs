using System.Collections.Generic;
using System.Diagnostics;

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

	using Neo4Net.Cursors;
	using Neo4Net.Index.@internal.gbptree;
	using Neo4Net.Index.@internal.gbptree;
	using Neo4Net.Index.@internal.gbptree;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.IOUtils.closeAll;

	/// <summary>
	/// Combines multiple <seealso cref="GBPTree"/> seekers into one seeker, keeping the total order among all keys.
	/// </summary>
	/// @param <KEY> type of key </param>
	/// @param <VALUE> type of value </param>
	internal class CombinedPartSeeker<KEY, VALUE> : RawCursor<Hit<KEY, VALUE>, IOException>, Hit<KEY, VALUE>
	{
		 private readonly KEY _end;
		 private readonly RawCursor<Hit<KEY, VALUE>, IOException>[] _partCursors;
		 private readonly object[] _partHeads;
		 private readonly Layout<KEY, VALUE> _layout;
		 private KEY _nextKey;
		 private VALUE _nextValue;

		 internal CombinedPartSeeker( Layout<KEY, VALUE> layout, IList<RawCursor<Hit<KEY, VALUE>, IOException>> parts )
		 {
			  this._layout = layout;
			  int length = parts.Count;
			  this._end = layout.NewKey();
			  this._partCursors = parts.ToArray();
			  this._partHeads = new object[length];
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next() throws java.io.IOException
		 public override bool Next()
		 {
			  // Pick lowest among all candidates
			  int nextKeyIndex = -1;
			  for ( int i = 0; i < _partCursors.Length; i++ )
			  {
					// Get candidate from already seen heads, if any
					KEY candidate = ( KEY ) _partHeads[i];
					if ( candidate == _end )
					{
						 continue;
					}

					// Get candidate from seeker, if available
					if ( candidate == default( KEY ) )
					{
						 if ( _partCursors[i].next() )
						 {
							  _partHeads[i] = candidate = _partCursors[i].get().key();
						 }
						 else
						 {
							  _partHeads[i] = _end;
						 }
					}

					// Was our candidate lower than lowest we've seen so far this round?
					if ( candidate != default( KEY ) )
					{
						 if ( nextKeyIndex == -1 || _layout.Compare( candidate, _nextKey ) < 0 )
						 {
							  _nextKey = candidate;
							  nextKeyIndex = i;
						 }
					}
			  }

			  if ( nextKeyIndex != -1 )
			  {
					// We have a next key/value
					_nextValue = _partCursors[nextKeyIndex].get().value();
					_partHeads[nextKeyIndex] = null;
					return true;
			  }

			  // We've reached the end of all parts
			  _nextKey = default( KEY );
			  _nextValue = default( VALUE );
			  return false;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  closeAll( _partCursors );
		 }

		 public override Hit<KEY, VALUE> Get()
		 {
			  return this;
		 }

		 public override KEY Key()
		 {
			  Debug.Assert( _nextKey != default( KEY ) );
			  return _nextKey;
		 }

		 public override VALUE Value()
		 {
			  Debug.Assert( _nextValue != default( VALUE ) );
			  return _nextValue;
		 }
	}

}