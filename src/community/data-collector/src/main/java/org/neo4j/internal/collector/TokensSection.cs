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
namespace Neo4Net.Internal.Collector
{

	using Kernel = Neo4Net.Kernel.Api.Internal.Kernel;
	using TokenRead = Neo4Net.Kernel.Api.Internal.TokenRead;
	using Transaction = Neo4Net.Kernel.Api.Internal.Transaction;
	using TransactionFailureException = Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException;
	using LoginContext = Neo4Net.Kernel.Api.Internal.security.LoginContext;

	/// <summary>
	/// Data collector section that simply return all tokens (propertyKeys, labels and relationship types) that
	/// are known to the database.
	/// </summary>
	internal sealed class TokensSection
	{
		 private TokensSection()
		 { // only static methods
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static java.util.stream.Stream<RetrieveResult> retrieve(Neo4Net.Kernel.Api.Internal.Kernel kernel) throws Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
		 internal static Stream<RetrieveResult> Retrieve( Kernel kernel )
		 {
			  using ( Transaction tx = kernel.BeginTransaction( Neo4Net.Kernel.Api.Internal.Transaction_Type.Explicit, LoginContext.AUTH_DISABLED ) )
			  {
					TokenRead tokens = tx.TokenRead();

					IList<string> labels = new List<string>( tokens.LabelCount() );
					tokens.LabelsGetAllTokens().forEachRemaining(t => labels.Add(t.name()));

					IList<string> relationshipTypes = new List<string>( tokens.RelationshipTypeCount() );
					tokens.RelationshipTypesGetAllTokens().forEachRemaining(t => relationshipTypes.Add(t.name()));

					IList<string> propertyKeys = new List<string>( tokens.PropertyKeyCount() );
					tokens.PropertyKeyGetAllTokens().forEachRemaining(t => propertyKeys.Add(t.name()));

					IDictionary<string, object> data = new Dictionary<string, object>();
					data["labels"] = labels;
					data["relationshipTypes"] = relationshipTypes;
					data["propertyKeys"] = propertyKeys;
					return Stream.of( new RetrieveResult( Sections.TOKENS, data ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static void putTokenCounts(java.util.Map<String,Object> metaData, Neo4Net.Kernel.Api.Internal.Kernel kernel) throws Neo4Net.Kernel.Api.Internal.Exceptions.TransactionFailureException
		 internal static void PutTokenCounts( IDictionary<string, object> metaData, Kernel kernel )
		 {
			  using ( Transaction tx = kernel.BeginTransaction( Neo4Net.Kernel.Api.Internal.Transaction_Type.Explicit, LoginContext.AUTH_DISABLED ) )
			  {
					TokenRead tokens = tx.TokenRead();
					metaData["labelCount"] = tokens.LabelCount();
					metaData["relationshipTypeCount"] = tokens.RelationshipTypeCount();
					metaData["propertyKeyCount"] = tokens.PropertyKeyCount();
					tx.Success();
			  }
		 }
	}

}