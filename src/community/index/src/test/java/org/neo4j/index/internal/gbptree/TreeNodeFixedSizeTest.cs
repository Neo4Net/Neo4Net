﻿/*
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
namespace Neo4Net.Index.Internal.gbptree
{
	using MutableLong = org.apache.commons.lang3.mutable.MutableLong;

	using PageCursor = Neo4Net.Io.pagecache.PageCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.index.Internal.gbptree.SimpleLongLayout.longLayout;

	public class TreeNodeFixedSizeTest : TreeNodeTestBase<MutableLong, MutableLong>
	{
		 private readonly SimpleLongLayout _layout = longLayout().build();

		 protected internal override TestLayout<MutableLong, MutableLong> Layout
		 {
			 get
			 {
				  return _layout;
			 }
		 }

		 protected internal override TreeNode<MutableLong, MutableLong> GetNode( int pageSize, Layout<MutableLong, MutableLong> layout )
		 {
			  return new TreeNodeFixedSize<MutableLong, MutableLong>( pageSize, layout );
		 }

		 internal override void AssertAdditionalHeader( PageCursor cursor, TreeNode<MutableLong, MutableLong> node, int pageSize )
		 { // no addition header
		 }
	}

}