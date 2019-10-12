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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using SpaceFillingCurveConfiguration = Org.Neo4j.Gis.Spatial.Index.curves.SpaceFillingCurveConfiguration;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using IndexDirectoryStructure = Org.Neo4j.Kernel.Api.Index.IndexDirectoryStructure;
	using SchemaDescriptorFactory = Org.Neo4j.Kernel.api.schema.SchemaDescriptorFactory;
	using IndexSpecificSpaceFillingCurveSettingsCache = Org.Neo4j.Kernel.Impl.Index.Schema.config.IndexSpecificSpaceFillingCurveSettingsCache;
	using IndexDescriptorFactory = Org.Neo4j.Storageengine.Api.schema.IndexDescriptorFactory;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;
	using PageCacheAndDependenciesRule = Org.Neo4j.Test.rule.PageCacheAndDependenciesRule;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.RecoveryCleanupWorkCollector.immediate;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexProvider.Monitor_Fields.EMPTY;

	public class GenericNativeIndexAccessorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.PageCacheAndDependenciesRule storage = new org.neo4j.test.rule.PageCacheAndDependenciesRule().with(new org.neo4j.test.rule.fs.DefaultFileSystemRule());
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
			  File indexFile = new File( indexDirectory, "my-index" );
			  StoreIndexDescriptor descriptor = IndexDescriptorFactory.forSchema( SchemaDescriptorFactory.forLabel( 1, 1 ) ).withId( indexId );
			  IndexSpecificSpaceFillingCurveSettingsCache spatialSettings = mock( typeof( IndexSpecificSpaceFillingCurveSettingsCache ) );
			  FileSystemAbstraction fs = Storage.fileSystem();
			  IndexDropAction dropAction = new FileSystemIndexDropAction( fs, directoryStructure );
			  GenericNativeIndexAccessor accessor = new GenericNativeIndexAccessor( Storage.pageCache(), fs, indexFile, new GenericLayout(1, spatialSettings), immediate(), EMPTY, descriptor, spatialSettings, mock(typeof(SpaceFillingCurveConfiguration)), dropAction, false );

			  // when
			  accessor.Drop();

			  // then
			  assertFalse( fs.FileExists( indexDirectory ) );
		 }
	}

}