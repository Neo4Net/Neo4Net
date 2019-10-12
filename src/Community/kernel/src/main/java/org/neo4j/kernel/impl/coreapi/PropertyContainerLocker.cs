using System.Threading;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.impl.coreapi
{
	using Lock = Neo4Net.Graphdb.Lock;
	using Node = Neo4Net.Graphdb.Node;
	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Statement = Neo4Net.Kernel.api.Statement;

	/// <summary>
	/// Manages user-facing locks.
	/// </summary>
	public class PropertyContainerLocker
	{
		 public virtual Lock ExclusiveLock( KernelTransaction ktx, PropertyContainer container )
		 {
			  using ( Statement ignore = ktx.AcquireStatement() )
			  {
					if ( container is Node )
					{
						 long id = ( ( Node ) container ).Id;
						 ktx.Locks().acquireExclusiveNodeLock(id);
						 return new CoreAPILock( () => ktx.Locks().releaseExclusiveNodeLock(id) );
					}
					else if ( container is Relationship )
					{
						 long id = ( ( Relationship ) container ).Id;
						 ktx.Locks().acquireExclusiveRelationshipLock(id);
						 return new CoreAPILock( () => ktx.Locks().releaseExclusiveRelationshipLock(id) );
					}
					else
					{
						 throw new System.NotSupportedException( "Only relationships and nodes can be locked." );
					}
			  }
		 }

		 public virtual Lock SharedLock( KernelTransaction ktx, PropertyContainer container )
		 {
			  using ( Statement ignore = ktx.AcquireStatement() )
			  {
					if ( container is Node )
					{
						 long id = ( ( Node ) container ).Id;
						 ktx.Locks().acquireSharedNodeLock(id);
						 return new CoreAPILock( () => ktx.Locks().releaseSharedNodeLock(id) );
					}
					else if ( container is Relationship )
					{
						 long id = ( ( Relationship ) container ).Id;
						 ktx.Locks().acquireSharedRelationshipLock(id);
						 return new CoreAPILock( () => ktx.Locks().releaseSharedRelationshipLock(id) );
					}
					else
					{
						 throw new System.NotSupportedException( "Only relationships and nodes can be locked." );
					}
			  }
		 }

		 private class CoreAPILock : Lock
		 {
			  internal bool Released;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly ThreadStart ReleaseConflict;

			  internal CoreAPILock( ThreadStart release )
			  {
					this.ReleaseConflict = release;
			  }

			  public override void Release()
			  {
					if ( Released )
					{
						 throw new System.InvalidOperationException( "Already released" );
					}
					Released = true;
					ReleaseConflict.run();
			  }
		 }

	}

}