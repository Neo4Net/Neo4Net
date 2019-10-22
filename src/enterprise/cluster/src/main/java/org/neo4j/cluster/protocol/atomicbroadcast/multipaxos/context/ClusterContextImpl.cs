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
namespace Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.context
{

	using ClusterConfiguration = Neo4Net.cluster.protocol.cluster.ClusterConfiguration;
	using ClusterContext = Neo4Net.cluster.protocol.cluster.ClusterContext;
	using ClusterListener = Neo4Net.cluster.protocol.cluster.ClusterListener;
	using ClusterMessage = Neo4Net.cluster.protocol.cluster.ClusterMessage;
	using ClusterState = Neo4Net.cluster.protocol.cluster.ClusterState;
	using HeartbeatContext = Neo4Net.cluster.protocol.heartbeat.HeartbeatContext;
	using HeartbeatListener = Neo4Net.cluster.protocol.heartbeat.HeartbeatListener;
	using Timeouts = Neo4Net.cluster.timeout.Timeouts;
	using Neo4Net.Helpers;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using Config = Neo4Net.Kernel.configuration.Config;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.function.Predicates.@in;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.Uris.parameter;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterables.asList;

	/// <summary>
	/// Context for <seealso cref="ClusterState"/> state machine.
	/// </summary>
	internal class ClusterContextImpl : AbstractContextImpl, ClusterContext
	{
		 private const string DISCOVERY_HEADER_SEPARATOR = ",";

		 // ClusterContext
		 private readonly Listeners<ClusterListener> _clusterListeners = new Listeners<ClusterListener>();
		 /*
		  * Holds instances that we have contacted and which have contacted us. This is achieved by filtering on
		  * receipt via contactingInstances and the DISCOVERED header.
		  * Cleared at the end of each discovery round.
		  */
		 private readonly IList<ClusterMessage.ConfigurationRequestState> _discoveredInstances = new LinkedList<ClusterMessage.ConfigurationRequestState>();

		 /*
		  * Holds instances that have contacted us, along with a set of the instances they have in turn been contacted
		  * from. This is used to determine which instances that have contacted us have received messages from us and thus
		  * are in our initial_hosts. This is used to filter who goes in discoveredInstances.
		  * This map is also used to create the DISCOVERED header, which is basically the keyset in string form.
		  */
		 private readonly IDictionary<InstanceId, ISet<InstanceId>> _contactingInstances = new Dictionary<InstanceId, ISet<InstanceId>>();

		 private IEnumerable<URI> _joiningInstances;
		 private ClusterMessage.ConfigurationResponseState _joinDeniedConfigurationResponseState;
		 private readonly IDictionary<InstanceId, URI> _currentlyJoiningInstances = new Dictionary<InstanceId, URI>();

		 private readonly Executor _executor;
		 private readonly ObjectOutputStreamFactory _objectOutputStreamFactory;
		 private readonly ObjectInputStreamFactory _objectInputStreamFactory;

		 private readonly LearnerContext _learnerContext;
		 private readonly HeartbeatContext _heartbeatContext;
		 private readonly Config _config;

		 private long _electorVersion;
		 private InstanceId _lastElector;

		 internal ClusterContextImpl( InstanceId me, CommonContextState commonState, LogProvider logging, Timeouts timeouts, Executor executor, ObjectOutputStreamFactory objectOutputStreamFactory, ObjectInputStreamFactory objectInputStreamFactory, LearnerContext learnerContext, HeartbeatContext heartbeatContext, Config config ) : base( me, commonState, logging, timeouts )
		 {
			  this._executor = executor;
			  this._objectOutputStreamFactory = objectOutputStreamFactory;
			  this._objectInputStreamFactory = objectInputStreamFactory;
			  this._learnerContext = learnerContext;
			  this._heartbeatContext = heartbeatContext;
			  this._config = config;
			  heartbeatContext.AddHeartbeatListener( new HeartbeatListener_AdapterAnonymousInnerClass( this ) );
		 }

		 private class HeartbeatListener_AdapterAnonymousInnerClass : Neo4Net.cluster.protocol.heartbeat.HeartbeatListener_Adapter
		 {
			 private readonly ClusterContextImpl _outerInstance;

			 public HeartbeatListener_AdapterAnonymousInnerClass( ClusterContextImpl outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void failed( InstanceId server )
			 {
				  outerInstance.invalidateElectorIfNecessary( server );
			 }
		 }

		 private void InvalidateElectorIfNecessary( InstanceId server )
		 {
			  if ( server.Equals( _lastElector ) )
			  {
					_lastElector = InstanceId.NONE;
					_electorVersion = Neo4Net.cluster.protocol.cluster.ClusterContext_Fields.NO_ELECTOR_VERSION;
			  }
		 }

		 private ClusterContextImpl( InstanceId me, CommonContextState commonState, LogProvider logging, Timeouts timeouts, IEnumerable<URI> joiningInstances, ClusterMessage.ConfigurationResponseState joinDeniedConfigurationResponseState, Executor executor, ObjectOutputStreamFactory objectOutputStreamFactory, ObjectInputStreamFactory objectInputStreamFactory, LearnerContext learnerContext, HeartbeatContext heartbeatContext, Config config ) : base( me, commonState, logging, timeouts )
		 {
			  this._joiningInstances = joiningInstances;
			  this._joinDeniedConfigurationResponseState = joinDeniedConfigurationResponseState;
			  this._executor = executor;
			  this._objectOutputStreamFactory = objectOutputStreamFactory;
			  this._objectInputStreamFactory = objectInputStreamFactory;
			  this._learnerContext = learnerContext;
			  this._heartbeatContext = heartbeatContext;
			  this._config = config;
		 }

		 // Cluster API
		 public virtual long LastElectorVersion
		 {
			 get
			 {
				  return _electorVersion;
			 }
			 set
			 {
				  this._electorVersion = value;
			 }
		 }


		 public override bool ShouldFilterContactingInstances()
		 {
			  return _config.get( ClusterSettings.strict_initial_hosts );
		 }

		 public virtual ISet<InstanceId> FailedInstances
		 {
			 get
			 {
				  return _heartbeatContext.Failed;
			 }
		 }

		 public virtual InstanceId LastElector
		 {
			 get
			 {
				  return _lastElector;
			 }
			 set
			 {
				  this._lastElector = value;
			 }
		 }


		 // Cluster API
		 public override void AddClusterListener( ClusterListener listener )
		 {
			  _clusterListeners.add( listener );
		 }

		 public override void RemoveClusterListener( ClusterListener listener )
		 {
			  _clusterListeners.remove( listener );
		 }

		 // Implementation
		 public override void Created( string name )
		 {
			  CommonState.Configuration = new ClusterConfiguration( name, LogProvider, Collections.singleton( CommonState.boundAt() ) );
			  Joined();
		 }

		 public override void Joining( string name, IEnumerable<URI> instanceList )
		 {
			  _joiningInstances = instanceList;
			  _discoveredInstances.Clear();
			  _joinDeniedConfigurationResponseState = null;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void acquiredConfiguration(final java.util.Map<org.Neo4Net.cluster.InstanceId, java.net.URI> memberList, final java.util.Map<String, org.Neo4Net.cluster.InstanceId> roles, final java.util.Set<org.Neo4Net.cluster.InstanceId> failedInstances)
		 public override void AcquiredConfiguration( IDictionary<InstanceId, URI> memberList, IDictionary<string, InstanceId> roles, ISet<InstanceId> failedInstances )
		 {
			  CommonState.configuration().Members = memberList;
			  CommonState.configuration().Roles = roles;
			  foreach ( InstanceId failedInstance in failedInstances )
			  {
					if ( !failedInstance.Equals( Me ) )
					{
						 LogProvider.getLog( typeof( ClusterContextImpl ) ).debug( "Adding instance " + failedInstance + " as failed from the start" );
						 _heartbeatContext.failed( failedInstance );
					}
			  }
		 }

		 public override void Joined()
		 {
			  CommonState.configuration().joined(Me, CommonState.boundAt());
			  _clusterListeners.notify( _executor, listener => listener.enteredCluster( CommonState.configuration() ) );
		 }

		 public override void Left()
		 {
			  Timeouts.cancelAllTimeouts();
			  CommonState.configuration().left();
			  _clusterListeners.notify( _executor, ClusterListener.leftCluster );
		 }

		 public override void Joined( InstanceId instanceId, URI atURI )
		 {
			  CommonState.configuration().joined(instanceId, atURI);

			  if ( CommonState.configuration().Members.ContainsKey(Me) )
			  {
					// Make sure this node is in cluster before notifying of others joining and leaving
					_clusterListeners.notify( _executor, listener => listener.joinedCluster( instanceId, atURI ) );
			  }
			  // else:
			  //   This typically happens in situations when several nodes join at once, and the ordering
			  //   of join messages is a little out of whack.

			  _currentlyJoiningInstances.Remove( instanceId );
			  InvalidateElectorIfNecessary( instanceId );
		 }

		 public override void Left( InstanceId node )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.net.URI member = commonState.configuration().getUriForId(node);
			  URI member = CommonState.configuration().getUriForId(node);
			  CommonState.configuration().left(node);
			  InvalidateElectorIfNecessary( node );
			  _clusterListeners.notify( _executor, listener => listener.leftCluster( node, member ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void elected(final String roleName, final org.Neo4Net.cluster.InstanceId instanceId)
		 public override void Elected( string roleName, InstanceId instanceId )
		 {
			  Elected( roleName, instanceId, InstanceId.NONE, Neo4Net.cluster.protocol.cluster.ClusterContext_Fields.NO_ELECTOR_VERSION );
		 }

		 public override void Elected( string roleName, InstanceId instanceId, InstanceId electorId, long version )
		 {
			  if ( electorId != null )
			  {
					if ( electorId.Equals( MyId ) )
					{
						 GetLog( this.GetType() ).debug("I elected instance " + instanceId + " for role " + roleName + " at version " + version);
						 if ( version < _electorVersion )
						 {
							  return;
						 }
					}
					else if ( electorId.Equals( _lastElector ) && ( version < _electorVersion && version > 1 ) )
					{
						 GetLog( this.GetType() ).warn("Election result for role " + roleName + " received from elector instance " + electorId + " with version " + version + ". I had version " + _electorVersion + " for elector " + _lastElector);
						 return;
					}
					else
					{
						 GetLog( this.GetType() ).debug("Setting elector to " + electorId + " and its version to " + version);
					}

					this._electorVersion = version;
					this._lastElector = electorId;
			  }
			  CommonState.configuration().elected(roleName, instanceId);
			  _clusterListeners.notify( _executor, listener => listener.elected( roleName, instanceId, CommonState.configuration().getUriForId(instanceId) ) );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void unelected(final String roleName, final org.Neo4Net.cluster.InstanceId instanceId)
		 public override void Unelected( string roleName, InstanceId instanceId )
		 {
			  Unelected( roleName, instanceId, InstanceId.NONE, Neo4Net.cluster.protocol.cluster.ClusterContext_Fields.NO_ELECTOR_VERSION );
		 }

		 public override void Unelected( string roleName, InstanceId instanceId, InstanceId electorId, long version )
		 {
			  CommonState.configuration().unelected(roleName);
			  _clusterListeners.notify( _executor, listener => listener.unelected( roleName, instanceId, CommonState.configuration().getUriForId(instanceId) ) );
		 }

		 public virtual ClusterConfiguration Configuration
		 {
			 get
			 {
				  return CommonState.configuration();
			 }
		 }

		 public override bool IsElectedAs( string roleName )
		 {
			  return Me.Equals( CommonState.configuration().getElected(roleName) );
		 }

		 public virtual bool InCluster
		 {
			 get
			 {
				  return Iterables.count( CommonState.configuration().MemberURIs ) != 0;
			 }
		 }

		 public virtual IEnumerable<URI> JoiningInstances
		 {
			 get
			 {
				  return _joiningInstances;
			 }
		 }

		 public virtual ObjectOutputStreamFactory ObjectOutputStreamFactory
		 {
			 get
			 {
				  return _objectOutputStreamFactory;
			 }
		 }

		 public virtual ObjectInputStreamFactory ObjectInputStreamFactory
		 {
			 get
			 {
				  return _objectInputStreamFactory;
			 }
		 }

		 public virtual IList<ClusterMessage.ConfigurationRequestState> DiscoveredInstances
		 {
			 get
			 {
				  return _discoveredInstances;
			 }
		 }

		 public override bool HaveWeContactedInstance( ClusterMessage.ConfigurationRequestState configurationRequested )
		 {
			  return _contactingInstances.ContainsKey( configurationRequested.JoiningId ) && _contactingInstances[configurationRequested.JoiningId].Contains( MyId );
		 }

		 public override void AddContactingInstance( ClusterMessage.ConfigurationRequestState instance, string discoveryHeader )
		 {
			  ISet<InstanceId> contactsOfRemote = _contactingInstances.computeIfAbsent( instance.JoiningId, k => new HashSet<InstanceId>() );
			  // Duplicates of previous calls will be ignored by virtue of this being a set
			  contactsOfRemote.addAll( ParseDiscoveryHeader( discoveryHeader ) );
		 }

		 public override string GenerateDiscoveryHeader()
		 {
			  /*
			   * Maps the keyset of contacting instances from InstanceId to strings, collects them in a Set and joins them
			   * in a string with the appropriate separator
			   */
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  return string.join( DISCOVERY_HEADER_SEPARATOR, _contactingInstances.Keys.Select( InstanceId::toString ).collect( Collectors.toSet() ) );
		 }

		 private ISet<InstanceId> ParseDiscoveryHeader( string discoveryHeader )
		 {
			  string[] instanceIds = discoveryHeader.Split( DISCOVERY_HEADER_SEPARATOR, true );
			  ISet<InstanceId> result = new HashSet<InstanceId>();
			  foreach ( string instanceId in instanceIds )
			  {
					try
					{
						 result.Add( new InstanceId( int.Parse( instanceId.Trim() ) ) );
					}
					catch ( System.FormatException )
					{
						 /*
						  * This will happen if the message did not contain a DISCOVERY header. There are two reasons for this.
						  * One, the first configurationRequest going out from every instance does have the header but
						  * it is empty, since it's sent before any configurationRequests are processed.
						  * The other is practically the backwards compatibility code for versions which do not carry this header.
						  *
						  * Since the header will be empty (default value for it is empty string), the split above will create
						  * an array with a single empty string. This fails the integer parse.
						  */
						 GetLog( this.GetType() ).debug("Could not parse discovery header for contacted instances, its value was " + discoveryHeader);
					}
			  }
			  return result;
		 }

		 public override string ToString()
		 {
			  return "Me: " + Me + " Bound at: " + CommonState.boundAt() + " Config:" + CommonState.configuration() +
						 " Current state: " + CommonState;
		 }

		 public virtual URI BoundAt
		 {
			 set
			 {
				  CommonState.setBoundAt( Me, value );
			 }
		 }

		 public override void JoinDenied( ClusterMessage.ConfigurationResponseState configurationResponseState )
		 {
			  if ( configurationResponseState == null )
			  {
					throw new System.ArgumentException( "Join denied configuration response state was null" );
			  }
			  this._joinDeniedConfigurationResponseState = configurationResponseState;
		 }

		 public override bool HasJoinBeenDenied()
		 {
			  return _joinDeniedConfigurationResponseState != null;
		 }

		 public virtual ClusterMessage.ConfigurationResponseState JoinDeniedConfigurationResponseState
		 {
			 get
			 {
				  if ( !HasJoinBeenDenied() )
				  {
						throw new System.InvalidOperationException( "Join has not been denied" );
				  }
				  return _joinDeniedConfigurationResponseState;
			 }
		 }

		 public virtual IEnumerable<InstanceId> OtherInstances
		 {
			 get
			 {
				  return Iterables.filter( @in( Me ).negate(), CommonState.configuration().MemberIds );
			 }
		 }

		 /// <summary>
		 /// Used to ensure that no other instance is trying to join with the same id from a different machine </summary>
		 public override bool IsInstanceJoiningFromDifferentUri( InstanceId joiningId, URI uri )
		 {
			  return _currentlyJoiningInstances.ContainsKey( joiningId ) && !_currentlyJoiningInstances[joiningId].Equals( uri );
		 }

		 public override void InstanceIsJoining( InstanceId joiningId, URI uri )
		 {
			  _currentlyJoiningInstances[joiningId] = uri;
		 }

		 public override string MyName()
		 {
			  string name = parameter( "name" ).apply( CommonState.boundAt() );
			  if ( !string.ReferenceEquals( name, null ) )
			  {
					return name;
			  }
			  else
			  {
					return Me.ToString();
			  }
		 }

		 public override void DiscoveredLastReceivedInstanceId( long id )
		 {
			  _learnerContext.LastDeliveredInstanceId = id;
			  _learnerContext.learnedInstanceId( id );
			  _learnerContext.NextInstanceId = id + 1;
		 }

		 public override bool IsCurrentlyAlive( InstanceId joiningId )
		 {
			  return !_heartbeatContext.Failed.Contains( joiningId );
		 }

		 public virtual long LastDeliveredInstanceId
		 {
			 get
			 {
				  return _learnerContext.LastDeliveredInstanceId;
			 }
		 }

		 public virtual ClusterContextImpl Snapshot( CommonContextState commonStateSnapshot, LogProvider logging, Timeouts timeouts, Executor executor, ObjectOutputStreamFactory objectOutputStreamFactory, ObjectInputStreamFactory objectInputStreamFactory, LearnerContextImpl snapshotLearnerContext, HeartbeatContextImpl snapshotHeartbeatContext )
		 {
			  return new ClusterContextImpl( Me, commonStateSnapshot, logging, timeouts, _joiningInstances == null ? null : new List<>( asList( _joiningInstances ) ), _joinDeniedConfigurationResponseState == null ? null : _joinDeniedConfigurationResponseState.snapshot(), executor, objectOutputStreamFactory, objectInputStreamFactory, snapshotLearnerContext, snapshotHeartbeatContext, _config );
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  ClusterContextImpl that = ( ClusterContextImpl ) o;

			  if ( _currentlyJoiningInstances != null ?!_currentlyJoiningInstances.Equals( that._currentlyJoiningInstances ) : that._currentlyJoiningInstances != null )
			  {
					return false;
			  }
//JAVA TO C# CONVERTER WARNING: LINQ 'SequenceEqual' is not always identical to Java AbstractList 'equals':
//ORIGINAL LINE: if (discoveredInstances != null ? !discoveredInstances.equals(that.discoveredInstances) : that.discoveredInstances != null)
			  if ( _discoveredInstances != null ?!_discoveredInstances.SequenceEqual( that._discoveredInstances ) : that._discoveredInstances != null )
			  {
					return false;
			  }
			  if ( _heartbeatContext != null ?!_heartbeatContext.Equals( that._heartbeatContext ) : that._heartbeatContext != null )
			  {
					return false;
			  }
			  if ( _joinDeniedConfigurationResponseState != null ?!_joinDeniedConfigurationResponseState.Equals( that._joinDeniedConfigurationResponseState ) : that._joinDeniedConfigurationResponseState != null )
			  {
					return false;
			  }
			  if ( _joiningInstances != null ?!_joiningInstances.Equals( that._joiningInstances ) : that._joiningInstances != null )
			  {
					return false;
			  }
			  return _learnerContext != null ? _learnerContext.Equals( that._learnerContext ) : that._learnerContext == null;
		 }

		 public override int GetHashCode()
		 {
			  int result = 0;
			  result = 31 * result + ( _discoveredInstances != null ? _discoveredInstances.GetHashCode() : 0 );
			  result = 31 * result + ( _joiningInstances != null ? _joiningInstances.GetHashCode() : 0 );
			  result = 31 * result + ( _joinDeniedConfigurationResponseState != null ? _joinDeniedConfigurationResponseState.GetHashCode() : 0 );
			  result = 31 * result + ( _currentlyJoiningInstances != null ? _currentlyJoiningInstances.GetHashCode() : 0 );
			  result = 31 * result + ( _learnerContext != null ? _learnerContext.GetHashCode() : 0 );
			  result = 31 * result + ( _heartbeatContext != null ? _heartbeatContext.GetHashCode() : 0 );
			  return result;
		 }
	}

}