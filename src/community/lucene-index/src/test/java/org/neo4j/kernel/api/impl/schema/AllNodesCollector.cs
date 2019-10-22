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
namespace Neo4Net.Kernel.Api.Impl.Schema
{
	using LeafReader = Org.Apache.Lucene.Index.LeafReader;
	using LeafReaderContext = Org.Apache.Lucene.Index.LeafReaderContext;
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;
	using Query = org.apache.lucene.search.Query;
	using SearcherFactory = org.apache.lucene.search.SearcherFactory;
	using SearcherManager = org.apache.lucene.search.SearcherManager;
	using SimpleCollector = org.apache.lucene.search.SimpleCollector;
	using Directory = org.apache.lucene.store.Directory;


	using Value = Neo4Net.Values.Storable.Value;

	public class AllNodesCollector : SimpleCollector
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static java.util.List<long> getAllNodes(org.apache.lucene.store.Directory directory, org.Neo4Net.values.storable.Value propertyValue) throws java.io.IOException
		 public static IList<long> GetAllNodes( Directory directory, Value propertyValue )
		 {
			  using ( SearcherManager manager = new SearcherManager( directory, new SearcherFactory() ) )
			  {
					IndexSearcher searcher = manager.acquire();
					Query query = LuceneDocumentStructure.NewSeekQuery( propertyValue );
					AllNodesCollector collector = new AllNodesCollector();
					searcher.search( query, collector );
					return collector._nodeIds;
			  }
		 }

		 private readonly IList<long> _nodeIds = new List<long>();
		 private LeafReader _reader;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void collect(int doc) throws java.io.IOException
		 public override void Collect( int doc )
		 {
			  _nodeIds.Add( LuceneDocumentStructure.GetNodeId( _reader.document( doc ) ) );
		 }

		 public override bool NeedsScores()
		 {
			  return false;
		 }

		 protected internal override void DoSetNextReader( LeafReaderContext context )
		 {
			  this._reader = context.reader();
		 }
	}

}