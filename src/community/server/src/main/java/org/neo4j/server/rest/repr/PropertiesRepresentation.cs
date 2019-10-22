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

	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using CRS = Neo4Net.GraphDb.spatial.CRS;
	using Point = Neo4Net.GraphDb.spatial.Point;
	using Neo4Net.Server.helpers;

	public sealed class PropertiesRepresentation : MappingRepresentation
	{
		 private readonly IPropertyContainer _entity;

		 public PropertiesRepresentation( IPropertyContainer IEntity ) : base( RepresentationType.Properties )
		 {
			  this._entity = IEntity;
		 }

		 public override bool Empty
		 {
			 get
			 {
				  return !_entity.PropertyKeys.GetEnumerator().hasNext();
			 }
		 }

		 protected internal override void Serialize( MappingSerializer serializer )
		 {
			  Serialize( serializer.Writer );
		 }

		 internal void Serialize( MappingWriter writer )
		 {
			  PropertyTypeDispatcher.consumeProperties( new Consumer( writer ), _entity );
		 }

		 private class Consumer : PropertyTypeDispatcher<string, Void>
		 {
			  internal readonly MappingWriter Writer;

			  internal Consumer( MappingWriter serializer )
			  {
					this.Writer = serializer;
			  }

			  protected internal override Void DispatchBooleanProperty( bool property, string param )
			  {
					Writer.writeBoolean( param, property );
					return null;
			  }

			  protected internal override Void DispatchPointProperty( Point property, string param )
			  {
					MappingWriter pointWriter = Writer.newMapping( RepresentationType.Point, param );
					WritePoint( pointWriter, property );
					pointWriter.Done();
					return null;
			  }

			  protected internal override Void DispatchTemporalProperty( Temporal property, string param )
			  {
					Writer.writeString( param, property.ToString() );
					return null;
			  }

			  protected internal override Void DispatchTemporalAmountProperty( TemporalAmount property, string param )
			  {
					Writer.writeString( param, property.ToString() );
					return null;
			  }

			  protected internal override Void DispatchByteProperty( sbyte property, string param )
			  {
					Writer.writeInteger( RepresentationType.Byte, param, property );
					return null;
			  }

			  protected internal override Void DispatchCharacterProperty( char property, string param )
			  {
					Writer.writeInteger( RepresentationType.Char, param, property );
					return null;
			  }

			  protected internal override Void DispatchDoubleProperty( double property, string param )
			  {
					Writer.writeFloatingPointNumber( RepresentationType.Double, param, property );
					return null;
			  }

			  protected internal override Void DispatchFloatProperty( float property, string param )
			  {
					Writer.writeFloatingPointNumber( RepresentationType.Float, param, property );
					return null;
			  }

			  protected internal override Void DispatchIntegerProperty( int property, string param )
			  {
					Writer.writeInteger( RepresentationType.Integer, param, property );
					return null;
			  }

			  protected internal override Void DispatchLongProperty( long property, string param )
			  {
					Writer.writeInteger( RepresentationType.Long, param, property );
					return null;
			  }

			  protected internal override Void DispatchShortProperty( short property, string param )
			  {
					Writer.writeInteger( RepresentationType.Short, param, property );
					return null;
			  }

			  protected internal override Void DispatchStringProperty( string property, string param )
			  {
					Writer.writeString( param, property );
					return null;
			  }

			  protected internal override Void DispatchStringArrayProperty( string[] property, string param )
			  {
					ListWriter list = Writer.newList( RepresentationType.String, param );
					foreach ( string s in property )
					{
						 list.WriteString( s );
					}
					list.Done();
					return null;
			  }

			  protected internal override Void DispatchPointArrayProperty( Point[] property, string param )
			  {
					ListWriter list = Writer.newList( RepresentationType.Point, param );
					foreach ( Point p in property )
					{
						 MappingWriter pointWriter = list.NewMapping( RepresentationType.Point );
						 WritePoint( pointWriter, p );
						 pointWriter.Done();
					}
					list.Done();
					return null;
			  }

			  protected internal override Void DispatchTemporalArrayProperty( Temporal[] property, string param )
			  {
					ListWriter list = Writer.newList( RepresentationType.Temporal, param );
					foreach ( Temporal p in property )
					{
						 list.WriteString( p.ToString() );
					}
					list.Done();
					return null;
			  }

			  protected internal override Void DispatchTemporalAmountArrayProperty( TemporalAmount[] property, string param )
			  {
					ListWriter list = Writer.newList( RepresentationType.TemporalAmount, param );
					foreach ( TemporalAmount p in property )
					{
						 list.WriteString( p.ToString() );
					}
					list.Done();
					return null;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") protected Void dispatchByteArrayProperty(PropertyArray<byte[], sbyte> array, String param)
			  protected internal override Void DispatchByteArrayProperty( PropertyArray<sbyte[], sbyte> array, string param )
			  {
					ListWriter list = Writer.newList( RepresentationType.Byte, param );
					foreach ( sbyte? b in array )
					{
						 list.writeInteger( RepresentationType.Byte, b );
					}
					list.Done();
					return null;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") protected Void dispatchShortArrayProperty(PropertyArray<short[], short> array, String param)
			  protected internal override Void DispatchShortArrayProperty( PropertyArray<short[], short> array, string param )
			  {
					ListWriter list = Writer.newList( RepresentationType.Short, param );
					foreach ( short? s in array )
					{
						 list.writeInteger( RepresentationType.Short, s );
					}
					list.Done();
					return null;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") protected Void dispatchCharacterArrayProperty(PropertyArray<char[], char> array, String param)
			  protected internal override Void DispatchCharacterArrayProperty( PropertyArray<char[], char> array, string param )
			  {
					ListWriter list = Writer.newList( RepresentationType.Char, param );
					foreach ( char? c in array )
					{
						 list.writeInteger( RepresentationType.Char, c );
					}
					list.Done();
					return null;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") protected Void dispatchIntegerArrayProperty(PropertyArray<int[], int> array, String param)
			  protected internal override Void DispatchIntegerArrayProperty( PropertyArray<int[], int> array, string param )
			  {
					ListWriter list = Writer.newList( RepresentationType.Integer, param );
					foreach ( int? i in array )
					{
						 list.writeInteger( RepresentationType.Integer, i );
					}
					list.Done();
					return null;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") protected Void dispatchLongArrayProperty(PropertyArray<long[], long> array, String param)
			  protected internal override Void DispatchLongArrayProperty( PropertyArray<long[], long> array, string param )
			  {
					ListWriter list = Writer.newList( RepresentationType.Long, param );
					foreach ( long? j in array )
					{
						 list.writeInteger( RepresentationType.Long, j );
					}
					list.Done();
					return null;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") protected Void dispatchFloatArrayProperty(PropertyArray<float[], float> array, String param)
			  protected internal override Void DispatchFloatArrayProperty( PropertyArray<float[], float> array, string param )
			  {
					ListWriter list = Writer.newList( RepresentationType.Float, param );
					foreach ( float? f in array )
					{
						 list.writeFloatingPointNumber( RepresentationType.Float, f );
					}
					list.Done();
					return null;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") protected Void dispatchDoubleArrayProperty(PropertyArray<double[], double> array, String param)
			  protected internal override Void DispatchDoubleArrayProperty( PropertyArray<double[], double> array, string param )
			  {
					ListWriter list = Writer.newList( RepresentationType.Double, param );
					foreach ( double? d in array )
					{
						 list.writeFloatingPointNumber( RepresentationType.Double, d );
					}
					list.Done();
					return null;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") protected Void dispatchBooleanArrayProperty(PropertyArray<boolean[], bool> array, String param)
			  protected internal override Void DispatchBooleanArrayProperty( PropertyArray<bool[], bool> array, string param )
			  {
					ListWriter list = Writer.newList( RepresentationType.Boolean, param );
					foreach ( bool? z in array )
					{
						 list.WriteBoolean( z.Value );
					}
					list.Done();
					return null;
			  }

			  internal virtual void WritePoint( MappingWriter pointWriter, Point property )
			  {
					pointWriter.WriteString( "type", property.GeometryType );
					//write coordinates
					ListWriter coordinatesWriter = pointWriter.NewList( RepresentationType.Double, "coordinates" );
					foreach ( double? coordinate in property.Coordinate.Coordinate )
					{
						 coordinatesWriter.writeFloatingPointNumber( RepresentationType.Double, coordinate );
					}
					coordinatesWriter.Done();

					//Write coordinate reference system
					CRS crs = property.CRS;
					MappingWriter crsWriter = pointWriter.NewMapping( RepresentationType.Map, "crs" );
					crsWriter.WriteInteger( RepresentationType.Integer, "srid", crs.Code );
					crsWriter.WriteString( "name", crs.Type );
					crsWriter.WriteString( "type", "link" );
					MappingWriter propertiesWriter = crsWriter.NewMapping( Representation.Map, "properties" );
					propertiesWriter.WriteString( "href", crs.Href + "ogcwkt/" );
					propertiesWriter.WriteString( "type","ogcwkt" );
					propertiesWriter.Done();
					crsWriter.Done();
			  }
		 }

		 public static Representation Value( object property )
		 {
			  return ValueRepresentation.Property( property );
		 }
	}

}