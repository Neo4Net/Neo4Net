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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{
	using Test = org.junit.Test;

	using Org.Neo4j.Index.@internal.gbptree;
	using Org.Neo4j.Index.@internal.gbptree;
	using IOLimiter = Org.Neo4j.Io.pagecache.IOLimiter;
	using IndexSample = Org.Neo4j.Storageengine.Api.schema.IndexSample;
	using NumberValue = Org.Neo4j.Values.Storable.NumberValue;
	using RandomValues = Org.Neo4j.Values.Storable.RandomValues;
	using Value = Org.Neo4j.Values.Storable.Value;
	using ValueType = Org.Neo4j.Values.Storable.ValueType;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.index.TestIndexDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.NativeIndexKey.Inclusion.NEUTRAL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.ValueCreatorUtil.FRACTION_DUPLICATE_NON_UNIQUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.ValueCreatorUtil.countUniqueValues;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.RandomValues.typesOfGroup;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.ValueGroup.NUMBER;

	public class NumberFullScanNonUniqueIndexSamplerTest : NativeIndexTestUtil<NumberIndexKey, NativeIndexValue>
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludeAllValuesInTree() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIncludeAllValuesInTree()
		 {
			  // GIVEN
			  Value[] values = GenerateNumberValues();
			  BuildTree( values );

			  // WHEN
			  IndexSample sample;
			  using ( GBPTree<NumberIndexKey, NativeIndexValue> gbpTree = Tree )
			  {
					FullScanNonUniqueIndexSampler<NumberIndexKey, NativeIndexValue> sampler = new FullScanNonUniqueIndexSampler<NumberIndexKey, NativeIndexValue>( gbpTree, Layout );
					sample = sampler.Result();
			  }

			  // THEN
			  assertEquals( values.Length, sample.SampleSize() );
			  assertEquals( countUniqueValues( values ), sample.UniqueValues() );
			  assertEquals( values.Length, sample.IndexSize() );
		 }

		 private Value[] GenerateNumberValues()
		 {
			  ValueType[] numberTypes = RandomValues.including( t => t.valueGroup == NUMBER );
			  int size = 20;
			  Value[] result = new NumberValue[size];
			  for ( int i = 0; i < size; i++ )
			  {
					result[i] = Random.randomValues().nextValueOfTypes(numberTypes);
			  }
			  return result;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void buildTree(org.neo4j.values.storable.Value[] values) throws java.io.IOException
		 private void BuildTree( Value[] values )
		 {
			  using ( GBPTree<NumberIndexKey, NativeIndexValue> gbpTree = Tree )
			  {
					using ( Writer<NumberIndexKey, NativeIndexValue> writer = gbpTree.Writer() )
					{
						 NumberIndexKey key = Layout.newKey();
						 NativeIndexValue value = Layout.newValue();
						 long nodeId = 0;
						 foreach ( Value number in values )
						 {
							  key.initialize( nodeId );
							  key.initFromValue( 0, number, NEUTRAL );
							  value.From( number );
							  writer.Put( key, value );
							  nodeId++;
						 }
					}
					gbpTree.Checkpoint( Org.Neo4j.Io.pagecache.IOLimiter_Fields.Unlimited );
			  }
		 }

		 protected internal override ValueCreatorUtil<NumberIndexKey, NativeIndexValue> CreateValueCreatorUtil()
		 {
			  return new ValueCreatorUtil<NumberIndexKey, NativeIndexValue>( forLabel( 42, 666 ).withId( 0 ), typesOfGroup( NUMBER ), FRACTION_DUPLICATE_NON_UNIQUE );
		 }

		 internal override IndexLayout<NumberIndexKey, NativeIndexValue> CreateLayout()
		 {
			  return new NumberLayoutNonUnique();
		 }
	}

}