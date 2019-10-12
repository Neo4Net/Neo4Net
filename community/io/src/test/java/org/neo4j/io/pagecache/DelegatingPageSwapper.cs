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
namespace Org.Neo4j.Io.pagecache
{

	/// <summary>
	/// A PageSwapper that delegates all calls to a wrapped PageSwapper instance.
	/// 
	/// Useful for overriding specific functionality in a sub-class.
	/// </summary>
	public class DelegatingPageSwapper : PageSwapper
	{
		 private readonly PageSwapper @delegate;

		 public DelegatingPageSwapper( PageSwapper @delegate )
		 {
			  this.@delegate = @delegate;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long read(long filePageId, long bufferAddress, int bufferSize) throws java.io.IOException
		 public override long Read( long filePageId, long bufferAddress, int bufferSize )
		 {
			  return @delegate.Read( filePageId, bufferAddress, bufferSize );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  @delegate.Close();
		 }

		 public override void Evicted( long filePageId )
		 {
			  @delegate.Evicted( filePageId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void force() throws java.io.IOException
		 public override void Force()
		 {
			  @delegate.Force();
		 }

		 public override File File()
		 {
			  return @delegate.File();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long write(long filePageId, long bufferAddress) throws java.io.IOException
		 public override long Write( long filePageId, long bufferAddress )
		 {
			  return @delegate.Write( filePageId, bufferAddress );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long getLastPageId() throws java.io.IOException
		 public virtual long LastPageId
		 {
			 get
			 {
				  return @delegate.LastPageId;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void truncate() throws java.io.IOException
		 public override void Truncate()
		 {
			  @delegate.Truncate();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void closeAndDelete() throws java.io.IOException
		 public override void CloseAndDelete()
		 {
			  @delegate.CloseAndDelete();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long read(long startFilePageId, long[] bufferAddresses, int bufferSize, int arrayOffset, int length) throws java.io.IOException
		 public override long Read( long startFilePageId, long[] bufferAddresses, int bufferSize, int arrayOffset, int length )
		 {
			  return @delegate.Read( startFilePageId, bufferAddresses, bufferSize, arrayOffset, length );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public long write(long startFilePageId, long[] bufferAddresses, int arrayOffset, int length) throws java.io.IOException
		 public override long Write( long startFilePageId, long[] bufferAddresses, int arrayOffset, int length )
		 {
			  return @delegate.Write( startFilePageId, bufferAddresses, arrayOffset, length );
		 }
	}

}