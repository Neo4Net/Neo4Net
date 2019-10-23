using System;
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
namespace Neo4Net.Kernel.builtinprocs
{

	using PrimitiveLongResourceIterator = Neo4Net.Collections.PrimitiveLongResourceIterator;
	using DependencyResolver = Neo4Net.GraphDb.DependencyResolver;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Neo4Net.GraphDb.index;
	using IndexManager = Neo4Net.GraphDb.index.IndexManager;
	using RelationshipIndex = Neo4Net.GraphDb.index.RelationshipIndex;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using MapUtil = Neo4Net.Helpers.Collections.MapUtil;
	using Neo4Net.Helpers.Collections;
	using IndexReference = Neo4Net.Kernel.Api.Internal.IndexReference;
	using InternalIndexState = Neo4Net.Kernel.Api.Internal.InternalIndexState;
	using NodeExplicitIndexCursor = Neo4Net.Kernel.Api.Internal.NodeExplicitIndexCursor;
	using RelationshipExplicitIndexCursor = Neo4Net.Kernel.Api.Internal.RelationshipExplicitIndexCursor;
	using SchemaReadCore = Neo4Net.Kernel.Api.Internal.SchemaReadCore;
	using TokenNameLookup = Neo4Net.Kernel.Api.Internal.TokenNameLookup;
	using TokenRead = Neo4Net.Kernel.Api.Internal.TokenRead;
	using KernelException = Neo4Net.Kernel.Api.Internal.Exceptions.KernelException;
	using ProcedureException = Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
	using IndexNotFoundKernelException = Neo4Net.Kernel.Api.Internal.Exceptions.schema.IndexNotFoundKernelException;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.schema.SchemaDescriptor;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using SilentTokenNameLookup = Neo4Net.Kernel.api.SilentTokenNameLookup;
	using Statement = Neo4Net.Kernel.api.Statement;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using Neo4Net.Kernel.Impl.Api;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Context = Neo4Net.Procedure.Context;
	using Description = Neo4Net.Procedure.Description;
	using Mode = Neo4Net.Procedure.Mode;
	using Name = Neo4Net.Procedure.Name;
	using Procedure = Neo4Net.Procedure.Procedure;
	using EntityType = Neo4Net.Kernel.Api.StorageEngine.EntityType;
	using PopulationProgress = Neo4Net.Kernel.Api.StorageEngine.schema.PopulationProgress;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.procedure.Mode.READ;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.procedure.Mode.SCHEMA;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.procedure.Mode.WRITE;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({"unused", "WeakerAccess"}) public class BuiltInProcedures
	public class BuiltInProcedures
	{
		 private const int NOT_EXISTING_INDEX_ID = -1;
		 public const string EXPLICIT_INDEX_DEPRECATION = "This procedure is deprecated by the schema and full-text indexes, and will be removed in 4.0.";
		 public const string DB_SCHEMA_DEPRECATION = "This procedure is deprecated by the db.schema.visualization procedure, and will be removed in 4.0.";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.Neo4Net.kernel.api.KernelTransaction tx;
		 public KernelTransaction Tx;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.Neo4Net.graphdb.DependencyResolver resolver;
		 public DependencyResolver Resolver;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.Neo4Net.kernel.internal.GraphDatabaseAPI graphDatabaseAPI;
		 public GraphDatabaseAPI GraphDatabaseAPI;

		 [Description("List all labels in the database."), Procedure(name : "db.labels", mode : READ)]
		 public virtual Stream<LabelResult> ListLabels()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  IList<LabelResult> labelResults = new IList<LabelResult> { TokenAccess.LABELS.inUse( Tx ).map( LabelResult::new ) };
			  return labelResults.stream();
		 }

		 [Description("List all property keys in the database."), Procedure(name : "db.propertyKeys", mode : READ)]
		 public virtual Stream<PropertyKeyResult> ListPropertyKeys()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  IList<PropertyKeyResult> propertyKeys = new IList<PropertyKeyResult> { TokenAccess.PROPERTY_KEYS.inUse( Tx ).map( PropertyKeyResult::new ) };
			  return propertyKeys.stream();
		 }

		 [Description("List all relationship types in the database."), Procedure(name : "db.relationshipTypes", mode : READ)]
		 public virtual Stream<RelationshipTypeResult> ListRelationshipTypes()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  IList<RelationshipTypeResult> relationshipTypes = new IList<RelationshipTypeResult> { TokenAccess.RELATIONSHIP_TYPES.inUse( Tx ).map( RelationshipTypeResult::new ) };
			  return relationshipTypes.stream();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.stream.Stream<IndexResult> listIndexes() throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
		 [Description("List all indexes in the database."), Procedure(name : "db.indexes", mode : READ)]
		 public virtual Stream<IndexResult> ListIndexes()
		 {
			  using ( Statement ignore = Tx.acquireStatement() )
			  {
					TokenRead tokenRead = Tx.tokenRead();
					TokenNameLookup tokens = new SilentTokenNameLookup( tokenRead );
					IndexingService indexingService = Resolver.resolveDependency( typeof( IndexingService ) );

					SchemaReadCore schemaRead = Tx.schemaRead().snapshot();
					IList<IndexReference> indexes = new IList<IndexReference> { schemaRead.IndexesGetAll() };
					indexes.sort( System.Collections.IComparer.comparing( a => a.userDescription( tokens ) ) );

					List<IndexResult> result = new List<IndexResult>();
					foreach ( IndexReference index in indexes )
					{
						 IndexType type = IndexType.getIndexTypeOf( index );

						 SchemaDescriptor schema = index.Schema();
						 long indexId = GetIndexId( indexingService, schema );
						 IList<string> tokenNames = Arrays.asList( tokens.EntityTokensGetNames( Schema.entityType(), Schema.EntityTokenIds ) );
						 IList<string> propertyNames = propertyNames( tokens, index );
						 string description = "INDEX ON " + Schema.userDescription( tokens );
						 IndexStatus status = GetIndexStatus( schemaRead, index );
						 IDictionary<string, string> providerDescriptorMap = IndexProviderDescriptorMap( schemaRead.Index( schema ) );
						 result.Add( new IndexResult( indexId, description, index.Name(), tokenNames, propertyNames, status.State, type.typeName(), status.PopulationProgress, providerDescriptorMap, status.FailureMessage ) );
					}
					return result.stream();
			  }
		 }

		 private static IndexStatus GetIndexStatus( SchemaReadCore schemaRead, IndexReference index )
		 {
			  IndexStatus status = new IndexStatus();
			  try
			  {
					InternalIndexState internalIndexState = schemaRead.IndexGetState( index );
					status.State = internalIndexState.ToString();
					PopulationProgress progress = schemaRead.IndexGetPopulationProgress( index );
					status.PopulationProgress = progress.ToIndexPopulationProgress().CompletedPercentage;
					status.FailureMessage = internalIndexState == InternalIndexState.FAILED ? schemaRead.IndexGetFailure( index ) : "";
			  }
			  catch ( IndexNotFoundKernelException )
			  {
					status.State = "NOT FOUND";
					status.PopulationProgress = 0;
					status.FailureMessage = "Index not found. It might have been concurrently dropped.";
			  }
			  return status;
		 }

		 private class IndexStatus
		 {
			  internal string State;
			  internal string FailureMessage;
			  internal float PopulationProgress;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Wait for an index to come online (for example: CALL db.awaitIndex(\":Person(name)\")).") @Procedure(name = "db.awaitIndex", mode = READ) public void awaitIndex(@Name("index") String index, @Name(value = "timeOutSeconds", defaultValue = "300") long timeout) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Wait for an index to come online (for example: CALL db.awaitIndex(\":Person(name)\"))."), Procedure(name : "db.awaitIndex", mode : READ)]
		 public virtual void AwaitIndex( string index, long timeout )
		 {
			  using ( IndexProcedures indexProcedures = indexProcedures() )
			  {
					indexProcedures.AwaitIndexByPattern( index, timeout, TimeUnit.SECONDS );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Wait for all indexes to come online (for example: CALL db.awaitIndexes(\"500\")).") @Procedure(name = "db.awaitIndexes", mode = READ) public void awaitIndexes(@Name(value = "timeOutSeconds", defaultValue = "300") long timeout)
		 [Description("Wait for all indexes to come online (for example: CALL db.awaitIndexes(\"500\"))."), Procedure(name : "db.awaitIndexes", mode : READ)]
		 public virtual void AwaitIndexes( long timeout )
		 {
			  GraphDatabaseAPI.schema().awaitIndexesOnline(timeout, TimeUnit.SECONDS);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Schedule resampling of an index (for example: CALL db.resampleIndex(\":Person(name)\")).") @Procedure(name = "db.resampleIndex", mode = READ) public void resampleIndex(@Name("index") String index) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Schedule resampling of an index (for example: CALL db.resampleIndex(\":Person(name)\"))."), Procedure(name : "db.resampleIndex", mode : READ)]
		 public virtual void ResampleIndex( string index )
		 {
			  using ( IndexProcedures indexProcedures = indexProcedures() )
			  {
					indexProcedures.ResampleIndex( index );
			  }
		 }

		 [Description("Schedule resampling of all outdated indexes."), Procedure(name : "db.resampleOutdatedIndexes", mode : READ)]
		 public virtual void ResampleOutdatedIndexes()
		 {
			  using ( IndexProcedures indexProcedures = indexProcedures() )
			  {
					indexProcedures.ResampleOutdatedIndexes();
			  }
		 }

		 [Procedure(name : "db.schema.nodeTypeProperties", mode : Neo4Net.Procedure.Mode.READ), Description("Show the derived property schema of the nodes in tabular form.")]
		 public virtual Stream<NodePropertySchemaInfoResult> NodePropertySchema()
		 {
			  return ( new SchemaCalculator( Tx ) ).calculateTabularResultStreamForNodes();
		 }

		 [Procedure(name : "db.schema.relTypeProperties", mode : Neo4Net.Procedure.Mode.READ), Description("Show the derived property schema of the relationships in tabular form.")]
		 public virtual Stream<RelationshipPropertySchemaInfoResult> RelationshipPropertySchema()
		 {
			  return ( new SchemaCalculator( Tx ) ).calculateTabularResultStreamForRels();
		 }

		 [Obsolete, Description("Show the schema of the data."), Procedure(name : "db.schema", mode : READ, deprecatedBy : DB_SCHEMA_DEPRECATION)]
		 public virtual Stream<SchemaProcedure.GraphResult> Schema()
		 {
			  return SchemaVisualization();
		 }

		 [Description("Visualize the schema of the data. Replaces db.schema."), Procedure(name : "db.schema.visualization", mode : READ)]
		 public virtual Stream<SchemaProcedure.GraphResult> SchemaVisualization()
		 {
			  return Stream.of( ( new SchemaProcedure( GraphDatabaseAPI, Tx ) ).buildSchemaGraph() );
		 }

		 [Description("List all constraints in the database."), Procedure(name : "db.constraints", mode : READ)]
		 public virtual Stream<ConstraintResult> ListConstraints()
		 {

			  SchemaReadCore schemaRead = Tx.schemaRead().snapshot();
			  TokenNameLookup tokens = new SilentTokenNameLookup( Tx.tokenRead() );

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return asList( schemaRead.ConstraintsGetAll() ).Select(constraint => constraint.prettyPrint(tokens)).OrderBy(c => c).Select(ConstraintResult::new);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Create a schema index with specified index provider (for example: CALL db.createIndex(\":Person(name)\", \"lucene+native-2.0\")) - " + "YIELD index, providerName, status") @Procedure(name = "db.createIndex", mode = SCHEMA) public java.util.stream.Stream<SchemaIndexInfo> createIndex(@Name("index") String index, @Name("providerName") String providerName) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Create a schema index with specified index provider (for example: CALL db.createIndex(\":Person(name)\", \"lucene+native-2.0\")) - " + "YIELD index, providerName, status"), Procedure(name : "db.createIndex", mode : SCHEMA)]
		 public virtual Stream<SchemaIndexInfo> CreateIndex( string index, string providerName )
		 {
			  using ( IndexProcedures indexProcedures = indexProcedures() )
			  {
					return indexProcedures.CreateIndex( index, providerName );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Create a unique property constraint with index backed by specified index provider " + "(for example: CALL db.createUniquePropertyConstraint(\":Person(name)\", \"lucene+native-2.0\")) - " + "YIELD index, providerName, status") @Procedure(name = "db.createUniquePropertyConstraint", mode = SCHEMA) public java.util.stream.Stream<BuiltInProcedures.SchemaIndexInfo> createUniquePropertyConstraint(@Name("index") String index, @Name("providerName") String providerName) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Create a unique property constraint with index backed by specified index provider " + "(for example: CALL db.createUniquePropertyConstraint(\":Person(name)\", \"lucene+native-2.0\")) - " + "YIELD index, providerName, status"), Procedure(name : "db.createUniquePropertyConstraint", mode : SCHEMA)]
		 public virtual Stream<BuiltInProcedures.SchemaIndexInfo> CreateUniquePropertyConstraint( string index, string providerName )
		 {
			  using ( IndexProcedures indexProcedures = indexProcedures() )
			  {
					return indexProcedures.CreateUniquePropertyConstraint( index, providerName );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @Description("Get node from explicit index. Replaces `START n=node:nodes(key = 'A')`") @Procedure(name = "db.index.explicit.seekNodes", mode = READ, deprecatedBy = EXPLICIT_INDEX_DEPRECATION) public java.util.stream.Stream<NodeResult> nodeManualIndexSeek(@Name("indexName") String explicitIndexName, @Name("key") String key, @Name("value") Object value) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Obsolete, Description("Get node from explicit index. Replaces `START n=node:nodes(key = 'A')`"), Procedure(name : "db.index.explicit.seekNodes", mode : READ, deprecatedBy : EXPLICIT_INDEX_DEPRECATION)]
		 public virtual Stream<NodeResult> NodeManualIndexSeek( string explicitIndexName, string key, object value )
		 {
			  try
			  {
					  using ( Statement ignore = Tx.acquireStatement() )
					  {
						NodeExplicitIndexCursor cursor = Tx.cursors().allocateNodeExplicitIndexCursor();
						Tx.indexRead().nodeExplicitIndexLookup(cursor, explicitIndexName, key, value);
      
						return ToStream( cursor, id => new NodeResult( GraphDatabaseAPI.getNodeById( id ) ) );
					  }
			  }
			  catch ( KernelException )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_LegacyIndex.LegacyIndexNotFound, "Node index %s not found", explicitIndexName );
			  }

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @Description("Search nodes in explicit index. Replaces `START n=node:nodes('key:foo*')`") @Procedure(name = "db.index.explicit.searchNodes", mode = READ, deprecatedBy = EXPLICIT_INDEX_DEPRECATION) public java.util.stream.Stream<WeightedNodeResult> nodeManualIndexSearch(@Name("indexName") String manualIndexName, @Name("query") Object query) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Obsolete, Description("Search nodes in explicit index. Replaces `START n=node:nodes('key:foo*')`"), Procedure(name : "db.index.explicit.searchNodes", mode : READ, deprecatedBy : EXPLICIT_INDEX_DEPRECATION)]
		 public virtual Stream<WeightedNodeResult> NodeManualIndexSearch( string manualIndexName, object query )
		 {
			  try
			  {
					  using ( Statement ignore = Tx.acquireStatement() )
					  {
						NodeExplicitIndexCursor cursor = Tx.cursors().allocateNodeExplicitIndexCursor();
						Tx.indexRead().nodeExplicitIndexQuery(cursor, manualIndexName, query);
						return ToWeightedNodeResultStream( cursor );
					  }
			  }
			  catch ( KernelException )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_LegacyIndex.LegacyIndexNotFound, "Node index %s not found", manualIndexName );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @Description("Get relationship from explicit index. Replaces `START r=relationship:relIndex(key = 'A')`") @Procedure(name = "db.index.explicit.seekRelationships", mode = READ, deprecatedBy = EXPLICIT_INDEX_DEPRECATION) public java.util.stream.Stream<RelationshipResult> relationshipManualIndexSeek(@Name("indexName") String manualIndexName, @Name("key") String key, @Name("value") Object value) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Obsolete, Description("Get relationship from explicit index. Replaces `START r=relationship:relIndex(key = 'A')`"), Procedure(name : "db.index.explicit.seekRelationships", mode : READ, deprecatedBy : EXPLICIT_INDEX_DEPRECATION)]
		 public virtual Stream<RelationshipResult> RelationshipManualIndexSeek( string manualIndexName, string key, object value )
		 {
			  try
			  {
					  using ( Statement ignore = Tx.acquireStatement() )
					  {
						RelationshipExplicitIndexCursor cursor = Tx.cursors().allocateRelationshipExplicitIndexCursor();
						Tx.indexRead().relationshipExplicitIndexLookup(cursor, manualIndexName, key, value, -1, -1);
						return ToStream( cursor, id => new RelationshipResult( GraphDatabaseAPI.getRelationshipById( id ) ) );
					  }
			  }
			  catch ( KernelException )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_LegacyIndex.LegacyIndexNotFound, "Relationship index %s not found", manualIndexName );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @Description("Search relationship in explicit index. Replaces `START r=relationship:relIndex('key:foo*')`") @Procedure(name = "db.index.explicit.searchRelationships", mode = READ, deprecatedBy = EXPLICIT_INDEX_DEPRECATION) public java.util.stream.Stream<WeightedRelationshipResult> relationshipManualIndexSearch(@Name("indexName") String manualIndexName, @Name("query") Object query) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Obsolete, Description("Search relationship in explicit index. Replaces `START r=relationship:relIndex('key:foo*')`"), Procedure(name : "db.index.explicit.searchRelationships", mode : READ, deprecatedBy : EXPLICIT_INDEX_DEPRECATION)]
		 public virtual Stream<WeightedRelationshipResult> RelationshipManualIndexSearch( string manualIndexName, object query )
		 {
			  try
			  {
					  using ( Statement ignore = Tx.acquireStatement() )
					  {
						RelationshipExplicitIndexCursor cursor = Tx.cursors().allocateRelationshipExplicitIndexCursor();
						Tx.indexRead().relationshipExplicitIndexQuery(cursor, manualIndexName, query, -1, -1);
						return ToWeightedRelationshipResultStream( cursor );
					  }
			  }
			  catch ( KernelException )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_LegacyIndex.LegacyIndexNotFound, "Relationship index %s not found", manualIndexName );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @Description("Search relationship in explicit index, starting at the node 'in'.") @Procedure(name = "db.index.explicit.searchRelationshipsIn", mode = READ, deprecatedBy = EXPLICIT_INDEX_DEPRECATION) public java.util.stream.Stream<WeightedRelationshipResult> relationshipManualIndexSearchWithBoundStartNode(@Name("indexName") String indexName, @Name("in") org.Neo4Net.graphdb.Node in, @Name("query") Object query) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Obsolete, Description("Search relationship in explicit index, starting at the node 'in'."), Procedure(name : "db.index.explicit.searchRelationshipsIn", mode : READ, deprecatedBy : EXPLICIT_INDEX_DEPRECATION)]
		 public virtual Stream<WeightedRelationshipResult> RelationshipManualIndexSearchWithBoundStartNode( string indexName, Node @in, object query )
		 {
			  try
			  {
					  using ( Statement ignore = Tx.acquireStatement() )
					  {
						RelationshipExplicitIndexCursor cursor = Tx.cursors().allocateRelationshipExplicitIndexCursor();
						Tx.indexRead().relationshipExplicitIndexQuery(cursor, indexName, query, @in.Id, -1);
      
						return ToWeightedRelationshipResultStream( cursor );
					  }
			  }
			  catch ( KernelException )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_LegacyIndex.LegacyIndexNotFound, "Relationship index %s not found", indexName );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @Description("Search relationship in explicit index, ending at the node 'out'.") @Procedure(name = "db.index.explicit.searchRelationshipsOut", mode = READ, deprecatedBy = EXPLICIT_INDEX_DEPRECATION) public java.util.stream.Stream<WeightedRelationshipResult> relationshipManualIndexSearchWithBoundEndNode(@Name("indexName") String indexName, @Name("out") org.Neo4Net.graphdb.Node out, @Name("query") Object query) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Obsolete, Description("Search relationship in explicit index, ending at the node 'out'."), Procedure(name : "db.index.explicit.searchRelationshipsOut", mode : READ, deprecatedBy : EXPLICIT_INDEX_DEPRECATION)]
		 public virtual Stream<WeightedRelationshipResult> RelationshipManualIndexSearchWithBoundEndNode( string indexName, Node @out, object query )
		 {
			  try
			  {
					  using ( Statement ignore = Tx.acquireStatement() )
					  {
						RelationshipExplicitIndexCursor cursor = Tx.cursors().allocateRelationshipExplicitIndexCursor();
						Tx.indexRead().relationshipExplicitIndexQuery(cursor, indexName, query, -1, @out.Id);
						return ToWeightedRelationshipResultStream( cursor );
					  }
			  }
			  catch ( KernelException )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_LegacyIndex.LegacyIndexNotFound, "Relationship index %s not found", indexName );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @Description("Search relationship in explicit index, starting at the node 'in' and ending at 'out'.") @Procedure(name = "db.index.explicit.searchRelationshipsBetween", mode = READ, deprecatedBy = EXPLICIT_INDEX_DEPRECATION) public java.util.stream.Stream<WeightedRelationshipResult> relationshipManualIndexSearchWithBoundNodes(@Name("indexName") String indexName, @Name("in") org.Neo4Net.graphdb.Node in, @Name("out") org.Neo4Net.graphdb.Node out, @Name("query") Object query) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Obsolete, Description("Search relationship in explicit index, starting at the node 'in' and ending at 'out'."), Procedure(name : "db.index.explicit.searchRelationshipsBetween", mode : READ, deprecatedBy : EXPLICIT_INDEX_DEPRECATION)]
		 public virtual Stream<WeightedRelationshipResult> RelationshipManualIndexSearchWithBoundNodes( string indexName, Node @in, Node @out, object query )
		 {
			  try
			  {
					  using ( Statement ignore = Tx.acquireStatement() )
					  {
						RelationshipExplicitIndexCursor cursor = Tx.cursors().allocateRelationshipExplicitIndexCursor();
						Tx.indexRead().relationshipExplicitIndexQuery(cursor, indexName, query, @in.Id, @out.Id);
      
						return ToWeightedRelationshipResultStream( cursor );
					  }
			  }
			  catch ( KernelException )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_LegacyIndex.LegacyIndexNotFound, "Relationship index %s not found", indexName );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @Description("Get node from explicit automatic index. Replaces `START n=node:node_auto_index(key = 'A')`") @Procedure(name = "db.index.explicit.auto.seekNodes", mode = READ, deprecatedBy = EXPLICIT_INDEX_DEPRECATION) public java.util.stream.Stream<NodeResult> nodeAutoIndexSeek(@Name("key") String key, @Name("value") Object value)
		 [Obsolete, Description("Get node from explicit automatic index. Replaces `START n=node:node_auto_index(key = 'A')`"), Procedure(name : "db.index.explicit.auto.seekNodes", mode : READ, deprecatedBy : EXPLICIT_INDEX_DEPRECATION)]
		 public virtual Stream<NodeResult> NodeAutoIndexSeek( string key, object value )
		 {
			  try
			  {
					  using ( Statement ignore = Tx.acquireStatement() )
					  {
						NodeExplicitIndexCursor cursor = Tx.cursors().allocateNodeExplicitIndexCursor();
						Tx.indexRead().nodeExplicitIndexLookup(cursor, "node_auto_index", key, value);
						return ToStream( cursor, id => new NodeResult( GraphDatabaseAPI.getNodeById( id ) ) );
					  }
			  }
			  catch ( KernelException )
			  {
					// auto index will not exist if no nodes have been added that match the auto-index rules
					return Stream.empty();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @Description("Search nodes in explicit automatic index. Replaces `START n=node:node_auto_index('key:foo*')`") @Procedure(name = "db.index.explicit.auto.searchNodes", mode = READ, deprecatedBy = EXPLICIT_INDEX_DEPRECATION) public java.util.stream.Stream<WeightedNodeResult> nodeAutoIndexSearch(@Name("query") Object query)
		 [Obsolete, Description("Search nodes in explicit automatic index. Replaces `START n=node:node_auto_index('key:foo*')`"), Procedure(name : "db.index.explicit.auto.searchNodes", mode : READ, deprecatedBy : EXPLICIT_INDEX_DEPRECATION)]
		 public virtual Stream<WeightedNodeResult> NodeAutoIndexSearch( object query )
		 {
			  try
			  {
					  using ( Statement ignore = Tx.acquireStatement() )
					  {
						NodeExplicitIndexCursor cursor = Tx.cursors().allocateNodeExplicitIndexCursor();
						Tx.indexRead().nodeExplicitIndexQuery(cursor, "node_auto_index", query);
      
						return ToWeightedNodeResultStream( cursor );
					  }
			  }
			  catch ( KernelException )
			  {
					// auto index will not exist if no nodes have been added that match the auto-index rules
					return Stream.empty();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @Description("Get relationship from explicit automatic index. Replaces `START r=relationship:relationship_auto_index(key " + "= 'A')`") @Procedure(name = "db.index.explicit.auto.seekRelationships", mode = READ, deprecatedBy = EXPLICIT_INDEX_DEPRECATION) public java.util.stream.Stream<RelationshipResult> relationshipAutoIndexSeek(@Name("key") String key, @Name("value") Object value)
		 [Obsolete, Description("Get relationship from explicit automatic index. Replaces `START r=relationship:relationship_auto_index(key " + "= 'A')`"), Procedure(name : "db.index.explicit.auto.seekRelationships", mode : READ, deprecatedBy : EXPLICIT_INDEX_DEPRECATION)]
		 public virtual Stream<RelationshipResult> RelationshipAutoIndexSeek( string key, object value )
		 {
			  try
			  {
					  using ( Statement ignore = Tx.acquireStatement() )
					  {
						RelationshipExplicitIndexCursor cursor = Tx.cursors().allocateRelationshipExplicitIndexCursor();
						Tx.indexRead().relationshipExplicitIndexLookup(cursor, "relationship_auto_index", key, value, -1, -1);
						return ToStream( cursor, id => new RelationshipResult( GraphDatabaseAPI.getRelationshipById( id ) ) );
					  }
			  }
			  catch ( KernelException )
			  {
					// auto index will not exist if no relationships have been added that match the auto-index rules
					return Stream.empty();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @Description("Search relationship in explicit automatic index. Replaces `START r=relationship:relationship_auto_index" + "('key:foo*')`") @Procedure(name = "db.index.explicit.auto.searchRelationships", mode = READ, deprecatedBy = EXPLICIT_INDEX_DEPRECATION) public java.util.stream.Stream<WeightedRelationshipResult> relationshipAutoIndexSearch(@Name("query") Object query)
		 [Obsolete, Description("Search relationship in explicit automatic index. Replaces `START r=relationship:relationship_auto_index" + "('key:foo*')`"), Procedure(name : "db.index.explicit.auto.searchRelationships", mode : READ, deprecatedBy : EXPLICIT_INDEX_DEPRECATION)]
		 public virtual Stream<WeightedRelationshipResult> RelationshipAutoIndexSearch( object query )
		 {
			  try
			  {
					  using ( Statement ignore = Tx.acquireStatement() )
					  {
						RelationshipExplicitIndexCursor cursor = Tx.cursors().allocateRelationshipExplicitIndexCursor();
						Tx.indexRead().relationshipExplicitIndexQuery(cursor, "relationship_auto_index", query, -1, -1);
						return ToWeightedRelationshipResultStream( cursor );
					  }
			  }
			  catch ( KernelException )
			  {
					// auto index will not exist if no relationships have been added that match the auto-index rules
					return Stream.empty();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @Description("Get or create a node explicit index - YIELD type,name,config") @Procedure(name = "db.index.explicit.forNodes", mode = WRITE, deprecatedBy = EXPLICIT_INDEX_DEPRECATION) public java.util.stream.Stream<ExplicitIndexInfo> nodeManualIndex(@Name("indexName") String explicitIndexName, @Name(value = "config", defaultValue = "") java.util.Map<String,String> config)
		 [Obsolete, Description("Get or create a node explicit index - YIELD type,name,config"), Procedure(name : "db.index.explicit.forNodes", mode : WRITE, deprecatedBy : EXPLICIT_INDEX_DEPRECATION)]
		 public virtual Stream<ExplicitIndexInfo> NodeManualIndex( string explicitIndexName, IDictionary<string, string> config )
		 {
			  IndexManager mgr = GraphDatabaseAPI.index();
			  Index<Node> index;
			  if ( config == null || config.Count == 0 )
			  {
					index = mgr.ForNodes( explicitIndexName );
			  }
			  else
			  {
					index = mgr.ForNodes( explicitIndexName, config );
			  }
			  return Stream.of( new ExplicitIndexInfo( "NODE", explicitIndexName, mgr.GetConfiguration( index ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @Description("Get or create a relationship explicit index - YIELD type,name,config") @Procedure(name = "db.index.explicit.forRelationships", mode = WRITE, deprecatedBy = EXPLICIT_INDEX_DEPRECATION) public java.util.stream.Stream<ExplicitIndexInfo> relationshipManualIndex(@Name("indexName") String explicitIndexName, @Name(value = "config", defaultValue = "") java.util.Map<String,String> config)
		 [Obsolete, Description("Get or create a relationship explicit index - YIELD type,name,config"), Procedure(name : "db.index.explicit.forRelationships", mode : WRITE, deprecatedBy : EXPLICIT_INDEX_DEPRECATION)]
		 public virtual Stream<ExplicitIndexInfo> RelationshipManualIndex( string explicitIndexName, IDictionary<string, string> config )
		 {
			  IndexManager mgr = GraphDatabaseAPI.index();
			  Index<Relationship> index;
			  if ( config == null || config.Count == 0 )
			  {
					index = mgr.ForRelationships( explicitIndexName );
			  }
			  else
			  {
					index = mgr.ForRelationships( explicitIndexName, config );
			  }
			  return Stream.of( new ExplicitIndexInfo( "RELATIONSHIP", explicitIndexName, mgr.GetConfiguration( index ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @Description("Check if a node explicit index exists") @Procedure(name = "db.index.explicit.existsForNodes", mode = READ, deprecatedBy = EXPLICIT_INDEX_DEPRECATION) public java.util.stream.Stream<BooleanResult> nodeManualIndexExists(@Name("indexName") String explicitIndexName)
		 [Obsolete, Description("Check if a node explicit index exists"), Procedure(name : "db.index.explicit.existsForNodes", mode : READ, deprecatedBy : EXPLICIT_INDEX_DEPRECATION)]
		 public virtual Stream<BooleanResult> NodeManualIndexExists( string explicitIndexName )
		 {
			  return Stream.of( new BooleanResult( GraphDatabaseAPI.index().existsForNodes(explicitIndexName) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @Description("Check if a relationship explicit index exists") @Procedure(name = "db.index.explicit.existsForRelationships", mode = READ, deprecatedBy = EXPLICIT_INDEX_DEPRECATION) public java.util.stream.Stream<BooleanResult> relationshipManualIndexExists(@Name("indexName") String explicitIndexName)
		 [Obsolete, Description("Check if a relationship explicit index exists"), Procedure(name : "db.index.explicit.existsForRelationships", mode : READ, deprecatedBy : EXPLICIT_INDEX_DEPRECATION)]
		 public virtual Stream<BooleanResult> RelationshipManualIndexExists( string explicitIndexName )
		 {
			  return Stream.of( new BooleanResult( GraphDatabaseAPI.index().existsForRelationships(explicitIndexName) ) );
		 }

		 [Obsolete, Description("List all explicit indexes - YIELD type,name,config"), Procedure(name : "db.index.explicit.list", mode : READ, deprecatedBy : EXPLICIT_INDEX_DEPRECATION)]
		 public virtual Stream<ExplicitIndexInfo> List()
		 {
			  IndexManager mgr = GraphDatabaseAPI.index();
			  IList<ExplicitIndexInfo> indexInfos = new List<ExplicitIndexInfo>( 100 );
			  foreach ( string name in mgr.NodeIndexNames() )
			  {
					Index<Node> index = mgr.ForNodes( name );
					indexInfos.Add( new ExplicitIndexInfo( "NODE", name, mgr.GetConfiguration( index ) ) );
			  }
			  foreach ( string name in mgr.RelationshipIndexNames() )
			  {
					RelationshipIndex index = mgr.ForRelationships( name );
					indexInfos.Add( new ExplicitIndexInfo( "RELATIONSHIP", name, mgr.GetConfiguration( index ) ) );
			  }
			  return indexInfos.stream();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @Description("Remove an explicit index - YIELD type,name,config") @Procedure(name = "db.index.explicit.drop", mode = WRITE, deprecatedBy = EXPLICIT_INDEX_DEPRECATION) public java.util.stream.Stream<ExplicitIndexInfo> manualIndexDrop(@Name("indexName") String explicitIndexName)
		 [Obsolete, Description("Remove an explicit index - YIELD type,name,config"), Procedure(name : "db.index.explicit.drop", mode : WRITE, deprecatedBy : EXPLICIT_INDEX_DEPRECATION)]
		 public virtual Stream<ExplicitIndexInfo> ManualIndexDrop( string explicitIndexName )
		 {
			  IndexManager mgr = GraphDatabaseAPI.index();
			  IList<ExplicitIndexInfo> results = new List<ExplicitIndexInfo>( 2 );
			  if ( mgr.ExistsForNodes( explicitIndexName ) )
			  {
					Index<Node> index = mgr.ForNodes( explicitIndexName );
					results.Add( new ExplicitIndexInfo( "NODE", explicitIndexName, mgr.GetConfiguration( index ) ) );
					index.Delete();
			  }
			  if ( mgr.ExistsForRelationships( explicitIndexName ) )
			  {
					RelationshipIndex index = mgr.ForRelationships( explicitIndexName );
					results.Add( new ExplicitIndexInfo( "RELATIONSHIP", explicitIndexName, mgr.GetConfiguration( index ) ) );
					index.delete();
			  }
			  return results.stream();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @Description("Add a node to an explicit index based on a specified key and value") @Procedure(name = "db.index.explicit.addNode", mode = WRITE, deprecatedBy = EXPLICIT_INDEX_DEPRECATION) public java.util.stream.Stream<BooleanResult> nodeManualIndexAdd(@Name("indexName") String explicitIndexName, @Name("node") org.Neo4Net.graphdb.Node node, @Name("key") String key, @Name("value") Object value)
		 [Obsolete, Description("Add a node to an explicit index based on a specified key and value"), Procedure(name : "db.index.explicit.addNode", mode : WRITE, deprecatedBy : EXPLICIT_INDEX_DEPRECATION)]
		 public virtual Stream<BooleanResult> NodeManualIndexAdd( string explicitIndexName, Node node, string key, object value )
		 {
			  GraphDatabaseAPI.index().forNodes(explicitIndexName).add(node, key, value);
			  // Failures will be expressed as exceptions before the return
			  return Stream.of( new BooleanResult( true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @Description("Add a relationship to an explicit index based on a specified key and value") @Procedure(name = "db.index.explicit.addRelationship", mode = WRITE, deprecatedBy = EXPLICIT_INDEX_DEPRECATION) public java.util.stream.Stream<BooleanResult> relationshipManualIndexAdd(@Name("indexName") String explicitIndexName, @Name("relationship") org.Neo4Net.graphdb.Relationship relationship, @Name("key") String key, @Name("value") Object value)
		 [Obsolete, Description("Add a relationship to an explicit index based on a specified key and value"), Procedure(name : "db.index.explicit.addRelationship", mode : WRITE, deprecatedBy : EXPLICIT_INDEX_DEPRECATION)]
		 public virtual Stream<BooleanResult> RelationshipManualIndexAdd( string explicitIndexName, Relationship relationship, string key, object value )
		 {
			  GraphDatabaseAPI.index().forRelationships(explicitIndexName).add(relationship, key, value);
			  // Failures will be expressed as exceptions before the return
			  return Stream.of( new BooleanResult( true ) );
		 }

		 private const string DEFAULT_KEY = " <[9895b15e-8693-4a21-a58b-4b7b87e09b8e]> ";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @Description("Remove a node from an explicit index with an optional key") @Procedure(name = "db.index.explicit.removeNode", mode = WRITE, deprecatedBy = EXPLICIT_INDEX_DEPRECATION) public java.util.stream.Stream<BooleanResult> nodeManualIndexRemove(@Name("indexName") String explicitIndexName, @Name("node") org.Neo4Net.graphdb.Node node, @Name(value = "key", defaultValue = DEFAULT_KEY) String key)
		 [Obsolete, Description("Remove a node from an explicit index with an optional key"), Procedure(name : "db.index.explicit.removeNode", mode : WRITE, deprecatedBy : EXPLICIT_INDEX_DEPRECATION)]
		 public virtual Stream<BooleanResult> NodeManualIndexRemove( string explicitIndexName, Node node, string key )
		 {
			  if ( key.Equals( DEFAULT_KEY ) )
			  {
					GraphDatabaseAPI.index().forNodes(explicitIndexName).remove(node);
			  }
			  else
			  {
					GraphDatabaseAPI.index().forNodes(explicitIndexName).remove(node, key);
			  }
			  // Failures will be expressed as exceptions before the return
			  return Stream.of( new BooleanResult( true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Deprecated @Description("Remove a relationship from an explicit index with an optional key") @Procedure(name = "db.index.explicit.removeRelationship", mode = WRITE, deprecatedBy = EXPLICIT_INDEX_DEPRECATION) public java.util.stream.Stream<BooleanResult> relationshipManualIndexRemove(@Name("indexName") String explicitIndexName, @Name("relationship") org.Neo4Net.graphdb.Relationship relationship, @Name(value = "key", defaultValue = DEFAULT_KEY) String key)
		 [Obsolete, Description("Remove a relationship from an explicit index with an optional key"), Procedure(name : "db.index.explicit.removeRelationship", mode : WRITE, deprecatedBy : EXPLICIT_INDEX_DEPRECATION)]
		 public virtual Stream<BooleanResult> RelationshipManualIndexRemove( string explicitIndexName, Relationship relationship, string key )
		 {
			  if ( key.Equals( DEFAULT_KEY ) )
			  {
					GraphDatabaseAPI.index().forRelationships(explicitIndexName).remove(relationship);
			  }
			  else
			  {
					GraphDatabaseAPI.index().forRelationships(explicitIndexName).remove(relationship, key);
			  }
			  // Failures will be expressed as exceptions before the return
			  return Stream.of( new BooleanResult( true ) );
		 }

		 private static long GetIndexId( IndexingService indexingService, SchemaDescriptor schema )
		 {
			  try
			  {
					return indexingService.GetIndexId( schema );
			  }
			  catch ( IndexNotFoundKernelException )
			  {
					return NOT_EXISTING_INDEX_ID;
			  }
		 }

		 private static IDictionary<string, string> IndexProviderDescriptorMap( IndexReference indexReference )
		 {
			  return MapUtil.stringMap( "key", indexReference.ProviderKey(), "version", indexReference.ProviderVersion() );
		 }

		 private static IList<string> PropertyNames( TokenNameLookup tokens, IndexReference index )
		 {
			  int[] propertyIds = index.Properties();
			  IList<string> propertyNames = new List<string>( propertyIds.Length );
			  foreach ( int propertyId in propertyIds )
			  {
					propertyNames.Add( tokens.PropertyKeyGetName( propertyId ) );
			  }
			  return propertyNames;
		 }

		 private static Stream<T> ToStream<T>( NodeExplicitIndexCursor cursor, System.Func<long, T> mapper )
		 {
			  PrefetchingResourceIterator<T> it = new PrefetchingResourceIteratorAnonymousInnerClass( cursor, mapper );
			  return Iterators.stream( it, Spliterator.ORDERED );
		 }

		 private class PrefetchingResourceIteratorAnonymousInnerClass : PrefetchingResourceIterator<T>
		 {
			 private NodeExplicitIndexCursor _cursor;
			 private System.Func<long, T> _mapper;

			 public PrefetchingResourceIteratorAnonymousInnerClass( NodeExplicitIndexCursor cursor, System.Func<long, T> mapper )
			 {
				 this._cursor = cursor;
				 this._mapper = mapper;
			 }

			 protected internal override T fetchNextOrNull()
			 {
				  if ( _cursor.next() )
				  {
						return _mapper( _cursor.nodeReference() );
				  }
				  else
				  {
						close();
						return null;
				  }
			 }

			 public override void close()
			 {
				  _cursor.close();
			 }
		 }

		 private static Stream<T> ToStream<T>( RelationshipExplicitIndexCursor cursor, System.Func<long, T> mapper )
		 {
			  PrefetchingResourceIterator<T> it = new PrefetchingResourceIteratorAnonymousInnerClass2( cursor, mapper );
			  return Iterators.stream( it, Spliterator.ORDERED );
		 }

		 private class PrefetchingResourceIteratorAnonymousInnerClass2 : PrefetchingResourceIterator<T>
		 {
			 private RelationshipExplicitIndexCursor _cursor;
			 private System.Func<long, T> _mapper;

			 public PrefetchingResourceIteratorAnonymousInnerClass2( RelationshipExplicitIndexCursor cursor, System.Func<long, T> mapper )
			 {
				 this._cursor = cursor;
				 this._mapper = mapper;
			 }

			 protected internal override T fetchNextOrNull()
			 {
				  if ( _cursor.next() )
				  {
						return _mapper( _cursor.relationshipReference() );
				  }
				  else
				  {
						close();
						return null;
				  }
			 }

			 public override void close()
			 {
				  _cursor.close();
			 }
		 }

		 private static Stream<T> ToStream<T>( PrimitiveLongResourceIterator iterator, System.Func<long, T> mapper )
		 {
			  IEnumerator<T> it = new IteratorAnonymousInnerClass( iterator, mapper );

			  return Iterators.stream( it, Spliterator.ORDERED );
		 }

		 private class IteratorAnonymousInnerClass : IEnumerator<T>
		 {
			 private PrimitiveLongResourceIterator _iterator;
			 private System.Func<long, T> _mapper;

			 public IteratorAnonymousInnerClass( PrimitiveLongResourceIterator iterator, System.Func<long, T> mapper )
			 {
				 this._iterator = iterator;
				 this._mapper = mapper;
			 }

			 public bool hasNext()
			 {
				  return _iterator.hasNext();
			 }

			 public T next()
			 {
				  return _mapper( _iterator.next() );
			 }
		 }

		 private Stream<WeightedNodeResult> ToWeightedNodeResultStream( NodeExplicitIndexCursor cursor )
		 {
			  IEnumerator<WeightedNodeResult> it = new PrefetchingResourceIteratorAnonymousInnerClass3( this, cursor );

			  return Iterators.stream( it, Spliterator.ORDERED );
		 }

		 private class PrefetchingResourceIteratorAnonymousInnerClass3 : PrefetchingResourceIterator<WeightedNodeResult>
		 {
			 private readonly BuiltInProcedures _outerInstance;

			 private NodeExplicitIndexCursor _cursor;

			 public PrefetchingResourceIteratorAnonymousInnerClass3( BuiltInProcedures outerInstance, NodeExplicitIndexCursor cursor )
			 {
				 this.outerInstance = outerInstance;
				 this._cursor = cursor;
			 }

			 public override void close()
			 {
				  _cursor.close();
			 }

			 protected internal override WeightedNodeResult fetchNextOrNull()
			 {
				  if ( _cursor.next() )
				  {
						return new WeightedNodeResult( _outerInstance.graphDatabaseAPI.getNodeById( _cursor.nodeReference() ), _cursor.score() );
				  }
				  else
				  {
						close();
						return null;
				  }
			 }
		 }

		 private Stream<WeightedRelationshipResult> ToWeightedRelationshipResultStream( RelationshipExplicitIndexCursor cursor )
		 {
			  IEnumerator<WeightedRelationshipResult> it = new PrefetchingResourceIteratorAnonymousInnerClass4( this, cursor );
			  return Iterators.stream( it, Spliterator.ORDERED );
		 }

		 private class PrefetchingResourceIteratorAnonymousInnerClass4 : PrefetchingResourceIterator<WeightedRelationshipResult>
		 {
			 private readonly BuiltInProcedures _outerInstance;

			 private RelationshipExplicitIndexCursor _cursor;

			 public PrefetchingResourceIteratorAnonymousInnerClass4( BuiltInProcedures outerInstance, RelationshipExplicitIndexCursor cursor )
			 {
				 this.outerInstance = outerInstance;
				 this._cursor = cursor;
			 }

			 public override void close()
			 {
				  _cursor.close();
			 }

			 protected internal override WeightedRelationshipResult fetchNextOrNull()
			 {
				  if ( _cursor.next() )
				  {
						return new WeightedRelationshipResult( _outerInstance.graphDatabaseAPI.getRelationshipById( _cursor.relationshipReference() ), _cursor.score() );
				  }
				  else
				  {
						close();
						return null;
				  }
			 }
		 }

		 private IndexProcedures IndexProcedures()
		 {
			  return new IndexProcedures( Tx, Resolver.resolveDependency( typeof( IndexingService ) ) );
		 }

		 public class LabelResult
		 {
			  public readonly string Label;

			  internal LabelResult( Label label )
			  {
					this.Label = label.Name();
			  }
		 }

		 public class PropertyKeyResult
		 {
			  public readonly string PropertyKey;

			  internal PropertyKeyResult( string propertyKey )
			  {
					this.PropertyKey = propertyKey;
			  }
		 }

		 public class RelationshipTypeResult
		 {
			  public readonly string RelationshipType;

			  internal RelationshipTypeResult( RelationshipType relationshipType )
			  {
					this.RelationshipType = relationshipType.Name();
			  }
		 }

		 public class BooleanResult
		 {
			  public BooleanResult( bool? success )
			  {
					this.Success = success;
			  }

			  public readonly bool? Success;
		 }

		 public class IndexResult
		 {
			  public readonly string Description;
			  public readonly string IndexName;
			  public readonly IList<string> TokenNames;
			  public readonly IList<string> Properties;
			  public readonly string State;
			  public readonly string Type;
			  public readonly double? Progress;
			  public readonly IDictionary<string, string> Provider;
			  public readonly long Id;
			  public readonly string FailureMessage;

			  internal IndexResult( long id, string description, string indexName, IList<string> tokenNames, IList<string> properties, string state, string type, float? progress, IDictionary<string, string> provider, string failureMessage )
			  {
					this.Id = id;
					this.Description = description;
					this.IndexName = indexName;
					this.TokenNames = tokenNames;
					this.Properties = properties;
					this.State = state;
					this.Type = type;
					this.Progress = progress.Value;
					this.Provider = provider;
					this.FailureMessage = failureMessage;
			  }
		 }

		 public class SchemaIndexInfo
		 {
			  public readonly string Index;
			  public readonly string ProviderName;
			  public readonly string Status;

			  public SchemaIndexInfo( string index, string providerName, string status )
			  {
					this.Index = index;
					this.ProviderName = providerName;
					this.Status = status;
			  }
		 }

		 public class ExplicitIndexInfo
		 {
			  public readonly string Type;
			  public readonly string Name;
			  public readonly IDictionary<string, string> Config;

			  public ExplicitIndexInfo( string type, string name, IDictionary<string, string> config )
			  {
					this.Type = type;
					this.Name = name;
					this.Config = config;
			  }
		 }

		 public class ConstraintResult
		 {
			  public readonly string Description;

			  internal ConstraintResult( string description )
			  {
					this.Description = description;
			  }
		 }

		 public class NodeResult
		 {
			  public NodeResult( Node node )
			  {
					this.Node = node;
			  }

			  public readonly Node Node;
		 }

		 public class WeightedNodeResult
		 {
			  public readonly Node Node;
			  public readonly double Weight;

			  public WeightedNodeResult( Node node, double weight )
			  {
					this.Node = node;
					this.Weight = weight;
			  }
		 }

		 public class WeightedRelationshipResult
		 {
			  public readonly Relationship Relationship;
			  public readonly double Weight;

			  public WeightedRelationshipResult( Relationship relationship, double weight )
			  {
					this.Relationship = relationship;
					this.Weight = weight;
			  }
		 }

		 public class RelationshipResult
		 {
			  public RelationshipResult( Relationship relationship )
			  {
					this.Relationship = relationship;
			  }

			  public readonly Relationship Relationship;
		 }

		 //When we have decided on what to call different indexes
		 //this should probably be moved to some more central place
		 private sealed class IndexType
		 {
			  public static readonly IndexType NodeLabelProperty = new IndexType( "NodeLabelProperty", InnerEnum.NodeLabelProperty, "node_label_property" );
			  public static readonly IndexType NodeUniqueProperty = new IndexType( "NodeUniqueProperty", InnerEnum.NodeUniqueProperty, "node_unique_property" );
			  public static readonly IndexType RelTypeProperty = new IndexType( "RelTypeProperty", InnerEnum.RelTypeProperty, "relationship_type_property" );
			  public static readonly IndexType NodeFulltext = new IndexType( "NodeFulltext", InnerEnum.NodeFulltext, "node_fulltext" );
			  public static readonly IndexType RelationshipFulltext = new IndexType( "RelationshipFulltext", InnerEnum.RelationshipFulltext, "relationship_fulltext" );

			  private static readonly IList<IndexType> valueList = new List<IndexType>();

			  static IndexType()
			  {
				  valueList.Add( NodeLabelProperty );
				  valueList.Add( NodeUniqueProperty );
				  valueList.Add( RelTypeProperty );
				  valueList.Add( NodeFulltext );
				  valueList.Add( RelationshipFulltext );
			  }

			  public enum InnerEnum
			  {
				  NodeLabelProperty,
				  NodeUniqueProperty,
				  RelTypeProperty,
				  NodeFulltext,
				  RelationshipFulltext
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  internal Private readonly;

			  internal IndexType( string name, InnerEnum innerEnum, string typeName )
			  {
					this._typeName = typeName;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal static IndexType GetIndexTypeOf( Neo4Net.Kernel.Api.Internal.IndexReference index )
			  {
					if ( index.FulltextIndex )
					{
						 if ( index.Schema().entityType() == EntityType.NODE )
						 {
							  return IndexType.NodeFulltext;
						 }
						 else
						 {
							  return IndexType.RelationshipFulltext;
						 }
					}
					else
					{
						 if ( index.Unique )
						 {
							  return IndexType.NodeUniqueProperty;
						 }
						 else
						 {
							  if ( index.Schema().entityType() == EntityType.NODE )
							  {
									return IndexType.NodeLabelProperty;
							  }
							  else
							  {
									return IndexType.RelTypeProperty;
							  }
						 }
					}
			  }

			  public string TypeName()
			  {
					return _typeName;
			  }

			 public static IList<IndexType> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static IndexType ValueOf( string name )
			 {
				 foreach ( IndexType enumInstance in IndexType.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }
	}

}