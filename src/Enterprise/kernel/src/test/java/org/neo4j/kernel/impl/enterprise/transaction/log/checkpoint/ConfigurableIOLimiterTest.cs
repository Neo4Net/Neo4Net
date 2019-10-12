using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Kernel.impl.enterprise.transaction.log.checkpoint
{
	using Test = org.junit.Test;


	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using Config = Neo4Net.Kernel.configuration.Config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;

	public class ConfigurableIOLimiterTest
	{
		 private const string ORIGIN = "test";
		 private ConfigurableIOLimiter _limiter;
		 private Config _config;
		 private AtomicLong _pauseNanosCounter;
		 private static readonly Flushable _flushable = () =>
		 {
		 };

		 private void CreateIOLimiter( Config config )
		 {
			  this._config = config;
			  _pauseNanosCounter = new AtomicLong();
			  System.Action<object, long> pauseNanos = ( blocker, nanos ) => _pauseNanosCounter.getAndAdd( nanos );
			  _limiter = new ConfigurableIOLimiter( config, pauseNanos );
		 }

		 private void CreateIOLimiter( int limit )
		 {
			  IDictionary<string, string> settings = stringMap( GraphDatabaseSettings.check_point_iops_limit.name(), "" + limit );
			  CreateIOLimiter( Config.defaults( settings ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustPutDefaultLimitOnIOWhenNoLimitIsConfigured()
		 public virtual void MustPutDefaultLimitOnIOWhenNoLimitIsConfigured()
		 {
			  CreateIOLimiter( Config.defaults() );

			  // Do 100*100 = 10000 IOs real quick, when we're limited to 1000 IOPS.
			  long stamp = Neo4Net.Io.pagecache.IOLimiter_Fields.INITIAL_STAMP;
			  RepeatedlyCallMaybeLimitIO( _limiter, stamp, 100 );

			  // This should have led to about 10 seconds of pause, minus the time we spent in the loop.
			  // So let's say 9 seconds - experiments indicate this gives us about a 10x margin.
			  assertThat( _pauseNanosCounter.get(), greaterThan(TimeUnit.SECONDS.toNanos(9)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustNotPutLimitOnIOWhenConfiguredToBeUnlimited()
		 public virtual void MustNotPutLimitOnIOWhenConfiguredToBeUnlimited()
		 {
			  CreateIOLimiter( -1 );
			  AssertUnlimited();
		 }

		 private void AssertUnlimited()
		 {
			  long pauseTime = _pauseNanosCounter.get();
			  RepeatedlyCallMaybeLimitIO( _limiter, Neo4Net.Io.pagecache.IOLimiter_Fields.INITIAL_STAMP, 1000000 );
			  assertThat( _pauseNanosCounter.get(), @is(pauseTime) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustNotPutLimitOnIOWhenLimitingIsDisabledAndNoLimitIsConfigured()
		 public virtual void MustNotPutLimitOnIOWhenLimitingIsDisabledAndNoLimitIsConfigured()
		 {
			  CreateIOLimiter( Config.defaults() );
			  _limiter.disableLimit();
			  try
			  {
					AssertUnlimited();
					_limiter.disableLimit();
					try
					{
						 AssertUnlimited();
					}
					finally
					{
						 _limiter.enableLimit();
					}
			  }
			  finally
			  {
					_limiter.enableLimit();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustRestrictIORateToConfiguredLimit()
		 public virtual void MustRestrictIORateToConfiguredLimit()
		 {
			  CreateIOLimiter( 100 );

			  // Do 10*100 = 1000 IOs real quick, when we're limited to 100 IOPS.
			  long stamp = Neo4Net.Io.pagecache.IOLimiter_Fields.INITIAL_STAMP;
			  RepeatedlyCallMaybeLimitIO( _limiter, stamp, 10 );

			  // This should have led to about 10 seconds of pause, minus the time we spent in the loop.
			  // So let's say 9 seconds - experiments indicate this gives us about a 10x margin.
			  assertThat( _pauseNanosCounter.get(), greaterThan(TimeUnit.SECONDS.toNanos(9)) );
		 }

		 private long RepeatedlyCallMaybeLimitIO( IOLimiter ioLimiter, long stamp, int iosPerIteration )
		 {
			  for ( int i = 0; i < 100; i++ )
			  {
					stamp = ioLimiter.MaybeLimitIO( stamp, iosPerIteration, _flushable );
			  }
			  return stamp;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustNotRestrictIOToConfiguredRateWhenLimitIsDisabled()
		 public virtual void MustNotRestrictIOToConfiguredRateWhenLimitIsDisabled()
		 {
			  CreateIOLimiter( 100 );

			  long stamp = Neo4Net.Io.pagecache.IOLimiter_Fields.INITIAL_STAMP;
			  _limiter.disableLimit();
			  try
			  {
					stamp = RepeatedlyCallMaybeLimitIO( _limiter, stamp, 10 );
					_limiter.disableLimit();
					try
					{
						 stamp = RepeatedlyCallMaybeLimitIO( _limiter, stamp, 10 );
					}
					finally
					{
						 _limiter.enableLimit();
					}
					RepeatedlyCallMaybeLimitIO( _limiter, stamp, 10 );
			  }
			  finally
			  {
					_limiter.enableLimit();
			  }

			  // We should've spent no time rushing
			  assertThat( _pauseNanosCounter.get(), @is(0L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dynamicConfigurationUpdateMustBecomeVisible()
		 public virtual void DynamicConfigurationUpdateMustBecomeVisible()
		 {
			  // Create initially unlimited
			  CreateIOLimiter( 0 );

			  // Then set a limit of 100 IOPS
			  _config.updateDynamicSetting( GraphDatabaseSettings.check_point_iops_limit.name(), "100", ORIGIN );

			  // Do 10*100 = 1000 IOs real quick, when we're limited to 100 IOPS.
			  long stamp = Neo4Net.Io.pagecache.IOLimiter_Fields.INITIAL_STAMP;
			  RepeatedlyCallMaybeLimitIO( _limiter, stamp, 10 );

			  // Then assert that the updated limit is respected
			  assertThat( _pauseNanosCounter.get(), greaterThan(TimeUnit.SECONDS.toNanos(9)) );

			  // Change back to unlimited
			  _config.updateDynamicSetting( GraphDatabaseSettings.check_point_iops_limit.name(), "-1", ORIGIN );

			  // And verify it's respected
			  AssertUnlimited();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dynamicConfigurationUpdateEnablingLimiterMustNotDisableLimiter()
		 public virtual void DynamicConfigurationUpdateEnablingLimiterMustNotDisableLimiter()
		 {
			  // Create initially unlimited
			  CreateIOLimiter( 0 );
			  // Disable the limiter...
			  _limiter.disableLimit();
			  // ...while a dynamic configuration update happens
			  _config.updateDynamicSetting( GraphDatabaseSettings.check_point_iops_limit.name(), "100", ORIGIN );
			  // The limiter must still be disabled...
			  AssertUnlimited();
			  // ...and re-enabling it...
			  _limiter.enableLimit();
			  // ...must make the limiter limit.
			  long stamp = Neo4Net.Io.pagecache.IOLimiter_Fields.INITIAL_STAMP;
			  RepeatedlyCallMaybeLimitIO( _limiter, stamp, 10 );
			  assertThat( _pauseNanosCounter.get(), greaterThan(TimeUnit.SECONDS.toNanos(9)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void dynamicConfigurationUpdateDisablingLimiterMustNotDisableLimiter()
		 public virtual void DynamicConfigurationUpdateDisablingLimiterMustNotDisableLimiter()
		 {
			  // Create initially limited
			  CreateIOLimiter( 100 );
			  // Disable the limiter...
			  _limiter.disableLimit();
			  // ...while a dynamic configuration update happens
			  _config.updateDynamicSetting( GraphDatabaseSettings.check_point_iops_limit.name(), "-1", ORIGIN );
			  // The limiter must still be disabled...
			  AssertUnlimited();
			  // ...and re-enabling it...
			  _limiter.enableLimit();
			  // ...must maintain the limiter disabled.
			  AssertUnlimited();
			  // Until it is re-enabled.
			  _config.updateDynamicSetting( GraphDatabaseSettings.check_point_iops_limit.name(), "100", ORIGIN );
			  long stamp = Neo4Net.Io.pagecache.IOLimiter_Fields.INITIAL_STAMP;
			  RepeatedlyCallMaybeLimitIO( _limiter, stamp, 10 );
			  assertThat( _pauseNanosCounter.get(), greaterThan(TimeUnit.SECONDS.toNanos(9)) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void configuredLimitMustReflectCurrentState()
		 public virtual void ConfiguredLimitMustReflectCurrentState()
		 {
			  CreateIOLimiter( 100 );

			  assertThat( _limiter.Limited, @is( true ) );
			  MultipleDisableShouldReportUnlimited( _limiter );
			  assertThat( _limiter.Limited, @is( true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void configuredDisabledLimitShouldBeUnlimited()
		 public virtual void ConfiguredDisabledLimitShouldBeUnlimited()
		 {
			  CreateIOLimiter( -1 );

			  assertThat( _limiter.Limited, @is( false ) );
			  MultipleDisableShouldReportUnlimited( _limiter );
			  assertThat( _limiter.Limited, @is( false ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void unlimitedShouldAlwaysBeUnlimited()
		 public virtual void UnlimitedShouldAlwaysBeUnlimited()
		 {
			  IOLimiter limiter = Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited;

			  assertThat( limiter.Limited, @is( false ) );
			  MultipleDisableShouldReportUnlimited( limiter );
			  assertThat( limiter.Limited, @is( false ) );

			  limiter.EnableLimit();
			  try
			  {
					assertThat( limiter.Limited, @is( false ) );
			  }
			  finally
			  {
					limiter.DisableLimit();
			  }
		 }

		 private static void MultipleDisableShouldReportUnlimited( IOLimiter limiter )
		 {
			  limiter.DisableLimit();
			  try
			  {
					assertThat( limiter.Limited, @is( false ) );
					limiter.DisableLimit();
					try
					{
						 assertThat( limiter.Limited, @is( false ) );
					}
					finally
					{
						 limiter.EnableLimit();
					}
					assertThat( limiter.Limited, @is( false ) );
			  }
			  finally
			  {
					limiter.EnableLimit();
			  }
		 }
	}

}