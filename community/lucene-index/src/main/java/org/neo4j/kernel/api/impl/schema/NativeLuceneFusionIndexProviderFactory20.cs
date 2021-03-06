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
namespace Org.Neo4j.Kernel.Api.Impl.Schema
{

	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Service = Org.Neo4j.Helpers.Service;
	using RecoveryCleanupWorkCollector = Org.Neo4j.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using IndexProviderDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using IndexDirectoryStructure = Org.Neo4j.Kernel.Api.Index.IndexDirectoryStructure;
	using IndexProvider = Org.Neo4j.Kernel.Api.Index.IndexProvider;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Org.Neo4j.Kernel.extension;
	using OperationalMode = Org.Neo4j.Kernel.impl.factory.OperationalMode;
	using Org.Neo4j.Kernel.Impl.Index.Schema;
	using NumberIndexProvider = Org.Neo4j.Kernel.Impl.Index.Schema.NumberIndexProvider;
	using SpatialIndexProvider = Org.Neo4j.Kernel.Impl.Index.Schema.SpatialIndexProvider;
	using StringIndexProvider = Org.Neo4j.Kernel.Impl.Index.Schema.StringIndexProvider;
	using TemporalIndexProvider = Org.Neo4j.Kernel.Impl.Index.Schema.TemporalIndexProvider;
	using FusionIndexProvider = Org.Neo4j.Kernel.Impl.Index.Schema.fusion.FusionIndexProvider;
	using FusionSlotSelector20 = Org.Neo4j.Kernel.Impl.Index.Schema.fusion.FusionSlotSelector20;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.SchemaIndex.NATIVE20;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexDirectoryStructure.directoriesByProvider;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(KernelExtensionFactory.class) public class NativeLuceneFusionIndexProviderFactory20 extends NativeLuceneFusionIndexProviderFactory<NativeLuceneFusionIndexProviderFactory20.Dependencies>
	public class NativeLuceneFusionIndexProviderFactory20 : NativeLuceneFusionIndexProviderFactory<NativeLuceneFusionIndexProviderFactory20.Dependencies>
	{
		 public static readonly string Key = NATIVE20.providerKey();
		 public static readonly IndexProviderDescriptor Descriptor = new IndexProviderDescriptor( Key, NATIVE20.providerVersion() );

		 public NativeLuceneFusionIndexProviderFactory20() : base(Key)
		 {
		 }

		 protected internal override string DescriptorString()
		 {
			  return Descriptor.ToString();
		 }

		 protected internal override IndexProvider InternalCreate( PageCache pageCache, File storeDir, FileSystemAbstraction fs, IndexProvider.Monitor monitor, Config config, OperationalMode operationalMode, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector )
		 {
			  return Create( pageCache, storeDir, fs, monitor, config, operationalMode, recoveryCleanupWorkCollector );
		 }

		 public static FusionIndexProvider Create( PageCache pageCache, File databaseDirectory, FileSystemAbstraction fs, IndexProvider.Monitor monitor, Config config, OperationalMode operationalMode, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector )
		 {
			  IndexDirectoryStructure.Factory childDirectoryStructure = SubProviderDirectoryStructure( databaseDirectory );
			  bool readOnly = IndexProviderFactoryUtil.IsReadOnly( config, operationalMode );
			  bool archiveFailedIndex = config.Get( GraphDatabaseSettings.archive_failed_index );

			  StringIndexProvider @string = IndexProviderFactoryUtil.StringProvider( pageCache, fs, childDirectoryStructure, monitor, recoveryCleanupWorkCollector, readOnly );
			  NumberIndexProvider number = IndexProviderFactoryUtil.NumberProvider( pageCache, fs, childDirectoryStructure, monitor, recoveryCleanupWorkCollector, readOnly );
			  SpatialIndexProvider spatial = IndexProviderFactoryUtil.SpatialProvider( pageCache, fs, childDirectoryStructure, monitor, recoveryCleanupWorkCollector, readOnly, config );
			  TemporalIndexProvider temporal = IndexProviderFactoryUtil.TemporalProvider( pageCache, fs, childDirectoryStructure, monitor, recoveryCleanupWorkCollector, readOnly );
			  LuceneIndexProvider lucene = IndexProviderFactoryUtil.LuceneProvider( fs, childDirectoryStructure, monitor, config, operationalMode );

			  return new FusionIndexProvider( @string, number, spatial, temporal, lucene, new FusionSlotSelector20(), Descriptor, directoriesByProvider(databaseDirectory), fs, archiveFailedIndex );
		 }

		 public static IndexDirectoryStructure.Factory SubProviderDirectoryStructure( File databaseDirectory )
		 {
			  return NativeLuceneFusionIndexProviderFactory.SubProviderDirectoryStructure( databaseDirectory, Descriptor );
		 }

		 public interface Dependencies : AbstractIndexProviderFactory.Dependencies
		 {
		 }
	}

}