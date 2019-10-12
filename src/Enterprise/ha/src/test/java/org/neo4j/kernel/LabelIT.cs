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
namespace Neo4Net.Kernel
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Label = Neo4Net.Graphdb.Label;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using TransactionFailureException = Neo4Net.@internal.Kernel.Api.exceptions.TransactionFailureException;
	using HighlyAvailableGraphDatabase = Neo4Net.Kernel.ha.HighlyAvailableGraphDatabase;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using ClusterManager = Neo4Net.Kernel.impl.ha.ClusterManager;
	using ClusterRule = Neo4Net.Test.ha.ClusterRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;

	public class LabelIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.ha.ClusterRule clusterRule = new org.neo4j.test.ha.ClusterRule();
		 public readonly ClusterRule ClusterRule = new ClusterRule();

		 protected internal ClusterManager.ManagedCluster Cluster;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  Cluster = ClusterRule.startCluster();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void creatingIndexOnMasterShouldHaveSlavesBuildItAsWell()
		 public virtual void CreatingIndexOnMasterShouldHaveSlavesBuildItAsWell()
		 {
			  // GIVEN
			  HighlyAvailableGraphDatabase slave1 = Cluster.AnySlave;
			  HighlyAvailableGraphDatabase slave2 = Cluster.getAnySlave( slave1 );

			  Label label = label( "Person" );

			  // WHEN
			  TransactionContinuation txOnSlave1 = CreateNodeAndKeepTxOpen( slave1, label );
			  TransactionContinuation txOnSlave2 = CreateNodeAndKeepTxOpen( slave2, label );

			  Commit( txOnSlave1 );
			  Commit( txOnSlave2 );

			  // THEN
			  assertEquals( GetLabelId( slave1, label ), GetLabelId( slave2, label ) );
		 }

		 private static long GetLabelId( HighlyAvailableGraphDatabase db, Label label )
		 {
			  using ( Transaction ignore = Db.beginTx() )
			  {
					ThreadToStatementContextBridge bridge = ThreadToStatementContextBridgeFrom( db );
					return bridge.GetKernelTransactionBoundToThisThread( true ).tokenRead().nodeLabel(label.Name());
			  }
		 }

		 private static void Commit( TransactionContinuation txc )
		 {
			  txc.Resume();
			  txc.Commit();
		 }

		 private static TransactionContinuation CreateNodeAndKeepTxOpen( HighlyAvailableGraphDatabase db, Label label )
		 {
			  TransactionContinuation txc = new TransactionContinuation( db );
			  txc.Begin();
			  Db.createNode( label );
			  txc.Suspend();
			  return txc;
		 }

		 private static ThreadToStatementContextBridge ThreadToStatementContextBridgeFrom( HighlyAvailableGraphDatabase db )
		 {
			  return Db.DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) );
		 }

		 private class TransactionContinuation
		 {
			  internal readonly HighlyAvailableGraphDatabase Db;
			  internal KernelTransaction GraphDbTx;
			  internal readonly ThreadToStatementContextBridge Bridge;

			  internal TransactionContinuation( HighlyAvailableGraphDatabase db )
			  {
					this.Db = db;
					this.Bridge = ThreadToStatementContextBridgeFrom( db );
			  }

			  public virtual void Begin()
			  {
					Db.beginTx();
					GraphDbTx = Bridge.getKernelTransactionBoundToThisThread( false );
			  }

			  public virtual void Suspend()
			  {
					GraphDbTx = Bridge.getKernelTransactionBoundToThisThread( true );
					Bridge.unbindTransactionFromCurrentThread();
			  }

			  public virtual void Resume()
			  {
					Bridge.bindTransactionToCurrentThread( GraphDbTx );
			  }

			  public virtual void Commit()
			  {
					try
					{
						 GraphDbTx.close();
					}
					catch ( TransactionFailureException e )
					{
						 throw new Neo4Net.Graphdb.TransactionFailureException( e.Message, e );
					}
			  }
		 }
	}

}