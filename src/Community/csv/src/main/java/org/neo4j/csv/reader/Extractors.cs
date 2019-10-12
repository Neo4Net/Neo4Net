using System;
using System.Collections.Generic;
using System.Reflection;

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
namespace Neo4Net.Csv.Reader
{

	using AnyValue = Neo4Net.Values.AnyValue;
	using CSVHeaderInformation = Neo4Net.Values.Storable.CSVHeaderInformation;
	using DateTimeValue = Neo4Net.Values.Storable.DateTimeValue;
	using DateValue = Neo4Net.Values.Storable.DateValue;
	using DurationValue = Neo4Net.Values.Storable.DurationValue;
	using LocalDateTimeValue = Neo4Net.Values.Storable.LocalDateTimeValue;
	using LocalTimeValue = Neo4Net.Values.Storable.LocalTimeValue;
	using PointValue = Neo4Net.Values.Storable.PointValue;
	using TimeValue = Neo4Net.Values.Storable.TimeValue;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Character.isWhitespace;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.collection.PrimitiveLongCollections.EMPTY_LONG_ARRAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Numbers.safeCastLongToByte;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Numbers.safeCastLongToInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.Numbers.safeCastLongToShort;

	/// <summary>
	/// Common implementations of <seealso cref="Extractor"/>. Since array values can have a delimiter of user choice that isn't
	/// an enum, but a regular class with a constructor where that delimiter can be specified.
	/// 
	/// The common <seealso cref="Extractor extractors"/> can be accessed using the accessor methods, like <seealso cref="string()"/>,
	/// <seealso cref="long_()"/> and others. Specific classes are declared as return types for those providing additional
	/// value accessors, f.ex <seealso cref="LongExtractor.longValue()"/>.
	/// 
	/// Typically an instance of <seealso cref="Extractors"/> would be instantiated along side a <seealso cref="BufferedCharSeeker"/>,
	/// assumed to be used by a single thread, since each <seealso cref="Extractor"/> it has is stateful. Example:
	/// 
	/// <pre>
	/// CharSeeker seeker = ...
	/// Mark mark = new Mark();
	/// Extractors extractors = new Extractors( ';' );
	/// 
	/// // ... seek a value, then extract like this
	/// int boxFreeIntValue = seeker.extract( mark, extractors.int_() ).intValue();
	/// // ... or using any other type of extractor.
	/// </pre>
	/// 
	/// Custom <seealso cref="Extractor extractors"/> can also be implemented and used, if need arises:
	/// 
	/// <pre>
	/// CharSeeker seeker = ...
	/// Mark mark = new Mark();
	/// MyStringDateToLongExtractor dateExtractor = new MyStringDateToLongExtractor();
	/// 
	/// // ... seek a value, then extract like this
	/// long timestamp = seeker.extract( mark, dateExtractor ).dateAsMillis();
	/// </pre>
	/// 
	/// ... even <seealso cref="Extractors.add(Extractor) added"/> to an <seealso cref="Extractors"/> instance, where its
	/// <seealso cref="Extractor.toString() toString"/> value is used as key for lookup in <seealso cref="valueOf(string)"/>.
	/// </summary>
	public class Extractors
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final java.util.Map<String, Extractor<?>> instances = new java.util.HashMap<>();
		 private readonly IDictionary<string, Extractor<object>> _instances = new Dictionary<string, Extractor<object>>();
		 private readonly Extractor<string> @string;
		 private readonly LongExtractor _long_;
		 private readonly IntExtractor _int_;
		 private readonly CharExtractor _char_;
		 private readonly ShortExtractor _short_;
		 private readonly ByteExtractor _byte_;
		 private readonly BooleanExtractor _boolean_;
		 private readonly FloatExtractor _float_;
		 private readonly DoubleExtractor _double_;
		 private readonly Extractor<string[]> _stringArray;
		 private readonly Extractor<bool[]> _booleanArray;
		 private readonly Extractor<sbyte[]> _byteArray;
		 private readonly Extractor<short[]> _shortArray;
		 private readonly Extractor<int[]> _intArray;
		 private readonly Extractor<long[]> _longArray;
		 private readonly Extractor<float[]> _floatArray;
		 private readonly Extractor<double[]> _doubleArray;
		 private readonly PointExtractor _point;
		 private readonly DateExtractor _date;
		 private readonly TimeExtractor _time;
		 private readonly DateTimeExtractor _dateTime;
		 private readonly LocalTimeExtractor _localTime;
		 private readonly LocalDateTimeExtractor _localDateTime;
		 private readonly DurationExtractor _duration;

		 public Extractors( char arrayDelimiter ) : this( arrayDelimiter, Configuration_Fields.Default.emptyQuotedStringsAsNull(), Configuration_Fields.Default.trimStrings(), _inUTC )
		 {
		 }

		 public Extractors( char arrayDelimiter, bool emptyStringsAsNull ) : this( arrayDelimiter, emptyStringsAsNull, Configuration_Fields.Default.trimStrings(), _inUTC )
		 {
		 }

		 public Extractors( char arrayDelimiter, bool emptyStringsAsNull, bool trimStrings ) : this( arrayDelimiter, emptyStringsAsNull, trimStrings, _inUTC )
		 {
		 }

		 /// <summary>
		 /// Why do we have a public constructor here and why isn't this class an enum?
		 /// It's because the array extractors can be configured with an array delimiter,
		 /// something that would be impossible otherwise. There's an equivalent <seealso cref="valueOf(string)"/>
		 /// method to keep the feel of an enum.
		 /// </summary>
		 public Extractors( char arrayDelimiter, bool emptyStringsAsNull, bool trimStrings, System.Func<ZoneId> defaultTimeZone )
		 {
			  try
			  {
					foreach ( System.Reflection.FieldInfo field in this.GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance) )
					{
						 if ( isStatic( field.Modifiers ) )
						 {
							  object value = field.get( null );
							  if ( value is Extractor )
							  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: instances.put(field.getName(), (Extractor<?>) value);
									_instances[field.Name] = ( Extractor<object> ) value;
							  }
						 }
					}

					Add( @string = new StringExtractor( emptyStringsAsNull ) );
					Add( _long_ = new LongExtractor() );
					Add( _int_ = new IntExtractor() );
					Add( _char_ = new CharExtractor() );
					Add( _short_ = new ShortExtractor() );
					Add( _byte_ = new ByteExtractor() );
					Add( _boolean_ = new BooleanExtractor() );
					Add( _float_ = new FloatExtractor() );
					Add( _double_ = new DoubleExtractor() );
					Add( _stringArray = new StringArrayExtractor( arrayDelimiter, trimStrings ) );
					Add( _booleanArray = new BooleanArrayExtractor( arrayDelimiter ) );
					Add( _byteArray = new ByteArrayExtractor( arrayDelimiter ) );
					Add( _shortArray = new ShortArrayExtractor( arrayDelimiter ) );
					Add( _intArray = new IntArrayExtractor( arrayDelimiter ) );
					Add( _longArray = new LongArrayExtractor( arrayDelimiter ) );
					Add( _floatArray = new FloatArrayExtractor( arrayDelimiter ) );
					Add( _doubleArray = new DoubleArrayExtractor( arrayDelimiter ) );
					Add( _point = new PointExtractor() );
					Add( _date = new DateExtractor() );
					Add( _time = new TimeExtractor( defaultTimeZone ) );
					Add( _dateTime = new DateTimeExtractor( defaultTimeZone ) );
					Add( _localTime = new LocalTimeExtractor() );
					Add( _localDateTime = new LocalDateTimeExtractor() );
					Add( _duration = new DurationExtractor() );
			  }
			  catch ( IllegalAccessException )
			  {
					throw new Exception( "Bug in reflection code gathering all extractors" );
			  }
		 }

		 public virtual void Add<T1>( Extractor<T1> extractor )
		 {
			  _instances[extractor.Name().ToUpper()] = extractor;
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public Extractor<?> valueOf(String name)
		 public virtual Extractor<object> ValueOf( string name )
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: Extractor<?> instance = instances.get(name.toUpperCase());
			  Extractor<object> instance = _instances[name.ToUpper()];
			  if ( instance == null )
			  {
					throw new System.ArgumentException( "'" + name + "'" );
			  }
			  return instance;
		 }

		 public virtual Extractor<string> String()
		 {
			  return @string;
		 }

		 public virtual LongExtractor Long_()
		 {
			  return _long_;
		 }

		 public virtual IntExtractor Int_()
		 {
			  return _int_;
		 }

		 public virtual CharExtractor Char_()
		 {
			  return _char_;
		 }

		 public virtual ShortExtractor Short_()
		 {
			  return _short_;
		 }

		 public virtual ByteExtractor Byte_()
		 {
			  return _byte_;
		 }

		 public virtual BooleanExtractor Boolean_()
		 {
			  return _boolean_;
		 }

		 public virtual FloatExtractor Float_()
		 {
			  return _float_;
		 }

		 public virtual DoubleExtractor Double_()
		 {
			  return _double_;
		 }

		 public virtual Extractor<string[]> StringArray()
		 {
			  return _stringArray;
		 }

		 public virtual Extractor<bool[]> BooleanArray()
		 {
			  return _booleanArray;
		 }

		 public virtual Extractor<sbyte[]> ByteArray()
		 {
			  return _byteArray;
		 }

		 public virtual Extractor<short[]> ShortArray()
		 {
			  return _shortArray;
		 }

		 public virtual Extractor<int[]> IntArray()
		 {
			  return _intArray;
		 }

		 public virtual Extractor<long[]> LongArray()
		 {
			  return _longArray;
		 }

		 public virtual Extractor<float[]> FloatArray()
		 {
			  return _floatArray;
		 }

		 public virtual Extractor<double[]> DoubleArray()
		 {
			  return _doubleArray;
		 }

		 public virtual PointExtractor Point()
		 {
			  return _point;
		 }

		 public virtual DateExtractor Date()
		 {
			  return _date;
		 }

		 public virtual TimeExtractor Time()
		 {
			  return _time;
		 }

		 public virtual DateTimeExtractor DateTime()
		 {
			  return _dateTime;
		 }

		 public virtual LocalTimeExtractor LocalTime()
		 {
			  return _localTime;
		 }

		 public virtual LocalDateTimeExtractor LocalDateTime()
		 {
			  return _localDateTime;
		 }

		 public virtual DurationExtractor Duration()
		 {
			  return _duration;
		 }

		 private abstract class AbstractExtractor<T> : Extractor<T>
		 {
			 public abstract T Value();
			 public abstract bool Extract( char[] data, int offset, int length, bool hadQuotes );
			 public abstract bool Extract( char[] data, int offset, int length, bool hadQuotes, CSVHeaderInformation optionalData );
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly string NameConflict;

			  internal AbstractExtractor( string name )
			  {
					this.NameConflict = name;
			  }

			  public override string Name()
			  {
					return NameConflict;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") public Extractor<T> clone()
			  public override Extractor<T> Clone()
			  {
					try
					{
						 return ( Extractor<T> ) base.Clone();
					}
					catch ( CloneNotSupportedException e )
					{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
						 throw new AssertionError( typeof( Extractor ).FullName + " implements " + typeof( ICloneable ).Name + ", at least this implementation assumes that. This doesn't seem to be the case anymore", e );
					}
			  }
		 }

		 private abstract class AbstractSingleValueExtractor<T> : AbstractExtractor<T>
		 {
			  internal AbstractSingleValueExtractor( string toString ) : base( toString )
			  {
			  }

			  public override bool Extract( char[] data, int offset, int length, bool hadQuotes, CSVHeaderInformation optionalData )
			  {
					if ( NullValue( length, hadQuotes ) )
					{
						 Clear();
						 return false;
					}
					return Extract0( data, offset, length, optionalData );
			  }

			  public override bool Extract( char[] data, int offset, int length, bool hadQuotes )
			  {
					return Extract( data, offset, length, hadQuotes, null );
			  }

			  protected internal virtual bool NullValue( int length, bool hadQuotes )
			  {
					return length == 0;
			  }

			  protected internal abstract void Clear();

			  protected internal abstract bool Extract0( char[] data, int offset, int length, CSVHeaderInformation optionalData );
		 }

		 private abstract class AbstractSingleAnyValueExtractor : AbstractSingleValueExtractor<AnyValue>
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  protected internal AnyValue ValueConflict;

			  internal AbstractSingleAnyValueExtractor( string toString ) : base( toString )
			  {
			  }

			  protected internal override void Clear()
			  {
					ValueConflict = Values.NO_VALUE;
			  }

			  public override AnyValue Value()
			  {
					return ValueConflict;
			  }
		 }

		 public class StringExtractor : AbstractSingleValueExtractor<string>
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal string ValueConflict;
			  internal readonly bool EmptyStringsAsNull;

			  public StringExtractor( bool emptyStringsAsNull ) : base( typeof( string ).Name )
			  {
					this.EmptyStringsAsNull = emptyStringsAsNull;
			  }

			  protected internal override void Clear()
			  {
					ValueConflict = null;
			  }

			  protected internal override bool NullValue( int length, bool hadQuotes )
			  {
					return length == 0 && ( !hadQuotes || EmptyStringsAsNull );
			  }

			  protected internal override bool Extract0( char[] data, int offset, int length, CSVHeaderInformation optionalData )
			  {
					ValueConflict = new string( data, offset, length );
					return true;
			  }

			  public override string Value()
			  {
					return ValueConflict;
			  }
		 }

		 public class LongExtractor : AbstractSingleValueExtractor<long>
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long ValueConflict;

			  internal LongExtractor() : base(Long.TYPE.SimpleName)
			  {
			  }

			  protected internal override void Clear()
			  {
					ValueConflict = 0;
			  }

			  protected internal override bool Extract0( char[] data, int offset, int length, CSVHeaderInformation optionalData )
			  {
					ValueConflict = ExtractLong( data, offset, length );
					return true;
			  }

			  public override long? Value()
			  {
					return ValueConflict;
			  }

			  /// <summary>
			  /// Value accessor bypassing boxing. </summary>
			  /// <returns> the number value in its primitive form. </returns>
			  public virtual long LongValue()
			  {
					return ValueConflict;
			  }
		 }

		 public class IntExtractor : AbstractSingleValueExtractor<int>
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal int ValueConflict;

			  internal IntExtractor() : base(Integer.TYPE.ToString())
			  {
			  }

			  protected internal override void Clear()
			  {
					ValueConflict = 0;
			  }

			  protected internal override bool Extract0( char[] data, int offset, int length, CSVHeaderInformation optionalData )
			  {
					ValueConflict = safeCastLongToInt( ExtractLong( data, offset, length ) );
					return true;
			  }

			  public override int? Value()
			  {
					return ValueConflict;
			  }

			  /// <summary>
			  /// Value accessor bypassing boxing. </summary>
			  /// <returns> the number value in its primitive form. </returns>
			  public virtual int IntValue()
			  {
					return ValueConflict;
			  }
		 }

		 public class ShortExtractor : AbstractSingleValueExtractor<short>
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal short ValueConflict;

			  internal ShortExtractor() : base(Short.TYPE.SimpleName)
			  {
			  }

			  protected internal override void Clear()
			  {
					ValueConflict = 0;
			  }

			  protected internal override bool Extract0( char[] data, int offset, int length, CSVHeaderInformation optionalData )
			  {
					ValueConflict = safeCastLongToShort( ExtractLong( data, offset, length ) );
					return true;
			  }

			  public override short? Value()
			  {
					return ValueConflict;
			  }

			  /// <summary>
			  /// Value accessor bypassing boxing. </summary>
			  /// <returns> the number value in its primitive form. </returns>
			  public virtual short ShortValue()
			  {
					return ValueConflict;
			  }
		 }

		 public class ByteExtractor : AbstractSingleValueExtractor<sbyte>
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal sbyte ValueConflict;

			  internal ByteExtractor() : base(Byte.TYPE.SimpleName)
			  {
			  }

			  protected internal override void Clear()
			  {
					ValueConflict = 0;
			  }

			  protected internal override bool Extract0( char[] data, int offset, int length, CSVHeaderInformation optionalData )
			  {
					ValueConflict = safeCastLongToByte( ExtractLong( data, offset, length ) );
					return true;
			  }

			  public override sbyte? Value()
			  {
					return ValueConflict;
			  }

			  /// <summary>
			  /// Value accessor bypassing boxing. </summary>
			  /// <returns> the number value in its primitive form. </returns>
			  public virtual int ByteValue()
			  {
					return ValueConflict;
			  }
		 }

		 private static readonly char[] _booleanMatch;
		 static Extractors()
		 {
			  _booleanMatch = new char[true.ToString().Length];
			  true.ToString().CopyTo(0, _booleanMatch, 0, _booleanMatch.Length - 0);
			  _booleanTrueCharacters = new char[true.ToString().Length];
			  true.ToString().CopyTo(0, _booleanTrueCharacters, 0, _booleanTrueCharacters.Length - 0);
		 }

		 public class BooleanExtractor : AbstractSingleValueExtractor<bool>
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal bool ValueConflict;

			  internal BooleanExtractor() : base(Boolean.TYPE.SimpleName)
			  {
			  }

			  protected internal override void Clear()
			  {
					ValueConflict = false;
			  }

			  protected internal override bool Extract0( char[] data, int offset, int length, CSVHeaderInformation optionalData )
			  {
					ValueConflict = ExtractBoolean( data, offset, length );
					return true;
			  }

			  public override bool? Value()
			  {
					return ValueConflict;
			  }

			  public virtual bool BooleanValue()
			  {
					return ValueConflict;
			  }
		 }

		 public class CharExtractor : AbstractSingleValueExtractor<char>
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal char ValueConflict;

			  internal CharExtractor() : base(Character.TYPE.SimpleName)
			  {
			  }

			  protected internal override void Clear()
			  {
					ValueConflict = ( char )0;
			  }

			  protected internal override bool Extract0( char[] data, int offset, int length, CSVHeaderInformation optionalData )
			  {
					if ( length > 1 )
					{
						 throw new System.InvalidOperationException( "Was told to extract a character, but length:" + length );
					}
					ValueConflict = data[offset];
					return true;
			  }

			  public override char? Value()
			  {
					return ValueConflict;
			  }

			  public virtual char CharValue()
			  {
					return ValueConflict;
			  }
		 }

		 public class FloatExtractor : AbstractSingleValueExtractor<float>
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal float ValueConflict;

			  internal FloatExtractor() : base(Float.TYPE.SimpleName)
			  {
			  }

			  protected internal override void Clear()
			  {
					ValueConflict = 0;
			  }

			  protected internal override bool Extract0( char[] data, int offset, int length, CSVHeaderInformation optionalData )
			  {
					try
					{
						 // TODO Figure out a way to do this conversion without round tripping to String
						 // parseFloat automatically handles leading/trailing whitespace so no need for us to do it
						 ValueConflict = float.Parse( string.valueOf( data, offset, length ) );
					}
					catch ( System.FormatException )
					{
						 throw new System.FormatException( "Not a number: \"" + string.valueOf( data, offset, length ) + "\"" );
					}
					return true;
			  }

			  public override float? Value()
			  {
					return ValueConflict;
			  }

			  public virtual float FloatValue()
			  {
					return ValueConflict;
			  }
		 }

		 public class DoubleExtractor : AbstractSingleValueExtractor<double>
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal double ValueConflict;

			  internal DoubleExtractor() : base(Double.TYPE.SimpleName)
			  {
			  }

			  protected internal override void Clear()
			  {
					ValueConflict = 0;
			  }

			  protected internal override bool Extract0( char[] data, int offset, int length, CSVHeaderInformation optionalData )
			  {
					try
					{
						 // TODO Figure out a way to do this conversion without round tripping to String
						 // parseDouble automatically handles leading/trailing whitespace so no need for us to do it
						 ValueConflict = double.Parse( string.valueOf( data, offset, length ) );
					}
					catch ( System.FormatException )
					{
						 throw new System.FormatException( "Not a number: \"" + string.valueOf( data, offset, length ) + "\"" );
					}
					return true;
			  }

			  public override double? Value()
			  {
					return ValueConflict;
			  }

			  public virtual double DoubleValue()
			  {
					return ValueConflict;
			  }
		 }

		 private abstract class ArrayExtractor<T> : AbstractExtractor<T>
		 {
			  protected internal readonly char ArrayDelimiter;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  protected internal T ValueConflict;

			  internal ArrayExtractor( char arrayDelimiter, Type componentType ) : base( componentType.Name + "[]" )
			  {
					this.ArrayDelimiter = arrayDelimiter;
			  }

			  public override T Value()
			  {
					return ValueConflict;
			  }

			  public override bool Extract( char[] data, int offset, int length, bool hadQuotes, CSVHeaderInformation optionalData )
			  {
					Extract0( data, offset, length, optionalData );
					return true;
			  }

			  public override bool Extract( char[] data, int offset, int length, bool hadQuotes )
			  {
					return Extract( data, offset, length, hadQuotes, null );
			  }

			  protected internal abstract void Extract0( char[] data, int offset, int length, CSVHeaderInformation optionalData );

			  protected internal virtual int CharsToNextDelimiter( char[] data, int offset, int length )
			  {
					for ( int i = 0; i < length; i++ )
					{
						 if ( data[offset + i] == ArrayDelimiter )
						 {
							  return i;
						 }
					}
					return length;
			  }

			  protected internal virtual int NumberOfValues( char[] data, int offset, int length )
			  {
					int count = length > 0 ? 1 : 0;
					for ( int i = 0; i < length; i++ )
					{
						 if ( data[offset + i] == ArrayDelimiter )
						 {
							  count++;
						 }
					}
					return count;
			  }

			  public override int GetHashCode()
			  {
					return this.GetType().GetHashCode();
			  }

			  public override bool Equals( object obj )
			  {
					return obj != null && this.GetType().Equals(obj.GetType());
			  }
		 }

		 private class StringArrayExtractor : ArrayExtractor<string[]>
		 {
			  internal static readonly string[] Empty = new string[0];
			  internal readonly bool TrimStrings;

			  internal StringArrayExtractor( char arrayDelimiter, bool trimStrings ) : base( arrayDelimiter, typeof( string ) )
			  {
					this.TrimStrings = trimStrings;
			  }

			  protected internal override void Extract0( char[] data, int offset, int length, CSVHeaderInformation optionalData )
			  {
					int numberOfValues = numberOfValues( data, offset, length );
					ValueConflict = numberOfValues > 0 ? new string[numberOfValues] : Empty;
					for ( int arrayIndex = 0, charIndex = 0; arrayIndex < numberOfValues; arrayIndex++, charIndex++ )
					{
						 int numberOfChars = CharsToNextDelimiter( data, offset + charIndex, length - charIndex );
						 ValueConflict[arrayIndex] = new string( data, offset + charIndex, numberOfChars );
						 if ( TrimStrings )
						 {
							  ValueConflict[arrayIndex] = ValueConflict[arrayIndex].Trim();
						 }
						 charIndex += numberOfChars;
					}
			  }
		 }

		 private class ByteArrayExtractor : ArrayExtractor<sbyte[]>
		 {
			  internal static readonly sbyte[] Empty = new sbyte[0];

			  internal ByteArrayExtractor( char arrayDelimiter ) : base( arrayDelimiter, Byte.TYPE )
			  {
			  }

			  protected internal override void Extract0( char[] data, int offset, int length, CSVHeaderInformation optionalData )
			  {
					int numberOfValues = numberOfValues( data, offset, length );
					ValueConflict = numberOfValues > 0 ? new sbyte[numberOfValues] : Empty;
					for ( int arrayIndex = 0, charIndex = 0; arrayIndex < numberOfValues; arrayIndex++, charIndex++ )
					{
						 int numberOfChars = CharsToNextDelimiter( data, offset + charIndex, length - charIndex );
						 ValueConflict[arrayIndex] = safeCastLongToByte( ExtractLong( data, offset + charIndex, numberOfChars ) );
						 charIndex += numberOfChars;
					}
			  }
		 }

		 private class ShortArrayExtractor : ArrayExtractor<short[]>
		 {
			  internal static readonly short[] Empty = new short[0];

			  internal ShortArrayExtractor( char arrayDelimiter ) : base( arrayDelimiter, Short.TYPE )
			  {
			  }

			  protected internal override void Extract0( char[] data, int offset, int length, CSVHeaderInformation optionalData )
			  {
					int numberOfValues = numberOfValues( data, offset, length );
					ValueConflict = numberOfValues > 0 ? new short[numberOfValues] : Empty;
					for ( int arrayIndex = 0, charIndex = 0; arrayIndex < numberOfValues; arrayIndex++, charIndex++ )
					{
						 int numberOfChars = CharsToNextDelimiter( data, offset + charIndex, length - charIndex );
						 ValueConflict[arrayIndex] = safeCastLongToShort( ExtractLong( data, offset + charIndex, numberOfChars ) );
						 charIndex += numberOfChars;
					}
			  }
		 }

		 private class IntArrayExtractor : ArrayExtractor<int[]>
		 {
			  internal static readonly int[] Empty = new int[0];

			  internal IntArrayExtractor( char arrayDelimiter ) : base( arrayDelimiter, Integer.TYPE )
			  {
			  }

			  protected internal override void Extract0( char[] data, int offset, int length, CSVHeaderInformation optionalData )
			  {
					int numberOfValues = numberOfValues( data, offset, length );
					ValueConflict = numberOfValues > 0 ? new int[numberOfValues] : Empty;
					for ( int arrayIndex = 0, charIndex = 0; arrayIndex < numberOfValues; arrayIndex++, charIndex++ )
					{
						 int numberOfChars = CharsToNextDelimiter( data, offset + charIndex, length - charIndex );
						 ValueConflict[arrayIndex] = safeCastLongToInt( ExtractLong( data, offset + charIndex, numberOfChars ) );
						 charIndex += numberOfChars;
					}
			  }
		 }

		 private class LongArrayExtractor : ArrayExtractor<long[]>
		 {
			  internal LongArrayExtractor( char arrayDelimiter ) : base( arrayDelimiter, Long.TYPE )
			  {
			  }

			  protected internal override void Extract0( char[] data, int offset, int length, CSVHeaderInformation optionalData )
			  {
					int numberOfValues = numberOfValues( data, offset, length );
					ValueConflict = numberOfValues > 0 ? new long[numberOfValues] : EMPTY_LONG_ARRAY;
					for ( int arrayIndex = 0, charIndex = 0; arrayIndex < numberOfValues; arrayIndex++, charIndex++ )
					{
						 int numberOfChars = CharsToNextDelimiter( data, offset + charIndex, length - charIndex );
						 ValueConflict[arrayIndex] = ExtractLong( data, offset + charIndex, numberOfChars );
						 charIndex += numberOfChars;
					}
			  }
		 }

		 private class FloatArrayExtractor : ArrayExtractor<float[]>
		 {
			  internal static readonly float[] Empty = new float[0];

			  internal FloatArrayExtractor( char arrayDelimiter ) : base( arrayDelimiter, Float.TYPE )
			  {
			  }

			  protected internal override void Extract0( char[] data, int offset, int length, CSVHeaderInformation optionalData )
			  {
					int numberOfValues = numberOfValues( data, offset, length );
					ValueConflict = numberOfValues > 0 ? new float[numberOfValues] : Empty;
					for ( int arrayIndex = 0, charIndex = 0; arrayIndex < numberOfValues; arrayIndex++, charIndex++ )
					{
						 int numberOfChars = CharsToNextDelimiter( data, offset + charIndex, length - charIndex );
						 // TODO Figure out a way to do this conversion without round tripping to String
						 // parseFloat automatically handles leading/trailing whitespace so no need for us to do it
						 ValueConflict[arrayIndex] = float.Parse( string.valueOf( data, offset + charIndex, numberOfChars ) );
						 charIndex += numberOfChars;
					}
			  }
		 }

		 private class DoubleArrayExtractor : ArrayExtractor<double[]>
		 {
			  internal static readonly double[] Empty = new double[0];

			  internal DoubleArrayExtractor( char arrayDelimiter ) : base( arrayDelimiter, Double.TYPE )
			  {
			  }

			  protected internal override void Extract0( char[] data, int offset, int length, CSVHeaderInformation optionalData )
			  {
					int numberOfValues = numberOfValues( data, offset, length );
					ValueConflict = numberOfValues > 0 ? new double[numberOfValues] : Empty;
					for ( int arrayIndex = 0, charIndex = 0; arrayIndex < numberOfValues; arrayIndex++, charIndex++ )
					{
						 int numberOfChars = CharsToNextDelimiter( data, offset + charIndex, length - charIndex );
						 // TODO Figure out a way to do this conversion without round tripping to String
						 // parseDouble automatically handles leading/trailing whitespace so no need for us to do it
						 ValueConflict[arrayIndex] = double.Parse( string.valueOf( data, offset + charIndex, numberOfChars ) );
						 charIndex += numberOfChars;
					}
			  }
		 }

		 private class BooleanArrayExtractor : ArrayExtractor<bool[]>
		 {
			  internal static readonly bool[] Empty = new bool[0];

			  internal BooleanArrayExtractor( char arrayDelimiter ) : base( arrayDelimiter, Boolean.TYPE )
			  {
			  }

			  protected internal override void Extract0( char[] data, int offset, int length, CSVHeaderInformation optionalData )
			  {
					int numberOfValues = numberOfValues( data, offset, length );
					ValueConflict = numberOfValues > 0 ? new bool[numberOfValues] : Empty;
					for ( int arrayIndex = 0, charIndex = 0; arrayIndex < numberOfValues; arrayIndex++, charIndex++ )
					{
						 int numberOfChars = CharsToNextDelimiter( data, offset + charIndex, length - charIndex );
						 ValueConflict[arrayIndex] = ExtractBoolean( data, offset + charIndex, numberOfChars );
						 charIndex += numberOfChars;
					}
			  }
		 }

		 public class PointExtractor : AbstractSingleAnyValueExtractor
		 {
			  internal PointExtractor() : base(NAME)
			  {
			  }

			  protected internal override bool Extract0( char[] data, int offset, int length, CSVHeaderInformation optionalData )
			  {
					ValueConflict = PointValue.parse( CharBuffer.wrap( data, offset, length ), optionalData );
					return true;
			  }

			  public override AnyValue Value()
			  {
					return ValueConflict;
			  }

			  public const string NAME = "Point";
		 }

		 public class DateExtractor : AbstractSingleAnyValueExtractor
		 {
			  internal DateExtractor() : base(NAME)
			  {
			  }

			  protected internal override bool Extract0( char[] data, int offset, int length, CSVHeaderInformation optionalData )
			  {
					ValueConflict = DateValue.parse( CharBuffer.wrap( data, offset, length ) );
					return true;
			  }

			  public override AnyValue Value()
			  {
					return ValueConflict;
			  }

			  public const string NAME = "Date";
		 }

		 public class TimeExtractor : AbstractSingleAnyValueExtractor
		 {
			  internal System.Func<ZoneId> DefaultTimeZone;

			  internal TimeExtractor( System.Func<ZoneId> defaultTimeZone ) : base( NAME )
			  {
					this.DefaultTimeZone = defaultTimeZone;
			  }

			  protected internal override bool Extract0( char[] data, int offset, int length, CSVHeaderInformation optionalData )
			  {
					ValueConflict = TimeValue.parse( CharBuffer.wrap( data, offset, length ), DefaultTimeZone, optionalData );
					return true;
			  }

			  public override AnyValue Value()
			  {
					return ValueConflict;
			  }

			  public const string NAME = "Time";
		 }

		 public class DateTimeExtractor : AbstractSingleAnyValueExtractor
		 {
			  internal System.Func<ZoneId> DefaultTimeZone;

			  internal DateTimeExtractor( System.Func<ZoneId> defaultTimeZone ) : base( NAME )
			  {
					this.DefaultTimeZone = defaultTimeZone;
			  }

			  protected internal override bool Extract0( char[] data, int offset, int length, CSVHeaderInformation optionalData )
			  {
					ValueConflict = DateTimeValue.parse( CharBuffer.wrap( data, offset, length ), DefaultTimeZone, optionalData );
					return true;
			  }

			  public override AnyValue Value()
			  {
					return ValueConflict;
			  }

			  public const string NAME = "DateTime";
		 }

		 public class LocalTimeExtractor : AbstractSingleAnyValueExtractor
		 {
			  internal LocalTimeExtractor() : base(NAME)
			  {
			  }

			  protected internal override bool Extract0( char[] data, int offset, int length, CSVHeaderInformation optionalData )
			  {
					ValueConflict = LocalTimeValue.parse( CharBuffer.wrap( data, offset, length ) );
					return true;
			  }

			  public override AnyValue Value()
			  {
					return ValueConflict;
			  }

			  public const string NAME = "LocalTime";
		 }

		 public class LocalDateTimeExtractor : AbstractSingleAnyValueExtractor
		 {
			  internal LocalDateTimeExtractor() : base(NAME)
			  {
			  }

			  protected internal override bool Extract0( char[] data, int offset, int length, CSVHeaderInformation optionalData )
			  {
					ValueConflict = LocalDateTimeValue.parse( CharBuffer.wrap( data, offset, length ) );
					return true;
			  }

			  public override AnyValue Value()
			  {
					return ValueConflict;
			  }

			  public const string NAME = "LocalDateTime";
		 }

		 public class DurationExtractor : AbstractSingleAnyValueExtractor
		 {
			  internal DurationExtractor() : base(NAME)
			  {
			  }

			  protected internal override bool Extract0( char[] data, int offset, int length, CSVHeaderInformation optionalData )
			  {
					ValueConflict = DurationValue.parse( CharBuffer.wrap( data, offset, length ) );
					return true;
			  }

			  public override AnyValue Value()
			  {
					return ValueConflict;
			  }

			  public const string NAME = "Duration";
		 }

		 private static readonly System.Func<ZoneId> _inUTC = () => UTC;

		 private static long ExtractLong( char[] data, int originalOffset, int fullLength )
		 {
			  long result = 0;
			  bool negate = false;
			  int offset = originalOffset;
			  int length = fullLength;

			  // Leading whitespace can be ignored
			  while ( length > 0 && isWhitespace( data[offset] ) )
			  {
					offset++;
					length--;
			  }
			  // Trailing whitespace can be ignored
			  while ( length > 0 && isWhitespace( data[offset + length - 1] ) )
			  {
					length--;
			  }

			  if ( length > 0 && data[offset] == '-' )
			  {
					negate = true;
					offset++;
					length--;
			  }

			  if ( length < 1 )
			  {
					throw new System.FormatException( "Not an integer: \"" + string.valueOf( data, originalOffset, fullLength ) + "\"" );
			  }

			  try
			  {
					for ( int i = 0; i < length; i++ )
					{
						 result = result * 10 + Digit( data[offset + i] );
					}
			  }
			  catch ( System.FormatException )
			  {
					throw new System.FormatException( "Not an integer: \"" + string.valueOf( data, originalOffset, fullLength ) + "\"" );
			  }

			  return negate ? -result : result;
		 }

		 private static int Digit( char ch )
		 {
			  int digit = ch - '0';
			  if ( ( digit < 0 ) || ( digit > 9 ) )
			  {
					throw new System.FormatException();
			  }
			  return digit;
		 }

		 private static readonly char[] _booleanTrueCharacters;

		 private static bool ExtractBoolean( char[] data, int originalOffset, int fullLength )
		 {
			  int offset = originalOffset;
			  int length = fullLength;
			  // Leading whitespace can be ignored
			  while ( length > 0 && isWhitespace( data[offset] ) )
			  {
					offset++;
					length--;
			  }
			  // Trailing whitespace can be ignored
			  while ( length > 0 && isWhitespace( data[offset + length - 1] ) )
			  {
					length--;
			  }

			  // See if the rest exactly match "true"
			  if ( length != _booleanTrueCharacters.Length )
			  {
					return false;
			  }

			  for ( int i = 0; i < _booleanTrueCharacters.Length && i < length; i++ )
			  {
					if ( data[offset + i] != _booleanTrueCharacters[i] )
					{
						 return false;
					}
			  }

			  return true;
		 }
	}

}