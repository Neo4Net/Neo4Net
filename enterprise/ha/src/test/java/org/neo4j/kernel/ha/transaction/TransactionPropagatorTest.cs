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
namespace Org.Neo4j.Kernel.ha.transaction
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using InstanceId = Org.Neo4j.cluster.InstanceId;
	using Org.Neo4j.com;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Slave = Org.Neo4j.Kernel.ha.com.master.Slave;
	using SlavePriorities = Org.Neo4j.Kernel.ha.com.master.SlavePriorities;
	using SlavePriority = Org.Neo4j.Kernel.ha.com.master.SlavePriority;
	using Slaves = Org.Neo4j.Kernel.ha.com.master.Slaves;
	using Configuration = Org.Neo4j.Kernel.ha.transaction.TransactionPropagator.Configuration;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using LifeRule = Org.Neo4j.Kernel.Lifecycle.LifeRule;
	using Log = Org.Neo4j.Logging.Log;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Arrays.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsEqual.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.anyString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.times;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.HaSettings.TxPushStrategy.fixed_ascending;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.HaSettings.TxPushStrategy.fixed_descending;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.ha.HaSettings.tx_push_strategy;

	public class TransactionPropagatorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.kernel.lifecycle.LifeRule life = new org.neo4j.kernel.lifecycle.LifeRule(true);
		 public readonly LifeRule Life = new LifeRule( true );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCapUndesiredSlaveCountPushLogging()
		 public virtual void ShouldCapUndesiredSlaveCountPushLogging()
		 {
			  // GIVEN
			  int serverId = 1;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.cluster.InstanceId instanceId = new org.neo4j.cluster.InstanceId(serverId);
			  InstanceId instanceId = new InstanceId( serverId );
			  Configuration config = new ConfigurationAnonymousInnerClass( this, instanceId );
			  Log logger = mock( typeof( Log ) );
			  Slaves slaves = mock( typeof( Slaves ) );
			  when( slaves.GetSlaves() ).thenReturn(Collections.emptyList());
			  CommitPusher pusher = mock( typeof( CommitPusher ) );
			  TransactionPropagator propagator = Life.add( new TransactionPropagator( config, logger, slaves, pusher ) );

			  // WHEN
			  for ( int i = 0; i < 10; i++ )
			  {
					propagator.Committed( Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID, serverId );
			  }

			  // THEN
			  verify( logger, times( 1 ) ).info( anyString() );
		 }

		 private class ConfigurationAnonymousInnerClass : Configuration
		 {
			 private readonly TransactionPropagatorTest _outerInstance;

			 private InstanceId _instanceId;

			 public ConfigurationAnonymousInnerClass( TransactionPropagatorTest outerInstance, InstanceId instanceId )
			 {
				 this.outerInstance = outerInstance;
				 this._instanceId = instanceId;
			 }

			 public int TxPushFactor
			 {
				 get
				 {
					  return 1;
				 }
			 }

			 public InstanceId ServerId
			 {
				 get
				 {
					  return _instanceId;
				 }
			 }

			 public SlavePriority ReplicationStrategy
			 {
				 get
				 {
					  return SlavePriorities.fixedDescending();
				 }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPrioritizeAscendingIfAsked()
		 public virtual void ShouldPrioritizeAscendingIfAsked()
		 {
			  // GIVEN
			  Configuration propagator = TransactionPropagator.From( Config.defaults( tx_push_strategy, fixed_ascending.name() ) );
			  SlavePriority strategy = propagator.ReplicationStrategy;

			  // WHEN
			  IEnumerable<Slave> prioritize = strategy.Prioritize( asList( Slave( 1 ), Slave( 0 ), Slave( 2 ) ) );

			  // THEN
			  assertThat( Iterables.asList( prioritize ), equalTo( asList( Slave( 0 ), Slave( 1 ), Slave( 2 ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPrioritizeDescendingIfAsked()
		 public virtual void ShouldPrioritizeDescendingIfAsked()
		 {
			  // GIVEN
			  Configuration propagator = TransactionPropagator.From( Config.defaults( tx_push_strategy, fixed_descending.name() ) );
			  SlavePriority strategy = propagator.ReplicationStrategy;

			  // WHEN
			  IEnumerable<Slave> prioritize = strategy.Prioritize( asList( Slave( 1 ), Slave( 0 ), Slave( 2 ) ) );

			  // THEN
			  assertThat( Iterables.asList( prioritize ), equalTo( asList( Slave( 2 ), Slave( 1 ), Slave( 0 ) ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.neo4j.kernel.ha.com.master.Slave slave(final int id)
		 private Slave Slave( int id )
		 {
			  return new SlaveAnonymousInnerClass( this, id );
		 }

		 private class SlaveAnonymousInnerClass : Slave
		 {
			 private readonly TransactionPropagatorTest _outerInstance;

			 private int _id;

			 public SlaveAnonymousInnerClass( TransactionPropagatorTest outerInstance, int id )
			 {
				 this.outerInstance = outerInstance;
				 this._id = id;
			 }

			 public Response<Void> pullUpdates( long upToAndIncludingTxId )
			 {
				  throw new System.NotSupportedException();
			 }

			 public int ServerId
			 {
				 get
				 {
					  return _id;
				 }
			 }

			 public override bool Equals( object obj )
			 {
				  return obj is Slave && ( ( Slave ) obj ).ServerId == _id;
			 }

			 public override int GetHashCode()
			 {
				  return _id;
			 }

			 public override string ToString()
			 {
				  return "Slave[" + _id + "]";
			 }
		 }
	}

}