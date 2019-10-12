using System;
using System.Threading;

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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{
	using MutableInt = org.apache.commons.lang3.mutable.MutableInt;
	using Test = org.junit.Test;


	using Iterators = Org.Neo4j.Helpers.Collection.Iterators;
	using Race = Org.Neo4j.Test.Race;
	using CoordinateReferenceSystem = Org.Neo4j.Values.Storable.CoordinateReferenceSystem;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.count;

	public class SpatialIndexCacheTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("Duplicates") @Test public void stressCache() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StressCache()
		 {
			  StringFactory factory = new StringFactory();
			  SpatialIndexCache<string> cache = new SpatialIndexCache<string>( factory );

			  ExecutorService pool = Executors.newFixedThreadPool( 20 );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?>[] futures = new java.util.concurrent.Future[100];
			  Future<object>[] futures = new Future[100];
			  AtomicBoolean shouldContinue = new AtomicBoolean( true );

			  try
			  {
					for ( int i = 0; i < futures.Length; i++ )
					{
						 futures[i] = pool.submit( new CacheStresser( cache, shouldContinue ) );
					}

					Thread.Sleep( 5_000 );

					shouldContinue.set( false );

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (java.util.concurrent.Future<?> future : futures)
					foreach ( Future<object> future in futures )
					{
						 future.get( 10, TimeUnit.SECONDS );
					}
			  }
			  finally
			  {
					pool.shutdown();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void stressInstantiationWithClose() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void StressInstantiationWithClose()
		 {
			  // given
			  StringFactory factory = new StringFactory();
			  SpatialIndexCache<string> cache = new SpatialIndexCache<string>( factory );
			  Race race = ( new Race() ).withRandomStartDelays();
			  MutableInt instantiatedAtClose = new MutableInt();
			  race.AddContestant(() =>
			  {
				try
				{
					 cache.UncheckedSelect( CoordinateReferenceSystem.WGS84 );
					 cache.UncheckedSelect( CoordinateReferenceSystem.Cartesian_3D );
				}
				catch ( System.InvalidOperationException )
				{
					 // This exception is OK since it may have been closed
				}
			  }, 1);
			  race.AddContestant(() =>
			  {
				cache.CloseInstantiateCloseLock();
				instantiatedAtClose.Value = count( cache );
			  }, 1);

			  // when
			  race.Go();

			  // then
			  try
			  {
					cache.UncheckedSelect( CoordinateReferenceSystem.Cartesian );
					fail( "No instantiation after closed" );
			  }
			  catch ( System.InvalidOperationException )
			  {
					// good
			  }
			  assertEquals( instantiatedAtClose.intValue(), count(cache) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
		 private static readonly CoordinateReferenceSystem[] _coordinateReferenceSystems = Iterators.stream( CoordinateReferenceSystem.all().GetEnumerator() ).toArray(CoordinateReferenceSystem[]::new);

		 internal class CacheStresser : ThreadStart
		 {
			  internal readonly SpatialIndexCache<string> Cache;
			  internal readonly AtomicBoolean ShouldContinue;
			  internal readonly Random Random = new Random();

			  internal CacheStresser( SpatialIndexCache<string> cache, AtomicBoolean shouldContinue )
			  {
					this.Cache = cache;
					this.ShouldContinue = shouldContinue;
			  }

			  public override void Run()
			  {
					while ( ShouldContinue.get() )
					{
						 Stress();
					}
			  }

			  internal virtual void Stress()
			  {
					// select
					Cache.select( _coordinateReferenceSystems[Random.Next( _coordinateReferenceSystems.Length )] );

					// iterate
					foreach ( string s in Cache )
					{
						 if ( string.ReferenceEquals( s, null ) )
						 {
							  throw new System.InvalidOperationException( "iterated over null" );
						 }
					}
			  }
		 }

		 private class StringFactory : SpatialIndexCache.Factory<string>
		 {
			  internal AtomicInteger[] Counters = new AtomicInteger[_coordinateReferenceSystems.Length];

			  internal StringFactory()
			  {
					for ( int i = 0; i < Counters.Length; i++ )
					{
						 Counters[i] = new AtomicInteger( 0 );
					}
			  }

			  public override string NewSpatial( CoordinateReferenceSystem crs )
			  {
					for ( int i = 0; i < _coordinateReferenceSystems.Length; i++ )
					{
						 if ( _coordinateReferenceSystems[i].Equals( crs ) )
						 {
							  int count = Counters[i].incrementAndGet();
							  if ( count > 1 )
							  {
									throw new System.InvalidOperationException( "called new on same crs multiple times" );
							  }
							  break;
						 }
					}
					return crs.ToString();
			  }
		 }
	}

}