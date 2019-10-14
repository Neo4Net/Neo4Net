using System;

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
namespace Neo4Net.Kernel.impl.store.record
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.Record.NULL_REFERENCE;

	public class RelationshipGroupRecord : AbstractBaseRecord
	{
		 private int _type;
		 private long _next;
		 private long _firstOut;
		 private long _firstIn;
		 private long _firstLoop;
		 private long _owningNode;

		 // Not stored, just kept in memory temporarily when loading the group chain
		 private long _prev;

		 [Obsolete]
		 public RelationshipGroupRecord( long id, int type ) : base( id )
		 {
			  this._type = type;
		 }

		 [Obsolete]
		 public RelationshipGroupRecord( long id, int type, long firstOut, long firstIn, long firstLoop, long owningNode, bool inUse ) : this( id, type, firstOut, firstIn, firstLoop, owningNode, NULL_REFERENCE.intValue(), inUse )
		 {
		 }

		 [Obsolete]
		 public RelationshipGroupRecord( long id, int type, long firstOut, long firstIn, long firstLoop, long owningNode, long next, bool inUse ) : base( id )
		 {
			  InUse = inUse;
			  this._type = type;
			  this._firstOut = firstOut;
			  this._firstIn = firstIn;
			  this._firstLoop = firstLoop;
			  this._owningNode = owningNode;
			  this._next = next;
		 }

		 public RelationshipGroupRecord( long id ) : base( id )
		 {
		 }

		 public virtual RelationshipGroupRecord Initialize( bool inUse, int type, long firstOut, long firstIn, long firstLoop, long owningNode, long next )
		 {
			  base.Initialize( inUse );
			  this._type = type;
			  this._firstOut = firstOut;
			  this._firstIn = firstIn;
			  this._firstLoop = firstLoop;
			  this._owningNode = owningNode;
			  this._next = next;
			  this._prev = NULL_REFERENCE.intValue();
			  return this;
		 }

		 public override void Clear()
		 {
			  Initialize( false, NULL_REFERENCE.intValue(), NULL_REFERENCE.intValue(), NULL_REFERENCE.intValue(), NULL_REFERENCE.intValue(), NULL_REFERENCE.intValue(), NULL_REFERENCE.intValue() );
			  _prev = NULL_REFERENCE.intValue();
		 }

		 public virtual int Type
		 {
			 get
			 {
				  return _type;
			 }
			 set
			 {
				  this._type = value;
			 }
		 }


		 public virtual long FirstOut
		 {
			 get
			 {
				  return _firstOut;
			 }
			 set
			 {
				  this._firstOut = value;
			 }
		 }


		 public virtual long FirstIn
		 {
			 get
			 {
				  return _firstIn;
			 }
			 set
			 {
				  this._firstIn = value;
			 }
		 }


		 public virtual long FirstLoop
		 {
			 get
			 {
				  return _firstLoop;
			 }
			 set
			 {
				  this._firstLoop = value;
			 }
		 }


		 public virtual long Next
		 {
			 get
			 {
				  return _next;
			 }
			 set
			 {
				  this._next = value;
			 }
		 }


		 /// <summary>
		 /// The previous pointer, i.e. previous group in this chain of groups isn't
		 /// persisted in the store, but only set during reading of the group
		 /// chain. </summary>
		 /// <param name="prev"> the id of the previous group in this chain. </param>
		 public virtual long Prev
		 {
			 set
			 {
				  this._prev = value;
			 }
			 get
			 {
				  return _prev;
			 }
		 }


		 public virtual long OwningNode
		 {
			 get
			 {
				  return _owningNode;
			 }
			 set
			 {
				  this._owningNode = value;
			 }
		 }


		 public override string ToString()
		 {
			  return "RelationshipGroup[" + Id +
						",type=" + _type +
						",out=" + _firstOut +
						",in=" + _firstIn +
						",loop=" + _firstLoop +
						",prev=" + _prev +
						",next=" + _next +
						",used=" + InUse() +
						",owner=" + OwningNode +
						",secondaryUnitId=" + SecondaryUnitId + "]";
		 }

		 public override RelationshipGroupRecord Clone()
		 {
			  RelationshipGroupRecord clone = ( new RelationshipGroupRecord( Id ) ).initialize( InUse(), _type, _firstOut, _firstIn, _firstLoop, _owningNode, _next );
			  clone.SecondaryUnitId = SecondaryUnitId;
			  return clone;
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
			  if ( !base.Equals( o ) )
			  {
					return false;
			  }
			  RelationshipGroupRecord that = ( RelationshipGroupRecord ) o;
			  return _type == that._type && _next == that._next && _firstOut == that._firstOut && _firstIn == that._firstIn && _firstLoop == that._firstLoop && _owningNode == that._owningNode;
			  // don't compare prev since it's not persisted
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( base.GetHashCode(), _type, _next, _firstOut, _firstIn, _firstLoop, _owningNode, _prev );
		 }
	}

}