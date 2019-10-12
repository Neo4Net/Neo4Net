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
namespace Org.Neo4j.Kernel.Impl.Api.explicitindex
{
	using Org.Neo4j.Graphdb;
	using Org.Neo4j.Graphdb.index;
	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;
	using Org.Neo4j.Helpers.Collection;

	public abstract class AbstractIndexHits<T> : PrefetchingIterator<T>, IndexHits<T>
	{
		public abstract float CurrentScore();
		public abstract java.util.stream.Stream<T> Stream();
		public abstract int Size();
		 public override ResourceIterator<T> Iterator()
		 {
			  return this;
		 }

		 public override void Close()
		 { // Nothing to close by default
		 }

		 public virtual T Single
		 {
			 get
			 {
				  // This instance will be closed by this call
				  return Iterators.singleOrNull( this );
			 }
		 }
	}

}