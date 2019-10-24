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
namespace Neo4Net.Kernel.ha.cluster.member
{

	using OnlineBackupKernelExtension = Neo4Net.backup.OnlineBackupKernelExtension;
	using InstanceId = Neo4Net.cluster.InstanceId;
	using MapUtil = Neo4Net.Collections.Helpers.MapUtil;
	using HighAvailabilityModeSwitcher = Neo4Net.Kernel.ha.cluster.modeswitch.HighAvailabilityModeSwitcher;
	using StoreId = Neo4Net.Kernel.Api.StorageEngine.StoreId;

	public class ClusterMember
	{
		 private readonly InstanceId _instanceId;
		 private readonly IDictionary<string, URI> _roles;
		 private readonly StoreId _storeId;
		 private readonly bool _alive;

		 public ClusterMember( InstanceId instanceId ) : this( instanceId, Collections.emptyMap(), StoreId.DEFAULT, true )
		 {
		 }

		 public ClusterMember( InstanceId instanceId, IDictionary<string, URI> roles, StoreId storeId, bool alive )
		 {
			  this._instanceId = instanceId;
			  this._roles = roles;
			  this._storeId = storeId;
			  this._alive = alive;
		 }

		 public virtual InstanceId InstanceId
		 {
			 get
			 {
				  return _instanceId;
			 }
		 }

		 public virtual URI HAUri
		 {
			 get
			 {
				  URI haURI = _roles[HighAvailabilityModeSwitcher.MASTER];
				  if ( haURI == null )
				  {
						haURI = _roles[HighAvailabilityModeSwitcher.SLAVE];
				  }
				  return haURI;
			 }
		 }

		 public virtual string HARole
		 {
			 get
			 {
				  if ( _roles.ContainsKey( HighAvailabilityModeSwitcher.MASTER ) )
				  {
						return HighAvailabilityModeSwitcher.MASTER;
				  }
				  if ( _roles.ContainsKey( HighAvailabilityModeSwitcher.SLAVE ) )
				  {
						return HighAvailabilityModeSwitcher.SLAVE;
				  }
				  return HighAvailabilityModeSwitcher.UNKNOWN;
			 }
		 }

		 public virtual bool HasRole( string role )
		 {
			  return _roles.ContainsKey( role );
		 }

		 public virtual IEnumerable<string> Roles
		 {
			 get
			 {
				  return _roles.Keys;
			 }
		 }

		 public virtual IEnumerable<URI> RoleURIs
		 {
			 get
			 {
				  return _roles.Values;
			 }
		 }

		 public virtual StoreId StoreId
		 {
			 get
			 {
				  return _storeId;
			 }
		 }

		 public virtual bool Alive
		 {
			 get
			 {
				  return _alive;
			 }
		 }

		 internal virtual ClusterMember AvailableAs( string role, URI roleUri, StoreId storeId )
		 {
			  IDictionary<string, URI> copy = new Dictionary<string, URI>( _roles );
			  if ( role.Equals( HighAvailabilityModeSwitcher.MASTER ) )
			  {
					copy.Remove( HighAvailabilityModeSwitcher.SLAVE );
			  }
			  else if ( role.Equals( HighAvailabilityModeSwitcher.SLAVE ) )
			  {
					copy.Remove( HighAvailabilityModeSwitcher.MASTER );
					copy.Remove( OnlineBackupKernelExtension.BACKUP );
			  }
			  copy[role] = roleUri;
			  return new ClusterMember( this._instanceId, copy, storeId, this._alive );
		 }

		 internal virtual ClusterMember Unavailable()
		 {
			  return new ClusterMember( this._instanceId, Collections.emptyMap(), _storeId, this._alive );
		 }

		 internal virtual ClusterMember UnavailableAs( string role )
		 {
			  return new ClusterMember( this._instanceId, MapUtil.copyAndRemove( _roles, role ), this._storeId, this._alive );
		 }

		 internal virtual ClusterMember Alive()
		 {
			  return new ClusterMember( this._instanceId, _roles, _storeId, true );
		 }

		 internal virtual ClusterMember Failed()
		 {
			  return new ClusterMember( this._instanceId, _roles, _storeId, false );
		 }

		 public override string ToString()
		 {
			  return "ClusterMember{" +
						"instanceId=" + _instanceId +
						", roles=" + _roles +
						", storeId=" + _storeId +
						", alive=" + _alive +
						'}';
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
			  ClusterMember that = ( ClusterMember ) o;
			  return _instanceId.Equals( that._instanceId );
		 }

		 public override int GetHashCode()
		 {
			  return _instanceId.GetHashCode();
		 }
	}

}