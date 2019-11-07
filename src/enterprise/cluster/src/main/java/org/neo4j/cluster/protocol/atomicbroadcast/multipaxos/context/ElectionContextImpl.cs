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

	using ClusterContext = Neo4Net.cluster.protocol.cluster.ClusterContext;
	using ClusterMessage = Neo4Net.cluster.protocol.cluster.ClusterMessage;
	using ElectionContext = Neo4Net.cluster.protocol.election.ElectionContext;
	using ElectionCredentials = Neo4Net.cluster.protocol.election.ElectionCredentials;
	using ElectionCredentialsProvider = Neo4Net.cluster.protocol.election.ElectionCredentialsProvider;
	using ElectionRole = Neo4Net.cluster.protocol.election.ElectionRole;
	using NotElectableElectionCredentials = Neo4Net.cluster.protocol.election.NotElectableElectionCredentials;
	using HeartbeatContext = Neo4Net.cluster.protocol.heartbeat.HeartbeatContext;
	using HeartbeatListener = Neo4Net.cluster.protocol.heartbeat.HeartbeatListener;
	using Timeouts = Neo4Net.cluster.timeout.Timeouts;
	using LogProvider = Neo4Net.Logging.LogProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.cluster.util.Quorums.isQuorum;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterables.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterables.filter;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterables.map;

	public class ElectionContextImpl : AbstractContextImpl, ElectionContext, HeartbeatListener
	{
		 private readonly ClusterContext _clusterContext;
		 private readonly HeartbeatContext _heartbeatContext;

		 private readonly IList<ElectionRole> _roles;
		 private readonly IDictionary<string, Election> _elections;
		 private readonly ElectionCredentialsProvider _electionCredentialsProvider;

		 internal ElectionContextImpl( InstanceId me, CommonContextState commonState, LogProvider logging, Timeouts timeouts, IEnumerable<ElectionRole> roles, ClusterContext clusterContext, HeartbeatContext heartbeatContext, ElectionCredentialsProvider electionCredentialsProvider ) : base( me, commonState, logging, timeouts )
		 {
			  this._electionCredentialsProvider = electionCredentialsProvider;
			  this._roles = new List<ElectionRole>( asList( roles ) );
			  this._elections = new Dictionary<string, Election>();
			  this._clusterContext = clusterContext;
			  this._heartbeatContext = heartbeatContext;

			  heartbeatContext.AddHeartbeatListener( this );
		 }

		 internal ElectionContextImpl( InstanceId me, CommonContextState commonState, LogProvider logging, Timeouts timeouts, ClusterContext clusterContext, HeartbeatContext heartbeatContext, IList<ElectionRole> roles, IDictionary<string, Election> elections, ElectionCredentialsProvider electionCredentialsProvider ) : base( me, commonState, logging, timeouts )
		 {
			  this._clusterContext = clusterContext;
			  this._heartbeatContext = heartbeatContext;
			  this._roles = roles;
			  this._elections = elections;
			  this._electionCredentialsProvider = electionCredentialsProvider;

			  heartbeatContext.AddHeartbeatListener( this );
		 }

		 public override void Created()
		 {
			  foreach ( ElectionRole role in _roles )
			  {
					// Elect myself for all roles
					_clusterContext.elected( role.Name, _clusterContext.MyId, _clusterContext.MyId, 1 );
			  }
		 }

		 public virtual IList<ElectionRole> PossibleRoles
		 {
			 get
			 {
				  return _roles;
			 }
		 }

		 /*
		  * Removes all roles from the provided node. This is expected to be the first call when receiving a demote
		  * message for a node, since it is the way to ensure that election will happen for each role that node had
		  */
		 public override void NodeFailed( InstanceId node )
		 {
			  IEnumerable<string> rolesToDemote = GetRoles( node );
			  foreach ( string role in rolesToDemote )
			  {
					_clusterContext.Configuration.removeElected( role );
			  }
		 }

		 public override IEnumerable<string> GetRoles( InstanceId server )
		 {
			  return _clusterContext.Configuration.getRolesOf( server );
		 }

		 public virtual ClusterContext ClusterContext
		 {
			 get
			 {
				  return _clusterContext;
			 }
		 }

		 public virtual HeartbeatContext HeartbeatContext
		 {
			 get
			 {
				  return _heartbeatContext;
			 }
		 }

		 public override bool IsElectionProcessInProgress( string role )
		 {
			  return _elections.ContainsKey( role );
		 }

		 public override void StartElectionProcess( string role )
		 {
			  _clusterContext.getLog( this.GetType() ).info("Doing elections for role " + role);
			  if ( !_clusterContext.MyId.Equals( _clusterContext.LastElector ) )
			  {
					_clusterContext.LastElector = _clusterContext.MyId;
			  }
			  _elections[role] = new Election( new DefaultWinnerStrategy( _clusterContext ) );
		 }

		 public override bool Voted( string role, InstanceId suggestedNode, ElectionCredentials suggestionCredentials, long electionVersion )
		 {
			  if ( !IsElectionProcessInProgress( role ) || ( electionVersion != -1 && electionVersion < _clusterContext.LastElectorVersion ) )
			  {
					return false;
			  }
			  IDictionary<InstanceId, Vote> votes = _elections[role].Votes;
			  votes[suggestedNode] = new Vote( suggestedNode, suggestionCredentials );
			  return true;
		 }

		 public override InstanceId GetElectionWinner( string role )
		 {
			  Election election = _elections[role];
			  if ( election == null || election.Votes.Count != NeededVoteCount )
			  {
					return null;
			  }

			  _elections.Remove( role );

			  return election.PickWinner();
		 }

		 public override ElectionCredentials GetCredentialsForRole( string role )
		 {
			  return _electionCredentialsProvider.getCredentials( role );
		 }

		 public override int GetVoteCount( string role )
		 {
			  Election election = _elections[role];
			  if ( election != null )
			  {
					IDictionary<InstanceId, Vote> voteList = election.Votes;
					if ( voteList == null )
					{
						 return 0;
					}

					return voteList.Count;
			  }
			  else
			  {
					return 0;
			  }
		 }

		 public virtual int NeededVoteCount
		 {
			 get
			 {
				  return _clusterContext.Configuration.Members.Count - _heartbeatContext.Failed.Count;
				  // TODO increment election epoch
			 }
		 }

		 public override void ForgetElection( string role )
		 {
			  _elections.Remove( role );
			  _clusterContext.LastElectorVersion = _clusterContext.LastElectorVersion + 1;
		 }

		 public virtual IEnumerable<string> RolesRequiringElection
		 {
			 get
			 {
	//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
				  return filter( role => _clusterContext.Configuration.getElected( role ) == null, map( ElectionRole::getName, _roles ) );
			 }
		 }

		 public override bool ElectionOk()
		 {
			  int total = _clusterContext.Configuration.Members.Count;
			  int available = total - _heartbeatContext.Failed.Count;
			  return isQuorum( available, total );
		 }

		 public virtual bool InCluster
		 {
			 get
			 {
				  return _clusterContext.InCluster;
			 }
		 }

		 public virtual IEnumerable<InstanceId> Alive
		 {
			 get
			 {
				  return _heartbeatContext.Alive;
			 }
		 }

		 public override InstanceId MyId
		 {
			 get
			 {
				  return _clusterContext.MyId;
			 }
		 }

		 public virtual bool Elector
		 {
			 get
			 {
				  // Only the first *alive* server should try elections. Everyone else waits
				  // This also takes into account the instances reported by the cluster join response as failed, to
				  // cover for the case where we just joined and our suspicions are not reliable yet.
				  IList<InstanceId> aliveInstances = new IList<InstanceId> { Alive };
	//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'removeAll' method:
				  aliveInstances.removeAll( Failed );
				  aliveInstances.Sort();
				  // Either we are the first one or the only one
				  return aliveInstances.IndexOf( MyId ) == 0 || aliveInstances.Count == 0;
			 }
		 }

		 public override bool IsFailed( InstanceId key )
		 {
			  return _heartbeatContext.Failed.Contains( key );
		 }

		 public override InstanceId GetElected( string roleName )
		 {
			  return _clusterContext.Configuration.getElected( roleName );
		 }

		 public override bool HasCurrentlyElectedVoted( string role, InstanceId currentElected )
		 {
			  return _elections.ContainsKey( role ) && _elections[role].Votes.ContainsKey( currentElected );
		 }

		 public virtual ISet<InstanceId> Failed
		 {
			 get
			 {
				  return _heartbeatContext.Failed;
			 }
		 }

		 public virtual ElectionContextImpl Snapshot( CommonContextState commonStateSnapshot, LogProvider logging, Timeouts timeouts, ClusterContextImpl snapshotClusterContext, HeartbeatContextImpl snapshotHeartbeatContext, ElectionCredentialsProvider credentialsProvider )

		 {
			  IDictionary<string, Election> electionsSnapshot = new Dictionary<string, Election>();
			  foreach ( KeyValuePair<string, Election> election in _elections.SetOfKeyValuePairs() )
			  {
					electionsSnapshot[election.Key] = election.Value.snapshot();
			  }

			  return new ElectionContextImpl( Me, commonStateSnapshot, logging, timeouts, snapshotClusterContext, snapshotHeartbeatContext, new List<ElectionRole>( _roles ), electionsSnapshot, credentialsProvider );
		 }

		 private class Election
		 {
			  internal readonly WinnerStrategy WinnerStrategy;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly IDictionary<InstanceId, Vote> VotesConflict;

			  internal Election( WinnerStrategy winnerStrategy )
			  {
					this.WinnerStrategy = winnerStrategy;
					this.VotesConflict = new Dictionary<InstanceId, Vote>();
			  }

			  internal Election( WinnerStrategy winnerStrategy, Dictionary<InstanceId, Vote> votes )
			  {
					this.VotesConflict = votes;
					this.WinnerStrategy = winnerStrategy;
			  }

			  public virtual IDictionary<InstanceId, Vote> Votes
			  {
				  get
				  {
						return VotesConflict;
				  }
			  }

			  public virtual InstanceId PickWinner()
			  {
					return WinnerStrategy.pickWinner( VotesConflict.Values );
			  }

			  public virtual Election Snapshot()
			  {
					return new Election( WinnerStrategy, new Dictionary<InstanceId, Vote>( VotesConflict ) );
			  }
		 }

		 public override ClusterMessage.VersionedConfigurationStateChange NewConfigurationStateChange()
		 {
			  ClusterMessage.VersionedConfigurationStateChange result = new ClusterMessage.VersionedConfigurationStateChange();
			  result.Elector = _clusterContext.MyId;
			  result.Version = _clusterContext.LastElectorVersion;
			  return result;
		 }

		 public override Neo4Net.cluster.protocol.election.ElectionContext_VoteRequest VoteRequestForRole( ElectionRole role )
		 {
			  return new Neo4Net.cluster.protocol.election.ElectionContext_VoteRequest( role.Name, _clusterContext.LastElectorVersion );
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

			  ElectionContextImpl that = ( ElectionContextImpl ) o;

			  if ( _elections != null ?!_elections.Equals( that._elections ) : that._elections != null )
			  {
					return false;
			  }
//JAVA TO C# CONVERTER WARNING: LINQ 'SequenceEqual' is not always identical to Java AbstractList 'equals':
//ORIGINAL LINE: return roles != null ? roles.equals(that.roles) : that.roles == null;
			  return _roles != null ? _roles.SequenceEqual( that._roles ) : that._roles == null;
		 }

		 public override int GetHashCode()
		 {
			  int result = _roles != null ? _roles.GetHashCode() : 0;
			  result = 31 * result + ( _elections != null ? _elections.GetHashCode() : 0 );
			  return result;
		 }

		 public override void Failed( InstanceId server )
		 {
			  foreach ( KeyValuePair<string, Election> ongoingElection in _elections.SetOfKeyValuePairs() )
			  {
					ongoingElection.Value.Votes.remove( server );
			  }
		 }

		 public override void Alive( InstanceId server )
		 {
			  // Not needed
		 }

		 public static IList<Vote> RemoveBlankVotes( ICollection<Vote> voteList )
		 {
			  return new IList<Vote> { filter( item => !( item.Credentials is NotElectableElectionCredentials ), voteList ) };
		 }
	}

}