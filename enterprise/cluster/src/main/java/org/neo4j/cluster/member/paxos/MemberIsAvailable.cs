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
namespace Org.Neo4j.cluster.member.paxos
{

	using StoreId = Org.Neo4j.Storageengine.Api.StoreId;

	/// <summary>
	/// This message is broadcast when a member of the cluster declares that
	/// it is ready to serve a particular role for the cluster.
	/// </summary>
	public class MemberIsAvailable : Externalizable
	{
		 private string _role;
		 private InstanceId _instanceId;
		 private URI _clusterUri;
		 private URI _roleUri;
		 private StoreId _storeId;

		 public MemberIsAvailable()
		 {
		 }

		 public MemberIsAvailable( string role, InstanceId instanceId, URI clusterUri, URI roleUri, StoreId storeId )
		 {
			  this._role = role;
			  this._instanceId = instanceId;
			  this._clusterUri = clusterUri;
			  this._roleUri = roleUri;
			  this._storeId = storeId;
		 }

		 public virtual string Role
		 {
			 get
			 {
				  return _role;
			 }
		 }

		 public virtual InstanceId InstanceId
		 {
			 get
			 {
				  return _instanceId;
			 }
		 }

		 public virtual URI ClusterUri
		 {
			 get
			 {
				  return _clusterUri;
			 }
		 }

		 public virtual URI RoleUri
		 {
			 get
			 {
				  return _roleUri;
			 }
		 }

		 public virtual StoreId StoreId
		 {
			 get
			 {
				  return _storeId;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeExternal(java.io.ObjectOutput out) throws java.io.IOException
		 public override void WriteExternal( ObjectOutput @out )
		 {
			  @out.writeUTF( _role );
			  @out.writeObject( _instanceId );
			  @out.writeUTF( _clusterUri.ToString() );
			  @out.writeUTF( _roleUri.ToString() );
			  _storeId.writeExternal( @out );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void readExternal(java.io.ObjectInput in) throws java.io.IOException, ClassNotFoundException
		 public override void ReadExternal( ObjectInput @in )
		 {
			  _role = @in.readUTF();
			  _instanceId = ( InstanceId ) @in.readObject();
			  _clusterUri = URI.create( @in.readUTF() );
			  _roleUri = URI.create( @in.readUTF() );
			  // if MemberIsAvailable message comes from old instance than we can't read storeId
			  try
			  {
					_storeId = StoreId.from( @in );
			  }
			  catch ( IOException )
			  {
					_storeId = StoreId.DEFAULT;
			  }
		 }

		 public override string ToString()
		 {
			  return string.Format( "MemberIsAvailable[ Role: {0}, InstanceId: {1}, Role URI: {2}, Cluster URI: {3}]", _role, _instanceId.ToString(), _roleUri.ToString(), _clusterUri.ToString() );
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

			  MemberIsAvailable that = ( MemberIsAvailable ) o;

			  if ( !_clusterUri.Equals( that._clusterUri ) )
			  {
					return false;
			  }
			  if ( !_instanceId.Equals( that._instanceId ) )
			  {
					return false;
			  }
			  if ( !_role.Equals( that._role ) )
			  {
					return false;
			  }
			  return _roleUri.Equals( that._roleUri );
		 }

		 public override int GetHashCode()
		 {
			  int result = _role.GetHashCode();
			  result = 31 * result + _instanceId.GetHashCode();
			  result = 31 * result + _clusterUri.GetHashCode();
			  result = 31 * result + _roleUri.GetHashCode();
			  return result;
		 }
	}

}