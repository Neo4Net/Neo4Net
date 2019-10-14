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
namespace Neo4Net.Internal.Kernel.Api
{
	using EqualsBuilder = org.apache.commons.lang3.builder.EqualsBuilder;
	using HashCodeBuilder = org.apache.commons.lang3.builder.HashCodeBuilder;
	using ToStringBuilder = org.apache.commons.lang3.builder.ToStringBuilder;
	using ToStringStyle = org.apache.commons.lang3.builder.ToStringStyle;

	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using NumberValue = Neo4Net.Values.Storable.NumberValue;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using UTF8StringValue = Neo4Net.Values.Storable.UTF8StringValue;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;
	using ValueTuple = Neo4Net.Values.Storable.ValueTuple;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.NO_VALUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.utf8Value;

	public abstract class IndexQuery
	{
		 /// <summary>
		 /// Searches the index for all entries that has the given property.
		 /// </summary>
		 /// <param name="propertyKeyId"> the property ID to match. </param>
		 /// <returns> an <seealso cref="IndexQuery"/> instance to be used for querying an index. </returns>
		 public static ExistsPredicate Exists( int propertyKeyId )
		 {
			  return new ExistsPredicate( propertyKeyId );
		 }

		 /// <summary>
		 /// Searches the index for a certain value.
		 /// </summary>
		 /// <param name="propertyKeyId"> the property ID to match. </param>
		 /// <param name="value"> the property value to search for. </param>
		 /// <returns> an <seealso cref="IndexQuery"/> instance to be used for querying an index. </returns>
		 public static ExactPredicate Exact( int propertyKeyId, object value )
		 {
			  return new ExactPredicate( propertyKeyId, value );
		 }

		 /// <summary>
		 /// Searches the index for numeric values between {@code from} and {@code to}.
		 /// </summary>
		 /// <param name="propertyKeyId"> the property ID to match. </param>
		 /// <param name="from"> lower bound of the range or null if unbounded </param>
		 /// <param name="fromInclusive"> the lower bound is inclusive if true. </param>
		 /// <param name="to"> upper bound of the range or null if unbounded </param>
		 /// <param name="toInclusive"> the upper bound is inclusive if true. </param>
		 /// <returns> an <seealso cref="IndexQuery"/> instance to be used for querying an index. </returns>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public static RangePredicate<?> range(int propertyKeyId, Number from, boolean fromInclusive, Number to, boolean toInclusive)
		 public static RangePredicate<object> Range( int propertyKeyId, Number from, bool fromInclusive, Number to, bool toInclusive )
		 {
			  return new NumberRangePredicate( propertyKeyId, from == null ? null : Values.numberValue( from ), fromInclusive, to == null ? null : Values.numberValue( to ), toInclusive );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public static RangePredicate<?> range(int propertyKeyId, String from, boolean fromInclusive, String to, boolean toInclusive)
		 public static RangePredicate<object> Range( int propertyKeyId, string from, bool fromInclusive, string to, bool toInclusive )
		 {
			  return new TextRangePredicate( propertyKeyId, string.ReferenceEquals( from, null ) ? null : Values.stringValue( from ), fromInclusive, string.ReferenceEquals( to, null ) ? null : Values.stringValue( to ), toInclusive );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public static <VALUE extends org.neo4j.values.storable.Value> RangePredicate<?> range(int propertyKeyId, VALUE from, boolean fromInclusive, VALUE to, boolean toInclusive)
		 public static RangePredicate<object> Range<VALUE>( int propertyKeyId, VALUE from, bool fromInclusive, VALUE to, bool toInclusive ) where VALUE : Neo4Net.Values.Storable.Value
		 {
			  if ( from == null && to == null )
			  {
					throw new System.ArgumentException( "Cannot create RangePredicate without at least one bound" );
			  }

			  ValueGroup valueGroup = from != null ? from.valueGroup() : to.valueGroup();
			  switch ( valueGroup.innerEnumValue )
			  {
			  case ValueGroup.InnerEnum.NUMBER:
					return new NumberRangePredicate( propertyKeyId, ( NumberValue )from, fromInclusive, ( NumberValue )to, toInclusive );

			  case ValueGroup.InnerEnum.TEXT:
					return new TextRangePredicate( propertyKeyId, ( TextValue )from, fromInclusive, ( TextValue )to, toInclusive );

			  case ValueGroup.InnerEnum.GEOMETRY:
					PointValue pFrom = ( PointValue )from;
					PointValue pTo = ( PointValue )to;
					CoordinateReferenceSystem crs = pFrom != null ? pFrom.CoordinateReferenceSystem : pTo.CoordinateReferenceSystem;
					return new GeometryRangePredicate( propertyKeyId, crs, pFrom, fromInclusive, pTo, toInclusive );

			  default:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return new RangePredicate<>(propertyKeyId, valueGroup, from, fromInclusive, to, toInclusive);
					return new RangePredicate<object>( propertyKeyId, valueGroup, from, fromInclusive, to, toInclusive );
			  }
		 }

		 /// <summary>
		 /// Create IndexQuery for retrieving all indexed entries of the given value group.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public static RangePredicate<?> range(int propertyKeyId, org.neo4j.values.storable.ValueGroup valueGroup)
		 public static RangePredicate<object> Range( int propertyKeyId, ValueGroup valueGroup )
		 {
			  if ( valueGroup == ValueGroup.GEOMETRY )
			  {
					throw new System.ArgumentException( "Cannot create GeometryRangePredicate without a specified CRS" );
			  }
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return new RangePredicate<>(propertyKeyId, valueGroup, null, true, null, true);
			  return new RangePredicate<object>( propertyKeyId, valueGroup, null, true, null, true );
		 }

		 /// <summary>
		 /// Create IndexQuery for retrieving all indexed entries with spatial value of the given
		 /// coordinate reference system.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public static RangePredicate<?> range(int propertyKeyId, org.neo4j.values.storable.CoordinateReferenceSystem crs)
		 public static RangePredicate<object> Range( int propertyKeyId, CoordinateReferenceSystem crs )
		 {
			  return new GeometryRangePredicate( propertyKeyId, crs, null, true, null, true );
		 }

		 /// <summary>
		 /// Searches the index string values starting with {@code prefix}.
		 /// </summary>
		 /// <param name="propertyKeyId"> the property ID to match. </param>
		 /// <param name="prefix"> the string prefix to search for. </param>
		 /// <returns> an <seealso cref="IndexQuery"/> instance to be used for querying an index. </returns>
		 public static StringPrefixPredicate StringPrefix( int propertyKeyId, TextValue prefix )
		 {
			  return new StringPrefixPredicate( propertyKeyId, prefix );
		 }

		 /// <summary>
		 /// Searches the index for string values containing the exact search string.
		 /// </summary>
		 /// <param name="propertyKeyId"> the property ID to match. </param>
		 /// <param name="contains"> the string to search for. </param>
		 /// <returns> an <seealso cref="IndexQuery"/> instance to be used for querying an index. </returns>
		 public static StringContainsPredicate StringContains( int propertyKeyId, TextValue contains )
		 {
			  return new StringContainsPredicate( propertyKeyId, contains );
		 }

		 /// <summary>
		 /// Searches the index string values ending with {@code suffix}.
		 /// </summary>
		 /// <param name="propertyKeyId"> the property ID to match. </param>
		 /// <param name="suffix"> the string suffix to search for. </param>
		 /// <returns> an <seealso cref="IndexQuery"/> instance to be used for querying an index. </returns>
		 public static StringSuffixPredicate StringSuffix( int propertyKeyId, TextValue suffix )
		 {
			  return new StringSuffixPredicate( propertyKeyId, suffix );
		 }

		 public static ValueTuple AsValueTuple( params IndexQuery.ExactPredicate[] query )
		 {
			  Value[] values = new Value[query.Length];
			  for ( int i = 0; i < query.Length; i++ )
			  {
					values[i] = query[i].Value();
			  }
			  return ValueTuple.of( values );
		 }

		 private readonly int _propertyKeyId;

		 protected internal IndexQuery( int propertyKeyId )
		 {
			  this._propertyKeyId = propertyKeyId;
		 }

		 public abstract IndexQueryType Type();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("EqualsWhichDoesntCheckParameterClass") @Override public final boolean equals(Object other)
		 public override sealed bool Equals( object other )
		 {
			  // equals() and hashcode() are only used for testing so we don't care that they are a bit slow.
			  return EqualsBuilder.reflectionEquals( this, other );
		 }

		 public override sealed int GetHashCode()
		 {
			  // equals() and hashcode() are only used for testing so we don't care that they are a bit slow.
			  return HashCodeBuilder.reflectionHashCode( this, false );
		 }

		 public override sealed string ToString()
		 {
			  // Only used to debugging, it's okay to be a bit slow.
			  return ToStringBuilder.reflectionToString( this, ToStringStyle.SHORT_PREFIX_STYLE );
		 }

		 public int PropertyKeyId()
		 {
			  return _propertyKeyId;
		 }

		 public abstract bool AcceptsValue( Value value );

		 public virtual bool AcceptsValueAt( PropertyCursor property )
		 {
			  return AcceptsValue( property.PropertyValue() );
		 }

		 /// <returns> Target <seealso cref="ValueGroup"/> for query or <seealso cref="ValueGroup.UNKNOWN"/> if not targeting single group. </returns>
		 public abstract ValueGroup ValueGroup();

		 public enum IndexQueryType
		 {
			  Exists,
			  Exact,
			  Range,
			  StringPrefix,
			  StringSuffix,
			  StringContains
		 }

		 public sealed class ExistsPredicate : IndexQuery
		 {
			  internal ExistsPredicate( int propertyKeyId ) : base( propertyKeyId )
			  {
			  }

			  public override IndexQueryType Type()
			  {
					return IndexQueryType.Exists;
			  }

			  public override bool AcceptsValue( Value value )
			  {
					return value != null && value != NO_VALUE;
			  }

			  public override bool AcceptsValueAt( PropertyCursor property )
			  {
					return true;
			  }

			  public override ValueGroup ValueGroup()
			  {
					return ValueGroup.UNKNOWN;
			  }
		 }

		 public sealed class ExactPredicate : IndexQuery
		 {
			  internal readonly Value ExactValue;

			  internal ExactPredicate( int propertyKeyId, object value ) : base( propertyKeyId )
			  {
					this.ExactValue = value is Value ? ( Value )value : Values.of( value );
			  }

			  public override IndexQueryType Type()
			  {
					return IndexQueryType.Exact;
			  }

			  public override bool AcceptsValue( Value value )
			  {
					return ExactValue.Equals( value );
			  }

			  public override ValueGroup ValueGroup()
			  {
					return ExactValue.valueGroup();
			  }

			  public Value Value()
			  {
					return ExactValue;
			  }
		 }

		 public class RangePredicate<T> : IndexQuery where T : Neo4Net.Values.Storable.Value
		 {
			  protected internal readonly T From;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  protected internal readonly bool FromInclusiveConflict;
			  protected internal readonly T To;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  protected internal readonly bool ToInclusiveConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  protected internal readonly ValueGroup ValueGroupConflict;

			  internal RangePredicate( int propertyKeyId, ValueGroup valueGroup, T from, bool fromInclusive, T to, bool toInclusive ) : base( propertyKeyId )
			  {
					this.ValueGroupConflict = valueGroup;
					this.From = from;
					this.FromInclusiveConflict = fromInclusive;
					this.To = to;
					this.ToInclusiveConflict = toInclusive;
			  }

			  public override IndexQueryType Type()
			  {
					return IndexQueryType.Range;
			  }

			  public override bool AcceptsValue( Value value )
			  {
					if ( value == null || value == NO_VALUE )
					{
						 return false;
					}
					if ( value.ValueGroup() == ValueGroupConflict )
					{
						 if ( From != null )
						 {
							  int compare = Values.COMPARATOR.Compare( value, From );
							  if ( compare < 0 || !FromInclusiveConflict && compare == 0 )
							  {
									return false;
							  }
						 }
						 if ( To != null )
						 {
							  int compare = Values.COMPARATOR.Compare( value, To );
							  return compare <= 0 && ( ToInclusiveConflict || compare != 0 );
						 }
						 return true;
					}
					return false;
			  }

			  public override ValueGroup ValueGroup()
			  {
					return ValueGroupConflict;
			  }

			  public virtual Value FromValue()
			  {
					return From == null ? NO_VALUE : From;
			  }

			  public virtual Value ToValue()
			  {
					return To == null ? NO_VALUE : To;
			  }

			  public virtual bool FromInclusive()
			  {
					return FromInclusiveConflict;
			  }

			  public virtual bool ToInclusive()
			  {
					return ToInclusiveConflict;
			  }

			  /// <returns> true if the order defined for this type can also be relied on for bounds comparisons. </returns>
			  public virtual bool RegularOrder
			  {
				  get
				  {
						return true;
				  }
			  }
		 }

		 public sealed class GeometryRangePredicate : RangePredicate<PointValue>
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly CoordinateReferenceSystem CrsConflict;

			  internal GeometryRangePredicate( int propertyKeyId, CoordinateReferenceSystem crs, PointValue from, bool fromInclusive, PointValue to, bool toInclusive ) : base( propertyKeyId, ValueGroup.GEOMETRY, from, fromInclusive, to, toInclusive )
			  {
					this.CrsConflict = crs;
			  }

			  public override bool AcceptsValue( Value value )
			  {
					if ( value == null )
					{
						 return false;
					}
					if ( value is PointValue )
					{
						 PointValue point = ( PointValue ) value;
						 if ( point.CoordinateReferenceSystem.Equals( CrsConflict ) )
						 {
							  bool? within = point.WithinRange( From, FromInclusiveConflict, To, ToInclusiveConflict );
							  return within == null ? false : within.Value;
						 }
					}
					return false;
			  }

			  public CoordinateReferenceSystem Crs()
			  {
					return CrsConflict;
			  }

			  public PointValue From()
			  {
					return From;
			  }

			  public PointValue To()
			  {
					return To;
			  }

			  /// <summary>
			  /// The order defined for spatial types cannot be used for bounds comparisons. </summary>
			  /// <returns> false </returns>
			  public override bool RegularOrder
			  {
				  get
				  {
						return false;
				  }
			  }
		 }

		 public sealed class NumberRangePredicate : RangePredicate<NumberValue>
		 {
			  internal NumberRangePredicate( int propertyKeyId, NumberValue from, bool fromInclusive, NumberValue to, bool toInclusive ) : base( propertyKeyId, ValueGroup.NUMBER, from, fromInclusive, to, toInclusive )
			  {
			  }

			  public Number From()
			  {
					return From == null ? null : From.asObject();
			  }

			  public Number To()
			  {
					return To == null ? null : To.asObject();
			  }
		 }

		 public sealed class TextRangePredicate : RangePredicate<TextValue>
		 {
			  internal TextRangePredicate( int propertyKeyId, TextValue from, bool fromInclusive, TextValue to, bool toInclusive ) : base( propertyKeyId, ValueGroup.TEXT, from, fromInclusive, to, toInclusive )
			  {
			  }

			  public string From()
			  {
					return From == null ? null : From.stringValue();
			  }

			  public string To()
			  {
					return To == null ? null : To.stringValue();
			  }
		 }

		 public abstract class StringPredicate : IndexQuery
		 {
			  internal StringPredicate( int propertyKeyId ) : base( propertyKeyId )
			  {
			  }

			  public override ValueGroup ValueGroup()
			  {
					return ValueGroup.TEXT;
			  }

			  protected internal virtual TextValue AsUTF8StringValue( TextValue @in )
			  {
					if ( @in is UTF8StringValue )
					{
						 return @in;
					}
					else
					{
						 return utf8Value( @in.StringValue().GetBytes(UTF_8) );
					}
			  }
		 }

		 public sealed class StringPrefixPredicate : StringPredicate
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly TextValue PrefixConflict;

			  internal StringPrefixPredicate( int propertyKeyId, TextValue prefix ) : base( propertyKeyId )
			  {
					//we know utf8 values are coming from the index so optimize for that
					this.PrefixConflict = AsUTF8StringValue( prefix );
			  }

			  public override IndexQueryType Type()
			  {
					return IndexQueryType.StringPrefix;
			  }

			  public override bool AcceptsValue( Value value )
			  {
					return Values.isTextValue( value ) && ( ( TextValue ) value ).startsWith( PrefixConflict );
			  }

			  public TextValue Prefix()
			  {
					return PrefixConflict;
			  }
		 }

		 public sealed class StringContainsPredicate : StringPredicate
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly TextValue ContainsConflict;

			  internal StringContainsPredicate( int propertyKeyId, TextValue contains ) : base( propertyKeyId )
			  {
					//we know utf8 values are coming from the index so optimize for that
					this.ContainsConflict = AsUTF8StringValue( contains );
			  }

			  public override IndexQueryType Type()
			  {
					return IndexQueryType.StringContains;
			  }

			  public override bool AcceptsValue( Value value )
			  {
					return Values.isTextValue( value ) && ( ( TextValue ) value ).contains( ContainsConflict );
			  }

			  public TextValue Contains()
			  {
					return ContainsConflict;
			  }
		 }

		 public sealed class StringSuffixPredicate : StringPredicate
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly TextValue SuffixConflict;

			  internal StringSuffixPredicate( int propertyKeyId, TextValue suffix ) : base( propertyKeyId )
			  {
					//we know utf8 values are coming from the index so optimize for that
					this.SuffixConflict = AsUTF8StringValue( suffix );
			  }

			  public override IndexQueryType Type()
			  {
					return IndexQueryType.StringSuffix;
			  }

			  public override bool AcceptsValue( Value value )
			  {
					return Values.isTextValue( value ) && ( ( TextValue ) value ).endsWith( SuffixConflict );
			  }

			  public TextValue Suffix()
			  {
					return SuffixConflict;
			  }
		 }
	}

}