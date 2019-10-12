using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Api.Impl.Fulltext
{
	using Analyzer = org.apache.lucene.analysis.Analyzer;
	using ParseException = org.apache.lucene.queryparser.classic.ParseException;


	using IOUtils = Org.Neo4j.Io.IOUtils;
	using PartitionSearcher = Org.Neo4j.Kernel.Api.Impl.Index.partition.PartitionSearcher;
	using IndexReaderCloseException = Org.Neo4j.Kernel.Api.Impl.Schema.reader.IndexReaderCloseException;
	using TokenHolder = Org.Neo4j.Kernel.impl.core.TokenHolder;
	using Value = Org.Neo4j.Values.Storable.Value;

	/// <summary>
	/// Index reader that is able to read/sample multiple partitions of a partitioned Lucene index.
	/// Internally uses multiple <seealso cref="SimpleFulltextIndexReader"/>s for individual partitions.
	/// </summary>
	/// <seealso cref= SimpleFulltextIndexReader </seealso>
	internal class PartitionedFulltextIndexReader : FulltextIndexReader
	{

		 private readonly IList<FulltextIndexReader> _indexReaders;

		 internal PartitionedFulltextIndexReader( IList<PartitionSearcher> partitionSearchers, string[] properties, Analyzer analyzer, TokenHolder propertyKeyTokenHolder ) : this( partitionSearchers.Select( PartitionSearcherReference::new ).Select( searcher -> new SimpleFulltextIndexReader( searcher, properties, analyzer, propertyKeyTokenHolder ) ).ToList() )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		 }

		 private PartitionedFulltextIndexReader( IList<FulltextIndexReader> readers )
		 {
			  this._indexReaders = readers;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public ScoreEntityIterator query(String query) throws org.apache.lucene.queryparser.classic.ParseException
		 public override ScoreEntityIterator Query( string query )
		 {
			  return PartitionedQuery( query );
		 }

		 public override void Close()
		 {
			  try
			  {
					IOUtils.closeAll( _indexReaders );
			  }
			  catch ( IOException e )
			  {
					throw new IndexReaderCloseException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private ScoreEntityIterator partitionedQuery(String query) throws org.apache.lucene.queryparser.classic.ParseException
		 private ScoreEntityIterator PartitionedQuery( string query )
		 {
			  IList<ScoreEntityIterator> results = new List<ScoreEntityIterator>();
			  foreach ( FulltextIndexReader indexReader in _indexReaders )
			  {
					results.Add( indexReader.Query( query ) );
			  }
			  return ScoreEntityIterator.MergeIterators( results );
		 }

		 public override long CountIndexedNodes( long nodeId, int[] propertyKeyIds, params Value[] propertyValues )
		 {
			  return _indexReaders.Select( reader => reader.countIndexedNodes( nodeId, propertyKeyIds, propertyValues ) ).Sum();
		 }
	}

}