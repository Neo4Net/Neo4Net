﻿using System.Collections.Generic;

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
namespace Neo4Net.Values.@virtual
{

	using Neo4Net.Values;

	public abstract class VirtualRelationshipValue : VirtualValue
	{
		 public abstract long Id();

		 public override int CompareTo( VirtualValue other, IComparer<AnyValue> comparator )
		 {
			  if ( !( other is VirtualRelationshipValue ) )
			  {
					throw new System.ArgumentException( "Cannot compare different virtual values" );
			  }

			  VirtualRelationshipValue otherNode = ( VirtualRelationshipValue ) other;
			  return Long.compare( Id(), otherNode.Id() );
		 }

		 public override int ComputeHash()
		 {
			  return Long.GetHashCode( Id() );
		 }

		 public override T Map<T>( ValueMapper<T> mapper )
		 {
			  return mapper.MapRelationship( this );
		 }

		 public override bool Equals( VirtualValue other )
		 {
			  if ( !( other is VirtualRelationshipValue ) )
			  {
					return false;
			  }
			  VirtualRelationshipValue that = ( VirtualRelationshipValue ) other;
			  return Id() == that.Id();
		 }

		 public override VirtualValueGroup ValueGroup()
		 {
			  return VirtualValueGroup.Edge;
		 }
	}

}