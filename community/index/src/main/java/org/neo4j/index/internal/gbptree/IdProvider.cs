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
namespace Org.Neo4j.Index.@internal.gbptree
{

	using PageCursor = Org.Neo4j.Io.pagecache.PageCursor;

	/// <summary>
	/// Provide tree node (page) ids which can be used for storing tree node data.
	/// Bytes on returned page ids must be empty (all zeros).
	/// </summary>
	internal interface IdProvider
	{
		 /// <summary>
		 /// Acquires a page id, guaranteed to currently not be used. The bytes on the page at this id
		 /// are all guaranteed to be zero at the point of returning from this method.
		 /// </summary>
		 /// <param name="stableGeneration"> current stable generation. </param>
		 /// <param name="unstableGeneration"> current unstable generation. </param>
		 /// <returns> page id guaranteed to current not be used and whose bytes are all zeros. </returns>
		 /// <exception cref="IOException"> on <seealso cref="PageCursor"/> error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: long acquireNewId(long stableGeneration, long unstableGeneration) throws java.io.IOException;
		 long AcquireNewId( long stableGeneration, long unstableGeneration );

		 /// <summary>
		 /// Releases a page id which has previously been used, but isn't anymore, effectively allowing
		 /// it to be reused and returned from <seealso cref="acquireNewId(long, long)"/>.
		 /// </summary>
		 /// <param name="stableGeneration"> current stable generation. </param>
		 /// <param name="unstableGeneration"> current unstable generation. </param>
		 /// <param name="id"> page id to release. </param>
		 /// <exception cref="IOException"> on <seealso cref="PageCursor"/> error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void releaseId(long stableGeneration, long unstableGeneration, long id) throws java.io.IOException;
		 void ReleaseId( long stableGeneration, long unstableGeneration, long id );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void visitFreelist(IdProvider_IdProviderVisitor visitor) throws java.io.IOException;
		 void VisitFreelist( IdProvider_IdProviderVisitor visitor );

		 long LastId();
	}

	 internal interface IdProvider_IdProviderVisitor
	 {
		  void BeginFreelistPage( long pageId );

		  void EndFreelistPage( long pageId );

		  void FreelistEntry( long pageId, long generation, int pos );
	 }

	  internal class IdProvider_IdProviderVisitor_Adaptor : IdProvider_IdProviderVisitor
	  {
			public override void BeginFreelistPage( long pageId )
			{
			}

			public override void EndFreelistPage( long pageId )
			{
			}

			public override void FreelistEntry( long pageId, long generation, int pos )
			{
			}
	  }

}