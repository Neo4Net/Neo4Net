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


	/// <summary>
	/// Wraps a <seealso cref="PageCache"/> and ensures that read <seealso cref="PageCursor"/> i.e. page cursors which are created
	/// with <seealso cref="PagedFile.PF_SHARED_READ_LOCK"/>, only read data inside <seealso cref="PageCursor.shouldRetry() do-shouldRetry"/>
	/// loop. It does so by raising a flag on the e.g. {@code getInt} methods, reseting that flag in
	/// <seealso cref="PageCursor.shouldRetry()"/> and asserting that flag not being cleared with doing
	/// <seealso cref="PageCursor.next()"/>, <seealso cref="PageCursor.next(long)"/> and <seealso cref="PageCursor.close()"/>.
	/// </summary>
	public class AccessCheckingPageCache : DelegatingPageCache
	{
		 public AccessCheckingPageCache( PageCache @delegate ) : base( @delegate )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.pagecache.PagedFile map(java.io.File file, int pageSize, java.nio.file.OpenOption... openOptions) throws java.io.IOException
		 public override PagedFile Map( File file, int pageSize, params OpenOption[] openOptions )
		 {
			  return new AccessCheckingPagedFile( base.Map( file, pageSize, openOptions ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Optional<org.neo4j.io.pagecache.PagedFile> getExistingMapping(java.io.File file) throws java.io.IOException
		 public override Optional<PagedFile> GetExistingMapping( File file )
		 {
			  return base.GetExistingMapping( file );
		 }
	}

}