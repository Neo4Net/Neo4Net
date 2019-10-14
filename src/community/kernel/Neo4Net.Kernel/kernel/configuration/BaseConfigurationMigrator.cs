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

	using Log = Neo4Net.Logging.Log;

	/// <summary>
	/// A basic approach to implementing configuration migrations.
	/// This applies migrations in both directions, meaning that you
	/// can still continue to both read and write with the old configuration
	/// value.
	/// </summary>
	public class BaseConfigurationMigrator : ConfigurationMigrator
	{
		 public interface Migration
		 {
			  bool AppliesTo( IDictionary<string, string> rawConfiguration );

			  IDictionary<string, string> Apply( IDictionary<string, string> rawConfiguration );

			  string DeprecationMessage { get; }
		 }

		 /// <summary>
		 /// Base class for implementing a migration that applies to a specific config property key.
		 /// 
		 /// By default, this class will print a  deprecation message and run <seealso cref="setValueWithOldSetting(string, System.Collections.IDictionary)"/>
		 /// if the specified property key has been set by the user. Override <seealso cref="appliesTo(System.Collections.IDictionary)"/> if you want to
		 /// trigger on more specific reasons than that.
		 /// </summary>
		 public abstract class SpecificPropertyMigration : Migration
		 {
			  internal readonly string PropertyKey;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly string DeprecationMessageConflict;

			  internal SpecificPropertyMigration( string propertyKey, string deprecationMessage )
			  {
					this.PropertyKey = propertyKey;
					this.DeprecationMessageConflict = deprecationMessage;
			  }

			  public override bool AppliesTo( IDictionary<string, string> rawConfiguration )
			  {
					return rawConfiguration.ContainsKey( PropertyKey );
			  }

			  public override IDictionary<string, string> Apply( IDictionary<string, string> rawConfiguration )
			  {
					string value = rawConfiguration.Remove( PropertyKey );
					SetValueWithOldSetting( value, rawConfiguration );
					return rawConfiguration;
			  }

			  public virtual string DeprecationMessage
			  {
				  get
				  {
						return DeprecationMessageConflict;
				  }
			  }

			  public abstract void SetValueWithOldSetting( string value, IDictionary<string, string> rawConfiguration );
		 }

		 private readonly IList<Migration> _migrations = new List<Migration>();

		 public virtual void Add( Migration migration )
		 {
			  _migrations.Add( migration );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @Nonnull public java.util.Map<String,String> apply(@Nonnull Map<String,String> rawConfiguration, @Nonnull Log log)
		 public override IDictionary<string, string> Apply( IDictionary<string, string> rawConfiguration, Log log )
		 {
			  bool printedDeprecationMessage = false;
			  foreach ( Migration migration in _migrations )
			  {
					if ( Migration.appliesTo( rawConfiguration ) )
					{
						 if ( !printedDeprecationMessage )
						 {
							  printedDeprecationMessage = true;
							  log.warn( "WARNING! Deprecated configuration options used. See manual for details" );
						 }
						 rawConfiguration = Migration.apply( rawConfiguration );
						 log.warn( Migration.DeprecationMessage );
					}
			  }
			  return rawConfiguration;
		 }
	}

}