using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Impl.Index.Schema
{

	using Neo4Net.Index.Internal.gbptree;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.IOUtils.closeAll;

	/// <summary>
	/// Take multiple <seealso cref="BlockEntryCursor"/> that each by themselves provide block entries in sorted order and lazily merge join, providing a view over all
	/// entries from given cursors in sorted order.
	/// Merging is done by keeping the cursors in an array amd pick the next lowest among them until all are exhausted, comparing
	/// <seealso cref="BlockEntryCursor.key()"/> (current key on each cursor).
	/// Instances handed out from <seealso cref="key()"/> and <seealso cref="value()"/> are reused, consumer is responsible for creating copy if there is a need to cache results.
	/// </summary>
	public class MergingBlockEntryReader<KEY, VALUE> : BlockEntryCursor<KEY, VALUE>
	{
		 // Means that a cursor needs to be advanced, i.e. its current head has already been used, or that it has no head yet
		 private const sbyte STATE_NEED_ADVANCE = 0;
		 // Means that a cursor has been advanced and its current key() contains its current head
		 private const sbyte STATE_HAS = 1;
		 // Means that a cursor has been exhausted and has no more entries in it
		 private const sbyte STATE_EXHAUSTED = 2;

		 private readonly Layout<KEY, VALUE> _layout;
		 private IList<Source> _sources = new List<Source>();
		 private Source _lastReturned;

		 internal MergingBlockEntryReader( Layout<KEY, VALUE> layout )
		 {
			  this._layout = layout;
		 }

		 internal virtual void AddSource( BlockEntryCursor<KEY, VALUE> source )
		 {
			  _sources.Add( new Source( this, source ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next() throws java.io.IOException
		 public override bool Next()
		 {
			  // Figure out lowest among cursor heads
			  KEY lowest = default( KEY );
			  Source lowestSource = null;
			  foreach ( Source source in _sources )
			  {
					KEY candidate = source.TryNext();
					if ( candidate != default( KEY ) && ( lowest == default( KEY ) || _layout.Compare( candidate, lowest ) < 0 ) )
					{
						 lowest = candidate;
						 lowestSource = source;
					}
			  }

			  // Make state transitions so that this entry is now considered used
			  if ( lowest != default( KEY ) )
			  {
					_lastReturned = lowestSource.TakeHead();
					return true;
			  }
			  return false;
		 }

		 public override KEY Key()
		 {
			  return _lastReturned.cursor.key();
		 }

		 public override VALUE Value()
		 {
			  return _lastReturned.cursor.value();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  closeAll( _sources );
		 }

		 private class Source : System.IDisposable
		 {
			 private readonly MergingBlockEntryReader<KEY, VALUE> _outerInstance;

			  internal readonly BlockEntryCursor<KEY, VALUE> Cursor;
			  internal sbyte State;

			  internal Source( MergingBlockEntryReader<KEY, VALUE> outerInstance, BlockEntryCursor<KEY, VALUE> cursor )
			  {
				  this._outerInstance = outerInstance;
					this.Cursor = cursor;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: KEY tryNext() throws java.io.IOException
			  internal virtual KEY TryNext()
			  {
					if ( State == STATE_NEED_ADVANCE )
					{
						 if ( Cursor.next() )
						 {
							  State = STATE_HAS;
							  return Cursor.key();
						 }
						 else
						 {
							  State = STATE_EXHAUSTED;
						 }
					}
					else if ( State == STATE_HAS )
					{
						 return Cursor.key();
					}
					return default( KEY );
			  }

			  internal virtual Source TakeHead()
			  {
					State = STATE_NEED_ADVANCE;
					return this;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
			  public override void Close()
			  {
					Cursor.Dispose();
			  }
		 }
	}

}