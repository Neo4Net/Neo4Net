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
namespace Neo4Net.@internal.Kernel.Api.helpers
{

	internal class StubGroupCursor : RelationshipGroupCursor
	{
		 private int _offset;
		 private readonly GroupData[] _groups;
		 private bool _isClosed;

		 internal StubGroupCursor( params GroupData[] groups )
		 {
			  this._groups = groups;
			  this._offset = -1;
			  this._isClosed = false;
		 }

		 internal virtual void Rewind()
		 {
			  this._offset = -1;
			  this._isClosed = false;
		 }

		 public override Neo4Net.@internal.Kernel.Api.RelationshipGroupCursor_Position Suspend()
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override void Resume( Neo4Net.@internal.Kernel.Api.RelationshipGroupCursor_Position position )
		 {
			  throw new System.NotSupportedException( "not implemented" );
		 }

		 public override bool Next()
		 {
			  _offset++;
			  return _offset >= 0 && _offset < _groups.Length;
		 }

		 public override void Close()
		 {
			  _isClosed = true;
		 }

		 public override bool Closed
		 {
			 get
			 {
				  return _isClosed;
			 }
		 }

		 public override int Type()
		 {
			  return _groups[_offset].type;
		 }

		 public override int OutgoingCount()
		 {
			  return _groups[_offset].countOut;
		 }

		 public override int IncomingCount()
		 {
			  return _groups[_offset].countIn;
		 }

		 public override int LoopCount()
		 {
			  return _groups[_offset].countLoop;
		 }

		 public override void Outgoing( RelationshipTraversalCursor cursor )
		 {
			  ( ( StubRelationshipCursor ) cursor ).Read( _groups[_offset].@out );
		 }

		 public override void Incoming( RelationshipTraversalCursor cursor )
		 {
			  ( ( StubRelationshipCursor ) cursor ).Read( _groups[_offset].@in );
		 }

		 public override void Loops( RelationshipTraversalCursor cursor )
		 {
			  ( ( StubRelationshipCursor ) cursor ).Read( _groups[_offset].loop );
		 }

		 public override long OutgoingReference()
		 {
			  return _groups[_offset].@out;
		 }

		 public override long IncomingReference()
		 {
			  return _groups[_offset].@in;
		 }

		 public override long LoopsReference()
		 {
			  return _groups[_offset].loop;
		 }

		 internal class GroupData
		 {
			  internal readonly int Out;
			  internal readonly int In;
			  internal readonly int Loop;
			  internal readonly int Type;
			  internal int CountIn;
			  internal int CountOut;
			  internal int CountLoop;

			  internal GroupData( int @out, int @in, int loop, int type )
			  {
					this.Out = @out;
					this.In = @in;
					this.Loop = loop;
					this.Type = type;
			  }

			  internal virtual GroupData WithOutCount( int count )
			  {
					this.CountOut = count;
					return this;
			  }

			  internal virtual GroupData WithInCount( int count )
			  {
					this.CountIn = count;
					return this;
			  }

			  internal virtual GroupData WithLoopCount( int count )
			  {
					this.CountLoop = count;
					return this;
			  }
		 }
	}

}