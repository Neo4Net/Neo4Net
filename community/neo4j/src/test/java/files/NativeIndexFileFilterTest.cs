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
namespace Files
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using IndexProviderDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.IndexProviderDescriptor;
	using LuceneIndexProviderFactory = Org.Neo4j.Kernel.Api.Impl.Schema.LuceneIndexProviderFactory;
	using NumberIndexProvider = Org.Neo4j.Kernel.Impl.Index.Schema.NumberIndexProvider;
	using SpatialIndexProvider = Org.Neo4j.Kernel.Impl.Index.Schema.SpatialIndexProvider;
	using StringIndexProvider = Org.Neo4j.Kernel.Impl.Index.Schema.StringIndexProvider;
	using TemporalIndexProvider = Org.Neo4j.Kernel.Impl.Index.Schema.TemporalIndexProvider;
	using NativeIndexFileFilter = Org.Neo4j.Kernel.@internal.NativeIndexFileFilter;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.schema.NativeLuceneFusionIndexProviderFactory.subProviderDirectoryStructure;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexDirectoryStructure.directoriesByProviderKey;

	public class NativeIndexFileFilterTest
	{
		 private static readonly IndexProviderDescriptor _luceneDescrtiptor = LuceneIndexProviderFactory.PROVIDER_DESCRIPTOR;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.fs.DefaultFileSystemRule fs = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public DefaultFileSystemRule Fs = new DefaultFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.TestDirectory directory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public TestDirectory Directory = TestDirectory.testDirectory();

		 private File _storeDir;
		 private NativeIndexFileFilter _filter;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  _storeDir = Directory.directory();
			  _filter = new NativeIndexFileFilter( _storeDir );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAcceptFileFromPureLuceneProvider() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAcceptFileFromPureLuceneProvider()
		 {
			  // given
			  File dir = directoriesByProviderKey( _storeDir ).forProvider( _luceneDescrtiptor ).directoryForIndex( 1 );
			  File file = new File( dir, "some-file" );
			  CreateFile( file );

			  // when
			  bool accepted = _filter.accept( file );

			  // then
			  assertFalse( accepted );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAcceptLuceneFileFromFusionProvider() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAcceptLuceneFileFromFusionProvider()
		 {
			  // given
			  File dir = subProviderDirectoryStructure( _storeDir, _luceneDescrtiptor ).forProvider( _luceneDescrtiptor ).directoryForIndex( 1 );
			  File file = new File( dir, "some-file" );
			  CreateFile( file );

			  // when
			  bool accepted = _filter.accept( file );

			  // then
			  assertFalse( accepted );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptNativeStringIndexFileFromFusionProvider() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcceptNativeStringIndexFileFromFusionProvider()
		 {
			  ShouldAcceptNativeIndexFileFromFusionProvider( new IndexProviderDescriptor( StringIndexProvider.KEY, "some-version" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptNativeNumberIndexFileFromFusionProvider() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcceptNativeNumberIndexFileFromFusionProvider()
		 {
			  ShouldAcceptNativeIndexFileFromFusionProvider( new IndexProviderDescriptor( NumberIndexProvider.KEY, "some-version" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptNativeSpatialIndexFileFromFusionProvider() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcceptNativeSpatialIndexFileFromFusionProvider()
		 {
			  ShouldAcceptNativeIndexFileFromFusionProvider( new IndexProviderDescriptor( SpatialIndexProvider.KEY, "some-version" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAcceptNativeTemporalIndexFileFromFusionProvider() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAcceptNativeTemporalIndexFileFromFusionProvider()
		 {
			  ShouldAcceptNativeIndexFileFromFusionProvider( new IndexProviderDescriptor( TemporalIndexProvider.KEY, "some-version" ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void shouldAcceptNativeIndexFileFromFusionProvider(org.neo4j.internal.kernel.api.schema.IndexProviderDescriptor descriptor) throws java.io.IOException
		 private void ShouldAcceptNativeIndexFileFromFusionProvider( IndexProviderDescriptor descriptor )
		 {
			  // given
			  File dir = subProviderDirectoryStructure( _storeDir, descriptor ).forProvider( descriptor ).directoryForIndex( 1 );
			  File file = new File( dir, "some-file" );
			  CreateFile( file );

			  // when
			  bool accepted = _filter.accept( file );

			  // then
			  assertTrue( accepted );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void createFile(java.io.File file) throws java.io.IOException
		 private void CreateFile( File file )
		 {
			  Fs.mkdirs( file.ParentFile );
			  Fs.create( file ).close();
		 }
	}

}