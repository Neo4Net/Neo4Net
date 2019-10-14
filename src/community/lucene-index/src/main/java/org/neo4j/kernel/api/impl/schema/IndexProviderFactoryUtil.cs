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
namespace Neo4Net.Kernel.Api.Impl.Schema
{
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using RecoveryCleanupWorkCollector = Neo4Net.Index.Internal.gbptree.RecoveryCleanupWorkCollector;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using DirectoryFactory = Neo4Net.Kernel.Api.Impl.Index.storage.DirectoryFactory;
	using IndexDirectoryStructure = Neo4Net.Kernel.Api.Index.IndexDirectoryStructure;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using Config = Neo4Net.Kernel.configuration.Config;
	using OperationalMode = Neo4Net.Kernel.impl.factory.OperationalMode;
	using NumberIndexProvider = Neo4Net.Kernel.Impl.Index.Schema.NumberIndexProvider;
	using SpatialIndexProvider = Neo4Net.Kernel.Impl.Index.Schema.SpatialIndexProvider;
	using StringIndexProvider = Neo4Net.Kernel.Impl.Index.Schema.StringIndexProvider;
	using TemporalIndexProvider = Neo4Net.Kernel.Impl.Index.Schema.TemporalIndexProvider;

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