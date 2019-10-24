using System.Collections.Generic;

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
namespace Neo4Net.Values
{

	using VirtualValueGroup = Neo4Net.Values.@virtual.VirtualValueGroup;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.NO_VALUE;

	/// <summary>
	/// Value that can exist transiently during computations, but that cannot be stored as a property value. A Virtual
	/// Value could be a NodeReference for example.
	/// </summary>
	public abstract class VirtualValue : AnyValue
	{
		 public override bool Eq( object other )
		 {
			  if ( other == null )
			  {
					return false;
			  }

			  if ( other is SequenceValue && this.SequenceValue )
			  {
					return ( ( SequenceValue ) this ).Equals( ( SequenceValue ) other );
			  }
			  return other is VirtualValue && Equals( ( VirtualValue ) other );
		 }

		 public abstract boolean ( VirtualValue other );

		 public override bool? TernaryEquals( AnyValue other )
		 {
			  if ( other == null || other == NO_VALUE )
			  {
					return null;
			  }
			  if ( other is SequenceValue && this.SequenceValue )
			  {
					return ( ( SequenceValue ) this ).TernaryEquality( ( SequenceValue ) other );
			  }
			  if ( other is VirtualValue && ( ( VirtualValue ) other ).ValueGroup() == ValueGroup() )
			  {
					return Equals( ( VirtualValue ) other );
			  }
			  return false;
		 }

		 public abstract VirtualValueGroup ValueGroup();

		 public abstract int CompareTo( VirtualValue other, IComparer<AnyValue> comparator );
	}

}