using System;

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
namespace Org.Neo4j.Kernel.impl.store.record
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.Record.NO_NEXT_PROPERTY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.Record.NO_NEXT_RELATIONSHIP;

	public class RelationshipRecord : PrimitiveRecord
	{
		 private long _firstNode;
		 private long _secondNode;
		 private int _type;
		 private long _firstPrevRel;
		 private long _firstNextRel;
		 private long _secondPrevRel;
		 private long _secondNextRel;
		 private bool _firstInFirstChain;
		 private bool _firstInSecondChain;

		 [Obsolete]
		 public RelationshipRecord( long id, long firstNode, long secondNode, int type ) : this( id )
		 {
			  this._firstNode = firstNode;
			  this._secondNode = secondNode;
			  this._type = type;
		 }

		 [Obsolete]
		 public RelationshipRecord( long id, bool inUse, long firstNode, long secondNode, int type, long firstPrevRel, long firstNextRel, long secondPrevRel, long secondNextRel, bool firstInFirstChain, bool firstInSecondChain ) : this( id, firstNode, secondNode, type )
		 {
			  InUse = inUse;
			  this._firstPrevRel = firstPrevRel;
			  this._firstNextRel = firstNextRel;
			  this._secondPrevRel = secondPrevRel;
			  this._secondNextRel = secondNextRel;
			  this._firstInFirstChain = firstInFirstChain;
			  this._firstInSecondChain = firstInSecondChain;

		 }

		 public RelationshipRecord( long id ) : base( id )
		 {
		 }

		 public virtual RelationshipRecord Initialize( bool inUse, long nextProp, long firstNode, long secondNode, int type, long firstPrevRel, long firstNextRel, long secondPrevRel, long secondNextRel, bool firstInFirstChain, bool firstInSecondChain )
		 {
			  base.Initialize( inUse, nextProp );
			  this._firstNode = firstNode;
			  this._secondNode = secondNode;
			  this._type = type;
			  this._firstPrevRel = firstPrevRel;
			  this._firstNextRel = firstNextRel;
			  this._secondPrevRel = secondPrevRel;
			  this._secondNextRel = secondNextRel;
			  this._firstInFirstChain = firstInFirstChain;
			  this._firstInSecondChain = firstInSecondChain;
			  return this;
		 }

		 public override void Clear()
		 {
			  Initialize( false, NO_NEXT_PROPERTY.intValue(), -1, -1, -1, 1, NO_NEXT_RELATIONSHIP.intValue(), 1, NO_NEXT_RELATIONSHIP.intValue(), true, true );
		 }

		 public virtual void SetLinks( long firstNode, long secondNode, int type )
		 {
			  this._firstNode = firstNode;
			  this._secondNode = secondNode;
			  this._type = type;
		 }

		 public virtual long FirstNode
		 {
			 get
			 {
				  return _firstNode;
			 }
			 set
			 {
				  this._firstNode = value;
			 }
		 }


		 public virtual long SecondNode
		 {
			 get
			 {
				  return _secondNode;
			 }
			 set
			 {
				  this._secondNode = value;
			 }
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


		 public virtual long FirstPrevRel
		 {
			 get
			 {
				  return _firstPrevRel;
			 }
			 set
			 {
				  this._firstPrevRel = value;
			 }
		 }


		 public virtual long FirstNextRel
		 {
			 get
			 {
				  return _firstNextRel;
			 }
			 set
			 {
				  this._firstNextRel = value;
			 }
		 }


		 public virtual long SecondPrevRel
		 {
			 get
			 {
				  return _secondPrevRel;
			 }
			 set
			 {
				  this._secondPrevRel = value;
			 }
		 }


		 public virtual long SecondNextRel
		 {
			 get
			 {
				  return _secondNextRel;
			 }
			 set
			 {
				  this._secondNextRel = value;
			 }
		 }


		 public virtual bool FirstInFirstChain
		 {
			 get
			 {
				  return _firstInFirstChain;
			 }
			 set
			 {
				  this._firstInFirstChain = value;
			 }
		 }


		 public virtual bool FirstInSecondChain
		 {
			 get
			 {
				  return _firstInSecondChain;
			 }
			 set
			 {
				  this._firstInSecondChain = value;
			 }
		 }


		 public override string ToString()
		 {
			  return "Relationship[" + Id +
						",used=" + InUse() +
						",source=" + _firstNode +
						",target=" + _secondNode +
						",type=" + _type +
						( _firstInFirstChain ? ",sCount=" : ",sPrev=" ) + _firstPrevRel +
						",sNext=" + _firstNextRel +
						( _firstInSecondChain ? ",tCount=" : ",tPrev=" ) + _secondPrevRel +
						",tNext=" + _secondNextRel +
						",prop=" + NextProp +
						",secondaryUnitId=" + SecondaryUnitId +
						( _firstInFirstChain ? ", sFirst" : ",!sFirst" ) + ( _firstInSecondChain ? ", tFirst" : ",!tFirst" ) + "]";
		 }

		 public override RelationshipRecord Clone()
		 {
			  RelationshipRecord record = ( new RelationshipRecord( Id ) ).initialize( InUse(), NextPropConflict, _firstNode, _secondNode, _type, _firstPrevRel, _firstNextRel, _secondPrevRel, _secondNextRel, _firstInFirstChain, _firstInSecondChain );
			  record.SecondaryUnitId = SecondaryUnitId;
			  return record;
		 }

		 public override PropertyRecord IdTo
		 {
			 set
			 {
				  value.RelId = Id;
			 }
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
			  RelationshipRecord that = ( RelationshipRecord ) o;
			  return _firstNode == that._firstNode && _secondNode == that._secondNode && _type == that._type && _firstPrevRel == that._firstPrevRel && _firstNextRel == that._firstNextRel && _secondPrevRel == that._secondPrevRel && _secondNextRel == that._secondNextRel && _firstInFirstChain == that._firstInFirstChain && _firstInSecondChain == that._firstInSecondChain;
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( base.GetHashCode(), _firstNode, _secondNode, _type, _firstPrevRel, _firstNextRel, _secondPrevRel, _secondNextRel, _firstInFirstChain, _firstInSecondChain );
		 }
	}

}