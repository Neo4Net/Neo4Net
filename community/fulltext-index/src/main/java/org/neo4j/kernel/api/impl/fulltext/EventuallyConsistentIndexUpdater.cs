﻿/*
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

	internal class EventuallyConsistentIndexUpdater : IndexUpdater
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.neo4j.kernel.api.impl.index.DatabaseIndex<? extends org.neo4j.storageengine.api.schema.IndexReader> index;
		 private readonly DatabaseIndex<IndexReader> _index;
		 private readonly IndexUpdater _indexUpdater;
		 private readonly IndexUpdateSink _indexUpdateSink;

		 internal EventuallyConsistentIndexUpdater<T1>( DatabaseIndex<T1> index, IndexUpdater indexUpdater, IndexUpdateSink indexUpdateSink ) where T1 : Org.Neo4j.Storageengine.Api.schema.IndexReader
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