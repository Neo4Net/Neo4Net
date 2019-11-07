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
namespace Neo4Net.Kernel.impl.store.counts.keys
{
	using CountsVisitor = Neo4Net.Kernel.Impl.Api.CountsVisitor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.util.IdPrettyPrinter.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.util.IdPrettyPrinter.relationshipType;

	public sealed class RelationshipKey : CountsKey
	{
		 private readonly int _startLabelId;
		 private readonly int _typeId;
		 private readonly int _endLabelId;

		 internal RelationshipKey( int startLabelId, int typeId, int endLabelId )
		 {
			  this._startLabelId = startLabelId;
			  this._typeId = typeId;
			  this._endLabelId = endLabelId;
		 }

		 public int StartLabelId
		 {
			 get
			 {
				  return _startLabelId;
			 }
		 }

		 public int TypeId
		 {
			 get
			 {
				  return _typeId;
			 }
		 }

		 public int EndLabelId
		 {
			 get
			 {
				  return _endLabelId;
			 }
		 }

		 public override string ToString()
		 {
			  return string.Format( "RelationshipKey[({0})-{1}->({2})]", label( _startLabelId ), relationshipType( _typeId ), label( _endLabelId ) );
		 }

		 public override void Accept( CountsVisitor visitor, long ignored, long count )
		 {
			  visitor.VisitRelationshipCount( _startLabelId, _typeId, _endLabelId, count );
		 }

		 public override CountsKeyType RecordType()
		 {
			  return CountsKeyType.EntityRelationship;
		 }

		 public override int GetHashCode()
		 {
			  int result = _startLabelId;
			  result = 31 * result + _typeId;
			  result = 31 * result + _endLabelId;
			  result = 31 * result + RecordType().GetHashCode();
			  return result;
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o is RelationshipKey )
			  {
					RelationshipKey that = ( RelationshipKey ) o;
					return _endLabelId == that._endLabelId && _startLabelId == that._startLabelId && _typeId == that._typeId;
			  }
			  return false;
		 }

		 public override int CompareTo( CountsKey other )
		 {
			  if ( other is RelationshipKey )
			  {
					RelationshipKey that = ( RelationshipKey ) other;
					if ( this._typeId != that._typeId )
					{
						 return Integer.compare( this._typeId, that._typeId );
					}
					if ( this._startLabelId != that._startLabelId )
					{
						 return Integer.compare( this._startLabelId, that._startLabelId );
					}
					return Integer.compare( this._endLabelId, that._endLabelId );
			  }
			  return RecordType().compareTo(other.RecordType());
		 }
	}

}