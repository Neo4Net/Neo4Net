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
namespace Neo4Net.Commandline.dbms.config
{
	using Test = org.junit.jupiter.api.Test;

	using Configuration = Neo4Net.@unsafe.Impl.Batchimport.Configuration;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.ByteUnit.kibiBytes;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.Configuration.DEFAULT;

	internal class WrappedBatchImporterConfigurationForNeo4jAdminTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDelegateDenseNodeThreshold()
		 internal virtual void ShouldDelegateDenseNodeThreshold()
		 {
			  shouldDelegate(expected => new ConfigurationAnonymousInnerClass(this)
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			 , Configuration::denseNodeThreshold, 1, 20);
		 }

		 private class ConfigurationAnonymousInnerClass : Configuration
		 {
			 private readonly WrappedBatchImporterConfigurationForNeo4jAdminTest _outerInstance;

			 public ConfigurationAnonymousInnerClass( WrappedBatchImporterConfigurationForNeo4jAdminTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public int denseNodeThreshold()
			 {
				  return expected;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDelegateMovingAverageSize()
		 internal virtual void ShouldDelegateMovingAverageSize()
		 {
			  shouldDelegate(expected => new ConfigurationAnonymousInnerClass2(this)
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			 , Configuration::movingAverageSize, 100, 200);
		 }

		 private class ConfigurationAnonymousInnerClass2 : Configuration
		 {
			 private readonly WrappedBatchImporterConfigurationForNeo4jAdminTest _outerInstance;

			 public ConfigurationAnonymousInnerClass2( WrappedBatchImporterConfigurationForNeo4jAdminTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public int movingAverageSize()
			 {
				  return expected;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDelegateSequentialBackgroundFlushing()
		 internal virtual void ShouldDelegateSequentialBackgroundFlushing()
		 {
			  shouldDelegate(expected => new ConfigurationAnonymousInnerClass3(this)
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			 , Configuration::sequentialBackgroundFlushing, true, false);
		 }

		 private class ConfigurationAnonymousInnerClass3 : Configuration
		 {
			 private readonly WrappedBatchImporterConfigurationForNeo4jAdminTest _outerInstance;

			 public ConfigurationAnonymousInnerClass3( WrappedBatchImporterConfigurationForNeo4jAdminTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public bool sequentialBackgroundFlushing()
			 {
				  return expected;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDelegateBatchSize()
		 internal virtual void ShouldDelegateBatchSize()
		 {
			  shouldDelegate(expected => new ConfigurationAnonymousInnerClass4(this)
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			 , Configuration::batchSize, 100, 200);
		 }

		 private class ConfigurationAnonymousInnerClass4 : Configuration
		 {
			 private readonly WrappedBatchImporterConfigurationForNeo4jAdminTest _outerInstance;

			 public ConfigurationAnonymousInnerClass4( WrappedBatchImporterConfigurationForNeo4jAdminTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public int batchSize()
			 {
				  return expected;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldOverrideMaxNumberOfProcessors()
		 internal virtual void ShouldOverrideMaxNumberOfProcessors()
		 {
			  shouldOverride(expected => new ConfigurationAnonymousInnerClass5(this)
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			 , Configuration::maxNumberOfProcessors, DEFAULT.maxNumberOfProcessors() + 1, DEFAULT.maxNumberOfProcessors() + 10);
		 }

		 private class ConfigurationAnonymousInnerClass5 : Configuration
		 {
			 private readonly WrappedBatchImporterConfigurationForNeo4jAdminTest _outerInstance;

			 public ConfigurationAnonymousInnerClass5( WrappedBatchImporterConfigurationForNeo4jAdminTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public int batchSize()
			 {
				  return expected;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDelegateParallelRecordWrites()
		 internal virtual void ShouldDelegateParallelRecordWrites()
		 {
			  shouldDelegate(expected => new ConfigurationAnonymousInnerClass6(this)
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			 , Configuration::parallelRecordWrites, true, false);
		 }

		 private class ConfigurationAnonymousInnerClass6 : Configuration
		 {
			 private readonly WrappedBatchImporterConfigurationForNeo4jAdminTest _outerInstance;

			 public ConfigurationAnonymousInnerClass6( WrappedBatchImporterConfigurationForNeo4jAdminTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public bool parallelRecordWrites()
			 {
				  return expected;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDelegateParallelRecordReads()
		 internal virtual void ShouldDelegateParallelRecordReads()
		 {
			  shouldDelegate(expected => new ConfigurationAnonymousInnerClass7(this)
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			 , Configuration::parallelRecordReads, true, false);
		 }

		 private class ConfigurationAnonymousInnerClass7 : Configuration
		 {
			 private readonly WrappedBatchImporterConfigurationForNeo4jAdminTest _outerInstance;

			 public ConfigurationAnonymousInnerClass7( WrappedBatchImporterConfigurationForNeo4jAdminTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public bool parallelRecordReads()
			 {
				  return expected;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDelegateHighIO()
		 internal virtual void ShouldDelegateHighIO()
		 {
			  shouldDelegate(expected => new ConfigurationAnonymousInnerClass8(this)
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			 , Configuration::highIO, true, false);
		 }

		 private class ConfigurationAnonymousInnerClass8 : Configuration
		 {
			 private readonly WrappedBatchImporterConfigurationForNeo4jAdminTest _outerInstance;

			 public ConfigurationAnonymousInnerClass8( WrappedBatchImporterConfigurationForNeo4jAdminTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public bool highIO()
			 {
				  return expected;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDelegateMaxMemoryUsage()
		 internal virtual void ShouldDelegateMaxMemoryUsage()
		 {
			  shouldDelegate(expected => new ConfigurationAnonymousInnerClass9(this)
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			 , Configuration::maxMemoryUsage, kibiBytes( 10 ), kibiBytes( 20 ));
		 }

		 private class ConfigurationAnonymousInnerClass9 : Configuration
		 {
			 private readonly WrappedBatchImporterConfigurationForNeo4jAdminTest _outerInstance;

			 public ConfigurationAnonymousInnerClass9( WrappedBatchImporterConfigurationForNeo4jAdminTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public long maxMemoryUsage()
			 {
				  return expected;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldDelegateAllowCacheAllocationOnHeap()
		 internal virtual void ShouldDelegateAllowCacheAllocationOnHeap()
		 {
			  shouldDelegate(expected => new ConfigurationAnonymousInnerClass10(this)
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			 , Configuration::allowCacheAllocationOnHeap, true, false);
		 }

		 private class ConfigurationAnonymousInnerClass10 : Configuration
		 {
			 private readonly WrappedBatchImporterConfigurationForNeo4jAdminTest _outerInstance;

			 public ConfigurationAnonymousInnerClass10( WrappedBatchImporterConfigurationForNeo4jAdminTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public bool allowCacheAllocationOnHeap()
			 {
				  return expected;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs private final <T> void shouldDelegate(System.Func<T,org.neo4j.unsafe.impl.batchimport.Configuration> configFactory, System.Func<org.neo4j.unsafe.impl.batchimport.Configuration,T> getter, T... expectedValues)
		 private void ShouldDelegate<T>( System.Func<T, Configuration> configFactory, System.Func<Configuration, T> getter, params T[] expectedValues )
		 {
			  foreach ( T expectedValue in expectedValues )
			  {
					// given
					Configuration configuration = configFactory( expectedValue );

					// when
					WrappedBatchImporterConfigurationForNeo4jAdmin wrapped = new WrappedBatchImporterConfigurationForNeo4jAdmin( configuration );

					// then
					assertEquals( expectedValue, getter( wrapped ) );
			  }

			  // then
			  assertEquals( getter( DEFAULT ), getter( new WrappedBatchImporterConfigurationForNeo4jAdmin( DEFAULT ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs private final <T> void shouldOverride(System.Func<T,org.neo4j.unsafe.impl.batchimport.Configuration> configFactory, System.Func<org.neo4j.unsafe.impl.batchimport.Configuration,T> getter, T... values)
		 private void ShouldOverride<T>( System.Func<T, Configuration> configFactory, System.Func<Configuration, T> getter, params T[] values )
		 {
			  foreach ( T value in values )
			  {
					// given
					Configuration configuration = configFactory( value );
					WrappedBatchImporterConfigurationForNeo4jAdmin vanilla = new WrappedBatchImporterConfigurationForNeo4jAdmin( DEFAULT );

					// when
					WrappedBatchImporterConfigurationForNeo4jAdmin wrapped = new WrappedBatchImporterConfigurationForNeo4jAdmin( configuration );

					// then
					assertEquals( getter( vanilla ), getter( wrapped ) );
			  }
		 }
	}

}