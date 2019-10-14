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
namespace Synchronization
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Label = Neo4Net.Graphdb.Label;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseBuilder = Neo4Net.Graphdb.factory.GraphDatabaseBuilder;
	using GraphDatabaseFactory = Neo4Net.Graphdb.factory.GraphDatabaseFactory;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using LogRotation = Neo4Net.Kernel.impl.transaction.log.rotation.LogRotation;
	using GraphDatabaseAPI = Neo4Net.Kernel.@internal.GraphDatabaseAPI;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;
	using Neo4Net.Test.OtherThreadExecutor;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;
	using Neo4Net.Test.rule.concurrent;

	public class TestStartTransactionDuringLogRotation
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.EmbeddedDatabaseRule()
		 public DatabaseRule db = new EmbeddedDatabaseRuleAnonymousInnerClass();

		 private class EmbeddedDatabaseRuleAnonymousInnerClass : EmbeddedDatabaseRule
		 {
			 protected internal override GraphDatabaseBuilder newBuilder( GraphDatabaseFactory factory )
			 {
				  return base.newBuilder( factory ).setConfig( GraphDatabaseSettings.logical_log_rotation_threshold, "1M" );
			 }
		 }
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.concurrent.OtherThreadRule<Void> t2 = new org.neo4j.test.rule.concurrent.OtherThreadRule<>("T2-" + getClass().getName());
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		 public readonly OtherThreadRule<Void> T2 = new OtherThreadRule<Void>( "T2-" + this.GetType().FullName );

		 private ExecutorService _executor;
		 private System.Threading.CountdownEvent _startLogRotationLatch;
		 private System.Threading.CountdownEvent _completeLogRotationLatch;
		 private AtomicBoolean _writerStopped;
		 private Monitors _monitors;
		 private Neo4Net.Kernel.impl.transaction.log.rotation.LogRotation_Monitor _rotationListener;
		 private Label _label;
		 private Future<Void> _rotationFuture;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws InterruptedException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  _executor = Executors.newCachedThreadPool();
			  _startLogRotationLatch = new System.Threading.CountdownEvent( 1 );
			  _completeLogRotationLatch = new System.Threading.CountdownEvent( 1 );
			  _writerStopped = new AtomicBoolean();
			  _monitors = Db.DependencyResolver.resolveDependency( typeof( Monitors ) );

			  _rotationListener = new LogRotation_MonitorAnonymousInnerClass( this );

			  _monitors.addMonitorListener( _rotationListener );
			  _label = Label.label( "Label" );

			  _rotationFuture = T2.execute( ForceLogRotation( db ) );

			  // Waiting for the writer task to start a log rotation
			  _startLogRotationLatch.await();

			  // Then we should be able to start a transaction, though perhaps not be able to finish it.
			  // This is what the individual test methods will be doing.
			  // The test passes when transaction.close completes within the test timeout, that is, it didn't deadlock.
		 }

		 private class LogRotation_MonitorAnonymousInnerClass : Neo4Net.Kernel.impl.transaction.log.rotation.LogRotation_Monitor
		 {
			 private readonly TestStartTransactionDuringLogRotation _outerInstance;

			 public LogRotation_MonitorAnonymousInnerClass( TestStartTransactionDuringLogRotation outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public void startedRotating( long currentVersion )
			 {
				  _outerInstance.startLogRotationLatch.Signal();
				  try
				  {
						_outerInstance.completeLogRotationLatch.await();
				  }
				  catch ( InterruptedException e )
				  {
						throw new Exception( e );
				  }
			 }

			 public void finishedRotating( long currentVersion )
			 {
			 }
		 }

		 private WorkerCommand<Void, Void> ForceLogRotation( GraphDatabaseAPI db )
		 {
			  return state =>
			  {
				using ( Transaction tx = Db.beginTx() )
				{
					 Db.createNode( _label ).setProperty( "a", 1 );
					 tx.success();
				}

				Db.DependencyResolver.resolveDependency( typeof( LogRotation ) ).rotateLogFile();
				return null;
			  };
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TearDown()
		 {
			  _rotationFuture.get();
			  _writerStopped.set( true );
			  _executor.shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 10000) public void logRotationMustNotObstructStartingReadTransaction()
		 public virtual void LogRotationMustNotObstructStartingReadTransaction()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.getNodeById( 0 );
					tx.Success();
					_completeLogRotationLatch.Signal();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test(timeout = 10000) public void logRotationMustNotObstructStartingWriteTransaction()
		 public virtual void LogRotationMustNotObstructStartingWriteTransaction()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode();
					tx.Success();
					_completeLogRotationLatch.Signal();
			  }
		 }
	}

}