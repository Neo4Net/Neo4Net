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
namespace Neo4Net.Kernel.Impl.Api.explicitindex
{
	using Neo4Net.GraphDb;
	using Neo4Net.GraphDb.Index;
	using Iterators = Neo4Net.Collections.Helpers.Iterators;
	using Neo4Net.Collections.Helpers;

	public abstract class AbstractIndexHits<T> : PrefetchingIterator<T>, IndexHits<T>
	{
		public abstract float CurrentScore();
		public abstract java.util.stream.Stream<T> Stream();
		public abstract int Size();
		 public override IResourceIterator<T> Iterator()
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