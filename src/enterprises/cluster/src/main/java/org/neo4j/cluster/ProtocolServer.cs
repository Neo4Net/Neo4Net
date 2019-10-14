using System;
using System.Text;

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
namespace Neo4Net.cluster
{

	using BindingNotifier = Neo4Net.cluster.com.BindingNotifier;
	using Neo4Net.cluster.statemachine;
	using StateMachineConversations = Neo4Net.cluster.statemachine.StateMachineConversations;
	using StateMachineProxyFactory = Neo4Net.cluster.statemachine.StateMachineProxyFactory;
	using StateTransitionListener = Neo4Net.cluster.statemachine.StateTransitionListener;
	using Timeouts = Neo4Net.cluster.timeout.Timeouts;
	using Neo4Net.Helpers;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	/// <summary>
	/// A ProtocolServer ties together the underlying StateMachines with an understanding of ones
	/// own server address (me), and provides a proxy factory for creating clients to invoke the CSM.
	/// </summary>
	public class ProtocolServer : LifecycleAdapter, BindingNotifier
	{
		 private readonly InstanceId _me;
		 private URI _boundAt;
		 protected internal StateMachineProxyFactory ProxyFactory;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 protected internal readonly StateMachines StateMachinesConflict;
		 private readonly Listeners<BindingListener> _bindingListeners = new Listeners<BindingListener>();
		 private Log _msgLog;

		 public ProtocolServer( InstanceId me, StateMachines stateMachines, LogProvider logProvider )
		 {
			  this._me = me;
			  this.StateMachinesConflict = stateMachines;
			  this._msgLog = logProvider.getLog( this.GetType() );

			  StateMachineConversations conversations = new StateMachineConversations( me );
			  ProxyFactory = new StateMachineProxyFactory( stateMachines, conversations, me, logProvider );
			  stateMachines.AddMessageProcessor( ProxyFactory );
		 }

		 public override void Shutdown()
		 {
			  _msgLog = null;
		 }

		 public override void AddBindingListener( BindingListener listener )
		 {
			  _bindingListeners.add( listener );
			  try
			  {
					if ( _boundAt != null )
					{
						 listener.ListeningAt( _boundAt );
					}
			  }
			  catch ( Exception t )
			  {
					_msgLog.error( "Failed while adding BindingListener", t );
			  }
		 }

		 public override void RemoveBindingListener( BindingListener listener )
		 {
			  _bindingListeners.remove( listener );
		 }

		 public virtual void ListeningAt( URI me )
		 {
			  this._boundAt = me;

			  _bindingListeners.notify( listener => listener.listeningAt( me ) );
		 }

		 /// <summary>
		 /// Ok to have this accessible like this?
		 /// </summary>
		 /// <returns> server id </returns>
		 public virtual InstanceId ServerId
		 {
			 get
			 {
				  return _me;
			 }
		 }

		 public virtual StateMachines StateMachines
		 {
			 get
			 {
				  return StateMachinesConflict;
			 }
		 }

		 public virtual void AddStateTransitionListener( StateTransitionListener stateTransitionListener )
		 {
			  StateMachinesConflict.addStateTransitionListener( stateTransitionListener );
		 }

		 public virtual T NewClient<T>( Type clientProxyInterface )
		 {
				 clientProxyInterface = typeof( T );
			  return ProxyFactory.newProxy( clientProxyInterface );
		 }

		 public override string ToString()
		 {
			  StringBuilder builder = new StringBuilder();
			  builder.Append( "Instance URI: " ).Append( _boundAt.ToString() ).Append("\n");
			  foreach ( StateMachine stateMachine in StateMachinesConflict.getStateMachines() )
			  {
					builder.Append( "  " ).Append( stateMachine ).Append( "\n" );
			  }
			  return builder.ToString();
		 }

		 public virtual Timeouts Timeouts
		 {
			 get
			 {
				  return StateMachinesConflict.Timeouts;
			 }
		 }

		 public virtual URI BoundAt()
		 {
			  return _boundAt;
		 }
	}

}