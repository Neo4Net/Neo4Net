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
namespace Org.Neo4j.Kernel.Api.Impl.Index.backup
{
	using IndexCommit = Org.Apache.Lucene.Index.IndexCommit;
	using SnapshotDeletionPolicy = Org.Apache.Lucene.Index.SnapshotDeletionPolicy;


	/// <summary>
	/// Iterator over Lucene index files for a particular <seealso cref="IndexCommit snapshot"/>.
	/// Applicable only to a single Lucene index partition.
	/// Internally uses <seealso cref="SnapshotDeletionPolicy.snapshot()"/> to create an <seealso cref="IndexCommit"/> that represents
	/// consistent state of the index for a particular point in time.
	/// </summary>
	public class WritableIndexSnapshotFileIterator : ReadOnlyIndexSnapshotFileIterator
	{
		 private readonly SnapshotDeletionPolicy _snapshotDeletionPolicy;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: WritableIndexSnapshotFileIterator(java.io.File indexDirectory, org.apache.lucene.index.SnapshotDeletionPolicy snapshotDeletionPolicy) throws java.io.IOException
		 internal WritableIndexSnapshotFileIterator( File indexDirectory, SnapshotDeletionPolicy snapshotDeletionPolicy ) : base( indexDirectory, snapshotDeletionPolicy.snapshot() )
		 {
			  this._snapshotDeletionPolicy = snapshotDeletionPolicy;
		 }

		 public override void Close()
		 {
			  try
			  {
					_snapshotDeletionPolicy.release( IndexCommit );
			  }
			  catch ( IOException e )
			  {
					throw new SnapshotReleaseException( "Unable to release lucene index snapshot for index in: " + IndexDirectory, e );
			  }
		 }

	}

}