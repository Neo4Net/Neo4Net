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

	internal class ListBasedBlockEntryCursor<KEY, VALUE> : BlockEntryCursor<KEY, VALUE>
	{
		 private readonly IEnumerator<BlockEntry<KEY, VALUE>> _entries;
		 private BlockEntry<KEY, VALUE> _next;

		 internal ListBasedBlockEntryCursor( IEnumerable<BlockEntry<KEY, VALUE>> entries )
		 {
			  this._entries = entries.GetEnumerator();
		 }

		 public override bool Next()
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  if ( _entries.hasNext() )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					_next = _entries.next();
					return true;
			  }
			  return false;
		 }

		 public override KEY Key()
		 {
			  return _next.key();
		 }

		 public override VALUE Value()
		 {
			  return _next.value();
		 }

		 public override void Close()
		 { // no-op
		 }
	}

}