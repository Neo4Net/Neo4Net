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
namespace Neo4Net.Kernel.Impl.Index.Schema
{

	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;
	using Neo4Net.Values.Storable;

	/// <summary>
	/// A collection of all instances of <seealso cref="Type"/> and mappings to and from them.
	/// </summary>
	internal class Types
	{
		 // A list of all supported types
		 internal static readonly GeometryType Geometry = new GeometryType( ( sbyte ) 0 );
		 internal static readonly ZonedDateTimeType ZonedDateTime = new ZonedDateTimeType( ( sbyte ) 1 );
		 internal static readonly LocalDateTimeType LocalDateTime = new LocalDateTimeType( ( sbyte ) 2 );
		 internal static readonly DateType Date = new DateType( ( sbyte ) 3 );
		 internal static readonly ZonedTimeType ZonedTime = new ZonedTimeType( ( sbyte ) 4 );
		 internal static readonly LocalTimeType LocalTime = new LocalTimeType( ( sbyte ) 5 );
		 internal static readonly DurationType Duration = new DurationType( ( sbyte ) 6 );
		 internal static readonly TextType Text = new TextType( ( sbyte ) 7 );
		 internal static readonly BooleanType Boolean = new BooleanType( ( sbyte ) 8 );
		 internal static readonly NumberType Number = new NumberType( ( sbyte ) 9 );
		 internal static readonly GeometryArrayType GeometryArray = new GeometryArrayType( ( sbyte ) 10 );
		 internal static readonly ZonedDateTimeArrayType ZonedDateTimeArray = new ZonedDateTimeArrayType( ( sbyte ) 11 );
		 internal static readonly LocalDateTimeArrayType LocalDateTimeArray = new LocalDateTimeArrayType( ( sbyte ) 12 );
		 internal static readonly DateArrayType DateArray = new DateArrayType( ( sbyte ) 13 );
		 internal static readonly ZonedTimeArrayType ZonedTimeArray = new ZonedTimeArrayType( ( sbyte ) 14 );
		 internal static readonly LocalTimeArrayType LocalTimeArray = new LocalTimeArrayType( ( sbyte ) 15 );
		 internal static readonly DurationArrayType DurationArray = new DurationArrayType( ( sbyte ) 16 );
		 internal static readonly TextArrayType TextArray = new TextArrayType( ( sbyte ) 17 );
		 internal static readonly BooleanArrayType BooleanArray = new BooleanArrayType( ( sbyte ) 18 );
		 internal static readonly NumberArrayType NumberArray = new NumberArrayType( ( sbyte ) 19 );

		 /// <summary>
		 /// Holds typeId --> <seealso cref="Type"/> mapping.
		 /// </summary>
		 internal static readonly Type[] ById = InstantiateTypes();

		 /// <summary>
		 /// Holds <seealso cref="ValueGroup.ordinal()"/> --> <seealso cref="Type"/> mapping.
		 /// </summary>
		 internal static readonly Type[] ByGroup = new Type[ValueGroup.values().length];

		 /// <summary>
		 /// Holds <seealso cref="ValueWriter.ArrayType"/> --> <seealso cref="Type"/> mapping.
		 /// </summary>
		 internal static readonly AbstractArrayType[] ByArrayType = new AbstractArrayType[Enum.GetValues( typeof( Neo4Net.Values.Storable.ValueWriter_ArrayType ) ).length];

		 /// <summary>
		 /// Lowest <seealso cref="Type"/> according to <seealso cref="Type.COMPARATOR"/>.
		 /// </summary>
		 internal static readonly Type LowestByValueGroup = Collections.min( Arrays.asList( ById ), Type.Comparator );

		 /// <summary>
		 /// Highest <seealso cref="Type"/> according to <seealso cref="Type.COMPARATOR"/>.
		 /// </summary>
		 internal static readonly Type HighestByValueGroup = Collections.max( Arrays.asList( ById ), Type.Comparator );

		 static Types()
		 {
			  // Build BY_GROUP mapping.
			  foreach ( Type type in ById )
			  {
					ByGroup[type.ValueGroup.ordinal()] = type;
			  }

			  // Build BY_ARRAY_TYPE mapping.
			  foreach ( Neo4Net.Values.Storable.ValueWriter_ArrayType arrayType in Enum.GetValues( typeof( Neo4Net.Values.Storable.ValueWriter_ArrayType ) ) )
			  {
					ByArrayType[( int )arrayType] = TypeOf( arrayType );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private static AbstractArrayType<?> typeOf(org.neo4j.values.storable.ValueWriter_ArrayType arrayType)
		 private static AbstractArrayType<object> TypeOf( Neo4Net.Values.Storable.ValueWriter_ArrayType arrayType )
		 {
			  switch ( arrayType )
			  {
			  case Neo4Net.Values.Storable.ValueWriter_ArrayType.Boolean:
					return BooleanArray;
			  case Neo4Net.Values.Storable.ValueWriter_ArrayType.Byte:
			  case Neo4Net.Values.Storable.ValueWriter_ArrayType.Short:
			  case Neo4Net.Values.Storable.ValueWriter_ArrayType.Int:
			  case Neo4Net.Values.Storable.ValueWriter_ArrayType.Long:
			  case Neo4Net.Values.Storable.ValueWriter_ArrayType.Float:
			  case Neo4Net.Values.Storable.ValueWriter_ArrayType.Double:
					return NumberArray;
			  case Neo4Net.Values.Storable.ValueWriter_ArrayType.String:
			  case Neo4Net.Values.Storable.ValueWriter_ArrayType.Char:
					return TextArray;
			  case Neo4Net.Values.Storable.ValueWriter_ArrayType.LocalDateTime:
					return LocalDateTimeArray;
			  case Neo4Net.Values.Storable.ValueWriter_ArrayType.Date:
					return DateArray;
			  case Neo4Net.Values.Storable.ValueWriter_ArrayType.Duration:
					return DurationArray;
			  case Neo4Net.Values.Storable.ValueWriter_ArrayType.Point:
					return GeometryArray;
			  case Neo4Net.Values.Storable.ValueWriter_ArrayType.LocalTime:
					return LocalTimeArray;
			  case Neo4Net.Values.Storable.ValueWriter_ArrayType.ZonedDateTime:
					return ZonedDateTimeArray;
			  case Neo4Net.Values.Storable.ValueWriter_ArrayType.ZonedTime:
					return ZonedTimeArray;
			  default:
					throw new System.NotSupportedException( arrayType.name() );
			  }
		 }

		 private static Type[] InstantiateTypes()
		 {
			  IList<Type> types = new List<Type>();

			  types.Add( Geometry );
			  types.Add( ZonedDateTime );
			  types.Add( LocalDateTime );
			  types.Add( Date );
			  types.Add( ZonedTime );
			  types.Add( LocalTime );
			  types.Add( Duration );
			  types.Add( Text );
			  types.Add( Boolean );
			  types.Add( Number );

			  types.Add( GeometryArray );
			  types.Add( ZonedDateTimeArray );
			  types.Add( LocalDateTimeArray );
			  types.Add( DateArray );
			  types.Add( ZonedTimeArray );
			  types.Add( LocalTimeArray );
			  types.Add( DurationArray );
			  types.Add( TextArray );
			  types.Add( BooleanArray );
			  types.Add( NumberArray );

			  // Assert order of typeId
			  sbyte expectedTypeId = 0;
			  foreach ( Type type in types )
			  {
					if ( type.TypeId != expectedTypeId )
					{
						 throw new System.InvalidOperationException( "The order in this list is not the intended one" );
					}
					expectedTypeId++;
			  }
			  return types.ToArray();
		 }
	}

}