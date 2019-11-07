using System.Threading;

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
namespace Neo4Net.causalclustering.core.state.snapshot
{
	using Test = org.junit.Test;


	using CatchupAddressProvider = Neo4Net.causalclustering.catchup.CatchupAddressProvider;
	using Predicates = Neo4Net.Functions.Predicates;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using DatabaseHealth = Neo4Net.Kernel.Internal.DatabaseHealth;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Log = Neo4Net.Logging.Log;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Internal.verification.VerificationModeFactory.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.core.state.snapshot.PersistentSnapshotDownloader.OPERATION_NAME;

	public class PersistentSnapshotDownloaderTest
	{
		private bool InstanceFieldsInitialized = false;

		public PersistentSnapshotDownloaderTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_catchupAddressProvider = CatchupAddressProvider.fromSingleAddress( _fromAddress );
		}

		 private readonly AdvertisedSocketAddress _fromAddress = new AdvertisedSocketAddress( "localhost", 1234 );
		 private CatchupAddressProvider _catchupAddressProvider;
		 private readonly DatabaseHealth _dbHealth = mock( typeof( DatabaseHealth ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPauseAndResumeApplicationProcessIfDownloadIsSuccessful() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPauseAndResumeApplicationProcessIfDownloadIsSuccessful()
		 {
			  // given
			  CoreStateDownloader coreStateDownloader = mock( typeof( CoreStateDownloader ) );
			  when( coreStateDownloader.DownloadSnapshot( any() ) ).thenReturn(true);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.causalclustering.core.state.CommandApplicationProcess applicationProcess = mock(Neo4Net.causalclustering.core.state.CommandApplicationProcess.class);
			  CommandApplicationProcess applicationProcess = mock( typeof( CommandApplicationProcess ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.logging.Log log = mock(Neo4Net.logging.Log.class);
			  Log log = mock( typeof( Log ) );
			  PersistentSnapshotDownloader persistentSnapshotDownloader = new PersistentSnapshotDownloader( _catchupAddressProvider, applicationProcess, coreStateDownloader, log, new NoTimeout(), () => _dbHealth, new Monitors() );

			  // when
			  persistentSnapshotDownloader.Run();

			  // then
			  verify( applicationProcess, times( 1 ) ).pauseApplier( OPERATION_NAME );
			  verify( applicationProcess, times( 1 ) ).resumeApplier( OPERATION_NAME );
			  verify( coreStateDownloader, times( 1 ) ).downloadSnapshot( any() );
			  assertTrue( persistentSnapshotDownloader.HasCompleted() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldResumeCommandApplicationProcessIfInterrupted() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldResumeCommandApplicationProcessIfInterrupted()
		 {
			  // given
			  CoreStateDownloader coreStateDownloader = mock( typeof( CoreStateDownloader ) );
			  when( coreStateDownloader.DownloadSnapshot( any() ) ).thenReturn(false);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.causalclustering.core.state.CommandApplicationProcess applicationProcess = mock(Neo4Net.causalclustering.core.state.CommandApplicationProcess.class);
			  CommandApplicationProcess applicationProcess = mock( typeof( CommandApplicationProcess ) );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.logging.Log log = mock(Neo4Net.logging.Log.class);
			  Log log = mock( typeof( Log ) );
			  NoTimeout timeout = new NoTimeout();
			  PersistentSnapshotDownloader persistentSnapshotDownloader = new PersistentSnapshotDownloader( _catchupAddressProvider, applicationProcess, coreStateDownloader, log, timeout, () => _dbHealth, new Monitors() );

			  // when
			  Thread thread = new Thread( persistentSnapshotDownloader );
			  thread.Start();
			  AwaitOneIteration( timeout );
			  thread.Interrupt();
			  thread.Join();

			  // then
			  verify( applicationProcess, times( 1 ) ).pauseApplier( OPERATION_NAME );
			  verify( applicationProcess, times( 1 ) ).resumeApplier( OPERATION_NAME );
			  assertTrue( persistentSnapshotDownloader.HasCompleted() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldResumeCommandApplicationProcessIfDownloaderIsStopped() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldResumeCommandApplicationProcessIfDownloaderIsStopped()
		 {
			  // given
			  CoreStateDownloader coreStateDownloader = mock( typeof( CoreStateDownloader ) );
			  when( coreStateDownloader.DownloadSnapshot( any() ) ).thenReturn(false);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.causalclustering.core.state.CommandApplicationProcess applicationProcess = mock(Neo4Net.causalclustering.core.state.CommandApplicationProcess.class);
			  CommandApplicationProcess applicationProcess = mock( typeof( CommandApplicationProcess ) );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.logging.Log log = mock(Neo4Net.logging.Log.class);
			  Log log = mock( typeof( Log ) );
			  NoTimeout timeout = new NoTimeout();
			  PersistentSnapshotDownloader persistentSnapshotDownloader = new PersistentSnapshotDownloader( null, applicationProcess, coreStateDownloader, log, timeout, () => _dbHealth, new Monitors() );

			  // when
			  Thread thread = new Thread( persistentSnapshotDownloader );
			  thread.Start();
			  AwaitOneIteration( timeout );
			  persistentSnapshotDownloader.Stop();
			  thread.Join();

			  // then
			  verify( applicationProcess, times( 1 ) ).pauseApplier( OPERATION_NAME );
			  verify( applicationProcess, times( 1 ) ).resumeApplier( OPERATION_NAME );
			  assertTrue( persistentSnapshotDownloader.HasCompleted() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldEventuallySucceed()
		 public virtual void ShouldEventuallySucceed()
		 {
			  // given
			  CoreStateDownloader coreStateDownloader = new EventuallySuccessfulDownloader( this, 3 );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.causalclustering.core.state.CommandApplicationProcess applicationProcess = mock(Neo4Net.causalclustering.core.state.CommandApplicationProcess.class);
			  CommandApplicationProcess applicationProcess = mock( typeof( CommandApplicationProcess ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.logging.Log log = mock(Neo4Net.logging.Log.class);
			  Log log = mock( typeof( Log ) );
			  NoTimeout timeout = new NoTimeout();
			  PersistentSnapshotDownloader persistentSnapshotDownloader = new PersistentSnapshotDownloader( _catchupAddressProvider, applicationProcess, coreStateDownloader, log, timeout, () => _dbHealth, new Monitors() );

			  // when
			  persistentSnapshotDownloader.Run();

			  // then
			  verify( applicationProcess, times( 1 ) ).pauseApplier( OPERATION_NAME );
			  verify( applicationProcess, times( 1 ) ).resumeApplier( OPERATION_NAME );
			  assertEquals( 3, timeout.CurrentCount() );
			  assertTrue( persistentSnapshotDownloader.HasCompleted() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotStartDownloadIfAlreadyCompleted() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotStartDownloadIfAlreadyCompleted()
		 {
			  // given
			  CoreStateDownloader coreStateDownloader = mock( typeof( CoreStateDownloader ) );
			  when( coreStateDownloader.DownloadSnapshot( any() ) ).thenReturn(true);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.causalclustering.core.state.CommandApplicationProcess applicationProcess = mock(Neo4Net.causalclustering.core.state.CommandApplicationProcess.class);
			  CommandApplicationProcess applicationProcess = mock( typeof( CommandApplicationProcess ) );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.logging.Log log = mock(Neo4Net.logging.Log.class);
			  Log log = mock( typeof( Log ) );
			  PersistentSnapshotDownloader persistentSnapshotDownloader = new PersistentSnapshotDownloader( _catchupAddressProvider, applicationProcess, coreStateDownloader, log, new NoTimeout(), () => _dbHealth, new Monitors() );

			  // when
			  persistentSnapshotDownloader.Run();
			  persistentSnapshotDownloader.Run();

			  // then
			  verify( coreStateDownloader, times( 1 ) ).downloadSnapshot( _catchupAddressProvider );
			  verify( applicationProcess, times( 1 ) ).pauseApplier( OPERATION_NAME );
			  verify( applicationProcess, times( 1 ) ).resumeApplier( OPERATION_NAME );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotStartIfCurrentlyRunning() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotStartIfCurrentlyRunning()
		 {
			  // given
			  CoreStateDownloader coreStateDownloader = mock( typeof( CoreStateDownloader ) );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.causalclustering.core.state.CommandApplicationProcess applicationProcess = mock(Neo4Net.causalclustering.core.state.CommandApplicationProcess.class);
			  CommandApplicationProcess applicationProcess = mock( typeof( CommandApplicationProcess ) );
			  when( coreStateDownloader.DownloadSnapshot( any() ) ).thenReturn(false);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.logging.Log log = mock(Neo4Net.logging.Log.class);
			  Log log = mock( typeof( Log ) );
			  NoTimeout timeout = new NoTimeout();
			  PersistentSnapshotDownloader persistentSnapshotDownloader = new PersistentSnapshotDownloader( _catchupAddressProvider, applicationProcess, coreStateDownloader, log, timeout, () => _dbHealth, new Monitors() );

			  Thread thread = new Thread( persistentSnapshotDownloader );

			  // when
			  thread.Start();
			  AwaitOneIteration( timeout );
			  persistentSnapshotDownloader.Run();
			  persistentSnapshotDownloader.Stop();
			  thread.Join();

			  // then
			  verify( applicationProcess, times( 1 ) ).pauseApplier( OPERATION_NAME );
			  verify( applicationProcess, times( 1 ) ).resumeApplier( OPERATION_NAME );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void awaitOneIteration(NoTimeout timeout) throws java.util.concurrent.TimeoutException
		 private void AwaitOneIteration( NoTimeout timeout )
		 {
			  Predicates.await( () => timeout.CurrentCount() > 0, 2, TimeUnit.SECONDS );
		 }

		 private class EventuallySuccessfulDownloader : CoreStateDownloader
		 {
			 private readonly PersistentSnapshotDownloaderTest _outerInstance;

			  internal int After;

			  internal EventuallySuccessfulDownloader( PersistentSnapshotDownloaderTest outerInstance, int after ) : base( null, null, null, null, NullLogProvider.Instance, null, null, null, null )
			  {
				  this._outerInstance = outerInstance;
					this.After = after;
			  }

			  internal override bool DownloadSnapshot( CatchupAddressProvider addressProvider )
			  {
					return After-- <= 0;
			  }
		 }
	}

}