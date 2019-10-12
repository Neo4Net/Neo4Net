using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Kernel.impl.store.format.highlimit
{
	using Test = org.junit.Test;


	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using StubPageCursor = Neo4Net.Io.pagecache.StubPageCursor;
	using StubPagedFile = Neo4Net.Io.pagecache.StubPagedFile;
	using AbstractBaseRecord = Neo4Net.Kernel.impl.store.record.AbstractBaseRecord;
	using RecordLoad = Neo4Net.Kernel.impl.store.record.RecordLoad;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.format.highlimit.BaseHighLimitRecordFormat.HEADER_BIT_FIRST_RECORD_UNIT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.format.highlimit.BaseHighLimitRecordFormat.HEADER_BIT_RECORD_UNIT;

	public class BaseHighLimitRecordFormatTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustNotCheckForOutOfBoundsWhenReadingSingleRecord() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustNotCheckForOutOfBoundsWhenReadingSingleRecord()
		 {
			  MyRecordFormat format = new MyRecordFormat( this );
			  StubPageCursor cursor = new StubPageCursor( 0, 3 );
			  format.Read( new MyRecord( this, 0 ), cursor, RecordLoad.NORMAL, 4 );
			  assertFalse( cursor.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustCheckForOutOfBoundsWhenReadingDoubleRecord() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustCheckForOutOfBoundsWhenReadingDoubleRecord()
		 {
			  MyRecordFormat format = new MyRecordFormat( this );
			  StubPageCursor cursor = new StubPageCursor( 0, 4 );
			  cursor.PutByte( 0, ( sbyte )( HEADER_BIT_RECORD_UNIT + HEADER_BIT_FIRST_RECORD_UNIT ) );
			  StubPagedFile pagedFile = new StubPagedFileAnonymousInnerClass( this, cursor );
			  format.ShortsPerRecordConflict.AddLast( 2 );
			  format.Read( new MyRecord( this, 0 ), cursor, RecordLoad.NORMAL, 4 );
			  assertTrue( cursor.CheckAndClearBoundsFlag() );
		 }

		 private class StubPagedFileAnonymousInnerClass : StubPagedFile
		 {
			 private readonly BaseHighLimitRecordFormatTest _outerInstance;

			 private StubPageCursor _cursor;

			 public StubPagedFileAnonymousInnerClass( BaseHighLimitRecordFormatTest outerInstance, StubPageCursor cursor ) : base( 3 )
			 {
				 this.outerInstance = outerInstance;
				 this._cursor = cursor;
			 }

			 protected internal override void prepareCursor( StubPageCursor cursor )
			 {
				  cursor.PutByte( 0, ( sbyte ) HEADER_BIT_RECORD_UNIT );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustNotCheckForOutOfBoundsWhenWritingSingleRecord() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustNotCheckForOutOfBoundsWhenWritingSingleRecord()
		 {
			  MyRecordFormat format = new MyRecordFormat( this );
			  StubPageCursor cursor = new StubPageCursor( 0, 3 );
			  MyRecord record = new MyRecord( this, 0 );
			  record.InUse = true;
			  format.Write( record, cursor, 4 );
			  assertFalse( cursor.CheckAndClearBoundsFlag() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustCheckForOutOfBoundsWhenWritingDoubleRecord() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustCheckForOutOfBoundsWhenWritingDoubleRecord()
		 {
			  MyRecordFormat format = new MyRecordFormat( this );
			  StubPageCursor cursor = new StubPageCursor( 0, 5 );
			  MyRecord record = new MyRecord( this, 0 );
			  record.RequiresSecondaryUnit = true;
			  record.SecondaryUnitId = 42;
			  record.InUse = true;
			  format.ShortsPerRecordConflict.AddLast( 3 ); // make the write go out of bounds
			  format.Write( record, cursor, 4 );
			  assertTrue( cursor.CheckAndClearBoundsFlag() );
		 }

		 private class MyRecordFormat : BaseHighLimitRecordFormat<MyRecord>
		 {
			 private readonly BaseHighLimitRecordFormatTest _outerInstance;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal LinkedList<int> ShortsPerRecordConflict = new ConcurrentLinkedQueue<int>();

			  protected internal MyRecordFormat( BaseHighLimitRecordFormatTest outerInstance ) : base( header -> 4, 4, HighLimitFormatSettings.DEFAULT_MAXIMUM_BITS_PER_ID )
			  {
				  this._outerInstance = outerInstance;
			  }

			  protected internal override void DoReadInternal( MyRecord record, PageCursor cursor, int recordSize, long inUseByte, bool inUse )
			  {
					int shortsPerRecord = ShortsPerRecord;
					for ( int i = 0; i < shortsPerRecord; i++ )
					{
						 short v = ( short )( ( cursor.Byte & 0xFF ) << 8 );
						 v += ( short )( cursor.Byte & 0xFF );
						 record.Value = v;
					}
			  }

			  internal virtual int ShortsPerRecord
			  {
				  get
				  {
						int? value = ShortsPerRecordConflict.RemoveFirst();
						return value == null ? 1 : value.Value;
				  }
			  }

			  protected internal override void DoWriteInternal( MyRecord record, PageCursor cursor )
			  {
					int intsPerRecord = ShortsPerRecord;
					for ( int i = 0; i < intsPerRecord; i++ )
					{
						 short v = record.Value;
						 sbyte a = ( sbyte )( ( v & 0x0000FF00 ) >> 8 );
						 sbyte b = unchecked( ( sbyte )( v & 0x000000FF ) );
						 cursor.PutByte( a );
						 cursor.PutByte( b );
					}
			  }

			  protected internal override sbyte HeaderBits( MyRecord record )
			  {
					return 0;
			  }

			  protected internal override bool CanUseFixedReferences( MyRecord record, int recordSize )
			  {
					return false;
			  }

			  protected internal override int RequiredDataLength( MyRecord record )
			  {
					return 4;
			  }

			  public override MyRecord NewRecord()
			  {
					return new MyRecord( _outerInstance, 0 );
			  }
		 }

		 private class MyRecord : AbstractBaseRecord
		 {
			 private readonly BaseHighLimitRecordFormatTest _outerInstance;

			  public short Value;

			  protected internal MyRecord( BaseHighLimitRecordFormatTest outerInstance, long id ) : base( id )
			  {
				  this._outerInstance = outerInstance;
			  }
		 }
	}

}