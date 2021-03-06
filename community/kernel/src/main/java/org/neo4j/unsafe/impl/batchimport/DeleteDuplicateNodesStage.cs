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
namespace Org.Neo4j.@unsafe.Impl.Batchimport
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;

	using ProgressListener = Org.Neo4j.Helpers.progress.ProgressListener;
	using IdMapper = Org.Neo4j.@unsafe.Impl.Batchimport.cache.idmapping.IdMapper;
	using Collector = Org.Neo4j.@unsafe.Impl.Batchimport.input.Collector;
	using Stage = Org.Neo4j.@unsafe.Impl.Batchimport.staging.Stage;
	using BatchingNeoStores = Org.Neo4j.@unsafe.Impl.Batchimport.store.BatchingNeoStores;

	/// <summary>
	/// After <seealso cref="IdMapper.prepare(LongFunction, Collector, ProgressListener)"/> any duplicate input ids have been
	/// detected, i.e. also duplicate imported nodes. This stage makes one pass over those duplicate node ids
	/// and deletes from from the store(s).
	/// </summary>
	public class DeleteDuplicateNodesStage : Stage
	{
		 public DeleteDuplicateNodesStage( Configuration config, LongIterator duplicateNodeIds, BatchingNeoStores neoStore, DataImporter.Monitor storeMonitor ) : base( "DEDUP", null, config, 0 )
		 {
			  Add( new DeleteDuplicateNodesStep( Control(), config, duplicateNodeIds, neoStore.NodeStore, neoStore.PropertyStore, storeMonitor ) );
		 }
	}

}