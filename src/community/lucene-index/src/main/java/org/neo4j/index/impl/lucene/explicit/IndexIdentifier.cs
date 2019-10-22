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
namespace Neo4Net.Index.impl.lucene.@explicit
{
	using IndexEntityType = Neo4Net.Kernel.impl.index.IndexEntityType;

	internal class IndexIdentifier
	{
		 internal readonly IndexEntityType IEntityType;
		 internal readonly string IndexName;
		 private readonly int _hashCode;

		 internal IndexIdentifier( IndexEntityType IEntityType, string indexName )
		 {
			  this.EntityType = IEntityType;
			  this.IndexName = indexName;
			  this._hashCode = CalculateHashCode();
		 }

		 public override bool Equals( object o )
		 {
			  if ( o == null || !this.GetType().Equals(o.GetType()) )
			  {
					return false;
			  }
			  IndexIdentifier i = ( IndexIdentifier ) o;
			  return IEntityType == i.EntityType && IndexName.Equals( i.IndexName );
		 }

		 private int CalculateHashCode()
		 {
			  int code = 17;
			  code += 7 * IEntityType.GetHashCode();
			  code += 7 * IndexName.GetHashCode();
			  return code;
		 }

		 public override int GetHashCode()
		 {
			  return this._hashCode;
		 }

		 public override string ToString()
		 {
			  return "Index[" + IndexName + "," + IEntityType.nameToLowerCase() + "]";
		 }
	}

}