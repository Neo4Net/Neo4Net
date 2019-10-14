using System.Text;

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
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using IndexSpecificSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.IndexSpecificSpaceFillingCurveSettingsCache;
	using Inject = Neo4Net.Test.extension.Inject;
	using RandomExtension = Neo4Net.Test.extension.RandomExtension;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(RandomExtension.class) class CompositeGenericKeyTest
	internal class CompositeGenericKeyTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject RandomRule random;
		 internal RandomRule Random;

		 /// <summary>
		 /// This test verify that the documented formula for calculating size limit for string array
		 /// actually calculate correctly.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void testDocumentedStringArrayKeySizeFormulaIsCorrect()
		 internal virtual void TestDocumentedStringArrayKeySizeFormulaIsCorrect()
		 {
			  CompositeGenericKey key = new CompositeGenericKey( 1, mock( typeof( IndexSpecificSpaceFillingCurveSettingsCache ) ) );
			  int maxArrayLength = Random.Next( 500 );
			  int maxStringLength = Random.Next( 100 );
			  for ( int i = 0; i < 100; i++ )
			  {
					string[] strings = Random.randomValues().nextStringArrayRaw(0, maxArrayLength, 0, maxStringLength);
					key.initialize( i );
					key.WriteValue( 0, Values.of( strings ), NativeIndexKey.Inclusion.Neutral );
					assertThat( IncludingEntityId( CalculateKeySize( strings ) ), equalTo( key.Size() ) );
			  }
		 }

		 private int IncludingEntityId( int keySize )
		 {
			  return Long.BYTES + keySize;
		 }

		 private int CalculateKeySize( string[] strings )
		 {
			  int arrayLength = strings.Length;
			  int totalStringLength = 0;
			  foreach ( string @string in strings )
			  {
					totalStringLength += @string.GetBytes( Encoding.UTF8 ).length;
			  }
			  return 1 + 2 + 2 * arrayLength + totalStringLength;
		 }
	}

}