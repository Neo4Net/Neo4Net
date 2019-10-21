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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.@string
{
	using MutableInt = org.apache.commons.lang3.mutable.MutableInt;

	using Neo4Net.Functions;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.pow;

	/// <summary>
	/// Calculates and keeps radix counts. Uses a <seealso cref="RadixCalculator"/> to calculate an integer radix value
	/// from a long value.
	/// </summary>
	public abstract class Radix
	{
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		 public static readonly IFactory<Radix> Long = long?::new;

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		 public static readonly IFactory<Radix> String = String::new;

		 internal readonly int[] RadixIndexCount = new int[( int ) pow( 2, RadixCalculator.RADIX_BITS - 1 )];

		 public virtual int RegisterRadixOf( long value )
		 {
			  int radix = Calculator().radixOf(value);
			  RadixIndexCount[radix]++;
			  return radix;
		 }

		 public virtual int[] RadixIndexCounts
		 {
			 get
			 {
				  return RadixIndexCount;
			 }
		 }

		 public abstract RadixCalculator Calculator();

		 public override string ToString()
		 {
			  return typeof( Radix ).Name + "." + this.GetType().Name;
		 }

		 public class String : Radix
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly RadixCalculator CalculatorConflict;

			  public String()
			  {
					this.CalculatorConflict = new RadixCalculator.String();
			  }

			  public override RadixCalculator Calculator()
			  {
					return CalculatorConflict;
			  }
		 }

		 public class Long : Radix
		 {
			  internal readonly MutableInt RadixShift;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly RadixCalculator CalculatorConflict;

			  public Long()
			  {
					this.RadixShift = new MutableInt();
					this.CalculatorConflict = new RadixCalculator.Long( RadixShift );
			  }

			  public override RadixCalculator Calculator()
			  {
					return CalculatorConflict;
			  }

			  public override int RegisterRadixOf( long value )
			  {
					RadixOverflow( value );
					return base.RegisterRadixOf( value );
			  }

			  internal virtual void RadixOverflow( long val )
			  {
					long shiftVal = ( val & ~RadixCalculator.LENGTH_BITS ) >> ( RadixCalculator.RADIX_BITS - 1 + RadixShift.intValue() );
					if ( shiftVal > 0 )
					{
						 while ( shiftVal > 0 )
						 {
							  RadixShift.increment();
							  CompressRadixIndex();
							  shiftVal = shiftVal >> 1;
						 }
					}
			  }

			  internal virtual void CompressRadixIndex()
			  {
					for ( int i = 0; i < RadixIndexCount.Length / 2; i++ )
					{
						 RadixIndexCount[i] = RadixIndexCount[2 * i] + RadixIndexCount[2 * i + 1];
					}
					for ( int i = RadixIndexCount.Length / 2; i < RadixIndexCount.Length; i++ )
					{
						 RadixIndexCount[i] = 0;
					}
			  }
		 }
	}

}