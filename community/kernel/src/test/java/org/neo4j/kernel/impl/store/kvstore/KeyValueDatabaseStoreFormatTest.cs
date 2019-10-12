using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.impl.store.kvstore
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Org.Neo4j.Helpers.Collection;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ConfigurablePageCacheRule = Org.Neo4j.Test.rule.ConfigurablePageCacheRule;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.kvstore.KeyValueDatabaseStoreFormatTest.Data.data;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.kvstore.KeyValueDatabaseStoreFormatTest.DataEntry.entry;

	public class KeyValueDatabaseStoreFormatTest
	{
		private bool InstanceFieldsInitialized = false;

		public KeyValueDatabaseStoreFormatTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			Directory = TestDirectory.testDirectory( Fs );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.EphemeralFileSystemRule fs = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule Fs = new EphemeralFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.ConfigurablePageCacheRule pages = new org.neo4j.test.rule.ConfigurablePageCacheRule();
		 public readonly ConfigurablePageCacheRule Pages = new ConfigurablePageCacheRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory directory = org.neo4j.test.rule.TestDirectory.testDirectory(fs);
		 public TestDirectory Directory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateAndOpenEmptyStoreWithEmptyHeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateAndOpenEmptyStoreWithEmptyHeader()
		 {
			  // given
			  Format format = new Format( this );

			  // when
			  format.CreateEmpty( NoHeaders() );

			  // then
			  using ( KeyValueStoreFile file = format.Open() )
			  {
					assertTrue( file.Headers().fields().Count == 0 );
					AssertEntries( 0, file );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateAndOpenEmptyStoreWithHeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateAndOpenEmptyStoreWithHeader()
		 {
			  // given
			  Format format = new Format( this, "foo", "bar" );
			  IDictionary<string, sbyte[]> headers = new Dictionary<string, sbyte[]>();
			  headers["foo"] = new sbyte[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, ( sbyte )'f', ( sbyte )'o', ( sbyte )'o' };
			  headers["bar"] = new sbyte[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, ( sbyte )'b', ( sbyte )'a', ( sbyte )'r' };

			  // when
			  format.CreateEmpty( headers );

			  // then
			  using ( KeyValueStoreFile file = format.Open() )
			  {
					AssertDeepEquals( headers, file.Headers() );
					AssertEntries( 0, file );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateAndOpenStoreWithNoDataAndEmptyHeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateAndOpenStoreWithNoDataAndEmptyHeader()
		 {
			  // given
			  Format format = new Format( this );

			  // when
			  using ( KeyValueStoreFile file = format.Create( NoHeaders(), NoData() ) )
			  {
			  // then
					assertTrue( file.Headers().fields().Count == 0 );
					AssertEntries( 0, file );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateAndOpenStoreWithNoDataWithHeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateAndOpenStoreWithNoDataWithHeader()
		 {
			  // given
			  Format format = new Format( this, "abc", "xyz" );
			  IDictionary<string, sbyte[]> headers = new Dictionary<string, sbyte[]>();
			  headers["abc"] = new sbyte[]{ ( sbyte )'h', ( sbyte )'e', ( sbyte )'l', ( sbyte )'l', ( sbyte )'o', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
			  headers["xyz"] = new sbyte[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, ( sbyte )'w', ( sbyte )'o', ( sbyte )'r', ( sbyte )'l', ( sbyte )'d' };

			  // when
			  using ( KeyValueStoreFile file = format.Create( headers, NoData() ) )
			  {
			  // then
					AssertDeepEquals( headers, file.Headers() );
					AssertEntries( 0, file );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateAndOpenStoreWithDataAndEmptyHeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateAndOpenStoreWithDataAndEmptyHeader()
		 {
			  // given
			  Format format = new Format( this );
			  Data data = data( entry( new sbyte[]{ ( sbyte )'o', ( sbyte )'n', ( sbyte )'e' }, new sbyte[]{ ( sbyte )'a', ( sbyte )'l', ( sbyte )'p', ( sbyte )'h', ( sbyte )'a' } ), entry( new sbyte[]{ ( sbyte )'t', ( sbyte )'w', ( sbyte )'o' }, new sbyte[]{ ( sbyte )'b', ( sbyte )'e', ( sbyte )'t', ( sbyte )'a' } ), entry( new sbyte[]{ ( sbyte )'z', ( sbyte )'e', ( sbyte )'d' }, new sbyte[]{ ( sbyte )'o', ( sbyte )'m', ( sbyte )'e', ( sbyte )'g', ( sbyte )'a' } ) );

			  // when
			  using ( KeyValueStoreFile file = format.Create( NoHeaders(), data ) )
			  {
			  // then
					assertTrue( file.Headers().fields().Count == 0 );
					file.Scan( ExpectData( data ) );
					assertEquals( "number of entries", 3, data.Index );
					AssertEntries( 3, file );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateAndOpenStoreWithDataAndHeader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateAndOpenStoreWithDataAndHeader()
		 {
			  // given
			  Format format = new Format( this, "abc", "xyz" );
			  IDictionary<string, sbyte[]> headers = new Dictionary<string, sbyte[]>();
			  headers["abc"] = new sbyte[]{ ( sbyte )'h', ( sbyte )'e', ( sbyte )'l', ( sbyte )'l', ( sbyte )'o', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
			  headers["xyz"] = new sbyte[]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, ( sbyte )'w', ( sbyte )'o', ( sbyte )'r', ( sbyte )'l', ( sbyte )'d' };
			  Data data = data( entry( new sbyte[]{ ( sbyte )'o', ( sbyte )'n', ( sbyte )'e' }, new sbyte[]{ ( sbyte )'a', ( sbyte )'l', ( sbyte )'p', ( sbyte )'h', ( sbyte )'a' } ), entry( new sbyte[]{ ( sbyte )'t', ( sbyte )'w', ( sbyte )'o' }, new sbyte[]{ ( sbyte )'b', ( sbyte )'e', ( sbyte )'t', ( sbyte )'a' } ), entry( new sbyte[]{ ( sbyte )'z', ( sbyte )'e', ( sbyte )'d' }, new sbyte[]{ ( sbyte )'o', ( sbyte )'m', ( sbyte )'e', ( sbyte )'g', ( sbyte )'a' } ) );

			  // when
			  using ( KeyValueStoreFile file = format.Create( headers, data ) )
			  {
			  // then
					AssertDeepEquals( headers, file.Headers() );
					file.Scan( ExpectData( data ) );
					assertEquals( "number of entries", 3, data.Index );
					AssertEntries( 3, file );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindEntriesInFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindEntriesInFile()
		 {
			  // given
			  Format format = new Format( this, "one", "two" );
			  IDictionary<string, sbyte[]> headers = new Dictionary<string, sbyte[]>();
			  headers["one"] = new sbyte[]{ 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
			  headers["two"] = new sbyte[]{ 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
			  IDictionary<string, string> config = new Dictionary<string, string>();
			  config[GraphDatabaseSettings.pagecache_memory.name()] = "8M";
			  Data data = data( entry( Bytes( 17 ), Bytes( 'v', 'a', 'l', 1 ) ), entry( Bytes( 22 ), Bytes( 'v', 'a', 'l', 2 ) ), entry( Bytes( 22 ), Bytes( 'v', 'a', 'l', 3 ) ), entry( Bytes( 25 ), Bytes( 'v', 'a', 'l', 4 ) ), entry( Bytes( 27 ), Bytes( 'v', 'a', 'l', 5 ) ), entry( Bytes( 27 ), Bytes( 'v', 'a', 'l', 6 ) ), entry( Bytes( 31 ), Bytes( 'v', 'a', 'l', 7 ) ), entry( Bytes( 63 ), Bytes( 'v', 'a', 'l', 8 ) ), entry( Bytes( 127 ), Bytes( 'v', 'a', 'l', 9 ) ), entry( Bytes( 255 ), Bytes( 'v', 'a', 'l', 10 ) ), entry( Bytes( 511 ), Bytes( 'v', 'a', 'l', 11 ) ), entry( Bytes( 1023 ), Bytes( 'v', 'a', 'l', 12 ) ), entry( Bytes( 1050 ), Bytes( 'v', 'a', 'l', 13 ) ), entry( Bytes( 2000 ), Bytes( 'v', 'a', 'l', 14 ) ) );

			  // when
			  using ( KeyValueStoreFile file = format.Create( config, headers, data ) )
			  {
			  // then
					AssertFind( file, 17, 17, true, new Bytes( 'v', 'a', 'l', 1 ) );
					AssertFind( file, 22, 22, true, new Bytes( 'v', 'a', 'l', 2 ), new Bytes( 'v', 'a', 'l', 3 ) );
					AssertFind( file, 25, 25, true, new Bytes( 'v', 'a', 'l', 4 ) );
					AssertFind( file, 27, 27, true, new Bytes( 'v', 'a', 'l', 5 ), new Bytes( 'v', 'a', 'l', 6 ) );
					AssertFind( file, 26, 30, false, new Bytes( 'v', 'a', 'l', 5 ), new Bytes( 'v', 'a', 'l', 6 ) );
					AssertFind( file, 31, 31, true, new Bytes( 'v', 'a', 'l', 7 ) );
					AssertFind( file, 32, 1024, false, new Bytes( 'v', 'a', 'l', 8 ), new Bytes( 'v', 'a', 'l', 9 ), new Bytes( 'v', 'a', 'l', 10 ), new Bytes( 'v', 'a', 'l', 11 ), new Bytes( 'v', 'a', 'l', 12 ) );
					AssertFind( file, 1050, 1050, true, new Bytes( 'v', 'a', 'l', 13 ) );
					AssertFind( file, 2000, 2000, true, new Bytes( 'v', 'a', 'l', 14 ) );
					AssertFind( file, 1500, 8000, false, new Bytes( 'v', 'a', 'l', 14 ) );
					AssertFind( file, 1050, 8000, true, new Bytes( 'v', 'a', 'l', 13 ), new Bytes( 'v', 'a', 'l', 14 ) );
					AssertFind( file, 2001, int.MaxValue, false );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFindAnythingWhenSearchKeyIsAfterTheLastKey() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFindAnythingWhenSearchKeyIsAfterTheLastKey()
		 {
			  // given
			  Format format = new Format( this );
			  IDictionary<string, sbyte[]> metadata = new Dictionary<string, sbyte[]>();
			  IDictionary<string, string> config = new Dictionary<string, string>();
			  config[GraphDatabaseSettings.pagecache_memory.name()] = "8M";
			  Data data = data( entry( Bytes( 12 ), Bytes( 'v', 'a', 'l', 1 ) ), entry( Bytes( 13 ), Bytes( 'v', 'a', 'l', 2 ) ), entry( Bytes( 15 ), Bytes( 'v', 'a', 'l', 3 ) ), entry( Bytes( 16 ), Bytes( 'v', 'a', 'l', 4 ) ), entry( Bytes( 17 ), Bytes( 'v', 'a', 'l', 5 ) ), entry( Bytes( 18 ), Bytes( 'v', 'a', 'l', 6 ) ) );

			  // when
			  using ( KeyValueStoreFile file = format.Create( config, metadata, data ) )
			  {
			  // then
					AssertFind( file, 14, 15, false, new Bytes( 'v', 'a', 'l', 3 ) ); // after the first page
					AssertFind( file, 19, 25, false ); // after the second page
					AssertFind( file, 18, 25, true, new Bytes( 'v', 'a', 'l', 6 ) ); // last entry of the last page
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTruncateTheFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldTruncateTheFile()
		 {
			  IDictionary<string, string> config = new Dictionary<string, string>();
			  config[GraphDatabaseSettings.pagecache_memory.name()] = "8M";

			  {
			  // given a well written file
					Format format = new Format( this, "one", "two" );
					IDictionary<string, sbyte[]> headers = new Dictionary<string, sbyte[]>();
					headers["one"] = new sbyte[]{ 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
					headers["two"] = new sbyte[]{ 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };

					Data data = data( entry( Bytes( 12 ), Bytes( 'v', 'a', 'l', 1 ) ), entry( Bytes( 13 ), Bytes( 'v', 'a', 'l', 2 ) ), entry( Bytes( 15 ), Bytes( 'v', 'a', 'l', 3 ) ), entry( Bytes( 16 ), Bytes( 'v', 'a', 'l', 4 ) ), entry( Bytes( 17 ), Bytes( 'v', 'a', 'l', 5 ) ), entry( Bytes( 18 ), Bytes( 'v', 'a', 'l', 6 ) ) );

					using ( KeyValueStoreFile ignored = format.Create( config, headers, data ) )
					{
					}
			  }

			  {
					// when failing on creating the next version of that file
					Format format = new Format( this, "three", "four" );
					IDictionary<string, sbyte[]> headers = new Dictionary<string, sbyte[]>();
					headers["three"] = new sbyte[]{ 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 };
					headers["four"] = new sbyte[]{ 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 };

					DataProvider data = new DataProviderAnonymousInnerClass( this );

					try
					{
							using ( KeyValueStoreFile ignored = format.Create( config, headers, data ) )
							{
							}
					}
					catch ( IOException io )
					{
						 // then only headers are present in the file and not the old content
						 assertEquals( "boom!", io.Message );
						 AssertFormatSpecifierAndHeadersOnly( headers, Fs.get(), StoreFile );
					}
			  }
		 }

		 private class DataProviderAnonymousInnerClass : DataProvider
		 {
			 private readonly KeyValueDatabaseStoreFormatTest _outerInstance;

			 public DataProviderAnonymousInnerClass( KeyValueDatabaseStoreFormatTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public void close()
			 {
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean visit(WritableBuffer key, WritableBuffer value) throws java.io.IOException
			 public bool visit( WritableBuffer key, WritableBuffer value )
			 {
				  throw new IOException( "boom!" );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertFormatSpecifierAndHeadersOnly(java.util.Map<String,byte[]> headers, org.neo4j.io.fs.FileSystemAbstraction fs, java.io.File file) throws java.io.IOException
		 private void AssertFormatSpecifierAndHeadersOnly( IDictionary<string, sbyte[]> headers, FileSystemAbstraction fs, File file )
		 {
			  assertTrue( fs.FileExists( file ) );
			  using ( Stream stream = fs.OpenAsInputStream( file ) )
			  {
					// format specifier
					int read;
					int size = 16;
					sbyte[] readEntry = new sbyte[size];
					sbyte[] allZeros = new sbyte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

					read = stream.Read( readEntry, 0, readEntry.Length );
					assertEquals( size, read );
					assertArrayEquals( allZeros, readEntry );

					read = stream.Read( readEntry, 0, readEntry.Length );
					assertEquals( size, read );
					assertArrayEquals( new sbyte[]{ ( sbyte ) - 1, ( sbyte ) - 1, ( sbyte ) - 1, ( sbyte ) - 1, ( sbyte ) - 1, ( sbyte ) - 1, ( sbyte ) - 1, ( sbyte ) - 1, ( sbyte ) - 1, ( sbyte ) - 1, ( sbyte ) - 1, ( sbyte ) - 1, ( sbyte ) - 1, ( sbyte ) - 1, ( sbyte ) - 1, ( sbyte ) - 1 }, readEntry );

					for ( int i = 0; i < headers.Count; i++ )
					{
						 read = stream.Read( readEntry, 0, readEntry.Length );
						 assertEquals( size, read );
						 assertArrayEquals( allZeros, readEntry );

						 read = stream.Read( readEntry, 0, readEntry.Length );
						 assertEquals( size, read );
						 headers.ContainsValue( readEntry );
					}

					assertEquals( -1, stream.Read() );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void assertFind(KeyValueStoreFile file, int min, int max, boolean exact, Bytes... expected) throws java.io.IOException
		 private static void AssertFind( KeyValueStoreFile file, int min, int max, bool exact, params Bytes[] expected )
		 {
			  Pair<bool, IList<Bytes>> result = Find( file, min, max );
			  assertEquals( "exact match", exact, result.First() );
			  assertEquals( string.Format( "find(min={0:D}, max={1:D})", min, max ), Arrays.asList( expected ), result.Other() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static org.neo4j.helpers.collection.Pair<bool,java.util.List<Bytes>> find(KeyValueStoreFile file, final int min, final int max) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private static Pair<bool, IList<Bytes>> Find( KeyValueStoreFile file, int min, int max )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<Bytes> values = new java.util.ArrayList<>();
			  IList<Bytes> values = new List<Bytes>();
			  bool result = file.Scan(key => key.putInt(key.size() - 4, min), (key, value) =>
			  {
				if ( key.getInt( key.size() - 4 ) <= max )
				{
					 values.Add( new Bytes( value.get( 0, new sbyte[value.size()] ) ) );
					 return true;
				}
				return false;
			  });
			  return Pair.of( result, values );
		 }

		 internal class Bytes
		 {
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  internal readonly sbyte[] BytesConflict;

			  internal Bytes( sbyte[] bytes )
			  {
					this.BytesConflict = bytes;
			  }

			  internal Bytes( params int[] data )
			  {
					this.BytesConflict = Bytes( data );
			  }

			  public override string ToString()
			  {
					return Arrays.ToString( BytesConflict );
			  }

			  public override bool Equals( object o )
			  {
					return this == o || o is Bytes && Arrays.Equals( BytesConflict, ( ( Bytes ) o ).BytesConflict );
			  }

			  public override int GetHashCode()
			  {
					return Arrays.GetHashCode( BytesConflict );
			  }
		 }

		 private static sbyte[] Bytes( params int[] data )
		 {
			  if ( data.Length > 4 )
			  {
					throw new AssertionError( "Invalid usage; should have <= 4 data items, got: " + data.Length );
			  }
			  sbyte[] result = new sbyte[16];
			  for ( int d = data.Length, r = result.Length - 4; d-- > 0; r -= 4 )
			  {
					int value = data[d];
					for ( int i = 4; i-- > 0; )
					{
						 result[r + i] = unchecked( ( sbyte )( value & 0xFF ) );
						 value = ( int )( ( uint )value >> 8 );
					}
			  }
			  return result;
		 }

		 private void AssertDeepEquals( IDictionary<string, sbyte[]> expected, Headers actual )
		 {
			  try
			  {
					int size = 0;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (HeaderField<?> field : actual.fields())
					foreach ( HeaderField<object> field in actual.Fields() )
					{
						 assertArrayEquals( field.ToString(), expected[field.ToString()], (sbyte[]) actual.Get(field) );
						 size++;
					}
					assertEquals( "number of headers", expected.Count, size );
			  }
			  catch ( AssertionError e )
			  {
					Console.WriteLine( actual );
					throw e;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void assertEntries(final int expected, KeyValueStoreFile file) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 internal static void AssertEntries( int expected, KeyValueStoreFile file )
		 {
//JAVA TO C# CONVERTER TODO TASK: Local classes are not converted by Java to C# Converter:
//			  class Visitor implements KeyValueVisitor
	//		  {
	//				int visited;
	//
	//				@@Override public boolean visit(ReadableBuffer key, ReadableBuffer value)
	//				{
	//					 if (++visited > expected)
	//					 {
	//						  fail("should not have more than " + expected + " data entries");
	//					 }
	//					 return true;
	//				}
	//
	//				void done()
	//				{
	//					 assertEquals("number of entries", expected, visited);
	//				}
	//		  }
			  Visitor visitor = new Visitor();
			  file.Scan( visitor );
			  visitor.done();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: static KeyValueVisitor expectData(final Data expected)
		 internal static KeyValueVisitor ExpectData( Data expected )
		 {
			  expected.Index = 0; // reset the visitor
			  return ( key, value ) =>
			  {
				sbyte[] expectedKey = new sbyte[key.size()];
				sbyte[] expectedValue = new sbyte[value.size()];
				if ( !expected.Visit( new BigEndianByteArrayBuffer( expectedKey ), new BigEndianByteArrayBuffer( expectedValue ) ) )
				{
					 return false;
				}
				AssertEqualContent( expectedKey, key );
				return true;
			  };
		 }

		 internal static void AssertEqualContent( sbyte[] expected, ReadableBuffer actual )
		 {
			  for ( int i = 0; i < expected.Length; i++ )
			  {
					if ( expected[i] != actual.GetByte( i ) )
					{
						 fail( "expected <" + Arrays.ToString( expected ) + "> but was <" + actual + ">" );
					}
			  }
		 }

		 internal class Format : KeyValueStoreFileFormat
		 {
			 private readonly KeyValueDatabaseStoreFormatTest _outerInstance;

			  internal readonly IDictionary<string, HeaderField<sbyte[]>> HeaderFields = new Dictionary<string, HeaderField<sbyte[]>>();

			  internal Format( KeyValueDatabaseStoreFormatTest outerInstance, params string[] defaultHeaderFields ) : this( outerInstance, StubCollector.HeaderFields( defaultHeaderFields ) )
			  {
				  this._outerInstance = outerInstance;
			  }

			  internal Format( KeyValueDatabaseStoreFormatTest outerInstance, HeaderField<sbyte[]>[] headerFields ) : base( 32, headerFields )
			  {
				  this._outerInstance = outerInstance;
					foreach ( HeaderField<sbyte[]> headerField in headerFields )
					{
						 this.HeaderFields[headerField.ToString()] = headerField;
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void createEmpty(java.util.Map<String,byte[]> headers) throws java.io.IOException
			  internal virtual void CreateEmpty( IDictionary<string, sbyte[]> headers )
			  {
					CreateEmptyStore( outerInstance.Fs.get(), outerInstance.StoreFile, 16, 16, headers(headers) );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: KeyValueStoreFile create(java.util.Map<String,byte[]> headers, DataProvider data) throws java.io.IOException
			  internal virtual KeyValueStoreFile Create( IDictionary<string, sbyte[]> headers, DataProvider data )
			  {
					return CreateStore( outerInstance.Fs.get(), outerInstance.Pages.getPageCache(outerInstance.Fs.get()), outerInstance.StoreFile, 16, 16, headers(headers), data );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: KeyValueStoreFile create(java.util.Map<String,String> config, java.util.Map<String,byte[]> headers, DataProvider data) throws java.io.IOException
			  internal virtual KeyValueStoreFile Create( IDictionary<string, string> config, IDictionary<string, sbyte[]> headers, DataProvider data )
			  {
					PageCacheRule.PageCacheConfig pageCacheConfig = PageCacheRule.config();
					PageCache pageCache = outerInstance.Pages.getPageCache( outerInstance.Fs.get(), pageCacheConfig, Config.defaults(config) );
					return CreateStore( outerInstance.Fs.get(), pageCache, outerInstance.StoreFile, 16, 16, headers(headers), data );
			  }

			  internal virtual Headers Headers( IDictionary<string, sbyte[]> headers )
			  {
					Headers.Builder builder = Headers.HeadersBuilder();
					foreach ( KeyValuePair<string, sbyte[]> entry in headers.SetOfKeyValuePairs() )
					{
						 builder.Put( HeaderFields[entry.Key], entry.Value );
					}
					return builder.Headers();
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: KeyValueStoreFile open() throws java.io.IOException
			  internal virtual KeyValueStoreFile Open()
			  {
					return OpenStore( outerInstance.Fs.get(), outerInstance.Pages.getPageCache(outerInstance.Fs.get()), outerInstance.StoreFile );
			  }

			  protected internal override void WriteFormatSpecifier( WritableBuffer formatSpecifier )
			  {
					for ( int i = 0; i < formatSpecifier.Size(); i++ )
					{
						 formatSpecifier.PutByte( i, unchecked( ( sbyte ) 0xFF ) );
					}
			  }
		 }

		 private File StoreFile
		 {
			 get
			 {
				  return Directory.createFile( "storeFile" );
			 }
		 }

		 internal class Data : DataProvider
		 {
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: static Data data(final DataEntry... data)
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  internal static Data DataConflict( params DataEntry[] data )
			  {
					return new Data( data );
			  }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  internal readonly DataEntry[] DataConflict;
			  internal int Index;

			  internal Data( DataEntry[] data )
			  {
					this.DataConflict = data;
			  }

			  public override bool Visit( WritableBuffer key, WritableBuffer value )
			  {
					if ( Index < DataConflict.Length )
					{
						 DataEntry entry = DataConflict[Index++];
						 Write( entry.Key, key );
						 Write( entry.Value, value );
						 return true;
					}
					return false;
			  }

			  public override void Close()
			  {
			  }
		 }

		 internal static void Write( sbyte[] source, WritableBuffer target )
		 {
			  for ( int i = 0; i < source.Length; i++ )
			  {
					target.PutByte( i, source[i] );
			  }
		 }

		 internal static DataProvider NoData()
		 {
			  return new DataProviderAnonymousInnerClass();
		 }

		 private class DataProviderAnonymousInnerClass : DataProvider
		 {
			 public bool visit( WritableBuffer key, WritableBuffer value )
			 {
				  return false;
			 }

			 public void close()
			 {
			 }
		 }

		 internal class DataEntry
		 {
			  internal static DataEntry Entry( sbyte[] key, sbyte[] value )
			  {
					return new DataEntry( key, value );
			  }

			  internal readonly sbyte[] Key;
			  internal sbyte[] Value;

			  internal DataEntry( sbyte[] key, sbyte[] value )
			  {
					this.Key = key;
					this.Value = value;
			  }
		 }

		 internal static IDictionary<string, sbyte[]> NoHeaders()
		 {
			  return Collections.emptyMap();
		 }
	}

}