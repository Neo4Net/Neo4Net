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

	using Point = Neo4Net.GraphDb.Spatial.Point;
	using Neo4Net.Server.helpers;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.genericMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.repr.ValueRepresentation.@bool;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.repr.ValueRepresentation.number;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.repr.ValueRepresentation.point;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.repr.ValueRepresentation.@string;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.repr.ValueRepresentation.temporal;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.repr.ValueRepresentation.temporalAmount;

	/// <summary>
	/// Converts common primitive and basic objects and arrays of the same into a
	/// representation. Handy for specialization.
	/// </summary>
	/// <seealso cref= org.Neo4Net.server.rest.management.repr.JmxAttributeRepresentationDispatcher </seealso>
	public abstract class RepresentationDispatcher : PropertyTypeDispatcher<string, Representation>
	{
		 protected internal override Representation DispatchBooleanProperty( bool property, string param )
		 {
			  return @bool( property );
		 }

		 protected internal override Representation DispatchDoubleProperty( double property, string param )
		 {
			  return number( property );
		 }

		 protected internal override Representation DispatchFloatProperty( float property, string param )
		 {
			  return number( property );
		 }

		 protected internal override Representation DispatchIntegerProperty( int property, string param )
		 {
			  return number( property );
		 }

		 protected internal override Representation DispatchLongProperty( long property, string param )
		 {
			  return number( property );
		 }

		 protected internal override Representation DispatchShortProperty( short property, string param )
		 {
			  return number( property );
		 }

		 protected internal override Representation DispatchStringProperty( string property, string param )
		 {
			  return @string( property );
		 }

		 protected internal override Representation DispatchStringArrayProperty( string[] array, string param )
		 {
			  List<Representation> values = new List<Representation>();
			  foreach ( string z in array )
			  {
					values.Add( @string( z ) );
			  }
			  return new ListRepresentation( "", values );
		 }

		 protected internal override Representation DispatchPointArrayProperty( Point[] array, string param )
		 {
			  List<Representation> values = new List<Representation>();
			  foreach ( Point p in array )
			  {
					values.Add( point( p ) );
			  }
			  return new ListRepresentation( "", values );
		 }

		 protected internal override Representation DispatchTemporalArrayProperty( Temporal[] array, string param )
		 {
			  List<Representation> values = new List<Representation>();
			  foreach ( Temporal t in array )
			  {
					values.Add( temporal( t ) );
			  }
			  return new ListRepresentation( "", values );
		 }

		 protected internal override Representation DispatchTemporalAmountArrayProperty( TemporalAmount[] array, string param )
		 {
			  List<Representation> values = new List<Representation>();
			  foreach ( TemporalAmount t in array )
			  {
					values.Add( temporalAmount( t ) );
			  }
			  return new ListRepresentation( "", values );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") protected Representation dispatchShortArrayProperty(PropertyArray<short[], short> array, String param)
		 protected internal override Representation DispatchShortArrayProperty( PropertyArray<short[], short> array, string param )
		 {
			  List<Representation> values = new List<Representation>();
			  foreach ( short? z in array )
			  {
					values.Add( number( z ) );
			  }
			  return new ListRepresentation( "", values );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") protected Representation dispatchIntegerArrayProperty(PropertyArray<int[], int> array, String param)
		 protected internal override Representation DispatchIntegerArrayProperty( PropertyArray<int[], int> array, string param )
		 {
			  List<Representation> values = new List<Representation>();
			  foreach ( int? z in array )
			  {
					values.Add( number( z ) );
			  }
			  return new ListRepresentation( "", values );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") protected Representation dispatchLongArrayProperty(PropertyArray<long[], long> array, String param)
		 protected internal override Representation DispatchLongArrayProperty( PropertyArray<long[], long> array, string param )
		 {
			  List<Representation> values = new List<Representation>();
			  foreach ( long? z in array )
			  {
					values.Add( number( z ) );
			  }
			  return new ListRepresentation( "", values );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") protected Representation dispatchFloatArrayProperty(PropertyArray<float[], float> array, String param)
		 protected internal override Representation DispatchFloatArrayProperty( PropertyArray<float[], float> array, string param )
		 {

			  List<Representation> values = new List<Representation>();
			  foreach ( float? z in array )
			  {
					values.Add( number( z ) );
			  }
			  return new ListRepresentation( "", values );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") protected Representation dispatchDoubleArrayProperty(PropertyArray<double[], double> array, String param)
		 protected internal override Representation DispatchDoubleArrayProperty( PropertyArray<double[], double> array, string param )
		 {
			  List<Representation> values = new List<Representation>();
			  foreach ( double? z in array )
			  {
					values.Add( number( z ) );
			  }
			  return new ListRepresentation( "", values );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") protected Representation dispatchBooleanArrayProperty(PropertyArray<boolean[], bool> array, String param)
		 protected internal override Representation DispatchBooleanArrayProperty( PropertyArray<bool[], bool> array, string param )
		 {
			  List<Representation> values = new List<Representation>();
			  foreach ( bool? z in array )
			  {
					values.Add( @bool( z ) );
			  }
			  return new ListRepresentation( "", values );
		 }

		 protected internal override Representation DispatchByteProperty( sbyte property, string param )
		 {
			  throw new System.NotSupportedException( "Representing bytes not implemented." );
		 }

		 protected internal override Representation DispatchPointProperty( Point property, string param )
		 {
			  return new MapRepresentation( genericMap( new LinkedHashMap<>(), "type", property.GeometryType, "coordinates", property.Coordinate, "crs", property.CRS ) );
		 }

		 protected internal override Representation DispatchTemporalProperty( Temporal property, string param )
		 {
			  return @string( property.ToString() );
		 }

		 protected internal override Representation DispatchTemporalAmountProperty( TemporalAmount property, string param )
		 {
			  return @string( property.ToString() );
		 }

		 protected internal override Representation DispatchCharacterProperty( char property, string param )
		 {
			  return number( property );
		 }
	}

}