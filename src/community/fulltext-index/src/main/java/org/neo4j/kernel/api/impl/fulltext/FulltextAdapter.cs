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
	using ParseException = org.apache.lucene.queryparser.classic.ParseException;


	using AnalyzerProvider = Neo4Net.GraphDb.Index.fulltext.AnalyzerProvider;
	using IndexNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.Schema.SchemaDescriptor;
	using EntityType = Neo4Net.Kernel.Api.StorageEngine.EntityType;

	public interface FulltextAdapter
	{
		 SchemaDescriptor SchemaFor( EntityType type, string[] IEntityTokens, Properties indexConfiguration, params string[] properties );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: ScoreEntityIterator query(Neo4Net.kernel.api.KernelTransaction tx, String indexName, String queryString) throws java.io.IOException, Neo4Net.Kernel.Api.Internal.Exceptions.Schema.IndexNotFoundKernelException, org.apache.lucene.queryparser.classic.ParseException;
		 ScoreEntityIterator Query( KernelTransaction tx, string indexName, string queryString );

		 void AwaitRefresh();

		 Stream<AnalyzerProvider> ListAvailableAnalyzers();
	}

}