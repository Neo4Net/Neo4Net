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
namespace Neo4Net.Server.rest.management.repr
{

	using ListRepresentation = Neo4Net.Server.rest.repr.ListRepresentation;
	using Representation = Neo4Net.Server.rest.repr.Representation;
	using RepresentationDispatcher = Neo4Net.Server.rest.repr.RepresentationDispatcher;
	using ValueRepresentation = Neo4Net.Server.rest.repr.ValueRepresentation;

	/// <summary>
	/// Converts CompositeData, to allow representations of JMX beans.
	/// </summary>
	public class JmxAttributeRepresentationDispatcher : RepresentationDispatcher
	{
		 protected internal override Representation DispatchOtherProperty( object property, string param )
		 {
			  if ( property is CompositeData )
			  {
					return DispatchCompositeData( ( CompositeData ) property );
			  }
			  else
			  {
					return ValueRepresentation.@string( property.ToString() );
			  }
		 }

		 protected internal override Representation DispatchOtherArray( object[] property, string param )
		 {
			  if ( property is CompositeData[] )
			  {
					return DispatchCompositeDataArray( ( CompositeData[] ) property, param );
			  }
			  else
			  {
					return base.DispatchOtherArray( property, param );
			  }
		 }

		 private JmxCompositeDataRepresentation DispatchCompositeData( CompositeData property )
		 {
			  return new JmxCompositeDataRepresentation( property );
		 }

		 private Representation DispatchCompositeDataArray( CompositeData[] property, string param )
		 {
			  List<Representation> values = new List<Representation>();
			  foreach ( CompositeData value in property )
			  {
					values.Add( new JmxCompositeDataRepresentation( value ) );
			  }
			  return new ListRepresentation( "", values );
		 }

	}

}