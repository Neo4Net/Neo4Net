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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using SpaceFillingCurveConfiguration = Neo4Net.Gis.Spatial.Index.curves.SpaceFillingCurveConfiguration;
	using RecoveryCleanupWorkCollector = Neo4Net.Index.Internal.gbptree.RecoveryCleanupWorkCollector;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using IndexDirectoryStructure = Neo4Net.Kernel.Api.Index.IndexDirectoryStructure;
	using SchemaDescriptorFactory = Neo4Net.Kernel.Api.schema.SchemaDescriptorFactory;
	using IndexSpecificSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.IndexSpecificSpaceFillingCurveSettingsCache;
	using IndexDescriptorFactory = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptorFactory;
	using StoreIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor;
	using PageCacheAndDependenciesRule = Neo4Net.Test.rule.PageCacheAndDependenciesRule;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.index.Internal.gbptree.RecoveryCleanupWorkCollector.immediate;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.api.index.IndexProvider.Monitor_Fields.EMPTY;

	public class GenericNativeIndexPopulatorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.PageCacheAndDependenciesRule storage = new Neo4Net.test.rule.PageCacheAndDependenciesRule().with(new Neo4Net.test.rule.fs.DefaultFileSystemRule());
		 public readonly PageCacheAndDependenciesRule Storage = new PageCacheAndDependenciesRule().with(new DefaultFileSystemRule());

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dropShouldDeleteEntireIndexFolder()
		 public virtual void DropShouldDeleteEntireIndexFolder()
		 {
			  // given
			  File root = Storage.directory().directory("root");
			  IndexDirectoryStructure directoryStructure = IndexDirectoryStructure.directoriesByProvider( root ).forProvider( GenericNativeIndexProvider.Descriptor );
			  long indexId = 8;
			  File indexDirectory = directoryStructure.DirectoryForIndex( indexId );
			  StoreIndexDescriptor descriptor = IndexDescriptorFactory.forSchema( SchemaDescriptorFactory.forLabel( 1, 1 ) ).withId( indexId );
			  IndexSpecificSpaceFillingCurveSettingsCache spatialSettings = mock( typeof( IndexSpecificSpaceFillingCurveSettingsCache ) );
			  PageCache pageCache = Storage.pageCache();
			  FileSystemAbstraction fs = Storage.fileSystem();
			  File indexFile = new File( indexDirectory, "my-index" );
			  GenericLayout layout = new GenericLayout( 1, spatialSettings );
			  RecoveryCleanupWorkCollector immediate = immediate();
			  IndexDropAction dropAction = new FileSystemIndexDropAction( fs, directoryStructure );
			  GenericNativeIndexPopulator populator = new GenericNativeIndexPopulator( pageCache, fs, indexFile, layout, EMPTY, descriptor, spatialSettings, directoryStructure, mock( typeof( SpaceFillingCurveConfiguration ) ), dropAction, false );
			  populator.Create();

			  // when
			  assertTrue( fs.ListFiles( indexDirectory ).Length > 0 );
			  populator.Drop();

			  // then
			  assertFalse( fs.FileExists( indexDirectory ) );
		 }
	}

}