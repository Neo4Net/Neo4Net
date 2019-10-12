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
namespace Neo4Net.Io.pagecache.randomharness
{

	using MuninnPageCache = Neo4Net.Io.pagecache.impl.muninn.MuninnPageCache;

	public class StandardRecordFormat : RecordFormat
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
			  return new StandardRecord( file, recordId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Record readRecord(org.neo4j.io.pagecache.PageCursor cursor) throws java.io.IOException
		 public override Record ReadRecord( PageCursor cursor )
		 {
			  int offset = cursor.Offset;
			  sbyte t;
			  sbyte f;
			  short f1;
			  int r;
			  long f2;
			  do
			  {
					cursor.Offset = offset;
					t = cursor.Byte;
					f = cursor.Byte;
					f1 = cursor.Short;
					r = cursor.Int;
					f2 = cursor.Long;
			  } while ( cursor.ShouldRetry() );
			  return new StandardRecord( t, f, f1, r, f2 );
		 }

		 public override Record ZeroRecord()
		 {
			  sbyte z = MuninnPageCache.ZERO_BYTE;
			  short sz = ( short )( ( z << 8 ) + z );
			  int iz = ( sz << 16 ) + sz;
			  long lz = ( ( ( long ) iz ) << 32 ) + iz;
			  return new StandardRecord( z, z, sz, iz, lz );
		 }

		 public override void Write( Record record, PageCursor cursor )
		 {
			  StandardRecord r = ( StandardRecord ) record;
			  sbyte[] pathBytes = r.File.Path.getBytes( StandardCharsets.UTF_8 );
			  sbyte fileByte = pathBytes[pathBytes.Length - 1];
			  cursor.PutByte( r.Type );
			  cursor.PutByte( fileByte );
			  cursor.PutShort( r.Fill1 );
			  cursor.PutInt( r.RecordId );
			  cursor.PutLong( r.Fill2 );
		 }

		 internal sealed class StandardRecord : Record
		 {
			  internal readonly sbyte Type;
			  internal readonly File File;
			  internal readonly int RecordId;
			  internal readonly short Fill1;
			  internal readonly long Fill2;

			  internal StandardRecord( File file, int recordId )
			  {
					this.Type = 42;
					this.File = file;
					this.RecordId = recordId;
					int fileHash = file.GetHashCode();

					int a = Xorshift( fileHash ^ Xorshift( recordId ) );
					int b = Xorshift( a );
					int c = Xorshift( b );
					long d = b;
					d = d << 32;
					d += c;
					Fill1 = ( short ) a;
					Fill2 = d;
			  }

			  internal StandardRecord( sbyte type, sbyte fileName, short fill1, int recordId, long fill2 )
			  {
					this.Type = type;
					this.File = fileName == 0 ? null : new File( StringHelper.NewString( new sbyte[]{ fileName } ) );
					this.Fill1 = fill1;
					this.RecordId = recordId;
					this.Fill2 = fill2;
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

					StandardRecord record = ( StandardRecord ) o;

					return Type == record.Type && RecordId == record.RecordId && Fill1 == record.Fill1 && Fill2 == record.Fill2 && FilesEqual( record );

			  }

			  internal bool FilesEqual( StandardRecord record )
			  {
					if ( File == record.File )
					{
						 return true;
					}
					if ( File == null || record.File == null )
					{
						 return false;
					}
					// We only look at the last letter of the path, because that's all that we can store in the record.
					sbyte[] thisPath = File.Path.getBytes( StandardCharsets.UTF_8 );
					sbyte[] thatPath = record.File.Path.getBytes( StandardCharsets.UTF_8 );
					return thisPath[thisPath.Length - 1] == thatPath[thatPath.Length - 1];
			  }

			  public override int GetHashCode()
			  {
					int result = ( int ) Type;
					result = 31 * result + ( File != null ? File.GetHashCode() : 0 );
					result = 31 * result + RecordId;
					result = 31 * result + ( int ) Fill1;
					result = 31 * result + ( int )( Fill2 ^ ( ( long )( ( ulong )Fill2 >> 32 ) ) );
					return result;
			  }

			  internal static int Xorshift( int x )
			  {
					x ^= x << 6;
					x ^= ( int )( ( uint )x >> 21 );
					return x ^ ( x << 7 );
			  }

			  public override string ToString()
			  {
					return Format( Type, File, RecordId, Fill1, Fill2 );
			  }

			  public string Format( sbyte type, File file, int recordId, short fill1, long fill2 )
			  {
					return string.Format( "Record{0}[file={1}, recordId={2}; {3:x4} {4:x16}]", type, file, recordId, fill1, fill2 );
			  }
		 }
	}

}