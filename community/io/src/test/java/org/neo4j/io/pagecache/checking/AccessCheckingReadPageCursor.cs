﻿/*
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
namespace Org.Neo4j.Io.pagecache.checking
{

	using DelegatingPageCursor = Org.Neo4j.Io.pagecache.impl.DelegatingPageCursor;

	public class AccessCheckingReadPageCursor : DelegatingPageCursor
	{
		 private bool _hasReadWithoutShouldRetry;

		 public AccessCheckingReadPageCursor( PageCursor @delegate ) : base( @delegate )
		 {
		 }

		 public override sbyte Byte
		 {
			 get
			 {
				  MarkAsRead();
				  return base.Byte;
			 }
		 }

		 public override void GetBytes( sbyte[] data )
		 {
			  MarkAsRead();
			  base.GetBytes( data );
		 }

		 public override short Short
		 {
			 get
			 {
				  MarkAsRead();
				  return base.Short;
			 }
		 }

		 public override short getShort( int offset )
		 {
			  MarkAsRead();
			  return base.GetShort( offset );
		 }

		 public override long Long
		 {
			 get
			 {
				  MarkAsRead();
				  return base.Long;
			 }
		 }

		 public override long getLong( int offset )
		 {
			  MarkAsRead();
			  return base.GetLong( offset );
		 }

		 public override void GetBytes( sbyte[] data, int arrayOffset, int length )
		 {
			  MarkAsRead();
			  base.GetBytes( data, arrayOffset, length );
		 }

		 public override int GetInt( int offset )
		 {
			  MarkAsRead();
			  return base.GetInt( offset );
		 }

		 public override sbyte getByte( int offset )
		 {
			  MarkAsRead();
			  return base.GetByte( offset );
		 }

		 public override int GetInt()
		 {
			  MarkAsRead();
			  return base.Int;
		 }

		 private void MarkAsRead()
		 {
			  _hasReadWithoutShouldRetry = true;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean shouldRetry() throws java.io.IOException
		 public override bool ShouldRetry()
		 {
			  _hasReadWithoutShouldRetry = false;
			  return base.ShouldRetry();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next() throws java.io.IOException
		 public override bool Next()
		 {
			  AssertNoReadWithoutShouldRetry();
			  return base.Next();
		 }

		 public override void Close()
		 {
			  AssertNoReadWithoutShouldRetry();
			  base.Close();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next(long pageId) throws java.io.IOException
		 public override bool Next( long pageId )
		 {
			  AssertNoReadWithoutShouldRetry();
			  return base.Next( pageId );
		 }

		 private void AssertNoReadWithoutShouldRetry()
		 {
			  if ( _hasReadWithoutShouldRetry )
			  {
					throw new AssertionError( "Performed read from a read cursor without shouldRetry" );
			  }
		 }
	}

}