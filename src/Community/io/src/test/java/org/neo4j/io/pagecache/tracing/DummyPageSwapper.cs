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
namespace Neo4Net.Io.pagecache.tracing
{

	public class DummyPageSwapper : PageSwapper
	{
		 private readonly string _filename;
		 private readonly int _filePageSize;

		 public DummyPageSwapper( string filename, int filePageSize )
		 {
			  this._filename = filename;
			  this._filePageSize = filePageSize;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long read(long filePageId, long bufferAddress, int bufferSize) throws java.io.IOException
		 public override long Read( long filePageId, long bufferAddress, int bufferSize )
		 {
			  return bufferSize;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long write(long filePageId, long bufferAddress) throws java.io.IOException
		 public override long Write( long filePageId, long bufferAddress )
		 {
			  return _filePageSize;
		 }

		 public override void Evicted( long filePageId )
		 {
		 }

		 public override File File()
		 {
			  return new File( _filename );
		 }

		 public override void Close()
		 {
		 }

		 public override void Force()
		 {
		 }

		 public virtual long LastPageId
		 {
			 get
			 {
				  return 0;
			 }
		 }

		 public override void Truncate()
		 {
		 }

		 public override void CloseAndDelete()
		 {
		 }

		 public override long Read( long startFilePageId, long[] bufferAddresses, int bufferSize, int arrayOffset, int length )
		 {
			  return 0;
		 }

		 public override long Write( long startFilePageId, long[] bufferAddresses, int arrayOffset, int length )
		 {
			  return 0;
		 }
	}

}