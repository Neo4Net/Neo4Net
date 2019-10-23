using System;
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
	using Parameters = org.junit.runners.Parameterized.Parameters;


	using Neo4Net.Functions;
	using ProgressListener = Neo4Net.Helpers.progress.ProgressListener;
	using Race = Neo4Net.Test.Race;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using RepeatRule = Neo4Net.Test.rule.RepeatRule;
	using Monitor = Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.@string.EncodingIdMapper.Monitor;
	using Comparator = Neo4Net.@unsafe.Impl.Batchimport.cache.idmapping.@string.ParallelSort.Comparator;
	using Collector = Neo4Net.@unsafe.Impl.Batchimport.input.Collector;
	using Group = Neo4Net.@unsafe.Impl.Batchimport.input.Group;
	using Groups = Neo4Net.@unsafe.Impl.Batchimport.input.Groups;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.toIntExact;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyNoMoreInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.collection.PrimitiveLongCollections.count;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.progress.ProgressListener_Fields.NONE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.cache.idmapping.IdMapper_Fields.ID_NOT_FOUND;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.cache.idmapping.@string.EncodingIdMapper.NO_MONITOR;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.@unsafe.impl.batchimport.input.Group_Fields.GLOBAL;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class EncodingIdMapperTest
	public class EncodingIdMapperTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameters(name = "processors:{0}") public static java.util.Collection<Object[]> data()
		 public static ICollection<object[]> Data()
		 {
			  ICollection<object[]> data = new List<object[]>();
			  data.Add( new object[]{ 1 } );
			  data.Add( new object[]{ 2 } );
			  int bySystem = Runtime.Runtime.availableProcessors() - 1;
			  if ( bySystem > 2 )
			  {
					data.Add( new object[]{ bySystem } );
			  }
			  return data;
		 }

		 private readonly int _processors;
		 private readonly Groups _groups = new Groups();

		 public EncodingIdMapperTest( int processors )
		 {
			  this._processors = processors;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleGreatAmountsOfStuff()
		 public virtual void ShouldHandleGreatAmountsOfStuff()
		 {
			  // GIVEN
			  IdMapper idMapper = Mapper( new StringEncoder(), Radix.String, NO_MONITOR );
			  System.Func<long, object> inputIdLookup = string.ValueOf;
			  int count = 300_000;

			  // WHEN
			  for ( long nodeId = 0; nodeId < count; nodeId++ )
			  {
					idMapper.Put( inputIdLookup( nodeId ), nodeId, GLOBAL );
			  }
			  idMapper.Prepare( inputIdLookup, mock( typeof( Collector ) ), NONE );

			  // THEN
			  for ( long nodeId = 0; nodeId < count; nodeId++ )
			  {
					// the UUIDs here will be generated in the same sequence as above because we reset the random
					object id = inputIdLookup( nodeId );
					if ( idMapper.Get( id, GLOBAL ) == ID_NOT_FOUND )
					{
						 fail( "Couldn't find " + id + " even though I added it just previously" );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnExpectedValueForNotFound()
		 public virtual void ShouldReturnExpectedValueForNotFound()
		 {
			  // GIVEN
			  IdMapper idMapper = Mapper( new StringEncoder(), Radix.String, NO_MONITOR );
			  idMapper.Prepare( null, mock( typeof( Collector ) ), NONE );

			  // WHEN
			  long id = idMapper.Get( "123", GLOBAL );

			  // THEN
			  assertEquals( ID_NOT_FOUND, id );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportyProgressForSortAndDetect()
		 public virtual void ShouldReportyProgressForSortAndDetect()
		 {
			  // GIVEN
			  IdMapper idMapper = Mapper( new StringEncoder(), Radix.String, NO_MONITOR );
			  ProgressListener progress = mock( typeof( ProgressListener ) );
			  idMapper.Prepare( null, mock( typeof( Collector ) ), progress );

			  // WHEN
			  long id = idMapper.Get( "123", GLOBAL );

			  // THEN
			  assertEquals( ID_NOT_FOUND, id );
			  verify( progress, times( 3 ) ).started( anyString() );
			  verify( progress, times( 3 ) ).done();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEncodeShortStrings()
		 public virtual void ShouldEncodeShortStrings()
		 {
			  // GIVEN
			  IdMapper mapper = mapper( new StringEncoder(), Radix.String, NO_MONITOR );

			  // WHEN
			  mapper.Put( "123", 0, GLOBAL );
			  mapper.Put( "456", 1, GLOBAL );
			  mapper.Prepare( null, mock( typeof( Collector ) ), NONE );

			  // THEN
			  assertEquals( 1L, mapper.Get( "456", GLOBAL ) );
			  assertEquals( 0L, mapper.Get( "123", GLOBAL ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEncodeSmallSetOfRandomData()
		 public virtual void ShouldEncodeSmallSetOfRandomData()
		 {
			  // GIVEN
			  int size = Random.Next( 10_000 ) + 2;
			  ValueType type = ValueType.values()[Random.Next(ValueType.values().length)];
			  IdMapper mapper = mapper( type.encoder(), type.radix(), NO_MONITOR );

			  // WHEN
			  ValueGenerator values = new ValueGenerator( this, type.data( Random.random() ) );
			  for ( int nodeId = 0; nodeId < size; nodeId++ )
			  {
					mapper.Put( values.Apply( nodeId ), nodeId, GLOBAL );
			  }
			  mapper.Prepare( values, mock( typeof( Collector ) ), NONE );

			  // THEN
			  for ( int nodeId = 0; nodeId < size; nodeId++ )
			  {
					object value = values.Values[nodeId];
					assertEquals( "Expected " + value + " to map to " + nodeId, nodeId, mapper.Get( value, GLOBAL ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportCollisionsForSameInputId()
		 public virtual void ShouldReportCollisionsForSameInputId()
		 {
			  // GIVEN
			  IdMapper mapper = mapper( new StringEncoder(), Radix.String, NO_MONITOR );
			  System.Func<long, object> values = values( "10", "9", "10" );
			  for ( int i = 0; i < 3; i++ )
			  {
					mapper.Put( values( i ), i, GLOBAL );
			  }

			  // WHEN
			  Collector collector = mock( typeof( Collector ) );
			  mapper.Prepare( values, collector, NONE );

			  // THEN
			  verify( collector, times( 1 ) ).collectDuplicateNode( "10", 2, GLOBAL.name() );
			  verifyNoMoreInteractions( collector );
		 }

		 private static System.Func<long, object> Wrap( IList<object> ids )
		 {
			  return nodeId => ids[toIntExact( nodeId )];
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCopeWithCollisionsBasedOnDifferentInputIds()
		 public virtual void ShouldCopeWithCollisionsBasedOnDifferentInputIds()
		 {
			  // GIVEN
			  Monitor monitor = mock( typeof( Monitor ) );
			  Encoder encoder = mock( typeof( Encoder ) );
			  when( encoder.Encode( any() ) ).thenReturn(12345L);
			  IdMapper mapper = mapper( encoder, Radix.String, monitor );
			  System.Func<long, object> ids = Values( "10", "9" );
			  for ( int i = 0; i < 2; i++ )
			  {
					mapper.Put( ids( i ), i, GLOBAL );
			  }

			  // WHEN
			  ProgressListener progress = mock( typeof( ProgressListener ) );
			  Collector collector = mock( typeof( Collector ) );
			  mapper.Prepare( ids, collector, progress );

			  // THEN
			  verifyNoMoreInteractions( collector );
			  verify( monitor ).numberOfCollisions( 2 );
			  assertEquals( 0L, mapper.Get( "10", GLOBAL ) );
			  assertEquals( 1L, mapper.Get( "9", GLOBAL ) );
			  // 7 times since SPLIT+SORT+DETECT+RESOLVE+SPLIT+SORT,DEDUPLICATE
			  verify( progress, times( 7 ) ).started( anyString() );
			  verify( progress, times( 7 ) ).done();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCopeWithMixedActualAndAccidentalCollisions()
		 public virtual void ShouldCopeWithMixedActualAndAccidentalCollisions()
		 {
			  // GIVEN
			  Monitor monitor = mock( typeof( Monitor ) );
			  Encoder encoder = mock( typeof( Encoder ) );
			  // Create these explicit instances so that we can use them in mock, even for same values
			  string a = "a";
			  string b = "b";
			  string c = "c";
			  string a2 = "a";
			  string e = "e";
			  string f = "f";
			  when( encoder.Encode( a ) ).thenReturn( 1L );
			  when( encoder.Encode( b ) ).thenReturn( 1L );
			  when( encoder.Encode( c ) ).thenReturn( 3L );
			  when( encoder.Encode( a2 ) ).thenReturn( 1L );
			  when( encoder.Encode( e ) ).thenReturn( 2L );
			  when( encoder.Encode( f ) ).thenReturn( 1L );
			  Group groupA = _groups.getOrCreate( "A" );
			  Group groupB = _groups.getOrCreate( "B" );
			  IdMapper mapper = mapper( encoder, Radix.String, monitor );
			  System.Func<long, object> ids = Values( "a", "b", "c", "a", "e", "f" );
			  Group[] groups = new Group[] { groupA, groupA, groupA, groupB, groupB, groupB };

			  // a/A --> 1
			  // b/A --> 1 accidental collision with a/A
			  // c/A --> 3
			  // a/B --> 1 actual collision with a/A
			  // e/B --> 2
			  // f/B --> 1 accidental collision with a/A

			  // WHEN
			  for ( int i = 0; i < 6; i++ )
			  {
					mapper.Put( ids( i ), i, groups[i] );
			  }
			  Collector collector = mock( typeof( Collector ) );
			  mapper.Prepare( ids, collector, mock( typeof( ProgressListener ) ) );

			  // THEN
			  verify( monitor ).numberOfCollisions( 4 );
			  assertEquals( 0L, mapper.Get( a, groupA ) );
			  assertEquals( 1L, mapper.Get( b, groupA ) );
			  assertEquals( 2L, mapper.Get( c, groupA ) );
			  assertEquals( 3L, mapper.Get( a2, groupB ) );
			  assertEquals( 4L, mapper.Get( e, groupB ) );
			  assertEquals( 5L, mapper.Get( f, groupB ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToHaveDuplicateInputIdButInDifferentGroups()
		 public virtual void ShouldBeAbleToHaveDuplicateInputIdButInDifferentGroups()
		 {
			  // GIVEN
			  Monitor monitor = mock( typeof( Monitor ) );
			  Group firstGroup = _groups.getOrCreate( "first" );
			  Group secondGroup = _groups.getOrCreate( "second" );
			  IdMapper mapper = mapper( new StringEncoder(), Radix.String, monitor );
			  System.Func<long, object> ids = Values( "10", "9", "10" );
			  int id = 0;
			  // group 0
			  mapper.Put( ids( id ), id++, firstGroup );
			  mapper.Put( ids( id ), id++, firstGroup );
			  // group 1
			  mapper.Put( ids( id ), id++, secondGroup );
			  Collector collector = mock( typeof( Collector ) );
			  mapper.Prepare( ids, collector, NONE );

			  // WHEN/THEN
			  verifyNoMoreInteractions( collector );
			  verify( monitor ).numberOfCollisions( 0 );
			  assertEquals( 0L, mapper.Get( "10", firstGroup ) );
			  assertEquals( 1L, mapper.Get( "9", firstGroup ) );
			  assertEquals( 2L, mapper.Get( "10", secondGroup ) );
			  assertFalse( mapper.LeftOverDuplicateNodesIds().hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyFindInputIdsInSpecificGroup()
		 public virtual void ShouldOnlyFindInputIdsInSpecificGroup()
		 {
			  // GIVEN
			  Group firstGroup = _groups.getOrCreate( "first" );
			  Group secondGroup = _groups.getOrCreate( "second" );
			  Group thirdGroup = _groups.getOrCreate( "third" );
			  IdMapper mapper = mapper( new StringEncoder(), Radix.String, NO_MONITOR );
			  System.Func<long, object> ids = Values( "8", "9", "10" );
			  int id = 0;
			  mapper.Put( ids( id ), id++, firstGroup );
			  mapper.Put( ids( id ), id++, secondGroup );
			  mapper.Put( ids( id ), id++, thirdGroup );
			  mapper.Prepare( ids, mock( typeof( Collector ) ), NONE );

			  // WHEN/THEN
			  assertEquals( 0L, mapper.Get( "8", firstGroup ) );
			  assertEquals( ID_NOT_FOUND, mapper.Get( "8", secondGroup ) );
			  assertEquals( ID_NOT_FOUND, mapper.Get( "8", thirdGroup ) );

			  assertEquals( ID_NOT_FOUND, mapper.Get( "9", firstGroup ) );
			  assertEquals( 1L, mapper.Get( "9", secondGroup ) );
			  assertEquals( ID_NOT_FOUND, mapper.Get( "9", thirdGroup ) );

			  assertEquals( ID_NOT_FOUND, mapper.Get( "10", firstGroup ) );
			  assertEquals( ID_NOT_FOUND, mapper.Get( "10", secondGroup ) );
			  assertEquals( 2L, mapper.Get( "10", thirdGroup ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleManyGroups()
		 public virtual void ShouldHandleManyGroups()
		 {
			  // GIVEN
			  int size = 256; // which results in GLOBAL (0) + 1-256 = 257 groups, i.e. requiring two bytes
			  for ( int i = 0; i < size; i++ )
			  {
					_groups.getOrCreate( "" + i );
			  }
			  IdMapper mapper = mapper( new LongEncoder(), Radix.Long, NO_MONITOR );

			  // WHEN
			  for ( int i = 0; i < size; i++ )
			  {
					mapper.Put( i, i, _groups.get( "" + i ) );
			  }
			  // null since this test should have been set up to not run into collisions
			  mapper.Prepare( null, mock( typeof( Collector ) ), NONE );

			  // THEN
			  for ( int i = 0; i < size; i++ )
			  {
					assertEquals( i, mapper.Get( i, _groups.get( "" + i ) ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectCorrectDuplicateInputIdsWhereManyAccidentalInManyGroups()
		 public virtual void ShouldDetectCorrectDuplicateInputIdsWhereManyAccidentalInManyGroups()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ControlledEncoder encoder = new ControlledEncoder(new LongEncoder());
			  ControlledEncoder encoder = new ControlledEncoder( new LongEncoder() );
			  const int idsPerGroup = 20;
			  int groupCount = 5;
			  for ( int i = 0; i < groupCount; i++ )
			  {
					_groups.getOrCreate( "Group " + i );
			  }
			  IdMapper mapper = mapper( encoder, Radix.Long, NO_MONITOR, ParallelSort.DEFAULT, numberOfCollisions => new LongCollisionValues( NumberArrayFactory.HEAP, numberOfCollisions ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicReference<org.Neo4Net.unsafe.impl.batchimport.input.Group> group = new java.util.concurrent.atomic.AtomicReference<>();
			  AtomicReference<Group> group = new AtomicReference<Group>();
			  System.Func<long, object> ids = nodeId =>
			  {
				int groupId = toIntExact( nodeId / idsPerGroup );
				if ( groupId == groupCount )
				{
					 return null;
				}
				group.set( _groups.get( groupId ) );

				// Let the first 10% in each group be accidental collisions with each other
				// i.e. all first 10% in each group collides with all other first 10% in each group
				if ( nodeId % idsPerGroup < 2 )
				{ // Let these colliding values encode into the same eId as well,
					 // so that they are definitely marked as collisions
					 encoder.UseThisIdToEncodeNoMatterWhatComesIn( Convert.ToInt64( 1234567 ) );
					 return Convert.ToInt64( nodeId % idsPerGroup );
				}

				// The other 90% will be accidental collisions for something else
				encoder.UseThisIdToEncodeNoMatterWhatComesIn( Convert.ToInt64( 123456 - group.get().id() ) );
				return Convert.ToInt64( nodeId );
			  };

			  // WHEN
			  int count = idsPerGroup * groupCount;
			  for ( long nodeId = 0; nodeId < count; nodeId++ )
			  {
					mapper.Put( ids( nodeId ), nodeId, group.get() );
			  }
			  Collector collector = mock( typeof( Collector ) );

			  mapper.Prepare( ids, collector, NONE );

			  // THEN
			  verifyNoMoreInteractions( collector );
			  for ( long nodeId = 0; nodeId < count; nodeId++ )
			  {
					assertEquals( nodeId, mapper.Get( ids( nodeId ), group.get() ) );
			  }
			  verifyNoMoreInteractions( collector );
			  assertFalse( mapper.LeftOverDuplicateNodesIds().hasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleHolesInIdSequence()
		 public virtual void ShouldHandleHolesInIdSequence()
		 {
			  // GIVEN
			  IdMapper mapper = mapper( new LongEncoder(), Radix.Long, NO_MONITOR );
			  IList<object> ids = new List<object>();
			  for ( int i = 0; i < 100; i++ )
			  {
					if ( Random.nextBoolean() )
					{
						 // Skip this one
					}
					else
					{
						 long? id = ( long ) i;
						 ids.Add( id );
						 mapper.Put( id, i, GLOBAL );
					}
			  }

			  // WHEN
			  mapper.Prepare( Values( ids.ToArray() ), mock(typeof(Collector)), NONE );

			  // THEN
			  foreach ( object id in ids )
			  {
					assertEquals( ( ( long? )id ).Value, mapper.Get( id, GLOBAL ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleLargeAmountsOfDuplicateNodeIds()
		 public virtual void ShouldHandleLargeAmountsOfDuplicateNodeIds()
		 {
			  // GIVEN
			  IdMapper mapper = mapper( new LongEncoder(), Radix.Long, NO_MONITOR );
			  long nodeId = 0;
			  int high = 10;
			  // a list of input ids
			  IList<object> ids = new List<object>();
			  for ( int run = 0; run < 2; run++ )
			  {
					for ( long i = 0; i < high / 2; i++ )
					{
						 ids.Add( high - ( i + 1 ) );
						 ids.Add( i );
					}
			  }
			  // fed to the IdMapper
			  foreach ( object inputId in ids )
			  {
					mapper.Put( inputId, nodeId++, GLOBAL );
			  }

			  // WHEN
			  Collector collector = mock( typeof( Collector ) );
			  mapper.Prepare( Values( ids.ToArray() ), collector, NONE );

			  // THEN
			  verify( collector, times( high ) ).collectDuplicateNode( any( typeof( object ) ), anyLong(), anyString() );
			  assertEquals( high, count( mapper.LeftOverDuplicateNodesIds() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectLargeAmountsOfCollisions()
		 public virtual void ShouldDetectLargeAmountsOfCollisions()
		 {
			  // GIVEN
			  IdMapper mapper = mapper( new StringEncoder(), Radix.String, NO_MONITOR );
			  int count = 20_000;
			  IList<object> ids = new List<object>();
			  long id = 0;

			  // Generate and add all input ids
			  for ( int elements = 0; elements < count; elements++ )
			  {
					string inputId = System.Guid.randomUUID().ToString();
					for ( int i = 0; i < 2; i++ )
					{
						 ids.Add( inputId );
						 mapper.Put( inputId, id++, GLOBAL );
					}
			  }

			  // WHEN
			  CountingCollector collector = new CountingCollector();
			  mapper.Prepare( Values( ids.ToArray() ), collector, NONE );

			  // THEN
			  assertEquals( count, collector.Count );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPutFromMultipleThreads() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPutFromMultipleThreads()
		 {
			  // GIVEN
			  IdMapper idMapper = Mapper( new StringEncoder(), Radix.String, NO_MONITOR );
			  AtomicLong highNodeId = new AtomicLong();
			  int batchSize = 1234;
			  Race race = new Race();
			  System.Func<long, object> inputIdLookup = string.ValueOf;
			  int countPerThread = 30_000;
			  race.AddContestants(_processors, () =>
			  {
				int cursor = batchSize;
				long nextNodeId = 0;
				for ( int j = 0; j < countPerThread; j++ )
				{
					 if ( cursor == batchSize )
					 {
						  nextNodeId = highNodeId.getAndAdd( batchSize );
						  cursor = 0;
					 }
					 long nodeId = nextNodeId++;
					 cursor++;

					 idMapper.Put( inputIdLookup( nodeId ), nodeId, GLOBAL );
				}
			  });

			  // WHEN
			  race.Go();
			  idMapper.Prepare( inputIdLookup, mock( typeof( Collector ) ), Neo4Net.Helpers.progress.ProgressListener_Fields.None );

			  // THEN
			  int count = _processors * countPerThread;
			  int countWithGapsWorstCase = count + batchSize * _processors;
			  int correctHits = 0;
			  for ( long nodeId = 0; nodeId < countWithGapsWorstCase; nodeId++ )
			  {
					long result = idMapper.Get( inputIdLookup( nodeId ), GLOBAL );
					if ( result != -1 )
					{
						 assertEquals( nodeId, result );
						 correctHits++;
					}
			  }
			  assertEquals( count, correctHits );
		 }

		 private System.Func<long, object> Values( params object[] values )
		 {
			  return value => values[toIntExact( value )];
		 }

		 private IdMapper Mapper( Encoder encoder, IFactory<Radix> radix, Monitor monitor )
		 {
			  return Mapper( encoder, radix, monitor, ParallelSort.DEFAULT );
		 }

		 private IdMapper Mapper( Encoder encoder, IFactory<Radix> radix, Monitor monitor, Comparator comparator )
		 {
			  return Mapper( encoder, radix, monitor, comparator, AutoDetect( encoder ) );
		 }

		 private IdMapper Mapper( Encoder encoder, IFactory<Radix> radix, Monitor monitor, Comparator comparator, System.Func<long, CollisionValues> collisionValuesFactory )
		 {
			  return new EncodingIdMapper( NumberArrayFactory.HEAP, encoder, radix, monitor, _randomTrackerFactory, _groups, collisionValuesFactory, 1_000, _processors, comparator );
		 }

		 private System.Func<long, CollisionValues> AutoDetect( Encoder encoder )
		 {
			  return numberOfCollisions => encoder is LongEncoder ? new LongCollisionValues( NumberArrayFactory.HEAP, numberOfCollisions ) : new StringCollisionValues( NumberArrayFactory.HEAP, numberOfCollisions );

		 }

		 private static readonly TrackerFactory _randomTrackerFactory = ( arrayFactory, size ) => DateTimeHelper.CurrentUnixTimeMillis() % 2 == 0 ? new IntTracker(arrayFactory.newIntArray(size, IntTracker.DEFAULT_VALUE)) : new BigIdTracker(arrayFactory.newByteArray(size, BigIdTracker.DefaultValue));

		 private class ValueGenerator : System.Func<long, object>
		 {
			 private readonly EncodingIdMapperTest _outerInstance;

			  internal readonly IFactory<object> Generator;
			  internal readonly IList<object> Values = new List<object>();
			  internal readonly ISet<object> Deduper = new HashSet<object>();

			  internal ValueGenerator( EncodingIdMapperTest outerInstance, IFactory<object> generator )
			  {
				  this._outerInstance = outerInstance;
					this.Generator = generator;
			  }

			  public override object Apply( long nodeId )
			  {
					while ( true )
					{
						 object value = Generator.newInstance();
						 if ( Deduper.Add( value ) )
						 {
							  Values.Add( value );
							  return value;
						 }
					}
			  }
		 }

		 private abstract class ValueType
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           LONGS { Encoder encoder() { return new LongEncoder(); } IFactory<Radix> radix() { return Radix.LONG; } IFactory<Object> data(final java.util.Random random) { return() -> random.nextInt(1_000_000_000); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           LONGS_AS_STRINGS { Encoder encoder() { return new StringEncoder(); } IFactory<Radix> radix() { return Radix.STRING; } IFactory<Object> data(final java.util.Random random) { return() -> String.ValueOf(random.nextInt(1_000_000_000)); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           VERY_LONG_STRINGS { char[] CHARS = "½!\"#¤%&/()=?`´;:,._-<>".toCharArray(); Encoder encoder() { return new StringEncoder(); } IFactory<Radix> radix() { return Radix.STRING; } IFactory<Object> data(final java.util.Random random) { return new org.Neo4Net.function.Factory<Object>() { public Object newInstance() { int length = 1500; for(int i = 0; i < 4; i++) { length = random.nextInt(length) + 20; } char[] chars = new char[length]; for(int i = 0; i < length; i++) { char ch; if(random.nextBoolean()) { ch = randomLetter(random); } else { ch = CHARS[random.nextInt(CHARS.length)]; } chars[i] = ch; } return new String(chars); } private char randomLetter(java.util.Random random) { int super; if(random.nextBoolean()) { super = 'a'; } else { super = 'A'; } int size = 'z' - 'a'; return(char)(super + random.nextInt(size)); } }; } };

			  private static readonly IList<ValueType> valueList = new List<ValueType>();

			  static ValueType()
			  {
				  valueList.Add( LONGS );
				  valueList.Add( LONGS_AS_STRINGS );
				  valueList.Add( VERY_LONG_STRINGS );
			  }

			  public enum InnerEnum
			  {
				  LONGS,
				  LONGS_AS_STRINGS,
				  VERY_LONG_STRINGS
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private ValueType( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal abstract Encoder encoder();

			  internal abstract Neo4Net.Functions.Factory<Radix> radix();

			  internal abstract Neo4Net.Functions.Factory<object> data( Random random );

			 public static IList<ValueType> values()
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

			 public static ValueType ValueOf( string name )
			 {
				 foreach ( ValueType enumInstance in ValueType.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.RandomRule random = new org.Neo4Net.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.RepeatRule repeater = new org.Neo4Net.test.rule.RepeatRule();
		 public readonly RepeatRule Repeater = new RepeatRule();

		 private class CountingCollector : Collector
		 {
			  internal int Count;

			  public override void CollectBadRelationship( object startId, string startIdGroup, string type, object endId, string endIdGroup, object specificValue )
			  {
					throw new System.NotSupportedException();
			  }

			  public override void CollectDuplicateNode( object id, long actualId, string group )
			  {
					Count++;
			  }

			  public virtual bool CollectingBadRelationships
			  {
				  get
				  {
						return false;
				  }
			  }

			  public override void CollectExtraColumns( string source, long row, string value )
			  {
					throw new System.NotSupportedException();
			  }

			  public override long BadEntries()
			  {
					throw new System.NotSupportedException();
			  }

			  public override void Close()
			  { // Nothing to close
			  }
		 }
	}

}