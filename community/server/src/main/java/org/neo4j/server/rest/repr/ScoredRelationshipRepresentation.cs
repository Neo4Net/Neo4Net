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
namespace Org.Neo4j.Server.rest.repr
{

	public sealed class ScoredRelationshipRepresentation : ScoredEntityRepresentation<RelationshipRepresentation>
	{
		 public ScoredRelationshipRepresentation( RelationshipRepresentation @delegate, float score ) : base( @delegate, score )
		 {
		 }

		 public override string Identity
		 {
			 get
			 {
				  return Delegate.Identity;
			 }
		 }

		 [Mapping("type")]
		 public ValueRepresentation Type
		 {
			 get
			 {
				  return Delegate.Type;
			 }
		 }

		 [Mapping("start")]
		 public ValueRepresentation StartNodeUri()
		 {
			  return Delegate.startNodeUri();
		 }

		 [Mapping("end")]
		 public ValueRepresentation EndNodeUri()
		 {
			  return Delegate.endNodeUri();
		 }

		 [Mapping("properties")]
		 public ValueRepresentation PropertiesUri()
		 {
			  return Delegate.propertiesUri();
		 }

		 [Mapping("property")]
		 public ValueRepresentation PropertyUriTemplate()
		 {
			  return Delegate.propertyUriTemplate();
		 }

		 [Mapping("metadata")]
		 public MapRepresentation Metadata()
		 {
			  return Delegate.metadata();
		 }
	}

}