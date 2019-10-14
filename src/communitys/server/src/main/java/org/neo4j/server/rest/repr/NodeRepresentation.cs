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
namespace Neo4Net.Server.rest.repr
{

	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Neo4Net.Helpers.Collections;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using TransactionStateChecker = Neo4Net.Server.rest.transactional.TransactionStateChecker;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;

	public sealed class NodeRepresentation : ObjectRepresentation, ExtensibleRepresentation, EntityRepresentation
	{
		 private readonly Node _node;
		 private TransactionStateChecker _checker;

		 public NodeRepresentation( Node node ) : base( RepresentationType.Node )
		 {
			  this._node = node;
		 }

		 public TransactionStateChecker TransactionStateChecker
		 {
			 set
			 {
				  this._checker = value;
			 }
		 }

		 public string Identity
		 {
			 get
			 {
				  return Convert.ToString( _node.Id );
			 }
		 }

		 [Mapping("self")]
		 public override ValueRepresentation SelfUri()
		 {
			  return ValueRepresentation.Uri( Path( "" ) );
		 }

		 public long Id
		 {
			 get
			 {
				  return _node.Id;
			 }
		 }

		 private string Path( string path )
		 {
			  return "node/" + _node.Id + path;
		 }

		 internal static string Path( Node node )
		 {
			  return "node/" + node.Id;
		 }

		 [Mapping("create_relationship")]
		 public ValueRepresentation RelationshipCreationUri()
		 {
			  return ValueRepresentation.Uri( Path( "/relationships" ) );
		 }

		 [Mapping("all_relationships")]
		 public ValueRepresentation AllRelationshipsUri()
		 {
			  return ValueRepresentation.Uri( Path( "/relationships/all" ) );
		 }

		 [Mapping("incoming_relationships")]
		 public ValueRepresentation IncomingRelationshipsUri()
		 {
			  return ValueRepresentation.Uri( Path( "/relationships/in" ) );
		 }

		 [Mapping("outgoing_relationships")]
		 public ValueRepresentation OutgoingRelationshipsUri()
		 {
			  return ValueRepresentation.Uri( Path( "/relationships/out" ) );
		 }

		 [Mapping("all_typed_relationships")]
		 public ValueRepresentation AllTypedRelationshipsUriTemplate()
		 {
			  return ValueRepresentation.Template( Path( "/relationships/all/{-list|&|types}" ) );
		 }

		 [Mapping("incoming_typed_relationships")]
		 public ValueRepresentation IncomingTypedRelationshipsUriTemplate()
		 {
			  return ValueRepresentation.Template( Path( "/relationships/in/{-list|&|types}" ) );
		 }

		 [Mapping("outgoing_typed_relationships")]
		 public ValueRepresentation OutgoingTypedRelationshipsUriTemplate()
		 {
			  return ValueRepresentation.Template( Path( "/relationships/out/{-list|&|types}" ) );
		 }

		 [Mapping("labels")]
		 public ValueRepresentation LabelsUriTemplate()
		 {
			  return ValueRepresentation.Template( Path( "/labels" ) );
		 }

		 [Mapping("properties")]
		 public ValueRepresentation PropertiesUri()
		 {
			  return ValueRepresentation.Uri( Path( "/properties" ) );
		 }

		 [Mapping("property")]
		 public ValueRepresentation PropertyUriTemplate()
		 {
			  return ValueRepresentation.Template( Path( "/properties/{key}" ) );
		 }

		 [Mapping("traverse")]
		 public ValueRepresentation TraverseUriTemplate()
		 {
			  return ValueRepresentation.Template( Path( "/traverse/{returnType}" ) );
		 }

		 [Mapping("paged_traverse")]
		 public ValueRepresentation PagedTraverseUriTemplate()
		 {
			  return ValueRepresentation.Template( Path( "/paged/traverse/{returnType}{?pageSize,leaseTime}" ) );
		 }

		 [Mapping("metadata")]
		 public MapRepresentation Metadata()
		 {
			  if ( Deleted )
			  {
					return new MapRepresentation( map( "id", _node.Id, "deleted", true ) );
			  }
			  else
			  {
					ICollection<string> labels = Iterables.asCollection( new IterableWrapperAnonymousInnerClass( this, _node.Labels ) );
					return new MapRepresentation( map( "id", _node.Id, "labels", labels ) );
			  }
		 }

		 private class IterableWrapperAnonymousInnerClass : IterableWrapper<string, Label>
		 {
			 private readonly NodeRepresentation _outerInstance;

			 public IterableWrapperAnonymousInnerClass( NodeRepresentation outerInstance, IEnumerable<Label> getLabels ) : base( getLabels )
			 {
				 this.outerInstance = outerInstance;
			 }

			 protected internal override string underlyingObjectToObject( Label label )
			 {
				  return label.Name();
			 }
		 }

		 private bool Deleted
		 {
			 get
			 {
				  return _checker != null && _checker.isNodeDeletedInCurrentTx( _node.Id );
			 }
		 }

		 internal override void ExtraData( MappingSerializer serializer )
		 {
			  if ( !Deleted )
			  {
					MappingWriter writer = serializer.Writer;
					MappingWriter properties = writer.NewMapping( RepresentationType.Properties, "data" );
					( new PropertiesRepresentation( _node ) ).serialize( properties );
					if ( writer.Interactive )
					{
						 serializer.PutList( "relationship_types", ListRepresentation.RelationshipTypes( _node.GraphDatabase.AllRelationshipTypes ) );
					}
					properties.Done();
			  }
		 }

		 public static ListRepresentation List( IEnumerable<Node> nodes )
		 {
			  return new ListRepresentation( RepresentationType.Node, new IterableWrapperAnonymousInnerClass2( nodes ) );
		 }

		 private class IterableWrapperAnonymousInnerClass2 : IterableWrapper<Representation, Node>
		 {
			 public IterableWrapperAnonymousInnerClass2( IEnumerable<Node> nodes ) : base( nodes )
			 {
			 }

			 protected internal override Representation underlyingObjectToObject( Node node )
			 {
				  return new NodeRepresentation( node );
			 }
		 }
	}

}