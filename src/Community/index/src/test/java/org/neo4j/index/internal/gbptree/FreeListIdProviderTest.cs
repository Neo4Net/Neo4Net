using System;
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
namespace Neo4Net.Index.@internal.gbptree
{
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using Monitor = Neo4Net.Index.@internal.gbptree.FreeListIdProvider.Monitor;
	using PageCursor = Neo4Net.Io.pagecache.PageCursor;
	using PagedFile = Neo4Net.Io.pagecache.PagedFile;
	using Inject = Neo4Net.Test.extension.Inject;
	using RandomExtension = Neo4Net.Test.extension.RandomExtension;
	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyInt;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.FreeListIdProvider.NO_MONITOR;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(RandomExtension.class) class FreeListIdProviderTest
	internal class FreeListIdProviderTest
	{
		private bool InstanceFieldsInitialized = false;

		public FreeListIdProviderTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_freelist = new FreeListIdProvider( _pagedFile, PAGE_SIZE, BASE_ID, _monitor );
		}

		 private const int PAGE_SIZE = 128;
		 private const long GENERATION_ONE = GenerationSafePointer.MIN_GENERATION;
		 private static readonly long _generationTwo = GENERATION_ONE + 1;
		 private static readonly long _generationThree = _generationTwo + 1;
		 private static readonly long _generationFour = _generationThree + 1;
		 private const long BASE_ID = 5;

		 private PageAwareByteArrayCursor _cursor;
		 private readonly PagedFile _pagedFile = mock( typeof( PagedFile ) );
		 private readonly FreelistPageMonitor _monitor = new FreelistPageMonitor();
		 private FreeListIdProvider _freelist;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.RandomRule random;
		 private RandomRule _random;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUpPagedFile() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SetUpPagedFile()
		 {
			  _cursor = new PageAwareByteArrayCursor( PAGE_SIZE );
			  when( _pagedFile.io( anyLong(), anyInt() ) ).thenAnswer(invocation => _cursor.duplicate(invocation.getArgument(0)));
			  _freelist.initialize( BASE_ID + 1, BASE_ID + 1, BASE_ID + 1, 0, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReleaseAndAcquireId() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReleaseAndAcquireId()
		 {
			  // GIVEN
			  long releasedId = 11;
			  FillPageWithRandomBytes( releasedId );

			  // WHEN
			  _freelist.releaseId( GENERATION_ONE, _generationTwo, releasedId );
			  long acquiredId = _freelist.acquireNewId( _generationTwo, _generationThree );

			  // THEN
			  assertEquals( releasedId, acquiredId );
			  _cursor.next( acquiredId );
			  AssertEmpty( _cursor );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReleaseAndAcquireIdsFromMultiplePages() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReleaseAndAcquireIdsFromMultiplePages()
		 {
			  // GIVEN
			  int entries = _freelist.entriesPerPage() + _freelist.entriesPerPage() / 2;
			  long baseId = 101;
			  for ( int i = 0; i < entries; i++ )
			  {
					_freelist.releaseId( GENERATION_ONE, _generationTwo, baseId + i );
			  }

			  // WHEN/THEN
			  for ( int i = 0; i < entries; i++ )
			  {
					long acquiredId = _freelist.acquireNewId( _generationTwo, _generationThree );
					assertEquals( baseId + i, acquiredId );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldPutFreedFreeListPagesIntoFreeListAsWell() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldPutFreedFreeListPagesIntoFreeListAsWell()
		 {
			  // GIVEN
			  long prevId;
			  long acquiredId = BASE_ID + 1;
			  long freelistPageId = BASE_ID + 1;
			  MutableLongSet released = new LongHashSet();
			  do
			  {
					prevId = acquiredId;
					acquiredId = _freelist.acquireNewId( GENERATION_ONE, _generationTwo );
					_freelist.releaseId( GENERATION_ONE, _generationTwo, acquiredId );
					released.add( acquiredId );
			  } while ( acquiredId - prevId == 1 );

			  // WHEN
			  while ( !released.Empty )
			  {
					long reAcquiredId = _freelist.acquireNewId( _generationTwo, _generationThree );
					released.remove( reAcquiredId );
			  }

			  // THEN
			  assertEquals( freelistPageId, _freelist.acquireNewId( _generationThree, _generationFour ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldStayBoundUnderStress() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldStayBoundUnderStress()
		 {
			  // GIVEN
			  MutableLongSet acquired = new LongHashSet();
			  IList<long> acquiredList = new List<long>(); // for quickly finding random to remove
			  long stableGeneration = GenerationSafePointer.MIN_GENERATION;
			  long unstableGeneration = stableGeneration + 1;
			  int iterations = 100;

			  // WHEN
			  for ( int i = 0; i < iterations; i++ )
			  {
					for ( int j = 0; j < 10; j++ )
					{
						 if ( _random.nextBoolean() )
						 {
							  // acquire
							  int count = _random.intBetween( 5, 10 );
							  for ( int k = 0; k < count; k++ )
							  {
									long acquiredId = _freelist.acquireNewId( stableGeneration, unstableGeneration );
									assertTrue( acquired.add( acquiredId ) );
									acquiredList.Add( acquiredId );
							  }
						 }
						 else
						 {
							  // release
							  int count = _random.intBetween( 5, 20 );
							  for ( int k = 0; k < count && !acquired.Empty; k++ )
							  {
									long id = acquiredList.RemoveAt( _random.Next( acquiredList.Count ) );
									assertTrue( acquired.remove( id ) );
									_freelist.releaseId( stableGeneration, unstableGeneration, id );
							  }
						 }
					}

					foreach ( long id in acquiredList )
					{
						 _freelist.releaseId( stableGeneration, unstableGeneration, id );
					}
					acquiredList.Clear();
					acquired.clear();

					// checkpoint, sort of
					stableGeneration = unstableGeneration;
					unstableGeneration++;
			  }

			  // THEN
			  assertTrue( _freelist.lastId() < 200, _freelist.lastId().ToString() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldVisitUnacquiredIds() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldVisitUnacquiredIds()
		 {
			  // GIVEN a couple of released ids
			  MutableLongSet expected = new LongHashSet();
			  for ( int i = 0; i < 100; i++ )
			  {
					expected.add( _freelist.acquireNewId( GENERATION_ONE, _generationTwo ) );
			  }
			  expected.forEach(id =>
			  {
				try
				{
					 _freelist.releaseId( GENERATION_ONE, _generationTwo, id );
				}
				catch ( IOException e )
				{
					 throw new Exception( e );
				}
			  });
			  // and only a few acquired
			  for ( int i = 0; i < 10; i++ )
			  {
					long acquiredId = _freelist.acquireNewId( _generationTwo, _generationThree );
					assertTrue( expected.remove( acquiredId ) );
			  }

			  // WHEN/THEN
			  _freelist.visitFreelist( new IdProvider_IdProviderVisitor_AdaptorAnonymousInnerClass( this, expected ) );
			  assertTrue( expected.Empty );
		 }

		 private class IdProvider_IdProviderVisitor_AdaptorAnonymousInnerClass : IdProvider_IdProviderVisitor_Adaptor
		 {
			 private readonly FreeListIdProviderTest _outerInstance;

			 private MutableLongSet _expected;

			 public IdProvider_IdProviderVisitor_AdaptorAnonymousInnerClass( FreeListIdProviderTest outerInstance, MutableLongSet expected )
			 {
				 this.outerInstance = outerInstance;
				 this._expected = expected;
			 }

			 public override void freelistEntry( long pageId, long generation, int pos )
			 {
				  assertTrue( _expected.remove( pageId ) );
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldVisitFreelistPageIds() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldVisitFreelistPageIds()
		 {
			  // GIVEN a couple of released ids
			  MutableLongSet expected = new LongHashSet();
			  // Add the already allocated free-list page id
			  expected.add( BASE_ID + 1 );
			  _monitor.set( new MonitorAnonymousInnerClass( this, expected ) );
			  for ( int i = 0; i < 100; i++ )
			  {
					long id = _freelist.acquireNewId( GENERATION_ONE, _generationTwo );
					_freelist.releaseId( GENERATION_ONE, _generationTwo, id );
			  }
			  assertTrue( expected.size() > 0 );

			  // WHEN/THEN
			  _freelist.visitFreelist( new IdProvider_IdProviderVisitor_AdaptorAnonymousInnerClass2( this, expected ) );
			  assertTrue( expected.Empty );
		 }

		 private class MonitorAnonymousInnerClass : Monitor
		 {
			 private readonly FreeListIdProviderTest _outerInstance;

			 private MutableLongSet _expected;

			 public MonitorAnonymousInnerClass( FreeListIdProviderTest outerInstance, MutableLongSet expected )
			 {
				 this.outerInstance = outerInstance;
				 this._expected = expected;
			 }

			 public void acquiredFreelistPageId( long freelistPageId )
			 {
				  _expected.add( freelistPageId );
			 }
		 }

		 private class IdProvider_IdProviderVisitor_AdaptorAnonymousInnerClass2 : IdProvider_IdProviderVisitor_Adaptor
		 {
			 private readonly FreeListIdProviderTest _outerInstance;

			 private MutableLongSet _expected;

			 public IdProvider_IdProviderVisitor_AdaptorAnonymousInnerClass2( FreeListIdProviderTest outerInstance, MutableLongSet expected )
			 {
				 this.outerInstance = outerInstance;
				 this._expected = expected;
			 }

			 public override void beginFreelistPage( long pageId )
			 {
				  assertTrue( _expected.remove( pageId ) );
			 }
		 }

		 private void FillPageWithRandomBytes( long releasedId )
		 {
			  _cursor.next( releasedId );
			  sbyte[] crapData = new sbyte[PAGE_SIZE];
			  ThreadLocalRandom.current().NextBytes(crapData);
			  _cursor.putBytes( crapData );
		 }

		 private static void AssertEmpty( PageCursor cursor )
		 {
			  sbyte[] data = new sbyte[PAGE_SIZE];
			  cursor.GetBytes( data );
			  foreach ( sbyte b in data )
			  {
					assertEquals( 0, b );
			  }
		 }

		 private class FreelistPageMonitor : Monitor
		 {
			  internal Monitor Actual = NO_MONITOR;

			  internal virtual void Set( Monitor actual )
			  {
					this.Actual = actual;
			  }

			  public override void AcquiredFreelistPageId( long freelistPageId )
			  {
					Actual.acquiredFreelistPageId( freelistPageId );
			  }

			  public override void ReleasedFreelistPageId( long freelistPageId )
			  {
					Actual.releasedFreelistPageId( freelistPageId );
			  }
		 }
	}

}