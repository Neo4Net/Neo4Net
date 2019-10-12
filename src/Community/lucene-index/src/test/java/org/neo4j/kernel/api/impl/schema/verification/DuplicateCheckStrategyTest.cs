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
namespace Neo4Net.Kernel.Api.Impl.Schema.verification
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using Neo4Net.Function;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using BucketsDuplicateCheckStrategy = Neo4Net.Kernel.Api.Impl.Schema.verification.DuplicateCheckStrategy.BucketsDuplicateCheckStrategy;
	using MapDuplicateCheckStrategy = Neo4Net.Kernel.Api.Impl.Schema.verification.DuplicateCheckStrategy.MapDuplicateCheckStrategy;
	using TextValue = Neo4Net.Values.Storable.TextValue;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueTuple = Neo4Net.Values.Storable.ValueTuple;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.schema.verification.DuplicateCheckStrategy.BucketsDuplicateCheckStrategy.BUCKET_STRATEGY_ENTRIES_THRESHOLD;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class DuplicateCheckStrategyTest
	public class DuplicateCheckStrategyTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters public static java.util.List<org.neo4j.function.Factory<? extends DuplicateCheckStrategy>> duplicateCheckStrategies()
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
		 public static IList<Factory<DuplicateCheckStrategy>> DuplicateCheckStrategies()
		 {
			  return Arrays.asList( () => new MapDuplicateCheckStrategy(1000), () => new BucketsDuplicateCheckStrategy(RandomNumberOfEntries()) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter public org.neo4j.function.Factory<DuplicateCheckStrategy> duplicateCheckStrategyFactory;
		 public Factory<DuplicateCheckStrategy> DuplicateCheckStrategyFactory;
		 private DuplicateCheckStrategy _checkStrategy;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public ExpectedException ExpectedException = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _checkStrategy = DuplicateCheckStrategyFactory.newInstance();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void checkStringSinglePropertyDuplicates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CheckStringSinglePropertyDuplicates()
		 {
			  string duplicatedString = "duplicate";

			  Value propertyValue = Values.stringValue( duplicatedString );

			  ExpectedException.expect( typeof( IndexEntryConflictException ) );
			  ExpectedException.expectMessage( format( "Both node 1 and node 2 share the property value %s", ValueTuple.of( propertyValue ) ) );

			  _checkStrategy.checkForDuplicate( propertyValue, 1 );
			  _checkStrategy.checkForDuplicate( propertyValue, 2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void checkNumericSinglePropertyDuplicates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CheckNumericSinglePropertyDuplicates()
		 {
			  double duplicatedNumber = 0.33d;
			  Value property = Values.doubleValue( duplicatedNumber );

			  ExpectedException.expect( typeof( IndexEntryConflictException ) );
			  ExpectedException.expectMessage( format( "Both node 3 and node 4 share the property value %s", ValueTuple.of( property ) ) );

			  _checkStrategy.checkForDuplicate( property, 3 );
			  _checkStrategy.checkForDuplicate( property, 4 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void duplicateFoundAmongUniqueStringSingleProperty() throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DuplicateFoundAmongUniqueStringSingleProperty()
		 {
			  for ( int i = 0; i < RandomNumberOfEntries(); i++ )
			  {
					string propertyValue = i.ToString();
					TextValue stringValue = Values.stringValue( propertyValue );
					_checkStrategy.checkForDuplicate( stringValue, i );
			  }

			  int duplicateTarget = BUCKET_STRATEGY_ENTRIES_THRESHOLD - 2;
			  string duplicate = duplicateTarget.ToString();
			  TextValue duplicatedValue = Values.stringValue( duplicate );
			  ExpectedException.expect( typeof( IndexEntryConflictException ) );
			  ExpectedException.expectMessage( format( "Both node %d and node 3 share the property value %s", duplicateTarget, ValueTuple.of( duplicatedValue ) ) );
			  _checkStrategy.checkForDuplicate( duplicatedValue, 3 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void duplicateFoundAmongUniqueNumberSingleProperty() throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DuplicateFoundAmongUniqueNumberSingleProperty()
		 {
			  double propertyValue = 0;
			  for ( int i = 0; i < RandomNumberOfEntries(); i++ )
			  {
					Value doubleValue = Values.doubleValue( propertyValue );
					_checkStrategy.checkForDuplicate( doubleValue, i );
					propertyValue += 1;
			  }

			  int duplicateTarget = BUCKET_STRATEGY_ENTRIES_THRESHOLD - 8;
			  double duplicateValue = duplicateTarget;
			  Value duplicate = Values.doubleValue( duplicateValue );
			  ExpectedException.expect( typeof( IndexEntryConflictException ) );
			  ExpectedException.expectMessage( format( "Both node %d and node 3 share the property value %s", duplicateTarget, ValueTuple.of( duplicate ) ) );
			  _checkStrategy.checkForDuplicate( duplicate, 3 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void noDuplicatesDetectedForUniqueStringSingleProperty() throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NoDuplicatesDetectedForUniqueStringSingleProperty()
		 {
			  for ( int i = 0; i < RandomNumberOfEntries(); i++ )
			  {
					string propertyValue = i.ToString();
					Value value = Values.stringValue( propertyValue );
					_checkStrategy.checkForDuplicate( value, i );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void noDuplicatesDetectedForUniqueNumberSingleProperty() throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NoDuplicatesDetectedForUniqueNumberSingleProperty()
		 {
			  double propertyValue = 0;
			  int numberOfIterations = RandomNumberOfEntries();
			  for ( int i = 0; i < numberOfIterations; i++ )
			  {
					propertyValue += 1d / numberOfIterations;
					Value value = Values.doubleValue( propertyValue );
					_checkStrategy.checkForDuplicate( value, i );
			  }
		 }

		 // multiple

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void checkStringMultiplePropertiesDuplicates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CheckStringMultiplePropertiesDuplicates()
		 {
			  string duplicateA = "duplicateA";
			  string duplicateB = "duplicateB";
			  Value propertyA = Values.stringValue( duplicateA );
			  Value propertyB = Values.stringValue( duplicateB );

			  ExpectedException.expect( typeof( IndexEntryConflictException ) );
			  ExpectedException.expectMessage( format( "Both node 1 and node 2 share the property value %s", ValueTuple.of( duplicateA, duplicateB ) ) );

			  _checkStrategy.checkForDuplicate( new Value[]{ propertyA, propertyB }, 1 );
			  _checkStrategy.checkForDuplicate( new Value[]{ propertyA, propertyB }, 2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void checkNumericMultiplePropertiesDuplicates() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CheckNumericMultiplePropertiesDuplicates()
		 {
			  Number duplicatedNumberA = 0.33d;
			  Number duplicatedNumberB = 2;
			  Value propertyA = Values.doubleValue( duplicatedNumberA.doubleValue() );
			  Value propertyB = Values.intValue( duplicatedNumberB.intValue() );

			  ExpectedException.expect( typeof( IndexEntryConflictException ) );
			  ExpectedException.expectMessage( format( "Both node 3 and node 4 share the property value %s", ValueTuple.of( propertyA, propertyB ) ) );

			  _checkStrategy.checkForDuplicate( new Value[]{ propertyA, propertyB }, 3 );
			  _checkStrategy.checkForDuplicate( new Value[]{ propertyA, propertyB }, 4 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void duplicateFoundAmongUniqueStringMultipleProperties() throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DuplicateFoundAmongUniqueStringMultipleProperties()
		 {
			  for ( int i = 0; i < RandomNumberOfEntries(); i++ )
			  {
					string propertyValueA = i.ToString();
					string propertyValueB = ( -i ).ToString();
					Value propertyA = Values.stringValue( propertyValueA );
					Value propertyB = Values.stringValue( propertyValueB );
					_checkStrategy.checkForDuplicate( new Value[]{ propertyA, propertyB }, i );
			  }

			  int duplicateTarget = BUCKET_STRATEGY_ENTRIES_THRESHOLD - 2;
			  string duplicatedValueA = duplicateTarget.ToString();
			  string duplicatedValueB = ( -duplicateTarget ).ToString();
			  Value propertyA = Values.stringValue( duplicatedValueA );
			  Value propertyB = Values.stringValue( duplicatedValueB );
			  ExpectedException.expect( typeof( IndexEntryConflictException ) );
			  ExpectedException.expectMessage( format( "Both node %d and node 3 share the property value %s", duplicateTarget, ValueTuple.of( propertyA, propertyB ) ) );
			  _checkStrategy.checkForDuplicate( new Value[]{ propertyA, propertyB }, 3 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void duplicateFoundAmongUniqueNumberMultipleProperties() throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void DuplicateFoundAmongUniqueNumberMultipleProperties()
		 {
			  double propertyValue = 0;
			  for ( int i = 0; i < RandomNumberOfEntries(); i++ )
			  {
					double propertyValueA = propertyValue;
					double propertyValueB = -propertyValue;
					Value propertyA = Values.doubleValue( propertyValueA );
					Value propertyB = Values.doubleValue( propertyValueB );
					_checkStrategy.checkForDuplicate( new Value[]{ propertyA, propertyB }, i );
					propertyValue += 1;
			  }

			  int duplicateTarget = BUCKET_STRATEGY_ENTRIES_THRESHOLD - 8;
			  double duplicateValueA = duplicateTarget;
			  double duplicateValueB = -duplicateTarget;
			  Value propertyA = Values.doubleValue( duplicateValueA );
			  Value propertyB = Values.doubleValue( duplicateValueB );
			  ExpectedException.expect( typeof( IndexEntryConflictException ) );
			  ExpectedException.expectMessage( format( "Both node %d and node 3 share the property value %s", duplicateTarget, ValueTuple.of( duplicateValueA, duplicateValueB ) ) );
			  _checkStrategy.checkForDuplicate( new Value[]{ propertyA, propertyB }, 3 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void noDuplicatesDetectedForUniqueStringMultipleProperties() throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NoDuplicatesDetectedForUniqueStringMultipleProperties()
		 {
			  for ( int i = 0; i < RandomNumberOfEntries(); i++ )
			  {
					string propertyValueA = i.ToString();
					string propertyValueB = ( -i ).ToString();
					Value propertyA = Values.stringValue( propertyValueA );
					Value propertyB = Values.stringValue( propertyValueB );
					_checkStrategy.checkForDuplicate( new Value[]{ propertyA, propertyB }, i );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void noDuplicatesDetectedForUniqueNumberMultipleProperties() throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void NoDuplicatesDetectedForUniqueNumberMultipleProperties()
		 {
			  double propertyValueA = 0;
			  double propertyValueB = 0;
			  int numberOfIterations = RandomNumberOfEntries();
			  for ( int i = 0; i < numberOfIterations; i++ )
			  {
					propertyValueA += 1d / numberOfIterations;
					propertyValueB -= 1d / numberOfIterations;
					Value propertyA = Values.doubleValue( propertyValueA );
					Value propertyB = Values.doubleValue( propertyValueB );
					_checkStrategy.checkForDuplicate( new Value[]{ propertyA, propertyB }, i );
			  }
		 }

		 private static int RandomNumberOfEntries()
		 {
			  return ThreadLocalRandom.current().Next(BUCKET_STRATEGY_ENTRIES_THRESHOLD, BUCKET_STRATEGY_ENTRIES_THRESHOLD << 1);
		 }

	}

}