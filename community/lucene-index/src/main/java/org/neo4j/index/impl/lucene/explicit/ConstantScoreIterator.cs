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
namespace Org.Neo4j.Index.impl.lucene.@explicit
{

	internal class ConstantScoreIterator : AbstractExplicitIndexHits
	{
		 private readonly IEnumerator<EntityId> _items;
		 private readonly int _size;
		 private readonly float _score;

		 internal ConstantScoreIterator( ICollection<EntityId> items, float score )
		 {
			  this._items = items.GetEnumerator();
			  this._score = score;
			  this._size = items.Count;
		 }

		 public override float CurrentScore()
		 {
			  return this._score;
		 }

		 public override int Size()
		 {
			  return this._size;
		 }

		 protected internal override bool FetchNext()
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  return _items.hasNext() && Next(_items.next().id());
		 }
	}

}