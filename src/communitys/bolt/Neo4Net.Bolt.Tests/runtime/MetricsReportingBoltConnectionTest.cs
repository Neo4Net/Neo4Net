using System;

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
namespace Neo4Net.Bolt.runtime
{
	using Test = org.junit.Test;

	using BoltTestUtil = Neo4Net.Bolt.testing.BoltTestUtil;
	using PackOutput = Neo4Net.Bolt.v1.packstream.PackOutput;
	using Job = Neo4Net.Bolt.v1.runtime.Job;
	using NullLogService = Neo4Net.Logging.@internal.NullLogService;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;

	public class MetricsReportingBoltConnectionTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotifyConnectionOpened()
		 public virtual void ShouldNotifyConnectionOpened()
		 {
			  BoltConnectionMetricsMonitor metricsMonitor = mock( typeof( BoltConnectionMetricsMonitor ) );
			  BoltConnection connection = NewConnection( metricsMonitor );

			  connection.Start();

			  verify( metricsMonitor ).connectionOpened();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotifyConnectionClosed()
		 public virtual void ShouldNotifyConnectionClosed()
		 {
			  BoltConnectionMetricsMonitor metricsMonitor = mock( typeof( BoltConnectionMetricsMonitor ) );
			  BoltConnection connection = NewConnection( metricsMonitor );

			  connection.Start();
			  connection.Stop();
			  connection.ProcessNextBatch();

			  verify( metricsMonitor ).connectionClosed();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotifyConnectionClosedOnBoltConnectionAuthFatality()
		 public virtual void ShouldNotifyConnectionClosedOnBoltConnectionAuthFatality()
		 {
			  VerifyConnectionClosed(machine =>
			  {
				throw new BoltConnectionAuthFatality( "auth failure", new Exception() );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotifyConnectionClosedOnBoltProtocolBreachFatality()
		 public virtual void ShouldNotifyConnectionClosedOnBoltProtocolBreachFatality()
		 {
			  VerifyConnectionClosed(machine =>
			  {
				throw new BoltProtocolBreachFatality( "protocol failure" );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotifyConnectionClosedOnUncheckedException()
		 public virtual void ShouldNotifyConnectionClosedOnUncheckedException()
		 {
			  VerifyConnectionClosed(machine =>
			  {
				throw new Exception( "unexpected error" );
			  });
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotifyMessageReceived()
		 public virtual void ShouldNotifyMessageReceived()
		 {
			  BoltConnectionMetricsMonitor metricsMonitor = mock( typeof( BoltConnectionMetricsMonitor ) );
			  BoltConnection connection = NewConnection( metricsMonitor );

			  connection.Start();
			  connection.Enqueue(machine =>
			  {

			  });

			  verify( metricsMonitor ).messageReceived();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotifyMessageProcessingStartedAndCompleted()
		 public virtual void ShouldNotifyMessageProcessingStartedAndCompleted()
		 {
			  BoltConnectionMetricsMonitor metricsMonitor = mock( typeof( BoltConnectionMetricsMonitor ) );
			  BoltConnection connection = NewConnection( metricsMonitor );

			  connection.Start();
			  connection.Enqueue(machine =>
			  {

			  });
			  connection.ProcessNextBatch();

			  verify( metricsMonitor ).messageProcessingStarted( anyLong() );
			  verify( metricsMonitor ).messageProcessingCompleted( anyLong() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotifyConnectionActivatedAndDeactivated()
		 public virtual void ShouldNotifyConnectionActivatedAndDeactivated()
		 {
			  BoltConnectionMetricsMonitor metricsMonitor = mock( typeof( BoltConnectionMetricsMonitor ) );
			  BoltConnection connection = NewConnection( metricsMonitor );

			  connection.Start();
			  connection.Enqueue(machine =>
			  {

			  });
			  connection.ProcessNextBatch();

			  verify( metricsMonitor ).connectionActivated();
			  verify( metricsMonitor ).connectionWaiting();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotifyMessageProcessingFailed()
		 public virtual void ShouldNotifyMessageProcessingFailed()
		 {
			  BoltConnectionMetricsMonitor metricsMonitor = mock( typeof( BoltConnectionMetricsMonitor ) );
			  BoltConnection connection = NewConnection( metricsMonitor );

			  connection.Start();
			  connection.Enqueue(machine =>
			  {
				throw new BoltConnectionAuthFatality( "some error", new Exception() );
			  });
			  connection.ProcessNextBatch();

			  verify( metricsMonitor ).messageProcessingFailed();
		 }

		 private static void VerifyConnectionClosed( Job throwingJob )
		 {
			  BoltConnectionMetricsMonitor metricsMonitor = mock( typeof( BoltConnectionMetricsMonitor ) );
			  BoltConnection connection = NewConnection( metricsMonitor );

			  connection.Start();
			  connection.Enqueue( throwingJob );
			  connection.ProcessNextBatch();

			  verify( metricsMonitor ).connectionClosed();
		 }

		 private static BoltConnection NewConnection( BoltConnectionMetricsMonitor metricsMonitor )
		 {
			  BoltChannel channel = BoltTestUtil.newTestBoltChannel();
			  return new MetricsReportingBoltConnection( channel, mock( typeof( PackOutput ) ), mock( typeof( BoltStateMachine ) ), NullLogService.Instance, mock( typeof( BoltConnectionLifetimeListener ) ), mock( typeof( BoltConnectionQueueMonitor ) ), metricsMonitor, Clocks.systemClock() );
		 }
	}

}