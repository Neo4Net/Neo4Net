using System;
using System.Collections.Generic;

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
namespace Neo4Net.cluster.protocol.atomicbroadcast.multipaxos
{
	using Test = org.junit.Test;


	using ClusterContext = Neo4Net.cluster.protocol.cluster.ClusterContext;
	using IntegerElectionCredentials = Neo4Net.cluster.protocol.election.IntegerElectionCredentials;
	using NotElectableElectionCredentials = Neo4Net.cluster.protocol.election.NotElectableElectionCredentials;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class DefaultWinnerStrategyTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogElectionProcess()
		 public virtual void ShouldLogElectionProcess()
		 {
			  // given
			  ClusterContext clusterContext = mock( typeof( ClusterContext ) );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.logging.Log log = mock(Neo4Net.logging.Log.class);
			  Log log = mock( typeof( Log ) );
			  LogProvider logProvider = new LogProviderAnonymousInnerClass( this, log );

			  when( clusterContext.GetLog( typeof( DefaultWinnerStrategy ) ) ).thenReturn( logProvider.GetLog( typeof( DefaultWinnerStrategy ) ) );

			  // when
			  ICollection<Vote> votes = Collections.emptyList();

			  DefaultWinnerStrategy strategy = new DefaultWinnerStrategy( clusterContext );
			  strategy.PickWinner( votes );

			  // then
			  verify( log ).debug( "Election: received votes [], eligible votes []" );
		 }

		 private class LogProviderAnonymousInnerClass : LogProvider
		 {
			 private readonly DefaultWinnerStrategyTest _outerInstance;

			 private Log _log;

			 public LogProviderAnonymousInnerClass( DefaultWinnerStrategyTest outerInstance, Log log )
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

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotPickAWinnerIfAllVotesAreForIneligibleCandidates()
		 public virtual void ShouldNotPickAWinnerIfAllVotesAreForIneligibleCandidates()
		 {
			  // given
			  InstanceId instanceOne = new InstanceId( 1 );
			  InstanceId instanceTwo = new InstanceId( 2 );

			  ClusterContext clusterContext = mock( typeof( ClusterContext ) );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.logging.Log log = mock(Neo4Net.logging.Log.class);
			  Log log = mock( typeof( Log ) );
			  LogProvider logProvider = new LogProviderAnonymousInnerClass2( this, log );

			  when( clusterContext.GetLog( typeof( DefaultWinnerStrategy ) ) ).thenReturn( logProvider.GetLog( typeof( DefaultWinnerStrategy ) ) );

			  // when
			  ICollection<Vote> votes = Arrays.asList( new Vote( instanceOne, new NotElectableElectionCredentials() ), new Vote(instanceTwo, new NotElectableElectionCredentials()) );

			  DefaultWinnerStrategy strategy = new DefaultWinnerStrategy( clusterContext );
			  InstanceId winner = strategy.PickWinner( votes );

			  // then
			  assertNull( winner );
		 }

		 private class LogProviderAnonymousInnerClass2 : LogProvider
		 {
			 private readonly DefaultWinnerStrategyTest _outerInstance;

			 private Log _log;

			 public LogProviderAnonymousInnerClass2( DefaultWinnerStrategyTest outerInstance, Log log )
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

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void theWinnerShouldHaveTheBestVoteCredentials()
		 public virtual void TheWinnerShouldHaveTheBestVoteCredentials()
		 {
			  // given
			  InstanceId instanceOne = new InstanceId( 1 );
			  InstanceId instanceTwo = new InstanceId( 2 );

			  ClusterContext clusterContext = mock( typeof( ClusterContext ) );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.logging.Log log = mock(Neo4Net.logging.Log.class);
			  Log log = mock( typeof( Log ) );
			  LogProvider logProvider = new LogProviderAnonymousInnerClass3( this, log );

			  when( clusterContext.GetLog( typeof( DefaultWinnerStrategy ) ) ).thenReturn( logProvider.GetLog( typeof( DefaultWinnerStrategy ) ) );

			  // when
			  ICollection<Vote> votes = Arrays.asList( new Vote( instanceOne, new IntegerElectionCredentials( 1 ) ), new Vote( instanceTwo, new IntegerElectionCredentials( 2 ) ) );

			  DefaultWinnerStrategy strategy = new DefaultWinnerStrategy( clusterContext );
			  InstanceId winner = strategy.PickWinner( votes );

			  // then
			  assertEquals( instanceTwo, winner );
		 }

		 private class LogProviderAnonymousInnerClass3 : LogProvider
		 {
			 private readonly DefaultWinnerStrategyTest _outerInstance;

			 private Log _log;

			 public LogProviderAnonymousInnerClass3( DefaultWinnerStrategyTest outerInstance, Log log )
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