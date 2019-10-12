using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Neo4Net.Kernel.ha
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using ClusterSettings = Neo4Net.cluster.ClusterSettings;
	using ComException = Neo4Net.com.ComException;
	using ResourceReleaser = Neo4Net.com.ResourceReleaser;
	using Neo4Net.com;
	using TransactionStream = Neo4Net.com.TransactionStream;
	using Neo4Net.com;
	using MapUtil = Neo4Net.Helpers.Collection.MapUtil;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Slave = Neo4Net.Kernel.ha.com.master.Slave;
	using SlavePriorities = Neo4Net.Kernel.ha.com.master.SlavePriorities;
	using SlavePriority = Neo4Net.Kernel.ha.com.master.SlavePriority;
	using CommitPusher = Neo4Net.Kernel.ha.transaction.CommitPusher;
	using TransactionPropagator = Neo4Net.Kernel.ha.transaction.TransactionPropagator;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using LogMatcher = Neo4Net.Logging.AssertableLogProvider.LogMatcher;
	using NullLog = Neo4Net.Logging.NullLog;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using CleanupRule = Neo4Net.Test.rule.CleanupRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.com.StoreIdTestFactory.newStoreIdForCurrentVersion;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.com.master.SlavePriorities.givenOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.com.master.SlavePriorities.roundRobin;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.scheduler.JobSchedulerFactory.createInitialisedScheduler;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.Level.ERROR;

	public class TestMasterCommittingAtSlave
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.CleanupRule cleanup = new org.neo4j.test.rule.CleanupRule();
		 public readonly CleanupRule Cleanup = new CleanupRule();
		 private const int MASTER_SERVER_ID = 0;

		 private IEnumerable<Slave> _slaves;
		 private AssertableLogProvider _logProvider = new AssertableLogProvider();
		 private AssertableLogProvider.LogMatcher _communicationLogMessage = new AssertableLogProvider.LogMatcher( any( typeof( string ) ), @is( ERROR ), containsString( "communication" ), any( typeof( object[] ) ), any( typeof( Exception ) ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void commitSuccessfullyToTheFirstOne()
		 public virtual void CommitSuccessfullyToTheFirstOne()
		 {
			  TransactionPropagator propagator = NewPropagator( 3, 1, givenOrder() );
			  propagator.Committed( 2, MASTER_SERVER_ID );
			  AssertCalls( ( FakeSlave ) _slaves.GetEnumerator().next(), 2L );
			  _logProvider.assertNone( _communicationLogMessage );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void commitACoupleOfTransactionsSuccessfully()
		 public virtual void CommitACoupleOfTransactionsSuccessfully()
		 {
			  TransactionPropagator propagator = NewPropagator( 3, 1, givenOrder() );
			  propagator.Committed( 2, MASTER_SERVER_ID );
			  propagator.Committed( 3, MASTER_SERVER_ID );
			  propagator.Committed( 4, MASTER_SERVER_ID );
			  AssertCalls( ( FakeSlave ) _slaves.GetEnumerator().next(), 2, 3, 4 );
			  _logProvider.assertNone( _communicationLogMessage );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void commitFailureAtFirstOneShouldMoveOnToNext()
		 public virtual void CommitFailureAtFirstOneShouldMoveOnToNext()
		 {
			  TransactionPropagator propagator = NewPropagator( 3, 1, givenOrder(), true );
			  propagator.Committed( 2, MASTER_SERVER_ID );
			  IEnumerator<Slave> slaveIt = _slaves.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCalls( ( FakeSlave ) slaveIt.next() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCalls( ( FakeSlave ) slaveIt.next(), 2 );
			  _logProvider.assertNone( _communicationLogMessage );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void commitSuccessfullyAtThreeSlaves()
		 public virtual void CommitSuccessfullyAtThreeSlaves()
		 {
			  TransactionPropagator propagator = NewPropagator( 5, 3, givenOrder() );
			  propagator.Committed( 2, MASTER_SERVER_ID );
			  propagator.Committed( 3, 1 );
			  propagator.Committed( 4, 2 );

			  IEnumerator<Slave> slaveIt = _slaves.GetEnumerator();

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCalls( ( FakeSlave ) slaveIt.next(), 2, 4 );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCalls( ( FakeSlave ) slaveIt.next(), 2, 3 );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCalls( ( FakeSlave ) slaveIt.next(), 2, 3, 4 );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCalls( ( FakeSlave ) slaveIt.next() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCalls( ( FakeSlave ) slaveIt.next() );
			  _logProvider.assertNone( _communicationLogMessage );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void commitSuccessfullyOnSomeOfThreeSlaves()
		 public virtual void CommitSuccessfullyOnSomeOfThreeSlaves()
		 {
			  TransactionPropagator propagator = NewPropagator( 5, 3, givenOrder(), false, true, true );
			  propagator.Committed( 2, MASTER_SERVER_ID );
			  IEnumerator<Slave> slaveIt = _slaves.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCalls( ( FakeSlave ) slaveIt.next(), 2 );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  slaveIt.next();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  slaveIt.next();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCalls( ( FakeSlave ) slaveIt.next(), 2 );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCalls( ( FakeSlave ) slaveIt.next(), 2 );
			  _logProvider.assertNone( _communicationLogMessage );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void roundRobinSingleSlave()
		 public virtual void RoundRobinSingleSlave()
		 {
			  TransactionPropagator propagator = NewPropagator( 3, 1, roundRobin() );
			  for ( long tx = 2; tx <= 6; tx++ )
			  {
					propagator.Committed( tx, MASTER_SERVER_ID );
			  }
			  IEnumerator<Slave> slaveIt = _slaves.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCalls( ( FakeSlave ) slaveIt.next(), 2, 5 );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCalls( ( FakeSlave ) slaveIt.next(), 3, 6 );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCalls( ( FakeSlave ) slaveIt.next(), 4 );
			  _logProvider.assertNone( _communicationLogMessage );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void roundRobinSomeFailing()
		 public virtual void RoundRobinSomeFailing()
		 {
			  TransactionPropagator propagator = NewPropagator( 4, 2, roundRobin(), false, true );
			  for ( long tx = 2; tx <= 6; tx++ )
			  {
					propagator.Committed( tx, MASTER_SERVER_ID );
			  }

			  /* SLAVE |    TX
			  *   0   | 2     5 6
			  * F 1   |
			  *   2   | 2 3 4   6
			  *   3   |   3 4 5
			  */

			  IEnumerator<Slave> slaveIt = _slaves.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCalls( ( FakeSlave ) slaveIt.next(), 2, 5, 6 );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  slaveIt.next();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCalls( ( FakeSlave ) slaveIt.next(), 2, 3, 4, 6 );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCalls( ( FakeSlave ) slaveIt.next(), 3, 4, 5 );
			  _logProvider.assertNone( _communicationLogMessage );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void notEnoughSlavesSuccessful()
		 public virtual void NotEnoughSlavesSuccessful()
		 {
			  TransactionPropagator propagator = NewPropagator( 3, 2, givenOrder(), true, true );
			  propagator.Committed( 2, MASTER_SERVER_ID );
			  IEnumerator<Slave> slaveIt = _slaves.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  slaveIt.next();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  slaveIt.next();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  AssertCalls( ( FakeSlave ) slaveIt.next(), 2 );
			  _logProvider.assertNone( _communicationLogMessage );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testFixedPriorityStrategy()
		 public virtual void TestFixedPriorityStrategy()
		 {
			  int[] serverIds = new int[]{ 55, 101, 66 };
			  SlavePriority @fixed = SlavePriorities.fixedDescending();
			  List<Slave> slaves = new List<Slave>( 3 );
			  slaves.Add( new FakeSlave( false, serverIds[0] ) );
			  slaves.Add( new FakeSlave( false, serverIds[1] ) );
			  slaves.Add( new FakeSlave( false, serverIds[2] ) );
			  IEnumerator<Slave> sortedSlaves = @fixed.Prioritize( slaves ).GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( serverIds[1], sortedSlaves.next().ServerId );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( serverIds[2], sortedSlaves.next().ServerId );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertEquals( serverIds[0], sortedSlaves.next().ServerId );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( !sortedSlaves.hasNext() );
		 }

		 private void AssertCalls( FakeSlave slave, params long[] txs )
		 {
			  foreach ( long tx in txs )
			  {
					long? slaveTx = slave.PopCalledTx();
					assertNotNull( slaveTx );
					assertEquals( ( long? ) tx, slaveTx );
			  }
			  assertFalse( slave.MoreTxs() );
		 }

		 private TransactionPropagator NewPropagator( int slaveCount, int replication, SlavePriority slavePriority, params bool[] failingSlaves )
		 {
			  _slaves = InstantiateSlaves( slaveCount, failingSlaves );

			  Config config = Config.defaults( MapUtil.stringMap( HaSettings.TxPushFactor.name(), "" + replication, ClusterSettings.server_id.name(), "" + MASTER_SERVER_ID ) );
			  JobScheduler scheduler = Cleanup.add( createInitialisedScheduler() );
			  TransactionPropagator result = new TransactionPropagator( TransactionPropagator.from( config, slavePriority ), NullLog.Instance, () => _slaves, new CommitPusher(scheduler) );
			  // Life
			  try
			  {
					scheduler.Start();

					result.Init();
					result.Start();
			  }
			  catch ( Exception e )
			  {
					throw new Exception( e );
			  }
			  return result;
		 }

		 private static IEnumerable<Slave> InstantiateSlaves( int count, bool[] failingSlaves )
		 {
			  IList<Slave> slaves = new List<Slave>();
			  for ( int i = 0; i < count; i++ )
			  {
					slaves.Add( new FakeSlave( i < failingSlaves.Length && failingSlaves[i], i + MASTER_SERVER_ID + 1 ) );
			  }
			  return slaves;
		 }

		 private class FakeSlave : Slave
		 {
			  internal volatile LinkedList<long> CalledWithTxId = new LinkedList<long>();
			  internal readonly bool Failing;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly int ServerIdConflict;

			  internal FakeSlave( bool failing, int serverId )
			  {
					this.Failing = failing;
					this.ServerIdConflict = serverId;
			  }

			  public override Response<Void> PullUpdates( long txId )
			  {
					if ( Failing )
					{
						 throw new ComException( "Told to fail" );
					}

					CalledWithTxId.AddLast( txId );
					return new TransactionStreamResponse<Void>( null, newStoreIdForCurrentVersion(), Neo4Net.com.TransactionStream_Fields.Empty, Neo4Net.com.ResourceReleaser_Fields.NoOp );
			  }

			  internal virtual long? PopCalledTx()
			  {
					return CalledWithTxId.RemoveFirst();
			  }

			  internal virtual bool MoreTxs()
			  {
					return CalledWithTxId.Count > 0;
			  }

			  public virtual int ServerId
			  {
				  get
				  {
						return ServerIdConflict;
				  }
			  }

			  public override string ToString()
			  {
					return "FakeSlave[" + ServerIdConflict + "]";
			  }
		 }
	}

}