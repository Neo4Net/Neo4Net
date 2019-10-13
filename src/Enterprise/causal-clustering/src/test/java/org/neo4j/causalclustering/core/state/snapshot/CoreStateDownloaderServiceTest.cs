using System;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.core.state.snapshot
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using CatchupAddressProvider = Neo4Net.causalclustering.catchup.CatchupAddressProvider;
	using Predicates = Neo4Net.Functions.Predicates;
	using AdvertisedSocketAddress = Neo4Net.Helpers.AdvertisedSocketAddress;
	using CountingJobScheduler = Neo4Net.Kernel.impl.util.CountingJobScheduler;
	using DatabaseHealth = Neo4Net.Kernel.@internal.DatabaseHealth;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;
	using NullLogProvider = Neo4Net.Logging.NullLogProvider;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.@internal.verification.VerificationModeFactory.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.core.state.snapshot.PersistentSnapshotDownloader.OPERATION_NAME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createInitialisedScheduler;

	public class CoreStateDownloaderServiceTest
	{
		private bool InstanceFieldsInitialized = false;

		public CoreStateDownloaderServiceTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_catchupAddressProvider = CatchupAddressProvider.fromSingleAddress( _someMemberAddress );
		}

		 private readonly AdvertisedSocketAddress _someMemberAddress = new AdvertisedSocketAddress( "localhost", 1234 );
		 private CatchupAddressProvider _catchupAddressProvider;
		 private JobScheduler _centralJobScheduler;
		 private DatabaseHealth _dbHealth = mock( typeof( DatabaseHealth ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void create()
		 public virtual void Create()
		 {
			  _centralJobScheduler = createInitialisedScheduler();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void shutdown() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Shutdown()
		 {
			  _centralJobScheduler.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRunPersistentDownloader() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRunPersistentDownloader()
		 {
			  CoreStateDownloader coreStateDownloader = mock( typeof( CoreStateDownloader ) );
			  when( coreStateDownloader.DownloadSnapshot( any() ) ).thenReturn(true);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.core.state.CommandApplicationProcess applicationProcess = mock(org.neo4j.causalclustering.core.state.CommandApplicationProcess.class);
			  CommandApplicationProcess applicationProcess = mock( typeof( CommandApplicationProcess ) );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.logging.Log log = mock(org.neo4j.logging.Log.class);
			  Log log = mock( typeof( Log ) );
			  CoreStateDownloaderService coreStateDownloaderService = new CoreStateDownloaderService( _centralJobScheduler, coreStateDownloader, applicationProcess, LogProvider( log ), new NoTimeout(), () => _dbHealth, new Monitors() );
			  coreStateDownloaderService.ScheduleDownload( _catchupAddressProvider );
			  WaitForApplierToResume( applicationProcess );

			  verify( applicationProcess, times( 1 ) ).pauseApplier( OPERATION_NAME );
			  verify( applicationProcess, times( 1 ) ).resumeApplier( OPERATION_NAME );
			  verify( coreStateDownloader, times( 1 ) ).downloadSnapshot( any() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyScheduleOnePersistentDownloaderTaskAtTheTime() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOnlyScheduleOnePersistentDownloaderTaskAtTheTime()
		 {
			  AtomicInteger schedules = new AtomicInteger();
			  CountingJobScheduler countingJobScheduler = new CountingJobScheduler( schedules, _centralJobScheduler );
			  Semaphore blockDownloader = new Semaphore( 0 );
			  CoreStateDownloader coreStateDownloader = new BlockingCoreStateDownloader( blockDownloader );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.core.state.CommandApplicationProcess applicationProcess = mock(org.neo4j.causalclustering.core.state.CommandApplicationProcess.class);
			  CommandApplicationProcess applicationProcess = mock( typeof( CommandApplicationProcess ) );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.logging.Log log = mock(org.neo4j.logging.Log.class);
			  Log log = mock( typeof( Log ) );
			  CoreStateDownloaderService coreStateDownloaderService = new CoreStateDownloaderService( countingJobScheduler, coreStateDownloader, applicationProcess, LogProvider( log ), new NoTimeout(), () => _dbHealth, new Monitors() );

			  coreStateDownloaderService.ScheduleDownload( _catchupAddressProvider );
			  Thread.Sleep( 50 );
			  coreStateDownloaderService.ScheduleDownload( _catchupAddressProvider );
			  coreStateDownloaderService.ScheduleDownload( _catchupAddressProvider );
			  coreStateDownloaderService.ScheduleDownload( _catchupAddressProvider );

			  assertEquals( 1, schedules.get() );
			  blockDownloader.release();
		 }

		 internal class BlockingCoreStateDownloader : CoreStateDownloader
		 {
			  internal readonly Semaphore Semaphore;

			  internal BlockingCoreStateDownloader( Semaphore semaphore ) : base( null, null, null, null, NullLogProvider.Instance, null, null, null, null )
			  {
					this.Semaphore = semaphore;
			  }

			  internal override bool DownloadSnapshot( CatchupAddressProvider addressProvider )
			  {
					Semaphore.acquireUninterruptibly();
					return true;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void waitForApplierToResume(org.neo4j.causalclustering.core.state.CommandApplicationProcess applicationProcess) throws java.util.concurrent.TimeoutException
		 private void WaitForApplierToResume( CommandApplicationProcess applicationProcess )
		 {
			  Predicates.await(() =>
			  {
				try
				{
					 verify( applicationProcess, times( 1 ) ).resumeApplier( OPERATION_NAME );
					 return true;
				}
				catch ( Exception )
				{
					 return false;
				}
			  }, 20, TimeUnit.SECONDS);
		 }

		 private LogProvider LogProvider( Log log )
		 {
			  return new LogProviderAnonymousInnerClass( this, log );
		 }

		 private class LogProviderAnonymousInnerClass : LogProvider
		 {
			 private readonly CoreStateDownloaderServiceTest _outerInstance;

			 private Log _log;

			 public LogProviderAnonymousInnerClass( CoreStateDownloaderServiceTest outerInstance, Log log )
			 {
				 this.outerInstance = outerInstance;
				 this._log = log;
			 }

			 public Log getLog( Type loggingClass )
			 {
				  return _log;
			 }

			 public Log getLog( string name )
			 {
				  return _log;
			 }
		 }
	}

}