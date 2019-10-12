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
namespace Neo4Net.Server.rest.repr
{

	public sealed class ScoredNodeRepresentation : ScoredEntityRepresentation<NodeRepresentation>
	{
		 public ScoredNodeRepresentation( NodeRepresentation @delegate, float score ) : base( @delegate, score )
		 {
		 }

		 [Mapping("create_relationship")]
		 public ValueRepresentation RelationshipCreationUri()
		 {
			  return Delegate.relationshipCreationUri();
		 }

		 [Mapping("all_relationships")]
		 public ValueRepresentation AllRelationshipsUri()
		 {
			  return Delegate.allRelationshipsUri();
		 }

		 [Mapping("incoming_relationships")]
		 public ValueRepresentation IncomingRelationshipsUri()
		 {
			  return Delegate.incomingRelationshipsUri();
		 }

		 [Mapping("outgoing_relationships")]
		 public ValueRepresentation OutgoingRelationshipsUri()
		 {
			  return Delegate.outgoingRelationshipsUri();
		 }

		 [Mapping("all_typed_relationships")]
		 public ValueRepresentation AllTypedRelationshipsUriTemplate()
		 {
			  return Delegate.allTypedRelationshipsUriTemplate();
		 }

		 [Mapping("incoming_typed_relationships")]
		 public ValueRepresentation IncomingTypedRelationshipsUriTemplate()
		 {
			  return Delegate.incomingTypedRelationshipsUriTemplate();
		 }

		 [Mapping("outgoing_typed_relationships")]
		 public ValueRepresentation OutgoingTypedRelationshipsUriTemplate()
		 {
			  return Delegate.outgoingTypedRelationshipsUriTemplate();
		 }

		 [Mapping("properties")]
		 public ValueRepresentation PropertiesUri()
		 {
			  return Delegate.propertiesUri();
		 }

		 [Mapping("labels")]
		 public ValueRepresentation LabelsUriTemplate()
		 {
			  return Delegate.labelsUriTemplate();
		 }

		 [Mapping("property")]
		 public ValueRepresentation PropertyUriTemplate()
		 {
			  return Delegate.propertyUriTemplate();
		 }

		 [Mapping("traverse")]
		 public ValueRepresentation TraverseUriTemplate()
		 {
			  return Delegate.traverseUriTemplate();
		 }

		 [Mapping("paged_traverse")]
		 public ValueRepresentation PagedTraverseUriTemplate()
		 {
			  return Delegate.pagedTraverseUriTemplate();
		 }

		 [Mapping("metadata")]
		 public MapRepresentation Metadata()
		 {
			  return Delegate.metadata();
		 }
	}

}