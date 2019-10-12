using System.Collections.Generic;

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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.cache.idmapping.@string
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;
	using Parameter = org.junit.runners.Parameterized.Parameter;
	using Parameters = org.junit.runners.Parameterized.Parameters;


	using PageCacheAndDependenciesRule = Org.Neo4j.Test.rule.PageCacheAndDependenciesRule;
	using RandomRule = Org.Neo4j.Test.rule.RandomRule;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;
	using RandomValues = Org.Neo4j.Values.Storable.RandomValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.pagecache.PageCache_Fields.PAGE_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory_Fields.AUTO_WITHOUT_PAGECACHE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory_Fields.CHUNKED_FIXED_SIZE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory.HEAP;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.cache.NumberArrayFactory.OFF_HEAP;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class StringCollisionValuesTest
	public class StringCollisionValuesTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.PageCacheAndDependenciesRule storage = new org.neo4j.test.rule.PageCacheAndDependenciesRule().with(new org.neo4j.test.rule.fs.DefaultFileSystemRule());
		 public readonly PageCacheAndDependenciesRule Storage = new PageCacheAndDependenciesRule().with(new DefaultFileSystemRule());
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.RandomRule random = new org.neo4j.test.rule.RandomRule().withConfiguration(new org.neo4j.values.storable.RandomValues.Default()
		 public readonly RandomRule random = new RandomRule().withConfiguration(new DefaultAnonymousInnerClass());

		 private class DefaultAnonymousInnerClass : RandomValues.Default
		 {
			 public override int stringMaxLength()
			 {
				  return ( 1 << ( sizeof( short ) * 8 ) ) - 1;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters public static java.util.Collection<System.Func<org.neo4j.test.rule.PageCacheAndDependenciesRule,org.neo4j.unsafe.impl.batchimport.cache.NumberArrayFactory>> data()
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