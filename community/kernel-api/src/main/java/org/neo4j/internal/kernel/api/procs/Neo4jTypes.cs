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
namespace Org.Neo4j.@internal.Kernel.Api.procs
{
	/// <summary>
	/// See also type_system.txt in the cypher code base, this is a mapping of that type system definition
	/// down to this level. Ideally this becomes canonical.
	/// 
	/// This should also move to replace the specialized type handling in packstream, or be tied to it in some
	/// way to ensure a strict mapping.
	/// </summary>
	public class Neo4jTypes
	{
		 public static readonly AnyType NTAny = new AnyType();
		 public static readonly TextType NTString = new TextType();
		 public static readonly NumberType NTNumber = new NumberType();
		 public static readonly IntegerType NTInteger = new IntegerType();
		 public static readonly FloatType NTFloat = new FloatType();
		 public static readonly BooleanType NTBoolean = new BooleanType();
		 public static readonly MapType NTMap = new MapType();
		 public static readonly ByteArrayType NTByteArray = new ByteArrayType();
		 public static readonly NodeType NTNode = new NodeType();
		 public static readonly RelationshipType NTRelationship = new RelationshipType();
		 public static readonly PathType NTPath = new PathType();
		 public static readonly GeometryType NTGeometry = new GeometryType();
		 public static readonly PointType NTPoint = new PointType();
		 public static readonly DateTimeType NTDateTime = new DateTimeType();
		 public static readonly LocalDateTimeType NTLocalDateTime = new LocalDateTimeType();
		 public static readonly DateType NTDate = new DateType();
		 public static readonly TimeType NTTime = new TimeType();
		 public static readonly LocalTimeType NTLocalTime = new LocalTimeType();
		 public static readonly DurationType NTDuration = new DurationType();

		 private Neo4jTypes()
		 {
		 }

		 public static ListType NTList( AnyType innerType )
		 {
			  return new ListType( innerType );
		 }

		 public class AnyType
		 {
			  internal readonly string Name;

			  public AnyType() : this("ANY?")
			  {
			  }

			  protected internal AnyType( string name )
			  {
					this.Name = name;
			  }

			  public override string ToString()
			  {
					return Name;
			  }
		 }

		 public class TextType : AnyType
		 {
			  public TextType() : base("STRING?")
			  {
			  }
		 }

		 public class NumberType : AnyType
		 {
			  public NumberType() : base("NUMBER?")
			  {
			  }

			  protected internal NumberType( string name ) : base( name )
			  {
			  }
		 }

		 public class IntegerType : NumberType
		 {
			  public IntegerType() : base("INTEGER?")
			  {
			  }
		 }

		 public class FloatType : NumberType
		 {
			  public FloatType() : base("FLOAT?")
			  {
			  }
		 }

		 public class BooleanType : AnyType
		 {
			  public BooleanType() : base("BOOLEAN?")
			  {
			  }
		 }

		 public class ListType : AnyType
		 {
			  /// <summary>
			  /// The type of values in this collection </summary>
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly AnyType InnerTypeConflict;

			  public ListType( AnyType innerType ) : base( "LIST? OF " + innerType )
			  {
					this.InnerTypeConflict = innerType;
			  }

			  public virtual AnyType InnerType()
			  {
					return InnerTypeConflict;
			  }

			  public override bool Equals( object o )
			  {
					if ( this == o )
					{
						 return true;
					}
					if ( o == null || this.GetType() != o.GetType() )
					{
						 return false;
					}
					ListType listType = ( ListType ) o;
					return InnerTypeConflict.Equals( listType.InnerTypeConflict );
			  }

			  public override int GetHashCode()
			  {
					return InnerTypeConflict.GetHashCode();
			  }
		 }

		 public class MapType : AnyType
		 {
			  public MapType() : base("MAP?")
			  {
			  }

			  protected internal MapType( string name ) : base( name )
			  {
			  }
		 }

		 public class ByteArrayType : AnyType
		 {
			  public ByteArrayType() : base("BYTEARRAY?")
			  {
			  }

			  protected internal ByteArrayType( string name ) : base( name )
			  {
			  }
		 }

		 public class NodeType : MapType
		 {
			  public NodeType() : base("NODE?")
			  {
			  }
		 }

		 public class RelationshipType : MapType
		 {
			  public RelationshipType() : base("RELATIONSHIP?")
			  {
			  }
		 }

		 public class PathType : AnyType
		 {
			  public PathType() : base("PATH?")
			  {
			  }
		 }

		 public class GeometryType : AnyType
		 {
			  public GeometryType() : base("GEOMETRY?")
			  {
			  }
		 }

		 public class PointType : AnyType
		 {
			  public PointType() : base("POINT?")
			  {
			  }
		 }

		 public class DateTimeType : AnyType
		 {
			  public DateTimeType() : base("DATETIME?")
			  {
			  }
		 }

		 public class LocalDateTimeType : AnyType
		 {
			  public LocalDateTimeType() : base("LOCALDATETIME?")
			  {
			  }
		 }

		 public class DateType : AnyType
		 {
			  public DateType() : base("DATE?")
			  {
			  }
		 }

		 public class TimeType : AnyType
		 {
			  public TimeType() : base("TIME?")
			  {
			  }
		 }

		 public class LocalTimeType : AnyType
		 {
			  public LocalTimeType() : base("LOCALTIME?")
			  {
			  }
		 }

		 public class DurationType : AnyType
		 {
			  public DurationType() : base("DURATION?")
			  {
			  }
		 }
	}

}