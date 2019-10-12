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
namespace Org.Neo4j.Kernel.impl.store.counts
{
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Org.Neo4j.Io.layout.DatabaseLayout;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using EmptyVersionContextSupplier = Org.Neo4j.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using State = Org.Neo4j.Kernel.impl.store.kvstore.State;
	using LogProvider = Org.Neo4j.Logging.LogProvider;

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