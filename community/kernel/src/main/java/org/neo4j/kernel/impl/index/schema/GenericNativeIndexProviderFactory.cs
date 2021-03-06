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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{

	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Service = Org.Neo4j.Helpers.Service;
	using RecoveryCleanupWorkCollector = Org.Neo4j.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using IndexDirectoryStructure = Org.Neo4j.Kernel.Api.Index.IndexDirectoryStructure;
	using IndexProvider = Org.Neo4j.Kernel.Api.Index.IndexProvider;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Org.Neo4j.Kernel.extension;
	using OperationalMode = Org.Neo4j.Kernel.impl.factory.OperationalMode;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexDirectoryStructure.directoriesByProvider;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Service.Implementation(KernelExtensionFactory.class) public class GenericNativeIndexProviderFactory extends AbstractIndexProviderFactory<GenericNativeIndexProviderFactory.Dependencies>
	public class GenericNativeIndexProviderFactory : AbstractIndexProviderFactory<GenericNativeIndexProviderFactory.Dependencies>
	{
		 public GenericNativeIndexProviderFactory() : base(GenericNativeIndexProvider.Key)
		 {
		 }

		 protected internal override System.Type LoggingClass()
		 {
			  return typeof( GenericNativeIndexProvider );
		 }

		 protected internal override string DescriptorString()
		 {
			  return GenericNativeIndexProvider.Descriptor.ToString();
		 }

		 protected internal override GenericNativeIndexProvider InternalCreate( PageCache pageCache, File storeDir, FileSystemAbstraction fs, IndexProvider.Monitor monitor, Config config, OperationalMode operationalMode, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector )
		 {
			  return Create( pageCache, storeDir, fs, monitor, config, operationalMode, recoveryCleanupWorkCollector );
		 }

		 public static GenericNativeIndexProvider Create( PageCache pageCache, File storeDir, FileSystemAbstraction fs, IndexProvider.Monitor monitor, Config config, OperationalMode mode, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector )
		 {
			  IndexDirectoryStructure.Factory directoryStructure = directoriesByProvider( storeDir );
			  bool readOnly = config.Get( GraphDatabaseSettings.read_only ) && ( OperationalMode.single == mode );
			  return new GenericNativeIndexProvider( directoryStructure, pageCache, fs, monitor, recoveryCleanupWorkCollector, readOnly, config );
		 }

		 public interface Dependencies : AbstractIndexProviderFactory.Dependencies
		 {
		 }
	}

}