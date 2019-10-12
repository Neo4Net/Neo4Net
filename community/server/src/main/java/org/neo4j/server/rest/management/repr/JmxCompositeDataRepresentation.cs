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
namespace Org.Neo4j.Server.rest.management.repr
{

	using ListRepresentation = Org.Neo4j.Server.rest.repr.ListRepresentation;
	using ObjectRepresentation = Org.Neo4j.Server.rest.repr.ObjectRepresentation;
	using Representation = Org.Neo4j.Server.rest.repr.Representation;
	using RepresentationDispatcher = Org.Neo4j.Server.rest.repr.RepresentationDispatcher;
	using ValueRepresentation = Org.Neo4j.Server.rest.repr.ValueRepresentation;

	public class JmxCompositeDataRepresentation : ObjectRepresentation
	{
		 protected internal CompositeData Data;
		 private static readonly RepresentationDispatcher _representationDispatcher = new JmxAttributeRepresentationDispatcher();

		 public JmxCompositeDataRepresentation( CompositeData data ) : base( "jmxCompositeData" )
		 {
			  this.Data = data;
		 }

		 [Mapping("type")]
		 public virtual ValueRepresentation Type
		 {
			 get
			 {
				  return ValueRepresentation.@string( Data.CompositeType.TypeName );
			 }
		 }

		 [Mapping("description")]
		 public virtual ValueRepresentation Description
		 {
			 get
			 {
				  return ValueRepresentation.@string( Data.CompositeType.Description );
			 }
		 }

		 [Mapping("value")]
		 public virtual ListRepresentation Value
		 {
			 get
			 {
   
				  List<Representation> values = new List<Representation>();
				  foreach ( object key in Data.CompositeType.Keys )
				  {
						string name = key.ToString();
						string description = Data.CompositeType.getDescription( name );
   
						Representation value = _representationDispatcher.dispatch( Data.get( name ), "" );
   
						values.Add( new NameDescriptionValueRepresentation( name, description, value ) );
				  }
   
				  return new ListRepresentation( "value", values );
			 }
		 }
	}

}