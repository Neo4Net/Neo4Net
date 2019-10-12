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
namespace Neo4Net.Index.impl.lucene.@explicit
{
	using Neo4Net.Kernel.Impl.Api.explicitindex;

	/// <summary>
	/// Empty implementation of <seealso cref="AbstractIndexHits"/> with no matches and <seealso cref="Float.NaN"/> as a score </summary>
	/// @param <T> index hits type </param>
	public class EmptyIndexHits<T> : AbstractIndexHits<T>
	{
		 public override int Size()
		 {
			  return 0;
		 }

		 public override float CurrentScore()
		 {
			  return Float.NaN;
		 }

		 protected internal override T FetchNextOrNull()
		 {
			  return default( T );
		 }
	}

}