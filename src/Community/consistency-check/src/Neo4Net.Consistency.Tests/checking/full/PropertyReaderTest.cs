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
namespace Neo4Net.Consistency.checking.full
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using PropertyStore = Neo4Net.Kernel.impl.store.PropertyStore;
	using StoreAccess = Neo4Net.Kernel.impl.store.StoreAccess;
	using StoreType = Neo4Net.Kernel.impl.store.StoreType;
	using PropertyRecord = Neo4Net.Kernel.Impl.Store.Records.PropertyRecord;
	using NeoStoresRule = Neo4Net.Test.rule.NeoStoresRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class PropertyReaderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.NeoStoresRule storesRule = new Neo4Net.test.rule.NeoStoresRule(PropertyReaderTest.class, Neo4Net.kernel.impl.store.StoreType.NODE, Neo4Net.kernel.impl.store.StoreType.COUNTS, Neo4Net.kernel.impl.store.StoreType.PROPERTY, Neo4Net.kernel.impl.store.StoreType.PROPERTY_ARRAY, Neo4Net.kernel.impl.store.StoreType.PROPERTY_STRING);
		 public readonly NeoStoresRule StoresRule = new NeoStoresRule( typeof( PropertyReaderTest ), StoreType.NODE, StoreType.COUNTS, StoreType.PROPERTY, StoreType.PROPERTY_ARRAY, StoreType.PROPERTY_STRING );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectAndAbortPropertyChainLoadingOnCircularReference() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectAndAbortPropertyChainLoadingOnCircularReference()
		 {
			  // given
			  NeoStores neoStores = StoresRule.builder().build();

			  // Create property chain 1 --> 2 --> 3 --> 4
			  //                             ↑           │
			  //                             └───────────┘
			  PropertyStore propertyStore = neoStores.PropertyStore;
			  PropertyRecord record = propertyStore.NewRecord();
			  // 1
			  record.Id = 1;
			  record.Initialize( true, -1, 2 );
			  propertyStore.UpdateRecord( record );
			  // 2
			  record.Id = 2;
			  record.Initialize( true, 1, 3 );
			  propertyStore.UpdateRecord( record );
			  // 3
			  record.Id = 3;
			  record.Initialize( true, 2, 4 );
			  propertyStore.UpdateRecord( record );
			  // 4
			  record.Id = 4;
			  record.Initialize( true, 3, 2 ); // <-- completing the circle
			  propertyStore.UpdateRecord( record );

			  // when
			  PropertyReader reader = new PropertyReader( new StoreAccess( neoStores ) );
			  try
			  {
					reader.GetPropertyRecordChain( 1 );
					fail( "Should have detected circular reference" );
			  }
			  catch ( PropertyReader.CircularPropertyRecordChainException e )
			  {
					// then good
					assertEquals( 4, e.PropertyRecordClosingTheCircle().Id );
			  }
		 }
	}

}