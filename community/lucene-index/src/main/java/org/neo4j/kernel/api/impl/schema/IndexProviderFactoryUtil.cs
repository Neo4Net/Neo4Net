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
namespace Org.Neo4j.Kernel.Api.Impl.Schema
{
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using RecoveryCleanupWorkCollector = Org.Neo4j.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using DirectoryFactory = Org.Neo4j.Kernel.Api.Impl.Index.storage.DirectoryFactory;
	using IndexDirectoryStructure = Org.Neo4j.Kernel.Api.Index.IndexDirectoryStructure;
	using IndexProvider = Org.Neo4j.Kernel.Api.Index.IndexProvider;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using OperationalMode = Org.Neo4j.Kernel.impl.factory.OperationalMode;
	using NumberIndexProvider = Org.Neo4j.Kernel.Impl.Index.Schema.NumberIndexProvider;
	using SpatialIndexProvider = Org.Neo4j.Kernel.Impl.Index.Schema.SpatialIndexProvider;
	using StringIndexProvider = Org.Neo4j.Kernel.Impl.Index.Schema.StringIndexProvider;
	using TemporalIndexProvider = Org.Neo4j.Kernel.Impl.Index.Schema.TemporalIndexProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.index.storage.DirectoryFactory.directoryFactory;

	internal class IndexProviderFactoryUtil
	{
		 internal static bool IsReadOnly( Config config, OperationalMode operationalMode )
		 {
			  return config.Get( GraphDatabaseSettings.read_only ) && ( OperationalMode.single == operationalMode );
		 }

		 internal static StringIndexProvider StringProvider( PageCache pageCache, FileSystemAbstraction fs, IndexDirectoryStructure.Factory childDirectoryStructure, IndexProvider.Monitor monitor, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, bool readOnly )
		 {
			  return new StringIndexProvider( pageCache, fs, childDirectoryStructure, monitor, recoveryCleanupWorkCollector, readOnly );
		 }

		 internal static NumberIndexProvider NumberProvider( PageCache pageCache, FileSystemAbstraction fs, IndexDirectoryStructure.Factory childDirectoryStructure, IndexProvider.Monitor monitor, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, bool readOnly )
		 {
			  return new NumberIndexProvider( pageCache, fs, childDirectoryStructure, monitor, recoveryCleanupWorkCollector, readOnly );
		 }

		 internal static SpatialIndexProvider SpatialProvider( PageCache pageCache, FileSystemAbstraction fs, IndexDirectoryStructure.Factory directoryStructure, IndexProvider.Monitor monitor, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, bool readOnly, Config config )
		 {
			  return new SpatialIndexProvider( pageCache, fs, directoryStructure, monitor, recoveryCleanupWorkCollector, readOnly, config );
		 }

		 internal static TemporalIndexProvider TemporalProvider( PageCache pageCache, FileSystemAbstraction fs, IndexDirectoryStructure.Factory directoryStructure, IndexProvider.Monitor monitor, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, bool readOnly )
		 {
			  return new TemporalIndexProvider( pageCache, fs, directoryStructure, monitor, recoveryCleanupWorkCollector, readOnly );
		 }

		 internal static LuceneIndexProvider LuceneProvider( FileSystemAbstraction fs, IndexDirectoryStructure.Factory directoryStructure, IndexProvider.Monitor monitor, Config config, OperationalMode operationalMode )
		 {
			  bool ephemeral = config.Get( GraphDatabaseSettings.ephemeral );
			  DirectoryFactory directoryFactory = directoryFactory( ephemeral );
			  return new LuceneIndexProvider( fs, directoryFactory, directoryStructure, monitor, config, operationalMode );
		 }
	}

}