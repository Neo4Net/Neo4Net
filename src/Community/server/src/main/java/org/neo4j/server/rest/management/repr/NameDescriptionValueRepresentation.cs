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
namespace Neo4Net.Server.rest.management.repr
{
	using ObjectRepresentation = Neo4Net.Server.rest.repr.ObjectRepresentation;
	using Representation = Neo4Net.Server.rest.repr.Representation;
	using ValueRepresentation = Neo4Net.Server.rest.repr.ValueRepresentation;

	public class NameDescriptionValueRepresentation : ObjectRepresentation
	{
		 private string _name;
		 private string _description;
		 private Representation _value;

		 public NameDescriptionValueRepresentation( string name, string description, Representation value ) : base( "nameDescriptionValue" )
		 {
			  this._name = name;
			  this._description = description;
			  this._value = value;
		 }

		 [Mapping("name")]
		 public virtual ValueRepresentation Name
		 {
			 get
			 {
				  return ValueRepresentation.@string( _name );
			 }
		 }

		 [Mapping("description")]
		 public virtual ValueRepresentation Description
		 {
			 get
			 {
				  return ValueRepresentation.@string( _description );
			 }
		 }

		 [Mapping("value")]
		 public virtual Representation Value
		 {
			 get
			 {
				  return _value;
			 }
		 }

	}

}