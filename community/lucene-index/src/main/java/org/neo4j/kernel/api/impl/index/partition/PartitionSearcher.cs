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
namespace Org.Neo4j.Kernel.Api.Impl.Index.partition
{
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;
	using ReferenceManager = org.apache.lucene.search.ReferenceManager;


	/// <summary>
	/// Container for <seealso cref="IndexSearcher"/> of the particular <seealso cref="AbstractIndexPartition partition"/>.
	/// Manages lifecycle of the underlying <seealso cref="IndexSearcher searcher"/>.
	/// </summary>
	public class PartitionSearcher : System.IDisposable
	{
		 private IndexSearcher _indexSearcher;
		 private ReferenceManager<IndexSearcher> _referenceManager;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public PartitionSearcher(org.apache.lucene.search.ReferenceManager<org.apache.lucene.search.IndexSearcher> referenceManager) throws java.io.IOException
		 public PartitionSearcher( ReferenceManager<IndexSearcher> referenceManager )
		 {
			  this._referenceManager = referenceManager;
			  this._indexSearcher = referenceManager.acquire();
			  this._indexSearcher.QueryCache = null;
		 }

		 public virtual IndexSearcher IndexSearcher
		 {
			 get
			 {
				  return _indexSearcher;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _referenceManager.release( _indexSearcher );
		 }
	}

}