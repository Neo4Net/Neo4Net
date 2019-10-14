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
namespace Neo4Net.Server.rest.management.repr
{

	using ListRepresentation = Neo4Net.Server.rest.repr.ListRepresentation;
	using ObjectRepresentation = Neo4Net.Server.rest.repr.ObjectRepresentation;
	using Representation = Neo4Net.Server.rest.repr.Representation;
	using RepresentationDispatcher = Neo4Net.Server.rest.repr.RepresentationDispatcher;
	using ValueRepresentation = Neo4Net.Server.rest.repr.ValueRepresentation;

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