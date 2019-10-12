using System;
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
namespace Neo4Net.Index.lucene.@unsafe.batchinsert
{

	using Neo4Net.Graphdb.index;
	using LuceneBatchInserterIndexProviderNewImpl = Neo4Net.Index.impl.lucene.@explicit.LuceneBatchInserterIndexProviderNewImpl;
	using BatchInserter = Neo4Net.@unsafe.Batchinsert.BatchInserter;
	using BatchInserterIndex = Neo4Net.@unsafe.Batchinsert.BatchInserterIndex;
	using BatchInserterIndexProvider = Neo4Net.@unsafe.Batchinsert.BatchInserterIndexProvider;

	/// <summary>
	/// The <seealso cref="org.neo4j.unsafe.batchinsert.BatchInserter"/> version of the Lucene-based indexes. Indexes
	/// created and populated using <seealso cref="org.neo4j.unsafe.batchinsert.BatchInserterIndex"/>s from this provider
	/// are compatible with the normal <seealso cref="Index"/>es. </summary>
	/// @deprecated This API will be removed in next major release. Please consider using schema indexes instead. 
	[Obsolete("This API will be removed in next major release. Please consider using schema indexes instead.")]
	public class LuceneBatchInserterIndexProvider : BatchInserterIndexProvider
	{
		 private readonly BatchInserterIndexProvider _provider;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public LuceneBatchInserterIndexProvider(final org.neo4j.unsafe.batchinsert.BatchInserter inserter)
		 [Obsolete]
		 public LuceneBatchInserterIndexProvider( BatchInserter inserter )
		 {
			  _provider = new LuceneBatchInserterIndexProviderNewImpl( inserter );
		 }

		 [Obsolete]
		 public override BatchInserterIndex NodeIndex( string indexName, IDictionary<string, string> config )
		 {
			  return _provider.nodeIndex( indexName, config );
		 }

		 [Obsolete]
		 public override BatchInserterIndex RelationshipIndex( string indexName, IDictionary<string, string> config )
		 {
			  return _provider.relationshipIndex( indexName, config );
		 }

		 [Obsolete]
		 public override void Shutdown()
		 {
			  _provider.shutdown();
		 }
	}

}