using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using TestIndexDescriptorFactory = Org.Neo4j.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class NativeUniqueIndexPopulatorTest<KEY extends NativeIndexKey<KEY>, VALUE extends NativeIndexValue> extends NativeIndexPopulatorTests.Unique<KEY,VALUE>
	public class NativeUniqueIndexPopulatorTest<KEY, VALUE> : NativeIndexPopulatorTests.Unique<KEY, VALUE> where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
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

		 private static readonly StoreIndexDescriptor _uniqueDescriptor = TestIndexDescriptorFactory.uniqueForLabel( 42, 666 ).withId( 0 );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: NativeIndexPopulator<KEY,VALUE> createPopulator() throws java.io.IOException
		 internal override NativeIndexPopulator<KEY, VALUE> CreatePopulator()
		 {
			  return TestCase.populatorFactory.create( pageCache, fs, IndexFile, layout, monitor, indexDescriptor );
		 }

		 internal override ValueCreatorUtil<KEY, VALUE> CreateValueCreatorUtil()
		 {
			  return new ValueCreatorUtil<KEY, VALUE>( _uniqueDescriptor, TestCase.typesOfGroup, ValueCreatorUtil.FRACTION_DUPLICATE_UNIQUE );
		 }

		 internal override IndexLayout<KEY, VALUE> CreateLayout()
		 {
			  return TestCase.indexLayoutFactory.create();
		 }
	}

}