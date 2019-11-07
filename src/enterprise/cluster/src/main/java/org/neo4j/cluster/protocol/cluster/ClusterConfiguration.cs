using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.cluster.protocol.cluster
{

	using Iterables = Neo4Net.Collections.Helpers.Iterables;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	/// <summary>
	/// Cluster configuration. Includes name of cluster, list of nodes, and role mappings
	/// </summary>
	public class ClusterConfiguration
	{
		 public const string COORDINATOR = "coordinator";

		 private readonly string _name;
		 private readonly Log _log;
		 private readonly IList<URI> _candidateMembers;
		 private volatile IDictionary<InstanceId, URI> _members;
		 private volatile IDictionary<string, InstanceId> _roles = new Dictionary<string, InstanceId>();

		 public ClusterConfiguration( string name, LogProvider logProvider, params string[] members )
		 {
			  this._name = name;
			  this._log = logProvider.getLog( this.GetType() );
			  this._candidateMembers = new List<URI>();
			  foreach ( string node in members )
			  {
					try
					{
						 this._candidateMembers.Add( new URI( node ) );
					}
					catch ( URISyntaxException e )
					{
						 Console.WriteLine( e.ToString() );
						 Console.Write( e.StackTrace );
					}
			  }
			  this._members = new Dictionary<InstanceId, URI>();
		 }

		 public ClusterConfiguration( string name, LogProvider logProvider, ICollection<URI> members )
		 {
			  this._name = name;
			  this._log = logProvider.getLog( this.GetType() );
			  this._candidateMembers = new List<URI>( members );
			  this._members = new Dictionary<InstanceId, URI>();
		 }

		 public ClusterConfiguration( ClusterConfiguration copy ) : this( copy, copy._log )
		 {
		 }

		 private ClusterConfiguration( ClusterConfiguration copy, Log log )
		 {
			  this._name = copy._name;
			  this._log = log;
			  this._candidateMembers = new List<URI>( copy._candidateMembers );
			  this._roles = new Dictionary<string, InstanceId>( copy._roles );
			  this._members = new Dictionary<InstanceId, URI>( copy._members );
		 }

		 public virtual void Joined( InstanceId joinedInstanceId, URI instanceUri )
		 {
			  if ( instanceUri.Equals( _members[joinedInstanceId] ) )
			  {
					return; // Already know that this node is in - ignore
			  }

			  IDictionary<InstanceId, URI> newMembers = new Dictionary<InstanceId, URI>( _members );
			  newMembers[joinedInstanceId] = instanceUri;
			  _members = newMembers;
		 }

		 public virtual void Left( InstanceId leftInstanceId )
		 {
			  _log.info( "Instance " + leftInstanceId + " is leaving the cluster" );
			  IDictionary<InstanceId, URI> newMembers = new Dictionary<InstanceId, URI>( _members );
			  newMembers.Remove( leftInstanceId );
			  _members = newMembers;

			  // Remove any roles that this node had
			  IEnumerator<KeyValuePair<string, InstanceId>> entries = _roles.SetOfKeyValuePairs().GetEnumerator();
			  while ( entries.MoveNext() )
			  {
					KeyValuePair<string, InstanceId> roleEntry = entries.Current;

					if ( roleEntry.Value.Equals( leftInstanceId ) )
					{
						 _log.info( "Removed role " + roleEntry.Value + " from leaving instance " + roleEntry.Key );
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
						 entries.remove();
					}
			  }
		 }

		 public virtual void Elected( string name, InstanceId electedInstanceId )
		 {
			  Debug.Assert( _members.ContainsKey( electedInstanceId ) );
			  IDictionary<string, InstanceId> newRoles = new Dictionary<string, InstanceId>( _roles );
			  newRoles[name] = electedInstanceId;
			  _roles = newRoles;
		 }

		 public virtual void Unelected( string roleName )
		 {
			  Debug.Assert( _roles.ContainsKey( roleName ) );
			  IDictionary<string, InstanceId> newRoles = new Dictionary<string, InstanceId>( _roles );
			  newRoles.Remove( roleName );
			  _roles = newRoles;
		 }

		 public virtual IDictionary<InstanceId, URI> Members
		 {
			 set
			 {
				  this._members = new Dictionary<InstanceId, URI>( value );
			 }
			 get
			 {
				  return _members;
			 }
		 }

		 public virtual IDictionary<string, InstanceId> Roles
		 {
			 set
			 {
				  foreach ( InstanceId electedInstanceId in value.Values )
				  {
						Debug.Assert( _members.ContainsKey( electedInstanceId ) );
				  }
   
				  this._roles = new Dictionary<string, InstanceId>( value );
			 }
			 get
			 {
				  return _roles;
			 }
		 }

		 public virtual IEnumerable<InstanceId> MemberIds
		 {
			 get
			 {
				  return _members.Keys;
			 }
		 }


		 public virtual IList<URI> MemberURIs
		 {
			 get
			 {
				  return Iterables.asList( _members.Values );
			 }
		 }

		 public virtual string Name
		 {
			 get
			 {
				  return _name;
			 }
		 }


		 public virtual void Left()
		 {
			  this._members = new Dictionary<InstanceId, URI>();
			  _roles = new Dictionary<string, InstanceId>();
		 }

		 public virtual void RemoveElected( string roleName )
		 {
			  IDictionary<string, InstanceId> newRoles = new Dictionary<string, InstanceId>( _roles );
			  InstanceId removed = newRoles.Remove( roleName );
			  _roles = newRoles;
			  _log.info( "Removed role " + roleName + " from instance " + removed );
		 }

		 public virtual InstanceId GetElected( string roleName )
		 {
			  return _roles[roleName];
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Iterable<String> getRolesOf(final Neo4Net.cluster.InstanceId node)
		 public virtual IEnumerable<string> GetRolesOf( InstanceId node )
		 {
			  return Iterables.map( DictionaryEntry.getKey, Iterables.filter( item => item.Value.Equals( node ), _roles.SetOfKeyValuePairs() ) );
		 }

		 public virtual URI GetUriForId( InstanceId node )
		 {
			  return _members[node];
		 }

		 public virtual InstanceId GetIdForUri( URI fromUri )
		 {
			  foreach ( KeyValuePair<InstanceId, URI> serverIdURIEntry in _members.SetOfKeyValuePairs() )
			  {
					if ( serverIdURIEntry.Value.Equals( fromUri ) )
					{
						 return serverIdURIEntry.Key;
					}
			  }
			  return null;
		 }

		 public virtual ClusterConfiguration Snapshot( Log log )
		 {
			  return new ClusterConfiguration( this, log );
		 }

		 public override string ToString()
		 {
			  return "Name:" + _name + " Nodes:" + _members + " Roles:" + _roles;
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

			  ClusterConfiguration that = ( ClusterConfiguration ) o;

//JAVA TO C# CONVERTER WARNING: LINQ 'SequenceEqual' is not always identical to Java AbstractList 'equals':
//ORIGINAL LINE: if (!candidateMembers.equals(that.candidateMembers))
			  if ( !_candidateMembers.SequenceEqual( that._candidateMembers ) )
			  {
					return false;
			  }
			  if ( !_members.Equals( that._members ) )
			  {
					return false;
			  }
			  if ( !_name.Equals( that._name ) )
			  {
					return false;
			  }
			  return _roles.Equals( that._roles );
		 }

		 public override int GetHashCode()
		 {
			  int result = _name.GetHashCode();
			  result = 31 * result + _candidateMembers.GetHashCode();
			  result = 31 * result + _members.GetHashCode();
			  result = 31 * result + _roles.GetHashCode();
			  return result;
		 }
	}

}