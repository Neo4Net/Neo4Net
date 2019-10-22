using System;
using System.Collections;
using System.Collections.Generic;

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
namespace Neo4Net.Dbms.archive
{
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;
	using ParameterizedTest = org.junit.jupiter.@params.ParameterizedTest;
	using EnumSource = org.junit.jupiter.@params.provider.EnumSource;


	using Predicates = Neo4Net.Functions.Predicates;
	using TransactionLogFiles = Neo4Net.Kernel.impl.transaction.log.files.TransactionLogFiles;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Pair.pair;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class ArchiveTest
	internal class ArchiveTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.Neo4Net.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @EnumSource(CompressionFormat.class) void shouldRoundTripAnEmptyDirectory(CompressionFormat compressionFormat) throws java.io.IOException, IncorrectFormat
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRoundTripAnEmptyDirectory( CompressionFormat compressionFormat )
		 {
			  Path directory = _testDirectory.directory( "a-directory" ).toPath();
			  Files.createDirectories( directory );

			  AssertRoundTrips( directory, compressionFormat );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @EnumSource(CompressionFormat.class) void shouldRoundTripASingleFile(CompressionFormat compressionFormat) throws java.io.IOException, IncorrectFormat
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRoundTripASingleFile( CompressionFormat compressionFormat )
		 {
			  Path directory = _testDirectory.directory( "a-directory" ).toPath();
			  Files.createDirectories( directory );
			  Files.write( directory.resolve( "a-file" ), "text".GetBytes() );

			  AssertRoundTrips( directory, compressionFormat );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @EnumSource(CompressionFormat.class) void shouldRoundTripAnEmptyFile(CompressionFormat compressionFormat) throws java.io.IOException, IncorrectFormat
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRoundTripAnEmptyFile( CompressionFormat compressionFormat )
		 {
			  Path directory = _testDirectory.directory( "a-directory" ).toPath();
			  Files.createDirectories( directory );
			  Files.write( directory.resolve( "a-file" ), new sbyte[0] );

			  AssertRoundTrips( directory, compressionFormat );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @EnumSource(CompressionFormat.class) void shouldRoundTripFilesWithDifferentContent(CompressionFormat compressionFormat) throws java.io.IOException, IncorrectFormat
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRoundTripFilesWithDifferentContent( CompressionFormat compressionFormat )
		 {
			  Path directory = _testDirectory.directory( "a-directory" ).toPath();
			  Files.createDirectories( directory );
			  Files.write( directory.resolve( "a-file" ), "text".GetBytes() );
			  Files.write( directory.resolve( "another-file" ), "some-different-text".GetBytes() );

			  AssertRoundTrips( directory, compressionFormat );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @EnumSource(CompressionFormat.class) void shouldRoundTripEmptyDirectories(CompressionFormat compressionFormat) throws java.io.IOException, IncorrectFormat
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRoundTripEmptyDirectories( CompressionFormat compressionFormat )
		 {
			  Path directory = _testDirectory.directory( "a-directory" ).toPath();
			  Path subdir = directory.resolve( "a-subdirectory" );
			  Files.createDirectories( subdir );
			  AssertRoundTrips( directory, compressionFormat );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @EnumSource(CompressionFormat.class) void shouldRoundTripFilesInDirectories(CompressionFormat compressionFormat) throws java.io.IOException, IncorrectFormat
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldRoundTripFilesInDirectories( CompressionFormat compressionFormat )
		 {
			  Path directory = _testDirectory.directory( "a-directory" ).toPath();
			  Path subdir = directory.resolve( "a-subdirectory" );
			  Files.createDirectories( subdir );
			  Files.write( subdir.resolve( "a-file" ), "text".GetBytes() );
			  AssertRoundTrips( directory, compressionFormat );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @EnumSource(CompressionFormat.class) void shouldCopeWithLongPaths(CompressionFormat compressionFormat) throws java.io.IOException, IncorrectFormat
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldCopeWithLongPaths( CompressionFormat compressionFormat )
		 {
			  Path directory = _testDirectory.directory( "a-directory" ).toPath();
			  Path subdir = directory.resolve( "a/very/long/path/which/is/not/realistic/for/a/database/today/but/which" + "/ensures/that/we/dont/get/caught/out/at/in/the/future/the/point/being/that/there/are/multiple/tar" + "/formats/some/of/which/do/not/cope/with/long/paths" );
			  Files.createDirectories( subdir );
			  Files.write( subdir.resolve( "a-file" ), "text".GetBytes() );
			  AssertRoundTrips( directory, compressionFormat );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @EnumSource(CompressionFormat.class) void shouldExcludeFilesMatchedByTheExclusionPredicate(CompressionFormat compressionFormat) throws java.io.IOException, IncorrectFormat
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldExcludeFilesMatchedByTheExclusionPredicate( CompressionFormat compressionFormat )
		 {
			  Path directory = _testDirectory.directory( "a-directory" ).toPath();
			  Files.createDirectories( directory );
			  Files.write( directory.resolve( "a-file" ), new sbyte[0] );
			  Files.write( directory.resolve( "another-file" ), new sbyte[0] );

			  Path archive = _testDirectory.file( "the-archive.dump" ).toPath();
			  ( new Dumper() ).Dump(directory, directory, archive, compressionFormat, path => path.FileName.ToString().Equals("another-file"));
			  Path newDirectory = _testDirectory.file( "the-new-directory" ).toPath();
			  ( new Loader() ).Load(archive, newDirectory, newDirectory);

			  Path expectedOutput = _testDirectory.directory( "expected-output" ).toPath();
			  Files.createDirectories( expectedOutput );
			  Files.write( expectedOutput.resolve( "a-file" ), new sbyte[0] );

			  assertEquals( DescribeRecursively( expectedOutput ), DescribeRecursively( newDirectory ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @EnumSource(CompressionFormat.class) void shouldExcludeWholeDirectoriesMatchedByTheExclusionPredicate(CompressionFormat compressionFormat) throws java.io.IOException, IncorrectFormat
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldExcludeWholeDirectoriesMatchedByTheExclusionPredicate( CompressionFormat compressionFormat )
		 {
			  Path directory = _testDirectory.directory( "a-directory" ).toPath();
			  Path subdir = directory.resolve( "subdir" );
			  Files.createDirectories( subdir );
			  Files.write( subdir.resolve( "a-file" ), new sbyte[0] );

			  Path archive = _testDirectory.file( "the-archive.dump" ).toPath();
			  ( new Dumper() ).Dump(directory, directory, archive, compressionFormat, path => path.FileName.ToString().Equals("subdir"));
			  Path newDirectory = _testDirectory.file( "the-new-directory" ).toPath();
			  ( new Loader() ).Load(archive, newDirectory, newDirectory);

			  Path expectedOutput = _testDirectory.directory( "expected-output" ).toPath();
			  Files.createDirectories( expectedOutput );

			  assertEquals( DescribeRecursively( expectedOutput ), DescribeRecursively( newDirectory ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ParameterizedTest @EnumSource(CompressionFormat.class) void dumpAndLoadTransactionLogsFromCustomLocations(CompressionFormat compressionFormat) throws java.io.IOException, IncorrectFormat
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void DumpAndLoadTransactionLogsFromCustomLocations( CompressionFormat compressionFormat )
		 {
			  Path directory = _testDirectory.directory( "dbDirectory" ).toPath();
			  Path txLogsDirectory = _testDirectory.directory( "txLogsDirectory" ).toPath();
			  Files.write( directory.resolve( "dbfile" ), new sbyte[0] );
			  Files.write( txLogsDirectory.resolve( TransactionLogFiles.DEFAULT_NAME + ".0" ), new sbyte[0] );

			  Path archive = _testDirectory.file( "the-archive.dump" ).toPath();
			  ( new Dumper() ).Dump(directory, txLogsDirectory, archive, compressionFormat, Predicates.alwaysFalse());
			  Path newDirectory = _testDirectory.file( "the-new-directory" ).toPath();
			  Path newTxLogsDirectory = _testDirectory.file( "newTxLogsDirectory" ).toPath();
			  ( new Loader() ).Load(archive, newDirectory, newTxLogsDirectory);

			  Path expectedOutput = _testDirectory.directory( "expected-output" ).toPath();
			  Files.createDirectories( expectedOutput );
			  Files.write( expectedOutput.resolve( "dbfile" ), new sbyte[0] );

			  Path expectedTxLogs = _testDirectory.directory( "expectedTxLogs" ).toPath();
			  Files.createDirectories( expectedTxLogs );
			  Files.write( expectedTxLogs.resolve( TransactionLogFiles.DEFAULT_NAME + ".0" ), new sbyte[0] );

			  assertEquals( DescribeRecursively( expectedOutput ), DescribeRecursively( newDirectory ) );
			  assertEquals( DescribeRecursively( expectedTxLogs ), DescribeRecursively( newTxLogsDirectory ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertRoundTrips(java.nio.file.Path oldDirectory, CompressionFormat compressionFormat) throws java.io.IOException, IncorrectFormat
		 private void AssertRoundTrips( Path oldDirectory, CompressionFormat compressionFormat )
		 {
			  Path archive = _testDirectory.file( "the-archive.dump" ).toPath();
			  ( new Dumper() ).Dump(oldDirectory, oldDirectory, archive, compressionFormat, Predicates.alwaysFalse());
			  Path newDirectory = _testDirectory.file( "the-new-directory" ).toPath();
			  ( new Loader() ).Load(archive, newDirectory, newDirectory);

			  assertEquals( DescribeRecursively( oldDirectory ), DescribeRecursively( newDirectory ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.Map<java.nio.file.Path,Description> describeRecursively(java.nio.file.Path directory) throws java.io.IOException
		 private IDictionary<Path, Description> DescribeRecursively( Path directory )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return Files.walk( directory ).map( path => pair( directory.relativize( path ), Describe( path ) ) ).collect( Hashtable::new, ( pathDescriptionHashMap, pathDescriptionPair ) => pathDescriptionHashMap.put( pathDescriptionPair.first(), pathDescriptionPair.other() ), Hashtable.putAll );
		 }

		 private Description Describe( Path file )
		 {
			  try
			  {
					return isDirectory( file ) ? new DirectoryDescription( this ) : new FileDescription( this, Files.readAllBytes( file ) );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 private interface Description
		 {
		 }

		 private class DirectoryDescription : Description
		 {
			 private readonly ArchiveTest _outerInstance;

			 public DirectoryDescription( ArchiveTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override bool Equals( object o )
			  {
					return this == o || !( o == null || this.GetType() != o.GetType() );
			  }

			  public override int GetHashCode()
			  {
					return 1;
			  }
		 }

		 private class FileDescription : Description
		 {
			 private readonly ArchiveTest _outerInstance;

			  internal readonly sbyte[] Bytes;

			  internal FileDescription( ArchiveTest outerInstance, sbyte[] bytes )
			  {
				  this._outerInstance = outerInstance;
					this.Bytes = bytes;
			  }

			  public override bool Equals( object o )
			  {
					if ( this == o )
					{
						 return true;
					}
					if ( o == null || this.GetType() != o.GetType() )
					{
						 return false;
					}
					FileDescription that = ( FileDescription ) o;
					return Arrays.Equals( Bytes, that.Bytes );
			  }

			  public override int GetHashCode()
			  {
					return Arrays.GetHashCode( Bytes );
			  }
		 }
	}

}