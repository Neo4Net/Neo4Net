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
namespace Neo4Net.Kernel.Api.Impl.Fulltext
{
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;

	using PartitionSearcher = Neo4Net.Kernel.Api.Impl.Index.partition.PartitionSearcher;

	internal class PartitionSearcherReference : SearcherReference
	{
		 private readonly PartitionSearcher _partitionSearcher;

		 internal PartitionSearcherReference( PartitionSearcher partitionSearcher )
		 {
			  this._partitionSearcher = partitionSearcher;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _partitionSearcher.Dispose();
		 }

		 public virtual IndexSearcher IndexSearcher
		 {
			 get
			 {
				  return _partitionSearcher.IndexSearcher;
			 }
		 }
	}

}