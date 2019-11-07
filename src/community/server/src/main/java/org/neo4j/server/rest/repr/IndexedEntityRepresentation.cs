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
	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;

	public sealed class IndexedEntityRepresentation : MappingRepresentation, ExtensibleRepresentation, IEntityRepresentation
	{
		 private readonly MappingRepresentation _entity;
		 private readonly ValueRepresentation _selfUri;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") public IndexedEntityRepresentation(Neo4Net.graphdb.Node node, String key, String value, IndexRepresentation indexRepresentation)
		 public IndexedEntityRepresentation( Node node, string key, string value, IndexRepresentation indexRepresentation ) : this( new NodeRepresentation( node ), node.Id, key, value, indexRepresentation )
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") public IndexedEntityRepresentation(Neo4Net.graphdb.Relationship rel, String key, String value, IndexRepresentation indexRepresentation)
		 public IndexedEntityRepresentation( Relationship rel, string key, string value, IndexRepresentation indexRepresentation ) : this( new RelationshipRepresentation( rel ), rel.Id, key, value, indexRepresentation )
		 {
		 }

		 private IndexedEntityRepresentation( MappingRepresentation IEntity, long IEntityId, string key, string value, IndexRepresentation indexRepresentation ) : base( IEntity.Type )
		 {
			  this._entity = IEntity;
			  _selfUri = ValueRepresentation.Uri( indexRepresentation.RelativeUriFor( key, value, IEntityId ) );
		 }

		 public string Identity
		 {
			 get
			 {
				  return ( ( ExtensibleRepresentation ) _entity ).Identity;
			 }
		 }

		 public override ValueRepresentation SelfUri()
		 {
			  return _selfUri;
		 }

		 protected internal override void Serialize( MappingSerializer serializer )
		 {
			  _entity.serialize( serializer );
			  SelfUri().putTo(serializer, "indexed");
		 }
	}

}