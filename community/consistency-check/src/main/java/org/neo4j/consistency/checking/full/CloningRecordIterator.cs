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
namespace Org.Neo4j.Consistency.checking.full
{

	using Resource = Org.Neo4j.Graphdb.Resource;
	using Org.Neo4j.Helpers.Collection;
	using AbstractBaseRecord = Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord;

	public class CloningRecordIterator<R> : PrefetchingResourceIterator<R> where R : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
	{
		 private readonly IEnumerator<R> _actualIterator;

		 public CloningRecordIterator( IEnumerator<R> actualIterator )
		 {
			  this._actualIterator = actualIterator;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") protected R fetchNextOrNull()
		 protected internal override R FetchNextOrNull()
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  return _actualIterator.hasNext() ? (R) _actualIterator.next().clone() : default(R);
		 }

		 public override void Close()
		 {
			  if ( _actualIterator is Resource )
			  {
					( ( Resource )_actualIterator ).close();
			  }
		 }

		 public static IEnumerator<R> Cloned<R>( IEnumerator<R> iterator ) where R : Org.Neo4j.Kernel.impl.store.record.AbstractBaseRecord
		 {
			  return new CloningRecordIterator<R>( iterator );
		 }
	}

}