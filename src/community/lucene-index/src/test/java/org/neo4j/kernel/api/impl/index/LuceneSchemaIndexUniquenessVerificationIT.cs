using System;
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
namespace Neo4Net.Kernel.Api.Impl.Index
{
	using RandomStringUtils = org.apache.commons.lang3.RandomStringUtils;
	using Codec = org.apache.lucene.codecs.Codec;
	using Document = org.apache.lucene.document.Document;
	using IndexWriterConfig = Org.Apache.Lucene.Index.IndexWriterConfig;
	using AfterEach = org.junit.jupiter.api.AfterEach;
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using Neo4Net.Functions;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using Strings = Neo4Net.Helpers.Strings;
	using IOUtils = Neo4Net.Io.IOUtils;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using DirectoryFactory = Neo4Net.Kernel.Api.Impl.Index.storage.DirectoryFactory;
	using LuceneDocumentStructure = Neo4Net.Kernel.Api.Impl.Schema.LuceneDocumentStructure;
	using LuceneSchemaIndexBuilder = Neo4Net.Kernel.Api.Impl.Schema.LuceneSchemaIndexBuilder;
	using SchemaIndex = Neo4Net.Kernel.Api.Impl.Schema.SchemaIndex;
	using NodePropertyAccessor = Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.Api.schema.index.TestIndexDescriptorFactory;
	using Config = Neo4Net.Kernel.configuration.Config;
	using IndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.IndexDescriptor;
	using DefaultFileSystemExtension = Neo4Net.Test.extension.DefaultFileSystemExtension;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using RandomValues = Neo4Net.Values.Storable.RandomValues;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertThrows;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.fail;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith({DefaultFileSystemExtension.class, TestDirectoryExtension.class}) class LuceneSchemaIndexUniquenessVerificationIT
	internal class LuceneSchemaIndexUniquenessVerificationIT
	{
		 private static readonly int _docsPerPartition = ThreadLocalRandom.current().Next(10, 100);
		 private const int PROPERTY_KEY_ID = 42;
		 private static readonly IndexDescriptor _descriptor = TestIndexDescriptorFactory.uniqueForLabel( 0, PROPERTY_KEY_ID );
		 private static readonly int _nodesToCreate = _docsPerPartition * 2 + 1;
		 private static readonly long _maxLongValue = long.MaxValue >> 10;
		 private static readonly long _minLongValue = _maxLongValue - 20;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private Neo4Net.test.rule.TestDirectory testDir;
		 private TestDirectory _testDir;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private Neo4Net.io.fs.DefaultFileSystemAbstraction fileSystem;
		 private DefaultFileSystemAbstraction _fileSystem;

		 private SchemaIndex _index;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setPartitionSize() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SetPartitionSize()
		 {
			  System.setProperty( "luceneSchemaIndex.maxPartitionSize", _docsPerPartition.ToString() );

			  IFactory<IndexWriterConfig> configFactory = new TestConfigFactory();
			  _index = LuceneSchemaIndexBuilder.create( _descriptor, Config.defaults() ).withFileSystem(_fileSystem).withIndexRootFolder(new File(_testDir.directory("uniquenessVerification"), "index")).withWriterConfig(configFactory).withDirectoryFactory(DirectoryFactory.PERSISTENT).build();

			  _index.create();
			  _index.open();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterEach void resetPartitionSize() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ResetPartitionSize()
		 {
			  System.setProperty( "luceneSchemaIndex.maxPartitionSize", "" );

			  IOUtils.closeAll( _index );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void stringValuesWithoutDuplicates() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StringValuesWithoutDuplicates()
		 {
			  ISet<Value> data = RandomStrings();

			  Insert( data );

			  AssertUniquenessConstraintHolds( data );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void stringValuesWithDuplicates() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void StringValuesWithDuplicates()
		 {
			  IList<Value> data = WithDuplicate( RandomStrings() );

			  Insert( data );

			  AssertUniquenessConstraintFails( data );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void smallLongValuesWithoutDuplicates() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SmallLongValuesWithoutDuplicates()
		 {
			  long min = RandomLongInRange( 100, 10_000 );
			  long max = min + _nodesToCreate;
			  ISet<Value> data = RandomLongs( min, max );

			  Insert( data );

			  AssertUniquenessConstraintHolds( data );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void smallLongValuesWithDuplicates() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SmallLongValuesWithDuplicates()
		 {
			  long min = RandomLongInRange( 100, 10_000 );
			  long max = min + _nodesToCreate;
			  IList<Value> data = WithDuplicate( RandomLongs( min, max ) );

			  Insert( data );

			  AssertUniquenessConstraintFails( data );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void largeLongValuesWithoutDuplicates() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void LargeLongValuesWithoutDuplicates()
		 {
			  long max = RandomLongInRange( _minLongValue, _maxLongValue );
			  long min = max - _nodesToCreate;
			  ISet<Value> data = RandomLongs( min, max );

			  Insert( data );

			  AssertUniquenessConstraintHolds( data );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void largeLongValuesWithDuplicates() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void LargeLongValuesWithDuplicates()
		 {
			  long max = RandomLongInRange( _minLongValue, _maxLongValue );
			  long min = max - _nodesToCreate;
			  IList<Value> data = WithDuplicate( RandomLongs( min, max ) );

			  Insert( data );

			  AssertUniquenessConstraintFails( data );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void smallDoubleValuesWithoutDuplicates() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SmallDoubleValuesWithoutDuplicates()
		 {
			  double min = RandomDoubleInRange( 100, 10_000 );
			  double max = min + _nodesToCreate;
			  ISet<Value> data = RandomDoubles( min, max );

			  Insert( data );

			  AssertUniquenessConstraintHolds( data );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void smallDoubleValuesWithDuplicates() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SmallDoubleValuesWithDuplicates()
		 {
			  double min = RandomDoubleInRange( 100, 10_000 );
			  double max = min + _nodesToCreate;
			  IList<Value> data = WithDuplicate( RandomDoubles( min, max ) );

			  Insert( data );

			  AssertUniquenessConstraintFails( data );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void largeDoubleValuesWithoutDuplicates() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void LargeDoubleValuesWithoutDuplicates()
		 {
			  double max = RandomDoubleInRange( double.MaxValue / 2, double.MaxValue );
			  double min = max / 2;
			  ISet<Value> data = RandomDoubles( min, max );

			  Insert( data );

			  AssertUniquenessConstraintHolds( data );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void largeDoubleValuesWithDuplicates() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void LargeDoubleValuesWithDuplicates()
		 {
			  double max = RandomDoubleInRange( double.MaxValue / 2, double.MaxValue );
			  double min = max / 2;
			  IList<Value> data = WithDuplicate( RandomDoubles( min, max ) );

			  Insert( data );

			  AssertUniquenessConstraintFails( data );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void smallArrayValuesWithoutDuplicates() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SmallArrayValuesWithoutDuplicates()
		 {
			  ISet<Value> data = RandomArrays( 3, 7 );

			  Insert( data );

			  AssertUniquenessConstraintHolds( data );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void smallArrayValuesWithDuplicates() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void SmallArrayValuesWithDuplicates()
		 {
			  IList<Value> data = WithDuplicate( RandomArrays( 3, 7 ) );

			  Insert( data );

			  AssertUniquenessConstraintFails( data );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void largeArrayValuesWithoutDuplicates() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void LargeArrayValuesWithoutDuplicates()
		 {
			  ISet<Value> data = RandomArrays( 70, 100 );

			  Insert( data );

			  AssertUniquenessConstraintHolds( data );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void largeArrayValuesWithDuplicates() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void LargeArrayValuesWithDuplicates()
		 {
			  IList<Value> data = WithDuplicate( RandomArrays( 70, 100 ) );

			  Insert( data );

			  AssertUniquenessConstraintFails( data );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void variousValuesWithoutDuplicates() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void VariousValuesWithoutDuplicates()
		 {
			  ISet<Value> data = RandomValues();

			  Insert( data );

			  AssertUniquenessConstraintHolds( data );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void variousValuesWitDuplicates() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void VariousValuesWitDuplicates()
		 {
			  IList<Value> data = WithDuplicate( RandomValues() );

			  Insert( data );

			  AssertUniquenessConstraintFails( data );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void insert(java.util.Collection<Neo4Net.values.storable.Value> data) throws java.io.IOException
		 private void Insert( ICollection<Value> data )
		 {
			  Value[] dataArray = data.toArray( new Value[data.Count] );
			  for ( int i = 0; i < dataArray.Length; i++ )
			  {
					Document doc = LuceneDocumentStructure.documentRepresentingProperties( i, dataArray[i] );
					_index.IndexWriter.addDocument( doc );
			  }
			  _index.maybeRefreshBlocking();
		 }

		 private void AssertUniquenessConstraintHolds( ICollection<Value> data )
		 {
			  try
			  {
					VerifyUniqueness( data );
			  }
			  catch ( Exception t )
			  {
					fail( "Unable to create uniqueness constraint for data: " + Strings.prettyPrint( data.ToArray() ) + "\n" + Exceptions.stringify(t) );
			  }
		 }

		 private void AssertUniquenessConstraintFails( ICollection<Value> data )
		 {
			  assertThrows( typeof( IndexEntryConflictException ), () => verifyUniqueness(data) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyUniqueness(java.util.Collection<Neo4Net.values.storable.Value> data) throws java.io.IOException, Neo4Net.kernel.api.exceptions.index.IndexEntryConflictException
		 private void VerifyUniqueness( ICollection<Value> data )
		 {
			  NodePropertyAccessor nodePropertyAccessor = new TestPropertyAccessor( new List<Value>( data ) );
			  _index.verifyUniqueness( nodePropertyAccessor, new int[]{ PROPERTY_KEY_ID } );
		 }

		 private ISet<Value> RandomStrings()
		 {
			  return ThreadLocalRandom.current().ints(_nodesToCreate, 1, 200).mapToObj(this.randomString).map(Values.of).collect(toSet());
		 }

		 private string RandomString( int size )
		 {
			  return ThreadLocalRandom.current().nextBoolean() ? RandomStringUtils.random(size) : RandomStringUtils.randomAlphabetic(size);
		 }

		 private ISet<Value> RandomLongs( long min, long max )
		 {
			  return ThreadLocalRandom.current().longs(_nodesToCreate, min, max).boxed().map(Values.of).collect(toSet());
		 }

		 private ISet<Value> RandomDoubles( double min, double max )
		 {
			  return ThreadLocalRandom.current().doubles(_nodesToCreate, min, max).boxed().map(Values.of).collect(toSet());
		 }

		 private ISet<Value> RandomArrays( int minLength, int maxLength )
		 {
			  RandomValues randoms = RandomValues.create( new ArraySizeConfig( minLength, maxLength ) );

			  return IntStream.range( 0, _nodesToCreate ).mapToObj( i => randoms.NextArray() ).collect(toSet());
		 }

		 private ISet<Value> RandomValues()
		 {
			  RandomValues randoms = RandomValues.create( new ArraySizeConfig( 5, 100 ) );

			  return IntStream.range( 0, _nodesToCreate ).mapToObj( i => randoms.NextValue() ).collect(toSet());
		 }

		 private static IList<Value> WithDuplicate( ISet<Value> set )
		 {
			  IList<Value> data = new List<Value>( set );
			  if ( data.Count == 0 )
			  {
					throw new System.InvalidOperationException();
			  }
			  else if ( data.Count == 1 )
			  {
					data.Add( data[0] );
			  }
			  else
			  {
					int duplicateIndex = RandomIntInRange( 0, data.Count );
					int duplicateValueIndex;
					do
					{
						 duplicateValueIndex = ThreadLocalRandom.current().Next(data.Count);
					} while ( duplicateValueIndex == duplicateIndex );
					Value duplicate = DuplicateValue( data[duplicateValueIndex] );
					data[duplicateIndex] = duplicate;
			  }
			  return data;
		 }

		 private static Value DuplicateValue( Value propertyValue )
		 {
			  return Values.of( propertyValue.AsObjectCopy() );
		 }

		 private static int RandomIntInRange( int min, int max )
		 {
			  return ThreadLocalRandom.current().Next(min, max);
		 }

		 private static long RandomLongInRange( long min, long max )
		 {
			  return ThreadLocalRandom.current().nextLong(min, max);
		 }

		 private static double RandomDoubleInRange( double min, double max )
		 {
			  return ThreadLocalRandom.current().nextDouble(min, max);
		 }

		 private class ArraySizeConfig : RandomValues.Default
		 {
			  internal readonly int MinLength;
			  internal readonly int MaxLength;

			  internal ArraySizeConfig( int minLength, int maxLength )
			  {
					this.MinLength = minLength;
					this.MaxLength = maxLength;
			  }

			  public override int ArrayMinLength()
			  {
					return base.ArrayMinLength();
			  }

			  public override int ArrayMaxLength()
			  {
					return base.ArrayMaxLength();
			  }
		 }

		 private class TestConfigFactory : IFactory<IndexWriterConfig>
		 {

			  public override IndexWriterConfig NewInstance()
			  {
					IndexWriterConfig verboseConfig = IndexWriterConfigs.Standard();
					verboseConfig.Codec = Codec.Default;
					return verboseConfig;
			  }
		 }
	}

}