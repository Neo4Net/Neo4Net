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
namespace Neo4Net.Kernel.Api.Impl.Fulltext
{
	using ArrayUtils = org.apache.commons.lang3.ArrayUtils;
	using ParseException = org.apache.lucene.queryparser.classic.ParseException;


	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using NotFoundException = Neo4Net.Graphdb.NotFoundException;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using AnalyzerProvider = Neo4Net.Graphdb.index.fulltext.AnalyzerProvider;
	using IndexDefinition = Neo4Net.Graphdb.schema.IndexDefinition;
	using Schema = Neo4Net.Graphdb.schema.Schema;
	using IndexReference = Neo4Net.Internal.Kernel.Api.IndexReference;
	using InvalidTransactionTypeKernelException = Neo4Net.Internal.Kernel.Api.exceptions.InvalidTransactionTypeKernelException;
	using ProcedureException = Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException;
	using IndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using SchemaKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.SchemaKernelException;
	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;
	using IndexProcedures = Neo4Net.Kernel.builtinprocs.IndexProcedures;
	using KernelTransactionImplementation = Neo4Net.Kernel.Impl.Api.KernelTransactionImplementation;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using Context = Neo4Net.Procedure.Context;
	using Description = Neo4Net.Procedure.Description;
	using Name = Neo4Net.Procedure.Name;
	using Procedure = Neo4Net.Procedure.Procedure;
	using EntityType = Neo4Net.Storageengine.Api.EntityType;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using FeatureToggles = Neo4Net.Utils.FeatureToggles;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.FulltextIndexProviderFactory.DESCRIPTOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.FulltextIndexSettings.INDEX_CONFIG_ANALYZER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.FulltextIndexSettings.INDEX_CONFIG_EVENTUALLY_CONSISTENT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.procedure.Mode.READ;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.procedure.Mode.SCHEMA;

	/// <summary>
	/// Procedures for querying the Fulltext indexes.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public class FulltextProcedures
	public class FulltextProcedures
	{
		 private static readonly long _indexOnlineQueryTimeoutSeconds = FeatureToggles.getInteger( typeof( FulltextProcedures ), "INDEX_ONLINE_QUERY_TIMEOUT_SECONDS", 30 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.kernel.api.KernelTransaction tx;
		 public KernelTransaction Tx;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.graphdb.GraphDatabaseService db;
		 public GraphDatabaseService Db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.graphdb.DependencyResolver resolver;
		 public DependencyResolver Resolver;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public FulltextAdapter accessor;
		 public FulltextAdapter Accessor;

		 [Description("List the available analyzers that the fulltext indexes can be configured with."), Procedure(name : "db.index.fulltext.listAvailableAnalyzers", mode : READ)]
		 public virtual Stream<AvailableAnalyzer> ListAvailableAnalyzers()
		 {
			  Stream<AnalyzerProvider> stream = Accessor.listAvailableAnalyzers();
			  return stream.flatMap(provider =>
			  {
				string description = provider.description();
				Spliterator<string> spliterator = provider.Keys.spliterator();
				return StreamSupport.stream( spliterator, false ).map( name => new AvailableAnalyzer( name, description ) );
			  });
		 }

		 [Description("Wait for the updates from recently committed transactions to be applied to any eventually-consistent fulltext indexes."), Procedure(name : "db.index.fulltext.awaitEventuallyConsistentIndexRefresh", mode : READ)]
		 public virtual void AwaitRefresh()
		 {
			  Accessor.awaitRefresh();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Similar to db.awaitIndex(index, timeout), except instead of an index pattern, the index is specified by name. " + "The name can be quoted by backticks, if necessary.") @Procedure(name = "db.index.fulltext.awaitIndex", mode = READ) public void awaitIndex(@Name("index") String index, @Name(value = "timeOutSeconds", defaultValue = "300") long timeout) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Similar to db.awaitIndex(index, timeout), except instead of an index pattern, the index is specified by name. " + "The name can be quoted by backticks, if necessary."), Procedure(name : "db.index.fulltext.awaitIndex", mode : READ)]
		 public virtual void AwaitIndex( string index, long timeout )
		 {
			  using ( IndexProcedures indexProcedures = indexProcedures() )
			  {
					indexProcedures.AwaitIndexByName( index, timeout, TimeUnit.SECONDS );
			  }
		 }

		 private IndexProcedures IndexProcedures()
		 {
			  return new IndexProcedures( Tx, Resolver.resolveDependency( typeof( IndexingService ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Create a node fulltext index for the given labels and properties. " + "The optional 'config' map parameter can be used to supply settings to the index. " + "Note: index specific settings are currently experimental, and might not replicated correctly in a cluster, or during backup. " + "Supported settings are '" + INDEX_CONFIG_ANALYZER + "', for specifying what analyzer to use " + "when indexing and querying. Use the `db.index.fulltext.listAvailableAnalyzers` procedure to see what options are available. " + "And '" + INDEX_CONFIG_EVENTUALLY_CONSISTENT + "' which can be set to 'true' to make this index eventually consistent, " + "such that updates from committing transactions are applied in a background thread.") @Procedure(name = "db.index.fulltext.createNodeIndex", mode = SCHEMA) public void createNodeFulltextIndex(@Name("indexName") String name, @Name("labels") java.util.List<String> labels, @Name("propertyNames") java.util.List<String> properties, @Name(value = "config", defaultValue = "") java.util.Map<String,String> indexConfigurationMap) throws org.neo4j.internal.kernel.api.exceptions.InvalidTransactionTypeKernelException, org.neo4j.internal.kernel.api.exceptions.schema.SchemaKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Create a node fulltext index for the given labels and properties. " + "The optional 'config' map parameter can be used to supply settings to the index. " + "Note: index specific settings are currently experimental, and might not replicated correctly in a cluster, or during backup. " + "Supported settings are '" + INDEX_CONFIG_ANALYZER + "', for specifying what analyzer to use " + "when indexing and querying. Use the `db.index.fulltext.listAvailableAnalyzers` procedure to see what options are available. " + "And '" + INDEX_CONFIG_EVENTUALLY_CONSISTENT + "' which can be set to 'true' to make this index eventually consistent, " + "such that updates from committing transactions are applied in a background thread."), Procedure(name : "db.index.fulltext.createNodeIndex", mode : SCHEMA)]
		 public virtual void CreateNodeFulltextIndex( string name, IList<string> labels, IList<string> properties, IDictionary<string, string> indexConfigurationMap )
		 {
			  Properties indexConfiguration = new Properties();
			  indexConfiguration.putAll( indexConfigurationMap );
			  SchemaDescriptor schemaDescriptor = Accessor.schemaFor( EntityType.NODE, StringArray( labels ), indexConfiguration, StringArray( properties ) );
			  Tx.schemaWrite().indexCreate(schemaDescriptor, DESCRIPTOR.name(), name);
		 }

		 private string[] StringArray( IList<string> strings )
		 {
			  return strings.toArray( ArrayUtils.EMPTY_STRING_ARRAY );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Create a relationship fulltext index for the given relationship types and properties. " + "The optional 'config' map parameter can be used to supply settings to the index. " + "Note: index specific settings are currently experimental, and might not replicated correctly in a cluster, or during backup. " + "Supported settings are '" + INDEX_CONFIG_ANALYZER + "', for specifying what analyzer to use " + "when indexing and querying. Use the `db.index.fulltext.listAvailableAnalyzers` procedure to see what options are available. " + "And '" + INDEX_CONFIG_EVENTUALLY_CONSISTENT + "' which can be set to 'true' to make this index eventually consistent, " + "such that updates from committing transactions are applied in a background thread.") @Procedure(name = "db.index.fulltext.createRelationshipIndex", mode = SCHEMA) public void createRelationshipFulltextIndex(@Name("indexName") String name, @Name("relationshipTypes") java.util.List<String> relTypes, @Name("propertyNames") java.util.List<String> properties, @Name(value = "config", defaultValue = "") java.util.Map<String,String> config) throws org.neo4j.internal.kernel.api.exceptions.InvalidTransactionTypeKernelException, org.neo4j.internal.kernel.api.exceptions.schema.SchemaKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Create a relationship fulltext index for the given relationship types and properties. " + "The optional 'config' map parameter can be used to supply settings to the index. " + "Note: index specific settings are currently experimental, and might not replicated correctly in a cluster, or during backup. " + "Supported settings are '" + INDEX_CONFIG_ANALYZER + "', for specifying what analyzer to use " + "when indexing and querying. Use the `db.index.fulltext.listAvailableAnalyzers` procedure to see what options are available. " + "And '" + INDEX_CONFIG_EVENTUALLY_CONSISTENT + "' which can be set to 'true' to make this index eventually consistent, " + "such that updates from committing transactions are applied in a background thread."), Procedure(name : "db.index.fulltext.createRelationshipIndex", mode : SCHEMA)]
		 public virtual void CreateRelationshipFulltextIndex( string name, IList<string> relTypes, IList<string> properties, IDictionary<string, string> config )
		 {
			  Properties settings = new Properties();
			  settings.putAll( config );
			  SchemaDescriptor schemaDescriptor = Accessor.schemaFor( EntityType.RELATIONSHIP, StringArray( relTypes ), settings, StringArray( properties ) );
			  Tx.schemaWrite().indexCreate(schemaDescriptor, DESCRIPTOR.name(), name);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Drop the specified index.") @Procedure(name = "db.index.fulltext.drop", mode = SCHEMA) public void drop(@Name("indexName") String name) throws org.neo4j.internal.kernel.api.exceptions.InvalidTransactionTypeKernelException, org.neo4j.internal.kernel.api.exceptions.schema.SchemaKernelException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Drop the specified index."), Procedure(name : "db.index.fulltext.drop", mode : SCHEMA)]
		 public virtual void Drop( string name )
		 {
			  IndexReference indexReference = GetValidIndexReference( name );
			  Tx.schemaWrite().indexDrop(indexReference);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Query the given fulltext index. Returns the matching nodes and their lucene query score, ordered by score.") @Procedure(name = "db.index.fulltext.queryNodes", mode = READ) public java.util.stream.Stream<NodeOutput> queryFulltextForNodes(@Name("indexName") String name, @Name("queryString") String query) throws org.apache.lucene.queryparser.classic.ParseException, org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Query the given fulltext index. Returns the matching nodes and their lucene query score, ordered by score."), Procedure(name : "db.index.fulltext.queryNodes", mode : READ)]
		 public virtual Stream<NodeOutput> QueryFulltextForNodes( string name, string query )
		 {
			  IndexReference indexReference = GetValidIndexReference( name );
			  AwaitOnline( indexReference );
			  EntityType entityType = indexReference.Schema().entityType();
			  if ( entityType != EntityType.NODE )
			  {
					throw new System.ArgumentException( "The '" + name + "' index (" + indexReference + ") is an index on " + entityType + ", so it cannot be queried for nodes." );
			  }
			  ScoreEntityIterator resultIterator = Accessor.query( Tx, name, query );
			  return resultIterator.Select( result => NodeOutput.ForExistingEntityOrNull( Db, result ) ).Where( Objects.nonNull );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("Query the given fulltext index. Returns the matching relationships and their lucene query score, ordered by score.") @Procedure(name = "db.index.fulltext.queryRelationships", mode = READ) public java.util.stream.Stream<RelationshipOutput> queryFulltextForRelationships(@Name("indexName") String name, @Name("queryString") String query) throws org.apache.lucene.queryparser.classic.ParseException, org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException, java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Description("Query the given fulltext index. Returns the matching relationships and their lucene query score, ordered by score."), Procedure(name : "db.index.fulltext.queryRelationships", mode : READ)]
		 public virtual Stream<RelationshipOutput> QueryFulltextForRelationships( string name, string query )
		 {
			  IndexReference indexReference = GetValidIndexReference( name );
			  AwaitOnline( indexReference );
			  EntityType entityType = indexReference.Schema().entityType();
			  if ( entityType != EntityType.RELATIONSHIP )
			  {
					throw new System.ArgumentException( "The '" + name + "' index (" + indexReference + ") is an index on " + entityType + ", so it cannot be queried for relationships." );
			  }
			  ScoreEntityIterator resultIterator = Accessor.query( Tx, name, query );
			  return resultIterator.Select( result => RelationshipOutput.ForExistingEntityOrNull( Db, result ) ).Where( Objects.nonNull );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: private org.neo4j.internal.kernel.api.IndexReference getValidIndexReference(@Name("indexName") String name)
		 private IndexReference GetValidIndexReference( string name )
		 {
			  IndexReference indexReference = Tx.schemaRead().indexGetForName(name);
			  if ( indexReference == IndexReference.NO_INDEX )
			  {
					throw new System.ArgumentException( "There is no such fulltext schema index: " + name );
			  }
			  return indexReference;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void awaitOnline(org.neo4j.internal.kernel.api.IndexReference indexReference) throws org.neo4j.internal.kernel.api.exceptions.schema.IndexNotFoundKernelException
		 private void AwaitOnline( IndexReference indexReference )
		 {
			  // We do the isAdded check on the transaction state first, because indexGetState will grab a schema read-lock, which can deadlock on the write-lock
			  // held by the index populator. Also, if we index was created in this transaction, then we will never see it come online in this transaction anyway.
			  // Indexes don't come online until the transaction that creates them has committed.
			  if ( !( ( KernelTransactionImplementation )Tx ).txState().indexDiffSetsBySchema(indexReference.Schema()).isAdded((IndexDescriptor) indexReference) )
			  {
					// If the index was not created in this transaction, then wait for it to come online before querying.
					Schema schema = Db.schema();
					IndexDefinition index = Schema.getIndexByName( indexReference.Name() );
					Schema.awaitIndexOnline( index, _indexOnlineQueryTimeoutSeconds, TimeUnit.SECONDS );
			  }
			  // If the index was created in this transaction, then we skip this check entirely.
			  // We will get an exception later, when we try to get an IndexReader, so this is fine.
		 }

		 public sealed class NodeOutput
		 {
			  public readonly Node Node;
			  public readonly double Score;

			  protected internal NodeOutput( Node node, double score )
			  {
					this.Node = node;
					this.Score = score;
			  }

			  public static NodeOutput ForExistingEntityOrNull( GraphDatabaseService db, ScoreEntityIterator.ScoreEntry result )
			  {
					try
					{
						 return new NodeOutput( Db.getNodeById( result.EntityId() ), result.Score() );
					}
					catch ( NotFoundException )
					{
						 // This node was most likely deleted by a concurrent transaction, so we just ignore it.
						 return null;
					}
			  }
		 }

		 public sealed class RelationshipOutput
		 {
			  public readonly Relationship Relationship;
			  public readonly double Score;

			  public RelationshipOutput( Relationship relationship, double score )
			  {
					this.Relationship = relationship;
					this.Score = score;
			  }

			  public static RelationshipOutput ForExistingEntityOrNull( GraphDatabaseService db, ScoreEntityIterator.ScoreEntry result )
			  {
					try
					{
						 return new RelationshipOutput( Db.getRelationshipById( result.EntityId() ), result.Score() );
					}
					catch ( NotFoundException )
					{
						 // This relationship was most likely deleted by a concurrent transaction, so we just ignore it.
						 return null;
					}
			  }
		 }

		 public sealed class AvailableAnalyzer
		 {
			  public readonly string Analyzer;
			  public readonly string Description;

			  internal AvailableAnalyzer( string name, string description )
			  {
					this.Analyzer = name;
					this.Description = description;
			  }
		 }
	}

}