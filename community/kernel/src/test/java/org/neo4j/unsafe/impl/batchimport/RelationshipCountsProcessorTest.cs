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
namespace Org.Neo4j.@unsafe.Impl.Batchimport
{
	using Test = org.junit.Test;
	using ArgumentMatcher = org.mockito.ArgumentMatcher;

	using CountsAccessor = Org.Neo4j.Kernel.Impl.Api.CountsAccessor;
	using RelationshipRecord = Org.Neo4j.Kernel.impl.store.record.RelationshipRecord;
	using NodeLabelsCache = Org.Neo4j.@unsafe.Impl.Batchimport.cache.NodeLabelsCache;
	using NumberArrayFactory = Org.Neo4j.@unsafe.Impl.Batchimport.cache.NumberArrayFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.longThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class RelationshipCountsProcessorTest
	{

		 private const int ANY = -1;
		 private readonly NodeLabelsCache _nodeLabelCache = mock( typeof( NodeLabelsCache ) );
		 private readonly Org.Neo4j.Kernel.Impl.Api.CountsAccessor_Updater _countsUpdater = mock( typeof( Org.Neo4j.Kernel.Impl.Api.CountsAccessor_Updater ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleBigNumberOfLabelsAndRelationshipTypes()
		 public virtual void ShouldHandleBigNumberOfLabelsAndRelationshipTypes()
		 {
			  /*
			   * This test ensures that the RelationshipCountsProcessor does not attempt to allocate a negative amount
			   * of memory when trying to get an array to store the relationship counts. This could happen when the labels
			   * and relationship types were enough in number to overflow an integer used to hold a product of those values.
			   * Here we ask the Processor to do that calculation and ensure that the number passed to the NumberArrayFactory
			   * is positive.
			   */
			  // Given
			  /*
			   * A large but not impossibly large number of labels and relationship types. These values are the simplest
			   * i could find in a reasonable amount of time that would result in an overflow. Given that the calculation
			   * involves squaring the labelCount, 22 bits are more than enough for an integer to overflow. However, the
			   * actual issue involves adding a product of relTypeCount and some other things, which makes hard to predict
			   * which values will make it go negative. These worked. Given that with these values the integer overflows
			   * some times over, it certainly works with much smaller numbers, but they don't come out of a nice simple bit
			   * shifting.
			   */
			  int relTypeCount = 1 << 8;
			  int labelCount = 1 << 22;
			  NumberArrayFactory numberArrayFactory = mock( typeof( NumberArrayFactory ) );

			  // When
			  new RelationshipCountsProcessor( _nodeLabelCache, labelCount, relTypeCount, _countsUpdater, numberArrayFactory );

			  // Then
			  verify( numberArrayFactory, times( 2 ) ).newLongArray( longThat( new IsNonNegativeLong( this ) ), anyLong() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testRelationshipCountersUpdates()
		 public virtual void TestRelationshipCountersUpdates()
		 {
			  int relationTypes = 2;
			  int labels = 3;

			  NodeLabelsCache.Client client = mock( typeof( NodeLabelsCache.Client ) );
			  when( _nodeLabelCache.newClient() ).thenReturn(client);
			  when( _nodeLabelCache.get( eq( client ), eq( 1L ), any( typeof( int[] ) ) ) ).thenReturn( new int[]{ 0, 2 } );
			  when( _nodeLabelCache.get( eq( client ), eq( 2L ), any( typeof( int[] ) ) ) ).thenReturn( new int[]{ 1 } );
			  when( _nodeLabelCache.get( eq( client ), eq( 3L ), any( typeof( int[] ) ) ) ).thenReturn( new int[]{ 1, 2 } );
			  when( _nodeLabelCache.get( eq( client ), eq( 4L ), any( typeof( int[] ) ) ) ).thenReturn( new int[]{} );

			  RelationshipCountsProcessor countsProcessor = new RelationshipCountsProcessor( _nodeLabelCache, labels, relationTypes, _countsUpdater, Org.Neo4j.@unsafe.Impl.Batchimport.cache.NumberArrayFactory_Fields.AutoWithoutPagecache );

			  countsProcessor.Process( Record( 1, 0, 3 ) );
			  countsProcessor.Process( Record( 2, 1, 4 ) );

			  countsProcessor.Done();

			  // wildcard counts
			  verify( _countsUpdater ).incrementRelationshipCount( ANY, ANY, ANY, 2L );
			  verify( _countsUpdater ).incrementRelationshipCount( ANY, 0, ANY, 1L );
			  verify( _countsUpdater ).incrementRelationshipCount( ANY, 1, ANY, 1L );

			  // start labels counts
			  verify( _countsUpdater ).incrementRelationshipCount( 0, 0, ANY, 1L );
			  verify( _countsUpdater ).incrementRelationshipCount( 2, 0, ANY, 1L );

			  // end labels counts
			  verify( _countsUpdater ).incrementRelationshipCount( ANY, 0, 1, 1L );
			  verify( _countsUpdater ).incrementRelationshipCount( ANY, 0, 2, 1L );
		 }

		 private class IsNonNegativeLong : ArgumentMatcher<long>
		 {
			 private readonly RelationshipCountsProcessorTest _outerInstance;

			 public IsNonNegativeLong( RelationshipCountsProcessorTest outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override bool Matches( long? argument )
			  {
					return argument != null && argument >= 0;
			  }
		 }

		 private RelationshipRecord Record( long startNode, int type, long endNode )
		 {
			  RelationshipRecord record = new RelationshipRecord( 0 );
			  record.InUse = true;
			  record.FirstNode = startNode;
			  record.SecondNode = endNode;
			  record.Type = type;
			  return record;
		 }
	}

}