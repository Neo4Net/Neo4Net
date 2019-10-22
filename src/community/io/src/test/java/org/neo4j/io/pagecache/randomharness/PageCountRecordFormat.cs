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
namespace Neo4Net.Io.pagecache.randomharness
{

	public class PageCountRecordFormat : RecordFormat
	{
		 public override int RecordSize
		 {
			 get
			 {
				  return 16;
			 }
		 }

		 public override Record CreateRecord( File file, int recordId )
		 {
			  return new PageCountRecord( recordId, RecordSize );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Record readRecord(org.Neo4Net.io.pagecache.PageCursor cursor) throws java.io.IOException
		 public override Record ReadRecord( PageCursor cursor )
		 {
			  int offset = cursor.Offset;
			  sbyte[] bytes = new sbyte[RecordSize];
			  do
			  {
					cursor.Offset = offset;
					cursor.GetBytes( bytes );
			  } while ( cursor.ShouldRetry() );
			  return new PageCountRecord( bytes );
		 }

		 public override Record ZeroRecord()
		 {
			  return new PageCountRecord( 0, RecordSize );
		 }

		 public override void Write( Record record, PageCursor cursor )
		 {
			  PageCountRecord r = ( PageCountRecord ) record;
			  int shorts = RecordSize / 2;
			  for ( int i = 0; i < shorts; i++ )
			  {
					cursor.PutShort( r.RecordId );
			  }
		 }

		 private sealed class PageCountRecord : Record
		 {
			  internal readonly sbyte[] Bytes;
			  internal readonly ByteBuffer Buf;

			  internal PageCountRecord( int recordId, int recordSize )
			  {
					if ( recordId > short.MaxValue )
					{
						 throw new System.ArgumentException( "Record ID greater than Short.MAX_VALUE: " + recordId );
					}
					if ( recordSize < 2 )
					{
						 throw new System.ArgumentException( "Record size must be positive: " + recordSize );
					}
					if ( recordSize % 2 != 0 )
					{
						 throw new System.ArgumentException( "Record size must be even: " + recordSize );
					}
					Bytes = new sbyte[recordSize];
					Buf = ByteBuffer.wrap( Bytes );
					for ( int i = 0; i < Bytes.Length; i += 2 )
					{
						 Buf.putShort( ( short ) recordId );
					}
			  }

			  internal PageCountRecord( sbyte[] bytes )
			  {
					if ( bytes.Length == 0 )
					{
						 throw new System.ArgumentException( "Bytes cannot be empty" );
					}
					if ( bytes.Length % 2 != 0 )
					{
						 throw new System.ArgumentException( "Record size must be even: " + bytes.Length );
					}
					sbyte first = bytes[0];
					foreach ( sbyte b in bytes )
					{
						 if ( b != first )
						 {
							  throw new System.ArgumentException( "All bytes must be the same: " + Arrays.ToString( bytes ) );
						 }
					}
					this.Bytes = bytes;
					this.Buf = ByteBuffer.wrap( bytes );
			  }

			  public short RecordId
			  {
				  get
				  {
						return Buf.getShort( 0 );
				  }
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

					PageCountRecord that = ( PageCountRecord ) o;

					return Arrays.Equals( Bytes, that.Bytes );

			  }

			  public override int GetHashCode()
			  {
					return Arrays.GetHashCode( Bytes );
			  }

			  public override string ToString()
			  {
					return "PageCountRecord[" +
							 "bytes=" + Arrays.ToString( Bytes ) +
							 ']';
			  }
		 }
	}

}