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
namespace Neo4Net.Helpers.Collections
{

	using Neo4Net.Graphdb;

	public class CombiningResourceIterator<T> : CombiningIterator<T>, ResourceIterator<T>
	{
		 private readonly IEnumerator<ResourceIterator<T>> _iterators;
		 private readonly ICollection<ResourceIterator<T>> _seenIterators = new List<ResourceIterator<T>>();
		 private ResourceIterator<T> _currentIterator;

		 public CombiningResourceIterator( IEnumerator<ResourceIterator<T>> iterators ) : base( iterators )
		 {
			  this._iterators = iterators;
		 }

		 protected internal override IEnumerator<T> NextIteratorOrNull()
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  if ( _iterators.hasNext() )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					_currentIterator = _iterators.next();
					_seenIterators.Add( _currentIterator );
					return _currentIterator;
			  }
			  return null;
		 }

		 public override void Close()
		 {
			  foreach ( ResourceIterator<T> seenIterator in _seenIterators )
			  {
					seenIterator.Close();
			  }

			  while ( _iterators.MoveNext() )
			  {
					_iterators.Current.close();
			  }
		 }
	}

}