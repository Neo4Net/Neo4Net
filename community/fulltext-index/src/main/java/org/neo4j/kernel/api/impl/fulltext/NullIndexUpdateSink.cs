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
namespace Org.Neo4j.Kernel.Api.Impl.Fulltext
{
	using Org.Neo4j.Kernel.Api.Impl.Index;
	using Org.Neo4j.Kernel.Api.Index;
	using IndexUpdater = Org.Neo4j.Kernel.Api.Index.IndexUpdater;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;

	/// <summary>
	/// An implementation of <seealso cref="IndexUpdateSink"/> that does not actually do anything.
	/// </summary>
	public class NullIndexUpdateSink : IndexUpdateSink
	{
		 public static readonly NullIndexUpdateSink Instance = new NullIndexUpdateSink();

		 private NullIndexUpdateSink() : base(null, 0)
		 {
		 }

		 public override void EnqueueUpdate<T1, T2>( DatabaseIndex<T1> index, IndexUpdater indexUpdater, IndexEntryUpdate<T2> update ) where T1 : Org.Neo4j.Storageengine.Api.schema.IndexReader
		 {
		 }

		 public override void CloseUpdater<T1>( DatabaseIndex<T1> index, IndexUpdater indexUpdater ) where T1 : Org.Neo4j.Storageengine.Api.schema.IndexReader
		 {
		 }

		 public override void AwaitUpdateApplication()
		 {
		 }
	}

}