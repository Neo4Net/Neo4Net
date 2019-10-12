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
namespace Neo4Net.Kernel.configuration
{
	using StringUtils = org.apache.commons.lang3.StringUtils;

	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using ByteUnit = Neo4Net.Io.ByteUnit;

	/// <summary>
	/// Migrations of old graph database settings.
	/// </summary>
	public class GraphDatabaseConfigurationMigrator : BaseConfigurationMigrator
	{
		 public GraphDatabaseConfigurationMigrator()
		 {
			  RegisterMigrations();
		 }

		 private void RegisterMigrations()
		 {
			  Add( new SpecificPropertyMigrationAnonymousInnerClass( this ) );

			  Add( new SpecificPropertyMigrationAnonymousInnerClass2( this ) );

			  Add( new SpecificPropertyMigrationAnonymousInnerClass3( this ) );

			  Add( new SpecificPropertyMigrationAnonymousInnerClass4( this ) );
			  Add( new SpecificPropertyMigrationAnonymousInnerClass5( this ) );
			  Add( new SpecificPropertyMigrationAnonymousInnerClass6( this ) );
			  Add( new SpecificPropertyMigrationAnonymousInnerClass7( this ) );
			  Add( new SpecificPropertyMigrationAnonymousInnerClass8( this ) );
		 }

		 private class SpecificPropertyMigrationAnonymousInnerClass : SpecificPropertyMigration
		 {
			 private readonly GraphDatabaseConfigurationMigrator _outerInstance;

			 public SpecificPropertyMigrationAnonymousInnerClass( GraphDatabaseConfigurationMigrator outerInstance ) : base( "dbms.index_sampling.buffer_size", "dbms.index_sampling.buffer_size has been replaced with dbms.index_sampling.sample_size_limit." )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void setValueWithOldSetting( string value, IDictionary<string, string> rawConfiguration )
			 {
				  if ( StringUtils.isNotEmpty( value ) )
				  {
						string oldSettingDefaultValue = GraphDatabaseSettings.index_sampling_buffer_size.DefaultValue;
						long? newValue = oldSettingDefaultValue.Equals( value ) ? ByteUnit.mebiBytes( 8 ) : Settings.BYTES.apply( value );
						rawConfiguration["dbms.index_sampling.sample_size_limit"] = newValue.ToString();
				  }
			 }
		 }

		 private class SpecificPropertyMigrationAnonymousInnerClass2 : SpecificPropertyMigration
		 {
			 private readonly GraphDatabaseConfigurationMigrator _outerInstance;

			 public SpecificPropertyMigrationAnonymousInnerClass2( GraphDatabaseConfigurationMigrator outerInstance ) : base( "dbms.transaction_timeout", "dbms.transaction_timeout has been replaced with dbms.rest.transaction.idle_timeout." )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void setValueWithOldSetting( string value, IDictionary<string, string> rawConfiguration )
			 {
				  rawConfiguration["dbms.rest.transaction.idle_timeout"] = value;
			 }
		 }

		 private class SpecificPropertyMigrationAnonymousInnerClass3 : SpecificPropertyMigration
		 {
			 private readonly GraphDatabaseConfigurationMigrator _outerInstance;

			 public SpecificPropertyMigrationAnonymousInnerClass3( GraphDatabaseConfigurationMigrator outerInstance ) : base( "unsupported.dbms.executiontime_limit.enabled", "unsupported.dbms.executiontime_limit.enabled is not supported anymore. " + "Set dbms.transaction.timeout settings to some positive value to enable execution guard and set " + "transaction timeout." )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void setValueWithOldSetting( string value, IDictionary<string, string> rawConfiguration )
			 {
			 }
		 }

		 private class SpecificPropertyMigrationAnonymousInnerClass4 : SpecificPropertyMigration
		 {
			 private readonly GraphDatabaseConfigurationMigrator _outerInstance;

			 public SpecificPropertyMigrationAnonymousInnerClass4( GraphDatabaseConfigurationMigrator outerInstance ) : base( "unsupported.dbms.executiontime_limit.time", "unsupported.dbms.executiontime_limit.time has been replaced with dbms.transaction.timeout." )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void setValueWithOldSetting( string value, IDictionary<string, string> rawConfiguration )
			 {
				  if ( StringUtils.isNotEmpty( value ) )
				  {
						if ( !rawConfiguration.ContainsKey( GraphDatabaseSettings.transaction_timeout.name() ) ) rawConfiguration.Add(GraphDatabaseSettings.transaction_timeout.name(), value);
				  }
			 }
		 }

		 private class SpecificPropertyMigrationAnonymousInnerClass5 : SpecificPropertyMigration
		 {
			 private readonly GraphDatabaseConfigurationMigrator _outerInstance;

			 public SpecificPropertyMigrationAnonymousInnerClass5( GraphDatabaseConfigurationMigrator outerInstance ) : base( "unsupported.dbms.shutdown_transaction_end_timeout", "unsupported.dbms.shutdown_transaction_end_timeout has been " + "replaced with dbms.shutdown_transaction_end_timeout." )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void setValueWithOldSetting( string value, IDictionary<string, string> rawConfiguration )
			 {
				  if ( StringUtils.isNotEmpty( value ) )
				  {
						if ( !rawConfiguration.ContainsKey( GraphDatabaseSettings.shutdown_transaction_end_timeout.name() ) ) rawConfiguration.Add(GraphDatabaseSettings.shutdown_transaction_end_timeout.name(), value);
				  }
			 }
		 }

		 private class SpecificPropertyMigrationAnonymousInnerClass6 : SpecificPropertyMigration
		 {
			 private readonly GraphDatabaseConfigurationMigrator _outerInstance;

			 public SpecificPropertyMigrationAnonymousInnerClass6( GraphDatabaseConfigurationMigrator outerInstance ) : base( "dbms.allow_format_migration", "dbms.allow_format_migration has been replaced with dbms.allow_upgrade." )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void setValueWithOldSetting( string value, IDictionary<string, string> rawConfiguration )
			 {
				  rawConfiguration[GraphDatabaseSettings.allow_upgrade.name()] = value;
			 }
		 }

		 private class SpecificPropertyMigrationAnonymousInnerClass7 : SpecificPropertyMigration
		 {
			 private readonly GraphDatabaseConfigurationMigrator _outerInstance;

			 public SpecificPropertyMigrationAnonymousInnerClass7( GraphDatabaseConfigurationMigrator outerInstance ) : base( "dbms.logs.timezone", "dbms.logs.timezone has been replaced with dbms.db.timezone." )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void setValueWithOldSetting( string value, IDictionary<string, string> rawConfiguration )
			 {
				  rawConfiguration[GraphDatabaseSettings.db_timezone.name()] = value;
			 }
		 }

		 private class SpecificPropertyMigrationAnonymousInnerClass8 : SpecificPropertyMigration
		 {
			 private readonly GraphDatabaseConfigurationMigrator _outerInstance;

			 public SpecificPropertyMigrationAnonymousInnerClass8( GraphDatabaseConfigurationMigrator outerInstance ) : base( "unsupported.dbms.enable_native_schema_index", "unsupported.dbms.enable_native_schema_index has been replaced with dbms.index.default_schema_provider." )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override void setValueWithOldSetting( string value, IDictionary<string, string> rawConfiguration )
			 {
				  if ( value.Equals( Settings.FALSE ) )
				  {
						if ( !rawConfiguration.ContainsKey( GraphDatabaseSettings.default_schema_provider.name() ) ) rawConfiguration.Add(GraphDatabaseSettings.default_schema_provider.name(), GraphDatabaseSettings.SchemaIndex.LUCENE10.providerName());
				  }
			 }
		 }
	}

}