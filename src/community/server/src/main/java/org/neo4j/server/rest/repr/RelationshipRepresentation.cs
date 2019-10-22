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
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Neo4Net.Helpers.Collections;
	using TransactionStateChecker = Neo4Net.Server.rest.transactional.TransactionStateChecker;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.map;

	public sealed class RelationshipRepresentation : ObjectRepresentation, ExtensibleRepresentation, IEntityRepresentation
	{
		 private readonly Relationship _rel;
		 private TransactionStateChecker _checker;

		 public RelationshipRepresentation( Relationship rel ) : base( RepresentationType.Relationship )
		 {
			  this._rel = rel;
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
				  return Convert.ToString( _rel.Id );
			 }
		 }

		 public long Id
		 {
			 get
			 {
				  return _rel.Id;
			 }
		 }

		 [Mapping("self")]
		 public override ValueRepresentation SelfUri()
		 {
			  return ValueRepresentation.Uri( Path( "" ) );
		 }

		 private string Path( string path )
		 {
			  return "relationship/" + _rel.Id + path;
		 }

		 internal static string Path( Relationship rel )
		 {
			  return "relationship/" + rel.Id;
		 }

		 [Mapping("type")]
		 public ValueRepresentation Type
		 {
			 get
			 {
				  return ValueRepresentation.RelationshipType( _rel.Type );
			 }
		 }

		 [Mapping("start")]
		 public ValueRepresentation StartNodeUri()
		 {
			  return ValueRepresentation.Uri( NodeRepresentation.Path( _rel.StartNode ) );
		 }

		 [Mapping("end")]
		 public ValueRepresentation EndNodeUri()
		 {
			  return ValueRepresentation.Uri( NodeRepresentation.Path( _rel.EndNode ) );
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

		 [Mapping("metadata")]
		 public MapRepresentation Metadata()
		 {
			  if ( Deleted )
			  {
					return new MapRepresentation( map( "id", _rel.Id, "deleted", true ) );
			  }
			  else
			  {
					return new MapRepresentation( map( "id", _rel.Id, "type", _rel.Type.name() ) );
			  }
		 }

		 private bool Deleted
		 {
			 get
			 {
				  return _checker != null && _checker.isRelationshipDeletedInCurrentTx( _rel.Id );
			 }
		 }

		 internal override void ExtraData( MappingSerializer serializer )
		 {
			  if ( !Deleted )
			  {
					MappingWriter properties = serializer.Writer.newMapping( RepresentationType.Properties, "data" );
					( new PropertiesRepresentation( _rel ) ).serialize( properties );
					properties.Done();
			  }
		 }

		 public static ListRepresentation List( IEnumerable<Relationship> relationships )
		 {
			  return new ListRepresentation( RepresentationType.Relationship, new IterableWrapperAnonymousInnerClass( relationships ) );
		 }

		 private class IterableWrapperAnonymousInnerClass : IterableWrapper<Representation, Relationship>
		 {
			 public IterableWrapperAnonymousInnerClass( IEnumerable<Relationship> relationships ) : base( relationships )
			 {
			 }

			 protected internal override Representation underlyingObjectToObject( Relationship relationship )
			 {
				  return new RelationshipRepresentation( relationship );
			 }
		 }
	}

}