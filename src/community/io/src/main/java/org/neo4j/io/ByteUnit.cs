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
namespace Neo4Net.Io
{

	using Neo4Net.Helpers.Collections;

	/// <summary>
	/// A ByteUnit is a unit for a quantity of bytes.
	/// <para>
	/// The unit knows how to convert between other units in its class, so you for instance can turn a number of KiBs into
	/// an accurate quantity of bytes. Precision can be lost when converting smaller units into larger units, because of
	/// integer division.
	/// </para>
	/// <para>
	/// These units all follow the EIC (International Electrotechnical Commission) standard, which uses a multiplier of
	/// 1.024. This system is also known as the binary system, and has been accepted as part of the International System of
	/// Quantities. It is therefor the recommended choice when communicating quantities of information, and the only one
	/// available in this implementation.
	/// </para>
	/// </summary>
	public sealed class ByteUnit
	{
		 /*
		 XXX Future notes: This class can potentially replace some of the functionality in org.Neo4Net.helpers.Format.
		  */

		 public static readonly ByteUnit Byte = new ByteUnit( "Byte", InnerEnum.Byte, 0, "B" );
		 public static readonly ByteUnit KibiByte = new ByteUnit( "KibiByte", InnerEnum.KibiByte, 1, "KiB", "KB", "K", "kB", "kb", "k" );
		 public static readonly ByteUnit MebiByte = new ByteUnit( "MebiByte", InnerEnum.MebiByte, 2, "MiB", "MB", "M", "mB", "mb", "m" );
		 public static readonly ByteUnit GibiByte = new ByteUnit( "GibiByte", InnerEnum.GibiByte, 3, "GiB", "GB", "G", "gB", "gb", "g" );
		 public static readonly ByteUnit TebiByte = new ByteUnit( "TebiByte", InnerEnum.TebiByte, 4, "TiB", "TB" );
		 public static readonly ByteUnit PebiByte = new ByteUnit( "PebiByte", InnerEnum.PebiByte, 5, "PiB", "PB" );
		 public static readonly ByteUnit ExbiByte = new ByteUnit( "ExbiByte", InnerEnum.ExbiByte, 6, "EiB", "EB" );

		 private static readonly IList<ByteUnit> valueList = new List<ByteUnit>();

		 static ByteUnit()
		 {
			 valueList.Add( Byte );
			 valueList.Add( KibiByte );
			 valueList.Add( MebiByte );
			 valueList.Add( GibiByte );
			 valueList.Add( TebiByte );
			 valueList.Add( PebiByte );
			 valueList.Add( ExbiByte );
		 }

		 public enum InnerEnum
		 {
			 Byte,
			 KibiByte,
			 MebiByte,
			 GibiByte,
			 TebiByte,
			 PebiByte,
			 ExbiByte
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Public const;
		 internal Public const;
		 internal Public const;

		 internal Private const;

		 internal Private readonly;
		 internal Private readonly;
		 internal Private readonly;

		 internal ByteUnit( string name, InnerEnum innerEnum, long power, params string[] names )
		 {
			  this._factor = FactorFromPower( power );
			  this._shortName = names[0];
			  this._names = names;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 /// <summary>
		 /// Compute the increment factor from the given power.
		 /// <para>
		 /// Giving zero always produces 1. Giving 1 will produce 1000 or 1024, for SI and EIC respectively, and so on.
		 /// </para>
		 /// </summary>
		 private long FactorFromPower( long power )
		 {
			  if ( power == 0 )
			  {
					return 1;
			  }
			  long product = EIC_MULTIPLIER;
			  for ( int i = 0; i < power - 1; i++ )
			  {
					product = product * EIC_MULTIPLIER;
			  }
			  return product;
		 }

		 /// <summary>
		 /// Get the short or abbreviated name of this unit, e.g. KiB or MiB.
		 /// </summary>
		 /// <returns> The short unit name. </returns>
		 public string Abbreviation()
		 {
			  return _shortName;
		 }

		 /// <summary>
		 /// Convert the given value of this unit, to a value in the given unit.
		 /// </summary>
		 /// <param name="value"> The value to convert from this unit. </param>
		 /// <param name="toUnit"> The unit of the resulting value. </param>
		 /// <returns> The value in the given result unit. </returns>
		 public long Convert( long value, ByteUnit toUnit )
		 {
			  return ToBytes( value ) / toUnit._factor;
		 }

		 public long ToBytes( long value )
		 {
			  return _factor * value;
		 }

		 public long ToKibiBytes( long value )
		 {
			  return Convert( value, KibiByte );
		 }

		 public long ToMebiBytes( long value )
		 {
			  return Convert( value, MebiByte );
		 }

		 public long ToGibiBytes( long value )
		 {
			  return Convert( value, GibiByte );
		 }

		 public long ToTebiBytes( long value )
		 {
			  return Convert( value, TebiByte );
		 }

		 public long ToPebiBytes( long value )
		 {
			  return Convert( value, PebiByte );
		 }

		 public long ToExbiBytes( long value )
		 {
			  return Convert( value, ExbiByte );
		 }

		 public static long Bytes( long bytes )
		 {
			  return bytes;
		 }

		 public static long KibiBytes( long kibibytes )
		 {
			  return KibiByte.toBytes( kibibytes );
		 }

		 public static long MebiBytes( long mebibytes )
		 {
			  return MebiByte.toBytes( mebibytes );
		 }

		 public static long GibiBytes( long gibibytes )
		 {
			  return GibiByte.toBytes( gibibytes );
		 }

		 public static long TebiBytes( long tebibytes )
		 {
			  return TebiByte.toBytes( tebibytes );
		 }

		 public static long PebiBytes( long pebibytes )
		 {
			  return PebiByte.toBytes( pebibytes );
		 }

		 public static long ExbiBytes( long exbibytes )
		 {
			  return ExbiByte.toBytes( exbibytes );
		 }

		 public static string BytesToString( long bytes )
		 {
			  if ( bytes > OneGibiByte )
			  {
					return format( Locale.ROOT, "%.4g%s", bytes / ( double ) OneGibiByte, GibiByte.shortName );
			  }
			  else if ( bytes > OneMebiByte )
			  {
					return format( Locale.ROOT, "%.4g%s", bytes / ( double ) OneMebiByte, MebiByte.shortName );
			  }
			  else if ( bytes > OneKibiByte )
			  {
					return format( Locale.ROOT, "%.4g%s", bytes / ( double ) OneKibiByte, KibiByte.shortName );
			  }
			  else
			  {
					return bytes + Byte.shortName;
			  }
		 }

		 public static long Parse( string text )
		 {
			  long result = 0;
			  int len = text.Length;
			  int unitCharacter = 0;
			  int digitCharacters = 0;
			  Stream<Pair<string, ByteUnit>> unitsStream = ListUnits();

			  for ( int i = 0; i < len; i++ )
			  {
					char ch = text[i];
					int digit = Character.digit( ch, 10 );
					if ( digit != -1 )
					{
						 if ( unitCharacter != 0 )
						 {
							  throw InvalidFormat( text );
						 }
						 if ( result != 0 )
						 {
							  result *= 10;
						 }
						 result += digit;
						 digitCharacters++;
					}
					else if ( !char.IsWhiteSpace( ch ) )
					{
						 int idx = unitCharacter;
						 unitsStream = unitsStream.filter( p => p.first().length() > idx && p.first().charAt(idx) == ch );
						 unitCharacter++;
					}
			  }

			  if ( digitCharacters == 0 )
			  {
					throw InvalidFormat( text );
			  }

			  if ( unitCharacter > 0 )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					ByteUnit byteUnit = unitsStream.map( Pair::other ).findFirst().orElse(null);
					if ( byteUnit == null )
					{
						 throw InvalidFormat( text );
					}
					result = byteUnit.ToBytes( result );
			  }

			  return result;
		 }

		 private static System.ArgumentException InvalidFormat( string text )
		 {
			  return new System.ArgumentException( "Invalid number format: '" + text + "'" );
		 }

		 private static java.util.stream.Stream<Neo4Net.Helpers.Collections.Pair<string, ByteUnit>> ListUnits()
		 {
			  return Arrays.stream( values() ).flatMap(b => Stream.of(b.names).map(n => Pair.of(n, b)));
		 }

		public static IList<ByteUnit> values()
		{
			return valueList;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static ByteUnit ValueOf( string name )
		{
			foreach ( ByteUnit enumInstance in ByteUnit.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}