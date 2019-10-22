/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.core.consensus.log.segmented
{
	using Matchers = org.hamcrest.Matchers;
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;

	using EphemeralFileSystemAbstraction = Neo4Net.GraphDb.mockfs.EphemeralFileSystemAbstraction;
	using Clocks = Neo4Net.Time.Clocks;
	using FakeClock = Neo4Net.Time.FakeClock;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.never;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.spy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.logging.NullLogProvider.getInstance;

	public class ReaderPoolTest
	{
		private bool InstanceFieldsInitialized = false;

		public ReaderPoolTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_fileNames = new FileNames( @base );
			_pool = new ReaderPool( 2, Instance, _fileNames, _fsa, _clock );
		}

		 private readonly File @base = new File( "base" );
		 private FileNames _fileNames;
		 private readonly EphemeralFileSystemAbstraction _fsa = spy( new EphemeralFileSystemAbstraction() );
		 private readonly FakeClock _clock = Clocks.fakeClock();

		 private ReaderPool _pool;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void before()
		 public virtual void Before()
		 {
			  _fsa.mkdirs( @base );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  _fsa.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReacquireReaderFromPool() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReacquireReaderFromPool()
		 {
			  // given
			  Reader reader = _pool.acquire( 0, 0 );
			  _pool.release( reader );

			  // when
			  Reader newReader = _pool.acquire( 0, 0 );

			  // then
			  verify( _fsa, times( 1 ) ).open( any(), any() );
			  assertThat( reader, @is( newReader ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPruneOldReaders() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPruneOldReaders()
		 {
			  // given
			  Reader readerA = spy( _pool.acquire( 0, 0 ) );
			  Reader readerB = spy( _pool.acquire( 0, 0 ) );

			  _pool.release( readerA );

			  _clock.forward( 2, MINUTES );
			  _pool.release( readerB );

			  // when
			  _clock.forward( 1, MINUTES );
			  _pool.prune( 2, MINUTES );

			  // then
			  verify( readerA ).close();
			  verify( readerB, never() ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReturnPrunedReaders() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotReturnPrunedReaders()
		 {
			  Reader readerA = _pool.acquire( 0, 0 );
			  Reader readerB = _pool.acquire( 0, 0 );

			  _pool.release( readerA );
			  _pool.release( readerB );

			  _clock.forward( 2, MINUTES );
			  _pool.prune( 1, MINUTES );

			  // when
			  Reader readerC = _pool.acquire( 0, 0 );
			  Reader readerD = _pool.acquire( 0, 0 );

			  // then
			  assertThat( asSet( readerC, readerD ), not( Matchers.containsInAnyOrder( readerA, readerB ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDisposeSuperfluousReaders() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDisposeSuperfluousReaders()
		 {
			  // given
			  Reader readerA = spy( _pool.acquire( 0, 0 ) );
			  Reader readerB = spy( _pool.acquire( 0, 0 ) );
			  Reader readerC = spy( _pool.acquire( 0, 0 ) );
			  Reader readerD = spy( _pool.acquire( 0, 0 ) );

			  _pool.release( readerA );
			  _pool.release( readerB );

			  // when
			  _pool.release( readerC );
			  _pool.release( readerD );

			  // then
			  verify( readerA ).close();
			  verify( readerB ).close();
			  verify( readerC, never() ).close();
			  verify( readerD, never() ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDisposeAllReleasedReaders() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDisposeAllReleasedReaders()
		 {
			  // given
			  Reader readerA = spy( _pool.acquire( 0, 0 ) );
			  Reader readerB = spy( _pool.acquire( 0, 0 ) );
			  Reader readerC = spy( _pool.acquire( 0, 0 ) );

			  _pool.release( readerA );
			  _pool.release( readerB );
			  _pool.release( readerC );

			  // when
			  _pool.close();

			  // then
			  verify( readerA ).close();
			  verify( readerB ).close();
			  verify( readerC ).close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPruneReadersOfVersion() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPruneReadersOfVersion()
		 {
			  // given
			  _pool = new ReaderPool( 8, Instance, _fileNames, _fsa, _clock );

			  Reader readerA = spy( _pool.acquire( 0, 0 ) );
			  Reader readerB = spy( _pool.acquire( 1, 0 ) );
			  Reader readerC = spy( _pool.acquire( 1, 0 ) );
			  Reader readerD = spy( _pool.acquire( 2, 0 ) );

			  _pool.release( readerA );
			  _pool.release( readerB );
			  _pool.release( readerC );
			  _pool.release( readerD );

			  // when
			  _pool.prune( 1 );

			  // then
			  verify( readerA, never() ).close();
			  verify( readerB ).close();
			  verify( readerC ).close();
			  verify( readerD, never() ).close();

			  // when
			  _pool.prune( 0 );
			  // then
			  verify( readerA ).close();
			  verify( readerD, never() ).close();

			  // when
			  _pool.prune( 2 );
			  // then
			  verify( readerD ).close();
		 }
	}

}