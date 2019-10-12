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
namespace Org.Neo4j.@unsafe.Impl.Batchimport
{
	using Test = org.junit.Test;

	using OsBeanUtil = Org.Neo4j.Io.os.OsBeanUtil;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ConfiguringPageCacheFactory = Org.Neo4j.Kernel.impl.pagecache.ConfiguringPageCacheFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThan;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.abs;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.max;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Math.min;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assume.assumeTrue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.pagecache_memory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.os.OsBeanUtil.VALUE_UNAVAILABLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.Settings.parseLongWithUnit;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.impl.batchimport.Configuration_Fields.MAX_PAGE_CACHE_MEMORY;

	public class ConfigurationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOverrideBigPageCacheMemorySettingContainingUnit()
		 public virtual void ShouldOverrideBigPageCacheMemorySettingContainingUnit()
		 {
			  // GIVEN
			  Config dbConfig = Config.defaults( pagecache_memory, "2g" );
			  Configuration config = new Configuration_Overridden( dbConfig );

			  // WHEN
			  long memory = config.PageCacheMemory();

			  // THEN
			  assertEquals( MAX_PAGE_CACHE_MEMORY, memory );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOverrideSmallPageCacheMemorySettingContainingUnit()
		 public virtual void ShouldOverrideSmallPageCacheMemorySettingContainingUnit()
		 {
			  // GIVEN
			  long overridden = parseLongWithUnit( "10m" );
			  Config dbConfig = Config.defaults( pagecache_memory, valueOf( overridden ) );
			  Configuration config = new Configuration_Overridden( dbConfig );

			  // WHEN
			  long memory = config.PageCacheMemory();

			  // THEN
			  assertEquals( overridden, memory );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldParseDefaultPageCacheMemorySetting()
		 public virtual void ShouldParseDefaultPageCacheMemorySetting()
		 {
			  // GIVEN
			  Configuration config = Configuration.DEFAULT;

			  // WHEN
			  long memory = config.PageCacheMemory();

			  // THEN
			  long heuristic = ConfiguringPageCacheFactory.defaultHeuristicPageCacheMemory();
			  assertTrue( Within( memory, heuristic, MAX_PAGE_CACHE_MEMORY ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCalculateCorrectMaxMemorySetting() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCalculateCorrectMaxMemorySetting()
		 {
			  long totalMachineMemory = OsBeanUtil.TotalPhysicalMemory;
			  assumeTrue( totalMachineMemory != VALUE_UNAVAILABLE );

			  // given
			  int percent = 70;
			  Configuration config = new ConfigurationAnonymousInnerClass( this, percent );

			  // when
			  long memory = config.MaxMemoryUsage();

			  // then
			  long expected = ( long )( ( totalMachineMemory - Runtime.Runtime.maxMemory() ) * (percent / 100D) );
			  long diff = abs( expected - memory );
			  assertThat( diff, lessThan( ( long )( expected / 10D ) ) );
		 }

		 private class ConfigurationAnonymousInnerClass : Configuration
		 {
			 private readonly ConfigurationTest _outerInstance;

			 private int _percent;

			 public ConfigurationAnonymousInnerClass( ConfigurationTest outerInstance, int percent )
			 {
				 this.outerInstance = outerInstance;
				 this._percent = percent;
			 }

			 public long maxMemoryUsage()
			 {
				  return Configuration.calculateMaxMemoryFromPercent( _percent );
			 }
		 }

		 private bool Within( long value, long firstBound, long otherBound )
		 {
			  return value >= min( firstBound, otherBound ) && value <= max( firstBound, otherBound );
		 }
	}

}