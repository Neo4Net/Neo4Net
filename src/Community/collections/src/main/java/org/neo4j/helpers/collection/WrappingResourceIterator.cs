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
namespace Neo4Net.Helpers.Collection
{

	internal class WrappingResourceIterator<T> : PrefetchingResourceIterator<T>
	{
		 private readonly IEnumerator<T> _iterator;

		 internal WrappingResourceIterator( IEnumerator<T> iterator )
		 {
			  this._iterator = iterator;
		 }

		 public override void Close()
		 {
		 }

		 public override void Remove()
		 {
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
			  _iterator.remove();
		 }

		 protected internal override T FetchNextOrNull()
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  return _iterator.hasNext() ? _iterator.next() : default(T);
		 }
	}

}