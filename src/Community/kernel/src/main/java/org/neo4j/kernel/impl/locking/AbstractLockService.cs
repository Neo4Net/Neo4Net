using System.Text;

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
namespace Neo4Net.Kernel.impl.locking
{
	/// <summary>
	/// Abstract implementation to keep all the boiler-plate code separate from actual locking logic.
	/// 
	/// Diagram of how classes inter-relate:
	/// <pre>
	/// (<seealso cref="LockService"/>) --------[returns]--------> (<seealso cref="Lock"/>)
	///  ^                                            ^
	///  |                                            |
	///  [implements]                                 [extends]
	///  |                                            |
	/// (<seealso cref="AbstractLockService"/>) -[contains]-> (<seealso cref="LockReference"/>) -[holds]-> (<seealso cref="LockedEntity"/>)
	///  ^      |                                     |                                     ^
	///  |      |                                     [references]                          |
	///  |      |                                     |                                     [extends]
	///  |      |                                     V                                     |
	///  |      +-----------[type param]-----------> (HANDLE)                              (<seealso cref="LockedNode"/>)
	///  |                                            ^
	///  [extends]                                    |
	///  |                                            [satisfies]
	///  |                                            |
	/// (<seealso cref="ReentrantLockService"/>)-[type param]->(<seealso cref="ReentrantLockService.OwnerQueueElement"/>)
	/// </pre>
	/// </summary>
	/// @param <HANDLE> A handle that the concrete implementation used for releasing the lock. </param>
	internal abstract class AbstractLockService<HANDLE> : LockService
	{
		public abstract Lock AcquireRelationshipLock( long relationshipId, LockService_LockType type );
		public abstract Lock AcquireNodeLock( long nodeId, LockService_LockType type );
		 public override Lock AcquireNodeLock( long nodeId, LockType type )
		 {
			  return Lock( new LockedNode( nodeId ) );
		 }

		 public override Lock AcquireRelationshipLock( long relationshipId, LockType type )
		 {
			  return Lock( new LockedRelationship( relationshipId ) );
		 }

		 private Lock Lock( LockedEntity key )
		 {
			  return new LockReference( this, key, Acquire( key ) );
		 }

		 protected internal abstract HANDLE Acquire( LockedEntity key );

		 protected internal abstract void Release( LockedEntity key, HANDLE handle );

		 protected internal abstract class LockedEntity
		 {
			  internal LockedEntity()
			  {
					// all instances defined in this class
			  }

			  public override sealed string ToString()
			  {
					StringBuilder repr = ( new StringBuilder( this.GetType().Name ) ).Append('[');
					ToString( repr );
					return repr.Append( ']' ).ToString();
			  }

			  internal abstract void ( StringBuilder repr );

			  public override abstract int ();

			  public override abstract boolean ( object obj );
		 }

		 private class LockReference : Lock
		 {
			 private readonly AbstractLockService<HANDLE> _outerInstance;

			  internal readonly LockedEntity Key;
			  internal HANDLE Handle;

			  internal LockReference( AbstractLockService<HANDLE> outerInstance, LockedEntity key, HANDLE handle )
			  {
				  this._outerInstance = outerInstance;
					this.Key = key;
					this.Handle = handle;
			  }

			  public override string ToString()
			  {
					StringBuilder repr = ( new StringBuilder( Key.GetType().Name ) ).Append('[');
					Key.ToString( repr );
					if ( Handle != default( HANDLE ) )
					{
						 repr.Append( "; HELD_BY=" ).Append( Handle );
					}
					else
					{
						 repr.Append( "; RELEASED" );
					}
					return repr.Append( ']' ).ToString();
			  }

			  public override void Release()
			  {
					if ( Handle == default( HANDLE ) )
					{
						 return;
					}
					try
					{
						 _outerInstance.release( Key, Handle );
					}
					finally
					{
						 Handle = default( HANDLE );
					}
			  }
		 }

		 internal class LockedPropertyContainer : LockedEntity
		 {
			  internal readonly long Id;

			  internal LockedPropertyContainer( long id )
			  {
					this.Id = id;
			  }

			  internal override void ToString( StringBuilder repr )
			  {
					repr.Append( "id=" ).Append( Id );
			  }

			  public override int GetHashCode()
			  {
					return ( int )( Id ^ ( ( long )( ( ulong )Id >> 32 ) ) );
			  }

			  public override bool Equals( object obj )
			  {
					if ( obj != null && obj.GetType().Equals(this.GetType()) )
					{
						 LockedPropertyContainer that = ( LockedPropertyContainer ) obj;
						 return this.Id == that.Id;
					}
					return false;
			  }
		 }

		 internal sealed class LockedNode : LockedPropertyContainer
		 {
			  internal LockedNode( long nodeId ) : base( nodeId )
			  {
			  }
		 }

		 internal sealed class LockedRelationship : LockedPropertyContainer
		 {
			  internal LockedRelationship( long relationshipId ) : base( relationshipId )
			  {
			  }
		 }
	}

}