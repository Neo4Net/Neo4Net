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

	public sealed class NodeKey : CountsKey
	{
		 private readonly int _labelId;

		 internal NodeKey( int labelId )
		 {
			  this._labelId = labelId;
		 }

		 public int LabelId
		 {
			 get
			 {
				  return _labelId;
			 }
		 }

		 public override string ToString()
		 {
			  return string.Format( "NodeKey[({0})]", label( _labelId ) );
		 }

		 public override void Accept( CountsVisitor visitor, long ignored, long count )
		 {
			  visitor.VisitNodeCount( _labelId, count );
		 }

		 public override CountsKeyType RecordType()
		 {
			  return CountsKeyType.EntityNode;
		 }

		 public override int GetHashCode()
		 {
			  int result = _labelId;
			  result = 31 * result + RecordType().GetHashCode();
			  return result;
		 }

		 public override bool Equals( object o )
		 {
			  return this == o || ( o is Neo4Net.Kernel.impl.store.counts.keys.NodeKey ) && _labelId == ( ( Neo4Net.Kernel.impl.store.counts.keys.NodeKey ) o )._labelId;
		 }

		 public override int CompareTo( CountsKey other )
		 {
			  if ( other is Neo4Net.Kernel.impl.store.counts.keys.NodeKey )
			  {
					Neo4Net.Kernel.impl.store.counts.keys.NodeKey that = ( Neo4Net.Kernel.impl.store.counts.keys.NodeKey ) other;
					return Integer.compare( this._labelId, that._labelId );
			  }
			  return RecordType().compareTo(other.RecordType());
		 }
	}

}