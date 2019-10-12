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
namespace Neo4Net.Kernel.Api.Impl.Index.sampler
{
	using Test = org.junit.jupiter.api.Test;


	using IndexSample = Neo4Net.Storageengine.Api.schema.IndexSample;
	using IndexSampler = Neo4Net.Storageengine.Api.schema.IndexSampler;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;

	internal class AggregatingIndexSamplerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void samplePartitionedIndex()
		 internal virtual void SamplePartitionedIndex()
		 {
			  IList<IndexSampler> samplers = Arrays.asList( CreateSampler( 1 ), CreateSampler( 2 ) );
			  AggregatingIndexSampler partitionedSampler = new AggregatingIndexSampler( samplers );

			  IndexSample sample = partitionedSampler.SampleIndex();

			  assertEquals( new IndexSample( 3, 3, 6 ), sample );
		 }

		 private static IndexSampler CreateSampler( long value )
		 {
			  return new TestIndexSampler( value );
		 }

		 private class TestIndexSampler : IndexSampler
		 {
			  internal readonly long Value;

			  internal TestIndexSampler( long value )
			  {
					this.Value = value;
			  }

			  public override IndexSample SampleIndex()
			  {
					return new IndexSample( Value, Value, Value * 2 );
			  }
		 }
	}

}