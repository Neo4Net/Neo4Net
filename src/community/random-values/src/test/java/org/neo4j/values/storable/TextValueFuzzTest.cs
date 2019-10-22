using System;

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
namespace Neo4Net.Values.Storable
{

	using Disabled = org.junit.jupiter.api.Disabled;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using Inject = Neo4Net.Test.extension.Inject;
	using RandomExtension = Neo4Net.Test.extension.RandomExtension;
	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.StringHelpers.assertConsistent;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(RandomExtension.class) class TextValueFuzzTest
	internal class TextValueFuzzTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.Neo4Net.test.rule.RandomRule random;
		 private RandomRule _random;

		 private const int ITERATIONS = 1000;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Disabled("we have decided to stick with String::compareTo under the hood which doesn't respect code point order " + "whenever the code point doesn't fit 16bits") @Test void shouldCompareToForAllValidStrings()
		 internal virtual void ShouldCompareToForAllValidStrings()
		 {
			  for ( int i = 0; i < ITERATIONS; i++ )
			  {
					assertConsistent( _random.nextString(), _random.nextString(), (t1, t2) => Math.Sign(t1.compareTo(t2)) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldCompareToForAllStringsInBasicMultilingualPlane()
		 internal virtual void ShouldCompareToForAllStringsInBasicMultilingualPlane()
		 {
			  for ( int i = 0; i < ITERATIONS; i++ )
			  {
					assertConsistent( _random.nextBasicMultilingualPlaneString(), _random.nextBasicMultilingualPlaneString(), (t1, t2) => Math.Sign(t1.compareTo(t2)) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldAdd()
		 internal virtual void ShouldAdd()
		 {
			  for ( int i = 0; i < ITERATIONS; i++ )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					assertConsistent( _random.nextString(), _random.nextString(), TextValue::plus );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldComputeLength()
		 internal virtual void ShouldComputeLength()
		 {
			  for ( int i = 0; i < ITERATIONS; i++ )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					assertConsistent( _random.nextString(), TextValue::length );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReverse()
		 internal virtual void ShouldReverse()
		 {
			  for ( int i = 0; i < ITERATIONS; i++ )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					assertConsistent( _random.nextString(), TextValue::reverse );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldTrim()
		 internal virtual void ShouldTrim()
		 {
			  for ( int i = 0; i < ITERATIONS; i++ )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					assertConsistent( _random.nextString(), TextValue::trim );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldHandleStringPredicates()
		 internal virtual void ShouldHandleStringPredicates()
		 {
			  for ( int i = 0; i < ITERATIONS; i++ )
			  {
					string value = _random.nextString();
					string other;
					if ( _random.nextBoolean() )
					{
						 other = value;
					}
					else
					{
						 other = _random.nextString();
					}

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					assertConsistent( value, other, TextValue::startsWith );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					assertConsistent( value, other, TextValue::endsWith );
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
					assertConsistent( value, other, TextValue::contains );
			  }
		 }

	}

}