using System;

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
namespace Neo4Net.management
{

	using Neo4Net.Collections.Helpers;

	/// <summary>
	/// This class captures the least amount of information available for a cluster member to any
	/// cluster participant.
	/// </summary>
	/// @deprecated high availability database/edition is deprecated in favour of causal clustering. It will be removed in next major release. 
	[Obsolete("high availability database/edition is deprecated in favour of causal clustering. It will be removed in next major release."), Serializable]
	public class ClusterMemberInfo
	{
		 private const long SERIAL_VERSION_UID = -514433972115185753L;

		 private string _instanceId;
		 private bool _available;
		 private bool _alive;
		 private string _haRole;
		 private string[] _uris;
		 private string[] _roles;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ConstructorProperties({ "instanceId", "available", "alive", "haRole", "uris", "roles" }) public ClusterMemberInfo(String instanceId, boolean available, boolean alive, String haRole, String[] uris, String[] roles)
		 public ClusterMemberInfo( string instanceId, bool available, bool alive, string haRole, string[] uris, string[] roles )
		 {
			  this._instanceId = instanceId;
			  this._available = available;
			  this._alive = alive;
			  this._haRole = haRole;
			  this._uris = uris;
			  this._roles = roles;
		 }

		 public virtual string InstanceId
		 {
			 get
			 {
				  return _instanceId;
			 }
		 }

		 public virtual bool Available
		 {
			 get
			 {
				  return _available;
			 }
		 }

		 public virtual bool Alive
		 {
			 get
			 {
				  return _alive;
			 }
		 }

		 public virtual string HaRole
		 {
			 get
			 {
				  return _haRole;
			 }
		 }

		 public virtual string[] Uris
		 {
			 get
			 {
				  return _uris;
			 }
		 }

		 public virtual string[] Roles
		 {
			 get
			 {
				  return _roles;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("boxing") public String toString()
		 public override string ToString()
		 {
			  return string.Format( "Neo4NetHaInstance[id={0},available={1},haRole={2},HA URI={3}]", _instanceId, _available, _haRole, Arrays.ToString( _uris ) );
		 }

		 public virtual Pair<Neo4NetManager, HighAvailability> Connect()
		 {
			  return Connect( null, null );
		 }

		 public virtual Pair<Neo4NetManager, HighAvailability> Connect( string username, string password )
		 {
			  URI address = null;
			  foreach ( string uri in _uris )
			  {
					if ( uri.StartsWith( "jmx", StringComparison.Ordinal ) )
					{
	//                address = uri;
					}
			  }
			  if ( address == null )
			  {
					throw new System.InvalidOperationException( "The instance does not have a public JMX server." );
			  }
			  Neo4NetManager manager = Neo4NetManager.Get( Url( address ), username, password, _instanceId );
			  return Pair.of( manager, manager.HighAvailabilityBean );
		 }

		 private JMXServiceURL Url( URI address )
		 {
			  try
			  {
					return new JMXServiceURL( address.toASCIIString() );
			  }
			  catch ( MalformedURLException )
			  {
					throw new System.InvalidOperationException( "The instance does not have a valid JMX server URL." );
			  }
		 }
	}

}