using System;
using System.Threading;

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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{

	using Org.Neo4j.Index.@internal.gbptree;
	using InternalIndexState = Org.Neo4j.@internal.Kernel.Api.InternalIndexState;
	using ZipUtils = Org.Neo4j.Io.compress.ZipUtils;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using IndexDirectoryStructure = Org.Neo4j.Kernel.Api.Index.IndexDirectoryStructure;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.GBPTree.NO_HEADER_READER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.NativeIndexPopulator.BYTE_FAILED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.NativeIndexPopulator.BYTE_ONLINE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.NativeIndexPopulator.BYTE_POPULATING;

	public class NativeIndexes
	{
		 private NativeIndexes()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.neo4j.internal.kernel.api.InternalIndexState readState(org.neo4j.io.pagecache.PageCache pageCache, java.io.File indexFile) throws java.io.IOException
		 public static InternalIndexState ReadState( PageCache pageCache, File indexFile )
		 {
			  NativeIndexHeaderReader headerReader = new NativeIndexHeaderReader( NO_HEADER_READER );
			  GBPTree.readHeader( pageCache, indexFile, headerReader );
			  switch ( headerReader.State )
			  {
			  case BYTE_FAILED:
					return InternalIndexState.FAILED;
			  case BYTE_ONLINE:
					return InternalIndexState.ONLINE;
			  case BYTE_POPULATING:
					return InternalIndexState.POPULATING;
			  default:
					throw new System.InvalidOperationException( "Unexpected initial state byte value " + headerReader.State );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static String readFailureMessage(org.neo4j.io.pagecache.PageCache pageCache, java.io.File indexFile) throws java.io.IOException
		 internal static string ReadFailureMessage( PageCache pageCache, File indexFile )
		 {
			  NativeIndexHeaderReader headerReader = new NativeIndexHeaderReader( NO_HEADER_READER );
			  GBPTree.readHeader( pageCache, indexFile, headerReader );
			  return headerReader.FailureMessage;
		 }

		 /// <summary>
		 /// Deletes index folder with the specific indexId, but has the option to first archive the index if it exists.
		 /// The zip archive will be placed next to the root directory for that index with a timestamp included in its name.
		 /// </summary>
		 /// <param name="fs"> <seealso cref="FileSystemAbstraction"/> this index lives in. </param>
		 /// <param name="directoryStructure"> <seealso cref="IndexDirectoryStructure"/> knowing the directory structure for the provider owning the index. </param>
		 /// <param name="indexId"> id of the index. </param>
		 /// <param name="archiveIfExists"> whether or not to archive the index before deleting it, if it exists. </param>
		 /// <returns> whether or not an archive was created. </returns>
		 /// <exception cref="IOException"> on I/O error. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static boolean deleteIndex(org.neo4j.io.fs.FileSystemAbstraction fs, org.neo4j.kernel.api.index.IndexDirectoryStructure directoryStructure, long indexId, boolean archiveIfExists) throws java.io.IOException
		 public static bool DeleteIndex( FileSystemAbstraction fs, IndexDirectoryStructure directoryStructure, long indexId, bool archiveIfExists )
		 {
			  File rootIndexDirectory = directoryStructure.DirectoryForIndex( indexId );
			  if ( archiveIfExists && fs.IsDirectory( rootIndexDirectory ) && fs.FileExists( rootIndexDirectory ) && fs.ListFiles( rootIndexDirectory ).Length > 0 )
			  {
					ZipUtils.zip( fs, rootIndexDirectory, new File( rootIndexDirectory.Parent, "archive-" + rootIndexDirectory.Name + "-" + DateTimeHelper.CurrentUnixTimeMillis() + ".zip" ) );
					return true;
			  }
			  int attempt = 0;
			  while ( attempt < 5 )
			  {
					attempt++;
					try
					{
						 fs.DeleteRecursively( rootIndexDirectory );
						 break;
					}
					catch ( Exception concurrentModificationException ) when ( concurrentModificationException is DirectoryNotEmptyException || concurrentModificationException is NoSuchFileException )
					{
						 // Looks like someone was poking around in our directory while we where deleting.
						 // Let's sleep for a bit and try again.
						 try
						 {
							  Thread.Sleep( 100 );
						 }
						 catch ( InterruptedException )
						 {
							  // Let's abandon this attempt to clean up.
							  Thread.CurrentThread.Interrupt();
							  break;
						 }
					}
			  }
			  return false;
		 }
	}

}