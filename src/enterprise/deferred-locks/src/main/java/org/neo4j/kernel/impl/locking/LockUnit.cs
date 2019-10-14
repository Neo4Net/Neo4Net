using System;

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
namespace Neo4Net.Kernel.impl.locking
{
	using ResourceType = Neo4Net.Storageengine.Api.@lock.ResourceType;

	/// <summary>
	/// Description of a lock that was deferred to commit time.
	/// </summary>
	public class LockUnit : IComparable<LockUnit>, ActiveLock
	{
		 private readonly ResourceType _resourceType;
		 private readonly long _resourceId;
		 private readonly bool _exclusive;

		 public LockUnit( ResourceType resourceType, long resourceId, bool exclusive )
		 {
			  this._resourceType = resourceType;
			  this._resourceId = resourceId;
			  this._exclusive = exclusive;
		 }

		 public override string Mode()
		 {
			  return _exclusive ? ActiveLock_Fields.EXCLUSIVE_MODE : ActiveLock_Fields.SHARED_MODE;
		 }

		 public override ResourceType ResourceType()
		 {
			  return _resourceType;
		 }

		 public override long ResourceId()
		 {
			  return _resourceId;
		 }

		 public virtual bool Exclusive
		 {
			 get
			 {
				  return _exclusive;
			 }
		 }

		 public override int GetHashCode()
		 {
			  const int prime = 31;
			  int result = 1;
			  result = prime * result + ( _exclusive ? 1231 : 1237 );
			  result = prime * result + ( int )( _resourceId ^ ( ( long )( ( ulong )_resourceId >> 32 ) ) );
			  result = prime * result + _resourceType.GetHashCode();
			  return result;
		 }

		 public override bool Equals( object obj )
		 {
			  if ( this == obj )
			  {
					return true;
			  }
			  if ( obj == null )
			  {
					return false;
			  }
			  if ( this.GetType() != obj.GetType() )
			  {
					return false;
			  }
			  LockUnit other = ( LockUnit ) obj;
			  if ( _exclusive != other._exclusive )
			  {
					return false;
			  }
			  if ( _resourceId != other._resourceId )
			  {
					return false;
			  }
			  else if ( _resourceType.typeId() != other._resourceType.typeId() )
			  {
					return false;
			  }
			  return true;
		 }

		 public override int CompareTo( LockUnit o )
		 {
			  // Exclusive locks go first to minimize amount of potential deadlocks
			  int exclusiveCompare = Boolean.compare( o._exclusive, _exclusive );
			  if ( exclusiveCompare != 0 )
			  {
					return exclusiveCompare;
			  }

			  // Then shared/exclusive locks are compared by resourceTypeId and then by resourceId
			  return _resourceType.typeId() == o._resourceType.typeId() ? Long.compare(_resourceId, o._resourceId) : Integer.compare(_resourceType.typeId(), o._resourceType.typeId());
		 }

		 public override string ToString()
		 {
			  return "Resource [resourceType=" + _resourceType + ", resourceId=" + _resourceId + ", exclusive=" + _exclusive + "]";
		 }
	}

}