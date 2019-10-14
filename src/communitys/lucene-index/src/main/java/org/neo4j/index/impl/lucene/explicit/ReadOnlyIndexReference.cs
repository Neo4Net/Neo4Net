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
namespace Neo4Net.Index.impl.lucene.@explicit
{
	using IndexWriter = Org.Apache.Lucene.Index.IndexWriter;
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;

	public class ReadOnlyIndexReference : IndexReference
	{

		 internal ReadOnlyIndexReference( IndexIdentifier identifier, IndexSearcher searcher ) : base( identifier, searcher )
		 {
		 }

		 public override IndexWriter Writer
		 {
			 get
			 {
				  throw new System.NotSupportedException( "Read only indexes do not have index writers." );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void dispose() throws java.io.IOException
		 public override void Dispose()
		 {
			 lock ( this )
			 {
				  DisposeSearcher();
			 }
		 }

		 public override bool CheckAndClearStale()
		 {
			  return false;
		 }

		 public override void SetStale()
		 {
			  throw new System.NotSupportedException( "Read only indexes can't be marked as stale." );
		 }

	}

}