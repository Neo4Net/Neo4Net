using System.Threading;

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
namespace Neo4Net.Kernel.Impl.Api.index
{
	using Test = org.junit.Test;

	using Race = Neo4Net.Test.Race;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertSame;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.schema.SchemaDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.api.index.TestIndexProviderDescriptor.PROVIDER_DESCRIPTOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.storageengine.api.schema.IndexDescriptorFactory.forSchema;

	public class IndexMapReferenceTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSynchronizeModifications() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSynchronizeModifications()
		 {
			  // given
			  IndexMapReference @ref = new IndexMapReference();
			  IndexProxy[] existing = MockedIndexProxies( 5, 0 );
			  @ref.Modify(indexMap =>
			  {
				for ( int i = 0; i < existing.Length; i++ )
				{
					 indexMap.putIndexProxy( existing[i] );
				}
				return indexMap;
			  });

			  // when
			  Race race = new Race();
			  for ( int i = 0; i < existing.Length; i++ )
			  {
					race.AddContestant( RemoveIndexProxy( @ref, i ), 1 );
			  }
			  IndexProxy[] created = MockedIndexProxies( 3, existing.Length );
			  for ( int i = 0; i < existing.Length; i++ )
			  {
					race.AddContestant( PutIndexProxy( @ref, created[i] ), 1 );
			  }
			  race.Go();

			  // then
			  for ( int i = 0; i < existing.Length; i++ )
			  {
					assertNull( @ref.getIndexProxy( i ) );
			  }
			  for ( int i = 0; i < created.Length; i++ )
			  {
					assertSame( created[i], @ref.getIndexProxy( existing.Length + i ) );
			  }
		 }

		 private ThreadStart PutIndexProxy( IndexMapReference @ref, IndexProxy proxy )
		 {
			  return () => @ref.modify(indexMap =>
			  {
				indexMap.putIndexProxy( proxy );
				return indexMap;
			  });
		 }

		 private ThreadStart RemoveIndexProxy( IndexMapReference @ref, long indexId )
		 {
			  return () => @ref.modify(indexMap =>
			  {
				indexMap.removeIndexProxy( indexId );
				return indexMap;
			  });
		 }

		 private IndexProxy[] MockedIndexProxies( int @base, int count )
		 {
			  IndexProxy[] existing = new IndexProxy[count];
			  for ( int i = 0; i < count; i++ )
			  {
					existing[i] = mock( typeof( IndexProxy ) );
					when( existing[i].Descriptor ).thenReturn( forSchema( forLabel( @base + i, 1 ), PROVIDER_DESCRIPTOR ).withId( i ).withoutCapabilities() );
			  }
			  return existing;
		 }
	}

}