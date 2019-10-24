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
namespace Neo4Net.Kernel.Api.Index
{
	using Test = org.junit.Test;

	using IndexProviderDescriptor = Neo4Net.Kernel.Api.Internal.Schema.IndexProviderDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.fs.FileUtils.path;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.index.IndexDirectoryStructure.baseSchemaIndexFolder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.index.IndexDirectoryStructure.directoriesByProvider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.index.IndexDirectoryStructure.directoriesByProviderKey;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.index.IndexDirectoryStructure.directoriesBySubProvider;

	public class IndexDirectoryStructureTest
	{
		private bool InstanceFieldsInitialized = false;

		public IndexDirectoryStructureTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_baseIndexDirectory = baseSchemaIndexFolder( _databaseStoreDir );
		}

		 private readonly IndexProviderDescriptor _provider = new IndexProviderDescriptor( "test", "0.5" );
		 private readonly File _databaseStoreDir = new File( "db" ).AbsoluteFile;
		 private File _baseIndexDirectory;
		 private readonly long _indexId = 15;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeCorrectDirectoriesForProviderKey()
		 public virtual void ShouldSeeCorrectDirectoriesForProviderKey()
		 {
			  AssertCorrectDirectories( directoriesByProviderKey( _databaseStoreDir ).forProvider( _provider ), path( _baseIndexDirectory, _provider.Key ), path( _baseIndexDirectory, _provider.Key, _indexId.ToString() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeCorrectDirectoriesForProvider()
		 public virtual void ShouldSeeCorrectDirectoriesForProvider()
		 {
			  AssertCorrectDirectories( directoriesByProvider( _databaseStoreDir ).forProvider( _provider ), path( _baseIndexDirectory, _provider.Key + "-" + _provider.Version ), path( _baseIndexDirectory, _provider.Key + "-" + _provider.Version, _indexId.ToString() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSeeCorrectDirectoriesForSubProvider()
		 public virtual void ShouldSeeCorrectDirectoriesForSubProvider()
		 {
			  IndexDirectoryStructure parentStructure = directoriesByProvider( _databaseStoreDir ).forProvider( _provider );
			  IndexProviderDescriptor subProvider = new IndexProviderDescriptor( "sub", "0.3" );
			  AssertCorrectDirectories( directoriesBySubProvider( parentStructure ).forProvider( subProvider ), path( _baseIndexDirectory, _provider.Key + "-" + _provider.Version ), path( _baseIndexDirectory, _provider.Key + "-" + _provider.Version, _indexId.ToString(), subProvider.Key + "-" + subProvider.Version ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleWeirdCharactersInProviderKey()
		 public virtual void ShouldHandleWeirdCharactersInProviderKey()
		 {
			  IndexProviderDescriptor providerWithWeirdName = new IndexProviderDescriptor( "native+lucene", "1.0" );
			  AssertCorrectDirectories( directoriesByProvider( _databaseStoreDir ).forProvider( providerWithWeirdName ), path( _baseIndexDirectory, "native_lucene-1.0" ), path( _baseIndexDirectory, "native_lucene-1.0", _indexId.ToString() ) );
		 }

		 private void AssertCorrectDirectories( IndexDirectoryStructure directoryStructure, File expectedRootDirectory, File expectedIndexDirectory )
		 {
			  // when
			  File rootDirectory = directoryStructure.RootDirectory();
			  File indexDirectory = directoryStructure.DirectoryForIndex( _indexId );

			  // then
			  assertEquals( expectedRootDirectory, rootDirectory );
			  assertEquals( expectedIndexDirectory, indexDirectory );
		 }
	}

}