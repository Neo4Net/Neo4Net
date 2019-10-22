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
namespace Neo4Net.Test
{

	using Direction = Neo4Net.GraphDb.Direction;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using TransactionFailureException = Neo4Net.GraphDb.TransactionFailureException;
	using GraphDatabaseBuilder = Neo4Net.GraphDb.factory.GraphDatabaseBuilder;
	using Neo4Net.GraphDb.index;
	using Neo4Net.GraphDb.index;
	using ConstraintDefinition = Neo4Net.GraphDb.schema.ConstraintDefinition;
	using IndexDefinition = Neo4Net.GraphDb.schema.IndexDefinition;
	using Config = Neo4Net.Kernel.configuration.Config;
	using IoPrimitiveUtils = Neo4Net.Kernel.impl.util.IoPrimitiveUtils;

	public class DbRepresentation
	{
		 private readonly IDictionary<long, NodeRep> _nodes = new SortedDictionary<long, NodeRep>();
		 private readonly ISet<IndexDefinition> _schemaIndexes = new HashSet<IndexDefinition>();
		 private readonly ISet<ConstraintDefinition> _constraints = new HashSet<ConstraintDefinition>();
		 private long _highestNodeId;
		 private long _highestRelationshipId;

		 public static DbRepresentation Of( IGraphDatabaseService db )
		 {
			  return of( db, true );
		 }

		 public static DbRepresentation Of( IGraphDatabaseService db, bool includeIndexes )
		 {
			  int retryCount = 5;
			  while ( true )
			  {
					try
					{
							using ( Transaction ignore = Db.beginTx() )
							{
							 DbRepresentation result = new DbRepresentation();
							 foreach ( Node node in Db.AllNodes )
							 {
								  NodeRep nodeRep = new NodeRep( db, node, includeIndexes );
								  result._nodes[node.Id] = nodeRep;
								  result._highestNodeId = Math.Max( node.Id, result._highestNodeId );
								  result._highestRelationshipId = Math.Max( nodeRep.HighestRelationshipId, result._highestRelationshipId );
      
							 }
							 foreach ( IndexDefinition indexDefinition in Db.schema().Indexes )
							 {
								  result._schemaIndexes.Add( indexDefinition );
							 }
							 foreach ( ConstraintDefinition constraintDefinition in Db.schema().Constraints )
							 {
								  result._constraints.Add( constraintDefinition );
							 }
							 return result;
							}
					}
					catch ( TransactionFailureException e )
					{
						 if ( retryCount-- < 0 )
						 {
							  throw e;
						 }
					}
			  }
		 }

		 public static DbRepresentation Of( File storeDir )
		 {
			  return Of( storeDir, true, Config.defaults() );
		 }

		 public static DbRepresentation Of( File storeDir, Config config )
		 {
			  return Of( storeDir, true, config );
		 }

		 public static DbRepresentation Of( File storeDir, bool includeIndexes, Config config )
		 {
			  GraphDatabaseBuilder builder = ( new TestGraphDatabaseFactory() ).NewEmbeddedDatabaseBuilder(storeDir);
			  builder.Config = config.Raw;

			  IGraphDatabaseService db = builder.NewGraphDatabase();
			  try
			  {
					return of( db, includeIndexes );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

		 public override bool Equals( object obj )
		 {
			  return CompareWith( ( DbRepresentation ) obj ).Count == 0;
		 }

		 // Accessed from HA-robustness, needs to be public
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("WeakerAccess") public java.util.Collection<String> compareWith(DbRepresentation other)
		 public virtual ICollection<string> CompareWith( DbRepresentation other )
		 {
			  ICollection<string> diffList = new List<string>();
			  DiffReport diff = new CollectionDiffReport( diffList );
			  NodeDiff( other, diff );
			  IndexDiff( other, diff );
			  ConstraintDiff( other, diff );
			  return diffList;
		 }

		 private void ConstraintDiff( DbRepresentation other, DiffReport diff )
		 {
			  foreach ( ConstraintDefinition constraint in _constraints )
			  {
					if ( !other._constraints.Contains( constraint ) )
					{
						 diff.Add( "I have constraint " + constraint + " which other doesn't" );
					}
			  }
			  foreach ( ConstraintDefinition otherConstraint in other._constraints )
			  {
					if ( !_constraints.Contains( otherConstraint ) )
					{
						 diff.Add( "Other has constraint " + otherConstraint + " which I don't" );
					}
			  }
		 }

		 private void IndexDiff( DbRepresentation other, DiffReport diff )
		 {
			  foreach ( IndexDefinition schemaIndex in _schemaIndexes )
			  {
					if ( !other._schemaIndexes.Contains( schemaIndex ) )
					{
						 diff.Add( "I have schema index " + schemaIndex + " which other doesn't" );
					}
			  }
			  foreach ( IndexDefinition otherSchemaIndex in other._schemaIndexes )
			  {
					if ( !_schemaIndexes.Contains( otherSchemaIndex ) )
					{
						 diff.Add( "Other has schema index " + otherSchemaIndex + " which I don't" );
					}
			  }
		 }

		 private void NodeDiff( DbRepresentation other, DiffReport diff )
		 {
			  foreach ( NodeRep node in _nodes.Values )
			  {
					NodeRep otherNode = other._nodes[node.Id];
					if ( otherNode == null )
					{
						 diff.Add( "I have node " + node.Id + " which other doesn't" );
						 continue;
					}
					node.CompareWith( otherNode, diff );
			  }

			  foreach ( long? id in other._nodes.Keys )
			  {
					if ( !_nodes.ContainsKey( id ) )
					{
						 diff.Add( "Other has node " + id + " which I don't" );
					}
			  }
		 }

		 public override int GetHashCode()
		 {
			  return _nodes.GetHashCode();
		 }

		 public override string ToString()
		 {
			  return _nodes.ToString();
		 }

		 private class NodeRep
		 {
			  internal readonly PropertiesRep Properties;
			  internal readonly IDictionary<long, PropertiesRep> OutRelationships = new Dictionary<long, PropertiesRep>();
			  internal readonly long HighestRelationshipId;
			  internal readonly long Id;
			  internal readonly IDictionary<string, IDictionary<string, Serializable>> Index;

			  internal NodeRep( IGraphDatabaseService db, Node node, bool includeIndexes )
			  {
					Id = node.Id;
					Properties = new PropertiesRep( node, node.Id );
					long highestRel = 0;
					foreach ( Relationship rel in node.GetRelationships( Direction.OUTGOING ) )
					{
						 OutRelationships[rel.Id] = new PropertiesRep( rel, rel.Id );
						 highestRel = Math.Max( highestRel, rel.Id );
					}
					this.HighestRelationshipId = highestRel;
					this.Index = includeIndexes ? CheckIndex( db ) : null;
			  }

			  internal virtual IDictionary<string, IDictionary<string, Serializable>> CheckIndex( IGraphDatabaseService db )
			  {
					IDictionary<string, IDictionary<string, Serializable>> result = new Dictionary<string, IDictionary<string, Serializable>>();
					foreach ( string indexName in Db.index().nodeIndexNames() )
					{
						 IDictionary<string, Serializable> thisIndex = new Dictionary<string, Serializable>();
						 Index<Node> tempIndex = Db.index().forNodes(indexName);
						 foreach ( KeyValuePair<string, Serializable> property in Properties.props.SetOfKeyValuePairs() )
						 {
							  using ( IndexHits<Node> content = tempIndex.get( property.Key, property.Value ) )
							  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
									if ( content.hasNext() )
									{
										 foreach ( Node hit in content )
										 {
											  if ( hit.Id == Id )
											  {
													thisIndex[property.Key] = property.Value;
													break;
											  }
										 }
									}
							  }
						 }
						 result[indexName] = thisIndex;
					}
					return result;
			  }

			  /*
			   * Yes, this is not the best way to do it - hash map does a deep equals. However,
			   * if things go wrong, this way give the ability to check where the inequality
			   * happened. If you feel strongly about this, feel free to change.
			   * Admittedly, the implementation could use some cleanup.
			   */
			  internal virtual void CompareIndex( NodeRep other, DiffReport diff )
			  {
					if ( other.Index == Index )
					{
						 return;
					}
					ICollection<string> allIndexes = new HashSet<string>();
					allIndexes.addAll( Index.Keys );
					allIndexes.addAll( other.Index.Keys );
					foreach ( string indexName in allIndexes )
					{
						 if ( !Index.ContainsKey( indexName ) )
						 {
							  diff.Add( this + " isn't indexed in " + indexName + " for mine" );
							  continue;
						 }
						 if ( !other.Index.ContainsKey( indexName ) )
						 {
							  diff.Add( this + " isn't indexed in " + indexName + " for other" );
							  continue;
						 }

						 IDictionary<string, Serializable> thisIndex = Index[indexName];
						 IDictionary<string, Serializable> otherIndex = other.Index[indexName];

						 if ( thisIndex.Count != otherIndex.Count )
						 {
							  diff.Add( "other index had a different mapping count than me for node " + this + " mine:" + thisIndex + ", other:" + otherIndex );
							  continue;
						 }

						 foreach ( KeyValuePair<string, Serializable> indexEntry in thisIndex.SetOfKeyValuePairs() )
						 {
							  if ( !indexEntry.Value.Equals( otherIndex[indexEntry.Key] ) )
							  {
									diff.Add( "other index had a different value indexed for " + indexEntry.Key + "=" + indexEntry.Value + ", namely " + otherIndex[indexEntry.Key] + " for " + this );
							  }
						 }
					}
			  }

			  internal virtual void CompareWith( NodeRep other, DiffReport diff )
			  {
					if ( other.Id != Id )
					{
						 diff.Add( "Id differs mine:" + Id + ", other:" + other.Id );
					}
					Properties.compareWith( other.Properties, diff );
					if ( Index != null && other.Index != null )
					{
						 CompareIndex( other, diff );
					}
					CompareRelationships( other, diff );
			  }

			  internal virtual void CompareRelationships( NodeRep other, DiffReport diff )
			  {
					foreach ( PropertiesRep rel in OutRelationships.Values )
					{
						 PropertiesRep otherRel = other.OutRelationships[rel.EntityId];
						 if ( otherRel == null )
						 {
							  diff.Add( "I have relationship " + rel.EntityId + " which other don't" );
							  continue;
						 }
						 rel.CompareWith( otherRel, diff );
					}

					foreach ( long? id in other.OutRelationships.Keys )
					{
						 if ( !OutRelationships.ContainsKey( id ) )
						 {
							  diff.Add( "Other has relationship " + id + " which I don't" );
						 }
					}
			  }

			  public override int GetHashCode()
			  {
					int result = 7;
					result += Properties.GetHashCode() * 7;
					result += OutRelationships.GetHashCode() * 13;
					result += ( int )( Id * 17 );
					if ( Index != null )
					{
						 result += Index.GetHashCode() * 19;
					}
					return result;
			  }

			  public override bool Equals( object o )
			  {
					if ( this == o )
					{
						 return true;
					}
					if ( o == null || this.GetType() != o.GetType() )
					{
						 return false;
					}
					NodeRep nodeRep = ( NodeRep ) o;
					return Id == nodeRep.Id && Objects.Equals( Properties, nodeRep.Properties ) && Objects.Equals( OutRelationships, nodeRep.OutRelationships ) && Objects.Equals( Index, nodeRep.Index );
			  }

			  public override string ToString()
			  {
					return "<id: " + Id + " props: " + Properties + ", rels: " + OutRelationships + ", index: " + Index + ">";
			  }
		 }

		 private class PropertiesRep
		 {
			  internal readonly IDictionary<string, Serializable> Props = new Dictionary<string, Serializable>();
			  internal readonly string IEntityToString;
			  internal readonly long IEntityId;

			  internal PropertiesRep( IPropertyContainer IEntity, long id )
			  {
					this.EntityId = id;
					this.EntityToString = IEntity.ToString();
					foreach ( string key in IEntity.PropertyKeys )
					{
						 Serializable value = ( Serializable ) IEntity.GetProperty( key, null );
						 // We do this because the node may have changed since we did getPropertyKeys()
						 if ( value != null )
						 {
							  if ( value.GetType().IsArray )
							  {
									Props[key] = new List<>( Arrays.asList( IoPrimitiveUtils.asArray( value ) ) );
							  }
							  else
							  {
									Props[key] = value;
							  }
						 }
					}
			  }

			  protected internal virtual void CompareWith( PropertiesRep other, DiffReport diff )
			  {
					bool equals = Props.Equals( other.Props );
					if ( !equals )
					{
						 diff.Add( "Properties diff for " + IEntityToString + " mine:" + Props + ", other:" + other.Props );
					}
			  }

			  public override int GetHashCode()
			  {
					return Props.GetHashCode();
			  }

			  public override bool Equals( object o )
			  {
					if ( this == o )
					{
						 return true;
					}
					if ( o == null || this.GetType() != o.GetType() )
					{
						 return false;
					}
					PropertiesRep that = ( PropertiesRep ) o;
					return Objects.Equals( Props, that.Props );
			  }

			  public override string ToString()
			  {
					return Props.ToString();
			  }
		 }

		 private interface DiffReport
		 {
			  void Add( string report );
		 }

		 private class CollectionDiffReport : DiffReport
		 {
			  internal readonly ICollection<string> Collection;

			  internal CollectionDiffReport( ICollection<string> collection )
			  {
					this.Collection = collection;
			  }

			  public override void Add( string report )
			  {
					Collection.Add( report );
			  }
		 }
	}

}