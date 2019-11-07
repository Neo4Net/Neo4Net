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
namespace Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.@string
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Parameter = org.junit.runners.Parameterized.Parameter;
	using Parameters = org.junit.runners.Parameterized.Parameters;


	using PageCacheAndDependenciesRule = Neo4Net.Test.rule.PageCacheAndDependenciesRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;
	using RandomValues = Neo4Net.Values.Storable.RandomValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.pagecache.PageCache_Fields.PAGE_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.cache.NumberArrayFactory_Fields.AUTO_WITHOUT_PAGECACHE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.cache.NumberArrayFactory_Fields.CHUNKED_FIXED_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.cache.NumberArrayFactory.HEAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.@unsafe.impl.batchimport.cache.NumberArrayFactory.OFF_HEAP;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class StringCollisionValuesTest
	public class StringCollisionValuesTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.PageCacheAndDependenciesRule storage = new Neo4Net.test.rule.PageCacheAndDependenciesRule().with(new Neo4Net.test.rule.fs.DefaultFileSystemRule());
		 public readonly PageCacheAndDependenciesRule Storage = new PageCacheAndDependenciesRule().with(new DefaultFileSystemRule());
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.RandomRule random = new Neo4Net.test.rule.RandomRule().withConfiguration(new Neo4Net.values.storable.RandomValues.Default()
		 public readonly RandomRule random = new RandomRule().withConfiguration(new DefaultAnonymousInnerClass());

		 private class DefaultAnonymousInnerClass : RandomValues.Default
		 {
			 public override int stringMaxLength()
			 {
				  return ( 1 << ( sizeof( short ) * 8 ) ) - 1;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters public static java.util.Collection<System.Func<Neo4Net.test.rule.PageCacheAndDependenciesRule,Neo4Net.unsafe.impl.batchimport.cache.NumberArrayFactory>> data()
		 public static ICollection<System.Func<PageCacheAndDependenciesRule, NumberArrayFactory>> Data()
		 {
			  return Arrays.asList( Storage => HEAP, Storage => OFF_HEAP, Storage => AUTO_WITHOUT_PAGECACHE, Storage => CHUNKED_FIXED_SIZE, Storage => new PageCachedNumberArrayFactory( Storage.pageCache(), Storage.directory().directory() ) );
		 }

		 [Parameter(0)]
		 public System.Func<PageCacheAndDependenciesRule, NumberArrayFactory> Factory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldStoreAndLoadStrings()
		 public virtual void ShouldStoreAndLoadStrings()
		 {
			  // given
			  using ( StringCollisionValues values = new StringCollisionValues( Factory.apply( Storage ), 10_000 ) )
			  {
					// when
					long[] offsets = new long[100];
					string[] strings = new string[offsets.Length];
					for ( int i = 0; i < offsets.Length; i++ )
					{
						 string @string = random.nextAlphaNumericString();
						 offsets[i] = values.Add( @string );
						 strings[i] = @string;
					}

					// then
					for ( int i = 0; i < offsets.Length; i++ )
					{
						 assertEquals( strings[i], values.Get( offsets[i] ) );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMoveOverToNextChunkOnNearEnd()
		 public virtual void ShouldMoveOverToNextChunkOnNearEnd()
		 {
			  // given
			  using ( StringCollisionValues values = new StringCollisionValues( Factory.apply( Storage ), 10_000 ) )
			  {
					char[] chars = new char[PAGE_SIZE - 3];
					Arrays.fill( chars, 'a' );

					// when
					string @string = new string( chars );
					long offset = values.Add( @string );
					string secondString = "abcdef";
					long secondOffset = values.Add( secondString );

					// then
					string readString = ( string ) values.Get( offset );
					assertEquals( @string, readString );
					string readSecondString = ( string ) values.Get( secondOffset );
					assertEquals( secondString, readSecondString );
			  }
		 }
	}

}