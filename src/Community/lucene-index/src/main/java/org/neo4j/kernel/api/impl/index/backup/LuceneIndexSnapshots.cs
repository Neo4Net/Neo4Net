using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Api.Impl.Index.backup
{
	using DirectoryReader = Org.Apache.Lucene.Index.DirectoryReader;
	using IndexCommit = Org.Apache.Lucene.Index.IndexCommit;
	using IndexDeletionPolicy = Org.Apache.Lucene.Index.IndexDeletionPolicy;
	using IndexWriter = Org.Apache.Lucene.Index.IndexWriter;
	using SegmentInfos = Org.Apache.Lucene.Index.SegmentInfos;
	using SnapshotDeletionPolicy = Org.Apache.Lucene.Index.SnapshotDeletionPolicy;
	using Directory = org.apache.lucene.store.Directory;


	using Neo4Net.Graphdb;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.emptyResourceIterator;

	/// <summary>
	/// Create iterators over Lucene index files for a particular <seealso cref="IndexCommit index commit"/>.
	/// Applicable only to a single Lucene index partition.
	/// </summary>
	public class LuceneIndexSnapshots
	{
		 private LuceneIndexSnapshots()
		 {
		 }

		 /// <summary>
		 /// Create index snapshot iterator for a writable index. </summary>
		 /// <param name="indexFolder"> index location folder </param>
		 /// <param name="indexWriter"> index writer </param>
		 /// <returns> index file name iterator </returns>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.neo4j.graphdb.ResourceIterator<java.io.File> forIndex(java.io.File indexFolder, org.apache.lucene.index.IndexWriter indexWriter) throws java.io.IOException
		 public static ResourceIterator<File> ForIndex( File indexFolder, IndexWriter indexWriter )
		 {
			  IndexDeletionPolicy deletionPolicy = indexWriter.Config.IndexDeletionPolicy;
			  if ( deletionPolicy is SnapshotDeletionPolicy )
			  {
					SnapshotDeletionPolicy policy = ( SnapshotDeletionPolicy ) deletionPolicy;
					return HasCommits( indexWriter ) ? new WritableIndexSnapshotFileIterator( indexFolder, policy ) : emptyResourceIterator();
			  }
			  else
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
					throw new UnsupportedIndexDeletionPolicy( "Can't perform index snapshot with specified index deletion " + "policy: " + deletionPolicy.GetType().FullName + ". " + "Only " + typeof(SnapshotDeletionPolicy).FullName + " is " + "supported" );
			  }
		 }

		 /// <summary>
		 /// Create index snapshot iterator for a read only index. </summary>
		 /// <param name="indexFolder"> index location folder </param>
		 /// <param name="directory"> index directory </param>
		 /// <returns> index file name resource iterator </returns>
		 /// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static org.neo4j.graphdb.ResourceIterator<java.io.File> forIndex(java.io.File indexFolder, org.apache.lucene.store.Directory directory) throws java.io.IOException
		 public static ResourceIterator<File> ForIndex( File indexFolder, Directory directory )
		 {
			  if ( !HasCommits( directory ) )
			  {
					return emptyResourceIterator();
			  }
			  ICollection<IndexCommit> indexCommits = DirectoryReader.listCommits( directory );
			  IndexCommit indexCommit = Iterables.last( indexCommits );
			  return new ReadOnlyIndexSnapshotFileIterator( indexFolder, indexCommit );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static boolean hasCommits(org.apache.lucene.index.IndexWriter indexWriter) throws java.io.IOException
		 private static bool HasCommits( IndexWriter indexWriter )
		 {
			  Directory directory = indexWriter.Directory;
			  return HasCommits( directory );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static boolean hasCommits(org.apache.lucene.store.Directory directory) throws java.io.IOException
		 private static bool HasCommits( Directory directory )
		 {
			  return DirectoryReader.indexExists( directory ) && SegmentInfos.readLatestCommit( directory ) != null;
		 }
	}

}