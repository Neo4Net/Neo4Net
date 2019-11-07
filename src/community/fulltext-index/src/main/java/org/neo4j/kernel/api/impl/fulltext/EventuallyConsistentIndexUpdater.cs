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
namespace Neo4Net.Kernel.Api.Impl.Fulltext
{
	using Neo4Net.Kernel.Api.Impl.Index;
	using Neo4Net.Kernel.Api.Index;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using IndexReader = Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader;

	internal class EventuallyConsistentIndexUpdater : IndexUpdater
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final Neo4Net.kernel.api.impl.index.DatabaseIndex<? extends Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader> index;
		 private readonly DatabaseIndex<IndexReader> _index;
		 private readonly IndexUpdater _indexUpdater;
		 private readonly IndexUpdateSink _indexUpdateSink;

		 internal EventuallyConsistentIndexUpdater<T1>( DatabaseIndex<T1> index, IndexUpdater indexUpdater, IndexUpdateSink indexUpdateSink ) where T1 : Neo4Net.Kernel.Api.StorageEngine.schema.IndexReader
		 {
			  this._index = index;
			  this._indexUpdater = indexUpdater;
			  this._indexUpdateSink = indexUpdateSink;
		 }

		 public override void Process<T1>( IndexEntryUpdate<T1> update )
		 {
			  _indexUpdateSink.enqueueUpdate( _index, _indexUpdater, update );
		 }

		 public override void Close()
		 {
			  _indexUpdateSink.closeUpdater( _index, _indexUpdater );
		 }
	}

}