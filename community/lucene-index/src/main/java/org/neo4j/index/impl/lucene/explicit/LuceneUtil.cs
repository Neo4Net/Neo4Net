using System;

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
namespace Org.Neo4j.Index.impl.lucene.@explicit
{
	using IndexReader = Org.Apache.Lucene.Index.IndexReader;
	using IndexWriter = Org.Apache.Lucene.Index.IndexWriter;
	using IndexSearcher = org.apache.lucene.search.IndexSearcher;
	using NumericRangeQuery = org.apache.lucene.search.NumericRangeQuery;
	using Query = org.apache.lucene.search.Query;

	using ValueContext = Org.Neo4j.Index.lucene.ValueContext;

	public abstract class LuceneUtil
	{
		 internal static void Close( IndexWriter writer )
		 {
			  Close( ( object ) writer );
		 }

		 internal static void Close( IndexSearcher searcher )
		 {
			  Close( ( object ) searcher );
		 }

		 internal static void Close( IndexReader reader )
		 {
			  Close( ( object ) reader );
		 }

		 private static void Close( object @object )
		 {
			  if ( @object == null )
			  {
					return;
			  }

			  try
			  {
					if ( @object is IndexWriter )
					{
						 ( ( IndexWriter ) @object ).close();
					}
					else if ( @object is IndexReader )
					{
						 ( ( IndexReader ) @object ).close();
					}
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 /// <summary>
		 /// Will create a <seealso cref="Query"/> with a query for numeric ranges, that is
		 /// values that have been indexed using <seealso cref="ValueContext.indexNumeric()"/>.
		 /// It will match the type of numbers supplied to the type of values that
		 /// are indexed in the index, f.ex. long, int, float and double.
		 /// If both {@code from} and {@code to} is {@code null} then it will default
		 /// to int.
		 /// </summary>
		 /// <param name="key"> the property key to query. </param>
		 /// <param name="from"> the low end of the range (inclusive) </param>
		 /// <param name="to"> the high end of the range (inclusive) </param>
		 /// <param name="includeFrom"> whether or not {@code from} (the lower bound) is inclusive
		 /// or not. </param>
		 /// <param name="includeTo"> whether or not {@code to} (the higher bound) is inclusive
		 /// or not. </param>
		 /// <returns> a <seealso cref="Query"/> to do numeric range queries with. </returns>
		 public static Query RangeQuery( string key, Number from, Number to, bool includeFrom, bool includeTo )
		 {
			  if ( from is long? || to is long? )
			  {
					return NumericRangeQuery.newLongRange( key, from != null ? from.longValue() : 0, to != null ? to.longValue() : long.MaxValue, includeFrom, includeTo );
			  }
			  else if ( from is double? || to is double? )
			  {
					return NumericRangeQuery.newDoubleRange( key, from != null ? from.doubleValue() : 0, to != null ? to.doubleValue() : double.MaxValue, includeFrom, includeTo );
			  }
			  else if ( from is float? || to is float? )
			  {
					return NumericRangeQuery.newFloatRange( key, from != null ? from.floatValue() : 0, to != null ? to.floatValue() : float.MaxValue, includeFrom, includeTo );
			  }
			  else
			  {
					return NumericRangeQuery.newIntRange( key, from != null ? from.intValue() : 0, to != null ? to.intValue() : int.MaxValue, includeFrom, includeTo );
			  }
		 }
	}

}