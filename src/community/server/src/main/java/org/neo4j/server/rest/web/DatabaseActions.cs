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
namespace Neo4Net.Server.rest.web
{
	using HttpContext = com.sun.jersey.api.core.HttpContext;
	using Sort = org.apache.lucene.search.Sort;


	using Predicates = Neo4Net.Functions.Predicates;
	using CommonEvaluators = Neo4Net.Graphalgo.CommonEvaluators;
	using Neo4Net.Graphalgo;
	using GraphAlgoFactory = Neo4Net.Graphalgo.GraphAlgoFactory;
	using Neo4Net.Graphalgo;
	using WeightedPath = Neo4Net.Graphalgo.WeightedPath;
	using ConstraintViolationException = Neo4Net.GraphDb.ConstraintViolationException;
	using Direction = Neo4Net.GraphDb.Direction;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using NotFoundException = Neo4Net.GraphDb.NotFoundException;
	using Path = Neo4Net.GraphDb.Path;
	using Neo4Net.GraphDb;
	using PathExpanderBuilder = Neo4Net.GraphDb.PathExpanderBuilder;
	using PathExpanders = Neo4Net.GraphDb.PathExpanders;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Neo4Net.GraphDb;
	using Neo4Net.GraphDb.index;
	using Neo4Net.GraphDb.index;
	using Neo4Net.GraphDb.index;
	using IndexManager = Neo4Net.GraphDb.index.IndexManager;
	using Neo4Net.GraphDb.index;
	using Neo4Net.GraphDb.index;
	using Neo4Net.GraphDb.index.UniqueFactory;
	using ConstraintCreator = Neo4Net.GraphDb.schema.ConstraintCreator;
	using ConstraintDefinition = Neo4Net.GraphDb.schema.ConstraintDefinition;
	using ConstraintType = Neo4Net.GraphDb.schema.ConstraintType;
	using IndexCreator = Neo4Net.GraphDb.schema.IndexCreator;
	using IndexDefinition = Neo4Net.GraphDb.schema.IndexDefinition;
	using Neo4Net.GraphDb.traversal;
	using Paths = Neo4Net.GraphDb.traversal.Paths;
	using Neo4Net.Helpers.Collections;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using Neo4Net.Helpers.Collections;
	using QueryContext = Neo4Net.Index.lucene.QueryContext;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Neo4Net.Server.database;
	using EndNodeNotFoundException = Neo4Net.Server.rest.domain.EndNodeNotFoundException;
	using PropertySettingStrategy = Neo4Net.Server.rest.domain.PropertySettingStrategy;
	using RelationshipExpanderBuilder = Neo4Net.Server.rest.domain.RelationshipExpanderBuilder;
	using StartNodeNotFoundException = Neo4Net.Server.rest.domain.StartNodeNotFoundException;
	using BadInputException = Neo4Net.Server.rest.repr.BadInputException;
	using ConstraintDefinitionRepresentation = Neo4Net.Server.rest.repr.ConstraintDefinitionRepresentation;
	using DatabaseRepresentation = Neo4Net.Server.rest.repr.DatabaseRepresentation;
	using IndexDefinitionRepresentation = Neo4Net.Server.rest.repr.IndexDefinitionRepresentation;
	using IndexRepresentation = Neo4Net.Server.rest.repr.IndexRepresentation;
	using IndexedEntityRepresentation = Neo4Net.Server.rest.repr.IndexedEntityRepresentation;
	using InvalidArgumentsException = Neo4Net.Server.rest.repr.InvalidArgumentsException;
	using ListRepresentation = Neo4Net.Server.rest.repr.ListRepresentation;
	using NodeIndexRepresentation = Neo4Net.Server.rest.repr.NodeIndexRepresentation;
	using NodeIndexRootRepresentation = Neo4Net.Server.rest.repr.NodeIndexRootRepresentation;
	using NodeRepresentation = Neo4Net.Server.rest.repr.NodeRepresentation;
	using Neo4Net.Server.rest.repr;
	using PropertiesRepresentation = Neo4Net.Server.rest.repr.PropertiesRepresentation;
	using RelationshipIndexRepresentation = Neo4Net.Server.rest.repr.RelationshipIndexRepresentation;
	using RelationshipIndexRootRepresentation = Neo4Net.Server.rest.repr.RelationshipIndexRootRepresentation;
	using RelationshipRepresentation = Neo4Net.Server.rest.repr.RelationshipRepresentation;
	using Representation = Neo4Net.Server.rest.repr.Representation;
	using RepresentationType = Neo4Net.Server.rest.repr.RepresentationType;
	using ScoredNodeRepresentation = Neo4Net.Server.rest.repr.ScoredNodeRepresentation;
	using ScoredRelationshipRepresentation = Neo4Net.Server.rest.repr.ScoredRelationshipRepresentation;
	using ValueRepresentation = Neo4Net.Server.rest.repr.ValueRepresentation;
	using WeightedPathRepresentation = Neo4Net.Server.rest.repr.WeightedPathRepresentation;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterables.filter;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterables.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.repr.RepresentationType.CONSTRAINT_DEFINITION;

	public class DatabaseActions
	{
		 public const string SCORE_ORDER = "score";
		 public const string RELEVANCE_ORDER = "relevance";
		 public const string INDEX_ORDER = "index";
		 private readonly GraphDatabaseAPI _graphDb;

		 private readonly PropertySettingStrategy _propertySetter;

		 public class Provider : InjectableProvider<DatabaseActions>
		 {
			  internal readonly DatabaseActions Database;

			  public Provider( DatabaseActions database ) : base( typeof( DatabaseActions ) )
			  {
					this.Database = database;
			  }

			  public override DatabaseActions GetValue( HttpContext c )
			  {
					return Database;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		 private readonly System.Func<ConstraintDefinition, Representation> _constraintDefToRepresentation = ConstraintDefinitionRepresentation::new;

		 public DatabaseActions( GraphDatabaseAPI graphDatabaseAPI )
		 {
			  this._graphDb = graphDatabaseAPI;
			  this._propertySetter = new PropertySettingStrategy( _graphDb );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.graphdb.Node node(long id) throws NodeNotFoundException
		 private Node Node( long id )
		 {
			  try
			  {
					return _graphDb.getNodeById( id );
			  }
			  catch ( NotFoundException e )
			  {
					throw new NodeNotFoundException( string.Format( "Cannot find node with id [{0:D}] in database.", id ), e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.Neo4Net.graphdb.Relationship relationship(long id) throws RelationshipNotFoundException
		 private Relationship Relationship( long id )
		 {
			  try
			  {
					return _graphDb.getRelationshipById( id );
			  }
			  catch ( NotFoundException e )
			  {
					throw new RelationshipNotFoundException( e );
			  }
		 }

		 // API

		 public virtual DatabaseRepresentation Root()
		 {
			  return new DatabaseRepresentation();
		 }

		 // Nodes

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.server.rest.repr.NodeRepresentation createNode(java.util.Map<String,Object> properties, org.Neo4Net.graphdb.Label... labels) throws PropertyValueException
		 public virtual NodeRepresentation CreateNode( IDictionary<string, object> properties, params Label[] labels )
		 {
			  Node node = _graphDb.createNode();
			  _propertySetter.setProperties( node, properties );
			  if ( labels != null )
			  {
					foreach ( Label label in labels )
					{
						 node.AddLabel( label );
					}
			  }
			  return new NodeRepresentation( node );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.server.rest.repr.NodeRepresentation getNode(long nodeId) throws NodeNotFoundException
		 public virtual NodeRepresentation GetNode( long nodeId )
		 {
			  return new NodeRepresentation( Node( nodeId ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void deleteNode(long nodeId) throws NodeNotFoundException, org.Neo4Net.graphdb.ConstraintViolationException
		 public virtual void DeleteNode( long nodeId )
		 {
			  Node node = node( nodeId );

			  if ( node.HasRelationship() )
			  {
					throw new ConstraintViolationException( string.Format( "The node with id {0:D} cannot be deleted. Check that the node is orphaned before deletion.", nodeId ) );
			  }

			  node.Delete();
		 }

		 // Property keys

		 public virtual Representation AllPropertyKeys
		 {
			 get
			 {
				  ICollection<ValueRepresentation> propKeys = Iterables.asSet( map( ValueRepresentation.@string, _graphDb.AllPropertyKeys ) );
   
				  return new ListRepresentation( RepresentationType.STRING, propKeys );
			 }
		 }

		 // Node properties

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.server.rest.repr.Representation getNodeProperty(long nodeId, String key) throws NodeNotFoundException, NoSuchPropertyException
		 public virtual Representation GetNodeProperty( long nodeId, string key )
		 {
			  Node node = node( nodeId );
			  try
			  {
					return PropertiesRepresentation.value( node.GetProperty( key ) );
			  }
			  catch ( NotFoundException )
			  {
					throw new NoSuchPropertyException( node, key );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void setNodeProperty(long nodeId, String key, Object value) throws PropertyValueException, NodeNotFoundException
		 public virtual void SetNodeProperty( long nodeId, string key, object value )
		 {
			  Node node = node( nodeId );
			  _propertySetter.setProperty( node, key, value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void removeNodeProperty(long nodeId, String key) throws NodeNotFoundException, NoSuchPropertyException
		 public virtual void RemoveNodeProperty( long nodeId, string key )
		 {
			  Node node = node( nodeId );
			  if ( node.RemoveProperty( key ) == null )
			  {
					throw new NoSuchPropertyException( node, key );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.server.rest.repr.PropertiesRepresentation getAllNodeProperties(long nodeId) throws NodeNotFoundException
		 public virtual PropertiesRepresentation GetAllNodeProperties( long nodeId )
		 {
			  return new PropertiesRepresentation( Node( nodeId ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void setAllNodeProperties(long nodeId, java.util.Map<String,Object> properties) throws PropertyValueException, NodeNotFoundException
		 public virtual void SetAllNodeProperties( long nodeId, IDictionary<string, object> properties )
		 {
			  _propertySetter.setAllProperties( Node( nodeId ), properties );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void removeAllNodeProperties(long nodeId) throws NodeNotFoundException, PropertyValueException
		 public virtual void RemoveAllNodeProperties( long nodeId )
		 {
			  _propertySetter.setAllProperties( Node( nodeId ), null );
		 }

		 // Labels

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void addLabelToNode(long nodeId, java.util.Collection<String> labelNames) throws NodeNotFoundException, org.Neo4Net.server.rest.repr.BadInputException
		 public virtual void AddLabelToNode( long nodeId, ICollection<string> labelNames )
		 {
			  try
			  {
					Node node = node( nodeId );
					foreach ( string labelName in labelNames )
					{
						 node.AddLabel( label( labelName ) );
					}
			  }
			  catch ( ConstraintViolationException e )
			  {
					throw new BadInputException( "Unable to add label, see nested exception.", e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void setLabelsOnNode(long nodeId, java.util.Collection<String> labels) throws NodeNotFoundException, org.Neo4Net.server.rest.repr.BadInputException
		 public virtual void SetLabelsOnNode( long nodeId, ICollection<string> labels )

		 {
			  Node node = node( nodeId );
			  try
			  {
					// Remove current labels
					foreach ( Label label in node.Labels )
					{
						 node.RemoveLabel( label );
					}

					// Add new labels
					foreach ( string labelName in labels )
					{
						 node.AddLabel( label( labelName ) );
					}
			  }
			  catch ( ConstraintViolationException e )
			  {
					throw new BadInputException( "Unable to add label, see nested exception.", e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void removeLabelFromNode(long nodeId, String labelName) throws NodeNotFoundException
		 public virtual void RemoveLabelFromNode( long nodeId, string labelName )
		 {
			  Node( nodeId ).removeLabel( label( labelName ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.server.rest.repr.ListRepresentation getNodeLabels(long nodeId) throws NodeNotFoundException
		 public virtual ListRepresentation GetNodeLabels( long nodeId )
		 {
			  IEnumerable<string> labels = new IterableWrapperAnonymousInnerClass( this );
			  return ListRepresentation.@string( labels );
		 }

		 private class IterableWrapperAnonymousInnerClass : IterableWrapper<string, Label>
		 {
			 private readonly DatabaseActions _outerInstance;

			 public IterableWrapperAnonymousInnerClass( DatabaseActions outerInstance ) : base( outerInstance.node( nodeId ).Labels )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override string underlyingObjectToObject( Label @object )
			 {
				  return @object.Name();
			 }
		 }

		 public virtual string[] NodeIndexNames
		 {
			 get
			 {
				  return _graphDb.index().nodeIndexNames();
			 }
		 }

		 public virtual string[] RelationshipIndexNames
		 {
			 get
			 {
				  return _graphDb.index().relationshipIndexNames();
			 }
		 }

		 public virtual IndexRepresentation CreateNodeIndex( IDictionary<string, object> indexSpecification )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String indexName = (String) indexSpecification.get("name");
			  string indexName = ( string ) indexSpecification["name"];

			  AssertIsLegalIndexName( indexName );

			  if ( indexSpecification.ContainsKey( "config" ) )
			  {

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.Map<String,String> config = (java.util.Map<String,String>) indexSpecification.get("config");
					IDictionary<string, string> config = ( IDictionary<string, string> ) indexSpecification["config"];
					_graphDb.index().forNodes(indexName, config);

					return new NodeIndexRepresentation( indexName, config );
			  }

			  _graphDb.index().forNodes(indexName);
			  return new NodeIndexRepresentation( indexName, Collections.emptyMap() );
		 }

		 public virtual IndexRepresentation CreateRelationshipIndex( IDictionary<string, object> indexSpecification )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String indexName = (String) indexSpecification.get("name");
			  string indexName = ( string ) indexSpecification["name"];

			  AssertIsLegalIndexName( indexName );

			  if ( indexSpecification.ContainsKey( "config" ) )
			  {

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") java.util.Map<String,String> config = (java.util.Map<String,String>) indexSpecification.get("config");
					IDictionary<string, string> config = ( IDictionary<string, string> ) indexSpecification["config"];
					_graphDb.index().forRelationships(indexName, config);

					return new RelationshipIndexRepresentation( indexName, config );
			  }

			  _graphDb.index().forRelationships(indexName);
			  return new RelationshipIndexRepresentation( indexName, Collections.emptyMap() );
		 }

		 public virtual void RemoveNodeIndex( string indexName )
		 {
			  if ( !_graphDb.index().existsForNodes(indexName) )
			  {
					throw new NotFoundException( "No node index named '" + indexName + "'." );
			  }

			  _graphDb.index().forNodes(indexName).delete();
		 }

		 public virtual void RemoveRelationshipIndex( string indexName )
		 {
			  if ( !_graphDb.index().existsForRelationships(indexName) )
			  {
					throw new NotFoundException( "No relationship index named '" + indexName + "'." );
			  }

			  _graphDb.index().forRelationships(indexName).delete();
		 }

		 public virtual bool NodeIsIndexed( string indexName, string key, object value, long nodeId )
		 {
			  Index<Node> index = _graphDb.index().forNodes(indexName);
			  Node expectedNode = _graphDb.getNodeById( nodeId );
			  using ( IndexHits<Node> hits = index.get( key, value ) )
			  {
					return IterableContains( hits, expectedNode );
			  }
		 }

		 public virtual bool RelationshipIsIndexed( string indexName, string key, object value, long relationshipId )
		 {

			  Index<Relationship> index = _graphDb.index().forRelationships(indexName);
			  Relationship expectedNode = _graphDb.getRelationshipById( relationshipId );
			  using ( IndexHits<Relationship> hits = index.get( key, value ) )
			  {
					return IterableContains( hits, expectedNode );
			  }
		 }

		 private bool IterableContains<T>( IEnumerable<T> iterable, T expectedElement )
		 {
			  foreach ( T possibleMatch in iterable )
			  {
					if ( possibleMatch.Equals( expectedElement ) )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 public virtual Representation IsAutoIndexerEnabled( string type )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.Neo4Net.graphdb.index.AutoIndexer<? extends org.Neo4Net.graphdb.PropertyContainer> index = getAutoIndexerForType(type);
			  AutoIndexer<PropertyContainer> index = GetAutoIndexerForType( type );
			  return ValueRepresentation.@bool( index.Enabled );
		 }

		 public virtual void SetAutoIndexerEnabled( string type, bool enable )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.Neo4Net.graphdb.index.AutoIndexer<? extends org.Neo4Net.graphdb.PropertyContainer> index = getAutoIndexerForType(type);
			  AutoIndexer<PropertyContainer> index = GetAutoIndexerForType( type );
			  index.Enabled = enable;
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.Neo4Net.graphdb.index.AutoIndexer<? extends org.Neo4Net.graphdb.PropertyContainer> getAutoIndexerForType(String type)
		 private AutoIndexer<PropertyContainer> GetAutoIndexerForType( string type )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.index.IndexManager indexManager = graphDb.index();
			  IndexManager indexManager = _graphDb.index();
			  switch ( type )
			  {
			  case "node":
					return indexManager.NodeAutoIndexer;
			  case "relationship":
					return indexManager.RelationshipAutoIndexer;
			  default:
					throw new System.ArgumentException( "invalid type " + type );
			  }
		 }

		 public virtual Representation GetAutoIndexedProperties( string type )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.Neo4Net.graphdb.index.AutoIndexer<? extends org.Neo4Net.graphdb.PropertyContainer> indexer = getAutoIndexerForType(type);
			  AutoIndexer<PropertyContainer> indexer = GetAutoIndexerForType( type );
			  return ListRepresentation.@string( indexer.AutoIndexedProperties );
		 }

		 public virtual void StartAutoIndexingProperty( string type, string property )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.Neo4Net.graphdb.index.AutoIndexer<? extends org.Neo4Net.graphdb.PropertyContainer> indexer = getAutoIndexerForType(type);
			  AutoIndexer<PropertyContainer> indexer = GetAutoIndexerForType( type );
			  indexer.StartAutoIndexingProperty( property );
		 }

		 public virtual void StopAutoIndexingProperty( string type, string property )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.Neo4Net.graphdb.index.AutoIndexer<? extends org.Neo4Net.graphdb.PropertyContainer> indexer = getAutoIndexerForType(type);
			  AutoIndexer<PropertyContainer> indexer = GetAutoIndexerForType( type );
			  indexer.StopAutoIndexingProperty( property );
		 }

		 // Relationships

		 public sealed class RelationshipDirection
		 {
			  public static readonly RelationshipDirection All = new RelationshipDirection( "All", InnerEnum.All, Neo4Net.GraphDb.Direction.Both );
			  public static readonly RelationshipDirection In = new RelationshipDirection( "In", InnerEnum.In, Neo4Net.GraphDb.Direction.Incoming );
			  public static readonly RelationshipDirection Out = new RelationshipDirection( "Out", InnerEnum.Out, Neo4Net.GraphDb.Direction.Outgoing );

			  private static readonly IList<RelationshipDirection> valueList = new List<RelationshipDirection>();

			  static RelationshipDirection()
			  {
				  valueList.Add( All );
				  valueList.Add( In );
				  valueList.Add( Out );
			  }

			  public enum InnerEnum
			  {
				  All,
				  In,
				  Out
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;
			  internal Final org;

			  internal RelationshipDirection( string name, InnerEnum innerEnum, Neo4Net.GraphDb.Direction @internal )
			  {
					this.Internal = @internal;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			 public static IList<RelationshipDirection> values()
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

			 public static RelationshipDirection valueOf( string name )
			 {
				 foreach ( RelationshipDirection enumInstance in RelationshipDirection.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.server.rest.repr.RelationshipRepresentation createRelationship(long startNodeId, long endNodeId, String type, java.util.Map<String,Object> properties) throws org.Neo4Net.server.rest.domain.StartNodeNotFoundException, org.Neo4Net.server.rest.domain.EndNodeNotFoundException, PropertyValueException
		 public virtual RelationshipRepresentation CreateRelationship( long startNodeId, long endNodeId, string type, IDictionary<string, object> properties )
		 {

			  Node start;
			  Node end;
			  try
			  {
					start = Node( startNodeId );
			  }
			  catch ( NodeNotFoundException e )
			  {
					throw new StartNodeNotFoundException( e );
			  }
			  try
			  {
					end = Node( endNodeId );
			  }
			  catch ( NodeNotFoundException e )
			  {
					throw new EndNodeNotFoundException( e );
			  }

			  Relationship rel = start.CreateRelationshipTo( end, RelationshipType.withName( type ) );

			  _propertySetter.setProperties( rel, properties );

			  return new RelationshipRepresentation( rel );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.server.rest.repr.RelationshipRepresentation getRelationship(long relationshipId) throws RelationshipNotFoundException
		 public virtual RelationshipRepresentation GetRelationship( long relationshipId )
		 {
			  return new RelationshipRepresentation( Relationship( relationshipId ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void deleteRelationship(long relationshipId) throws RelationshipNotFoundException
		 public virtual void DeleteRelationship( long relationshipId )
		 {
			  Relationship( relationshipId ).delete();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public org.Neo4Net.server.rest.repr.ListRepresentation getNodeRelationships(long nodeId, RelationshipDirection direction, java.util.Collection<String> types) throws NodeNotFoundException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual ListRepresentation GetNodeRelationships( long nodeId, RelationshipDirection direction, ICollection<string> types )
		 {
			  Node node = node( nodeId );
			  PathExpander expander;
			  if ( types.Count == 0 )
			  {
					expander = PathExpanders.forDirection( direction.Internal );
			  }
			  else
			  {
					PathExpanderBuilder builder = PathExpanderBuilder.empty();
					foreach ( string type in types )
					{
						 builder = builder.Add( RelationshipType.withName( type ), direction.Internal );
					}
					expander = builder.Build();
			  }
			  return RelationshipRepresentation.list( expander.expand( Paths.singleNodePath( node ), BranchState.NO_STATE ) );
		 }

		 // Node degrees

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public org.Neo4Net.server.rest.repr.Representation getNodeDegree(long nodeId, RelationshipDirection direction, java.util.Collection<String> types) throws NodeNotFoundException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual Representation GetNodeDegree( long nodeId, RelationshipDirection direction, ICollection<string> types )
		 {
			  Node node = node( nodeId );
			  if ( types.Count == 0 )
			  {
					return PropertiesRepresentation.value( node.GetDegree( direction.Internal ) );
			  }
			  else
			  {
					int sum = 0;
					foreach ( string type in types )
					{
						 sum += node.GetDegree( RelationshipType.withName( type ), direction.Internal );
					}
					return PropertiesRepresentation.value( sum );
			  }
		 }

		 // Relationship properties

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.server.rest.repr.PropertiesRepresentation getAllRelationshipProperties(long relationshipId) throws RelationshipNotFoundException
		 public virtual PropertiesRepresentation GetAllRelationshipProperties( long relationshipId )
		 {
			  return new PropertiesRepresentation( Relationship( relationshipId ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.server.rest.repr.Representation getRelationshipProperty(long relationshipId, String key) throws NoSuchPropertyException, RelationshipNotFoundException
		 public virtual Representation GetRelationshipProperty( long relationshipId, string key )
		 {
			  Relationship relationship = relationship( relationshipId );
			  try
			  {
					return PropertiesRepresentation.value( relationship.GetProperty( key ) );
			  }
			  catch ( NotFoundException )
			  {
					throw new NoSuchPropertyException( relationship, key );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void setAllRelationshipProperties(long relationshipId, java.util.Map<String,Object> properties) throws PropertyValueException, RelationshipNotFoundException
		 public virtual void SetAllRelationshipProperties( long relationshipId, IDictionary<string, object> properties )
		 {
			  _propertySetter.setAllProperties( Relationship( relationshipId ), properties );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void setRelationshipProperty(long relationshipId, String key, Object value) throws PropertyValueException, RelationshipNotFoundException
		 public virtual void SetRelationshipProperty( long relationshipId, string key, object value )
		 {
			  Relationship relationship = relationship( relationshipId );
			  _propertySetter.setProperty( relationship, key, value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void removeAllRelationshipProperties(long relationshipId) throws RelationshipNotFoundException, PropertyValueException
		 public virtual void RemoveAllRelationshipProperties( long relationshipId )
		 {
			  _propertySetter.setAllProperties( Relationship( relationshipId ), null );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void removeRelationshipProperty(long relationshipId, String key) throws RelationshipNotFoundException, NoSuchPropertyException
		 public virtual void RemoveRelationshipProperty( long relationshipId, string key )
		 {
			  Relationship relationship = relationship( relationshipId );

			  if ( relationship.RemoveProperty( key ) == null )
			  {
					throw new NoSuchPropertyException( relationship, key );
			  }
		 }

		 public virtual Representation NodeIndexRoot()
		 {
			  return new NodeIndexRootRepresentation( _graphDb.index() );
		 }

		 public virtual Representation RelationshipIndexRoot()
		 {
			  return new RelationshipIndexRootRepresentation( _graphDb.index() );
		 }

		 public virtual IndexedEntityRepresentation AddToRelationshipIndex( string indexName, string key, string value, long relationshipId )
		 {
			  Relationship relationship = _graphDb.getRelationshipById( relationshipId );
			  Index<Relationship> index = _graphDb.index().forRelationships(indexName);
			  index.Add( relationship, key, value );
			  return new IndexedEntityRepresentation( relationship, key, value, new RelationshipIndexRepresentation( indexName, Collections.emptyMap() ) );
		 }

		 public virtual IndexedEntityRepresentation AddToNodeIndex( string indexName, string key, string value, long nodeId )
		 {
			  Node node = _graphDb.getNodeById( nodeId );
			  Index<Node> index = _graphDb.index().forNodes(indexName);
			  index.Add( node, key, value );
			  return new IndexedEntityRepresentation( node, key, value, new NodeIndexRepresentation( indexName, Collections.emptyMap() ) );
		 }

		 public virtual void RemoveFromNodeIndex( string indexName, string key, string value, long id )
		 {
			  _graphDb.index().forNodes(indexName).remove(_graphDb.getNodeById(id), key, value);
		 }

		 public virtual void RemoveFromNodeIndexNoValue( string indexName, string key, long id )
		 {
			  _graphDb.index().forNodes(indexName).remove(_graphDb.getNodeById(id), key);
		 }

		 public virtual void RemoveFromNodeIndexNoKeyValue( string indexName, long id )
		 {
			  _graphDb.index().forNodes(indexName).remove(_graphDb.getNodeById(id));
		 }

		 public virtual void RemoveFromRelationshipIndex( string indexName, string key, string value, long id )
		 {
			  _graphDb.index().forRelationships(indexName).remove(_graphDb.getRelationshipById(id), key, value);
		 }

		 public virtual void RemoveFromRelationshipIndexNoValue( string indexName, string key, long id )
		 {
			  _graphDb.index().forRelationships(indexName).remove(_graphDb.getRelationshipById(id), key);
		 }

		 public virtual void RemoveFromRelationshipIndexNoKeyValue( string indexName, long id )
		 {
			  _graphDb.index().forRelationships(indexName).remove(_graphDb.getRelationshipById(id));
		 }

		 public virtual IndexedEntityRepresentation GetIndexedNode( string indexName, string key, string value, long id )
		 {
			  if ( !NodeIsIndexed( indexName, key, value, id ) )
			  {
					throw new NotFoundException();
			  }
			  Node node = _graphDb.getNodeById( id );
			  return new IndexedEntityRepresentation( node, key, value, new NodeIndexRepresentation( indexName, Collections.emptyMap() ) );
		 }

		 public virtual IndexedEntityRepresentation GetIndexedRelationship( string indexName, string key, string value, long id )
		 {
			  if ( !RelationshipIsIndexed( indexName, key, value, id ) )
			  {
					throw new NotFoundException();
			  }
			  Relationship node = _graphDb.getRelationshipById( id );
			  return new IndexedEntityRepresentation( node, key, value, new RelationshipIndexRepresentation( indexName, Collections.emptyMap() ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.Neo4Net.server.rest.repr.ListRepresentation getIndexedNodes(String indexName, final String key, final String value)
		 public virtual ListRepresentation GetIndexedNodes( string indexName, string key, string value )
		 {
			  if ( !_graphDb.index().existsForNodes(indexName) )
			  {
					throw new NotFoundException();
			  }

			  Index<Node> index = _graphDb.index().forNodes(indexName);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.server.rest.repr.IndexRepresentation indexRepresentation = new org.Neo4Net.server.rest.repr.NodeIndexRepresentation(indexName);
			  IndexRepresentation indexRepresentation = new NodeIndexRepresentation( indexName );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.index.IndexHits<org.Neo4Net.graphdb.Node> indexHits = index.get(key, value);
			  IndexHits<Node> indexHits = index.get( key, value );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.helpers.collection.IterableWrapper<org.Neo4Net.server.rest.repr.Representation,org.Neo4Net.graphdb.Node> results = new org.Neo4Net.helpers.collection.IterableWrapper<org.Neo4Net.server.rest.repr.Representation,org.Neo4Net.graphdb.Node>(indexHits)
			  IterableWrapper<Representation, Node> results = new IterableWrapperAnonymousInnerClass( this, indexHits, key, value, indexRepresentation );
			  return new ListRepresentation( RepresentationType.NODE, results );
		 }

		 private class IterableWrapperAnonymousInnerClass : IterableWrapper<Representation, Node>
		 {
			 private readonly DatabaseActions _outerInstance;

			 private string _key;
			 private string _value;
			 private IndexRepresentation _indexRepresentation;

			 public IterableWrapperAnonymousInnerClass( DatabaseActions outerInstance, IndexHits<Node> indexHits, string key, string value, IndexRepresentation indexRepresentation ) : base( indexHits )
			 {
				 this.outerInstance = outerInstance;
				 this._key = key;
				 this._value = value;
				 this._indexRepresentation = indexRepresentation;
			 }

			 protected internal override Representation underlyingObjectToObject( Node node )
			 {
				  return new IndexedEntityRepresentation( node, _key, _value, _indexRepresentation );
			 }
		 }

		 public virtual ListRepresentation GetIndexedNodesByQuery( string indexName, string query, string sort )
		 {
			  return GetIndexedNodesByQuery( indexName, null, query, sort );
		 }

		 public virtual ListRepresentation GetIndexedNodesByQuery( string indexName, string key, string query, string sort )
		 {
			  if ( !_graphDb.index().existsForNodes(indexName) )
			  {
					throw new NotFoundException();
			  }

			  if ( string.ReferenceEquals( query, null ) )
			  {
					return ToListNodeRepresentation();
			  }
			  Index<Node> index = _graphDb.index().forNodes(indexName);

			  IndexResultOrder order = GetOrdering( sort );
			  QueryContext queryCtx = order.updateQueryContext( new QueryContext( query ) );
			  IndexHits<Node> result = index.query( key, queryCtx );
			  return ToListNodeRepresentation( result, order );
		 }

		 private ListRepresentation ToListNodeRepresentation()
		 {
			  return new ListRepresentation( RepresentationType.NODE, Collections.emptyList() );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.Neo4Net.server.rest.repr.ListRepresentation toListNodeRepresentation(final org.Neo4Net.graphdb.index.IndexHits<org.Neo4Net.graphdb.Node> result, final IndexResultOrder order)
		 private ListRepresentation ToListNodeRepresentation( IndexHits<Node> result, IndexResultOrder order )
		 {
			  if ( result == null )
			  {
					return new ListRepresentation( RepresentationType.NODE, Collections.emptyList() );
			  }
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.helpers.collection.IterableWrapper<org.Neo4Net.server.rest.repr.Representation,org.Neo4Net.graphdb.Node> results = new org.Neo4Net.helpers.collection.IterableWrapper<org.Neo4Net.server.rest.repr.Representation,org.Neo4Net.graphdb.Node>(result)
			  IterableWrapper<Representation, Node> results = new IterableWrapperAnonymousInnerClass2( this, result, order );
			  return new ListRepresentation( RepresentationType.NODE, results );
		 }

		 private class IterableWrapperAnonymousInnerClass2 : IterableWrapper<Representation, Node>
		 {
			 private readonly DatabaseActions _outerInstance;

			 private IndexHits<Node> _result;
			 private Neo4Net.Server.rest.web.DatabaseActions.IndexResultOrder _order;

			 public IterableWrapperAnonymousInnerClass2( DatabaseActions outerInstance, IndexHits<Node> result, Neo4Net.Server.rest.web.DatabaseActions.IndexResultOrder order ) : base( result )
			 {
				 this.outerInstance = outerInstance;
				 this._result = result;
				 this._order = order;
			 }

			 protected internal override Representation underlyingObjectToObject( Node node )
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.server.rest.repr.NodeRepresentation nodeRepresentation = new org.Neo4Net.server.rest.repr.NodeRepresentation(node);
				  NodeRepresentation nodeRepresentation = new NodeRepresentation( node );
				  if ( _order == null )
				  {
						return nodeRepresentation;
				  }
				  return _order.getRepresentationFor( nodeRepresentation, _result.currentScore() );
			 }
		 }

		 private ListRepresentation ToListRelationshipRepresentation()
		 {
			  return new ListRepresentation( RepresentationType.RELATIONSHIP, Collections.emptyList() );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.Neo4Net.server.rest.repr.ListRepresentation toListRelationshipRepresentation(final org.Neo4Net.graphdb.index.IndexHits<org.Neo4Net.graphdb.Relationship> result, final IndexResultOrder order)
		 private ListRepresentation ToListRelationshipRepresentation( IndexHits<Relationship> result, IndexResultOrder order )
		 {
			  if ( result == null )
			  {
					return new ListRepresentation( RepresentationType.RELATIONSHIP, Collections.emptyList() );
			  }
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.helpers.collection.IterableWrapper<org.Neo4Net.server.rest.repr.Representation,org.Neo4Net.graphdb.Relationship> results = new org.Neo4Net.helpers.collection.IterableWrapper<org.Neo4Net.server.rest.repr.Representation, org.Neo4Net.graphdb.Relationship>(result)
			  IterableWrapper<Representation, Relationship> results = new IterableWrapperAnonymousInnerClass3( this, result, order );
			  return new ListRepresentation( RepresentationType.RELATIONSHIP, results );
		 }

		 private class IterableWrapperAnonymousInnerClass3 : IterableWrapper<Representation, Relationship>
		 {
			 private readonly DatabaseActions _outerInstance;

			 private IndexHits<Relationship> _result;
			 private Neo4Net.Server.rest.web.DatabaseActions.IndexResultOrder _order;

			 public IterableWrapperAnonymousInnerClass3( DatabaseActions outerInstance, IndexHits<Relationship> result, Neo4Net.Server.rest.web.DatabaseActions.IndexResultOrder order ) : base( result )
			 {
				 this.outerInstance = outerInstance;
				 this._result = result;
				 this._order = order;
			 }

			 protected internal override Representation underlyingObjectToObject( Relationship rel )
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.server.rest.repr.RelationshipRepresentation relationshipRepresentation = new org.Neo4Net.server.rest.repr.RelationshipRepresentation(rel);
				  RelationshipRepresentation relationshipRepresentation = new RelationshipRepresentation( rel );
				  if ( _order != null )
				  {
						return _order.getRepresentationFor( relationshipRepresentation, _result.currentScore() );
				  }
				  return relationshipRepresentation;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.helpers.collection.Pair<org.Neo4Net.server.rest.repr.IndexedEntityRepresentation,bool> getOrCreateIndexedNode(String indexName, String key, String value, System.Nullable<long> nodeOrNull, java.util.Map<String,Object> properties) throws org.Neo4Net.server.rest.repr.BadInputException, NodeNotFoundException
		 public virtual Pair<IndexedEntityRepresentation, bool> GetOrCreateIndexedNode( string indexName, string key, string value, long? nodeOrNull, IDictionary<string, object> properties )
		 {
			  AssertIsLegalIndexName( indexName );
			  Node result;
			  bool created;
			  if ( nodeOrNull != null )
			  {
					if ( properties != null )
					{
						 throw new InvalidArgumentsException( "Cannot specify properties for a new node, " + "when a node to index is specified." );
					}
					Node node = node( nodeOrNull );
					result = _graphDb.index().forNodes(indexName).putIfAbsent(node, key, value);
					created = result == null;
					if ( created )
					{
						 UniqueNodeFactory factory = new UniqueNodeFactory( this, indexName, properties );
						 UniqueFactory.UniqueEntity<Node> IEntity = factory.getOrCreateWithOutcome( key, value );
						 // when given a node id, return as created if that node was newly added to the index
						 created = IEntity.Entity().Id == node.Id || IEntity.WasCreated();
						 result = IEntity.Entity();
					}
			  }
			  else
			  {
					if ( properties != null )
					{
						 foreach ( KeyValuePair<string, object> entry in properties.SetOfKeyValuePairs() )
						 {
							  entry.Value = _propertySetter.convert( entry.Value );
						 }
					}
					UniqueNodeFactory factory = new UniqueNodeFactory( this, indexName, properties );
					UniqueFactory.UniqueEntity<Node> IEntity = factory.getOrCreateWithOutcome( key, value );
					result = IEntity.Entity();
					created = IEntity.WasCreated();
			  }
			  return Pair.of( new IndexedEntityRepresentation( result, key, value, new NodeIndexRepresentation( indexName, Collections.emptyMap() ) ), created );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.helpers.collection.Pair<org.Neo4Net.server.rest.repr.IndexedEntityRepresentation,bool> getOrCreateIndexedRelationship(String indexName, String key, String value, System.Nullable<long> relationshipOrNull, System.Nullable<long> startNode, String type, System.Nullable<long> endNode, java.util.Map<String,Object> properties) throws org.Neo4Net.server.rest.repr.BadInputException, RelationshipNotFoundException, NodeNotFoundException
		 public virtual Pair<IndexedEntityRepresentation, bool> GetOrCreateIndexedRelationship( string indexName, string key, string value, long? relationshipOrNull, long? startNode, string type, long? endNode, IDictionary<string, object> properties )
		 {
			  AssertIsLegalIndexName( indexName );
			  Relationship result;
			  bool created;
			  if ( relationshipOrNull != null )
			  {
					if ( startNode != null || !string.ReferenceEquals( type, null ) || endNode != null || properties != null )
					{
						 throw new InvalidArgumentsException( "Either specify a relationship to index uniquely, " + "or the means for creating it." );
					}
					Relationship relationship = relationship( relationshipOrNull );
					result = _graphDb.index().forRelationships(indexName).putIfAbsent(relationship, key, value);
					if ( created = result == null )
					{
						 UniqueRelationshipFactory factory = new UniqueRelationshipFactory( this, indexName, relationship.StartNode, relationship.EndNode, relationship.Type.name(), properties );
						 UniqueFactory.UniqueEntity<Relationship> IEntity = factory.getOrCreateWithOutcome( key, value );
						 // when given a relationship id, return as created if that relationship was newly added to the index
						 created = IEntity.Entity().Id == relationship.Id || IEntity.WasCreated();
						 result = IEntity.Entity();
					}
			  }
			  else if ( startNode == null || string.ReferenceEquals( type, null ) || endNode == null )
			  {
					throw new InvalidArgumentsException( "Either specify a relationship to index uniquely, " + "or the means for creating it." );
			  }
			  else
			  {
					UniqueRelationshipFactory factory = new UniqueRelationshipFactory( this, indexName, Node( startNode.Value ), Node( endNode.Value ), type, properties );
					UniqueFactory.UniqueEntity<Relationship> IEntity = factory.getOrCreateWithOutcome( key, value );
					result = IEntity.Entity();
					created = IEntity.WasCreated();
			  }
			  return Pair.of( new IndexedEntityRepresentation( result, key, value, new RelationshipIndexRepresentation( indexName, Collections.emptyMap() ) ), created );
		 }

		 private class UniqueRelationshipFactory : UniqueFactory.UniqueRelationshipFactory
		 {
			 private readonly DatabaseActions _outerInstance;

			  internal readonly Node Start;
			  internal readonly Node End;
			  internal readonly RelationshipType Type;
			  internal readonly IDictionary<string, object> Properties;

			  internal UniqueRelationshipFactory( DatabaseActions outerInstance, string index, Node start, Node end, string type, IDictionary<string, object> properties ) : base( outerInstance.graphDb, index )
			  {
				  this._outerInstance = outerInstance;
					this.Start = start;
					this.End = end;
					this.Type = RelationshipType.withName( type );
					this.Properties = properties;
			  }

			  protected internal override Relationship Create( IDictionary<string, object> ignored )
			  {
					return Start.createRelationshipTo( End, Type );
			  }

			  protected internal override void Initialize( Relationship relationship, IDictionary<string, object> indexed )
			  {
					foreach ( KeyValuePair<string, object> property in ( Properties == null ? indexed : Properties ).entrySet() )
					{
						 relationship.SetProperty( property.Key, property.Value );
					}
			  }
		 }

		 private class UniqueNodeFactory : UniqueFactory.UniqueNodeFactory
		 {
			 private readonly DatabaseActions _outerInstance;

			  internal readonly IDictionary<string, object> Properties;

			  internal UniqueNodeFactory( DatabaseActions outerInstance, string index, IDictionary<string, object> properties ) : base( outerInstance.graphDb, index )
			  {
				  this._outerInstance = outerInstance;
					this.Properties = properties;
			  }

			  protected internal override void Initialize( Node node, IDictionary<string, object> indexed )
			  {
					foreach ( KeyValuePair<string, object> property in ( Properties == null ? indexed : Properties ).entrySet() )
					{
						 node.SetProperty( property.Key, property.Value );
					}
			  }
		 }

		 public virtual Representation GetAutoIndexedNodes( string key, string value )
		 {
			  ReadableIndex<Node> index = _graphDb.index().NodeAutoIndexer.AutoIndex;

			  return ToListNodeRepresentation( index.Get( key, value ), null );
		 }

		 public virtual ListRepresentation GetAutoIndexedNodesByQuery( string query )
		 {
			  if ( !string.ReferenceEquals( query, null ) )
			  {
					ReadableIndex<Node> index = _graphDb.index().NodeAutoIndexer.AutoIndex;
					return ToListNodeRepresentation( index.Query( query ), null );
			  }
			  return ToListNodeRepresentation();
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public org.Neo4Net.server.rest.repr.ListRepresentation getIndexedRelationships(String indexName, final String key, final String value)
		 public virtual ListRepresentation GetIndexedRelationships( string indexName, string key, string value )
		 {
			  if ( !_graphDb.index().existsForRelationships(indexName) )
			  {
					throw new NotFoundException();
			  }

			  Index<Relationship> index = _graphDb.index().forRelationships(indexName);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.server.rest.repr.IndexRepresentation indexRepresentation = new org.Neo4Net.server.rest.repr.RelationshipIndexRepresentation(indexName);
			  IndexRepresentation indexRepresentation = new RelationshipIndexRepresentation( indexName );

			  IterableWrapper<Representation, Relationship> result = new IterableWrapperAnonymousInnerClass( this, index.get( key, value ), key, value, indexRepresentation );
			  return new ListRepresentation( RepresentationType.RELATIONSHIP, result );
		 }

		 private class IterableWrapperAnonymousInnerClass : IterableWrapper<Representation, Relationship>
		 {
			 private readonly DatabaseActions _outerInstance;

			 private string _key;
			 private string _value;
			 private IndexRepresentation _indexRepresentation;

			 public IterableWrapperAnonymousInnerClass( DatabaseActions outerInstance, UnknownType get, string key, string value, IndexRepresentation indexRepresentation ) : base( get )
			 {
				 this.outerInstance = outerInstance;
				 this._key = key;
				 this._value = value;
				 this._indexRepresentation = indexRepresentation;
			 }

			 protected internal override Representation underlyingObjectToObject( Relationship relationship )
			 {
				  return new IndexedEntityRepresentation( relationship, _key, _value, _indexRepresentation );
			 }
		 }

		 public virtual ListRepresentation GetIndexedRelationshipsByQuery( string indexName, string query, string sort )
		 {
			  return GetIndexedRelationshipsByQuery( indexName, null, query, sort );
		 }

		 public virtual ListRepresentation GetIndexedRelationshipsByQuery( string indexName, string key, string query, string sort )
		 {
			  if ( !_graphDb.index().existsForRelationships(indexName) )
			  {
					throw new NotFoundException();
			  }

			  if ( string.ReferenceEquals( query, null ) )
			  {
					return ToListRelationshipRepresentation();
			  }
			  Index<Relationship> index = _graphDb.index().forRelationships(indexName);

			  IndexResultOrder order = GetOrdering( sort );
			  QueryContext queryCtx = order.updateQueryContext( new QueryContext( query ) );

			  return ToListRelationshipRepresentation( index.query( key, queryCtx ), order );
		 }

		 public virtual Representation GetAutoIndexedRelationships( string key, string value )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.index.ReadableIndex<org.Neo4Net.graphdb.Relationship> index = graphDb.index().getRelationshipAutoIndexer().getAutoIndex();
			  ReadableIndex<Relationship> index = _graphDb.index().RelationshipAutoIndexer.AutoIndex;
			  return ToListRelationshipRepresentation( index.Get( key, value ), null );
		 }

		 public virtual ListRepresentation GetAutoIndexedRelationshipsByQuery( string query )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.index.ReadableIndex<org.Neo4Net.graphdb.Relationship> index = graphDb.index().getRelationshipAutoIndexer().getAutoIndex();
			  ReadableIndex<Relationship> index = _graphDb.index().RelationshipAutoIndexer.AutoIndex;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.index.IndexHits<org.Neo4Net.graphdb.Relationship> results = query != null ? index.query(query) : null;
			  IndexHits<Relationship> results = !string.ReferenceEquals( query, null ) ? index.Query( query ) : null;
			  return ToListRelationshipRepresentation( results, null );
		 }

		 // Graph algos

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("rawtypes") public org.Neo4Net.server.rest.repr.PathRepresentation findSinglePath(long startId, long endId, java.util.Map<String,Object> map)
		 public virtual PathRepresentation FindSinglePath( long startId, long endId, IDictionary<string, object> map )
		 {
			  FindParams findParams = ( new FindParams( this, startId, endId, map ) ).Invoke();
			  PathFinder finder = findParams.Finder;
			  Node startNode = findParams.StartNode;
			  Node endNode = findParams.EndNode;

			  Path path = finder.findSinglePath( startNode, endNode );
			  if ( path == null )
			  {
					throw new NotFoundException();
			  }
			  return findParams.PathRepresentationOf( path );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({"rawtypes", "unchecked"}) public org.Neo4Net.server.rest.repr.ListRepresentation findPaths(long startId, long endId, java.util.Map<String,Object> map)
		 public virtual ListRepresentation FindPaths( long startId, long endId, IDictionary<string, object> map )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final FindParams findParams = new FindParams(startId, endId, map).invoke();
			  FindParams findParams = ( new FindParams( this, startId, endId, map ) ).Invoke();
			  PathFinder finder = findParams.Finder;
			  Node startNode = findParams.StartNode;
			  Node endNode = findParams.EndNode;

			  System.Collections.IEnumerable paths = finder.findAllPaths( startNode, endNode );

			  IterableWrapper<PathRepresentation, Path> pathRepresentations = new IterableWrapperAnonymousInnerClass2( this, paths, findParams );

			  return new ListRepresentation( RepresentationType.PATH, pathRepresentations );
		 }

		 private class IterableWrapperAnonymousInnerClass2 : IterableWrapper<PathRepresentation, Path>
		 {
			 private readonly DatabaseActions _outerInstance;

			 private Neo4Net.Server.rest.web.DatabaseActions.FindParams _findParams;

			 public IterableWrapperAnonymousInnerClass2( DatabaseActions outerInstance, System.Collections.IEnumerable paths, Neo4Net.Server.rest.web.DatabaseActions.FindParams findParams ) : base( paths )
			 {
				 this.outerInstance = outerInstance;
				 this._findParams = findParams;
			 }

			 protected internal override PathRepresentation underlyingObjectToObject( Path path )
			 {
				  return _findParams.pathRepresentationOf( path );
			 }
		 }

		 private class FindParams
		 {
			 private readonly DatabaseActions _outerInstance;

			  internal readonly long StartId;
			  internal readonly long EndId;
			  internal readonly IDictionary<string, object> Map;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal Node StartNodeConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal Node EndNodeConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.Neo4Net.graphalgo.PathFinder<? extends org.Neo4Net.graphdb.Path> finder;
			  internal PathFinder<Path> FinderConflict;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("rawtypes") private PathRepresentationCreator representationCreator = PATH_REPRESENTATION_CREATOR;
			  internal PathRepresentationCreator RepresentationCreator = _pathRepresentationCreator;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: FindParams(final long startId, final long endId, final java.util.Map<String,Object> map)
			  internal FindParams( DatabaseActions outerInstance, long startId, long endId, IDictionary<string, object> map )
			  {
				  this._outerInstance = outerInstance;
					this.StartId = startId;
					this.EndId = endId;
					this.Map = map;
			  }

			  public virtual Node StartNode
			  {
				  get
				  {
						return StartNodeConflict;
				  }
			  }

			  public virtual Node EndNode
			  {
				  get
				  {
						return EndNodeConflict;
				  }
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public org.Neo4Net.graphalgo.PathFinder<? extends org.Neo4Net.graphdb.Path> getFinder()
			  public virtual PathFinder<Path> Finder
			  {
				  get
				  {
						return FinderConflict;
				  }
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public org.Neo4Net.server.rest.repr.PathRepresentation<? extends org.Neo4Net.graphdb.Path> pathRepresentationOf(org.Neo4Net.graphdb.Path path)
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
			  public virtual PathRepresentation<Path> PathRepresentationOf( Path path )
			  {
					return RepresentationCreator.from( path );
			  }

			  public virtual FindParams Invoke()
			  {
					StartNodeConflict = outerInstance.graphDb.GetNodeById( StartId );
					EndNodeConflict = outerInstance.graphDb.GetNodeById( EndId );

					int? maxDepthObj = ( int? ) Map["max_depth"];
					int maxDepth = ( maxDepthObj != null ) ? maxDepthObj.Value : 1;

					PathExpander expander = RelationshipExpanderBuilder.describeRelationships( Map );

					string algorithm = ( string ) Map["algorithm"];
					algorithm = ( !string.ReferenceEquals( algorithm, null ) ) ? algorithm : "shortestPath";

					FinderConflict = GetAlgorithm( algorithm, expander, maxDepth );
					return this;
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.Neo4Net.graphalgo.PathFinder<? extends org.Neo4Net.graphdb.Path> getAlgorithm(String algorithm, org.Neo4Net.graphdb.PathExpander expander, int maxDepth)
			  internal virtual PathFinder<Path> GetAlgorithm( string algorithm, PathExpander expander, int maxDepth )
			  {
					switch ( algorithm )
					{
					case "shortestPath":
						 return GraphAlgoFactory.shortestPath( expander, maxDepth );
					case "allSimplePaths":
						 return GraphAlgoFactory.allSimplePaths( expander, maxDepth );
					case "allPaths":
						 return GraphAlgoFactory.allPaths( expander, maxDepth );
					case "dijkstra":
						 string costProperty = ( string ) Map["cost_property"];
						 Number defaultCost = ( Number ) Map["default_cost"];
						 CostEvaluator<double> costEvaluator = defaultCost == null ? CommonEvaluators.doubleCostEvaluator( costProperty ) : CommonEvaluators.doubleCostEvaluator( costProperty, defaultCost.doubleValue() );
						 RepresentationCreator = _weightedPathRepresentationCreator;
						 return GraphAlgoFactory.dijkstra( expander, costEvaluator );
					default:
						 throw new Exception( "Failed to find matching algorithm" );
					}
			  }
		 }

		 /*
		  * This enum binds the parameter-string-to-result-order mapping and
		  * the kind of results returned. This is not correct in general but
		  * at the time of writing it is the way things are done and is
		  * quite handy. Feel free to rip out if requirements change.
		  */
		 private abstract class IndexResultOrder
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           INDEX_ORDER { QueryContext updateQueryContext(org.Neo4Net.index.lucene.QueryContext original) { return original.sort(org.apache.lucene.search.Sort.INDEXORDER); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           RELEVANCE_ORDER { QueryContext updateQueryContext(org.Neo4Net.index.lucene.QueryContext original) { return original.sort(org.apache.lucene.search.Sort.RELEVANCE); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           SCORE_ORDER { QueryContext updateQueryContext(org.Neo4Net.index.lucene.QueryContext original) { return original.sortByScore(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           NONE { Representation getRepresentationFor(org.Neo4Net.server.rest.repr.Representation delegate, float score) { return delegate; } QueryContext updateQueryContext(org.Neo4Net.index.lucene.QueryContext original) { return original; } };

			  private static readonly IList<IndexResultOrder> valueList = new List<IndexResultOrder>();

			  static IndexResultOrder()
			  {
				  valueList.Add( INDEX_ORDER );
				  valueList.Add( RELEVANCE_ORDER );
				  valueList.Add( SCORE_ORDER );
				  valueList.Add( NONE );
			  }

			  public enum InnerEnum
			  {
				  INDEX_ORDER,
				  RELEVANCE_ORDER,
				  SCORE_ORDER,
				  NONE
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private IndexResultOrder( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal Neo4Net.Server.rest.repr.Representation GetRepresentationFor( Neo4Net.Server.rest.repr.Representation @delegate, float score )
			  {
					if ( @delegate is NodeRepresentation )
					{
						 return new ScoredNodeRepresentation( ( NodeRepresentation ) @delegate, score );
					}
					if ( @delegate is RelationshipRepresentation )
					{
						 return new ScoredRelationshipRepresentation( ( RelationshipRepresentation ) @delegate, score );
					}
					return @delegate;
			  }

			  internal abstract Neo4Net.Index.lucene.QueryContext updateQueryContext( Neo4Net.Index.lucene.QueryContext original );

			 public static IList<IndexResultOrder> values()
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

			 public static IndexResultOrder valueOf( string name )
			 {
				 foreach ( IndexResultOrder enumInstance in IndexResultOrder.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 private IndexResultOrder GetOrdering( string order )
		 {
			  if ( INDEX_ORDER.Equals( order, StringComparison.OrdinalIgnoreCase ) )
			  {
					return IndexResultOrder.IndexOrder;
			  }
			  else if ( RELEVANCE_ORDER.Equals( order, StringComparison.OrdinalIgnoreCase ) )
			  {
					return IndexResultOrder.RelevanceOrder;
			  }
			  else if ( SCORE_ORDER.Equals( order, StringComparison.OrdinalIgnoreCase ) )
			  {
					return IndexResultOrder.ScoreOrder;
			  }
			  else
			  {
					return IndexResultOrder.None;
			  }
		 }

		 private interface PathRepresentationCreator<T> where T : Neo4Net.GraphDb.Path
		 {
			  PathRepresentation<T> From( T path );
		 }

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		 private static readonly PathRepresentationCreator<Path> _pathRepresentationCreator = PathRepresentation::new;

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		 private static readonly PathRepresentationCreator<WeightedPath> _weightedPathRepresentationCreator = WeightedPathRepresentation::new;

		 private void AssertIsLegalIndexName( string indexName )
		 {
			  if ( string.ReferenceEquals( indexName, null ) || indexName.Equals( "" ) )
			  {
					throw new System.ArgumentException( "Index name must not be empty." );
			  }
		 }

		 public virtual ListRepresentation GetNodesWithLabel( string labelName, IDictionary<string, object> properties )
		 {
			  IEnumerator<Node> nodes;

			  if ( properties.Count == 0 )
			  {
					nodes = _graphDb.findNodes( label( labelName ) );
			  }
			  else if ( properties.Count == 1 )
			  {
					KeyValuePair<string, object> prop = Iterables.single( properties.SetOfKeyValuePairs() );
					nodes = _graphDb.findNodes( label( labelName ), prop.Key, prop.Value );
			  }
			  else
			  {
					throw new System.ArgumentException( "Too many properties specified. Either specify one property to " + "filter by, or none at all." );
			  }

			  IterableWrapper<NodeRepresentation, Node> nodeRepresentations = new IterableWrapperAnonymousInnerClass( this, asList( nodes ) );

			  return new ListRepresentation( RepresentationType.NODE, nodeRepresentations );
		 }

		 private class IterableWrapperAnonymousInnerClass : IterableWrapper<NodeRepresentation, Node>
		 {
			 private readonly DatabaseActions _outerInstance;

			 public IterableWrapperAnonymousInnerClass( DatabaseActions outerInstance, UnknownType asList ) : base( asList )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override NodeRepresentation underlyingObjectToObject( Node node )
			 {
				  return new NodeRepresentation( node );
			 }
		 }

		 public virtual ListRepresentation GetAllLabels( bool inUse )
		 {
			  ResourceIterable<Label> labels = inUse ? _graphDb.AllLabelsInUse : _graphDb.AllLabels;
			  ICollection<ValueRepresentation> labelNames = Iterables.asSet( map( label => ValueRepresentation.@string( label.name() ), labels ) );

			  return new ListRepresentation( RepresentationType.STRING, labelNames );
		 }

		 public virtual IndexDefinitionRepresentation CreateSchemaIndex( string labelName, IEnumerable<string> propertyKey )
		 {
			  IndexCreator indexCreator = _graphDb.schema().indexFor(label(labelName));
			  foreach ( string key in propertyKey )
			  {
					indexCreator = indexCreator.On( key );
			  }
			  return new IndexDefinitionRepresentation( indexCreator.Create() );
		 }

		 public virtual ListRepresentation SchemaIndexes
		 {
			 get
			 {
				  IEnumerable<IndexDefinition> definitions = _graphDb.schema().Indexes;
				  IEnumerable<IndexDefinitionRepresentation> representations = map( definition => new IndexDefinitionRepresentation( definition, _graphDb.schema().getIndexState(definition), _graphDb.schema().getIndexPopulationProgress(definition) ), definitions );
				  return new ListRepresentation( RepresentationType.INDEX_DEFINITION, representations );
			 }
		 }

		 public virtual ListRepresentation getSchemaIndexes( string labelName )
		 {
			  IEnumerable<IndexDefinition> definitions = _graphDb.schema().getIndexes(label(labelName));
			  IEnumerable<IndexDefinitionRepresentation> representations = map( definition => new IndexDefinitionRepresentation( definition, _graphDb.schema().getIndexState(definition), _graphDb.schema().getIndexPopulationProgress(definition) ), definitions );
			  return new ListRepresentation( RepresentationType.INDEX_DEFINITION, representations );
		 }

		 public virtual bool DropSchemaIndex( string labelName, string propertyKey )
		 {
			  bool found = false;
			  foreach ( IndexDefinition index in _graphDb.schema().getIndexes(label(labelName)) )
			  {
					// TODO Assumption about single property key
					if ( propertyKey.Equals( Iterables.single( index.PropertyKeys ) ) )
					{
						 index.Drop();
						 found = true;
						 break;
					}
			  }
			  return found;
		 }

		 public virtual ConstraintDefinitionRepresentation CreatePropertyUniquenessConstraint( string labelName, IEnumerable<string> propertyKeys )
		 {
			  ConstraintCreator constraintCreator = _graphDb.schema().constraintFor(label(labelName));
			  foreach ( string key in propertyKeys )
			  {
					constraintCreator = constraintCreator.AssertPropertyIsUnique( key );
			  }
			  ConstraintDefinition constraintDefinition = constraintCreator.Create();
			  return new ConstraintDefinitionRepresentation( constraintDefinition );
		 }

		 public virtual bool DropPropertyUniquenessConstraint( string labelName, IEnumerable<string> propertyKeys )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Set<String> propertyKeysSet = org.Neo4Net.helpers.collection.Iterables.asSet(propertyKeys);
			  ISet<string> propertyKeysSet = Iterables.asSet( propertyKeys );
			  ConstraintDefinition constraint = Iterables.singleOrNull( FilteredNodeConstraints( labelName, PropertyUniquenessFilter( propertyKeysSet ) ) );
			  if ( constraint != null )
			  {
					constraint.Drop();
			  }
			  return constraint != null;
		 }

		 public virtual bool DropNodePropertyExistenceConstraint( string labelName, IEnumerable<string> propertyKeys )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Set<String> propertyKeysSet = org.Neo4Net.helpers.collection.Iterables.asSet(propertyKeys);
			  ISet<string> propertyKeysSet = Iterables.asSet( propertyKeys );
			  ConstraintDefinition constraint = Iterables.singleOrNull( FilteredNodeConstraints( labelName, NodePropertyExistenceFilter( propertyKeysSet ) ) );
			  if ( constraint != null )
			  {
					constraint.Drop();
			  }
			  return constraint != null;
		 }

		 public virtual bool DropRelationshipPropertyExistenceConstraint( string typeName, IEnumerable<string> propertyKeys )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Set<String> propertyKeysSet = org.Neo4Net.helpers.collection.Iterables.asSet(propertyKeys);
			  ISet<string> propertyKeysSet = Iterables.asSet( propertyKeys );
			  ConstraintDefinition constraint = Iterables.singleOrNull( FilteredRelationshipConstraints( typeName, RelationshipPropertyExistenceFilter( propertyKeysSet ) ) );
			  if ( constraint != null )
			  {
					constraint.Drop();
			  }
			  return constraint != null;
		 }

		 public virtual ListRepresentation GetNodePropertyExistenceConstraint( string labelName, IEnumerable<string> propertyKeys )
		 {
			  ISet<string> propertyKeysSet = Iterables.asSet( propertyKeys );
			  IEnumerable<ConstraintDefinition> constraints = FilteredNodeConstraints( labelName, NodePropertyExistenceFilter( propertyKeysSet ) );
			  if ( constraints.GetEnumerator().hasNext() )
			  {
					IEnumerable<Representation> representationIterable = map( _constraintDefToRepresentation, constraints );
					return new ListRepresentation( CONSTRAINT_DEFINITION, representationIterable );
			  }
			  else
			  {
					throw new System.ArgumentException( string.Format( "Constraint with label {0} for properties {1} does not exist", labelName, propertyKeys ) );
			  }
		 }

		 public virtual ListRepresentation GetRelationshipPropertyExistenceConstraint( string typeName, IEnumerable<string> propertyKeys )
		 {
			  ISet<string> propertyKeysSet = Iterables.asSet( propertyKeys );
			  IEnumerable<ConstraintDefinition> constraints = FilteredRelationshipConstraints( typeName, RelationshipPropertyExistenceFilter( propertyKeysSet ) );
			  if ( constraints.GetEnumerator().hasNext() )
			  {
					IEnumerable<Representation> representationIterable = map( _constraintDefToRepresentation, constraints );
					return new ListRepresentation( CONSTRAINT_DEFINITION, representationIterable );
			  }
			  else
			  {
					throw new System.ArgumentException( string.Format( "Constraint with relationship type {0} for properties {1} does not exist", typeName, propertyKeys ) );
			  }
		 }

		 public virtual ListRepresentation GetPropertyUniquenessConstraint( string labelName, IEnumerable<string> propertyKeys )
		 {
			  ISet<string> propertyKeysSet = Iterables.asSet( propertyKeys );
			  IEnumerable<ConstraintDefinition> constraints = FilteredNodeConstraints( labelName, PropertyUniquenessFilter( propertyKeysSet ) );
			  if ( constraints.GetEnumerator().hasNext() )
			  {
					IEnumerable<Representation> representationIterable = map( _constraintDefToRepresentation, constraints );
					return new ListRepresentation( CONSTRAINT_DEFINITION, representationIterable );
			  }
			  else
			  {
					throw new System.ArgumentException( string.Format( "Constraint with label {0} for properties {1} does not exist", labelName, propertyKeys ) );
			  }
		 }

		 private IEnumerable<ConstraintDefinition> FilteredNodeConstraints( string labelName, System.Predicate<ConstraintDefinition> filter )
		 {
			  IEnumerable<ConstraintDefinition> constraints = _graphDb.schema().getConstraints(label(labelName));
			  return filter( filter, constraints );
		 }

		 private IEnumerable<ConstraintDefinition> FilteredRelationshipConstraints( string typeName, System.Predicate<ConstraintDefinition> filter )
		 {
			  RelationshipType type = RelationshipType.withName( typeName );
			  IEnumerable<ConstraintDefinition> constraints = _graphDb.schema().getConstraints(type);
			  return filter( filter, constraints );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Iterable<org.Neo4Net.graphdb.schema.ConstraintDefinition> filteredNodeConstraints(String labelName, final org.Neo4Net.graphdb.schema.ConstraintType type)
		 private IEnumerable<ConstraintDefinition> FilteredNodeConstraints( string labelName, ConstraintType type )
		 {
			  return filter( item => item.isConstraintType( type ), _graphDb.schema().getConstraints(label(labelName)) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private Iterable<org.Neo4Net.graphdb.schema.ConstraintDefinition> filteredRelationshipConstraints(String typeName, final org.Neo4Net.graphdb.schema.ConstraintType type)
		 private IEnumerable<ConstraintDefinition> FilteredRelationshipConstraints( string typeName, ConstraintType type )
		 {
			  return filter( item => item.isConstraintType( type ), _graphDb.schema().getConstraints(RelationshipType.withName(typeName)) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private System.Predicate<org.Neo4Net.graphdb.schema.ConstraintDefinition> propertyUniquenessFilter(final java.util.Set<String> propertyKeysSet)
		 private System.Predicate<ConstraintDefinition> PropertyUniquenessFilter( ISet<string> propertyKeysSet )
		 {
			  return item => item.isConstraintType( ConstraintType.UNIQUENESS ) && propertyKeysSet.SetEquals( Iterables.asSet( item.PropertyKeys ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private System.Predicate<org.Neo4Net.graphdb.schema.ConstraintDefinition> nodePropertyExistenceFilter(final java.util.Set<String> propertyKeysSet)
		 private System.Predicate<ConstraintDefinition> NodePropertyExistenceFilter( ISet<string> propertyKeysSet )
		 {
			  return item => item.isConstraintType( ConstraintType.NODE_PROPERTY_EXISTENCE ) && propertyKeysSet.SetEquals( Iterables.asSet( item.PropertyKeys ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private System.Predicate<org.Neo4Net.graphdb.schema.ConstraintDefinition> relationshipPropertyExistenceFilter(final java.util.Set<String> propertyKeysSet)
		 private System.Predicate<ConstraintDefinition> RelationshipPropertyExistenceFilter( ISet<string> propertyKeysSet )
		 {
			  return item => item.isConstraintType( ConstraintType.RELATIONSHIP_PROPERTY_EXISTENCE ) && propertyKeysSet.SetEquals( Iterables.asSet( item.PropertyKeys ) );
		 }

		 public virtual ListRepresentation Constraints
		 {
			 get
			 {
				  return new ListRepresentation( CONSTRAINT_DEFINITION, map( _constraintDefToRepresentation, _graphDb.schema().Constraints ) );
			 }
		 }

		 public virtual ListRepresentation GetLabelConstraints( string labelName )
		 {
			  return new ListRepresentation( CONSTRAINT_DEFINITION, map( _constraintDefToRepresentation, FilteredNodeConstraints( labelName, Predicates.alwaysTrue() ) ) );
		 }

		 public virtual Representation GetLabelUniquenessConstraints( string labelName )
		 {
			  return new ListRepresentation( CONSTRAINT_DEFINITION, map( _constraintDefToRepresentation, FilteredNodeConstraints( labelName, ConstraintType.UNIQUENESS ) ) );
		 }

		 public virtual Representation GetLabelExistenceConstraints( string labelName )
		 {
			  return new ListRepresentation( CONSTRAINT_DEFINITION, map( _constraintDefToRepresentation, FilteredNodeConstraints( labelName, ConstraintType.NODE_PROPERTY_EXISTENCE ) ) );
		 }

		 public virtual Representation GetRelationshipTypeExistenceConstraints( string typeName )
		 {
			  return new ListRepresentation( CONSTRAINT_DEFINITION, map( _constraintDefToRepresentation, FilteredRelationshipConstraints( typeName, ConstraintType.RELATIONSHIP_PROPERTY_EXISTENCE ) ) );
		 }
	}

}