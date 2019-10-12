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
namespace Org.Neo4j.Kernel.builtinprocs
{

	using Direction = Org.Neo4j.Graphdb.Direction;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Org.Neo4j.Graphdb;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using IndexReference = Org.Neo4j.@internal.Kernel.Api.IndexReference;
	using Read = Org.Neo4j.@internal.Kernel.Api.Read;
	using SchemaRead = Org.Neo4j.@internal.Kernel.Api.SchemaRead;
	using TokenNameLookup = Org.Neo4j.@internal.Kernel.Api.TokenNameLookup;
	using TokenRead = Org.Neo4j.@internal.Kernel.Api.TokenRead;
	using ConstraintDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.constraints.ConstraintDescriptor;
	using KernelTransaction = Org.Neo4j.Kernel.api.KernelTransaction;
	using SilentTokenNameLookup = Org.Neo4j.Kernel.api.SilentTokenNameLookup;
	using Statement = Org.Neo4j.Kernel.api.Statement;
	using PropertyNameUtils = Org.Neo4j.Kernel.impl.coreapi.schema.PropertyNameUtils;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;

	public class SchemaProcedure
	{

		 private readonly GraphDatabaseAPI _graphDatabaseAPI;
		 private readonly KernelTransaction _kernelTransaction;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public SchemaProcedure(final org.neo4j.kernel.internal.GraphDatabaseAPI graphDatabaseAPI, final org.neo4j.kernel.api.KernelTransaction kernelTransaction)
		 public SchemaProcedure( GraphDatabaseAPI graphDatabaseAPI, KernelTransaction kernelTransaction )
		 {
			  this._graphDatabaseAPI = graphDatabaseAPI;
			  this._kernelTransaction = kernelTransaction;
		 }

		 public virtual GraphResult BuildSchemaGraph()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<String,VirtualNodeHack> nodes = new java.util.HashMap<>();
			  IDictionary<string, VirtualNodeHack> nodes = new Dictionary<string, VirtualNodeHack>();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<String,java.util.Set<VirtualRelationshipHack>> relationships = new java.util.HashMap<>();
			  IDictionary<string, ISet<VirtualRelationshipHack>> relationships = new Dictionary<string, ISet<VirtualRelationshipHack>>();

			  using ( Statement statement = _kernelTransaction.acquireStatement() )
			  {
					Read dataRead = _kernelTransaction.dataRead();
					TokenRead tokenRead = _kernelTransaction.tokenRead();
					TokenNameLookup tokenNameLookup = new SilentTokenNameLookup( tokenRead );
					SchemaRead schemaRead = _kernelTransaction.schemaRead();
					using ( Transaction transaction = _graphDatabaseAPI.beginTx() )
					{
						 // add all labelsInDatabase
						 using ( ResourceIterator<Label> labelsInDatabase = _graphDatabaseAPI.AllLabelsInUse.GetEnumerator() )
						 {
							  while ( labelsInDatabase.MoveNext() )
							  {
									Label label = labelsInDatabase.Current;
									int labelId = tokenRead.NodeLabel( label.Name() );
									IDictionary<string, object> properties = new Dictionary<string, object>();

									IEnumerator<IndexReference> indexReferences = schemaRead.IndexesGetForLabel( labelId );
									List<string> indexes = new List<string>();
									while ( indexReferences.MoveNext() )
									{
										 IndexReference index = indexReferences.Current;
										 if ( !index.Unique )
										 {
											  string[] propertyNames = PropertyNameUtils.getPropertyKeys( tokenNameLookup, index.Properties() );
											  indexes.Add( string.join( ",", propertyNames ) );
										 }
									}
									properties["indexes"] = indexes;

									IEnumerator<ConstraintDescriptor> nodePropertyConstraintIterator = schemaRead.ConstraintsGetForLabel( labelId );
									List<string> constraints = new List<string>();
									while ( nodePropertyConstraintIterator.MoveNext() )
									{
										 ConstraintDescriptor constraint = nodePropertyConstraintIterator.Current;
										 constraints.Add( constraint.PrettyPrint( tokenNameLookup ) );
									}
									properties["constraints"] = constraints;

									GetOrCreateLabel( label.Name(), properties, nodes );
							  }
						 }

						 //add all relationships

						 using ( ResourceIterator<RelationshipType> relationshipTypeIterator = _graphDatabaseAPI.AllRelationshipTypesInUse.GetEnumerator() )
						 {
							  while ( relationshipTypeIterator.MoveNext() )
							  {
									RelationshipType relationshipType = relationshipTypeIterator.Current;
									string relationshipTypeGetName = relationshipType.Name();
									int relId = tokenRead.RelationshipType( relationshipTypeGetName );
									using ( ResourceIterator<Label> labelsInUse = _graphDatabaseAPI.AllLabelsInUse.GetEnumerator() )
									{
										 IList<VirtualNodeHack> startNodes = new LinkedList<VirtualNodeHack>();
										 IList<VirtualNodeHack> endNodes = new LinkedList<VirtualNodeHack>();

										 while ( labelsInUse.MoveNext() )
										 {
											  Label labelToken = labelsInUse.Current;
											  string labelName = labelToken.Name();
											  IDictionary<string, object> properties = new Dictionary<string, object>();
											  VirtualNodeHack node = GetOrCreateLabel( labelName, properties, nodes );
											  int labelId = tokenRead.NodeLabel( labelName );

											  if ( dataRead.CountsForRelationship( labelId, relId, Org.Neo4j.@internal.Kernel.Api.Read_Fields.ANY_LABEL ) > 0 )
											  {
													startNodes.Add( node );
											  }
											  if ( dataRead.CountsForRelationship( Org.Neo4j.@internal.Kernel.Api.Read_Fields.ANY_LABEL, relId, labelId ) > 0 )
											  {
													endNodes.Add( node );
											  }
										 }

										 foreach ( VirtualNodeHack startNode in startNodes )
										 {
											  foreach ( VirtualNodeHack endNode in endNodes )
											  {
															  AddRelationship( startNode, endNode, relationshipTypeGetName, relationships );
											  }
										 }
									}
							  }
						 }
						 transaction.Success();
						 return GetGraphResult( nodes, relationships );
					}
			  }
		 }

		 public class GraphResult
		 {
			  public readonly IList<Node> Nodes;
			  public readonly IList<Relationship> Relationships;

			  public GraphResult( IList<Node> nodes, IList<Relationship> relationships )
			  {
					this.Nodes = nodes;
					this.Relationships = relationships;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private VirtualNodeHack getOrCreateLabel(String label, java.util.Map<String,Object> properties, final java.util.Map<String,VirtualNodeHack> nodeMap)
		 private VirtualNodeHack GetOrCreateLabel( string label, IDictionary<string, object> properties, IDictionary<string, VirtualNodeHack> nodeMap )
		 {
			  if ( nodeMap.ContainsKey( label ) )
			  {
					return nodeMap[label];
			  }
			  VirtualNodeHack node = new VirtualNodeHack( label, properties );
			  nodeMap[label] = node;
			  return node;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private void addRelationship(VirtualNodeHack startNode, VirtualNodeHack endNode, String relType, final java.util.Map<String,java.util.Set<VirtualRelationshipHack>> relationshipMap)
		 private void AddRelationship( VirtualNodeHack startNode, VirtualNodeHack endNode, string relType, IDictionary<string, ISet<VirtualRelationshipHack>> relationshipMap )
		 {
			  ISet<VirtualRelationshipHack> relationshipsForType;
			  if ( !relationshipMap.ContainsKey( relType ) )
			  {
					relationshipsForType = new HashSet<VirtualRelationshipHack>();
					relationshipMap[relType] = relationshipsForType;
			  }
			  else
			  {
					relationshipsForType = relationshipMap[relType];
			  }
			  VirtualRelationshipHack relationship = new VirtualRelationshipHack( startNode, endNode, relType );
			  if ( !relationshipsForType.Contains( relationship ) )
			  {
					relationshipsForType.Add( relationship );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private GraphResult getGraphResult(final java.util.Map<String,VirtualNodeHack> nodeMap, final java.util.Map<String,java.util.Set<VirtualRelationshipHack>> relationshipMap)
		 private GraphResult GetGraphResult( IDictionary<string, VirtualNodeHack> nodeMap, IDictionary<string, ISet<VirtualRelationshipHack>> relationshipMap )
		 {
			  IList<Relationship> relationships = new LinkedList<Relationship>();
			  foreach ( ISet<VirtualRelationshipHack> relationship in relationshipMap.Values )
			  {
					( ( IList<Relationship> )relationships ).AddRange( relationship );
			  }

			  GraphResult graphResult;
			  graphResult = new GraphResult( new List<Node>( nodeMap.Values ), relationships );

			  return graphResult;
		 }

		 private class VirtualRelationshipHack : Relationship
		 {

			  internal static AtomicLong MinId = new AtomicLong( -1 );

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly long IdConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly Node StartNodeConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly Node EndNodeConflict;
			  internal readonly RelationshipType RelationshipType;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: VirtualRelationshipHack(final VirtualNodeHack startNode, final VirtualNodeHack endNode, final String type)
			  internal VirtualRelationshipHack( VirtualNodeHack startNode, VirtualNodeHack endNode, string type )
			  {
					this.IdConflict = MinId.AndDecrement;
					this.StartNodeConflict = startNode;
					this.EndNodeConflict = endNode;
					RelationshipType = () => type;
			  }

			  public virtual long Id
			  {
				  get
				  {
						return IdConflict;
				  }
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

			  public virtual RelationshipType Type
			  {
				  get
				  {
						return RelationshipType;
				  }
			  }

			  public virtual IDictionary<string, object> AllProperties
			  {
				  get
				  {
						return new Dictionary<string, object>();
				  }
			  }

			  public override void Delete()
			  {

			  }

			  public override Node GetOtherNode( Node node )
			  {
					return null;
			  }

			  public virtual Node[] Nodes
			  {
				  get
				  {
						return new Node[0];
				  }
			  }

			  public override bool IsType( RelationshipType type )
			  {
					return false;
			  }

			  public virtual GraphDatabaseService GraphDatabase
			  {
				  get
				  {
						return null;
				  }
			  }

			  public override bool HasProperty( string key )
			  {
					return false;
			  }

			  public override object GetProperty( string key )
			  {
					return null;
			  }

			  public override object GetProperty( string key, object defaultValue )
			  {
					return null;
			  }

			  public override void SetProperty( string key, object value )
			  {

			  }

			  public override object RemoveProperty( string key )
			  {
					return null;
			  }

			  public virtual IEnumerable<string> PropertyKeys
			  {
				  get
				  {
						return null;
				  }
			  }

			  public override IDictionary<string, object> GetProperties( params string[] keys )
			  {
					return null;
			  }

			  public override string ToString()
			  {
					return string.Format( "VirtualRelationshipHack[{0}]", IdConflict );
			  }
		 }

		 private class VirtualNodeHack : Node
		 {

			  internal readonly Dictionary<string, object> PropertyMap = new Dictionary<string, object>();

			  internal static AtomicLong MinId = new AtomicLong( -1 );
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly long IdConflict;
			  internal readonly Label Label;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: VirtualNodeHack(final String label, java.util.Map<String,Object> properties)
			  internal VirtualNodeHack( string label, IDictionary<string, object> properties )
			  {
					this.IdConflict = MinId.AndDecrement;
					this.Label = Label.label( label );
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
					PropertyMap.putAll( properties );
					PropertyMap["name"] = label;
			  }

			  public virtual long Id
			  {
				  get
				  {
						return IdConflict;
				  }
			  }

			  public virtual IDictionary<string, object> AllProperties
			  {
				  get
				  {
						return PropertyMap;
				  }
			  }

			  public virtual IEnumerable<Label> Labels
			  {
				  get
				  {
						return Collections.singletonList( Label );
				  }
			  }

			  public override void Delete()
			  {

			  }

			  public virtual IEnumerable<Relationship> Relationships
			  {
				  get
				  {
						return null;
				  }
			  }

			  public override bool HasRelationship()
			  {
					return false;
			  }

			  public virtual IEnumerable<Relationship> getRelationships( params RelationshipType[] types )
			  {
					return null;
			  }

			  public virtual IEnumerable<Relationship> getRelationships( Direction direction, params RelationshipType[] types )
			  {
					return null;
			  }

			  public virtual IEnumerable<Relationship> getRelationships( RelationshipType type, Direction direction )
			  {
					return null;
			  }

			  public virtual IEnumerable<Relationship> getRelationships( Direction direction )
			  {
					return null;
			  }

			  public override bool HasRelationship( params RelationshipType[] types )
			  {
					return false;
			  }

			  public override bool HasRelationship( Direction direction, params RelationshipType[] types )
			  {
					return false;
			  }

			  public override bool HasRelationship( RelationshipType type, Direction direction )
			  {
					return false;
			  }

			  public override bool HasRelationship( Direction direction )
			  {
					return false;
			  }

			  public override Relationship GetSingleRelationship( RelationshipType type, Direction dir )
			  {
					return null;
			  }

			  public override Relationship CreateRelationshipTo( Node otherNode, RelationshipType type )
			  {
					return null;
			  }

			  public virtual IEnumerable<RelationshipType> RelationshipTypes
			  {
				  get
				  {
						return null;
				  }
			  }

			  public virtual int Degree
			  {
				  get
				  {
						return 0;
				  }
			  }

			  public virtual int getDegree( RelationshipType type )
			  {
					return 0;
			  }

			  public virtual int getDegree( RelationshipType type, Direction direction )
			  {
					return 0;
			  }

			  public virtual int getDegree( Direction direction )
			  {
					return 0;
			  }

			  public override void AddLabel( Label label )
			  {

			  }

			  public override void RemoveLabel( Label label )
			  {

			  }

			  public override bool HasLabel( Label label )
			  {
					return false;
			  }

			  public virtual GraphDatabaseService GraphDatabase
			  {
				  get
				  {
						return null;
				  }
			  }

			  public override bool HasProperty( string key )
			  {
					return false;
			  }

			  public override object GetProperty( string key )
			  {
					return null;
			  }

			  public override object GetProperty( string key, object defaultValue )
			  {
					return null;
			  }

			  public override void SetProperty( string key, object value )
			  {

			  }

			  public override object RemoveProperty( string key )
			  {
					return null;
			  }

			  public virtual IEnumerable<string> PropertyKeys
			  {
				  get
				  {
						return null;
				  }
			  }

			  public override IDictionary<string, object> GetProperties( params string[] keys )
			  {
					return null;
			  }

			  public override string ToString()
			  {
					return string.Format( "VirtualNodeHack[{0}]", IdConflict );
			  }
		 }
	}

}