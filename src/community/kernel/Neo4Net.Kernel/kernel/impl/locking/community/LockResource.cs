/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Kernel.impl.locking.community
{
	using MathUtil = Neo4Net.Helpers.MathUtil;
	using ResourceType = Neo4Net.Kernel.Api.StorageEngine.@lock.ResourceType;

	public class LockResource
	{
		 private readonly ResourceType _resourceType;
		 private readonly long _resourceId;

		 /// <summary>
		 /// Local reference count, used for each client to count references to a lock. </summary>
		 private int _refCount = 1;

		 public LockResource( ResourceType resourceType, long resourceId )
		 {
			  this._resourceType = resourceType;
			  this._resourceId = resourceId;
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

			  LockResource that = ( LockResource ) o;
			  return _resourceId == that._resourceId && _resourceType.Equals( that._resourceType );

		 }

		 public override int GetHashCode()
		 {
			  int result = _resourceType.GetHashCode();
			  result = 31 * result + ( int )( _resourceId ^ ( ( long )( ( ulong )_resourceId >> 32 ) ) );
			  return result;
		 }

		 public override string ToString()
		 {
			  return string.Format( "{0}({1:D})", _resourceType, _resourceId );
		 }

		 public virtual void AcquireReference()
		 {
			  _refCount = Math.incrementExact( _refCount );
		 }

		 public virtual int ReleaseReference()
		 {
			  return _refCount = MathUtil.decrementExactNotPastZero( _refCount );
		 }

		 public virtual long ResourceId()
		 {
			  return _resourceId;
		 }

		 public virtual ResourceType Type()
		 {
			  return _resourceType;
		 }
	}

}