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
namespace Neo4Net.Kernel.Impl.Api.index.sampling
{
	using Test = org.junit.Test;

	using IndexSample = Neo4Net.Storageengine.Api.schema.IndexSample;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;

	public class DefaultNonUniqueIndexSamplerTest
	{
		 private readonly string _value = "aaa";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSampleNothing()
		 public virtual void ShouldSampleNothing()
		 {
			  // given
			  NonUniqueIndexSampler sampler = new DefaultNonUniqueIndexSampler( 10 );

			  // when
			  // nothing has been sampled

			  // then
			  AssertSampledValues( sampler, 0, 0, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSampleASingleValue()
		 public virtual void ShouldSampleASingleValue()
		 {
			  // given
			  NonUniqueIndexSampler sampler = new DefaultNonUniqueIndexSampler( 10 );

			  // when
			  sampler.Include( _value, 2 );

			  // then
			  AssertSampledValues( sampler, 2, 1, 2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSampleDuplicateValues()
		 public virtual void ShouldSampleDuplicateValues()
		 {
			  // given
			  NonUniqueIndexSampler sampler = new DefaultNonUniqueIndexSampler( 10 );

			  // when
			  sampler.Include( _value, 5 );
			  sampler.Include( _value, 4 );
			  sampler.Include( "bbb", 3 );

			  // then
			  AssertSampledValues( sampler, 12, 2, 12 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDivideTheSamplingInStepsNotBiggerThanBatchSize()
		 public virtual void ShouldDivideTheSamplingInStepsNotBiggerThanBatchSize()
		 {
			  // given
			  NonUniqueIndexSampler sampler = new DefaultNonUniqueIndexSampler( 1 );

			  // when
			  sampler.Include( _value, 5 );
			  sampler.Include( _value, 4 );
			  sampler.Include( "bbb", 3 );

			  // then
			  int expectedSampledSize = 12 / 3;
			  AssertSampledValues( sampler, 12, 1, expectedSampledSize );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExcludeValuesFromTheCurrentSampling1()
		 public virtual void ShouldExcludeValuesFromTheCurrentSampling1()
		 {
			  // given
			  NonUniqueIndexSampler sampler = new DefaultNonUniqueIndexSampler( 10 );
			  sampler.Include( _value, 5 );
			  sampler.Include( _value, 4 );
			  sampler.Include( "bbb", 3 );

			  // when
			  sampler.Exclude( _value, 3 );

			  // then
			  AssertSampledValues( sampler, 9, 2, 9 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldExcludeValuesFromTheCurrentSampling2()
		 public virtual void ShouldExcludeValuesFromTheCurrentSampling2()
		 {
			  // given
			  NonUniqueIndexSampler sampler = new DefaultNonUniqueIndexSampler( 10 );
			  sampler.Include( _value, 1 );
			  sampler.Include( _value, 4 );
			  sampler.Include( "bbb", 1 );

			  // when
			  sampler.Exclude( _value, 4 );

			  // then
			  AssertSampledValues( sampler, 2, 2, 2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDoNothingWhenExcludingAValueInAnEmptySample()
		 public virtual void ShouldDoNothingWhenExcludingAValueInAnEmptySample()
		 {
			  // given
			  NonUniqueIndexSampler sampler = new DefaultNonUniqueIndexSampler( 10 );

			  // when
			  sampler.Exclude( _value, 1 );
			  sampler.Include( _value, 1 );

			  // then
			  AssertSampledValues( sampler, 1, 1, 1 );
		 }

		 private void AssertSampledValues( NonUniqueIndexSampler sampler, long expectedIndexSize, long expectedUniqueValues, long expectedSampledSize )
		 {
			  IndexSample sample = sampler.Result();
			  assertEquals( expectedIndexSize, sample.IndexSize() );
			  assertEquals( expectedUniqueValues, sample.UniqueValues() );
			  assertEquals( expectedSampledSize, sample.SampleSize() );
		 }
	}

}