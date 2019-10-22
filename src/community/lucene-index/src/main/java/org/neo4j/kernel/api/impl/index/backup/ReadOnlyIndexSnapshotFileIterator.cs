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
	using IndexCommit = Org.Apache.Lucene.Index.IndexCommit;


	using Neo4Net.GraphDb;
	using Neo4Net.Helpers.Collections;

	/// <summary>
	/// Iterator over Lucene read only index files for a particular <seealso cref="IndexCommit snapshot"/>.
	/// Applicable only to a single Lucene index partition.
	/// 
	/// </summary>
	internal class ReadOnlyIndexSnapshotFileIterator : PrefetchingIterator<File>, ResourceIterator<File>
	{
		 private readonly File _indexDirectory;
		 private readonly IEnumerator<string> _fileNames;
		 private readonly IndexCommit _indexCommit;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ReadOnlyIndexSnapshotFileIterator(java.io.File indexDirectory, org.apache.lucene.index.IndexCommit indexCommit) throws java.io.IOException
		 internal ReadOnlyIndexSnapshotFileIterator( File indexDirectory, IndexCommit indexCommit )
		 {
			  this._indexDirectory = indexDirectory;
			  this._indexCommit = indexCommit;
			  this._fileNames = this._indexCommit.FileNames.GetEnumerator();
		 }

		 protected internal override File FetchNextOrNull()
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  if ( !_fileNames.hasNext() )
			  {
					return null;
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  return new File( _indexDirectory, _fileNames.next() );
		 }

		 public override void Close()
		 {
			  // nothing by default
		 }

		 internal virtual IndexCommit IndexCommit
		 {
			 get
			 {
				  return _indexCommit;
			 }
		 }

		 internal virtual File IndexDirectory
		 {
			 get
			 {
				  return _indexDirectory;
			 }
		 }
	}

}