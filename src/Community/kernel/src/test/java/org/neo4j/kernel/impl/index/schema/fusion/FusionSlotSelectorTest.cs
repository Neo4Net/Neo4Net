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
namespace Neo4Net.Kernel.Impl.Index.Schema.fusion
{
	using Test = org.junit.jupiter.api.Test;

	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.IndexSlot.NUMBER;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.fusion.SlotSelector.validateSelectorInstances;

	internal class FusionSlotSelectorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void throwIfToFewInstances()
		 internal virtual void ThrowIfToFewInstances()
		 {
			  // given
			  Dictionary<IndexSlot, IndexProvider> instances = new Dictionary<IndexSlot, IndexProvider>( typeof( IndexSlot ) );
			  foreach ( IndexSlot indexSlot in Enum.GetValues( typeof( IndexSlot ) ) )
			  {
					instances[indexSlot] = IndexProvider.EMPTY;
			  }
			  InstanceSelector<IndexProvider> instanceSelector = new InstanceSelector<IndexProvider>( instances );

			  // when, then
			  assertThrows( typeof( System.ArgumentException ), () => validateSelectorInstances(instanceSelector, NUMBER) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void throwIfToManyInstances()
		 internal virtual void ThrowIfToManyInstances()
		 {
			  // given
			  Dictionary<IndexSlot, IndexProvider> instances = new Dictionary<IndexSlot, IndexProvider>( typeof( IndexSlot ) );
			  foreach ( IndexSlot indexSlot in Enum.GetValues( typeof( IndexSlot ) ) )
			  {
					instances[indexSlot] = IndexProvider.EMPTY;
			  }
			  IndexProvider mockedIndxProvider = mock( typeof( IndexProvider ) );
			  instances[NUMBER] = mockedIndxProvider;
			  InstanceSelector<IndexProvider> instanceSelector = new InstanceSelector<IndexProvider>( instances );

			  // when, then
			  assertThrows( typeof( System.ArgumentException ), () => validateSelectorInstances(instanceSelector) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldValidateSelectorInstances()
		 internal virtual void ShouldValidateSelectorInstances()
		 {
			  // given
			  Dictionary<IndexSlot, IndexProvider> instances = new Dictionary<IndexSlot, IndexProvider>( typeof( IndexSlot ) );
			  foreach ( IndexSlot indexSlot in Enum.GetValues( typeof( IndexSlot ) ) )
			  {
					instances[indexSlot] = IndexProvider.EMPTY;
			  }
			  IndexProvider mockedIndxProvider = mock( typeof( IndexProvider ) );
			  instances[NUMBER] = mockedIndxProvider;
			  InstanceSelector<IndexProvider> selector = new InstanceSelector<IndexProvider>( instances );

			  // when
			  validateSelectorInstances( selector, NUMBER );

			  // then this should be fine
		 }
	}

}