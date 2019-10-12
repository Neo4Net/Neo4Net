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
namespace Neo4Net.Kernel.api.impl
{
	using Document = org.apache.lucene.document.Document;
	using Query = org.apache.lucene.search.Query;


	using LuceneDocumentStructure = Neo4Net.Kernel.Api.Impl.Schema.LuceneDocumentStructure;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

	public class LuceneTestUtil
	{
		 public static IList<Value[]> ValueTupleList( params object[] objects )
		 {
			  return java.util.objects.Select( LuceneTestUtil.valueTuple ).ToList();
		 }

		 public static Value[] ValueTuple( object @object )
		 {
			  return new Value[]{ Values.of( @object ) };
		 }

		 public static Document DocumentRepresentingProperties( long nodeId, params object[] objects )
		 {
			  return LuceneDocumentStructure.documentRepresentingProperties( nodeId, Values.values( objects ) );
		 }

		 public static Query NewSeekQuery( params object[] objects )
		 {
			  return LuceneDocumentStructure.newSeekQuery( Values.values( objects ) );
		 }
	}

}