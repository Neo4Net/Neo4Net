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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class NativeNonUniqueIndexPopulatorTest<KEY extends NativeIndexKey<KEY>, VALUE extends NativeIndexValue> extends NativeIndexPopulatorTests.NonUnique<KEY,VALUE>
	public class NativeNonUniqueIndexPopulatorTest<KEY, VALUE> : NativeIndexPopulatorTests.NonUnique<KEY, VALUE> where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{index} {0}") public static java.util.Collection<Object[]> data()
		 public static ICollection<object[]> Data()
		 {
			  return NativeIndexPopulatorTestCases.AllCases();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter() public NativeIndexPopulatorTestCases.TestCase<KEY,VALUE> testCase;
		 public NativeIndexPopulatorTestCases.TestCase<KEY, VALUE> TestCase;

		 private static readonly StoreIndexDescriptor _nonUniqueDescriptor = TestIndexDescriptorFactory.forLabel( 42, 666 ).withId( 0 );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: NativeIndexPopulator<KEY,VALUE> createPopulator() throws java.io.IOException
		 internal override NativeIndexPopulator<KEY, VALUE> CreatePopulator()
		 {
			  return TestCase.populatorFactory.create( pageCache, fs, IndexFile, layout, monitor, indexDescriptor );
		 }

		 internal override ValueCreatorUtil<KEY, VALUE> CreateValueCreatorUtil()
		 {
			  return new ValueCreatorUtil<KEY, VALUE>( _nonUniqueDescriptor, TestCase.typesOfGroup, ValueCreatorUtil.FRACTION_DUPLICATE_NON_UNIQUE );
		 }

		 internal override IndexLayout<KEY, VALUE> CreateLayout()
		 {
			  return TestCase.indexLayoutFactory.create();
		 }
	}

}