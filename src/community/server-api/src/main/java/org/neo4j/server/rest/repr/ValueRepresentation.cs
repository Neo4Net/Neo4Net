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

	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using CRS = Neo4Net.GraphDb.Spatial.CRS;
	using Coordinate = Neo4Net.GraphDb.Spatial.Coordinate;
	using Geometry = Neo4Net.GraphDb.Spatial.Geometry;
	using Point = Neo4Net.GraphDb.Spatial.Point;
	using Neo4Net.Helpers.Collections;
	using Neo4Net.Server.helpers;

	public class ValueRepresentation : Representation
	{
		 private readonly object _value;

		 private ValueRepresentation( RepresentationType type, object value ) : base( type )
		 {
			  this._value = value;
		 }

		 internal override string Serialize( RepresentationFormat format, URI baseUri, ExtensionInjector extensions )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String result = format.serializeValue(type, value);
			  string result = format.SerializeValue( Type, _value );
			  format.Complete();
			  return result;
		 }

		 internal override void AddTo( ListSerializer serializer )
		 {
			  serializer.Writer.writeValue( Type, _value );
		 }

		 internal override void PutTo( MappingSerializer serializer, string key )
		 {
			  serializer.Writer.writeValue( Type, key, _value );
		 }

		 public static ValueRepresentation OfNull()
		 {
			  return new ValueRepresentation( RepresentationType.Null, null );
		 }

		 public static ValueRepresentation String( string value )
		 {
			  return new ValueRepresentation( RepresentationType.String, value );
		 }

		 public static ValueRepresentation Point( Point value )
		 {
			  return new ValueRepresentation( RepresentationType.Point, value );
		 }

		 public static ValueRepresentation Temporal( Temporal value )
		 {
			  return new ValueRepresentation( RepresentationType.Temporal, value.ToString() );
		 }

		 public static ValueRepresentation TemporalAmount( TemporalAmount value )
		 {
			  return new ValueRepresentation( RepresentationType.TemporalAmount, value.ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") public static ValueRepresentation number(int value)
		 public static ValueRepresentation Number( int value )
		 {
			  return new ValueRepresentation( RepresentationType.Integer, value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") public static ValueRepresentation number(long value)
		 public static ValueRepresentation Number( long value )
		 {
			  return new ValueRepresentation( RepresentationType.Long, value );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") public static ValueRepresentation number(double value)
		 public static ValueRepresentation Number( double value )
		 {
			  return new ValueRepresentation( RepresentationType.Double, value );
		 }

		 public static ValueRepresentation Bool( bool value )
		 {
			  return new ValueRepresentation( RepresentationType.Boolean, value );
		 }

		 public static ValueRepresentation RelationshipType( RelationshipType type )
		 {
			  return new ValueRepresentation( RepresentationType.RelationshipType, type.Name() );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static ValueRepresentation uri(final String path)
		 public static ValueRepresentation Uri( string path )
		 {
			  return new ValueRepresentationAnonymousInnerClass( RepresentationType.Uri, path );
		 }

		 private class ValueRepresentationAnonymousInnerClass : ValueRepresentation
		 {
			 private string _path;

			 public ValueRepresentationAnonymousInnerClass( Neo4Net.Server.rest.repr.RepresentationType uri, string path ) : base( uri, null )
			 {
				 this._path = path;
			 }

			 internal override string serialize( RepresentationFormat format, URI baseUri, ExtensionInjector extensions )
			 {
				  return Serializer.JoinBaseWithRelativePath( baseUri, _path );
			 }

			 internal override void addTo( ListSerializer serializer )
			 {
				  serializer.AddUri( _path );
			 }

			 internal override void putTo( MappingSerializer serializer, string key )
			 {
				  serializer.PutRelativeUri( key, _path );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static ValueRepresentation template(final String path)
		 public static ValueRepresentation Template( string path )
		 {
			  return new ValueRepresentationAnonymousInnerClass2( RepresentationType.Template, path );
		 }

		 private class ValueRepresentationAnonymousInnerClass2 : ValueRepresentation
		 {
			 private string _path;

			 public ValueRepresentationAnonymousInnerClass2( Neo4Net.Server.rest.repr.RepresentationType template, string path ) : base( template, null )
			 {
				 this._path = path;
			 }

			 internal override string serialize( RepresentationFormat format, URI baseUri, ExtensionInjector extensions )
			 {
				  return Serializer.JoinBaseWithRelativePath( baseUri, _path );
			 }

			 internal override void addTo( ListSerializer serializer )
			 {
				  serializer.AddUriTemplate( _path );
			 }

			 internal override void putTo( MappingSerializer serializer, string key )
			 {
				  serializer.PutRelativeUriTemplate( key, _path );
			 }
		 }

		 internal static Representation Property( object property )
		 {
			  return PROPERTY_REPRESENTATION.dispatch( property, null );
		 }

		 private static readonly PropertyTypeDispatcher<Void, Representation> PROPERTY_REPRESENTATION = new PropertyTypeDispatcherAnonymousInnerClass();

		 private class PropertyTypeDispatcherAnonymousInnerClass : PropertyTypeDispatcher<Void, Representation>
		 {
			 protected internal override Representation dispatchBooleanProperty( bool property, Void param )
			 {
				  return Bool( property );
			 }

			 protected internal override Representation dispatchPointProperty( Point point, Void param )
			 {
				  return new ValueRepresentation( RepresentationType.Point, point );
			 }

			 protected internal override Representation dispatchTemporalProperty( Temporal temporal, Void param )
			 {
				  return new ValueRepresentation( RepresentationType.Temporal, temporal );
			 }

			 protected internal override Representation dispatchTemporalAmountProperty( TemporalAmount temporalAmount, Void param )
			 {
				  return new ValueRepresentation( RepresentationType.TemporalAmount, temporalAmount );
			 }

			 protected internal override Representation dispatchByteProperty( sbyte property, Void param )
			 {
				  return new ValueRepresentation( RepresentationType.Byte, property );
			 }

			 protected internal override Representation dispatchCharacterProperty( char property, Void param )
			 {
				  return new ValueRepresentation( RepresentationType.Char, property );
			 }

			 protected internal override Representation dispatchDoubleProperty( double property, Void param )
			 {
				  return new ValueRepresentation( RepresentationType.Double, property );
			 }

			 protected internal override Representation dispatchFloatProperty( float property, Void param )
			 {
				  return new ValueRepresentation( RepresentationType.Float, property );
			 }

			 protected internal override Representation dispatchIntegerProperty( int property, Void param )
			 {
				  return new ValueRepresentation( RepresentationType.Integer, property );
			 }

			 protected internal override Representation dispatchLongProperty( long property, Void param )
			 {
				  return new ValueRepresentation( RepresentationType.Long, property );
			 }

			 protected internal override Representation dispatchShortProperty( short property, Void param )
			 {
				  return new ValueRepresentation( RepresentationType.Short, property );
			 }

			 protected internal override Representation dispatchStringProperty( string property, Void param )
			 {
				  return String( property );
			 }

			 protected internal override Representation dispatchStringArrayProperty( string[] property, Void param )
			 {
				  return ListRepresentation.Strings( property );
			 }

			 protected internal override Representation dispatchPointArrayProperty( Point[] property, Void param )
			 {
				  return ListRepresentation.Points( property );
			 }

			 protected internal override Representation dispatchTemporalArrayProperty( Temporal[] property, Void param )
			 {
				  return ListRepresentation.Temporals( property );
			 }

			 protected internal override Representation dispatchTemporalAmountArrayProperty( TemporalAmount[] property, Void param )
			 {
				  return ListRepresentation.TemporalAmounts( property );
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private Iterable<Representation> dispatch(PropertyArray<?,?> array)
			 private IEnumerable<Representation> dispatch<T1>( PropertyArray<T1> array )
			 {
				  return new IterableWrapperAnonymousInnerClass( this );
			 }

			 private class IterableWrapperAnonymousInnerClass : IterableWrapper<Representation, object>
			 {
				 private readonly PropertyTypeDispatcherAnonymousInnerClass _outerInstance;

				 public IterableWrapperAnonymousInnerClass( PropertyTypeDispatcherAnonymousInnerClass outerInstance ) : base( ( IEnumerable<object> ) array )
				 {
					 this.outerInstance = outerInstance;
				 }

				 protected internal override Representation underlyingObjectToObject( object @object )
				 {
					  return Property( @object );
				 }
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") protected Representation dispatchByteArrayProperty(PropertyArray<byte[],sbyte> array, Void param)
			 protected internal override Representation dispatchByteArrayProperty( PropertyArray<sbyte[], sbyte> array, Void param )
			 {
				  return toListRepresentation( RepresentationType.Byte, array );
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") protected Representation dispatchShortArrayProperty(PropertyArray<short[],short> array, Void param)
			 protected internal override Representation dispatchShortArrayProperty( PropertyArray<short[], short> array, Void param )
			 {
				  return toListRepresentation( RepresentationType.Short, array );
			 }

			 private ListRepresentation toListRepresentation<T1>( RepresentationType type, PropertyArray<T1> array )
			 {
				  return new ListRepresentation( type, dispatch( array ) );
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") protected Representation dispatchCharacterArrayProperty(PropertyArray<char[],char> array, Void param)
			 protected internal override Representation dispatchCharacterArrayProperty( PropertyArray<char[], char> array, Void param )
			 {
				  return toListRepresentation( RepresentationType.Char, array );
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") protected Representation dispatchIntegerArrayProperty(PropertyArray<int[],int> array, Void param)
			 protected internal override Representation dispatchIntegerArrayProperty( PropertyArray<int[], int> array, Void param )
			 {
				  return toListRepresentation( RepresentationType.Integer, array );
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") protected Representation dispatchLongArrayProperty(PropertyArray<long[],long> array, Void param)
			 protected internal override Representation dispatchLongArrayProperty( PropertyArray<long[], long> array, Void param )
			 {
				  return toListRepresentation( RepresentationType.Long, array );
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") protected Representation dispatchFloatArrayProperty(PropertyArray<float[],float> array, Void param)
			 protected internal override Representation dispatchFloatArrayProperty( PropertyArray<float[], float> array, Void param )
			 {
				  return toListRepresentation( RepresentationType.Float, array );
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") protected Representation dispatchDoubleArrayProperty(PropertyArray<double[],double> array, Void param)
			 protected internal override Representation dispatchDoubleArrayProperty( PropertyArray<double[], double> array, Void param )
			 {
				  return toListRepresentation( RepresentationType.Double, array );
			 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") protected Representation dispatchBooleanArrayProperty(PropertyArray<boolean[],bool> array, Void param)
			 protected internal override Representation dispatchBooleanArrayProperty( PropertyArray<bool[], bool> array, Void param )
			 {
				  return toListRepresentation( RepresentationType.Boolean, array );
			 }
		 }
	}

}