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
namespace Neo4Net.Kernel.Impl.Index.Schema.fusion
{

	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.ArrayUtils.contains;

	/// <summary>
	/// Given a set of values selects a slot to use.
	/// </summary>
	public interface SlotSelector
	{

		 void ValidateSatisfied( InstanceSelector<IndexProvider> instances );

		 /// <summary>
		 /// Selects a slot to use based on the given values. The values can be anything that can yield a <seealso cref="ValueGroup value group"/>,
		 /// which is what the {@code groupOf} function extracts from each value.
		 /// </summary>
		 /// @param <V> type of value to extract <seealso cref="ValueGroup"/> from. </param>
		 /// <param name="values"> values, something which can yield a <seealso cref="ValueGroup"/>. </param>
		 /// <param name="groupOf"> <seealso cref="Function"/> to get <seealso cref="ValueGroup"/> for the given values. </param>
		 /// <returns> <seealso cref="IndexSlot"/> or {@code null} if no single slot could be selected. This means that all slots are needed. </returns>
		 IndexSlot selectSlot<V>( V[] values, System.Func<V, ValueGroup> groupOf );

		 /// <summary>
		 /// Standard utility method for typical implementation of <seealso cref="SlotSelector.validateSatisfied(InstanceSelector)"/>.
		 /// </summary>
		 /// <param name="instances"> instances to validate. </param>
		 /// <param name="aliveIndex"> slots to ensure have been initialized with non-empty instances. </param>
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static void validateSelectorInstances(InstanceSelector<org.Neo4Net.kernel.api.index.IndexProvider> instances, IndexSlot... aliveIndex)
	//	 {
	//		  for (IndexSlot indexSlot : IndexSlot.values())
	//		  {
	//				boolean expected = contains(aliveIndex, indexSlot);
	//				boolean actual = instances.select(indexSlot) != IndexProvider.EMPTY;
	//				if (expected != actual)
	//				{
	//					 throw new IllegalArgumentException(String.format("Only indexes expected to be separated from IndexProvider.EMPTY are %s but was %s", Arrays.toString(aliveIndex), instances));
	//				}
	//		  }
	//	 }
	}

	public static class SlotSelector_Fields
	{
		 public static readonly SlotSelector NullInstance = new SlotSelector_NullInstance();
	}

	 public class SlotSelector_NullInstance : SlotSelector
	 {
		  public override void ValidateSatisfied( InstanceSelector<IndexProvider> instances )
		  { // no-op
		  }

		  public override IndexSlot SelectSlot<V>( V[] values, System.Func<V, ValueGroup> groupOf )
		  {
				throw new System.NotSupportedException( "NullInstance cannot select a slot for you. Please use the real deal." );
		  }
	 }

}