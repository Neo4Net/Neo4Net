﻿using System;

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
namespace Neo4Net.Index.@internal.gbptree
{
	using MutableLong = org.apache.commons.lang3.mutable.MutableLong;

	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.SimpleLongLayout.longLayout;

	public class GBPTreeFixedSizeIT : GBPTreeITBase<MutableLong, MutableLong>
	{
		 protected internal override TestLayout<MutableLong, MutableLong> GetLayout( RandomRule random )
		 {
			  return longLayout().withKeyPadding(random.IntBetween(0, 10)).build();
		 }

		 internal override Type<MutableLong> KeyClass
		 {
			 get
			 {
				  return typeof( MutableLong );
			 }
		 }
	}

}