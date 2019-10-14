using System;

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
	using Service = Neo4Net.Helpers.Service;
	using RecoveryCleanupWorkCollector = Neo4Net.Index.Internal.gbptree.RecoveryCleanupWorkCollector;
	using IndexProviderDescriptor = Neo4Net.Internal.Kernel.Api.schema.IndexProviderDescriptor;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using IndexDirectoryStructure = Neo4Net.Kernel.Api.Index.IndexDirectoryStructure;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Neo4Net.Kernel.extension;
	using OperationalMode = Neo4Net.Kernel.impl.factory.OperationalMode;
	using Neo4Net.Kernel.Impl.Index.Schema;
	using SpatialIndexProvider = Neo4Net.Kernel.Impl.Index.Schema.SpatialIndexProvider;
	using TemporalIndexProvider = Neo4Net.Kernel.Impl.Index.Schema.TemporalIndexProvider;
	using FusionIndexProvider = Neo4Net.Kernel.Impl.Index.Schema.fusion.FusionIndexProvider;
	using FusionSlotSelector00 = Neo4Net.Kernel.Impl.Index.Schema.fusion.FusionSlotSelector00;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.SchemaIndex.LUCENE10;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexDirectoryStructure.directoriesByProvider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexDirectoryStructure.directoriesByProviderKey;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexProvider.EMPTY;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(KernelExtensionFactory.class) public class LuceneIndexProviderFactory extends org.neo4j.kernel.impl.index.schema.AbstractIndexProviderFactory<LuceneIndexProviderFactory.Dependencies>
	public class LuceneIndexProviderFactory : AbstractIndexProviderFactory<LuceneIndexProviderFactory.Dependencies>
	{
		 private static readonly string _key = LUCENE10.providerKey();
		 public static readonly IndexProviderDescriptor ProviderDescriptor = new IndexProviderDescriptor( _key, LUCENE10.providerVersion() );

		 public LuceneIndexProviderFactory() : base(_key)
		 {
		 }

		 protected internal override Type LoggingClass()
		 {
			  return typeof( LuceneIndexProvider );
		 }

		 protected internal override string DescriptorString()
		 {
			  return ProviderDescriptor.ToString();
		 }

		 protected internal override IndexProvider InternalCreate( PageCache pageCache, File storeDir, FileSystemAbstraction fs, IndexProvider.Monitor monitor, Config config, OperationalMode operationalMode, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector )
		 {
			  return NewInstance( pageCache, storeDir, fs, monitor, config, operationalMode, recoveryCleanupWorkCollector );
		 }

		 public static FusionIndexProvider NewInstance( PageCache pageCache, File databaseDirectory, FileSystemAbstraction fs, IndexProvider.Monitor monitor, Config config, OperationalMode operationalMode, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector )
		 {
			  bool readOnly = IndexProviderFactoryUtil.IsReadOnly( config, operationalMode );
			  bool archiveFailedIndex = config.Get( GraphDatabaseSettings.archive_failed_index );
			  IndexDirectoryStructure.Factory luceneDirStructure = directoriesByProviderKey( databaseDirectory );
			  IndexDirectoryStructure.Factory childDirectoryStructure = SubProviderDirectoryStructure( databaseDirectory );

			  LuceneIndexProvider lucene = IndexProviderFactoryUtil.LuceneProvider( fs, luceneDirStructure, monitor, config, operationalMode );
			  TemporalIndexProvider temporal = IndexProviderFactoryUtil.TemporalProvider( pageCache, fs, childDirectoryStructure, monitor, recoveryCleanupWorkCollector, readOnly );
			  SpatialIndexProvider spatial = IndexProviderFactoryUtil.SpatialProvider( pageCache, fs, childDirectoryStructure, monitor, recoveryCleanupWorkCollector, readOnly, config );

			  return new FusionIndexProvider( EMPTY, EMPTY, spatial, temporal, lucene, new FusionSlotSelector00(), ProviderDescriptor, directoriesByProvider(databaseDirectory), fs, archiveFailedIndex );
		 }

		 private static IndexDirectoryStructure.Factory SubProviderDirectoryStructure( File databaseDirectory )
		 {
			  return NativeLuceneFusionIndexProviderFactory.SubProviderDirectoryStructure( databaseDirectory, ProviderDescriptor );
		 }

		 public interface Dependencies : AbstractIndexProviderFactory.Dependencies
		 {
		 }
	}

}