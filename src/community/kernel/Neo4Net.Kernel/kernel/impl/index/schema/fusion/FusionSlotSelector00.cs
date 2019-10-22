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
namespace Neo4Net.Kernel.Impl.Index.Schema.fusion
{

	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.fusion.IndexSlot.LUCENE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.fusion.IndexSlot.SPATIAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.fusion.IndexSlot.TEMPORAL;

	/// <summary>
	/// Selector for index provider "lucene-1.x".
	/// The version name "00" comes from lucene-1.x originally not being a fusion index.
	/// </summary>
	public class FusionSlotSelector00 : SlotSelector
	{
		 public override void ValidateSatisfied( InstanceSelector<IndexProvider> instances )
		 {
			  SlotSelector.validateSelectorInstances( instances, LUCENE, SPATIAL, TEMPORAL );
		 }

		 public override IndexSlot SelectSlot<V>( V[] values, System.Func<V, ValueGroup> groupOf )
		 {
			  if ( values.Length > 1 )
			  {
					return LUCENE;
			  }

			  ValueGroup singleGroup = groupOf( values[0] );
			  switch ( singleGroup.category() )
			  {
			  case GEOMETRY:
					return SPATIAL;
			  case TEMPORAL:
					return TEMPORAL;
			  case UNKNOWN:
					return null;
			  default:
					return LUCENE;
			  }
		 }
	}

}