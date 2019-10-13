﻿/*
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
namespace Neo4Net.Kernel.impl.store.counts
{
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Neo4Net.Kernel.configuration.Config;
	using State = Neo4Net.Kernel.impl.store.kvstore.State;
	using LogProvider = Neo4Net.Logging.LogProvider;

	[State(State.Strategy.READ_ONLY_CONCURRENT_HASH_MAP)]
	public class ReadOnlyCountsTracker : CountsTracker
	{
		 public ReadOnlyCountsTracker( LogProvider logProvider, FileSystemAbstraction fileSystem, PageCache pageCache, Config config, DatabaseLayout databaseLayout ) : base( logProvider, fileSystem, pageCache, config, databaseLayout, EmptyVersionContextSupplier.EMPTY )
		 {
		 }

		 public override long Rotate( long txId )
		 {
			  return -1;
		 }
	}

}