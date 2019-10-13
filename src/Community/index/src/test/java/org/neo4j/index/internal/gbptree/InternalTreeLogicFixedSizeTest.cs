﻿/*
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
namespace Neo4Net.Index.@internal.gbptree
{
	using MutableLong = org.apache.commons.lang3.mutable.MutableLong;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.SimpleLongLayout.longLayout;

	public class InternalTreeLogicFixedSizeTest : InternalTreeLogicTestBase<MutableLong, MutableLong>
	{
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 internal new SimpleLongLayout LayoutConflict = longLayout().build();

		 protected internal override ValueMerger<MutableLong, MutableLong> Adder
		 {
			 get
			 {
				  return ( existingKey, newKey, @base, add ) =>
				  {
					@base.add( add.longValue() );
					return @base;
				  };
			 }
		 }

		 protected internal override TreeNode<MutableLong, MutableLong> GetTreeNode( int pageSize, Layout<MutableLong, MutableLong> layout )
		 {
			  return new TreeNodeFixedSize<MutableLong, MutableLong>( pageSize, layout );
		 }

		 protected internal override TestLayout<MutableLong, MutableLong> Layout
		 {
			 get
			 {
				  return LayoutConflict;
			 }
		 }
	}

}