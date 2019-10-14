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
namespace Neo4Net.@internal.Collector
{

	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using IndexReference = Neo4Net.@internal.Kernel.Api.IndexReference;
	using Kernel = Neo4Net.@internal.Kernel.Api.Kernel;
	using NamedToken = Neo4Net.@internal.Kernel.Api.NamedToken;
	using Read = Neo4Net.@internal.Kernel.Api.Read;
	using SchemaRead = Neo4Net.@internal.Kernel.Api.SchemaRead;
	using TokenRead = Neo4Net.@internal.Kernel.Api.TokenRead;
	using Transaction = Neo4Net.@internal.Kernel.Api.Transaction;
	using TransactionFailureException = Neo4Net.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using IndexNotFoundKernelException = Neo4Net.@internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using ConstraintDescriptor = Neo4Net.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using LoginContext = Neo4Net.@internal.Kernel.Api.security.LoginContext;
	using SilentTokenNameLookup = Neo4Net.Kernel.api.SilentTokenNameLookup;
	using Register = Neo4Net.Register.Register;
	using Registers = Neo4Net.Register.Registers;
	using EntityType = Neo4Net.Storageengine.Api.EntityType;

	/// <summary>
	/// The Graph Counts section holds all data that is available form the counts store, plus metadata
	/// about the available indexes and constraints. This essentially captures all the knowledge the
	/// planner has when planning, meaning that the data from this section could be used to investigate
	/// planning problems.
	/// </summary>
	internal sealed class GraphCountsSection
	{
		 private GraphCountsSection()
		 { // only static functionality
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static java.util.stream.Stream<RetrieveResult> retrieve(org.neo4j.internal.kernel.api.Kernel kernel, Anonymizer anonymizer) throws org.neo4j.internal.kernel.api.exceptions.TransactionFailureException, org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 internal static Stream<RetrieveResult> Retrieve( Kernel kernel, Anonymizer anonymizer )
		 {
			  using ( Transaction tx = kernel.BeginTransaction( Neo4Net.@internal.Kernel.Api.Transaction_Type.Explicit, LoginContext.AUTH_DISABLED ) )
			  {
					TokenRead tokens = tx.TokenRead();
					Read read = tx.DataRead();

					IDictionary<string, object> data = new Dictionary<string, object>();
					data["nodes"] = NodeCounts( tokens, read, anonymizer );
					data["relationships"] = RelationshipCounts( tokens, read, anonymizer );
					data["indexes"] = Indexes( tokens, tx.SchemaRead(), anonymizer );
					data["constraints"] = Constraints( tokens, tx.SchemaRead(), anonymizer );

					return Stream.of( new RetrieveResult( Sections.GRAPH_COUNTS, data ) );
			  }
		 }

		 private static IList<IDictionary<string, object>> NodeCounts( TokenRead tokens, Read read, Anonymizer anonymizer )
		 {
			  IList<IDictionary<string, object>> nodeCounts = new List<IDictionary<string, object>>();
			  IDictionary<string, object> nodeCount = new Dictionary<string, object>();

			  nodeCount["count"] = read.CountsForNodeWithoutTxState( -1 );
			  nodeCounts.Add( nodeCount );

			  tokens.LabelsGetAllTokens().forEachRemaining(t =>
			  {
			  long count = read.CountsForNodeWithoutTxState( t.id() );
			  IDictionary<string, object> labelCount = new Dictionary<string, object>();
			  labelCount.put( "label", anonymizer.Label( t.name(), t.id() ) );
			  labelCount.put( "count", count );
			  nodeCounts.Add( labelCount );
			  });

			  return nodeCounts;
		 }

		 private static IList<IDictionary<string, object>> RelationshipCounts( TokenRead tokens, Read read, Anonymizer anonymizer )
		 {
			  IList<IDictionary<string, object>> relationshipCounts = new List<IDictionary<string, object>>();
			  IDictionary<string, object> relationshipCount = new Dictionary<string, object>();
			  relationshipCount["count"] = read.CountsForRelationshipWithoutTxState( -1, -1, -1 );
			  relationshipCounts.Add( relationshipCount );

			  IList<NamedToken> labels = Iterators.asList( tokens.LabelsGetAllTokens() );

			  tokens.RelationshipTypesGetAllTokens().forEachRemaining(t =>
			  {
			  long count = read.CountsForRelationshipWithoutTxState( -1, t.id(), -1 );
			  IDictionary<string, object> relationshipTypeCount = new Dictionary<string, object>();
			  relationshipTypeCount.put( "relationshipType", anonymizer.RelationshipType( t.name(), t.id() ) );
			  relationshipTypeCount.put( "count", count );
			  relationshipCounts.Add( relationshipTypeCount );
			  foreach ( NamedToken label in labels )
			  {
				  long startCount = read.CountsForRelationshipWithoutTxState( label.id(), t.id(), -1 );
				  if ( startCount > 0 )
				  {
					  IDictionary<string, object> x = new Dictionary<string, object>();
					  x.put( "relationshipType", anonymizer.RelationshipType( t.name(), t.id() ) );
					  x.put( "startLabel", anonymizer.Label( label.name(), label.id() ) );
					  x.put( "count", startCount );
					  relationshipCounts.Add( x );
				  }
				  long endCount = read.CountsForRelationshipWithoutTxState( -1, t.id(), label.id() );
				  if ( endCount > 0 )
				  {
					  IDictionary<string, object> x = new Dictionary<string, object>();
					  x.put( "relationshipType", anonymizer.RelationshipType( t.name(), t.id() ) );
					  x.put( "endLabel", anonymizer.Label( label.name(), label.id() ) );
					  x.put( "count", endCount );
					  relationshipCounts.Add( x );
				  }
			  }
			  });

			  return relationshipCounts;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static java.util.List<java.util.Map<String,Object>> indexes(org.neo4j.internal.kernel.api.TokenRead tokens, org.neo4j.internal.kernel.api.SchemaRead schemaRead, Anonymizer anonymizer) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 private static IList<IDictionary<string, object>> Indexes( TokenRead tokens, SchemaRead schemaRead, Anonymizer anonymizer )
		 {
			  IList<IDictionary<string, object>> indexes = new List<IDictionary<string, object>>();

			  SilentTokenNameLookup tokenLookup = new SilentTokenNameLookup( tokens );

			  IEnumerator<IndexReference> iterator = schemaRead.IndexesGetAll();
			  while ( iterator.MoveNext() )
			  {
					IndexReference index = iterator.Current;

					IDictionary<string, object> data = new Dictionary<string, object>();
					data["labels"] = Map( index.Schema().EntityTokenIds, id => anonymizer.Label(tokenLookup.LabelGetName(id), id) );

					data["properties"] = Map( index.Schema().PropertyIds, id => anonymizer.PropertyKey(tokenLookup.PropertyKeyGetName(id), id) );

					Neo4Net.Register.Register_DoubleLongRegister register = Registers.newDoubleLongRegister();
					schemaRead.IndexUpdatesAndSize( index, register );
					data["totalSize"] = register.ReadSecond();
					data["updatesSinceEstimation"] = register.ReadFirst();
					schemaRead.IndexSample( index, register );
					data["estimatedUniqueSize"] = register.ReadFirst();

					indexes.Add( data );
			  }

			  return indexes;
		 }

		 private static IList<IDictionary<string, object>> Constraints( TokenRead tokens, SchemaRead schemaRead, Anonymizer anonymizer )
		 {
			  IList<IDictionary<string, object>> constraints = new List<IDictionary<string, object>>();

			  SilentTokenNameLookup tokenLookup = new SilentTokenNameLookup( tokens );

			  IEnumerator<ConstraintDescriptor> iterator = schemaRead.ConstraintsGetAll();
			  while ( iterator.MoveNext() )
			  {
					ConstraintDescriptor constraint = iterator.Current;
					EntityType entityType = constraint.Schema().entityType();
					IDictionary<string, object> data = new Dictionary<string, object>();

					data["properties"] = Map( constraint.Schema().PropertyIds, id => anonymizer.PropertyKey(tokenLookup.PropertyKeyGetName(id), id) );
					data["type"] = ConstraintType( constraint );
					int entityTokenId = constraint.Schema().EntityTokenIds[0];

					switch ( entityType.innerEnumValue )
					{
					case EntityType.InnerEnum.NODE:
						 data["label"] = anonymizer.Label( tokenLookup.LabelGetName( entityTokenId ), entityTokenId );
						 constraints.Add( data );
						 break;
					case EntityType.InnerEnum.RELATIONSHIP:
						 data["relationshipType"] = anonymizer.RelationshipType( tokenLookup.RelationshipTypeGetName( entityTokenId ), entityTokenId );
						 constraints.Add( data );
						 break;
					default:
				break;
					}
			  }

			  return constraints;
		 }

		 private static IList<string> Map( int[] ids, System.Func<int, string> f )
		 {
			  return Arrays.stream( ids ).mapToObj( f ).collect( Collectors.toList() );
		 }

		 private static string ConstraintType( ConstraintDescriptor constraint )
		 {
			  switch ( constraint.Type() )
			  {
			  case EXISTS:
					return "Existence constraint";
			  case UNIQUE:
					return "Uniqueness constraint";
			  case UNIQUE_EXISTS:
					return "Node Key";
			  default:
					throw new System.ArgumentException( "Unknown constraint type: " + constraint.Type() );
			  }
		 }
	}

}