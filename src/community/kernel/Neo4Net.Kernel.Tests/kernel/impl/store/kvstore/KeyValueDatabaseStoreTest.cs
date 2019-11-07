using System.Diagnostics;

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
namespace Neo4Net.Kernel.impl.store.kvstore
{
	using Test = org.junit.Test;


	using StubPageCursor = Neo4Net.Io.pagecache.StubPageCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.kvstore.KeyValueDatabaseStoreTest.CataloguePage.findPage;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.kvstore.KeyValueDatabaseStoreTest.CataloguePage.page;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.kvstore.KeyValueStoreFile.maxPage;

	public class KeyValueDatabaseStoreTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindPageInPageCatalogue()
		 public virtual void ShouldFindPageInPageCatalogue()
		 {
			  assertEquals( "(single page) in middle of range", 0, findPage( 50, page( 1, 100 ) ) );
			  assertEquals( "(single page) at beginning of range", 0, findPage( 1, page( 1, 100 ) ) );
			  assertEquals( "(single page) at end of range", 0, findPage( 100, page( 1, 100 ) ) );
			  assertEquals( "(single page) before range", 0, findPage( 1, page( 10, 100 ) ) );
			  assertEquals( "(single page) after range", 1, findPage( 200, page( 1, 100 ) ) );

			  assertEquals( "(two pages) at beginning of second page", 1, findPage( 11, page( 1, 10 ), page( 11, 20 ) ) );
			  assertEquals( "(two pages) at end of first page", 0, findPage( 10, page( 1, 10 ), page( 11, 20 ) ) );
			  assertEquals( "(two pages) between pages (-> second page)", 1, findPage( 11, page( 1, 10 ), page( 21, 30 ) ) );
			  assertEquals( "(two pages) between pages (-> second page)", 1, findPage( 11, page( 1, 10 ), page( 12, 30 ) ) );
			  assertEquals( "(two pages) after pages", 2, findPage( 31, page( 1, 10 ), page( 21, 30 ) ) );

			  assertEquals( "(three pages) after pages", 3, findPage( 100, page( 1, 10 ), page( 21, 30 ), page( 41, 50 ) ) );

			  assertEquals( "overlapping page boundary", 0, findPage( 17, page( 2, 17 ), page( 17, 32 ), page( 32, 50 ) ) );
			  assertEquals( "multiple pages with same key", 1, findPage( 3, page( 1, 2 ), page( 2, 3 ), page( 3, 3 ), page( 3, 3 ), page( 3, 3 ), page( 3, 3 ), page( 3, 3 ), page( 3, 3 ), page( 3, 4 ), page( 5, 6 ) ) );
		 }

		 /// <summary>
		 /// key size = 1 byte </summary>
		 internal class CataloguePage
		 {
			  internal static int FindPage( int key, params CataloguePage[] pages )
			  {
					Debug.Assert( key >= 0 && key <= 0xFF, "invalid usage" );
					sbyte[] catalogue = new sbyte[pages.Length * 2];
					for ( int i = 0, min = 0; i < pages.Length; i++ )
					{
						 CataloguePage page = pages[i];
						 assert( page.First & 0xFF ) >= min : "invalid catalogue";
						 catalogue[i * 2] = page.First;
						 catalogue[i * 2 + 1] = page.Last;
						 min = page.Last & 0xFF;
					}
					return KeyValueStoreFile.FindPage( new BigEndianByteArrayBuffer( new sbyte[]{ ( sbyte ) key } ), catalogue );
			  }

			  internal static CataloguePage Page( int first, int last )
			  {
					Debug.Assert( first >= 0 && last >= 0 && first <= 0xFF && last <= 0xFF && first <= last, "invalid usage" );
					return new CataloguePage( ( sbyte ) first, ( sbyte ) last );
			  }

			  internal readonly sbyte First;
			  internal sbyte Last;

			  internal CataloguePage( sbyte first, sbyte last )
			  {
					this.First = first;
					this.Last = last;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldComputeMaxPage()
		 public virtual void ShouldComputeMaxPage()
		 {
			  assertEquals( "less than one page", 0, maxPage( 1024, 4, 100 ) );
			  assertEquals( "exactly one page", 0, maxPage( 1024, 4, 256 ) );
			  assertEquals( "just over one page", 1, maxPage( 1024, 4, 257 ) );
			  assertEquals( "exactly two pages", 1, maxPage( 1024, 4, 512 ) );
			  assertEquals( "over two pages", 2, maxPage( 1024, 4, 700 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindRecordInPage() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindRecordInPage()
		 {
			  // given
			  sbyte[] key = new sbyte[1];
			  sbyte[] value = new sbyte[3];
			  DataPage page = new DataPageAnonymousInnerClass( this, key, value );

			  // when/then
			  for ( int i = 0; i < 256; i++ )
			  {
					assertEquals( i + 5, page.FindOffset( i ) );
					assertEquals( i, key[0] & 0xFF );
			  }
		 }

		 private class DataPageAnonymousInnerClass : DataPage
		 {
			 private readonly KeyValueDatabaseStoreTest _outerInstance;

			 private sbyte[] _key;
			 private sbyte[] _value;

			 public DataPageAnonymousInnerClass( KeyValueDatabaseStoreTest outerInstance, sbyte[] key, sbyte[] value ) : base( 4096, 5, 256, key, value )
			 {
				 this.outerInstance = outerInstance;
				 this._key = key;
				 this._value = value;
			 }

			 internal override void writeDataEntry( int record, WritableBuffer key, WritableBuffer value )
			 {
				  key.PutByte( 0, ( sbyte ) record );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindRecordInPageWithDuplicates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindRecordInPageWithDuplicates()
		 {
			  // given
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte[] keys = new byte[]{1, 2, 2, 3, 4};
			  sbyte[] keys = new sbyte[]{ 1, 2, 2, 3, 4 };
			  sbyte[] key = new sbyte[1];
			  sbyte[] value = new sbyte[3];
			  DataPage page = new DataPageAnonymousInnerClass2( this, key, value, keys );

			  // when/then
			  assertEquals( 0, page.FindOffset( 0 ) );
			  assertEquals( 0, value[0] & 0xFF );
			  assertEquals( 1, page.FindOffset( 1 ) );
			  assertEquals( 1, value[0] & 0xFF );
			  assertEquals( 1, page.FindOffset( 2 ) );
			  assertEquals( 1, value[0] & 0xFF );
			  assertEquals( 3, page.FindOffset( 3 ) );
			  assertEquals( 3, value[0] & 0xFF );
			  assertEquals( 4, page.FindOffset( 4 ) );
			  assertEquals( 4, value[0] & 0xFF );
		 }

		 private class DataPageAnonymousInnerClass2 : DataPage
		 {
			 private readonly KeyValueDatabaseStoreTest _outerInstance;

			 private sbyte[] _keys;
			 private sbyte[] _key;
			 private sbyte[] _value;

			 public DataPageAnonymousInnerClass2( KeyValueDatabaseStoreTest outerInstance, sbyte[] key, sbyte[] value, sbyte[] keys ) : base( 4096, 0, 5, key, value )
			 {
				 this.outerInstance = outerInstance;
				 this._keys = keys;
				 this._key = key;
				 this._value = value;
			 }

			 internal override void writeDataEntry( int record, WritableBuffer key, WritableBuffer value )
			 {
				  key.PutByte( 0, _keys[record] );
				  value.PutByte( 0, ( sbyte ) record );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(expected = Neo4Net.kernel.impl.store.UnderlyingStorageException.class) public void shouldThrowOnOutOfBoundsPageAccess() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowOnOutOfBoundsPageAccess()
		 {
			  // given
			  AtomicBoolean goOutOfBounds = new AtomicBoolean();
			  sbyte[] key = new sbyte[1];
			  sbyte[] value = new sbyte[3];
			  DataPage page = new DataPageAnonymousInnerClass3( this, key, value, goOutOfBounds );

			  page.FindOffset( 0 );
			  goOutOfBounds.set( true );
			  page.FindOffset( 0 );
		 }

		 private class DataPageAnonymousInnerClass3 : DataPage
		 {
			 private readonly KeyValueDatabaseStoreTest _outerInstance;

			 private AtomicBoolean _goOutOfBounds;
			 private sbyte[] _key;
			 private sbyte[] _value;

			 public DataPageAnonymousInnerClass3( KeyValueDatabaseStoreTest outerInstance, sbyte[] key, sbyte[] value, AtomicBoolean goOutOfBounds ) : base( 4096, 3, 128, key, value )
			 {
				 this.outerInstance = outerInstance;
				 this._goOutOfBounds = goOutOfBounds;
				 this._key = key;
				 this._value = value;
			 }

			 internal override void writeDataEntry( int record, WritableBuffer key, WritableBuffer value )
			 {
				  key.PutByte( 0, ( sbyte ) 0x42 );
			 }

			 public override bool checkAndClearBoundsFlag()
			 {
				  return _goOutOfBounds.get() | base.checkAndClearBoundsFlag();
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindFirstRecordGreaterThanIfNoExactMatch() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindFirstRecordGreaterThanIfNoExactMatch()
		 {
			  // given
			  sbyte[] key = new sbyte[1];
			  sbyte[] value = new sbyte[3];
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicInteger delta = new java.util.concurrent.atomic.AtomicInteger(1);
			  AtomicInteger delta = new AtomicInteger( 1 );
			  DataPage page = new DataPageAnonymousInnerClass4( this, key, value, delta );
			  delta.set( 0 );

			  // when / then
			  for ( int i = 0; i < 128; i++ )
			  {
					assertEquals( i + 3, page.FindOffset( i ) );
					assertEquals( ( i * 2 ) + 1, key[0] & 0xFF );
			  }
		 }

		 private class DataPageAnonymousInnerClass4 : DataPage
		 {
			 private readonly KeyValueDatabaseStoreTest _outerInstance;

			 private sbyte[] _key;
			 private sbyte[] _value;
			 private AtomicInteger _delta;

			 public DataPageAnonymousInnerClass4( KeyValueDatabaseStoreTest outerInstance, sbyte[] key, sbyte[] value, AtomicInteger delta ) : base( 4096, 3, 128, key, value )
			 {
				 this.outerInstance = outerInstance;
				 this._key = key;
				 this._value = value;
				 this._delta = delta;
			 }

			 internal override void writeDataEntry( int record, WritableBuffer key, WritableBuffer value )
			 {
				  key.PutByte( 0, ( sbyte )( record * 2 + _delta.get() ) );
			 }
		 }

		 private abstract class DataPage : StubPageCursor
		 {
			  internal readonly int HeaderRecords;
			  internal readonly int DataRecords;
			  internal readonly sbyte[] Key;
			  internal readonly sbyte[] Value;

			  internal DataPage( int pageSize, int headerRecords, int dataRecords, sbyte[] key, sbyte[] value ) : base( 0, pageSize )
			  {
					int recordSize = key.Length + value.Length;
					assert( recordSize & -recordSize ) == recordSize : "invalid usage";
					Debug.Assert( recordSize * ( headerRecords + dataRecords ) <= pageSize, "invalid usage" );
					Debug.Assert( dataRecords <= ( 1 << ( key.Length * 8 ) ), "invalid usage" );
					this.Key = key;
					this.Value = value;
					this.HeaderRecords = headerRecords;
					this.DataRecords = dataRecords;
					BigEndianByteArrayBuffer k = new BigEndianByteArrayBuffer( key );
					BigEndianByteArrayBuffer v = new BigEndianByteArrayBuffer( value );
					for ( int record = 0; record < dataRecords; record++ )
					{
						 WriteDataEntry( record, k, v );
						 for ( int i = 0; i < key.Length; i++ )
						 {
							  PutByte( ( record + headerRecords ) * recordSize + i, key[i] );
						 }
						 for ( int i = 0; i < value.Length; i++ )
						 {
							  PutByte( ( record + headerRecords ) * recordSize + key.Length + i, value[i] );
						 }
						 Arrays.fill( key, ( sbyte ) 0 );
						 Arrays.fill( value, ( sbyte ) 0 );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: int findOffset(int key) throws java.io.IOException
			  internal virtual int FindOffset( int key )
			  {
					BigEndianByteArrayBuffer searchKey = new BigEndianByteArrayBuffer( this.Key.Length );
					BigEndianByteArrayBuffer value = new BigEndianByteArrayBuffer( this.Value );
					WriteDataEntry( key, searchKey, value );
					Arrays.fill( this.Value, ( sbyte ) 0 );
					return KeyValueStoreFile.FindEntryOffset( this, searchKey, new BigEndianByteArrayBuffer( this.Key ), value, HeaderRecords, HeaderRecords + DataRecords );
			  }

			  internal abstract void WriteDataEntry( int record, WritableBuffer key, WritableBuffer value );
		 }
	}

}