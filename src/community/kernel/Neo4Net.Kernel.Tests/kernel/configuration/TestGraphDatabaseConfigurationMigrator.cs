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
namespace Neo4Net.Kernel.configuration
{
	using StringUtils = org.apache.commons.lang3.StringUtils;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using Log = Neo4Net.Logging.Log;
	using NullLog = Neo4Net.Logging.NullLog;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.GraphDatabaseSettings.SchemaIndex.LUCENE10;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.factory.GraphDatabaseSettings.SchemaIndex.NATIVE10;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.MapUtil.stringMap;

	/// <summary>
	/// Test configuration migration rules
	/// </summary>
	public class TestGraphDatabaseConfigurationMigrator
	{

		 private ConfigurationMigrator _migrator;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public Neo4Net.logging.AssertableLogProvider logProvider = new Neo4Net.logging.AssertableLogProvider(true);
		 public AssertableLogProvider LogProvider = new AssertableLogProvider( true );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp()
		 public virtual void SetUp()
		 {
			  _migrator = new GraphDatabaseConfigurationMigrator();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testNoMigration()
		 public virtual void TestNoMigration()
		 {
			  assertThat( _migrator.apply( stringMap( "foo", "bar" ), NullLog.Instance ), equalTo( stringMap( "foo", "bar" ) ) );
			  LogProvider.assertNoLoggingOccurred();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void migrateIndexSamplingBufferSizeIfPresent()
		 public virtual void MigrateIndexSamplingBufferSizeIfPresent()
		 {
			  IDictionary<string, string> resultConfig = _migrator.apply( stringMap( "dbms.index_sampling.buffer_size", "64m" ), Log );
			  assertEquals( "Old property should be migrated to new one with correct value", resultConfig, stringMap( "dbms.index_sampling.sample_size_limit", "8388608" ) );
			  AssertContainsWarningMessage( "dbms.index_sampling.buffer_size has been replaced with dbms.index_sampling.sample_size_limit." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void skipMigrationOfIndexSamplingBufferSizeIfNotPresent()
		 public virtual void SkipMigrationOfIndexSamplingBufferSizeIfNotPresent()
		 {
			  IDictionary<string, string> resultConfig = _migrator.apply( stringMap( "dbms.index_sampling.sample_size_limit", "8388600" ), Log );
			  assertEquals( "Nothing to migrate should be the same", resultConfig, stringMap( "dbms.index_sampling.sample_size_limit", "8388600" ) );
			  LogProvider.assertNoLoggingOccurred();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void migrateRestTransactionTimeoutIfPresent()
		 public virtual void MigrateRestTransactionTimeoutIfPresent()
		 {
			  IDictionary<string, string> migratedProperties = _migrator.apply( stringMap( "dbms.transaction_timeout", "120s" ), Log );
			  assertEquals( "Old property should be migrated to new", migratedProperties, stringMap( "dbms.rest.transaction.idle_timeout", "120s" ) );

			  AssertContainsWarningMessage( "dbms.transaction_timeout has been replaced with dbms.rest.transaction.idle_timeout." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void skipMigrationOfTransactionTimeoutIfNotPresent()
		 public virtual void SkipMigrationOfTransactionTimeoutIfNotPresent()
		 {
			  IDictionary<string, string> migratedProperties = _migrator.apply( stringMap( "dbms.rest.transaction.idle_timeout", "120s" ), Log );
			  assertEquals( "Nothing to migrate", migratedProperties, stringMap( "dbms.rest.transaction.idle_timeout", "120s" ) );
			  LogProvider.assertNoLoggingOccurred();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void migrateExecutionTimeLimitIfPresent()
		 public virtual void MigrateExecutionTimeLimitIfPresent()
		 {
			  IDictionary<string, string> migratedProperties = _migrator.apply( stringMap( "unsupported.dbms.executiontime_limit.time", "120s" ), Log );
			  assertEquals( "Old property should be migrated to new", migratedProperties, stringMap( "dbms.transaction.timeout", "120s" ) );

			  AssertContainsWarningMessage( "unsupported.dbms.executiontime_limit.time has been replaced with dbms.transaction.timeout." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void skipMigrationOfExecutionTimeLimitIfNotPresent()
		 public virtual void SkipMigrationOfExecutionTimeLimitIfNotPresent()
		 {
			  IDictionary<string, string> migratedProperties = _migrator.apply( stringMap( "dbms.transaction.timeout", "120s" ), Log );
			  assertEquals( "Nothing to migrate", migratedProperties, stringMap( "dbms.transaction.timeout", "120s" ) );
			  LogProvider.assertNoLoggingOccurred();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void skipMigrationOfExecutionTimeLimitIfTransactionTimeoutConfigured()
		 public virtual void SkipMigrationOfExecutionTimeLimitIfTransactionTimeoutConfigured()
		 {
			  IDictionary<string, string> migratedProperties = _migrator.apply( stringMap( "unsupported.dbms.executiontime_limit.time", "12s", "dbms.transaction.timeout", "120s" ), Log );
			  assertEquals( "Should keep pre configured transaction timeout.", migratedProperties, stringMap( "dbms.transaction.timeout", "120s" ) );
			  AssertContainsWarningMessage();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void migrateTransactionEndTimeout()
		 public virtual void MigrateTransactionEndTimeout()
		 {
			  IDictionary<string, string> migratedProperties = _migrator.apply( stringMap( "unsupported.dbms.shutdown_transaction_end_timeout", "12s" ), Log );
			  assertEquals( "Old property should be migrated to new", migratedProperties, stringMap( "dbms.shutdown_transaction_end_timeout", "12s" ) );

			  AssertContainsWarningMessage( "unsupported.dbms.shutdown_transaction_end_timeout has been " + "replaced with dbms.shutdown_transaction_end_timeout." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void skipMigrationOfTransactionEndTimeoutIfNotPresent()
		 public virtual void SkipMigrationOfTransactionEndTimeoutIfNotPresent()
		 {
			  IDictionary<string, string> migratedProperties = _migrator.apply( stringMap( "dbms.shutdown_transaction_end_timeout", "12s" ), Log );
			  assertEquals( "Nothing to migrate", migratedProperties, stringMap( "dbms.shutdown_transaction_end_timeout", "12s" ) );
			  LogProvider.assertNoLoggingOccurred();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void skipMigrationOfTransactionEndTimeoutIfCustomTransactionEndTimeoutConfigured()
		 public virtual void SkipMigrationOfTransactionEndTimeoutIfCustomTransactionEndTimeoutConfigured()
		 {
			  IDictionary<string, string> migratedProperties = _migrator.apply( stringMap( "unsupported.dbms.shutdown_transaction_end_timeout", "12s", "dbms.shutdown_transaction_end_timeout", "14s" ), Log );
			  assertEquals( "Should keep pre configured transaction timeout.", migratedProperties, stringMap( "dbms.shutdown_transaction_end_timeout", "14s" ) );
			  AssertContainsWarningMessage();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void migrateAllowFormatMigration()
		 public virtual void MigrateAllowFormatMigration()
		 {
			  IDictionary<string, string> migratedProperties = _migrator.apply( stringMap( "dbms.allow_format_migration", "true" ), Log );
			  assertEquals( "Old property should be migrated to new", migratedProperties, stringMap( "dbms.allow_upgrade", "true" ) );

			  AssertContainsWarningMessage( "dbms.allow_format_migration has been replaced with dbms.allow_upgrade." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void migrateEnableNativeSchemaIndex()
		 public virtual void MigrateEnableNativeSchemaIndex()
		 {
			  IDictionary<string, string> migratedProperties = _migrator.apply( stringMap( "unsupported.dbms.enable_native_schema_index", "false" ), Log );
			  assertEquals( "Old property should be migrated to new", migratedProperties, stringMap( "dbms.index.default_schema_provider", LUCENE10.providerName() ) );

			  AssertContainsWarningMessage( "unsupported.dbms.enable_native_schema_index has been replaced with dbms.index.default_schema_provider." );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void skipMigrationOfEnableNativeSchemaIndexIfNotPresent()
		 public virtual void SkipMigrationOfEnableNativeSchemaIndexIfNotPresent()
		 {
			  IDictionary<string, string> migratedProperties = _migrator.apply( stringMap( "dbms.index.default_schema_provider", NATIVE10.providerName() ), Log );
			  assertEquals( "Nothing to migrate", migratedProperties, stringMap( "dbms.index.default_schema_provider", NATIVE10.providerName() ) );
			  LogProvider.assertNoLoggingOccurred();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void skipMigrationOfEnableNativeSchemaIndexIfDefaultSchemaIndexConfigured()
		 public virtual void SkipMigrationOfEnableNativeSchemaIndexIfDefaultSchemaIndexConfigured()
		 {
			  IDictionary<string, string> migratedProperties = _migrator.apply( stringMap( "dbms.index.default_schema_provider", NATIVE10.providerName(), "unsupported.dbms.enable_native_schema_index", "false" ), Log );
			  assertEquals( "Should keep pre configured default schema index.", migratedProperties, stringMap( "dbms.index.default_schema_provider", NATIVE10.providerName() ) );
			  AssertContainsWarningMessage();
		 }

		 private Log Log
		 {
			 get
			 {
				  return LogProvider.getLog( typeof( GraphDatabaseConfigurationMigrator ) );
			 }
		 }

		 private void AssertContainsWarningMessage()
		 {
			  LogProvider.rawMessageMatcher().assertContains("WARNING! Deprecated configuration options used. See manual for details");
		 }

		 private void AssertContainsWarningMessage( string deprecationMessage )
		 {
			  AssertContainsWarningMessage();
			  if ( StringUtils.isNotEmpty( deprecationMessage ) )
			  {
					LogProvider.rawMessageMatcher().assertContains(deprecationMessage);
			  }
		 }
	}

}